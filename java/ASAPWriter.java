// Generated automatically with "cito". Do not edit.
package net.sf.asap;

public final class ASAPWriter
{

	public static void enumSaveExts(StringConsumer output, ASAPInfo info, byte[] module, int moduleLen)
	{
		switch (info.type) {
			case ASAPModuleType.SAP_B:
			case ASAPModuleType.SAP_C:
			case ASAPModuleType.SAP_D:
			case ASAPModuleType.SAP_S:
				output.run("sap");
				String ext = info.getOriginalModuleExt(module, moduleLen);
				if (ext != null)
					output.run(ext);
				break;
			default:
				output.run(info.getOriginalModuleExt(module, moduleLen));
				output.run("sap");
				break;
		}
	}

	public static void write(String filename, ByteWriter w, ASAPInfo info, byte[] module, int moduleLen) throws Exception
	{
		int destExt = ASAPInfo.getPackedExt(filename);
		if (destExt == 7364979) {
			ASAPWriter.writeExecutable(w, null, info, module, moduleLen);
			return;
		}
		String possibleExt = info.getOriginalModuleExt(module, moduleLen);
		if (possibleExt != null && destExt == (possibleExt.charAt(0) + (possibleExt.charAt(1) << 8) + (possibleExt.charAt(2) << 16) | 2105376)) {
			switch (info.type) {
				case ASAPModuleType.SAP_B:
				case ASAPModuleType.SAP_C:
					int blockLen = ASAPInfo.getWord(module, info.headerLen + 4) - ASAPInfo.getWord(module, info.headerLen + 2) + 7;
					if (blockLen < 7 || info.headerLen + blockLen >= moduleLen)
						throw new Exception("Cannot extract module from SAP");
					ASAPWriter.writeBytes(w, module, info.headerLen, info.headerLen + blockLen);
					break;
				default:
					ASAPWriter.writeBytes(w, module, 0, moduleLen);
					break;
			}
			return;
		}
		throw new Exception("Impossible conversion");
	}

	private static void writeBytes(ByteWriter w, byte[] array, int startIndex, int endIndex)
	{
		while (startIndex < endIndex)
			w.run(array[startIndex++] & 0xff);
	}

	private static void writeCmcInit(ByteWriter w, int[] initAndPlayer, ASAPInfo info)
	{
		if (initAndPlayer == null)
			return;
		ASAPWriter.writeWord(w, 4064);
		ASAPWriter.writeWord(w, 4080);
		w.run(72);
		w.run(162);
		w.run(info.music & 255);
		w.run(160);
		w.run(info.music >> 8);
		w.run(169);
		w.run(112);
		w.run(32);
		ASAPWriter.writeWord(w, initAndPlayer[1] + 3);
		ASAPWriter.writePlaTaxLda0(w);
		w.run(76);
		ASAPWriter.writeWord(w, initAndPlayer[1] + 3);
		initAndPlayer[0] = 4064;
		initAndPlayer[1] += 6;
	}

	private static void writeDec(ByteWriter w, int value)
	{
		if (value >= 10) {
			ASAPWriter.writeDec(w, value / 10);
			value %= 10;
		}
		w.run(48 + value);
	}

	private static void writeDecSapTag(ByteWriter w, String tag, int value)
	{
		ASAPWriter.writeString(w, tag);
		ASAPWriter.writeDec(w, value);
		w.run(13);
		w.run(10);
	}

	public static void writeDuration(ByteWriter w, int value)
	{
		if (value < 0 || value >= 6000000)
			return;
		int seconds = value / 1000;
		value %= 1000;
		ASAPWriter.writeTwoDigits(w, seconds / 60);
		w.run(58);
		ASAPWriter.writeTwoDigits(w, seconds % 60);
		if (value != 0) {
			w.run(46);
			ASAPWriter.writeTwoDigits(w, value / 10);
			value %= 10;
			if (value != 0)
				w.run(48 + value);
		}
	}

