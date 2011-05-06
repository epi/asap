// Generated automatically with "cito". Do not edit.
package net.sf.asap;

/**
 * 8-bit Atari chip music emulator.
 * This class performs no I/O operations - all music data must be passed in byte arrays.
 */
public final class ASAP
{
	private int blocksPlayed;

	private void call6502(int addr)
	{
		this.memory[53760] = 32;
		this.memory[53761] = (byte) addr;
		this.memory[53762] = (byte) (addr >> 8);
		this.memory[53763] = -46;
		this.cpu.pc = 53760;
	}

	private void call6502Player()
	{
		int player = this.moduleInfo.player;
		switch (this.moduleInfo.type) {
			case ASAPModuleType.SAP_B:
				this.call6502(player);
				break;
			case ASAPModuleType.SAP_C:
			case ASAPModuleType.CMC:
			case ASAPModuleType.CM3:
			case ASAPModuleType.CMR:
			case ASAPModuleType.CMS:
				this.call6502(player + 6);
				break;
			case ASAPModuleType.SAP_D:
				if (player >= 0) {
					this.memory[256 + this.cpu.s] = (byte) (this.cpu.pc >> 8);
					this.cpu.s = this.cpu.s - 1 & 255;
					this.memory[256 + this.cpu.s] = (byte) this.cpu.pc;
					this.cpu.s = this.cpu.s - 1 & 255;
					this.memory[53760] = 8;
					this.memory[53761] = 72;
					this.memory[53762] = -118;
					this.memory[53763] = 72;
					this.memory[53764] = -104;
					this.memory[53765] = 72;
					this.memory[53766] = 32;
					this.memory[53767] = (byte) player;
					this.memory[53768] = (byte) (player >> 8);
					this.memory[53769] = 104;
					this.memory[53770] = -88;
					this.memory[53771] = 104;
					this.memory[53772] = -86;
					this.memory[53773] = 104;
					this.memory[53774] = 64;
					this.cpu.pc = 53760;
				}
				break;
			case ASAPModuleType.SAP_S:
				int i = (this.memory[69] & 0xff) - 1;
				this.memory[69] = (byte) i;
				if (i == 0)
					this.memory[45179] = (byte) ((this.memory[45179] & 0xff) + 1);
				break;
			case ASAPModuleType.DLT:
				this.call6502(player + 259);
				break;
			case ASAPModuleType.MPT:
			case ASAPModuleType.RMT:
			case ASAPModuleType.TM2:
				this.call6502(player + 3);
				break;
			case ASAPModuleType.TMC:
				if (--this.tmcPerFrameCounter <= 0) {
					this.tmcPerFrameCounter = this.memory[this.moduleInfo.music + 31] & 0xff;
					this.call6502(player + 3);
				}
				else
					this.call6502(player + 6);
				break;
		}
	}
	private int consol;
	private final byte[] covox = new byte[4];
	private final Cpu6502 cpu = new Cpu6502();
	private int currentDuration;
	private int currentSong;
	int cycle;

	/**
	 * Enables silence detection.
	 * Causes playback to stop after the specified period of silence.
	 * Must be called after each call of <code>Load</code>.
	 * @param seconds Length of silence which ends playback.
	 */
	public void detectSilence(int seconds)
	{
		this.silenceCycles = seconds * this.pokeys.mainClock;
	}

	private int do6502Frame()
	{
		this.nextEventCycle = 0;
		this.nextScanlineCycle = 0;
		this.nmist = this.nmist == NmiStatus.RESET ? NmiStatus.ON_V_BLANK : NmiStatus.WAS_V_BLANK;
		int cycles = this.moduleInfo.ntsc ? 29868 : 35568;
		this.cpu.doFrame(this, cycles);
		this.cycle -= cycles;
		if (this.nextPlayerCycle != 8388608)
			this.nextPlayerCycle -= cycles;
		if (this.pokeys.timer1Cycle != 8388608)
			this.pokeys.timer1Cycle -= cycles;
		if (this.pokeys.timer2Cycle != 8388608)
			this.pokeys.timer2Cycle -= cycles;
		if (this.pokeys.timer4Cycle != 8388608)
			this.pokeys.timer4Cycle -= cycles;
		return cycles;
	}

