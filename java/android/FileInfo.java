/*
 * FileInfo.java - ASAP for Android
 *
 * Copyright (C) 2010-2022  Piotr Fusik
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
import java.io.IOException;
import java.io.LineNumberReader;
import java.util.ArrayList;

class FileInfo
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

	static FileInfo getShuffleAll(Context context)
	{
		FileInfo info = new FileInfo(null);
		info.title = context.getString(R.string.shuffle_all);
		return info;
	}

	static FileInfo[] listIndex(Context context, String query)
	{
		ArrayList<FileInfo> coll = new ArrayList<FileInfo>();
		if (query == null)
			coll.add(getShuffleAll(context));
		try (LineNumberReader r = Util.openIndex(context)) {
			for (;;) {
				String name = r.readLine();
				if (name == null)
					break;
				String title = r.readLine();
				String author = r.readLine();
				String date = r.readLine();
				String songs = r.readLine();
				if (query == null || Util.matches(title, query) || Util.matches(author, query)) {
					FileInfo info = new FileInfo(name);
					info.title = title;
					info.author = author;
					info.date = date;
					info.songs = Integer.parseInt(songs);
					coll.add(info);
				}
			}
		}
		catch (IOException ex) {
			// shouldn't happen
		}
		return coll.toArray(new FileInfo[coll.size()]);
	}
}
