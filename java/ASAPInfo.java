// Generated automatically with "cito". Do not edit.
package net.sf.asap;

/**
 * Information about a music file.
 */
public final class ASAPInfo
{

	private void addSong(int playerCalls)
	{
		this.durations[this.songs++] = (int) ((long) (playerCalls * this.fastplay) * 114000 / 1773447);
	}
	String author;
	int channels;

	private int checkDate()
	{
		int n = this.date.length();
		switch (n) {
			case 10:
				if (!this.checkTwoDateDigits(0) || this.date.charAt(2) != 47)
					return -1;
				//$FALL-THROUGH$
			case 7:
				if (!this.checkTwoDateDigits(n - 7) || this.date.charAt(n - 5) != 47)
					return -1;
				//$FALL-THROUGH$
			case 4:
				if (!this.checkTwoDateDigits(n - 4) || !this.checkTwoDateDigits(n - 2))
					return -1;
				return n;
			default:
				return -1;
		}
	}

	private boolean checkTwoDateDigits(int i)
	{
		int d1 = this.date.charAt(i);
		int d2 = this.date.charAt(i + 1);
		return d1 >= 48 && d1 <= 57 && d2 >= 48 && d2 <= 57;
	}

	private static void checkValidChar(int c) throws Exception
	{
		if (c < 32 || c > 122 || c == 34 || c == 96)
			throw new Exception("Invalid character");
	}

	private static void checkValidText(String s) throws Exception
	{
		int n = s.length();
		if (n > 127)
			throw new Exception("Text too long");
		for (int i = 0; i < n; i++)
			ASAPInfo.checkValidChar(s.charAt(i));
	}
	/**
	 * Short license notice.
	 * Display after the credits.
	 */
	public static final String COPYRIGHT = "This program is free software; you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published\nby the Free Software Foundation; either version 2 of the License,\nor (at your option) any later version.";
	int covoxAddr;
	/**
	 * Short credits for ASAP.
	 */
	public static final String CREDITS = "Another Slight Atari Player (C) 2005-2011 Piotr Fusik\nCMC, MPT, TMC, TM2 players (C) 1994-2005 Marcin Lewandowski\nRMT player (C) 2002-2005 Radek Sterba\nDLT player (C) 2009 Marek Konopka\nCMS player (C) 1999 David Spilka\n";
	String date;
	int defaultSong;
	final int[] durations = new int[32];
	int fastplay;
	private String filename;

	/**
	 * Returns author's name.
	 * A nickname may be included in parentheses after the real name.
	 * Multiple authors are separated with <code>" &amp; "</code>.
	 * An empty string means the author is unknown.
	 */
	public String getAuthor()
	{
		return this.author;
	}

	/**
	 * Returns 1 for mono or 2 for stereo.
	 */
	public int getChannels()
	{
		return this.channels;
	}

	/**
	 * Returns music creation date.
	 * Some of the possible formats are:
	 * <ul>
	 * <li>YYYY</li>
	 * <li>MM/YYYY</li>
	 * <li>DD/MM/YYYY</li>
	 * <li>YYYY-YYYY</li>
	 * </ul>
	 * An empty string means the date is unknown.
	 */
	public String getDate()
	{
		return this.date;
	}

	/**
	 * Returns day of month of the music creation date.
	 * -1 means the day is unknown.
	 */
	public int getDayOfMonth()
	{
		int n = this.checkDate();
		if (n != 10)
			return -1;
		return this.getTwoDateDigits(0);
	}

	/**
	 * Returns 0-based index of the "main" song.
	 * The specified song should be played by default.
	 */
	public int getDefaultSong()
	{
		return this.defaultSong;
	}

	/**
	 * Returns length of the specified song.
	 * The length is specified in milliseconds. -1 means the length is indeterminate.
	 */
	public int getDuration(int song)
	{
		return this.durations[song];
	}

	/**
	 * Returns human-readable description of the filename extension.
	 * @param ext Filename extension without the leading dot.
	 */
	public static String getExtDescription(String ext) throws Exception
	{
		if (ext.length() != 3)
			throw new Exception("Unknown extension");
		switch (ext.charAt(0) + (ext.charAt(1) << 8) + (ext.charAt(2) << 16) | 2105376) {
			case 7364979:
				return "Slight Atari Player";
			case 6516067:
				return "Chaos Music Composer";
			case 3370339:
				return "CMC \"3/4\"";
			case 7499107:
				return "CMC \"Rzog\"";
			case 7564643:
				return "Stereo Double CMC";
			case 6516068:
				return "DoublePlay CMC";
			case 7629924:
				return "Delta Music Composer";
			case 7630957:
				return "Music ProTracker";
			case 6582381:
				return "MPT DoublePlay";
			case 7630194:
				return "Raster Music Tracker";
			case 6516084:
			case 3698036:
				return "Theta Music Composer 1.x";
			case 3304820:
				return "Theta Music Composer 2.x";
			case 7890296:
				return "Atari 8-bit executable";
			default:
				throw new Exception("Unknown extension");
		}
	}

	/**
	 * Returns information whether the specified song loops.
	 * Returns:
	 * <ul>
	 * <li><code>true</code> if the song loops</li>
	 * <li><code>false</code> if the song stops</li>
	 * </ul>
	 * 
	 */
	public boolean getLoop(int song)
	{
		return this.loops[song];
	}

	/**
	 * Returns music creation month (1-12).
	 * -1 means the month is unknown.
	 */
	public int getMonth()
	{
		int n = this.checkDate();
		if (n < 7)
			return -1;
		return this.getTwoDateDigits(n - 7);
	}

