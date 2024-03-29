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
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.SearchView;
import android.widget.TextView;

class FileInfoAdapter extends ArrayAdapter<FileInfo>
{
	private final LayoutInflater layoutInflater;

	protected FileInfoAdapter(Context context, int rowViewResourceId, FileInfo[] infos)
	{
		super(context, rowViewResourceId, infos);
		layoutInflater = LayoutInflater.from(context);
	}

	private static class ViewHolder
	{
		private TextView title;
		private TextView author;
		private TextView date;
		private TextView songs;
	}

	public View getView(int position, View convertView, ViewGroup parent)
	{
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

		FileInfo info = getItem(position);
		holder.title.setText(info.title);
		holder.author.setText(info.author);
		holder.date.setText(info.date);
		holder.songs.setText(info.songs > 1 ? getContext().getString(R.string.songs_format, info.songs) : null);

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

	private void setTag(int controlId, String value)
	{
		TextView control = (TextView) findViewById(controlId);
		if (value == null)
			control.setVisibility(View.GONE);
		else {
			control.setText(value);
			control.setVisibility(View.VISIBLE);
		}
	}

	private final MediaController.Callback mediaControllerCallback = new MediaController.Callback() {
			@Override
			public void onMetadataChanged(MediaMetadata metadata)
			{
				setTag(R.id.playing_name, metadata.getString(MediaMetadata.METADATA_KEY_TITLE));
				setTag(R.id.playing_author, metadata.getString(MediaMetadata.METADATA_KEY_ARTIST));
				setTag(R.id.playing_date, metadata.getString(MediaMetadata.METADATA_KEY_DATE));
				int songs = (int) metadata.getLong(MediaMetadata.METADATA_KEY_NUM_TRACKS);
				if (songs > 1)
					setTag(R.id.playing_song, getString(R.string.song_format, metadata.getLong(MediaMetadata.METADATA_KEY_TRACK_NUMBER), songs));
				else
					findViewById(R.id.playing_song).setVisibility(View.GONE);
				findViewById(R.id.playing_panel).setVisibility(View.VISIBLE);
			}

			@Override
			public void onPlaybackStateChanged(PlaybackState state)
			{
				long actions = state.getActions();
				findViewById(R.id.play).setVisibility((actions & PlaybackState.ACTION_PLAY) != 0 ? View.VISIBLE : View.GONE);
				findViewById(R.id.pause).setVisibility((actions & PlaybackState.ACTION_PAUSE) != 0 ? View.VISIBLE : View.GONE);
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
		String name = ((FileInfo) l.getItemAtPosition(position)).filename;
		play(name == null ? Util.asmaRoot : Util.getAsmaUri(name));
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