	private void do6502Init(int pc, int a, int x, int y) throws Exception
	{
		this.cpu.pc = pc;
		this.cpu.a = a & 255;
		this.cpu.x = x & 255;
		this.cpu.y = y & 255;
		this.memory[53760] = -46;
		this.memory[510] = -1;
		this.memory[511] = -47;
		this.cpu.s = 253;
		for (int frame = 0; frame < 50; frame++) {
			this.do6502Frame();
			if (this.cpu.pc == 53760)
				return;
		}
		throw new Exception("INIT routine didn't return");
	}

	private int doFrame()
	{
		this.pokeys.startFrame();
		int cycles = this.do6502Frame();
		this.pokeys.endFrame(cycles);
		return cycles;
	}

	/**
	 * Fills the specified buffer with generated samples.
	 * @param buffer The destination buffer.
	 * @param bufferLen Number of bytes to fill.
	 * @param format Format of samples.
	 */
	public int generate(byte[] buffer, int bufferLen, int format)
	{
		return this.generateAt(buffer, 0, bufferLen, format);
	}

	private int generateAt(byte[] buffer, int bufferOffset, int bufferLen, int format)
	{
		if (this.silenceCycles > 0 && this.silenceCyclesCounter <= 0)
			return 0;
		int blockShift = this.moduleInfo.channels - 1 + (format != ASAPSampleFormat.U8 ? 1 : 0);
		int bufferBlocks = bufferLen >> blockShift;
		if (this.currentDuration > 0) {
			int totalBlocks = ASAP.millisecondsToBlocks(this.currentDuration);
			if (bufferBlocks > totalBlocks - this.blocksPlayed)
				bufferBlocks = totalBlocks - this.blocksPlayed;
		}
		int block = 0;
		for (;;) {
			int blocks = this.pokeys.generate(buffer, bufferOffset + (block << blockShift), bufferBlocks - block, format);
			this.blocksPlayed += blocks;
			block += blocks;
			if (block >= bufferBlocks)
				break;
			int cycles = this.doFrame();
			if (this.silenceCycles > 0) {
				if (this.pokeys.isSilent()) {
					this.silenceCyclesCounter -= cycles;
					if (this.silenceCyclesCounter <= 0)
						break;
				}
				else
					this.silenceCyclesCounter = this.silenceCycles;
			}
		}
		return block << blockShift;
	}

	/**
	 * Returns current playback position in blocks.
	 * A block is one sample or a pair of samples for stereo.
	 */
	public int getBlocksPlayed()
	{
		return this.blocksPlayed;
	}

	/**
	 * Returns information about the loaded module.
	 */
	public ASAPInfo getInfo()
	{
		return this.moduleInfo;
	}

	/**
	 * Returns POKEY channel volume - an integer between 0 and 15.
	 * @param channel POKEY channel number (from 0 to 7).
	 */
	public int getPokeyChannelVolume(int channel)
	{
		switch (channel) {
			case 0:
				return this.pokeys.basePokey.audc1 & 15;
			case 1:
				return this.pokeys.basePokey.audc2 & 15;
			case 2:
				return this.pokeys.basePokey.audc3 & 15;
			case 3:
				return this.pokeys.basePokey.audc4 & 15;
			case 4:
				return this.pokeys.extraPokey.audc1 & 15;
			case 5:
				return this.pokeys.extraPokey.audc2 & 15;
			case 6:
				return this.pokeys.extraPokey.audc3 & 15;
			case 7:
				return this.pokeys.extraPokey.audc4 & 15;
			default:
				return 0;
		}
	}

	/**
	 * Returns current playback position in milliseconds.
	 */
	public int getPosition()
	{
		return this.blocksPlayed * 10 / 441;
	}

	/**
	 * Fills leading bytes of the specified buffer with WAV file header.
	 * The number of changed bytes is <code>WavHeaderLength</code>.
	 * @param buffer The destination buffer.
	 * @param format Format of samples.
	 */
	public void getWavHeader(byte[] buffer, int format)
	{
		int use16bit = format != ASAPSampleFormat.U8 ? 1 : 0;
		int blockSize = this.moduleInfo.channels << use16bit;
		int bytesPerSecond = 44100 * blockSize;
		int totalBlocks = ASAP.millisecondsToBlocks(this.currentDuration);
		int nBytes = (totalBlocks - this.blocksPlayed) * blockSize;
		buffer[0] = 82;
		buffer[1] = 73;
		buffer[2] = 70;
		buffer[3] = 70;
		ASAP.putLittleEndian(buffer, 4, nBytes + 36);
		buffer[8] = 87;
		buffer[9] = 65;
		buffer[10] = 86;
		buffer[11] = 69;
		buffer[12] = 102;
		buffer[13] = 109;
		buffer[14] = 116;
		buffer[15] = 32;
		buffer[16] = 16;
		buffer[17] = 0;
		buffer[18] = 0;
		buffer[19] = 0;
		buffer[20] = 1;
		buffer[21] = 0;
		buffer[22] = (byte) this.moduleInfo.channels;
		buffer[23] = 0;
		ASAP.putLittleEndian(buffer, 24, 44100);
		ASAP.putLittleEndian(buffer, 28, bytesPerSecond);
		buffer[32] = (byte) blockSize;
		buffer[33] = 0;
		buffer[34] = (byte) (8 << use16bit);
		buffer[35] = 0;
		buffer[36] = 100;
		buffer[37] = 97;
		buffer[38] = 116;
		buffer[39] = 97;
		ASAP.putLittleEndian(buffer, 40, nBytes);
	}