	/**
	 * Returns the extension of the original module format.
	 * For native modules it simply returns their extension.
	 * For the SAP format it attempts to detect the original module format.
	 * @param module Contents of the file.
	 * @param moduleLen Length of the file.
	 */
	public String getOriginalModuleExt(byte[] module, int moduleLen)
	{
		switch (this.type) {
			case ASAPModuleType.SAP_B:
				if ((this.init == 1019 || this.init == 1017) && this.player == 1283)
					return "dlt";
				if (this.init == 1267 || this.init == 62707 || this.init == 1263)
					return this.fastplay == 156 ? "mpd" : "mpt";
				if (this.init == 3200 || this.getRmtSapOffset(module, moduleLen) > 0)
					return "rmt";
				if (this.init == 1269 || this.init == 62709 || this.init == 1266 || (this.init == 1255 || this.init == 62695 || this.init == 1252) && this.fastplay == 156 || (this.init == 1253 || this.init == 62693 || this.init == 1250) && (this.fastplay == 104 || this.fastplay == 78))
					return "tmc";
				if (this.init == 4224)
					return "tm2";
				return null;
			case ASAPModuleType.SAP_C:
				if ((this.player == 1280 || this.player == 62720) && moduleLen >= 1024) {
					if (this.fastplay == 156)
						return "dmc";
					if (this.channels > 1)
						return "cms";
					if (module[moduleLen - 170] == 30)
						return "cmr";
					if (module[moduleLen - 909] == 48)
						return "cm3";
					return "cmc";
				}
				return null;
			case ASAPModuleType.CMC:
				return this.fastplay == 156 ? "dmc" : "cmc";
			case ASAPModuleType.CM3:
				return "cm3";
			case ASAPModuleType.CMR:
				return "cmr";
			case ASAPModuleType.CMS:
				return "cms";
			case ASAPModuleType.DLT:
				return "dlt";
			case ASAPModuleType.MPT:
				return this.fastplay == 156 ? "mpd" : "mpt";
			case ASAPModuleType.RMT:
				return "rmt";
			case ASAPModuleType.TMC:
				return "tmc";
			case ASAPModuleType.TM2:
				return "tm2";
			default:
				return null;
		}
	}

	static int getPackedExt(String filename)
	{
		int ext = 0;
		for (int i = filename.length(); --i > 0;) {
			int c = filename.charAt(i);
			if (c <= 32 || c > 122)
				return 0;
			if (c == 46)
				return ext | 2105376;
			ext = (ext << 8) + c;
		}
		return 0;
	}

	private static int getRmtInstrumentFrames(byte[] module, int instrument, int volume, int volumeFrame, boolean onExtraPokey)
	{
		int addrToOffset = ASAPInfo.getWord(module, 2) - 6;
		instrument = ASAPInfo.getWord(module, 14) - addrToOffset + (instrument << 1);
		if (module[instrument + 1] == 0)
			return 0;
		instrument = ASAPInfo.getWord(module, instrument) - addrToOffset;
		int perFrame = module[12] & 0xff;
		int playerCall = volumeFrame * perFrame;
		int playerCalls = playerCall;
		int index = (module[instrument] & 0xff) + 1 + playerCall * 3;
		int indexEnd = (module[instrument + 2] & 0xff) + 3;
		int indexLoop = module[instrument + 3] & 0xff;
		if (indexLoop >= indexEnd)
			return 0;
		int volumeSlideDepth = module[instrument + 6] & 0xff;
		int volumeMin = module[instrument + 7] & 0xff;
		if (index >= indexEnd)
			index = (index - indexEnd) % (indexEnd - indexLoop) + indexLoop;
		else {
			do {
				int vol = module[instrument + index] & 0xff;
				if (onExtraPokey)
					vol >>= 4;
				if ((vol & 15) >= CI_CONST_ARRAY_1[volume])
					playerCalls = playerCall + 1;
				playerCall++;
				index += 3;
			}
			while (index < indexEnd);
		}
		if (volumeSlideDepth == 0)
			return playerCalls / perFrame;
		int volumeSlide = 128;
		boolean silentLoop = false;
		for (;;) {
			if (index >= indexEnd) {
				if (silentLoop)
					break;
				silentLoop = true;
				index = indexLoop;
			}
			int vol = module[instrument + index] & 0xff;
			if (onExtraPokey)
				vol >>= 4;
			if ((vol & 15) >= CI_CONST_ARRAY_1[volume]) {
				playerCalls = playerCall + 1;
				silentLoop = false;
			}
			playerCall++;
			index += 3;
			volumeSlide -= volumeSlideDepth;
			if (volumeSlide < 0) {
				volumeSlide += 256;
				if (--volume <= volumeMin)
					break;
			}
		}
		return playerCalls / perFrame;
	}

	int getRmtSapOffset(byte[] module, int moduleLen)
	{
		if (this.player != 13315)
			return -1;
		int offset = this.headerLen + ASAPInfo.getWord(module, this.headerLen + 4) - ASAPInfo.getWord(module, this.headerLen + 2) + 7;
		if (offset + 6 >= moduleLen || module[offset + 4] != 82 || module[offset + 5] != 77 || module[offset + 6] != 84)
			return -1;
		return offset;
	}

	/**
	 * Returns number of songs in the file.
	 */
	public int getSongs()
	{
		return this.songs;
	}

	/**
	 * Returns music title.
	 * An empty string means the title is unknown.
	 */
	public String getTitle()
	{
		return this.name;
	}

	/**
	 * Returns music title or filename.
	 * If title is unknown returns filename without the path or extension.
	 */
	public String getTitleOrFilename()
	{
		return this.name.length() > 0 ? this.name : this.filename;
	}

	private int getTwoDateDigits(int i)
	{
		return (this.date.charAt(i) - 48) * 10 + this.date.charAt(i + 1) - 48;
	}

	static int getWord(byte[] array, int i)
	{
		return (array[i] & 0xff) + ((array[i + 1] & 0xff) << 8);
	}

	/**
	 * Returns music creation year.
	 * -1 means the year is unknown.
	 */
	public int getYear()
	{
		int n = this.checkDate();
		if (n < 0)
			return -1;
		return this.getTwoDateDigits(n - 4) * 100 + this.getTwoDateDigits(n - 2);
	}

	private static boolean hasStringAt(byte[] module, int moduleIndex, String s)
	{
		int n = s.length();
		for (int i = 0; i < n; i++)
			if ((module[moduleIndex + i] & 0xff) != s.charAt(i))
				return false;
		return true;
	}
	int headerLen;
	int init;

