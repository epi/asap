/*
 * PlayerService.java - ASAP for Android
 *
 * Copyright (C) 2010-2021  Piotr Fusik
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
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.media.AudioAttributes;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.media.MediaMetadata;
import android.media.session.MediaSession;
import android.media.session.PlaybackState;
import android.net.Uri;
import android.os.Binder;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.widget.MediaController;
import android.widget.Toast;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;

public class PlayerService extends Service implements Runnable, AudioManager.OnAudioFocusChangeListener, MediaController.MediaPlayerControl
{
	// User interface -----------------------------------------------------------------------------------------

	private static final int NOTIFICATION_ID = 1;

	private final Handler toastHandler = new Handler();

	private void showError(final int messageId)
	{
		toastHandler.post(() -> Toast.makeText(PlayerService.this, messageId, Toast.LENGTH_SHORT).show());
	}

	private void showInfo()
	{
		sendBroadcast(new Intent(Player.ACTION_SHOW_INFO));
	}

	private MediaSession mediaSession;

	private void showNotification()
	{
		String title = info.getTitleOrFilename();
		String author = info.getAuthor();
		String date = info.getDate();
		PendingIntent intent = PendingIntent.getActivity(this, 0, new Intent(this, Player.class), 0);

		MediaMetadata.Builder metadata = new MediaMetadata.Builder()
			.putString(MediaMetadata.METADATA_KEY_TITLE, title);
		if (author.length() > 0)
			metadata.putString(MediaMetadata.METADATA_KEY_ARTIST, author);
		if (date.length() > 0)
			metadata.putString(MediaMetadata.METADATA_KEY_DATE, date);
		int duration = info.getDuration(song);
		if (duration > 0)
			metadata.putLong(MediaMetadata.METADATA_KEY_DURATION, duration);
		int songs = info.getSongs();
		if (songs > 1) {
			metadata.putLong(MediaMetadata.METADATA_KEY_TRACK_NUMBER, song + 1);
			metadata.putLong(MediaMetadata.METADATA_KEY_NUM_TRACKS, songs);
		}
		int year = info.getYear();
		if (year > 0)
			metadata.putLong(MediaMetadata.METADATA_KEY_YEAR, year);
		mediaSession.setMetadata(metadata.build());
		mediaSession.setSessionActivity(intent);
		mediaSession.setActive(true);

		Notification.Builder builder = new Notification.Builder(this)
			.setSmallIcon(R.drawable.icon)
			.setContentTitle(title)
			.setContentText(author)
			.setContentIntent(intent)
			.setStyle(new Notification.MediaStyle().setMediaSession(mediaSession.getSessionToken()))
			.setVisibility(Notification.VISIBILITY_PUBLIC);
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
			final String CHANNEL_ID = "NOW_PLAYING";
			NotificationChannel channel = new NotificationChannel(CHANNEL_ID, getString(R.string.notification_channel), NotificationManager.IMPORTANCE_LOW);
			((NotificationManager) getSystemService(NOTIFICATION_SERVICE)).createNotificationChannel(channel);
			builder.setChannelId(CHANNEL_ID);
		}
		startForeground(NOTIFICATION_ID, builder.build());
	}

	private void setPlaybackState(int state, float speed, long actions)
	{
		mediaSession.setPlaybackState(new PlaybackState.Builder()
			.setState(state, asap.getPosition(), speed)
			.setActions(actions)
			.build());
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

	private boolean setSearchPlaylist(String query)
	{
		FileInfo[] infos = FileInfo.listIndex(this, query);
		if (infos.length == 0)
			return false;
		playlist.clear();
		for (FileInfo info : infos)
			playlist.add(Util.getAsmaUri(info.filename));
		return true;
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

	private final long COMMON_PLAYBACK_STATE = PlaybackState.ACTION_PLAY_FROM_SEARCH | PlaybackState.ACTION_SEEK_TO | PlaybackState.ACTION_SKIP_TO_PREVIOUS | PlaybackState.ACTION_SKIP_TO_NEXT;

	private boolean handlePlayAction(AudioTrack audioTrack)
	{
		int pos;
		synchronized (this) {
			if (action == ACTION_PAUSE) {
				setPlaybackState(PlaybackState.STATE_PAUSED, 0, PlaybackState.ACTION_PLAY | COMMON_PLAYBACK_STATE);
				audioTrack.pause();
				while (action == ACTION_PAUSE) {
					try {
						wait();
					}
					catch (InterruptedException ex) {
					}
				}
				if (action == ACTION_PLAY) {
					setPlaybackState(PlaybackState.STATE_PLAYING, 1, PlaybackState.ACTION_PAUSE | COMMON_PLAYBACK_STATE);
					audioTrack.play();
				}
			}
			if (action != ACTION_PLAY) {
				audioTrack.stop();
				return false;
			}

			pos = seekPosition;
			seekPosition = -1;
		}
		if (pos >= 0) {
			if (pos < asap.getPosition())
				setPlaybackState(PlaybackState.STATE_REWINDING, -10, 0);
			else
				setPlaybackState(PlaybackState.STATE_FAST_FORWARDING, 10, 0);
			try {
				asap.seek(pos);
			}
			catch (Exception ex) {
			}
			setPlaybackState(PlaybackState.STATE_PLAYING, 1, PlaybackState.ACTION_PAUSE | COMMON_PLAYBACK_STATE);
		}
		return true;
	}

	private void playLoop()
	{
		action = ACTION_PLAY;
		seekPosition = -1;
		setPlaybackState(PlaybackState.STATE_PLAYING, 1, PlaybackState.ACTION_PAUSE | COMMON_PLAYBACK_STATE);

		int channelConfig = info.getChannels() == 1 ? AudioFormat.CHANNEL_OUT_MONO : AudioFormat.CHANNEL_OUT_STEREO;
		int bufferLen = AudioTrack.getMinBufferSize(ASAP.SAMPLE_RATE, channelConfig, AudioFormat.ENCODING_PCM_16BIT) >> 1;
		if (bufferLen < 16384)
			bufferLen = 16384;
		byte[] byteBuffer = new byte[bufferLen << 1];
		short[] shortBuffer = new short[bufferLen];
		AudioAttributes attributes = new AudioAttributes.Builder()
			.setContentType(AudioAttributes.CONTENT_TYPE_MUSIC)
			.setUsage(AudioAttributes.USAGE_MEDIA)
			.build();
		AudioFormat format = new AudioFormat.Builder()
			.setChannelMask(channelConfig)
			.setEncoding(AudioFormat.ENCODING_PCM_16BIT)
			.setSampleRate(ASAP.SAMPLE_RATE)
			.build();
		AudioTrack audioTrack = new AudioTrack(attributes, format, bufferLen << 1, AudioTrack.MODE_STREAM, AudioManager.AUDIO_SESSION_ID_GENERATE);
		audioTrack.play();

		while (handlePlayAction(audioTrack)) {
			bufferLen = asap.generate(byteBuffer, byteBuffer.length, ASAPSampleFormat.S16_L_E) >> 1;
			for (int i = 0; i < bufferLen; i++)
				shortBuffer[i] = (short) ((byteBuffer[i << 1] & 0xff) | byteBuffer[i << 1 | 1] << 8);
			audioTrack.write(shortBuffer, 0, bufferLen);
			if (bufferLen < shortBuffer.length)
				action = ACTION_NEXT;
		}
		audioTrack.release();
	}

	public void run()
	{
		if (!gainFocus())
			return;
		while (handleLoadAction()) {
			showInfo();
			showNotification();
			playLoop();
		}
		stopForeground(true);
		releaseFocus();
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
		// return audioTrack != null ? audioTrack.getAudioSessionId() : 0;
		return 0;
	}

	private AudioManager getAudioManager()
	{
		return (AudioManager) getSystemService(AUDIO_SERVICE);
	}

	@Override
	public void onAudioFocusChange(int focusChange)
	{
		switch (focusChange) {
		case AudioManager.AUDIOFOCUS_LOSS:
		case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT:
			pause();
			break;
		case AudioManager.AUDIOFOCUS_GAIN:
			start();
			break;
		default:
			break;
		}
	}

	private boolean gainFocus()
	{
		return getAudioManager().requestAudioFocus(this, AudioManager.STREAM_MUSIC, AudioManager.AUDIOFOCUS_GAIN) == AudioManager.AUDIOFOCUS_REQUEST_GRANTED;
	}

	private void releaseFocus()
	{
		getAudioManager().abandonAudioFocus(this);
	}

	private final MediaSession.Callback callback = new MediaSession.Callback() {
		@Override
		public void onPause()
		{
			pause();
		}

		@Override
		public void onPlay()
		{
			start();
		}

		@Override
		public void onSeekTo(long pos)
		{
			seekTo((int) pos);
		}

		@Override
		public void onSkipToNext()
		{
			playPreviousSong();
		}

		@Override
		public void onSkipToPrevious()
		{
			playNextSong();
		}

		@Override
		public void onPlayFromSearch(String query, Bundle extras)
		{
			if (setSearchPlaylist(query)) {
				uri = playlist.get(0);
				setAction(ACTION_LOAD);
			}
		}
	};

	private final BroadcastReceiver becomingNoisyReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent)
		{
			pause();
			showInfo(); // just to update the MediaController
		}
	};

	@Override
	public void onCreate()
	{
		mediaSession = new MediaSession(this, "ASAP");
		mediaSession.setCallback(callback);
		mediaSession.setFlags(MediaSession.FLAG_HANDLES_MEDIA_BUTTONS | MediaSession.FLAG_HANDLES_TRANSPORT_CONTROLS);
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId)
	{
		registerReceiver(becomingNoisyReceiver, new IntentFilter(AudioManager.ACTION_AUDIO_BECOMING_NOISY));

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
		return START_NOT_STICKY;
	}

	@Override
	public void onDestroy()
	{
		stop();

		unregisterReceiver(becomingNoisyReceiver);

		mediaSession.release();
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