	void handleEvent()
	{
		int cycle = this.cycle;
		if (cycle >= this.nextScanlineCycle) {
			if (cycle - this.nextScanlineCycle < 50)
				this.cycle = cycle += 9;
			this.nextScanlineCycle += 114;
			if (cycle >= this.nextPlayerCycle) {
				this.call6502Player();
				this.nextPlayerCycle += 114 * this.moduleInfo.fastplay;
			}
		}
		int nextEventCycle = this.nextScanlineCycle;
		if (cycle >= this.pokeys.timer1Cycle) {
			this.pokeys.irqst &= ~1;
			this.pokeys.timer1Cycle = 8388608;
		}
		else if (nextEventCycle > this.pokeys.timer1Cycle)
			nextEventCycle = this.pokeys.timer1Cycle;
		if (cycle >= this.pokeys.timer2Cycle) {
			this.pokeys.irqst &= ~2;
			this.pokeys.timer2Cycle = 8388608;
		}
		else if (nextEventCycle > this.pokeys.timer2Cycle)
			nextEventCycle = this.pokeys.timer2Cycle;
		if (cycle >= this.pokeys.timer4Cycle) {
			this.pokeys.irqst &= ~4;
			this.pokeys.timer4Cycle = 8388608;
		}
		else if (nextEventCycle > this.pokeys.timer4Cycle)
			nextEventCycle = this.pokeys.timer4Cycle;
		this.nextEventCycle = nextEventCycle;
	}

	/**
	 * Loads music data ("module").
	 * @param filename Filename, used to determine the format.
	 * @param module Contents of the file.
	 * @param moduleLen Length of the file.
	 */
	public void load(String filename, byte[] module, int moduleLen) throws Exception
	{
		this.silenceCycles = 0;
		this.moduleInfo.load(filename, module, moduleLen);
		byte[] playerRoutine = ASAP6502.getPlayerRoutine(this.moduleInfo);
		if (playerRoutine != null) {
			int player = ASAPInfo.getWord(playerRoutine, 2);
			int playerLastByte = ASAPInfo.getWord(playerRoutine, 4);
			if (this.moduleInfo.music <= playerLastByte)
				throw new Exception("Module address conflicts with the player routine");
			this.memory[19456] = 0;
			System.arraycopy(module, 6, this.memory, this.moduleInfo.music, moduleLen - 6);
			System.arraycopy(playerRoutine, 6, this.memory, player, playerLastByte + 1 - player);
			if (this.moduleInfo.player < 0)
				this.moduleInfo.player = player;
			return;
		}
		clear(this.memory);
		int moduleIndex = this.moduleInfo.headerLen + 2;
		while (moduleIndex + 5 <= moduleLen) {
			int startAddr = ASAPInfo.getWord(module, moduleIndex);
			int blockLen = ASAPInfo.getWord(module, moduleIndex + 2) + 1 - startAddr;
			if (blockLen <= 0 || moduleIndex + blockLen > moduleLen)
				throw new Exception("Invalid binary block");
			moduleIndex += 4;
			System.arraycopy(module, moduleIndex, this.memory, startAddr, blockLen);
			moduleIndex += blockLen;
			if (moduleIndex == moduleLen)
				return;
			if (moduleIndex + 7 <= moduleLen && module[moduleIndex] == -1 && module[moduleIndex + 1] == -1)
				moduleIndex += 2;
		}
		throw new Exception("Invalid binary block");
	}
	final byte[] memory = new byte[65536];

	private static int millisecondsToBlocks(int milliseconds)
	{
		return milliseconds * 441 / 10;
	}
	private final ASAPInfo moduleInfo = new ASAPInfo();