	private static boolean isDltPatternEnd(byte[] module, int pos, int i)
	{
		for (int ch = 0; ch < 4; ch++) {
			int pattern = module[8198 + (ch << 8) + pos] & 0xff;
			if (pattern < 64) {
				int offset = 6 + (pattern << 7) + (i << 1);
				if ((module[offset] & 128) == 0 && (module[offset + 1] & 128) != 0)
					return true;
			}
		}
		return false;
	}

	private static boolean isDltTrackEmpty(byte[] module, int pos)
	{
		return (module[8198 + pos] & 0xff) >= 67 && (module[8454 + pos] & 0xff) >= 64 && (module[8710 + pos] & 0xff) >= 64 && (module[8966 + pos] & 0xff) >= 64;
	}

	/**
	 * Returns <code>true</code> for NTSC song and <code>false</code> for PAL song.
	 */
	public boolean isNtsc()
	{
		return this.ntsc;
	}

	/**
	 * Checks whether the filename extension represents a module type supported by ASAP.
	 * Returns <code>true</code> if the filename extension is supported by ASAP.
	 * @param ext Filename extension without the leading dot.
	 */
	public static boolean isOurExt(String ext)
	{
		return ext.length() == 3 && ASAPInfo.isOurPackedExt(ext.charAt(0) + (ext.charAt(1) << 8) + (ext.charAt(2) << 16) | 2105376);
	}

	/**
	 * Checks whether the filename represents a module type supported by ASAP.
	 * Returns <code>true</code> if the filename is supported by ASAP.
	 * @param filename Filename to check the extension of.
	 */
	public static boolean isOurFile(String filename)
	{
		return ASAPInfo.isOurPackedExt(ASAPInfo.getPackedExt(filename));
	}

	private static boolean isOurPackedExt(int ext)
	{
		switch (ext) {
			case 7364979:
			case 6516067:
			case 3370339:
			case 7499107:
			case 7564643:
			case 6516068:
			case 7629924:
			case 7630957:
			case 6582381:
			case 7630194:
			case 6516084:
			case 3698036:
			case 3304820:
				return true;
			default:
				return false;
		}
	}

	/**
	 * Loads file information.
	 * @param filename Filename, used to determine the format.
	 * @param module Contents of the file.
	 * @param moduleLen Length of the file.
	 */
	public void load(String filename, byte[] module, int moduleLen) throws Exception
	{
		int len = filename.length();
		int basename = 0;
		int ext = -1;
		for (int i = len; --i >= 0;) {
			int c = filename.charAt(i);
			if (c == 47 || c == 92) {
				basename = i + 1;
				break;
			}
			if (c == 46)
				ext = i;
		}
		if (ext < 0)
			throw new Exception("Filename has no extension");
		ext -= basename;
		if (ext > 127)
			ext = 127;
		this.filename = filename.substring(basename, basename + ext);
		this.author = "";
		this.name = "";
		this.date = "";
		this.channels = 1;
		this.songs = 1;
		this.defaultSong = 0;
		for (int i = 0; i < 32; i++) {
			this.durations[i] = -1;
			this.loops[i] = false;
		}
		this.ntsc = false;
		this.fastplay = 312;
		this.music = -1;
		this.init = -1;
		this.player = -1;
		this.covoxAddr = -1;
		switch (ASAPInfo.getPackedExt(filename)) {
			case 7364979:
				this.parseSap(module, moduleLen);
				return;
			case 6516067:
				this.parseCmc(module, moduleLen, ASAPModuleType.CMC);
				return;
			case 3370339:
				this.parseCmc(module, moduleLen, ASAPModuleType.CM3);
				return;
			case 7499107:
				this.parseCmc(module, moduleLen, ASAPModuleType.CMR);
				return;
			case 7564643:
				this.channels = 2;
				this.parseCmc(module, moduleLen, ASAPModuleType.CMS);
				return;
			case 6516068:
				this.fastplay = 156;
				this.parseCmc(module, moduleLen, ASAPModuleType.CMC);
				return;
			case 7629924:
				this.parseDlt(module, moduleLen);
				return;
			case 7630957:
				this.parseMpt(module, moduleLen);
				return;
			case 6582381:
				this.fastplay = 156;
				this.parseMpt(module, moduleLen);
				return;
			case 7630194:
				this.parseRmt(module, moduleLen);
				return;
			case 6516084:
			case 3698036:
				this.parseTmc(module, moduleLen);
				return;
			case 3304820:
				this.parseTm2(module, moduleLen);
				return;
			default:
				throw new Exception("Unknown filename extension");
		}
	}
	final boolean[] loops = new boolean[32];
	/**
	 * Maximum length of a supported input file.
	 * You may assume that files longer than this are not supported by ASAP.
	 */
	public static final int MAX_MODULE_LENGTH = 65000;
	/**
	 * Maximum number of songs in a file.
	 */
	public static final int MAX_SONGS = 32;
	/**
	 * Maximum length of text metadata.
	 */
	public static final int MAX_TEXT_LENGTH = 127;
	int music;
	String name;
	boolean ntsc;

	private void parseCmc(byte[] module, int moduleLen, int type) throws Exception
	{
		if (moduleLen < 774)
			throw new Exception("Module too short");
		this.type = type;
		this.parseModule(module, moduleLen);
		int lastPos = 84;
		while (--lastPos >= 0) {
			if ((module[518 + lastPos] & 0xff) < 176 || (module[603 + lastPos] & 0xff) < 64 || (module[688 + lastPos] & 0xff) < 64)
				break;
			if (this.channels == 2) {
				if ((module[774 + lastPos] & 0xff) < 176 || (module[859 + lastPos] & 0xff) < 64 || (module[944 + lastPos] & 0xff) < 64)
					break;
			}
		}
		this.songs = 0;
		this.parseCmcSong(module, 0);
		for (int pos = 0; pos < lastPos && this.songs < 32; pos++)
			if (module[518 + pos] == -113 || module[518 + pos] == -17)
				this.parseCmcSong(module, pos + 1);
	}

