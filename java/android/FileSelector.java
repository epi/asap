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
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;
import java.io.File;
import java.io.IOException;
import java.util.Arrays;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Comparator;
import java.util.Enumeration;
import java.util.HashSet;
import java.util.zip.ZipFile;
import java.util.zip.ZipEntry;

public class FileSelector extends ListActivity
{
	private File path;
	private String zipPath;

	private void onAccessDenied()
	{
		Toast.makeText(this, R.string.access_denied, Toast.LENGTH_SHORT).show();
	}

	private Collection<String> listDirectory()
	{
		ArrayList<String> names = new ArrayList<String>();
		File[] files = path.listFiles();
		if (files == null)
			onAccessDenied();
		else {
			for (File file : files) {
				String name = file.getName();
				if (file.isDirectory())
					names.add(name + '/');
				else if (ASAPInfo.isOurFile(name) || Util.isZip(name))
					names.add(name);
			}
		}
		return names;
	}

	private Collection<String> listZipDirectory(String zipPath)
	{
		if (zipPath == null)
			zipPath = "";
		int zipPathLen = zipPath.length();
		HashSet<String> names = new HashSet<String>();
		ZipFile zip = null;
		try {
			zip = new ZipFile(path);
			Enumeration<? extends ZipEntry> zipEntries = zip.entries();
			while (zipEntries.hasMoreElements()) {
				ZipEntry entry = zipEntries.nextElement();
				if (!entry.isDirectory()) {
					String name = entry.getName();
					if (name.startsWith(zipPath) && ASAPInfo.isOurFile(name)) {
						int i = name.indexOf('/', zipPathLen);
						if (i < 0)
							name = name.substring(zipPathLen); // file
						else
							name = name.substring(zipPathLen, i + 1); // file in a subdirectory - add subdirectory with the trailing slash
						names.add(name);
					}
				}
			}
		}
		catch (IOException ex) {
			onAccessDenied();
		}
		finally {
			Util.close(zip);
		}
		this.zipPath = zipPath;
		return names;
	}

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		Uri uri = getIntent().getData();
		if (uri == null)
			path = Environment.getExternalStorageDirectory();
		else
			path = new File(uri.getPath());

		Collection<String> coll = path.isDirectory() ? listDirectory() : listZipDirectory(uri.getFragment());
		String[] names = coll.toArray(new String[coll.size()]);
		Arrays.sort(names, new Comparator<String>() {
			public int compare(String name1, String name2)
			{
				boolean dir1 = name1.endsWith("/");
				boolean dir2 = name2.endsWith("/");
				if (dir1 != dir2)
					return dir1 ? -1 : 1;
				return name1.compareTo(name2);
			}
		});
		setListAdapter(new ArrayAdapter<String>(this, R.layout.list_item, names));
		getListView().setTextFilterEnabled(true);
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id)
	{
		String name = (String) l.getItemAtPosition(position);
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
		return Util.onCreateOptionsMenu(this, menu);
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item)
	{
		return Util.onOptionsItemSelected(this, item);
	}
}
