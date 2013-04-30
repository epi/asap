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
import android.widget.MediaController;
import android.widget.Toast;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;

public class PlayerService extends Service implements Runnable, MediaController.MediaPlayerControl
{
	private Uri uri;
	int song;
	private static final int SONG_DEFAULT = -1;
	private static final int SONG_LAST = -2;

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

	private void playFile(Uri uri, int song)
	{
		stop();
		this.uri = uri;
		this.song = song;
		stop = false;
		thread = new Thread(this);
		thread.start();
	}


	// I/O ----------------------------------------------------------------------------------------------------

	private static InputStream httpGet(Uri uri) throws IOException
	{
		DefaultHttpClient client = new DefaultHttpClient();
		HttpGet request = new HttpGet(uri.toString());
		HttpResponse response = client.execute(request);
		StatusLine status = response.getStatusLine();
		if (status.getStatusCode() != 200)
			throw new IOException("HTTP error " + status);
		return response.getEntity().getContent();
	}


	// Playlist -----------------------------------------------------------------------------------------------

	static final String EXTRA_PLAYLIST = "asap.intent.extra.PLAYLIST";

	private final ArrayList<Uri> playlist = new ArrayList<Uri>();

	private void setPlaylist(final Uri uri, boolean shuffle)
	{
		playlist.clear();
		FileContainer container = new FileContainer() {
				@Override
				protected void onSongFile(String name, InputStream is) {
					playlist.add(Util.buildUri(uri, name));
				}
			};
		try {
			container.list(uri, false, true);
			if (shuffle)
				Collections.shuffle(playlist);
			else if (!Util.isM3u(uri))
				Collections.sort(playlist);
		}
		catch (IOException ex) {
			// playlist is not essential
		}
	}

	private int getPlaylistIndex()
	{
		return playlist.indexOf(uri);
	}

	private void playFileFromPlaylist(int playlistIndex, int song)
	{
		playFile(playlist.get(playlistIndex), song);
	}


	// Playback -----------------------------------------------------------------------------------------------

	private final ASAP asap = new ASAP();
	ASAPInfo info;
	private AudioTrack audioTrack;
	private int seekPosition;

	private boolean isPaused()
	{
		return audioTrack == null || audioTrack.getPlayState() == AudioTrack.PLAYSTATE_PAUSED;
	}

	public boolean isPlaying()
	{
		return !isPaused();
	}

	public boolean canPause()
	{
		return !isPaused();
	}

	public boolean canSeekBackward()
	{
		return false;
	}

	public boolean canSeekForward()
	{
		return false;
	}

	public int getBufferPercentage()
	{
		return 100;
	}

	public void pause()
	{
		if (audioTrack != null)
			audioTrack.pause();
	}

	public void start()
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
			start();
		else
			pause();
	}

	private void playSong() throws Exception
	{
		synchronized (asap) {
			seekPosition = -1;
			asap.playSong(song, info.getLoop(song) ? -1 : info.getDuration(song));
		}
		sendBroadcast(new Intent(Player.ACTION_SHOW_INFO));
		start();
	}

	void playNextSong()
	{
		if (song + 1 < info.getSongs()) {
			song++;
			try {
				playSong();
				return;
			}
			catch (Exception ex) {
			}
		}

		int playlistIndex = getPlaylistIndex();
		if (playlistIndex >= 0) {
			if (++playlistIndex >= playlist.size())
				playlistIndex = 0;
			playFileFromPlaylist(playlistIndex, 0);
		}
	}

	void playPreviousSong()
	{
		if (song > 0) {
			song--;
			try {
				playSong();
				return;
			}
			catch (Exception ex) {
			}
		}

		int playlistIndex = getPlaylistIndex();
		if (playlistIndex >= 0) {
			if (playlistIndex == 0)
				playlistIndex = playlist.size();
			playFileFromPlaylist(playlistIndex - 1, SONG_LAST);
		}
	}

	public int getDuration()
	{
		if (song < 0)
			return -1;
		return info.getDuration(song);
	}

	public int getCurrentPosition()
	{
		return asap.getPosition();
	}

	public void seekTo(int pos)
	{
		synchronized (asap) {
			seekPosition = pos;
		}
	}

	public void run()
	{
		// read file
		String filename = uri.getPath();
		byte[] module = new byte[ASAPInfo.MAX_MODULE_LENGTH];
		int moduleLen;
		try {
			InputStream is;
			switch (uri.getScheme()) {
			case "file":
				if (Util.isZip(filename)) {
					String zipFilename = filename;
					filename = uri.getFragment();
					is = new ZipInputStream(zipFilename, filename);
				}
				else
					is = new FileInputStream(filename);
				break;
			case "http":
				is = httpGet(uri);
				break;
			default:
				throw new FileNotFoundException(uri.toString());
			}
			moduleLen = Util.readAndClose(is, module);
		}
		catch (IOException ex) {
			showError(R.string.error_reading_file);
			return;
		}

		// load file
		try {
			asap.load(filename, module, moduleLen);
			info = asap.getInfo();
			switch (song) {
			case SONG_DEFAULT:
				song = info.getDefaultSong();
				break;
			case SONG_LAST:
				song = info.getSongs() - 1;
				break;
			default:
				break;
			}
			playSong();
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
				int pos = seekPosition;
				if (pos >= 0) {
					seekPosition = -1;
					try {
						asap.seek(pos);
					}
					catch (Exception ex) {
					}
				}
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

		registerMediaButtonEventReceiver("registerMediaButtonEventReceiver");

		TelephonyManager telephony = (TelephonyManager) getSystemService(TELEPHONY_SERVICE);
		telephony.listen(new PhoneStateListener() {
				public void onCallStateChanged(int state, String incomingNumber) {
					if (state == TelephonyManager.CALL_STATE_RINGING)
						pause();
				}
			}, PhoneStateListener.LISTEN_CALL_STATE);

		Uri uri = intent.getData();
		String playlistUri = intent.getStringExtra(EXTRA_PLAYLIST);
		if (playlistUri != null)
			setPlaylist(Uri.parse(playlistUri), false);
		else if ("file".equals(uri.getScheme())) {
			if (ASAPInfo.isOurFile(uri.toString()))
				setPlaylist(Util.getParent(uri), false);
			else {
				setPlaylist(uri, true);
				uri = playlist.get(0);
			}
		}
		playFile(uri, SONG_DEFAULT);
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
