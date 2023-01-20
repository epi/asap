/*
 * FileInfo.java - ASAP for Android
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

import android.content.Context;
import java.io.IOException;
import java.io.LineNumberReader;
import java.util.ArrayList;

class FileInfo
{
	final String filename;
	final String title;
	final String author;
	final String date;
	final int songs;

	private FileInfo(String filename, String title, String author, String date, int songs)
	{
		this.filename = filename;
		this.title = title;
		this.author = author;
		this.date = date;
		this.songs = songs;
	}

	@Override
	public String toString()
	{
		return title;
	}

	static FileInfo getShuffleAll(Context context)
	{
		return new FileInfo(null, context.getString(R.string.shuffle_all), null, null, 0);
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
				if (query == null || Util.matches(title, query) || Util.matches(author, query))
					coll.add(new FileInfo(name, title, author, date, Integer.parseInt(songs)));
			}
		}
		catch (IOException ex) {
			// shouldn't happen
		}
		return coll.toArray(new FileInfo[coll.size()]);
	}
}
