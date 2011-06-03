/*
 * Player.java - ASAP for Android
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

import android.app.Activity;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.View.OnClickListener;
import android.widget.MediaController;
import android.widget.TextView;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.IOException;
import java.util.zip.ZipFile;
import org.apache.http.HttpResponse;
import org.apache.http.StatusLine;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;

public class Player extends Activity implements Runnable
{
	private final ASAP asap = new ASAP();
	private ASAPInfo info;
	private String filename;
	private int song;
	private MediaController mediaController;
	private boolean stop;
	private AudioTrack audioTrack;

	private View getContentView()
	{
		ViewGroup vg = (ViewGroup) getWindow().getDecorView();
		return vg.getChildAt(0);
	}

	private void showError(int messageId)
	{
		setTitle(R.string.error_title);
		setContentView(R.layout.error);
		((TextView) findViewById(R.id.filename)).setText(filename);
		((TextView) findViewById(R.id.message)).setText(messageId);
	}

	private void setTag(int controlId, String value)
	{
		TextView control = (TextView) findViewById(controlId);
		if (value.length() == 0)
			control.setVisibility(View.GONE);
		else {
			control.setText(value);
			control.setVisibility(View.VISIBLE);
		}
	}

	@Override
	public boolean onTouchEvent(MotionEvent event)
	{
		if (mediaController == null)
			return false;
		mediaController.show();
		return true;
	}

	private boolean isPaused()
	{
		return audioTrack == null || audioTrack.getPlayState() == AudioTrack.PLAYSTATE_PAUSED;
	}

	private void resume()
	{
		if (audioTrack != null) {
			audioTrack.play();
			synchronized (this) {
				notify();
			}
		}
	}

	private void playSong(int song)
	{
		this.song = song;
		try {
			synchronized (asap) {
				asap.playSong(song, info.getLoop(song) ? -1 : info.getDuration(song));
			}
		}
		catch (Exception ex) {
			showError(R.string.invalid_file);
			return;
		}
		resume();
		int songs = info.getSongs();
		if (songs > 1)
			setTag(R.id.song, getString(R.string.song_format, song + 1, songs));
		else
			setTag(R.id.song, "");
	}

	private void playNextSong()
	{
		if (song + 1 < info.getSongs())
			playSong(song + 1);
	}

	private void playPreviousSong()
	{
		if (song > 0)
			playSong(song - 1); 
	}

	private void seek(int pos)
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
		int config = info.getChannels() == 1 ? AudioFormat.CHANNEL_CONFIGURATION_MONO : AudioFormat.CHANNEL_CONFIGURATION_STEREO;
		int len = AudioTrack.getMinBufferSize(ASAP.SAMPLE_RATE, config, AudioFormat.ENCODING_PCM_8BIT);
		if (len < 16384)
			len = 16384;
		final byte[] buffer = new byte[len];
		audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, ASAP.SAMPLE_RATE, config, AudioFormat.ENCODING_PCM_8BIT, len, AudioTrack.MODE_STREAM);
		audioTrack.play();

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

	private static InputStream httpGet(Uri uri)
		throws IOException
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

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		Uri uri = getIntent().getData();
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
		}
		catch (Exception ex) {
			showError(R.string.invalid_file);
			return;
		}
		info = asap.getInfo();

		setTitle(R.string.playing_title);
		setContentView(R.layout.playing);
		setTag(R.id.name, info.getTitleOrFilename());
		setTag(R.id.author, info.getAuthor());
		setTag(R.id.date, info.getDate());
		findViewById(R.id.stop_button).setOnClickListener(new OnClickListener() {
			public void onClick(View v) { finish(); }
		});
		mediaController = new MediaController(this, false);
		mediaController.setAnchorView(getContentView());
		mediaController.setMediaPlayer(new MediaController.MediaPlayerControl() {
			public boolean canPause() { return !isPaused(); }
			public boolean canSeekBackward() { return false; }
			public boolean canSeekForward() { return false; }
			public int getBufferPercentage() { return 100; }
			public int getCurrentPosition() { return asap.getPosition(); }
			public int getDuration() { return info.getDuration(song); }
			public boolean isPlaying() { return !isPaused(); }
			public void pause() { audioTrack.pause(); }
			public void seekTo(int pos) { seek(pos); }
			public void start() { resume(); }
		});
		if (info.getSongs() > 1) {
			mediaController.setPrevNextListeners(new OnClickListener() {
				public void onClick(View v) { playNextSong(); }
			},
			new OnClickListener() {
				public void onClick(View v) { playPreviousSong(); }
			});
		}
		new Handler().postDelayed(new Runnable() {
			public void run() { mediaController.show(); }
		}, 500);

		stop = false;
		playSong(info.getDefaultSong());
		new Thread(this).start();
	}

	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event)
	{
		if (mediaController == null) {
			// error shown
			return super.onKeyDown(keyCode, event);
		}
		switch (keyCode) {
		case KeyEvent.KEYCODE_MEDIA_PLAY_PAUSE:
			if (isPaused())
				resume();
			else
				audioTrack.pause();
			return true;
		case KeyEvent.KEYCODE_MEDIA_NEXT:
			playNextSong();
			return true;
		case KeyEvent.KEYCODE_MEDIA_PREVIOUS:
			playPreviousSong();
			return true;
		default:
			return super.onKeyDown(keyCode, event);
		}
	}

	@Override
	public void onStop()
	{
		super.onStop();
		synchronized (this) {
			stop = true;
			notify();
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		return Util.onCreateOptionsMenu(this, menu);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item)
	{
		return Util.onOptionsItemSelected(this, item);
	}
}