	private void parseCmcSong(byte[] module, int pos)
	{
		int tempo = module[25] & 0xff;
		int playerCalls = 0;
		int repStartPos = 0;
		int repEndPos = 0;
		int repTimes = 0;
		byte[] seen = new byte[85];
		while (pos >= 0 && pos < 85) {
			if (pos == repEndPos && repTimes > 0) {
				for (int i = 0; i < 85; i++)
					if ((seen[i] & 0xff) == 1 || (seen[i] & 0xff) == 3)
						seen[i] = 0;
				repTimes--;
				pos = repStartPos;
			}
			if (seen[pos] != 0) {
				if ((seen[pos] & 0xff) != 1)
					this.loops[this.songs] = true;
				break;
			}
			seen[pos] = 1;
			int p1 = module[518 + pos] & 0xff;
			int p2 = module[603 + pos] & 0xff;
			int p3 = module[688 + pos] & 0xff;
			if (p1 == 254 || p2 == 254 || p3 == 254) {
				pos++;
				continue;
			}
			p1 >>= 4;
			if (p1 == 8)
				break;
			switch (p1) {
				case 9:
					pos = p2;
					continue;
				case 10:
					pos -= p2;
					continue;
				case 11:
					pos += p2;
					continue;
				case 12:
					tempo = p2;
					pos++;
					continue;
				case 13:
					pos++;
					repStartPos = pos;
					repEndPos = pos + p2;
					repTimes = p3 - 1;
					continue;
				default:
					break;
			}
			if (p1 == 14) {
				this.loops[this.songs] = true;
				break;
			}
			p2 = repTimes > 0 ? 3 : 2;
			for (p1 = 0; p1 < 85; p1++)
				if ((seen[p1] & 0xff) == 1)
					seen[p1] = (byte) p2;
			playerCalls += tempo * (this.type == ASAPModuleType.CM3 ? 48 : 64);
			pos++;
		}
		this.addSong(playerCalls);
	}

	private static int parseDec(byte[] module, int moduleIndex, int maxVal) throws Exception
	{
		if (module[moduleIndex] == 13)
			throw new Exception("Missing number");
		for (int r = 0;;) {
			int c = module[moduleIndex++] & 0xff;
			if (c == 13)
				return r;
			if (c < 48 || c > 57)
				throw new Exception("Invalid number");
			r = 10 * r + c - 48;
			if (r > maxVal)
				throw new Exception("Number too big");
		}
	}

	private void parseDlt(byte[] module, int moduleLen) throws Exception
	{
		if (moduleLen != 11270 && moduleLen != 11271)
			throw new Exception("Invalid module length");
		this.type = ASAPModuleType.DLT;
		this.parseModule(module, moduleLen);
		if (this.music != 8192)
			throw new Exception("Unsupported module address");
		boolean[] seen = new boolean[128];
		this.songs = 0;
		for (int pos = 0; pos < 128 && this.songs < 32; pos++) {
			if (!seen[pos])
				this.parseDltSong(module, seen, pos);
		}
		if (this.songs == 0)
			throw new Exception("No songs found");
	}

	private void parseDltSong(byte[] module, boolean[] seen, int pos)
	{
		while (pos < 128 && !seen[pos] && ASAPInfo.isDltTrackEmpty(module, pos))
			seen[pos++] = true;
		this.songPos[this.songs] = (byte) pos;
		int playerCalls = 0;
		boolean loop = false;
		int tempo = 6;
		while (pos < 128) {
			if (seen[pos]) {
				loop = true;
				break;
			}
			seen[pos] = true;
			int p1 = module[8198 + pos] & 0xff;
			if (p1 == 64 || ASAPInfo.isDltTrackEmpty(module, pos))
				break;
			if (p1 == 65)
				pos = module[8326 + pos] & 0xff;
			else if (p1 == 66)
				tempo = module[8326 + pos++] & 0xff;
			else {
				for (int i = 0; i < 64 && !ASAPInfo.isDltPatternEnd(module, pos, i); i++)
					playerCalls += tempo;
				pos++;
			}
		}
		if (playerCalls > 0) {
			this.loops[this.songs] = loop;
			this.addSong(playerCalls);
		}
	}

	/**
	 * Returns the number of milliseconds represented by the given string.
	 * @param s Time in the <code>"mm:ss.xxx"</code> format.
	 */
	public static int parseDuration(String s) throws Exception
	{
		int i = 0;
		int n = s.length();
		int d;
		if (i >= n)
			throw new Exception("Invalid duration");
		d = s.charAt(i) - 48;
		if (d < 0 || d > 9)
			throw new Exception("Invalid duration");
		i++;
		int r = d;
		if (i < n) {
			d = s.charAt(i) - 48;
			if (d >= 0 && d <= 9) {
				i++;
				r = 10 * r + d;
			}
			if (i < n && s.charAt(i) == 58) {
				i++;
				if (i >= n)
					throw new Exception("Invalid duration");
				d = s.charAt(i) - 48;
				if (d < 0 || d > 5)
					throw new Exception("Invalid duration");
				i++;
				r = (6 * r + d) * 10;
				if (i >= n)
					throw new Exception("Invalid duration");
				d = s.charAt(i) - 48;
				if (d < 0 || d > 9)
					throw new Exception("Invalid duration");
				i++;
				r += d;
			}
		}
		r *= 1000;
		if (i >= n)
			return r;
		if (s.charAt(i) != 46)
			throw new Exception("Invalid duration");
		i++;
		if (i >= n)
			throw new Exception("Invalid duration");
		d = s.charAt(i) - 48;
		if (d < 0 || d > 9)
			throw new Exception("Invalid duration");
		i++;
		r += 100 * d;
		if (i >= n)
			return r;
		d = s.charAt(i) - 48;
		if (d < 0 || d > 9)
			throw new Exception("Invalid duration");
		i++;
		r += 10 * d;
		if (i >= n)
			return r;
		d = s.charAt(i) - 48;
		if (d < 0 || d > 9)
			throw new Exception("Invalid duration");
		i++;
		r += d;
		return r;
	}

