/*
 * FileInfo.java - ASAP for Android
 *
 * Copyright (C) 2010-2015  Piotr Fusik
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

import android.content.Context;
import java.io.InputStreamReader;
import java.io.IOException;
import java.io.LineNumberReader;
import java.util.ArrayList;

class FileInfo implements Comparable<FileInfo>
{
	String filename;
	String title;
	String author;
	String date;
	int songs;

	FileInfo(String filename)
	{
		this.title = this.filename = filename;
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
		if (this.filename == null)
			return that.filename == null;
		return this.filename.equals(that.filename);
	}

	@Override
	public int hashCode()
	{
		return filename == null ? 0 : filename.hashCode();
	}

	public int compareTo(FileInfo that)
	{
		if (this.filename == null)
			return -1;
		if (that.filename == null)
			return 1;
		boolean dir1 = this.filename.endsWith("/");
		boolean dir2 = that.filename.endsWith("/");
		if (dir1 != dir2)
			return dir1 ? -1 : 1;
		int titleCmp = this.title.compareTo(that.title);
		if (titleCmp != 0)
			return titleCmp;
		return this.filename.compareTo(that.filename);
	}

	static FileInfo getShuffleAll(Context context)
	{
		FileInfo info = new FileInfo(null);
		info.title = context.getString(R.string.shuffle_all);
		return info;
	}

	private static FileInfo[] readIndex(Context context)
	{
		ArrayList<FileInfo> coll = new ArrayList<FileInfo>();
		coll.add(getShuffleAll(context));
		try {
			LineNumberReader r = new LineNumberReader(new InputStreamReader(context.getAssets().open("index.txt")));
			try {
				for (;;) {
					String name = r.readLine();
					if (name == null)
						break;
					FileInfo info = new FileInfo(name);
					info.title = r.readLine();
					info.author = r.readLine();
					info.date = r.readLine();
					info.songs = Integer.parseInt(r.readLine());
					coll.add(info);
				}
			}
			finally {
				r.close();
			}
		}
		catch (IOException ex) {
			// shouldn't happen
		}
		return coll.toArray(new FileInfo[coll.size()]);
	}

	private static FileInfo[] indexCache = null;

	static FileInfo[] listIndex(Context context)
	{
		if (indexCache == null) {
			synchronized (FileInfo.class) {
				if (indexCache == null)
					indexCache = readIndex(context);
			}
		}
		return indexCache;
	}
}