	static void writeExecutable(ByteWriter w, int[] initAndPlayer, ASAPInfo info, byte[] module, int moduleLen) throws Exception
	{
		byte[] playerRoutine = ASAP6502.getPlayerRoutine(info);
		int player = -1;
		int playerLastByte = -1;
		if (playerRoutine != null) {
			player = ASAPInfo.getWord(playerRoutine, 2);
			playerLastByte = ASAPInfo.getWord(playerRoutine, 4);
			if (info.music <= playerLastByte)
				throw new Exception("Module address conflicts with the player routine");
		}
		int startAddr;
		switch (info.type) {
			case ASAPModuleType.SAP_B:
				ASAPWriter.writeExecutableFromSap(w, initAndPlayer, info, 66, module, moduleLen);
				break;
			case ASAPModuleType.SAP_C:
				ASAPWriter.writeExecutableFromSap(w, initAndPlayer, info, 67, module, moduleLen);
				ASAPWriter.writeCmcInit(w, initAndPlayer, info);
				break;
			case ASAPModuleType.SAP_D:
				ASAPWriter.writeExecutableFromSap(w, initAndPlayer, info, 68, module, moduleLen);
				break;
			case ASAPModuleType.SAP_S:
				ASAPWriter.writeExecutableFromSap(w, initAndPlayer, info, 83, module, moduleLen);
				break;
			case ASAPModuleType.CMC:
			case ASAPModuleType.CM3:
			case ASAPModuleType.CMR:
			case ASAPModuleType.CMS:
				ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, 67, -1, player);
				w.run(255);
				w.run(255);
				ASAPWriter.writeBytes(w, module, 2, moduleLen);
				ASAPWriter.writeBytes(w, playerRoutine, 2, playerLastByte - player + 7);
				ASAPWriter.writeCmcInit(w, initAndPlayer, info);
				break;
			case ASAPModuleType.DLT:
				startAddr = ASAPWriter.writeExecutableHeaderForSongPos(w, initAndPlayer, info, player, 5, 7, 259);
				if (moduleLen == 11270) {
					ASAPWriter.writeBytes(w, module, 0, 4);
					ASAPWriter.writeWord(w, 19456);
					ASAPWriter.writeBytes(w, module, 6, moduleLen);
					w.run(0);
				}
				else
					ASAPWriter.writeBytes(w, module, 0, moduleLen);
				ASAPWriter.writeWord(w, startAddr);
				ASAPWriter.writeWord(w, playerLastByte);
				if (info.songs != 1) {
					ASAPWriter.writeBytes(w, info.songPos, 0, info.songs);
					w.run(170);
					w.run(188);
					ASAPWriter.writeWord(w, startAddr);
				}
				else {
					w.run(160);
					w.run(0);
				}
				w.run(76);
				ASAPWriter.writeWord(w, player + 256);
				ASAPWriter.writeBytes(w, playerRoutine, 6, playerLastByte - player + 7);
				break;
			case ASAPModuleType.MPT:
				startAddr = ASAPWriter.writeExecutableHeaderForSongPos(w, initAndPlayer, info, player, 13, 17, 3);
				ASAPWriter.writeBytes(w, module, 0, moduleLen);
				ASAPWriter.writeWord(w, startAddr);
				ASAPWriter.writeWord(w, playerLastByte);
				if (info.songs != 1) {
					ASAPWriter.writeBytes(w, info.songPos, 0, info.songs);
					w.run(72);
				}
				w.run(160);
				w.run(info.music & 255);
				w.run(162);
				w.run(info.music >> 8);
				w.run(169);
				w.run(0);
				w.run(32);
				ASAPWriter.writeWord(w, player);
				if (info.songs != 1) {
					w.run(104);
					w.run(168);
					w.run(190);
					ASAPWriter.writeWord(w, startAddr);
				}
				else {
					w.run(162);
					w.run(0);
				}
				w.run(169);
				w.run(2);
				ASAPWriter.writeBytes(w, playerRoutine, 6, playerLastByte - player + 7);
				break;
			case ASAPModuleType.RMT:
				ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, 66, 3200, 1539);
				ASAPWriter.writeBytes(w, module, 0, ASAPInfo.getWord(module, 4) - info.music + 7);
				ASAPWriter.writeWord(w, 3200);
				if (info.songs != 1) {
					ASAPWriter.writeWord(w, 3210 + info.songs);
					w.run(168);
					w.run(185);
					ASAPWriter.writeWord(w, 3211);
				}
				else {
					ASAPWriter.writeWord(w, 3208);
					w.run(169);
					w.run(0);
				}
				w.run(162);
				w.run(info.music & 255);
				w.run(160);
				w.run(info.music >> 8);
				w.run(76);
				ASAPWriter.writeWord(w, 1536);
				if (info.songs != 1)
					ASAPWriter.writeBytes(w, info.songPos, 0, info.songs);
				ASAPWriter.writeBytes(w, playerRoutine, 2, playerLastByte - player + 7);
				break;
			case ASAPModuleType.TMC:
				int player2 = player + CI_CONST_ARRAY_1[(module[37] & 0xff) - 1];
				startAddr = player2 + CI_CONST_ARRAY_2[(module[37] & 0xff) - 1];
				if (info.songs != 1)
					startAddr -= 3;
				ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, 66, startAddr, player2);
				ASAPWriter.writeBytes(w, module, 0, moduleLen);
				ASAPWriter.writeWord(w, startAddr);
				ASAPWriter.writeWord(w, playerLastByte);
				if (info.songs != 1)
					w.run(72);
				w.run(160);
				w.run(info.music & 255);
				w.run(162);
				w.run(info.music >> 8);
				w.run(169);
				w.run(112);
				w.run(32);
				ASAPWriter.writeWord(w, player);
				if (info.songs != 1)
					ASAPWriter.writePlaTaxLda0(w);
				else {
					w.run(169);
					w.run(96);
				}
				switch (module[37]) {
					case 2:
						w.run(6);
						w.run(0);
						w.run(76);
						ASAPWriter.writeWord(w, player);
						w.run(165);
						w.run(0);
						w.run(230);
						w.run(0);
						w.run(74);
						w.run(144);
						w.run(5);
						w.run(176);
						w.run(6);
						break;
					case 3:
					case 4:
						w.run(160);
						w.run(1);
						w.run(132);
						w.run(0);
						w.run(208);
						w.run(10);
						w.run(198);
						w.run(0);
						w.run(208);
						w.run(12);
						w.run(160);
						w.run(module[37] & 0xff);
						w.run(132);
						w.run(0);
						w.run(208);
						w.run(3);
						break;
				}
				ASAPWriter.writeBytes(w, playerRoutine, 6, playerLastByte - player + 7);
				break;
			case ASAPModuleType.TM2:
				ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, 66, 4224, 1283);
				ASAPWriter.writeBytes(w, module, 0, moduleLen);
				ASAPWriter.writeWord(w, 4224);
				if (info.songs != 1) {
					ASAPWriter.writeWord(w, 4240);
					w.run(72);
				}
				else
					ASAPWriter.writeWord(w, 4238);
				w.run(160);
				w.run(info.music & 255);
				w.run(162);
				w.run(info.music >> 8);
				w.run(169);
				w.run(112);
				w.run(32);
				ASAPWriter.writeWord(w, 1280);
				if (info.songs != 1)
					ASAPWriter.writePlaTaxLda0(w);
				else {
					w.run(169);
					w.run(0);
					w.run(170);
				}
				w.run(76);
				ASAPWriter.writeWord(w, 1280);
				ASAPWriter.writeBytes(w, playerRoutine, 2, playerLastByte - player + 7);
				break;
		}
	}

	private static void writeExecutableFromSap(ByteWriter w, int[] initAndPlayer, ASAPInfo info, int type, byte[] module, int moduleLen)
	{
		ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, type, info.init, info.player);
		ASAPWriter.writeBytes(w, module, info.headerLen, moduleLen);
	}

	private static void writeExecutableHeader(ByteWriter w, int[] initAndPlayer, ASAPInfo info, int type, int init, int player)
	{
		if (initAndPlayer == null)
			ASAPWriter.writeSapHeader(w, info, type, init, player);
		else {
			initAndPlayer[0] = init;
			initAndPlayer[1] = player;
		}
	}

	private static int writeExecutableHeaderForSongPos(ByteWriter w, int[] initAndPlayer, ASAPInfo info, int player, int codeForOneSong, int codeForManySongs, int playerOffset)
	{
		if (info.songs != 1) {
			ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, 66, player - codeForManySongs, player + playerOffset);
			return player - codeForManySongs - info.songs;
		}
		ASAPWriter.writeExecutableHeader(w, initAndPlayer, info, 66, player - codeForOneSong, player + playerOffset);
		return player - codeForOneSong;
	}

	private static void writeHexSapTag(ByteWriter w, String tag, int value)
	{
		if (value < 0)
			return;
		ASAPWriter.writeString(w, tag);
		for (int i = 12; i >= 0; i -= 4) {
			int digit = value >> i & 15;
			w.run(digit + (digit < 10 ? 48 : 55));
		}
		w.run(13);
		w.run(10);
	}

	private static void writePlaTaxLda0(ByteWriter w)
	{
		w.run(104);
		w.run(170);
		w.run(169);
		w.run(0);
	}

	private static void writeSapHeader(ByteWriter w, ASAPInfo info, int type, int init, int player)
	{
		ASAPWriter.writeString(w, "SAP\r\n");
		ASAPWriter.writeTextSapTag(w, "AUTHOR ", info.author);
		ASAPWriter.writeTextSapTag(w, "NAME ", info.name);
		ASAPWriter.writeTextSapTag(w, "DATE ", info.date);
		if (info.songs > 1) {
			ASAPWriter.writeDecSapTag(w, "SONGS ", info.songs);
			if (info.defaultSong > 0)
				ASAPWriter.writeDecSapTag(w, "DEFSONG ", info.defaultSong);
		}
		if (info.channels > 1)
			ASAPWriter.writeString(w, "STEREO\r\n");
		if (info.ntsc)
			ASAPWriter.writeString(w, "NTSC\r\n");
		ASAPWriter.writeString(w, "TYPE ");
		w.run(type);
		w.run(13);
		w.run(10);
		if (info.fastplay != 312)
			ASAPWriter.writeDecSapTag(w, "FASTPLAY ", info.fastplay);
		if (type == 67)
			ASAPWriter.writeHexSapTag(w, "MUSIC ", info.music);
		ASAPWriter.writeHexSapTag(w, "INIT ", init);
		ASAPWriter.writeHexSapTag(w, "PLAYER ", player);
		ASAPWriter.writeHexSapTag(w, "COVOX ", info.covoxAddr);
		for (int song = 0; song < info.songs; song++) {
			if (info.durations[song] < 0)
				break;
			ASAPWriter.writeString(w, "TIME ");
			ASAPWriter.writeDuration(w, info.durations[song]);
			if (info.loops[song])
				ASAPWriter.writeString(w, " LOOP");
			w.run(13);
			w.run(10);
		}
	}

	private static void writeString(ByteWriter w, String s)
	{
		int n = s.length();
		for (int i = 0; i < n; i++)
			w.run(s.charAt(i));
	}

	private static void writeTextSapTag(ByteWriter w, String tag, String value)
	{
		ASAPWriter.writeString(w, tag);
		w.run(34);
		if (value.length() == 0)
			value = "<?>";
		ASAPWriter.writeString(w, value);
		w.run(34);
		w.run(13);
		w.run(10);
	}

	private static void writeTwoDigits(ByteWriter w, int value)
	{
		w.run(48 + value / 10);
		w.run(48 + value % 10);
	}

	private static void writeWord(ByteWriter w, int value)
	{
		w.run(value & 255);
		w.run(value >> 8);
	}
	private static final int[] CI_CONST_ARRAY_1 = { 3, -9, -10, -10 };
	private static final int[] CI_CONST_ARRAY_2 = { -14, -16, -17, -17 };
}