	private static int parseHex(byte[] module, int moduleIndex) throws Exception
	{
		if (module[moduleIndex] == 13)
			throw new Exception("Missing number");
		for (int r = 0;;) {
			int c = module[moduleIndex++] & 0xff;
			if (c == 13)
				return r;
			if (r > 4095)
				throw new Exception("Number too big");
			r <<= 4;
			if (c >= 48 && c <= 57)
				r += c - 48;
			else if (c >= 65 && c <= 70)
				r += c - 65 + 10;
			else if (c >= 97 && c <= 102)
				r += c - 97 + 10;
			else
				throw new Exception("Invalid number");
		}
	}

	private void parseModule(byte[] module, int moduleLen) throws Exception
	{
		if ((module[0] != -1 || module[1] != -1) && (module[0] != 0 || module[1] != 0))
			throw new Exception("Invalid two leading bytes of the module");
		this.music = ASAPInfo.getWord(module, 2);
		int musicLastByte = ASAPInfo.getWord(module, 4);
		if (this.music <= 55295 && musicLastByte >= 53248)
			throw new Exception("Module address conflicts with hardware registers");
		int blockLen = musicLastByte + 1 - this.music;
		if (6 + blockLen != moduleLen) {
			if (this.type != ASAPModuleType.RMT || 11 + blockLen > moduleLen)
				throw new Exception("Module length doesn't match headers");
			int infoAddr = ASAPInfo.getWord(module, 6 + blockLen);
			if (infoAddr != this.music + blockLen)
				throw new Exception("Invalid address of RMT info");
			int infoLen = ASAPInfo.getWord(module, 8 + blockLen) + 1 - infoAddr;
			if (10 + blockLen + infoLen != moduleLen)
				throw new Exception("Invalid RMT info block");
		}
	}

	private void parseMpt(byte[] module, int moduleLen) throws Exception
	{
		if (moduleLen < 464)
			throw new Exception("Module too short");
		this.type = ASAPModuleType.MPT;
		this.parseModule(module, moduleLen);
		int track0Addr = ASAPInfo.getWord(module, 2) + 458;
		if ((module[454] & 0xff) + ((module[458] & 0xff) << 8) != track0Addr)
			throw new Exception("Invalid address of the first track");
		int songLen = (module[455] & 0xff) + ((module[459] & 0xff) << 8) - track0Addr >> 1;
		if (songLen > 254)
			throw new Exception("Song too long");
		boolean[] globalSeen = new boolean[256];
		this.songs = 0;
		for (int pos = 0; pos < songLen && this.songs < 32; pos++) {
			if (!globalSeen[pos]) {
				this.songPos[this.songs] = (byte) pos;
				this.parseMptSong(module, globalSeen, songLen, pos);
			}
		}
		if (this.songs == 0)
			throw new Exception("No songs found");
	}

	private void parseMptSong(byte[] module, boolean[] globalSeen, int songLen, int pos)
	{
		int addrToOffset = ASAPInfo.getWord(module, 2) - 6;
		int tempo = module[463] & 0xff;
		int playerCalls = 0;
		byte[] seen = new byte[256];
		int[] patternOffset = new int[4];
		int[] blankRows = new int[4];
		int[] blankRowsCounter = new int[4];
		while (pos < songLen) {
			if (seen[pos] != 0) {
				if ((seen[pos] & 0xff) != 1)
					this.loops[this.songs] = true;
				break;
			}
			seen[pos] = 1;
			globalSeen[pos] = true;
			int i = module[464 + pos * 2] & 0xff;
			if (i == 255) {
				pos = module[465 + pos * 2] & 0xff;
				continue;
			}
			int ch;
			for (ch = 3; ch >= 0; ch--) {
				i = (module[454 + ch] & 0xff) + ((module[458 + ch] & 0xff) << 8) - addrToOffset;
				i = module[i + pos * 2] & 0xff;
				if (i >= 64)
					break;
				i <<= 1;
				i = ASAPInfo.getWord(module, 70 + i);
				patternOffset[ch] = i == 0 ? 0 : i - addrToOffset;
				blankRowsCounter[ch] = 0;
			}
			if (ch >= 0)
				break;
			for (i = 0; i < songLen; i++)
				if ((seen[i] & 0xff) == 1)
					seen[i] = 2;
			for (int patternRows = module[462] & 0xff; --patternRows >= 0;) {
				for (ch = 3; ch >= 0; ch--) {
					if (patternOffset[ch] == 0 || --blankRowsCounter[ch] >= 0)
						continue;
					for (;;) {
						i = module[patternOffset[ch]++] & 0xff;
						if (i < 64 || i == 254)
							break;
						if (i < 128)
							continue;
						if (i < 192) {
							blankRows[ch] = i - 128;
							continue;
						}
						if (i < 208)
							continue;
						if (i < 224) {
							tempo = i - 207;
							continue;
						}
						patternRows = 0;
					}
					blankRowsCounter[ch] = blankRows[ch];
				}
				playerCalls += tempo;
			}
			pos++;
		}
		if (playerCalls > 0)
			this.addSong(playerCalls);
	}

	private void parseRmt(byte[] module, int moduleLen) throws Exception
	{
		if (moduleLen < 48)
			throw new Exception("Module too short");
		if (module[6] != 82 || module[7] != 77 || module[8] != 84 || module[13] != 1)
			throw new Exception("Invalid module header");
		int posShift;
		switch (module[9]) {
			case 52:
				posShift = 2;
				break;
			case 56:
				this.channels = 2;
				posShift = 3;
				break;
			default:
				throw new Exception("Unsupported number of channels");
		}
		int perFrame = module[12] & 0xff;
		if (perFrame < 1 || perFrame > 4)
			throw new Exception("Unsupported player call rate");
		this.type = ASAPModuleType.RMT;
		this.parseModule(module, moduleLen);
		int songLen = ASAPInfo.getWord(module, 4) + 1 - ASAPInfo.getWord(module, 20);
		if (posShift == 3 && (songLen & 4) != 0 && module[6 + ASAPInfo.getWord(module, 4) - ASAPInfo.getWord(module, 2) - 3] == -2)
			songLen += 4;
		songLen >>= posShift;
		if (songLen >= 256)
			throw new Exception("Song too long");
		boolean[] globalSeen = new boolean[256];
		this.songs = 0;
		for (int pos = 0; pos < songLen && this.songs < 32; pos++) {
			if (!globalSeen[pos]) {
				this.songPos[this.songs] = (byte) pos;
				this.parseRmtSong(module, globalSeen, songLen, posShift, pos);
			}
		}
		this.fastplay = 312 / perFrame;
		this.player = 1536;
		if (this.songs == 0)
			throw new Exception("No songs found");
	}

