/*
 * FileSelector.java - ASAP for Android
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

import android.app.ListActivity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ListAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;
import java.io.InputStream;
import java.io.IOException;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashSet;

public class FileSelector extends ListActivity
{
	private boolean isDetails;
	private Uri uri;
	private boolean isM3u;

	private static class FileInfo implements Comparable<FileInfo>
	{
		private String filename;
		private String title;
		private String author;
		private String date;
		private int songs;

		private FileInfo(String filename)
		{
			this.title = this.filename = filename;
		}

		private FileInfo(String filename, InputStream is) throws Exception
		{
			this(filename);
			if (is != null) {
				byte[] module = new byte[ASAPInfo.MAX_MODULE_LENGTH];
				int moduleLen = Util.readAndClose(is, module);
				ASAPInfo info = new ASAPInfo();
				info.load(filename, module, moduleLen);
				this.title = info.getTitleOrFilename();
				this.author = info.getAuthor();
				this.date = info.getDate();
				this.songs = info.getSongs();
			}
		}

		@Override
		public String toString()
		{
			return title;
		}

		@Override
		public boolean equals(Object obj)
		{
			if (!(obj instanceof FileInfo))
				return false;
			FileInfo that = (FileInfo) obj;
			return this.filename.equals(that.filename);
		}

		@Override
		public int hashCode()
		{
			return filename.hashCode();
		}

		public int compareTo(FileInfo that)
		{
			boolean dir1 = this.filename.endsWith("/");
			boolean dir2 = that.filename.endsWith("/");
			if (dir1 != dir2)
				return dir1 ? -1 : 1;
			return this.title.compareTo(that.title);
		}
	}

	private static class FileInfoAdapter extends ArrayAdapter<FileInfo>
	{
		private LayoutInflater layoutInflater;

		private FileInfoAdapter(Context context, int rowViewResourceId, FileInfo[] infos)
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
			holder.songs.setText(info.songs > 1 ? getContext().getString(R.string.songs_format, info.songs) : "");

			return convertView;
		}
	}

	private void reload()
	{
		uri = getIntent().getData();
		if (uri == null)
			uri = Uri.fromFile(Environment.getExternalStorageDirectory());

		final Collection<FileInfo> coll = Util.isZip(uri.getPath()) ? new HashSet<FileInfo>() : new ArrayList<FileInfo>();
		try {
			isM3u = FileContainer.list(uri, new FileContainer.Consumer() {
					public void onSongFile(String name, InputStream is) throws Exception {
						coll.add(new FileInfo(name, is));
					}
					public void onContainer(String name) {
						coll.add(new FileInfo(name));
					}
				}, true, false);
		}
		catch (IOException ex) {
			Toast.makeText(this, R.string.access_denied, Toast.LENGTH_SHORT).show();
		}

		FileInfo[] infos = coll.toArray(new FileInfo[coll.size()]);
		if (!isM3u)
			Arrays.sort(infos);
		ListAdapter adapter = isDetails
			? new FileInfoAdapter(this, R.layout.fileinfo_list_item, infos)
			: new ArrayAdapter<FileInfo>(this, R.layout.filename_list_item, infos);
		setListAdapter(adapter);
	}

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		getListView().setTextFilterEnabled(true);
		isDetails = getPreferences(MODE_PRIVATE).getBoolean("fileDetails", false);
		reload();
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id)
	{
		FileInfo info = (FileInfo) l.getItemAtPosition(position);
		String name = info.filename;
		Class klass = ASAPInfo.isOurFile(name) ? Player.class : FileSelector.class;
		Intent intent = new Intent(Intent.ACTION_VIEW, Util.buildUri(uri, name), this, klass);
		if (isM3u)
			intent.putExtra(PlayerService.EXTRA_PLAYLIST, uri.toString());
		startActivity(intent);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
		getMenuInflater().inflate(R.menu.file_selector, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item)
	{
		switch (item.getItemId()) {
		case R.id.menu_toggle_details:
			isDetails = !isDetails;
			getPreferences(MODE_PRIVATE).edit().putBoolean("fileDetails", isDetails).commit();
			reload();
			return true;
		case R.id.menu_about:
			Util.showAbout(this);
			return true;
		default:
			return false;
		}
	}
}
