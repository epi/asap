/*
 * Util.java - ASAP for Android
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
import android.net.Uri;
import java.io.InputStreamReader;
import java.io.IOException;
import java.io.LineNumberReader;

class Util
{
	static final Uri asmaRoot = Uri.fromParts("asma", "", null);

	static Uri getAsmaUri(String path)
	{
		return Uri.fromParts("asma", path, null);
	}

	static LineNumberReader openIndex(Context context) throws IOException
	{
		return new LineNumberReader(new InputStreamReader(context.getAssets().open("index.txt")));
	}

	static boolean matches(String value, String query)
	{
		int pos = -1;
		do {
			if (value.regionMatches(true, ++pos, query, 0, query.length())
			 || (pos + 1 < value.length() && value.charAt(pos) == '(' && value.regionMatches(true, ++pos, query, 0, query.length())))
				return true;
			pos = value.indexOf(' ', pos);
		} while (pos >= 0);
		return false;
	}
}