	private void parseRmtSong(byte[] module, boolean[] globalSeen, int songLen, int posShift, int pos)
	{
		int addrToOffset = ASAPInfo.getWord(module, 2) - 6;
		int tempo = module[11] & 0xff;
		int frames = 0;
		int songOffset = ASAPInfo.getWord(module, 20) - addrToOffset;
		int patternLoOffset = ASAPInfo.getWord(module, 16) - addrToOffset;
		int patternHiOffset = ASAPInfo.getWord(module, 18) - addrToOffset;
		byte[] seen = new byte[256];
		int[] patternBegin = new int[8];
		int[] patternOffset = new int[8];
		int[] blankRows = new int[8];
		int[] instrumentNo = new int[8];
		int[] instrumentFrame = new int[8];
		int[] volumeValue = new int[8];
		int[] volumeFrame = new int[8];
		while (pos < songLen) {
			if (seen[pos] != 0) {
				if ((seen[pos] & 0xff) != 1)
					this.loops[this.songs] = true;
				break;
			}
			seen[pos] = 1;
			globalSeen[pos] = true;
			if (module[songOffset + (pos << posShift)] == -2) {
				pos = module[songOffset + (pos << posShift) + 1] & 0xff;
				continue;
			}
			for (int ch = 0; ch < 1 << posShift; ch++) {
				int p = module[songOffset + (pos << posShift) + ch] & 0xff;
				if (p == 255)
					blankRows[ch] = 256;
				else {
					patternOffset[ch] = patternBegin[ch] = (module[patternLoOffset + p] & 0xff) + ((module[patternHiOffset + p] & 0xff) << 8) - addrToOffset;
					blankRows[ch] = 0;
				}
			}
			for (int i = 0; i < songLen; i++)
				if ((seen[i] & 0xff) == 1)
					seen[i] = 2;
			for (int patternRows = module[10] & 0xff; --patternRows >= 0;) {
				for (int ch = 0; ch < 1 << posShift; ch++) {
					if (--blankRows[ch] > 0)
						continue;
					for (;;) {
						int i = module[patternOffset[ch]++] & 0xff;
						if ((i & 63) < 62) {
							i += (module[patternOffset[ch]++] & 0xff) << 8;
							if ((i & 63) != 61) {
								instrumentNo[ch] = i >> 10;
								instrumentFrame[ch] = frames;
							}
							volumeValue[ch] = i >> 6 & 15;
							volumeFrame[ch] = frames;
							break;
						}
						if (i == 62) {
							blankRows[ch] = module[patternOffset[ch]++] & 0xff;
							break;
						}
						if ((i & 63) == 62) {
							blankRows[ch] = i >> 6;
							break;
						}
						if ((i & 191) == 63) {
							tempo = module[patternOffset[ch]++] & 0xff;
							continue;
						}
						if (i == 191) {
							patternOffset[ch] = patternBegin[ch] + (module[patternOffset[ch]] & 0xff);
							continue;
						}
						patternRows = -1;
						break;
					}
					if (patternRows < 0)
						break;
				}
				if (patternRows >= 0)
					frames += tempo;
			}
			pos++;
		}
		int instrumentFrames = 0;
		for (int ch = 0; ch < 1 << posShift; ch++) {
			int frame = instrumentFrame[ch];
			frame += ASAPInfo.getRmtInstrumentFrames(module, instrumentNo[ch], volumeValue[ch], volumeFrame[ch] - frame, ch >= 4);
			if (instrumentFrames < frame)
				instrumentFrames = frame;
		}
		if (frames > instrumentFrames) {
			if (frames - instrumentFrames > 100)
				this.loops[this.songs] = false;
			frames = instrumentFrames;
		}
		if (frames > 0)
			this.addSong(frames);
	}

