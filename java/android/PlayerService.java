/*
 * PlayerService.java - ASAP for Android
 *
 * Copyright (C) 2010-2013  Piotr Fusik
 *
 * This file is part of ASAP (Another Slight Atari Player),
 * see http://asap.sourceforge.net
 *
 * ASAP is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published
 * by the Free Software Foundation; either version 2 of the License,
 * or (at your option) any later version.
 *
 * ASAP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ASAP; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

package net.sf.asap;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.ComponentName;
import android.content.Intent;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.net.Uri;
import android.os.Binder;
import android.os.Handler;
import android.os.IBinder;
import android.telephony.PhoneStateListener;
import android.telephony.TelephonyManager;
import android.widget.Toast;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.IOException;
import java.util.zip.ZipFile;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;

public class PlayerService extends Service implements Runnable
{
	// User interface -----------------------------------------------------------------------------------------

	private NotificationManager notMan;
	private static final int NOTIFICATION_ID = 1;

	@Override
	public void onCreate()
	{
		notMan = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
	}

	private void startForegroundCompat(int id, Notification notification)
	{
		if (!Util.invokeMethod(this, "startForeground", id, notification)) {
			// Fall back on the old API.
			Util.invokeMethod(this, "setForeground", true);
			notMan.notify(id, notification);
		}
	}

	private void stopForegroundCompat(int id)
	{
		if (!Util.invokeMethod(this, "stopForeground", true)) {
			// Fall back on the old API.
			// Cancel before changing the foreground state, since we could be killed at that point.
			notMan.cancel(id);
			Util.invokeMethod(this, "setForeground", false);
		}
	}

	private final Handler toastHandler = new Handler();

	private void showError(final int messageId)
	{
		toastHandler.post(new Runnable() {
			public void run() {
				Toast.makeText(PlayerService.this, messageId, Toast.LENGTH_SHORT).show();
			}
		});
	}


	// Threading ----------------------------------------------------------------------------------------------

	private Thread thread;
	private boolean stop;

	private void stop()
	{
		if (thread != null) {
			synchronized (this) {
				stop = true;
				notify();
			}
			try {
				thread.join();
			}
			catch (InterruptedException ex) {
			}
			thread = null;
			stopForegroundCompat(NOTIFICATION_ID);
		}
	}


	// I/O ----------------------------------------------------------------------------------------------------

	private Uri uri;
	private String filename;

	private InputStream httpGet(Uri uri) throws IOException
	{
		DefaultHttpClient client = new DefaultHttpClient();
		HttpGet request = new HttpGet(uri.toString());
		HttpResponse response = client.execute(request);
		StatusLine status = response.getStatusLine();
		if (status.getStatusCode() != 200)
			throw new IOException("HTTP error " + status);
		return response.getEntity().getContent();
	}

	/**
	 * Reads bytes from the stream into the byte array
	 * until end of stream or array is full.
	 * @param is source stream
	 * @param b output array
	 * @return number of bytes read
	 */
	private static int readAndClose(InputStream is, byte[] b) throws IOException
	{
		int got = 0;
		int len = b.length;
		try {
			while (got < len) {
				int i = is.read(b, got, len - got);
				if (i <= 0)
					break;
				got += i;
			}
		}
		finally {
			is.close();
		}
		return got;
	}


	// Playback -----------------------------------------------------------------------------------------------

	private final ASAP asap = new ASAP();
	ASAPInfo info;
	int song;
	private AudioTrack audioTrack;

	boolean isPaused()
	{
		return audioTrack == null || audioTrack.getPlayState() == AudioTrack.PLAYSTATE_PAUSED;
	}

	void pause()
	{
		audioTrack.pause();
	}

	void resume()
	{
		if (audioTrack != null) {
			audioTrack.play();
			synchronized (this) {
				notify();
			}
		}
	}

	void togglePause()
	{
		if (isPaused())
			resume();
		else
			pause();
	}

	private void playSong(int song) throws Exception
	{
		synchronized (asap) {
			asap.playSong(song, info.getLoop(song) ? -1 : info.getDuration(song));
		}
		this.song = song;
		sendBroadcast(new Intent(Player.ACTION_SHOW_INFO));
		resume();
	}

	void playNextSong()
	{
		if (song + 1 < info.getSongs()) {
			try {
				playSong(song + 1);
			}
			catch (Exception ex) {
			}
		}
	}

	void playPreviousSong()
	{
		if (song > 0) {
			try {
				playSong(song - 1);
			}
			catch (Exception ex) {
			}
		}
	}

	int getDuration()
	{
		return info.getDuration(song);
	}

	int getPosition()
	{
		return asap.getPosition();
	}

	void seek(int pos)
	{
		try {
			synchronized (asap) {
				asap.seek(pos);
			}
		}
		catch (Exception ex) {
			showError(R.string.invalid_file);
		}
	}

	public void run()
	{
		// read file
		filename = uri.getLastPathSegment();
		final byte[] module = new byte[ASAPInfo.MAX_MODULE_LENGTH];
		int moduleLen;
		ZipFile zip = null;
		try {
			InputStream is;
			if (Util.isZip(filename)) {
				zip = new ZipFile(uri.getPath());
				filename = uri.getFragment();
				is = zip.getInputStream(zip.getEntry(filename));
			}
			else {
				try {
					is = getContentResolver().openInputStream(uri);
				}
				catch (FileNotFoundException ex) {
					if (uri.getScheme().equals("http"))
						is = httpGet(uri);
					else
						throw ex;
				}
			}
			moduleLen = readAndClose(is, module);
		}
		catch (IOException ex) {
			showError(R.string.error_reading_file);
			return;
		}
		finally {
			Util.close(zip);
		}

		// load file
		try {
			asap.load(filename, module, moduleLen);
			info = asap.getInfo();
			playSong(info.getDefaultSong());
		}
		catch (Exception ex) {
			showError(R.string.invalid_file);
			return;
		}

		PendingIntent contentIntent = PendingIntent.getActivity(this, 0, new Intent(this, Player.class), 0);
		String title = info.getTitleOrFilename();
		Notification notification = new Notification(R.drawable.icon, title, System.currentTimeMillis());
		notification.flags |= Notification.FLAG_ONGOING_EVENT;
		notification.setLatestEventInfo(this, title, info.getAuthor(), contentIntent);
		startForegroundCompat(NOTIFICATION_ID, notification);

		// playback
		int channelConfig = info.getChannels() == 1 ? AudioFormat.CHANNEL_CONFIGURATION_MONO : AudioFormat.CHANNEL_CONFIGURATION_STEREO;
		int bufferLen = AudioTrack.getMinBufferSize(ASAP.SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_8BIT);
		if (bufferLen < 16384)
			bufferLen = 16384;
		final byte[] buffer = new byte[bufferLen];
		audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, ASAP.SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_8BIT, bufferLen, AudioTrack.MODE_STREAM);
		audioTrack.play();

		for (;;) {
			synchronized (this) {
				if (bufferLen < buffer.length || isPaused()) {
					try {
						wait();
					}
					catch (InterruptedException ex) {
					}
				}
				if (stop) {
					audioTrack.stop();
					return;
				}
			}
			synchronized (asap) {
				bufferLen = asap.generate(buffer, buffer.length, ASAPSampleFormat.U8);
			}
			audioTrack.write(buffer, 0, bufferLen);
		}
	}

	private void registerMediaButtonEventReceiver(String methodName)
	{
		Object audioManager = getSystemService(AUDIO_SERVICE);
		ComponentName eventReceiver = new ComponentName(getPackageName(), MediaButtonEventReceiver.class.getName());
		Util.invokeMethod(audioManager, methodName, eventReceiver);
	}

	@Override
	public void onStart(Intent intent, int startId)
	{
		super.onStart(intent, startId);
		stop();
		uri = intent.getData();
		stop = false;
		thread = new Thread(this);
		thread.start();
		registerMediaButtonEventReceiver("registerMediaButtonEventReceiver");

		TelephonyManager telephony = (TelephonyManager) getSystemService(TELEPHONY_SERVICE);
		telephony.listen(new PhoneStateListener() {
				public void onCallStateChanged(int state, String incomingNumber) {
					if (state == TelephonyManager.CALL_STATE_RINGING)
						pause();
				}
			}, PhoneStateListener.LISTEN_CALL_STATE);
	}

	@Override
	public void onDestroy()
	{
		super.onDestroy();
		stop();
		registerMediaButtonEventReceiver("unregisterMediaButtonEventReceiver");
	}


	// Player.java interface ----------------------------------------------------------------------------------

	class LocalBinder extends Binder
	{
		PlayerService getService()
		{
			return PlayerService.this;
		}
	}

	private final IBinder binder = new LocalBinder();

	@Override
	public IBinder onBind(Intent intent)
	{
		return binder;
	}
}
