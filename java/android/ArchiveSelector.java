/*
 * ArchiveSelector.java - ASAP for Android
 *
 * Copyright (C) 2015-2022  Piotr Fusik
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
import android.content.Context;
import android.content.Intent;
import android.media.AudioManager;
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
	private LayoutInflater layoutInflater;

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

public class ArchiveSelector extends ListActivity
{
	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		setVolumeControlStream(AudioManager.STREAM_MUSIC);
		Intent intent = getIntent();
		if (Intent.ACTION_VIEW.equals(intent.getAction())) {
			finish();
			startActivity(new Intent(Intent.ACTION_VIEW, intent.getData(), this, Player.class));
		}
		else {
			String query = Intent.ACTION_SEARCH.equals(intent.getAction()) ? intent.getStringExtra(SearchManager.QUERY) : null;
			setListAdapter(new FileInfoAdapter(this, R.layout.fileinfo_list_item, FileInfo.listIndex(this, query)));
		}
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id)
	{
		Intent intent;
		FileInfo info = (FileInfo) l.getItemAtPosition(position);
		String name = info.filename;
		if (name == null) {
			// shuffle all
			intent = new Intent(Intent.ACTION_VIEW, Util.asmaRoot, this, Player.class);
		}
		else
			intent = new Intent(Intent.ACTION_VIEW, Util.getAsmaUri(name), this, Player.class);
		startActivity(intent);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.archive_selector, menu);

		SearchManager searchManager = (SearchManager) getSystemService(SEARCH_SERVICE);
		SearchView searchView = (SearchView) menu.findItem(R.id.menu_search).getActionView();
		searchView.setSearchableInfo(searchManager.getSearchableInfo(getComponentName()));
		return true;
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
			startActivity(new Intent(Intent.ACTION_VIEW, data.getData(), this, Player.class));
	}
}
