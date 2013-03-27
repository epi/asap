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
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.io.IOException;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Enumeration;
import java.util.HashSet;
import java.util.zip.ZipFile;
import java.util.zip.ZipEntry;

public class FileSelector extends ListActivity
{
	private boolean isDetails;
	private File path;
	private String zipPath;

	private void onAccessDenied()
	{
		Toast.makeText(this, R.string.access_denied, Toast.LENGTH_SHORT).show();
	}

	private static interface LazyInputStream
	{
		InputStream get() throws IOException;
	}

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
			byte[] module = new byte[ASAPInfo.MAX_MODULE_LENGTH];
			int moduleLen = Util.readAndClose(is, module);
			ASAPInfo info = new ASAPInfo();
			info.load(filename, module, moduleLen);
			this.title = info.getTitleOrFilename();
			this.author = info.getAuthor();
			this.date = info.getDate();
			this.songs = info.getSongs();
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

	private void tryAdd(Collection<FileInfo> infos, String filename, LazyInputStream lazyIs)
	{
		FileInfo info;
		if (isDetails) {
			try {
				info = new FileInfo(filename, lazyIs.get());
			}
			catch (Exception ex) {
				// don't add files we cannot read or understand
				return;
			}
		}
		else
			info = new FileInfo(filename);
		infos.add(info);
	}

	private Collection<FileInfo> listDirectory()
	{
		ArrayList<FileInfo> infos = new ArrayList<FileInfo>();
		File[] files = path.listFiles();
		if (files == null)
			onAccessDenied();
		else {
			for (final File file : files) {
				String name = file.getName();
				if (file.isDirectory())
					infos.add(new FileInfo(name + '/'));
				else if (ASAPInfo.isOurFile(name)) {
					tryAdd(infos, name, new LazyInputStream() {
						public InputStream get() throws IOException {
							return new FileInputStream(file);
						}
					});
				}
				else if (Util.isZip(name))
					infos.add(new FileInfo(name));
			}
		}
		return infos;
	}

	private Collection<FileInfo> listZipDirectory(String zipPath)
	{
		if (zipPath == null)
			zipPath = "";
		int zipPathLen = zipPath.length();
		HashSet<FileInfo> infos = new HashSet<FileInfo>();
		try {
			final ZipFile zip = new ZipFile(path);
			try {
				Enumeration<? extends ZipEntry> zipEntries = zip.entries();
				while (zipEntries.hasMoreElements()) {
					final ZipEntry zipEntry = zipEntries.nextElement();
					if (!zipEntry.isDirectory()) {
						String name = zipEntry.getName();
						if (name.startsWith(zipPath) && ASAPInfo.isOurFile(name)) {
							int i = name.indexOf('/', zipPathLen);
							if (i < 0) {
								// file
								tryAdd(infos, name.substring(zipPathLen), new LazyInputStream() {
									public InputStream get() throws IOException {
										return zip.getInputStream(zipEntry);
									}
								});
							}
							else {
								// file in a subdirectory - add subdirectory with the trailing slash
								infos.add(new FileInfo(name.substring(zipPathLen, i + 1)));
							}
						}
					}
				}
			}
			finally {
				zip.close();
			}
		}
		catch (IOException ex) {
			onAccessDenied();
		}
		this.zipPath = zipPath;
		return infos;
	}

	private void reload()
	{
		Uri uri = getIntent().getData();
		if (uri == null)
			path = Environment.getExternalStorageDirectory();
		else
			path = new File(uri.getPath());

		Collection<FileInfo> coll = path.isDirectory() ? listDirectory() : listZipDirectory(uri.getFragment());
		FileInfo[] infos = coll.toArray(new FileInfo[coll.size()]);
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
		Uri uri = zipPath == null
			? Uri.fromFile(new File(path, name))
			: Uri.fromFile(path).buildUpon().fragment(zipPath + name).build();
		Class klass = name.endsWith("/") || Util.isZip(name) ? FileSelector.class : Player.class;
		Intent intent = new Intent(Intent.ACTION_VIEW, uri, this, klass);
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
