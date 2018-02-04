/*
 * PlayerService.java - ASAP for Android
 *
 * Copyright (C) 2010-2018  Piotr Fusik
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
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
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

public class PlayerService extends Service implements Runnable, MediaController.MediaPlayerControl
{
	// User interface -----------------------------------------------------------------------------------------

	private static final int NOTIFICATION_ID = 1;

	private final Handler toastHandler = new Handler();

	private void showError(final int messageId)
	{
		toastHandler.post(new Runnable() {
			public void run() {
				Toast.makeText(PlayerService.this, messageId, Toast.LENGTH_SHORT).show();
			}
		});
	}

	private void showInfo()
	{
		sendBroadcast(new Intent(Player.ACTION_SHOW_INFO));
	}

	private void showNotification()
	{
		PendingIntent intent = PendingIntent.getActivity(this, 0, new Intent(this, Player.class), 0);
		Notification notification = new Notification.Builder(this)
			.setSmallIcon(R.drawable.icon)
			.setContentTitle(info.getTitleOrFilename())
			.setContentText(info.getAuthor())
			.setContentIntent(intent)
			.setOngoing(true)
			.getNotification();
		startForeground(NOTIFICATION_ID, notification);
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
			container.list(this, uri, false, true);
			if (shuffle)
				Collections.shuffle(playlist);
			else if (!Util.isAsma(uri) && !Util.endsWithIgnoreCase(uri.toString(), ".m3u"))
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


	// Playback -----------------------------------------------------------------------------------------------

	private static final int ACTION_STOP = 0;
	private static final int ACTION_LOAD = 1;
	private static final int ACTION_PLAY = 2;
	private static final int ACTION_PAUSE = 3;
	private static final int ACTION_NEXT = 4;
	private static final int ACTION_PREV = 5;
	private int action = ACTION_STOP;
	private Thread thread = null;

	private synchronized void setAction(int action)
	{
		this.action = action;
		if (thread != null && thread.isAlive())
			notify();
		else {
			thread = new Thread(this);
			thread.start();
		}
	}

	private void stop()
	{
		if (thread != null) {
			setAction(ACTION_STOP);
			try {
				thread.join();
			}
			catch (InterruptedException ex) {
			}
			thread = null;
		}
	}

	private Uri uri;
	int song;
	private static final int SONG_DEFAULT = -1;
	private static final int SONG_LAST = -2;
	private int seekPosition;

	private final ASAP asap = new ASAP();
	ASAPInfo info;

	private boolean load()
	{
		// read file
		String filename;
		byte[] module = new byte[ASAPInfo.MAX_MODULE_LENGTH];
		int moduleLen;
		try {
			String scheme = uri.getScheme();
			InputStream stream;
			if ("asma".equals(scheme)) {
				filename = uri.getSchemeSpecificPart();
				stream = getAssets().open(filename);
			}
			else if ("file".equals(scheme)) {
				filename = uri.getPath();
				if (Util.endsWithIgnoreCase(filename, ".zip")) {
					String zipFilename = filename;
					filename = uri.getFragment();
					stream = new ZipInputStream(zipFilename, filename);
				}
				else if (Util.endsWithIgnoreCase(filename, ".atr")) {
					JavaAATR atr = new JavaAATR(filename);
					filename = uri.getFragment();
					stream = new AATRFileInputStream(atr, filename);
				}
				else {
					stream = new FileInputStream(filename);
				}
			}
			else {
				showError(R.string.error_reading_file);
				return false;
			}
			moduleLen = Util.readAndClose(stream, module);
		}
		catch (IOException ex) {
			showError(R.string.error_reading_file);
			return false;
		}

		// load into ASAP
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
			asap.playSong(song, info.getDuration(song));
		}
		catch (Exception ex) {
			showError(R.string.invalid_file);
			return false;
		}

		return true;
	}

	private synchronized boolean handleLoadAction()
	{
		switch (action) {
		case ACTION_LOAD:
			return load();

		case ACTION_NEXT:
			if (info != null && song >= 0 && song + 1 < info.getSongs())
				song++;
			else {
				int playlistIndex = getPlaylistIndex();
				if (playlistIndex < 0)
					return false;
				if (++playlistIndex >= playlist.size())
					playlistIndex = 0;
				song = 0;
				uri = playlist.get(playlistIndex);
			}
			return load();

		case ACTION_PREV:
			if (song > 0)
				song--;
			else {
				int playlistIndex = getPlaylistIndex();
				if (playlistIndex < 0)
					return false;
				if (playlistIndex == 0)
					playlistIndex = playlist.size();
				song = SONG_LAST;
				uri = playlist.get(playlistIndex - 1);
			}
			return load();

		default:
			return false;
		}
	}

	private boolean handlePlayAction(AudioTrack audioTrack)
	{
		int pos;
		synchronized (this) {
			if (action == ACTION_PAUSE) {
				audioTrack.pause();
				while (action == ACTION_PAUSE) {
					try {
						wait();
					}
					catch (InterruptedException ex) {
					}
				}
				if (action == ACTION_PLAY)
					audioTrack.play();
			}
			if (action != ACTION_PLAY) {
				audioTrack.stop();
				return false;
			}

			pos = seekPosition;
			seekPosition = -1;
		}
		if (pos >= 0) {
			try {
				asap.seek(pos);
			}
			catch (Exception ex) {
			}
		}
		return true;
	}

	private void playLoop()
	{
		action = ACTION_PLAY;
		seekPosition = -1;

		int channelConfig = info.getChannels() == 1 ? AudioFormat.CHANNEL_CONFIGURATION_MONO : AudioFormat.CHANNEL_CONFIGURATION_STEREO;
		int bufferLen = AudioTrack.getMinBufferSize(ASAP.SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_16BIT) >> 1;
		if (bufferLen < 16384)
			bufferLen = 16384;
		byte[] byteBuffer = new byte[bufferLen << 1];
		short[] shortBuffer = new short[bufferLen];
		AudioTrack audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, ASAP.SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_16BIT, bufferLen << 1, AudioTrack.MODE_STREAM);
		audioTrack.play();

		while (handlePlayAction(audioTrack)) {
			bufferLen = asap.generate(byteBuffer, byteBuffer.length, ASAPSampleFormat.S16_L_E) >> 1;
			for (int i = 0; i < bufferLen; i++)
				shortBuffer[i] = (short) ((byteBuffer[i << 1] & 0xff) | byteBuffer[i << 1 | 1] << 8);
			audioTrack.write(shortBuffer, 0, bufferLen);
			if (bufferLen < shortBuffer.length)
				action = ACTION_NEXT;
		}
	}

	public void run()
	{
		while (handleLoadAction()) {
			showInfo();
			showNotification();
			playLoop();
		}
		stopForeground(true);
	}

	private boolean isPaused()
	{
		return action == ACTION_PAUSE;
	}

	public boolean isPlaying()
	{
		return action != ACTION_PAUSE;
	}

	public boolean canPause()
	{
		return true;
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
		setAction(ACTION_PAUSE);
	}

	public void start()
	{
		setAction(ACTION_PLAY);
	}

	synchronized void togglePause()
	{
		if (isPaused())
			start();
		else
			pause();
	}

	void playNextSong()
	{
		setAction(ACTION_NEXT);
	}

	void playPreviousSong()
	{
		setAction(ACTION_PREV);
	}

	public int getDuration()
	{
		if (info == null || song < 0)
			return -1;
		return info.getDuration(song);
	}

	public int getCurrentPosition()
	{
		return asap.getPosition();
	}

	public synchronized void seekTo(int pos)
	{
		seekPosition = pos;
		start();
	}

	public int getAudioSessionId()
	{
		// API 9: return audioTrack != null ? audioTrack.getAudioSessionId() : 0;
		return 0;
	}

	private final BroadcastReceiver headsetReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent)
		{
			if (!isInitialStickyBroadcast() && intent.getIntExtra("state", -1) == 0) {
				pause();
				showInfo(); // just to update the MediaController
			}
		}
	};

	@Override
	public void onStart(Intent intent, int startId)
	{
		super.onStart(intent, startId);

		registerReceiver(headsetReceiver, new IntentFilter(Intent.ACTION_HEADSET_PLUG));

		AudioManager audioManager = (AudioManager) getSystemService(AUDIO_SERVICE);
		ComponentName eventReceiver = new ComponentName(getPackageName(), MediaButtonEventReceiver.class.getName());
		audioManager.registerMediaButtonEventReceiver(eventReceiver);

		TelephonyManager telephony = (TelephonyManager) getSystemService(TELEPHONY_SERVICE);
		telephony.listen(new PhoneStateListener() {
				public void onCallStateChanged(int state, String incomingNumber) {
					if (state == TelephonyManager.CALL_STATE_RINGING)
						pause();
				}
			}, PhoneStateListener.LISTEN_CALL_STATE);

		song = SONG_DEFAULT;
		uri = intent.getData();
		String playlistUri = intent.getStringExtra(EXTRA_PLAYLIST);
		if (playlistUri != null)
			setPlaylist(Uri.parse(playlistUri), false);
		else if (ASAPInfo.isOurFile(uri.toString()))
			setPlaylist(Util.getParent(uri), false);
		else {
			// shuffle
			setPlaylist(uri, true);
			uri = playlist.get(0);
		}
		setAction(ACTION_LOAD);
	}

	@Override
	public void onDestroy()
	{
		super.onDestroy();
		stop();

		AudioManager audioManager = (AudioManager) getSystemService(AUDIO_SERVICE);
		ComponentName eventReceiver = new ComponentName(getPackageName(), MediaButtonEventReceiver.class.getName());
		audioManager.unregisterMediaButtonEventReceiver(eventReceiver);

		unregisterReceiver(headsetReceiver);
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