	private void parseSap(byte[] module, int moduleLen) throws Exception
	{
		if (!ASAPInfo.hasStringAt(module, 0, "SAP\r\n"))
			throw new Exception("Missing SAP header");
		this.fastplay = -1;
		int type = 0;
		int moduleIndex = 5;
		int durationIndex = 0;
		while (module[moduleIndex] != -1) {
			if (moduleIndex + 8 >= moduleLen)
				throw new Exception("Missing binary part");
			if (ASAPInfo.hasStringAt(module, moduleIndex, "AUTHOR ")) {
				int len = ASAPInfo.parseText(module, moduleIndex + 7);
				if (len > 0)
					this.author = new String(module, moduleIndex + 7 + 1, len);
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "NAME ")) {
				int len = ASAPInfo.parseText(module, moduleIndex + 5);
				if (len > 0)
					this.name = new String(module, moduleIndex + 5 + 1, len);
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "DATE ")) {
				int len = ASAPInfo.parseText(module, moduleIndex + 5);
				if (len > 0)
					this.date = new String(module, moduleIndex + 5 + 1, len);
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "SONGS ")) {
				this.songs = ASAPInfo.parseDec(module, moduleIndex + 6, 32);
				if (this.songs < 1)
					throw new Exception("Number too small");
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "DEFSONG ")) {
				this.defaultSong = ASAPInfo.parseDec(module, moduleIndex + 8, 31);
				if (this.defaultSong < 0)
					throw new Exception("Number too small");
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "STEREO\r"))
				this.channels = 2;
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "NTSC\r"))
				this.ntsc = true;
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "TIME ")) {
				if (durationIndex >= 32)
					throw new Exception("Too many TIME tags");
				moduleIndex += 5;
				int len;
				for (len = 0; module[moduleIndex + len] != 13; len++) {
				}
				if (len > 5 && ASAPInfo.hasStringAt(module, moduleIndex + len - 5, " LOOP")) {
					this.loops[durationIndex] = true;
					len -= 5;
				}
				if (len > 9)
					throw new Exception("Invalid TIME tag");
				String s = new String(module, moduleIndex, len);
				int duration = ASAPInfo.parseDuration(s);
				this.durations[durationIndex++] = duration;
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "TYPE "))
				type = module[moduleIndex + 5] & 0xff;
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "FASTPLAY ")) {
				this.fastplay = ASAPInfo.parseDec(module, moduleIndex + 9, 312);
				if (this.fastplay < 1)
					throw new Exception("Number too small");
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "MUSIC ")) {
				this.music = ASAPInfo.parseHex(module, moduleIndex + 6);
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "INIT ")) {
				this.init = ASAPInfo.parseHex(module, moduleIndex + 5);
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "PLAYER ")) {
				this.player = ASAPInfo.parseHex(module, moduleIndex + 7);
			}
			else if (ASAPInfo.hasStringAt(module, moduleIndex, "COVOX ")) {
				this.covoxAddr = ASAPInfo.parseHex(module, moduleIndex + 6);
				if (this.covoxAddr != 54784)
					throw new Exception("COVOX should be D600");
				this.channels = 2;
			}
			while (module[moduleIndex++] != 13) {
				if (moduleIndex >= moduleLen)
					throw new Exception("Malformed SAP header");
			}
			if (module[moduleIndex++] != 10)
				throw new Exception("Malformed SAP header");
		}
		if (this.defaultSong >= this.songs)
			throw new Exception("DEFSONG too big");
		switch (type) {
			case 66:
				if (this.player < 0)
					throw new Exception("Missing PLAYER tag");
				if (this.init < 0)
					throw new Exception("Missing INIT tag");
				this.type = ASAPModuleType.SAP_B;
				break;
			case 67:
				if (this.player < 0)
					throw new Exception("Missing PLAYER tag");
				if (this.music < 0)
					throw new Exception("Missing MUSIC tag");
				this.type = ASAPModuleType.SAP_C;
				break;
			case 68:
				if (this.init < 0)
					throw new Exception("Missing INIT tag");
				this.type = ASAPModuleType.SAP_D;
				break;
			case 83:
				if (this.init < 0)
					throw new Exception("Missing INIT tag");
				this.type = ASAPModuleType.SAP_S;
				if (this.fastplay < 0)
					this.fastplay = 78;
				break;
			default:
				throw new Exception("Unsupported TYPE");
		}
		if (this.fastplay < 0)
			this.fastplay = this.ntsc ? 262 : 312;
		else if (this.ntsc && this.fastplay > 262)
			throw new Exception("FASTPLAY too big");
		if (module[moduleIndex + 1] != -1)
			throw new Exception("Invalid binary header");
		this.headerLen = moduleIndex;
	}

	private static int parseText(byte[] module, int moduleIndex) throws Exception
	{
		if (module[moduleIndex] != 34)
			throw new Exception("Missing quote");
		if (ASAPInfo.hasStringAt(module, moduleIndex + 1, "<?>\"\r"))
			return 0;
		for (int len = 0;; len++) {
			int c = module[moduleIndex + 1 + len] & 0xff;
			if (c == 34 && module[moduleIndex + 2 + len] == 13)
				return len;
			ASAPInfo.checkValidChar(c);
		}
	}

	private void parseTm2(byte[] module, int moduleLen) throws Exception
	{
		if (moduleLen < 932)
			throw new Exception("Module too short");
		this.type = ASAPModuleType.TM2;
		this.parseModule(module, moduleLen);
		int i = module[37] & 0xff;
		if (i < 1 || i > 4)
			throw new Exception("Unsupported player call rate");
		this.fastplay = 312 / i;
		this.player = 1280;
		if (module[31] != 0)
			this.channels = 2;
		int lastPos = 65535;
		for (i = 0; i < 128; i++) {
			int instrAddr = (module[134 + i] & 0xff) + ((module[774 + i] & 0xff) << 8);
			if (instrAddr != 0 && instrAddr < lastPos)
				lastPos = instrAddr;
		}
		for (i = 0; i < 256; i++) {
			int patternAddr = (module[262 + i] & 0xff) + ((module[518 + i] & 0xff) << 8);
			if (patternAddr != 0 && patternAddr < lastPos)
				lastPos = patternAddr;
		}
		lastPos -= ASAPInfo.getWord(module, 2) + 896;
		if (902 + lastPos >= moduleLen)
			throw new Exception("Module too short");
		int c;
		do {
			if (lastPos <= 0)
				throw new Exception("No songs found");
			lastPos -= 17;
			c = module[918 + lastPos] & 0xff;
		}
		while (c == 0 || c >= 128);
		this.songs = 0;
		this.parseTm2Song(module, 0);
		for (i = 0; i < lastPos && this.songs < 32; i += 17) {
			c = module[918 + i] & 0xff;
			if (c == 0 || c >= 128)
				this.parseTm2Song(module, i + 17);
		}
	}

	private void parseTm2Song(byte[] module, int pos)
	{
		int addrToOffset = ASAPInfo.getWord(module, 2) - 6;
		int tempo = (module[36] & 0xff) + 1;
		int playerCalls = 0;
		int[] patternOffset = new int[8];
		int[] blankRows = new int[8];
		for (;;) {
			int patternRows = module[918 + pos] & 0xff;
			if (patternRows == 0)
				break;
			if (patternRows >= 128) {
				this.loops[this.songs] = true;
				break;
			}
			for (int ch = 7; ch >= 0; ch--) {
				int pat = module[917 + pos - 2 * ch] & 0xff;
				patternOffset[ch] = (module[262 + pat] & 0xff) + ((module[518 + pat] & 0xff) << 8) - addrToOffset;
				blankRows[ch] = 0;
			}
			while (--patternRows >= 0) {
				for (int ch = 7; ch >= 0; ch--) {
					if (--blankRows[ch] >= 0)
						continue;
					for (;;) {
						int i = module[patternOffset[ch]++] & 0xff;
						if (i == 0) {
							patternOffset[ch]++;
							break;
						}
						if (i < 64) {
							if ((module[patternOffset[ch]++] & 0xff) >= 128)
								patternOffset[ch]++;
							break;
						}
						if (i < 128) {
							patternOffset[ch]++;
							break;
						}
						if (i == 128) {
							blankRows[ch] = module[patternOffset[ch]++] & 0xff;
							break;
						}
						if (i < 192)
							break;
						if (i < 208) {
							tempo = i - 191;
							continue;
						}
						if (i < 224) {
							patternOffset[ch]++;
							break;
						}
						if (i < 240) {
							patternOffset[ch] += 2;
							break;
						}
						if (i < 255) {
							blankRows[ch] = i - 240;
							break;
						}
						blankRows[ch] = 64;
						break;
					}
				}
				playerCalls += tempo;
			}
			pos += 17;
		}
		this.addSong(playerCalls);
	}

	private void parseTmc(byte[] module, int moduleLen) throws Exception
	{
		if (moduleLen < 464)
			throw new Exception("Module too short");
		this.type = ASAPModuleType.TMC;
		this.parseModule(module, moduleLen);
		this.channels = 2;
		int i = 0;
		while (module[102 + i] == 0) {
			if (++i >= 64)
				throw new Exception("No instruments");
		}
		int lastPos = ((module[102 + i] & 0xff) << 8) + (module[38 + i] & 0xff) - ASAPInfo.getWord(module, 2) - 432;
		if (437 + lastPos >= moduleLen)
			throw new Exception("Module too short");
		do {
			if (lastPos <= 0)
				throw new Exception("No songs found");
			lastPos -= 16;
		}
		while ((module[437 + lastPos] & 0xff) >= 128);
		this.songs = 0;
		this.parseTmcSong(module, 0);
		for (i = 0; i < lastPos && this.songs < 32; i += 16)
			if ((module[437 + i] & 0xff) >= 128)
				this.parseTmcSong(module, i + 16);
		i = module[37] & 0xff;
		if (i < 1 || i > 4)
			throw new Exception("Unsupported player call rate");
		this.fastplay = 312 / i;
	}

	private void parseTmcSong(byte[] module, int pos)
	{
		int addrToOffset = ASAPInfo.getWord(module, 2) - 6;
		int tempo = (module[36] & 0xff) + 1;
		int frames = 0;
		int[] patternOffset = new int[8];
		int[] blankRows = new int[8];
		while ((module[437 + pos] & 0xff) < 128) {
			for (int ch = 7; ch >= 0; ch--) {
				int pat = module[437 + pos - 2 * ch] & 0xff;
				patternOffset[ch] = (module[166 + pat] & 0xff) + ((module[294 + pat] & 0xff) << 8) - addrToOffset;
				blankRows[ch] = 0;
			}
			for (int patternRows = 64; --patternRows >= 0;) {
				for (int ch = 7; ch >= 0; ch--) {
					if (--blankRows[ch] >= 0)
						continue;
					for (;;) {
						int i = module[patternOffset[ch]++] & 0xff;
						if (i < 64) {
							patternOffset[ch]++;
							break;
						}
						if (i == 64) {
							i = module[patternOffset[ch]++] & 0xff;
							if ((i & 127) == 0)
								patternRows = 0;
							else
								tempo = (i & 127) + 1;
							if (i >= 128)
								patternOffset[ch]++;
							break;
						}
						if (i < 128) {
							i = module[patternOffset[ch]++] & 127;
							if (i == 0)
								patternRows = 0;
							else
								tempo = i + 1;
							patternOffset[ch]++;
							break;
						}
						if (i < 192)
							continue;
						blankRows[ch] = i - 191;
						break;
					}
				}
				frames += tempo;
			}
			pos += 16;
		}
		if ((module[436 + pos] & 0xff) < 128)
			this.loops[this.songs] = true;
		this.addSong(frames);
	}
	int player;

	/**
	 * Sets author's name.
	 * A nickname may be included in parentheses after the real name.
	 * Multiple authors are separated with <code>" &amp; "</code>.
	 * An empty string means the author is unknown.
	 */
	public void setAuthor(String value) throws Exception
	{
		ASAPInfo.checkValidText(value);
		this.author = value;
	}

	/**
	 * Sets music creation date.
	 * Some of the possible formats are:
	 * <ul>
	 * <li>YYYY</li>
	 * <li>MM/YYYY</li>
	 * <li>DD/MM/YYYY</li>
	 * <li>YYYY-YYYY</li>
	 * </ul>
	 * An empty string means the date is unknown.
	 */
	public void setDate(String value) throws Exception
	{
		ASAPInfo.checkValidText(value);
		this.date = value;
	}

	/**
	 * Sets length of the specified song.
	 * The length is specified in milliseconds. -1 means the length is indeterminate.
	 */
	public void setDuration(int song, int duration) throws Exception
	{
		if (song < 0 || song >= this.songs)
			throw new Exception("Song out of range");
		this.durations[song] = duration;
	}

	/**
	 * Sets information whether the specified song loops.
	 * Use:
	 * <ul>
	 * <li><code>true</code> if the song loops</li>
	 * <li><code>false</code> if the song stops</li>
	 * </ul>
	 * 
	 */
	public void setLoop(int song, boolean loop) throws Exception
	{
		if (song < 0 || song >= this.songs)
			throw new Exception("Song out of range");
		this.loops[song] = loop;
	}

	/**
	 * Sets music title.
	 * An empty string means the title is unknown.
	 */
	public void setTitle(String value) throws Exception
	{
		ASAPInfo.checkValidText(value);
		this.name = value;
	}
	final byte[] songPos = new byte[32];
	int songs;
	int type;
	/**
	 * ASAP version as a string.
	 */
	public static final String VERSION = "3.0.0";
	/**
	 * ASAP version - major part.
	 */
	public static final int VERSION_MAJOR = 3;
	/**
	 * ASAP version - micro part.
	 */
	public static final int VERSION_MICRO = 0;
	/**
	 * ASAP version - minor part.
	 */
	public static final int VERSION_MINOR = 0;
	/**
	 * Years ASAP was created in.
	 */
	public static final String YEARS = "2005-2011";
	private static final byte[] CI_CONST_ARRAY_1 = { 16, 8, 4, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 };
}
