/*
 * ASAP2WAV.java - converter of ASAP-supported formats to WAV files
 *
 * Copyright (C) 2007-2008  Piotr Fusik
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
import net.sf.asap.ASAP;
import net.sf.asap.ASAP_ModuleInfo;

public class ASAP2WAV
{
	private static boolean noInputFiles = true;
	private static String outputFilename = null;
	private static boolean outputHeader = true;
	private static int song = -1;
	private static int bitsPerSample = 16;
	private static int duration = -1;
	private static int muteMask = 0;

	private static void printHelp()
	{
		System.out.print(
			"Usage: java ASAP2WAV [OPTIONS] INPUTFILE...\n" +
			"Each INPUTFILE must be in a supported format:\n" +
			"SAP, CMC, CMR, DMC, MPT, MPD, RMT, TMC, TM8 or TM2.\n" +
			"Options:\n" +
			"-o FILE     --output=FILE      Set output file name\n" +
			"-o -        --output=-         Write to standard output\n" +
			"-s SONG     --song=SONG        Select subsong number (zero-based)\n" +
			"-t TIME     --time=TIME        Set output length MM:SS\n" +
			"-b          --byte-samples     Output 8-bit samples\n" +
			"-w          --word-samples     Output 16-bit samples (default)\n" +
			"            --raw              Output raw audio (no WAV header)\n" +
			"-m CHANNELS --mute=CHANNELS    Mute POKEY chanels (1-8)\n" +
			"-h          --help             Display this information\n" +
			"-v          --version          Display version information\n"
		);
		noInputFiles = false;
	}

	private static void printVersion()
	{
		System.out.println("ASAP2WAV " + ASAP.VERSION);
		noInputFiles = false;
	}

	private static void setSong(String s)
	{
		song = Integer.parseInt(s);
	}

	private static void setTime(String s)
	{
		duration = ASAP.parseDuration(s);
	}

	private static void setMuteMask(String s)
	{
		int mask = 0;
		for (int i = 0; i < s.length(); i++) {
			int c = s.charAt(i);
			if (c >= '1' && c <= '8')
				mask |= 1 << (c - '1');
		}
		muteMask = mask;
	}

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

	private static void processFile(String inputFilename) throws IOException
	{
		InputStream is = new FileInputStream(inputFilename);
		byte[] module = new byte[ASAP.MODULE_MAX];
		int module_len = is.read(module);
		is.close();
		ASAP asap = new ASAP();
		asap.load(inputFilename, module, module_len);
		ASAP_ModuleInfo module_info = asap.getModuleInfo();
		if (song < 0)
			song = module_info.default_song;
		if (duration < 0) {
			duration = module_info.durations[song];
			if (duration < 0)
				duration = 180 * 1000;
		}
		asap.playSong(song, duration);
		asap.mutePokeyChannels(muteMask);
		if (outputFilename == null) {
			int i = inputFilename.lastIndexOf('.');
			outputFilename = inputFilename.substring(0, i) + ".wav";
		}
		OutputStream os;
		if (outputFilename.equals("-"))
			os = System.out;
		else
			os = new FileOutputStream(outputFilename);
		int n_bytes;
		if (outputHeader) {
			int block_size = module_info.channels * (bitsPerSample / 8);
			int bytes_per_second = ASAP.SAMPLE_RATE * block_size;
			n_bytes = duration * (ASAP.SAMPLE_RATE / 100) / 10 * block_size;
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
			writeShort(os, bitsPerSample);
			writeTag(os, "data");
			writeInt(os, n_bytes);
		}
		byte[] buffer = new byte[8192];
		do {
			n_bytes = asap.generate(buffer, bitsPerSample);
			os.write(buffer, 0, n_bytes);
		} while (n_bytes == buffer.length);
		os.close();
		outputFilename = null;
		song = -1;
		duration = -1;
		noInputFiles = false;
	}

	public static void main(String[] args) throws IOException
	{
		for (int i = 0; i < args.length; i++) {
			String arg = args[i];
			if (arg.charAt(0) != '-')
				processFile(arg);
			else if (arg.equals("-o"))
				outputFilename = args[++i];
			else if (arg.startsWith("--output="))
				outputFilename = arg.substring(9);
			else if (arg.equals("-s"))
				setSong(args[++i]);
			else if (arg.startsWith("--song="))
				setSong(arg.substring(7));
			else if (arg.equals("-t"))
				setTime(args[++i]);
			else if (arg.startsWith("--time="))
				setTime(arg.substring(7));
			else if (arg.equals("-b") || arg.equals("--byte-samples"))
				bitsPerSample = 8;
			else if (arg.equals("-w") || arg.equals("--word-samples"))
				bitsPerSample = 16;
			else if (arg.equals("--raw"))
				outputHeader = false;
			else if (arg.equals("-m"))
				setMuteMask(args[++i]);
			else if (arg.startsWith("--mute="))
				setMuteMask(arg.substring(7));
			else if (arg.equals("-h") || arg.equals("--help"))
				printHelp();
			else if (arg.equals("-v") || arg.equals("--version"))
				printVersion();
			else
				throw new IllegalArgumentException("unknown option: " + arg);
		}
		if (noInputFiles) {
			printHelp();
			System.exit(1);
		}
	}
}