	/**
	 * Mutes the selected POKEY channels.
	 * @param mask An 8-bit mask which selects POKEY channels to be muted.
	 */
	public void mutePokeyChannels(int mask)
	{
		this.pokeys.basePokey.mute(mask);
		this.pokeys.extraPokey.mute(mask >> 4);
	}
	int nextEventCycle;
	private int nextPlayerCycle;
	private int nextScanlineCycle;
	private int nmist;

	int peekHardware(int addr)
	{
		switch (addr & 65311) {
			case 53268:
				return this.moduleInfo.ntsc ? 15 : 1;
			case 53770:
			case 53786:
				return this.pokeys.getRandom(addr, this.cycle);
			case 53774:
				return this.pokeys.irqst;
			case 53790:
				if (this.pokeys.extraPokeyMask != 0) {
					return 255;
				}
				return this.pokeys.irqst;
			case 53772:
			case 53788:
			case 53775:
			case 53791:
				return 255;
			case 54283:
			case 54299:
				if (this.cycle > (this.moduleInfo.ntsc ? 29868 : 35568))
					return 0;
				return this.cycle / 228;
			case 54287:
				switch (this.nmist) {
					case NmiStatus.RESET:
						return 31;
					case NmiStatus.WAS_V_BLANK:
						return 95;
					case NmiStatus.ON_V_BLANK:
					default:
						return this.cycle < 28291 ? 31 : 95;
				}
			default:
				return this.memory[addr] & 0xff;
		}
	}

	/**
	 * Prepares playback of the specified song of the loaded module.
	 * @param song Zero-based song index.
	 * @param duration Playback time in milliseconds, -1 means infinity.
	 */
	public void playSong(int song, int duration) throws Exception
	{
		if (song < 0 || song >= this.moduleInfo.songs)
			throw new Exception("Song number out of range");
		this.currentSong = song;
		this.currentDuration = duration;
		this.nextPlayerCycle = 8388608;
		this.blocksPlayed = 0;
		this.silenceCyclesCounter = this.silenceCycles;
		this.cycle = 0;
		this.cpu.nz = 0;
		this.cpu.c = 0;
		this.cpu.vdi = 0;
		this.nmist = NmiStatus.ON_V_BLANK;
		this.consol = 8;
		this.covox[0] = -128;
		this.covox[1] = -128;
		this.covox[2] = -128;
		this.covox[3] = -128;
		this.pokeys.initialize(this.moduleInfo.ntsc ? 1789772 : 1773447, this.moduleInfo.channels > 1);
		this.mutePokeyChannels(255);
		switch (this.moduleInfo.type) {
			case ASAPModuleType.SAP_B:
				this.do6502Init(this.moduleInfo.init, song, 0, 0);
				break;
			case ASAPModuleType.SAP_C:
			case ASAPModuleType.CMC:
			case ASAPModuleType.CM3:
			case ASAPModuleType.CMR:
			case ASAPModuleType.CMS:
				this.do6502Init(this.moduleInfo.player + 3, 112, this.moduleInfo.music, this.moduleInfo.music >> 8);
				this.do6502Init(this.moduleInfo.player + 3, 0, song, 0);
				break;
			case ASAPModuleType.SAP_D:
			case ASAPModuleType.SAP_S:
				this.cpu.pc = this.moduleInfo.init;
				this.cpu.a = song;
				this.cpu.x = 0;
				this.cpu.y = 0;
				this.cpu.s = 255;
				break;
			case ASAPModuleType.DLT:
				this.do6502Init(this.moduleInfo.player + 256, 0, 0, this.moduleInfo.songPos[song] & 0xff);
				break;
			case ASAPModuleType.MPT:
				this.do6502Init(this.moduleInfo.player, 0, this.moduleInfo.music >> 8, this.moduleInfo.music);
				this.do6502Init(this.moduleInfo.player, 2, this.moduleInfo.songPos[song] & 0xff, 0);
				break;
			case ASAPModuleType.RMT:
				this.do6502Init(this.moduleInfo.player, this.moduleInfo.songPos[song] & 0xff, this.moduleInfo.music, this.moduleInfo.music >> 8);
				break;
			case ASAPModuleType.TMC:
			case ASAPModuleType.TM2:
				this.do6502Init(this.moduleInfo.player, 112, this.moduleInfo.music >> 8, this.moduleInfo.music);
				this.do6502Init(this.moduleInfo.player, 0, song, 0);
				this.tmcPerFrameCounter = 1;
				break;
		}
		this.mutePokeyChannels(0);
		this.nextPlayerCycle = 0;
	}

