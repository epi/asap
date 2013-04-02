/*
 * Util.java - ASAP for Android
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

import android.app.Activity;
import android.app.AlertDialog;
import android.net.Uri;
import java.io.File;
import java.io.InputStream;
import java.io.IOException;
import java.lang.reflect.Method;

class Util
{
	static boolean isZip(String filename)
	{
		int n = filename.length();
		return n >= 4 && filename.regionMatches(true, n - 4, ".zip", 0, 4);
	}

	static boolean isM3u(String filename)
	{
		int n = filename.length();
		return n >= 4 && filename.regionMatches(true, n - 4, ".m3u", 0, 4);
	}

	static String getParent(String path)
	{
		int i = path.lastIndexOf('/');
		if (i < 0)
			return "";
		return path.substring(0, i + 1);
	}

	static Uri getParent(Uri uri)
	{
		String path = uri.getFragment();
		if (path != null)
			return uri.buildUpon().fragment(getParent(path)).build();
		return Uri.fromFile(new File(getParent(uri.getPath())));
	}

	static String stripM3u(String path)
	{
		return isM3u(path) ? getParent(path) : path;
	}

	static Uri buildUri(Uri baseUri, String relativePath)
	{
		String path = baseUri.getPath();
		if (isZip(path)) {
			String zipPath = baseUri.getFragment();
			if (zipPath == null)
				zipPath = relativePath;
			else {
				zipPath = stripM3u(zipPath) + relativePath;
			}
			return baseUri.buildUpon().fragment(zipPath).build();
		}
		return Uri.fromFile(new File(stripM3u(path), relativePath));
	}

	/**
	 * Reads bytes from the stream into the byte array
	 * until end of stream or array is full.
	 * @param is source stream
	 * @param b output array
	 * @return number of bytes read
	 */
	static int readAndClose(InputStream is, byte[] b) throws IOException
	{
		int got = 0;
		int len = b.length;
		try {
			while (got < len) {
				int i = is.read(b, got, len - got);
				if (i <= 0)
					break;
				got += i;
			}
		}
		finally {
			is.close();
		}
		return got;
	}

	static boolean invokeMethod(Object thiz, String name, Object... args)
	{
		Class[] classes = new Class[args.length];
		for (int i = 0; i < args.length; i++)
			classes[i] = args[i].getClass();
		try {
			Method method = thiz.getClass().getMethod(name, classes);
			method.invoke(thiz, args);
		}
		catch (Exception ex) {
			return false;
		}
		return true;
	}

	static void showAbout(Activity activity)
	{
		new AlertDialog.Builder(activity).setTitle(R.string.about_title).setIcon(R.drawable.icon).setMessage(R.string.about_message).show();
	}
}
