/*
 * Player.java - ASAP for Android
 *
 * Copyright (C) 2010-2023  Piotr Fusik
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

import android.app.ListActivity;
import android.app.SearchManager;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.media.AudioManager;
import android.media.MediaMetadata;
import android.media.browse.MediaBrowser;
import android.media.session.MediaController;
import android.media.session.PlaybackState;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.SystemClock;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.SearchView;
import android.widget.SeekBar;
import android.widget.TextView;

class FileInfoAdapter extends ArrayAdapter<FileInfo>
{
	private final LayoutInflater layoutInflater;
	private String playingFilename = "";

	protected FileInfoAdapter(Context context, int rowViewResourceId, FileInfo[] infos)
	{
		super(context, rowViewResourceId, infos);
		layoutInflater = LayoutInflater.from(context);
	}

	void setPlayingFilename(String filename)
	{
		playingFilename = filename;
		notifyDataSetChanged();
	}

	private static final int VIEW_TYPE_SHUFFLE_ALL = 0;
	private static final int VIEW_TYPE_FILE = 1;

	@Override
	public int getViewTypeCount()
	{
		return 2;
	}

	@Override
	public int getItemViewType(int position)
	{
		return getItem(position) == FileInfo.SHUFFLE_ALL ? VIEW_TYPE_SHUFFLE_ALL : VIEW_TYPE_FILE;
	}

	private static class ViewHolder
	{
		private TextView title;
		private TextView author;
		private TextView date;
		private TextView songs;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent)
	{
		FileInfo info = getItem(position);
		if (info == FileInfo.SHUFFLE_ALL)
			return convertView == null ? layoutInflater.inflate(R.layout.shuffle_all_list_item, null) : convertView;

		ViewHolder holder;
		if (convertView == null) {
			convertView = layoutInflater.inflate(R.layout.fileinfo_list_item, null);
			holder = new ViewHolder();
			holder.title = (TextView) convertView.findViewById(R.id.title);
			holder.author = (TextView) convertView.findViewById(R.id.author);
			holder.date = (TextView) convertView.findViewById(R.id.date);
			holder.songs = (TextView) convertView.findViewById(R.id.songs);
			convertView.setTag(holder);
		}
		else
			holder = (ViewHolder) convertView.getTag();
		holder.title.setText(info.title);
		holder.author.setText(info.author);
		holder.date.setText(info.date);
		holder.songs.setText(info.songs > 1 ? getContext().getString(R.string.songs_format, info.songs) : null);
		convertView.setBackgroundColor(playingFilename.equals(info.filename) ? 0xc0661111 : 0);
		return convertView;
	}
}

public class Player extends ListActivity
{
	private void play(Uri uri)
	{
		startService(new Intent(Intent.ACTION_VIEW, uri, this, PlayerService.class));
	}

	private void setButtonAction(int controlId, String action)
	{
		findViewById(controlId).setOnClickListener(v -> startService(new Intent(action, null, this, PlayerService.class)));
	}

	private void showTag(int controlId, String value)
	{
		TextView control = (TextView) findViewById(controlId);
		if (value == null)
			control.setVisibility(View.GONE);
		else {
			control.setText(value);
			control.setVisibility(View.VISIBLE);
		}
	}

	private void showTime(int controlId, long milliseconds)
	{
		int seconds = (int) (milliseconds / 1000);
		((TextView) findViewById(controlId)).setText(milliseconds < 0 ? "" : String.format("%02d:%02d", seconds / 60, seconds % 60));
	}

	private int duration = 0;
	private long zeroPositionRealtime;
	private final Handler positionUpdateHandler = new Handler();

	private void showPosition(long position)
	{
		showTime(R.id.playing_position, position);
		if (duration > 0)
			((SeekBar) findViewById(R.id.seekbar)).setProgress((int) (1000L * position / duration));
	}

	private final Runnable positionUpdater = () -> {
			if (zeroPositionRealtime != 0) {
				long position = SystemClock.elapsedRealtime() - zeroPositionRealtime;
				showPosition(position);
				schedulePositionUpdate(position);
			}
		};

	private void schedulePositionUpdate(long position)
	{
		positionUpdateHandler.postDelayed(positionUpdater, 1000 - position % 1000);
	}

	private long getPosition(int progress)
	{
		return (long) progress * duration / 1000;
	}

	private final SeekBar.OnSeekBarChangeListener seekBarListener = new SeekBar.OnSeekBarChangeListener() {
			@Override
			public void onStartTrackingTouch(SeekBar seekBar)
			{
				zeroPositionRealtime = 0;
			}

			@Override
			public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser)
			{
				if (fromUser)
					showTime(R.id.playing_position, getPosition(progress));
			}

			@Override
			public void onStopTrackingTouch(SeekBar seekBar)
			{
				mediaController.getTransportControls().seekTo(getPosition(seekBar.getProgress()));
			}
		};

	private final MediaController.Callback mediaControllerCallback = new MediaController.Callback() {
			@Override
			public void onMetadataChanged(MediaMetadata metadata)
			{
				getWindow().setBackgroundDrawableResource(metadata.getLong(PlayerService.METADATA_KEY_CHANNELS) == 2 ? R.drawable.stereo : R.drawable.background);
				String filename = Uri.parse(metadata.getString(MediaMetadata.METADATA_KEY_MEDIA_URI)).getSchemeSpecificPart();
				((FileInfoAdapter) getListAdapter()).setPlayingFilename(filename);
				showTag(R.id.playing_name, metadata.getString(MediaMetadata.METADATA_KEY_TITLE));
				showTag(R.id.playing_author, metadata.getString(MediaMetadata.METADATA_KEY_ARTIST));
				showTag(R.id.playing_date, metadata.getString(MediaMetadata.METADATA_KEY_DATE));
				int songs = (int) metadata.getLong(MediaMetadata.METADATA_KEY_NUM_TRACKS);
				if (songs > 1)
					showTag(R.id.playing_song, getString(R.string.song_format, metadata.getLong(MediaMetadata.METADATA_KEY_TRACK_NUMBER), songs));
				else
					findViewById(R.id.playing_song).setVisibility(View.GONE);
				duration = (int) metadata.getLong(MediaMetadata.METADATA_KEY_DURATION);
				showTime(R.id.playing_time, duration == 0 ? -1 : duration);
				findViewById(R.id.playing_panel).setVisibility(View.VISIBLE);
			}

			@Override
			public void onPlaybackStateChanged(PlaybackState state)
			{
				long actions = state.getActions();
				findViewById(R.id.play).setVisibility((actions & PlaybackState.ACTION_PLAY) != 0 ? View.VISIBLE : View.GONE);
				findViewById(R.id.pause).setVisibility((actions & PlaybackState.ACTION_PAUSE) != 0 ? View.VISIBLE : View.GONE);
				long position = state.getPosition();
				showPosition(position);
				if (state.getState() == PlaybackState.STATE_PLAYING) {
					zeroPositionRealtime = SystemClock.elapsedRealtime() - position;
					schedulePositionUpdate(position);
				}
				else
					zeroPositionRealtime = 0; // stop an already scheduled positionUpdater
			}
		};
	private MediaController mediaController;
	private final MediaBrowser.ConnectionCallback mediaBrowserConnectionCallback = new MediaBrowser.ConnectionCallback() {
			@Override
			public void onConnected()
			{
				mediaController = new MediaController(Player.this, mediaBrowser.getSessionToken());
				mediaController.registerCallback(mediaControllerCallback);
				MediaMetadata metadata = mediaController.getMetadata();
				if (metadata != null)
					mediaControllerCallback.onMetadataChanged(metadata);
				PlaybackState state = mediaController.getPlaybackState();
				if (state != null)
					mediaControllerCallback.onPlaybackStateChanged(state);
			}

			@Override
			public void onConnectionFailed()
			{
				mediaController = null;
			}

			@Override
			public void onConnectionSuspended()
			{
				mediaController = null;
			}
		};
	private MediaBrowser mediaBrowser;

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setVolumeControlStream(AudioManager.STREAM_MUSIC);
		Intent intent = getIntent();
		if (Intent.ACTION_VIEW.equals(intent.getAction())) {
			finish();
			play(intent.getData());
		}
		else {
			setContentView(R.layout.player);
			String query = Intent.ACTION_SEARCH.equals(intent.getAction()) ? intent.getStringExtra(SearchManager.QUERY) : null;
			setListAdapter(new FileInfoAdapter(this, R.layout.fileinfo_list_item, FileInfo.listIndex(this, query)));
			setButtonAction(R.id.prev, PlayerService.ACTION_PREVIOUS);
			setButtonAction(R.id.play, PlayerService.ACTION_PLAY);
			setButtonAction(R.id.pause, PlayerService.ACTION_PAUSE);
			setButtonAction(R.id.next, PlayerService.ACTION_NEXT);
			((SeekBar) findViewById(R.id.seekbar)).setOnSeekBarChangeListener(seekBarListener);
			mediaBrowser = new MediaBrowser(this, new ComponentName(this, PlayerService.class), mediaBrowserConnectionCallback, null);
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.player, menu);

		SearchManager searchManager = (SearchManager) getSystemService(SEARCH_SERVICE);
		SearchView searchView = (SearchView) menu.findItem(R.id.menu_search).getActionView();
		searchView.setSearchableInfo(searchManager.getSearchableInfo(getComponentName()));
		return true;
	}

	@Override
	public void onStart()
	{
		super.onStart();
		mediaBrowser.connect();
	}

	@Override
	public void onStop()
	{
		mediaBrowser.disconnect();
		super.onStop();
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id)
	{
		FileInfo info = (FileInfo) l.getItemAtPosition(position);
		play(info == FileInfo.SHUFFLE_ALL ? Util.asmaRoot : Util.getAsmaUri(info.filename));
	}

	private static final int OPEN_REQUEST_CODE = 1;

	@Override
	public boolean onOptionsItemSelected(MenuItem item)
	{
		switch (item.getItemId()) {
		case R.id.menu_browse:
			Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
			intent.addCategory(Intent.CATEGORY_OPENABLE);
			intent.setType("*/*");
			startActivityForResult(intent, OPEN_REQUEST_CODE);
			return true;
		default:
			return false;
		}
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data)
	{
		if (requestCode == OPEN_REQUEST_CODE && resultCode == RESULT_OK && data != null)
			play(data.getData());
	}
}