	void pokeHardware(int addr, int data)
	{
		if (addr >> 8 == 210) {
			if ((addr & this.pokeys.extraPokeyMask + 15) == 14) {
				this.pokeys.irqst |= data ^ 255;
				if ((data & this.pokeys.irqst & 1) != 0) {
					if (this.pokeys.timer1Cycle == 8388608) {
						int t = this.pokeys.basePokey.tickCycle1;
						while (t < this.cycle)
							t += this.pokeys.basePokey.periodCycles1;
						this.pokeys.timer1Cycle = t;
						if (this.nextEventCycle > t)
							this.nextEventCycle = t;
					}
				}
				else
					this.pokeys.timer1Cycle = 8388608;
				if ((data & this.pokeys.irqst & 2) != 0) {
					if (this.pokeys.timer2Cycle == 8388608) {
						int t = this.pokeys.basePokey.tickCycle2;
						while (t < this.cycle)
							t += this.pokeys.basePokey.periodCycles2;
						this.pokeys.timer2Cycle = t;
						if (this.nextEventCycle > t)
							this.nextEventCycle = t;
					}
				}
				else
					this.pokeys.timer2Cycle = 8388608;
				if ((data & this.pokeys.irqst & 4) != 0) {
					if (this.pokeys.timer4Cycle == 8388608) {
						int t = this.pokeys.basePokey.tickCycle4;
						while (t < this.cycle)
							t += this.pokeys.basePokey.periodCycles4;
						this.pokeys.timer4Cycle = t;
						if (this.nextEventCycle > t)
							this.nextEventCycle = t;
					}
				}
				else
					this.pokeys.timer4Cycle = 8388608;
			}
			else
				this.pokeys.poke(addr, data, this.cycle);
		}
		else if ((addr & 65295) == 54282) {
			int x = this.cycle % 114;
			this.cycle += (x <= 106 ? 106 : 220) - x;
		}
		else if ((addr & 65295) == 54287) {
			this.nmist = this.cycle < 28292 ? NmiStatus.ON_V_BLANK : NmiStatus.RESET;
		}
		else if ((addr & 65280) == this.moduleInfo.covoxAddr) {
			Pokey pokey;
			addr &= 3;
			if (addr == 0 || addr == 3)
				pokey = this.pokeys.basePokey;
			else
				pokey = this.pokeys.extraPokey;
			pokey.addDelta(this.pokeys, this.cycle, data - (this.covox[addr] & 0xff) << 17);
			this.covox[addr] = (byte) data;
		}
		else if ((addr & 65311) == 53279) {
			data &= 8;
			int delta = this.consol - data << 20;
			this.pokeys.basePokey.addDelta(this.pokeys, this.cycle, delta);
			this.pokeys.extraPokey.addDelta(this.pokeys, this.cycle, delta);
			this.consol = data;
		}
		else
			this.memory[addr] = (byte) data;
	}
	final PokeyPair pokeys = new PokeyPair();

	private static void putLittleEndian(byte[] buffer, int offset, int value)
	{
		buffer[offset] = (byte) value;
		buffer[offset + 1] = (byte) (value >> 8);
		buffer[offset + 2] = (byte) (value >> 16);
		buffer[offset + 3] = (byte) (value >> 24);
	}
	/**
	 * Output sample rate.
	 */
	public static final int SAMPLE_RATE = 44100;

	/**
	 * Changes the playback position.
	 * @param position The requested absolute position in milliseconds.
	 */
	public void seek(int position) throws Exception
	{
		int block = ASAP.millisecondsToBlocks(position);
		if (block < this.blocksPlayed)
			this.playSong(this.currentSong, this.currentDuration);
		while (this.blocksPlayed + this.pokeys.samples < block) {
			this.blocksPlayed += this.pokeys.samples;
			this.doFrame();
		}
		this.pokeys.sampleIndex = block - this.blocksPlayed;
		this.blocksPlayed = block;
	}
	private int silenceCycles;
	private int silenceCyclesCounter;
	private int tmcPerFrameCounter;
	/**
	 * WAV file header length.
	 */
	public static final int WAV_HEADER_LENGTH = 44;
	private static void clear(byte[] array)
	{
		for (int i = 0; i < array.length; i++)
			array[i] = 0;
	}
}
