/*
 * FileContainer.java - ASAP for Android
 *
 * Copyright (C) 2013-2015  Piotr Fusik
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
import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.IOException;
import java.util.Enumeration;
import java.util.zip.ZipFile;
import java.util.zip.ZipEntry;

/*
 * Container of files: either a directory, a ZIP directory or an M3U playlist.
 */
abstract class FileContainer
{
	protected abstract void onSongFile(String name, InputStream is);

	// directory, m3u, zip, atr
	protected void onContainer(String name)
	{
	}

	private void listAsma(Context context)
	{
		FileInfo[] infos = FileInfo.listIndex(context);
		for (int i = 1 /* skip "shuffle all" */; i < infos.length; i++)
			onSongFile(infos[i].filename, null);
	}

	private void listDirectory(File dir, boolean inputStreams) throws IOException
	{
		File[] files = dir.listFiles();
		if (files == null)
			throw new FileNotFoundException();
		for (File file : files) {
			String name = file.getName();
			if (file.isDirectory())
				onContainer(name + '/');
			else if (ASAPInfo.isOurFile(name)) {
				try {
					onSongFile(name, inputStreams ? new FileInputStream(file) : null);
				}
				catch (IOException ex) {
					// ignore files we cannot read
				}
			}
			else if (Util.endsWithIgnoreCase(name, ".zip")
				  || Util.endsWithIgnoreCase(name, ".m3u")
				  || Util.endsWithIgnoreCase(name, ".atr")) {
				onContainer(name);
			}
		}
	}

	private void listM3u(Uri uri, boolean inputStreams) throws IOException
	{
		String path = uri.getPath();
		InputStream m3uIs;
		ZipInputStream zis;
		if (Util.endsWithIgnoreCase(path, ".zip")) {
			String zipFilename = path;
			path = uri.getFragment();
			m3uIs = zis = new ZipInputStream(zipFilename, path);
		}
		else {
			m3uIs = new FileInputStream(uri.getPath());
			zis = null;
		}
		path = Util.getParent(path);
		BufferedReader lineReader = new BufferedReader(new InputStreamReader(m3uIs));

		try {
			for (;;) {
				String line = lineReader.readLine();
				if (line == null)
					break;
				if (line.length() == 0 || line.charAt(0) == '#')
					continue;
				line = line.replace('\\', '/');

				if (ASAPInfo.isOurFile(line)) {
					InputStream is;
					if (inputStreams) {
						try {
							if (zis != null)
								is = zis.openInputStream(path + line);
							else
								is = new FileInputStream(path + line);
						}
						catch (IOException ex) {
							// ignore files we cannot read
							continue;
						}
					}
					else
						is = null;
					onSongFile(line, is);
				}
			}
		}
		finally {
			lineReader.close();
		}
	}

	private void listZipDirectory(File zipFile, String zipPath, boolean inputStreams, boolean recurseZip) throws IOException
	{
		if (zipPath == null)
			zipPath = "";
		int zipPathLen = zipPath.length();
		ZipFile zip = new ZipFile(zipFile);
		try {
			Enumeration<? extends ZipEntry> zipEntries = zip.entries();
			while (zipEntries.hasMoreElements()) {
				final ZipEntry zipEntry = zipEntries.nextElement();
				if (!zipEntry.isDirectory()) {
					String name = zipEntry.getName();
					if (name.startsWith(zipPath) && (ASAPInfo.isOurFile(name) || Util.endsWithIgnoreCase(name, ".m3u"))) {
						if (!recurseZip) {
							int i = name.indexOf('/', zipPathLen);
							if (i >= 0) {
								// file in a subdirectory - add subdirectory with the trailing slash
								onContainer(name.substring(zipPathLen, i + 1));
								continue;
							}
						}
						// file
						name = name.substring(zipPathLen);
						if (Util.endsWithIgnoreCase(name, ".m3u"))
							onContainer(name);
						else {
							try {
								onSongFile(name, inputStreams ? zip.getInputStream(zipEntry) : null);
							}
							catch (IOException ex) {
								// ignore files we cannot read
							}
						}
					}
				}
			}
		}
		finally {
			zip.close();
		}
	}

	void listAtr(String filename, boolean inputStreams) throws IOException
	{
		AATRStream stream = new AATRStream(filename);
		try {
			AATR disk = stream.open();
			for (;;) {
				String name = disk.nextFile();
				if (name == null)
					break;
				if (ASAPInfo.isOurFile(name))
					onSongFile(name, inputStreams ? new AATRFileInputStream(disk) : null);
			}
		}
		finally {
			stream.close();
		}
	}

	void list(Context context, Uri uri, boolean inputStreams, boolean recurseZip) throws IOException
	{
		if (Util.isAsma(uri))
			listAsma(context);
		else {
			String path = uri.getPath();
			File file = new File(path);
			if (file.isDirectory())
				listDirectory(file, inputStreams);
			else if (Util.endsWithIgnoreCase(uri.toString(), ".m3u"))
				listM3u(uri, inputStreams);
			else if (Util.endsWithIgnoreCase(path, ".zip"))
				listZipDirectory(file, uri.getFragment(), inputStreams, recurseZip);
			else if (Util.endsWithIgnoreCase(path, ".atr"))
				listAtr(path, inputStreams);
		}
	}
}
