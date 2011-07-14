/*
 * FileSelector.java - ASAP for Android
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

import android.app.AlertDialog;
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
import java.io.File;
import java.io.IOException;
import java.util.Comparator;
import java.util.Enumeration;
import java.util.HashSet;
import java.util.zip.ZipFile;
import java.util.zip.ZipEntry;

public class FileSelector extends ListActivity
{
	private ArrayAdapter<String> listAdapter;
	private File currentDir;
	private String zipPath;

	private void onAccessDenied()
	{
		new AlertDialog.Builder(this).setMessage(R.string.access_denied).show();
	}

	private void sortDirectory()
	{
		listAdapter.sort(new Comparator<String>() {
			public int compare(String name1, String name2)
			{
				if (name1.equals(".."))
					return -1;
				if (name2.equals(".."))
					return 1;
				boolean dir1 = name1.endsWith("/");
				boolean dir2 = name2.endsWith("/");
				if (dir1 != dir2)
					return dir1 ? -1 : 1;
				return name1.compareTo(name2);
			}
		});
		getListView().setSelection(0); // scroll to the top
	}

	private void enterDirectory(File dir)
	{
		File[] files = dir.listFiles();
		if (files == null) {
			onAccessDenied();
			return;
		}
		currentDir = dir;
		zipPath = null;
		listAdapter.clear();
		if (dir.getParentFile() != null)
			listAdapter.add("..");
		for (File file : files) {
			String name = file.getName();
			if (file.isDirectory())
				listAdapter.add(name + '/');
			else if (ASAPInfo.isOurFile(name) || Util.isZip(name))
				listAdapter.add(name);
		}
		sortDirectory();
	}

	private void enterZipDirectory(String path)
	{
		int pathLen = path.length();
		HashSet<String> names = new HashSet<String>();
		ZipFile zip = null;
		try {
			zip = new ZipFile(currentDir);
			Enumeration<? extends ZipEntry> entries = zip.entries();
			while (entries.hasMoreElements()) {
				ZipEntry entry = entries.nextElement();
				if (!entry.isDirectory()) {
					String name = entry.getName();
					if (name.startsWith(path) && ASAPInfo.isOurFile(name)) {
						int i = name.indexOf('/', pathLen);
						if (i < 0)
							name = name.substring(pathLen); // file
						else
							name = name.substring(pathLen, i + 1); // file in a subdirectory - add subdirectory with the trailing slash
						names.add(name);
					}
				}
			}
		}
		catch (IOException ex) {
			onAccessDenied();
			return;
		}
		finally {
			Util.close(zip);
		}
		zipPath = path;
		listAdapter.clear();
		listAdapter.add("..");
		for (String name : names)
			listAdapter.add(name);
		sortDirectory();
	}

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		listAdapter = new ArrayAdapter<String>(this, R.layout.list_item);
		setListAdapter(listAdapter);
		enterDirectory(Environment.getExternalStorageDirectory());
	}

	private void play(Uri uri)
	{
		Intent intent = new Intent(Intent.ACTION_VIEW, uri, this, Player.class);
		startActivity(intent);
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id)
	{
		String name = listAdapter.getItem(position);
		if (zipPath == null) {
			if (name.equals("..")) {
				enterDirectory(currentDir.getParentFile());
				return;
			}
			File file = new File(currentDir, name);
			if (name.endsWith("/"))
				enterDirectory(file);
			else if (Util.isZip(name)) {
				currentDir = file;
				enterZipDirectory("");
			}
			else
				play(Uri.fromFile(file));
		}
		else {
			if (name.equals("..")) {
				if (zipPath.length() == 0)
					enterDirectory(currentDir.getParentFile());
				else {
					int i = zipPath.lastIndexOf('/', zipPath.length() - 2);
					// the following also handles i==-1
					enterZipDirectory(zipPath.substring(0, i + 1));
				}
			}
			else if (name.endsWith("/"))
				enterZipDirectory(zipPath + name);
			else
				play(new Uri.Builder().scheme("file").path(currentDir.getAbsolutePath()).fragment(zipPath + name).build());
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
