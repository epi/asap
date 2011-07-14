/*
 * PlayerService.java - ASAP for Android
 *
 * Copyright (C) 2010-2011  Piotr Fusik
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
import android.content.Intent;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.net.Uri;
import android.os.Binder;
import android.os.IBinder;
import android.widget.Toast;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.IOException;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.zip.ZipFile;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;

public class PlayerService extends Service implements Runnable
{
	private Uri uri;
	private final ASAP asap = new ASAP();
	ASAPInfo info;
	private String filename;
	int song;
	private boolean stop;
	private AudioTrack audioTrack;

	private static final int NOTIFICATION_ID = 1;
	private static final Class[] START_FOREGROUND_SIGNATURE = new Class[] { int.class, Notification.class };
	private static final Class[] STOP_FOREGROUND_SIGNATURE = new Class[] { boolean.class };
	private Method startForeground;
	private Method stopForeground;
	private final Object[] startForegroundArgs = new Object[2];
	private final Object[] stopForegroundArgs = new Object[1];
	private NotificationManager notMan;

	private void invokeMethod(Method method, Object[] args)
	{
		try {
			method.invoke(this, args);
		} catch (InvocationTargetException ex) {
			// should not happen
		} catch (IllegalAccessException ex) {
			// should not happen
		}
	}

	private void startForegroundCompat(int id, Notification notification)
	{
		if (startForeground != null) {
			startForegroundArgs[0] = Integer.valueOf(id);
			startForegroundArgs[1] = notification;
			invokeMethod(startForeground, startForegroundArgs);
		}
		else {
			// fall back on the old API
			setForeground(true);
			notMan.notify(id, notification);
		}
	}

	private void stopForegroundCompat(int id)
	{
		if (stopForeground != null) {
			stopForegroundArgs[0] = Boolean.TRUE;
			invokeMethod(stopForeground, stopForegroundArgs);
		}
		else {
			// fall back on the old API.
			// cancel before changing the foreground state, since we could be killed at that point
			notMan.cancel(id);
			setForeground(false);
		}
	}

	private void showError(int messageId)
	{
		Toast.makeText(this, messageId, Toast.LENGTH_SHORT);
	}

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
		filename = uri.getLastPathSegment();
		ZipFile zip = null;
		final byte[] module = new byte[ASAPInfo.MAX_MODULE_LENGTH];
		int moduleLen;
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
		try {
			asap.load(filename, module, moduleLen);
			info = asap.getInfo();
			playSong(info.getDefaultSong());
		}
		catch (Exception ex) {
			showError(R.string.invalid_file);
			return;
		}
		stop = false;

		int config = info.getChannels() == 1 ? AudioFormat.CHANNEL_CONFIGURATION_MONO : AudioFormat.CHANNEL_CONFIGURATION_STEREO;
		int len = AudioTrack.getMinBufferSize(ASAP.SAMPLE_RATE, config, AudioFormat.ENCODING_PCM_8BIT);
		if (len < 16384)
			len = 16384;
		final byte[] buffer = new byte[len];
		audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, ASAP.SAMPLE_RATE, config, AudioFormat.ENCODING_PCM_8BIT, len, AudioTrack.MODE_STREAM);
		audioTrack.play();

		PendingIntent contentIntent = PendingIntent.getActivity(this, 0, new Intent(this, Player.class), 0);
		String title = info.getTitleOrFilename();
		Notification notification = new Notification(R.drawable.icon, title, System.currentTimeMillis());
		notification.flags |= Notification.FLAG_ONGOING_EVENT;
		notification.setLatestEventInfo(this, title, info.getAuthor(), contentIntent);
		startForegroundCompat(NOTIFICATION_ID, notification);

		for (;;) {
			synchronized (this) {
				if (len < buffer.length || isPaused()) {
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
				len = asap.generate(buffer, buffer.length, ASAPSampleFormat.U8);
			}
			audioTrack.write(buffer, 0, len);
		}
	}

	private InputStream httpGet(Uri uri) throws IOException
	{
		Notification notification = new Notification(R.drawable.icon, filename, System.currentTimeMillis());
		notification.flags |= Notification.FLAG_ONGOING_EVENT;
		notMan.notify(NOTIFICATION_ID, notification);
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

	@Override
	public void onCreate()
	{
		try {
			startForeground = Service.class.getMethod("startForeground", START_FOREGROUND_SIGNATURE);
			stopForeground = Service.class.getMethod("stopForeground", STOP_FOREGROUND_SIGNATURE);
		}
		catch (NoSuchMethodException ex) {
		}
        notMan = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
	}

    @Override
    public void onStart(Intent intent, int startId)
	{
    	super.onStart(intent, startId);
		uri = intent.getData();
		new Thread(this).start();
    }

	@Override
    public void onDestroy()
	{
		super.onDestroy();
		synchronized (this) {
			stop = true;
			notify();
		}
		stopForegroundCompat(NOTIFICATION_ID);
    }

	private final IBinder binder = new LocalBinder();

    public class LocalBinder extends Binder
	{
    	PlayerService getService()
		{
            return PlayerService.this;
        }
    }

    @Override
    public IBinder onBind(Intent intent)
	{
        return binder;
    }
}
