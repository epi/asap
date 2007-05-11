/*
 * ASAP2WAV.java - converter of ASAP-supported formats to WAV files
 *
 * Copyright (C) 2007  Piotr Fusik
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

import java.io.*;
import net.sf.asap.*;

public class ASAP2WAV
{
	private static final int BITS_PER_SAMPLE = 16;

	private static void writeTag(OutputStream os, String tag) throws IOException
	{
		os.write(tag.charAt(0));
		os.write(tag.charAt(1));
		os.write(tag.charAt(2));
		os.write(tag.charAt(3));
	}

	private static void writeShort(OutputStream os, int x) throws IOException
	{
		os.write(x);
		os.write(x >> 8);
	}

	private static void writeInt(OutputStream os, int x) throws IOException
	{
		os.write(x);
		os.write(x >> 8);
		os.write(x >> 16);
		os.write(x >> 24);
	}

	public static void main(String[] args) throws IOException
	{
		if (args.length != 2) {
			System.err.println("Usage: java ASAP2WAV inputfile outputfile");
			System.exit(1);
		}
		InputStream is = new FileInputStream(args[0]);
		byte[] module = new byte[ASAP.MODULE_MAX];
		int module_len = is.read(module);
		is.close();
		ASAP asap = new ASAP();
		asap.load(args[0], module, module_len);
		ASAP_ModuleInfo module_info = asap.getModuleInfo();
		int song = module_info.default_song;
		int duration = 10000;
		asap.playSong(song, duration);
		int block_size = module_info.channels * (BITS_PER_SAMPLE / 8);
		int bytes_per_second = ASAP.SAMPLE_RATE * block_size;
		int n_bytes = duration * (ASAP.SAMPLE_RATE / 100) / 10 * block_size;
		OutputStream os = new FileOutputStream(args[1]);
		writeTag(os, "RIFF");
		writeInt(os, n_bytes + 36);
		writeTag(os, "WAVE");
		writeTag(os, "fmt ");
		writeInt(os, 16);
		writeShort(os, 1);
		writeShort(os, module_info.channels);
		writeInt(os, ASAP.SAMPLE_RATE);
		writeInt(os, bytes_per_second);
		writeShort(os, block_size);
		writeShort(os, BITS_PER_SAMPLE);
		writeTag(os, "data");
		writeInt(os, n_bytes);
		byte[] buffer = new byte[8192];
		do {
			n_bytes = asap.generate(buffer, BITS_PER_SAMPLE);
			os.write(buffer, 0, n_bytes);
		}  while (n_bytes == buffer.length);
		os.close();
	}
}
