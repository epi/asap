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
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.os.IBinder;
import android.net.Uri;
import android.view.KeyEvent;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.View.OnClickListener;
import android.widget.MediaController;
import android.widget.TextView;

public class Player extends Activity
{
	private PlayerService service;
	private final ServiceConnection connection = new ServiceConnection() {
		public void onServiceConnected(ComponentName className, IBinder service)
		{
			Player.this.service = ((PlayerService.LocalBinder) service).getService();
			showInfo();
		}
		public void onServiceDisconnected(ComponentName className)
		{
			Player.this.service = null;
		}
	};
	private MediaController mediaController;

	private View getContentView()
	{
		ViewGroup vg = (ViewGroup) getWindow().getDecorView();
		return vg.getChildAt(0);
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

	void showInfo()
	{
		ASAPInfo info = service.info;
		if (info == null)
			return;
		setTag(R.id.name, info.getTitleOrFilename());
		setTag(R.id.author, info.getAuthor());
		setTag(R.id.date, info.getDate());
		int songs = info.getSongs();
		if (songs > 1)
			setTag(R.id.song, getString(R.string.song_format, service.song + 1, songs));
		else
			setTag(R.id.song, "");
		if (mediaController == null) {
			mediaController = new MediaController(this, false);
			mediaController.setAnchorView(getContentView());
			mediaController.setMediaPlayer(new MediaController.MediaPlayerControl() {
					public boolean canPause() { return !service.isPaused(); }
					public boolean canSeekBackward() { return false; }
					public boolean canSeekForward() { return false; }
					public int getBufferPercentage() { return 100; }
					public int getCurrentPosition() { return service.getPosition(); }
					public int getDuration() { return service.getDuration(); }
					public boolean isPlaying() { return !service.isPaused(); }
					public void pause() { service.pause(); }
					public void seekTo(int pos) { service.seek(pos); }
					public void start() { service.resume(); }
				});
			if (songs > 1) {
				mediaController.setPrevNextListeners(new OnClickListener() {
					public void onClick(View v) { service.playNextSong(); }
				},
				new OnClickListener() {
					public void onClick(View v) { service.playPreviousSong(); }
				});
			}
			mediaController.show(2000000000);
		}
	}

	static final String ACTION_SHOW_INFO = "net.sf.asap.action.SHOW_INFO";

	private BroadcastReceiver receiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {
			showInfo();
		}
	};

	@Override
	protected void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setTitle(R.string.playing_title);
		setContentView(R.layout.playing);

		Uri uri = getIntent().getData();
		Intent intent = new Intent(Intent.ACTION_VIEW, uri, this, PlayerService.class);
		if (uri != null)
			startService(intent);
		bindService(intent, connection, Context.BIND_AUTO_CREATE);

		findViewById(R.id.stop_button).setOnClickListener(new OnClickListener() {
			public void onClick(View v) {
				unbindService(connection);
				service.stopSelf();
				finish();
			}
		});

		registerReceiver(receiver, new IntentFilter(ACTION_SHOW_INFO));
	}

	@Override
	protected void onDestroy()
	{
		super.onDestroy();
		unregisterReceiver(receiver);
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
			service.togglePause();
			return true;
		case KeyEvent.KEYCODE_MEDIA_NEXT:
			service.playNextSong();
			return true;
		case KeyEvent.KEYCODE_MEDIA_PREVIOUS:
			service.playPreviousSong();
			return true;
		default:
			return super.onKeyDown(keyCode, event);
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
