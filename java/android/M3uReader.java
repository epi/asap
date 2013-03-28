/*
 * M3uReader.java - ASAP for Android
 *
 * Copyright (C) 2013  Piotr Fusik
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

import android.net.Uri;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.IOException;
import java.io.Reader;
import java.util.zip.ZipFile;

public class M3uReader
{
	private ZipFile zip;
	private String baseDir;
	private BufferedReader lineReader;	

	public M3uReader(Uri uri) throws IOException
	{
		Reader reader;
		String path = uri.getPath();
		if (Util.isZip(path)) {
			zip = new ZipFile(path);
			path = uri.getFragment();
			reader = new InputStreamReader(zip.getInputStream(zip.getEntry(path)));
		}
		else
			reader = new FileReader(path);
		baseDir = new File(path).getParent();
		lineReader = new BufferedReader(reader);
	}

	public boolean isInZip()
	{
		return zip != null;
	}

	public String readFilename() throws IOException
	{
		String line;
		do {
			line = lineReader.readLine();
			if (line == null)
				return null;
		} while (line.length() == 0 || line.charAt(0) == '#');
		line = line.replace('\\', '/');
		return line;
	}

	public InputStream openInputStream(String filename) throws IOException
	{
		File file = new File(baseDir, filename);
		if (zip != null)
			return zip.getInputStream(zip.getEntry(file.getPath()));
		return new FileInputStream(file);
	}

	public void close() throws IOException
	{
		lineReader.close();
		if (zip != null)
			zip.close();
	}
}
