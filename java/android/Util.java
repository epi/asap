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
import java.io.InputStream;
import java.io.IOException;
import java.lang.reflect.Method;

class Util
{
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

	static boolean isZip(String filename)
	{
		int n = filename.length();
		return n >= 4 && filename.regionMatches(true, n - 4, ".zip", 0, 4);
	}
}
