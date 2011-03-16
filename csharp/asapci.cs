// Generated automatically with "cito". Do not edit.
namespace Sf.Asap
{

	/// <summary>8-bit Atari chip music emulator.</summary>
	/// <remarks>This class performs no I/O operations - all music data must be passed in byte arrays.</remarks>
	public class ASAP
	{
		int BlocksPlayed;

		void Call6502(int addr)
		{
			this.Memory[53760] = 32;
			this.Memory[53761] = (byte) addr;
			this.Memory[53762] = (byte) (addr >> 8);
			this.Memory[53763] = 210;
			this.Cpu.Pc = 53760;
		}

		void Call6502Player()
		{
			int player = this.ModuleInfo.Player;
			switch (this.ModuleInfo.Type) {
				case ASAPModuleType.SapB:
					this.Call6502(player);
					break;
				case ASAPModuleType.SapC:
				case ASAPModuleType.Cmc:
				case ASAPModuleType.Cm3:
				case ASAPModuleType.Cmr:
				case ASAPModuleType.Cms:
					this.Call6502(player + 6);
					break;
				case ASAPModuleType.SapD:
					if (player >= 0) {
						this.Memory[256 + this.Cpu.S] = (byte) (this.Cpu.Pc >> 8);
						this.Cpu.S = this.Cpu.S - 1 & 255;
						this.Memory[256 + this.Cpu.S] = (byte) this.Cpu.Pc;
						this.Cpu.S = this.Cpu.S - 1 & 255;
						this.Memory[53760] = 8;
						this.Memory[53761] = 72;
						this.Memory[53762] = 138;
						this.Memory[53763] = 72;
						this.Memory[53764] = 138;
						this.Memory[53765] = 72;
						this.Memory[53766] = 32;
						this.Memory[53767] = (byte) player;
						this.Memory[53768] = (byte) (player >> 8);
						this.Memory[53769] = 104;
						this.Memory[53770] = 168;
						this.Memory[53771] = 104;
						this.Memory[53772] = 170;
						this.Memory[53773] = 104;
						this.Memory[53774] = 64;
						this.Cpu.Pc = 53760;
					}
					break;
				case ASAPModuleType.SapS:
					int i = this.Memory[69] - 1;
					this.Memory[69] = (byte) i;
					if (i == 0)
						this.Memory[45179] = (byte) (this.Memory[45179] + 1);
					break;
				case ASAPModuleType.Dlt:
					this.Call6502(player + 259);
					break;
				case ASAPModuleType.Mpt:
				case ASAPModuleType.Rmt:
				case ASAPModuleType.Tm2:
					this.Call6502(player + 3);
					break;
				case ASAPModuleType.Tmc:
					if (--this.TmcPerFrameCounter <= 0) {
						this.TmcPerFrameCounter = this.TmcPerFrame;
						this.Call6502(player + 3);
					}
					else
						this.Call6502(player + 6);
					break;
			}
		}
		int Consol;
		readonly byte[] Covox = new byte[4];
		readonly Cpu6502 Cpu = new Cpu6502();
		int CurrentDuration;
		int CurrentSong;
		internal int Cycle;

		/// <summary>Enables silence detection.</summary>
		/// <remarks>Causes playback to stop after the specified period of silence.
		/// Must be called after each call of <c>Load</c>.</remarks>
		/// <param name="seconds">Length of silence which ends playback.</param>
		public void DetectSilence(int seconds)
		{
			this.SilenceCycles = seconds * this.Pokeys.MainClock;
		}

		int Do6502Frame()
		{
			this.NextEventCycle = 0;
			this.NextScanlineCycle = 0;
			this.Nmist = this.Nmist == NmiStatus.Reset ? NmiStatus.OnVBlank : NmiStatus.WasVBlank;
			int cycles = this.ModuleInfo.Ntsc ? 29868 : 35568;
			this.Cpu.DoFrame(this, cycles);
			this.Cycle -= cycles;
			if (this.NextPlayerCycle != 8388608)
				this.NextPlayerCycle -= cycles;
			if (this.Pokeys.Timer1Cycle != 8388608)
				this.Pokeys.Timer1Cycle -= cycles;
			if (this.Pokeys.Timer2Cycle != 8388608)
				this.Pokeys.Timer2Cycle -= cycles;
			if (this.Pokeys.Timer4Cycle != 8388608)
				this.Pokeys.Timer4Cycle -= cycles;
			return cycles;
		}

		void Do6502Init(int pc, int a, int x, int y)
		{
			this.Cpu.Pc = pc;
			this.Cpu.A = a & 255;
			this.Cpu.X = x & 255;
			this.Cpu.Y = y & 255;
			this.Memory[53760] = 210;
			this.Memory[510] = 255;
			this.Memory[511] = 209;
			this.Cpu.S = 253;
			for (int frame = 0; frame < 50; frame++) {
				this.Do6502Frame();
				if (this.Cpu.Pc == 53760)
					return;
			}
			throw new System.Exception("INIT routine didn't return");
		}

		int DoFrame()
		{
			this.Pokeys.StartFrame();
			int cycles = this.Do6502Frame();
			this.Pokeys.EndFrame(cycles);
			return cycles;
		}

		/// <summary>Fills the specified buffer with generated samples.</summary>
		/// <param name="buffer">The destination buffer.</param>
		/// <param name="bufferLen">Number of bytes to fill.</param>
		/// <param name="format">Format of samples.</param>
		public int Generate(byte[] buffer, int bufferLen, ASAPSampleFormat format)
		{
			return this.GenerateAt(buffer, 0, bufferLen, format);
		}

		int GenerateAt(byte[] buffer, int bufferOffset, int bufferLen, ASAPSampleFormat format)
		{
			if (this.SilenceCycles > 0 && this.SilenceCyclesCounter <= 0)
				return 0;
			int blockShift = this.ModuleInfo.Channels - 1 + (format != ASAPSampleFormat.U8 ? 1 : 0);
			int bufferBlocks = bufferLen >> blockShift;
			if (this.CurrentDuration > 0) {
				int totalBlocks = MillisecondsToBlocks(this.CurrentDuration);
				if (bufferBlocks > totalBlocks - this.BlocksPlayed)
					bufferBlocks = totalBlocks - this.BlocksPlayed;
			}
			int block = 0;
			for (;;) {
				int blocks = this.Pokeys.Generate(buffer, bufferOffset + (block << blockShift), bufferBlocks - block, format);
				this.BlocksPlayed += blocks;
				block += blocks;
				if (block >= bufferBlocks)
					break;
				int cycles = this.DoFrame();
				if (this.SilenceCycles > 0) {
					if (this.Pokeys.IsSilent()) {
						this.SilenceCyclesCounter -= cycles;
						if (this.SilenceCyclesCounter <= 0)
							break;
					}
					else
						this.SilenceCyclesCounter = this.SilenceCycles;
				}
			}
			return block << blockShift;
		}

		/// <summary>Returns current playback position in blocks.</summary>
		/// <remarks>A block is one sample or a pair of samples for stereo.</remarks>
		public int GetBlocksPlayed()
		{
			return this.BlocksPlayed;
		}

		/// <summary>Returns information about the loaded module.</summary>
		public ASAPInfo GetInfo()
		{
			return this.ModuleInfo;
		}

		/// <summary>Returns POKEY channel volume.</summary>
		/// <param name="channel">POKEY channel number (from 0 to 7).</param>
		public int GetPokeyChannelVolume(int channel)
		{
			switch (channel) {
				case 0:
					return this.Pokeys.BasePokey.Audc1 & 15;
				case 1:
					return this.Pokeys.BasePokey.Audc2 & 15;
				case 2:
					return this.Pokeys.BasePokey.Audc3 & 15;
				case 3:
					return this.Pokeys.BasePokey.Audc4 & 15;
				case 4:
					return this.Pokeys.ExtraPokey.Audc1 & 15;
				case 5:
					return this.Pokeys.ExtraPokey.Audc2 & 15;
				case 6:
					return this.Pokeys.ExtraPokey.Audc3 & 15;
				case 7:
					return this.Pokeys.ExtraPokey.Audc4 & 15;
				default:
					return 0;
			}
		}

		/// <summary>Returns current playback position in milliseconds.</summary>
		public int GetPosition()
		{
			return this.BlocksPlayed * 10 / 441;
		}

		/// <summary>Fills leading bytes of the specified buffer with WAV file header.</summary>
		/// <remarks>The number of changed bytes is <c>WavHeaderLength</c>.</remarks>
		/// <param name="buffer">The destination buffer.</param>
		/// <param name="format">Format of samples.</param>
		public void GetWavHeader(byte[] buffer, ASAPSampleFormat format)
		{
			int use16bit = format != ASAPSampleFormat.U8 ? 1 : 0;
			int blockSize = this.ModuleInfo.Channels << use16bit;
			int bytesPerSecond = 44100 * blockSize;
			int totalBlocks = MillisecondsToBlocks(this.CurrentDuration);
			int nBytes = (totalBlocks - this.BlocksPlayed) * blockSize;
			buffer[0] = 82;
			buffer[1] = 73;
			buffer[2] = 70;
			buffer[3] = 70;
			PutLittleEndian(buffer, 4, nBytes + 36);
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
			buffer[22] = (byte) this.ModuleInfo.Channels;
			buffer[23] = 0;
			PutLittleEndian(buffer, 24, 44100);
			PutLittleEndian(buffer, 28, bytesPerSecond);
			buffer[32] = (byte) blockSize;
			buffer[33] = 0;
			buffer[34] = (byte) (8 << use16bit);
			buffer[35] = 0;
			buffer[36] = 100;
			buffer[37] = 97;
			buffer[38] = 116;
			buffer[39] = 97;
			PutLittleEndian(buffer, 40, nBytes);
		}

		internal void HandleEvent()
		{
			int cycle = this.Cycle;
			if (cycle >= this.NextScanlineCycle) {
				if (cycle - this.NextScanlineCycle < 50)
					this.Cycle = cycle += 9;
				this.NextScanlineCycle += 114;
				if (cycle >= this.NextPlayerCycle) {
					this.Call6502Player();
					this.NextPlayerCycle += 114 * this.ModuleInfo.Fastplay;
				}
			}
			int nextEventCycle = this.NextScanlineCycle;
			if (cycle >= this.Pokeys.Timer1Cycle) {
				this.Pokeys.Irqst &= ~1;
				this.Pokeys.Timer1Cycle = 8388608;
			}
			else if (nextEventCycle > this.Pokeys.Timer1Cycle)
				nextEventCycle = this.Pokeys.Timer1Cycle;
			if (cycle >= this.Pokeys.Timer2Cycle) {
				this.Pokeys.Irqst &= ~2;
				this.Pokeys.Timer2Cycle = 8388608;
			}
			else if (nextEventCycle > this.Pokeys.Timer2Cycle)
				nextEventCycle = this.Pokeys.Timer2Cycle;
			if (cycle >= this.Pokeys.Timer4Cycle) {
				this.Pokeys.Irqst &= ~4;
				this.Pokeys.Timer4Cycle = 8388608;
			}
			else if (nextEventCycle > this.Pokeys.Timer4Cycle)
				nextEventCycle = this.Pokeys.Timer4Cycle;
			this.NextEventCycle = nextEventCycle;
		}

		/// <summary>Loads music data ("module").</summary>
		/// <param name="filename">Filename, used to determine the format.</param>
		/// <param name="module">Contents of the file.</param>
		/// <param name="moduleLen">Length of the file.</param>
		public void Load(string filename, byte[] module, int moduleLen)
		{
			this.SilenceCycles = 0;
			this.ModuleInfo.ParseFile(this, filename, module, moduleLen);
		}
		internal readonly byte[] Memory = new byte[65536];

		static int MillisecondsToBlocks(int milliseconds)
		{
			return milliseconds * 441 / 10;
		}
		readonly ASAPInfo ModuleInfo = new ASAPInfo();

		/// <summary>Mutes the selected POKEY channels.</summary>
		/// <param name="mask">An 8-bit mask which selects POKEY channels to be muted.</param>
		public void MutePokeyChannels(int mask)
		{
			this.Pokeys.BasePokey.Mute(mask);
			this.Pokeys.ExtraPokey.Mute(mask >> 4);
		}
		internal int NextEventCycle;
		int NextPlayerCycle;
		int NextScanlineCycle;
		NmiStatus Nmist;

		internal int PeekHardware(int addr)
		{
			switch (addr & 65311) {
				case 53268:
					return this.ModuleInfo.Ntsc ? 15 : 1;
				case 53770:
				case 53786:
					return this.Pokeys.GetRandom(addr, this.Cycle);
				case 53774:
					return this.Pokeys.Irqst;
				case 53790:
					if (this.Pokeys.ExtraPokeyMask != 0) {
						return 255;
					}
					return this.Pokeys.Irqst;
				case 53772:
				case 53788:
				case 53775:
				case 53791:
					return 255;
				case 54283:
				case 54299:
					if (this.Cycle > (this.ModuleInfo.Ntsc ? 29868 : 35568))
						return 0;
					return this.Cycle / 228;
				case 54287:
					switch (this.Nmist) {
						case NmiStatus.Reset:
							return 31;
						case NmiStatus.WasVBlank:
							return 95;
						case NmiStatus.OnVBlank:
						default:
							return this.Cycle < 28291 ? 31 : 95;
					}
				default:
					return this.Memory[addr];
			}
		}

		/// <summary>Prepares playback of the specified song of the loaded module.</summary>
		/// <param name="song">Zero-based song index.</param>
		/// <param name="duration">Playback time in milliseconds, -1 means infinity.</param>
		public void PlaySong(int song, int duration)
		{
			if (song < 0 || song >= this.ModuleInfo.Songs)
				throw new System.Exception("Song number out of range");
			this.CurrentSong = song;
			this.CurrentDuration = duration;
			this.NextPlayerCycle = 8388608;
			this.BlocksPlayed = 0;
			this.SilenceCyclesCounter = this.SilenceCycles;
			this.Cycle = 0;
			this.Cpu.Nz = 0;
			this.Cpu.C = 0;
			this.Cpu.Vdi = 0;
			this.Nmist = NmiStatus.OnVBlank;
			this.Consol = 8;
			this.Covox[0] = 128;
			this.Covox[1] = 128;
			this.Covox[2] = 128;
			this.Covox[3] = 128;
			this.Pokeys.Initialize(this.ModuleInfo.Ntsc ? 1789772 : 1773447, this.ModuleInfo.Channels > 1);
			this.MutePokeyChannels(255);
			switch (this.ModuleInfo.Type) {
				case ASAPModuleType.SapB:
					this.Do6502Init(this.ModuleInfo.Init, song, 0, 0);
					break;
				case ASAPModuleType.SapC:
				case ASAPModuleType.Cmc:
				case ASAPModuleType.Cm3:
				case ASAPModuleType.Cmr:
				case ASAPModuleType.Cms:
					this.Do6502Init(this.ModuleInfo.Player + 3, 112, this.ModuleInfo.Music, this.ModuleInfo.Music >> 8);
					this.Do6502Init(this.ModuleInfo.Player + 3, 0, song, 0);
					break;
				case ASAPModuleType.SapD:
				case ASAPModuleType.SapS:
					this.Cpu.Pc = this.ModuleInfo.Init;
					this.Cpu.A = song;
					this.Cpu.X = 0;
					this.Cpu.Y = 0;
					this.Cpu.S = 255;
					break;
				case ASAPModuleType.Dlt:
					this.Do6502Init(this.ModuleInfo.Player + 256, 0, 0, this.ModuleInfo.SongPos[song]);
					break;
				case ASAPModuleType.Mpt:
					this.Do6502Init(this.ModuleInfo.Player, 0, this.ModuleInfo.Music >> 8, this.ModuleInfo.Music);
					this.Do6502Init(this.ModuleInfo.Player, 2, this.ModuleInfo.SongPos[song], 0);
					break;
				case ASAPModuleType.Rmt:
					this.Do6502Init(this.ModuleInfo.Player, this.ModuleInfo.SongPos[song], this.ModuleInfo.Music, this.ModuleInfo.Music >> 8);
					break;
				case ASAPModuleType.Tmc:
				case ASAPModuleType.Tm2:
					this.Do6502Init(this.ModuleInfo.Player, 112, this.ModuleInfo.Music >> 8, this.ModuleInfo.Music);
					this.Do6502Init(this.ModuleInfo.Player, 0, song, 0);
					this.TmcPerFrameCounter = 1;
					break;
			}
			this.MutePokeyChannels(0);
			this.NextPlayerCycle = 0;
		}

		internal void PokeHardware(int addr, int data)
		{
			if (addr >> 8 == 210) {
				if ((addr & this.Pokeys.ExtraPokeyMask + 15) == 14) {
					this.Pokeys.Irqst |= data ^ 255;
					if ((data & this.Pokeys.Irqst & 1) != 0) {
						if (this.Pokeys.Timer1Cycle == 8388608) {
							int t = this.Pokeys.BasePokey.TickCycle1;
							while (t < this.Cycle)
								t += this.Pokeys.BasePokey.PeriodCycles1;
							this.Pokeys.Timer1Cycle = t;
							if (this.NextEventCycle > t)
								this.NextEventCycle = t;
						}
					}
					else
						this.Pokeys.Timer1Cycle = 8388608;
					if ((data & this.Pokeys.Irqst & 2) != 0) {
						if (this.Pokeys.Timer2Cycle == 8388608) {
							int t = this.Pokeys.BasePokey.TickCycle2;
							while (t < this.Cycle)
								t += this.Pokeys.BasePokey.PeriodCycles2;
							this.Pokeys.Timer2Cycle = t;
							if (this.NextEventCycle > t)
								this.NextEventCycle = t;
						}
					}
					else
						this.Pokeys.Timer2Cycle = 8388608;
					if ((data & this.Pokeys.Irqst & 4) != 0) {
						if (this.Pokeys.Timer4Cycle == 8388608) {
							int t = this.Pokeys.BasePokey.TickCycle4;
							while (t < this.Cycle)
								t += this.Pokeys.BasePokey.PeriodCycles4;
							this.Pokeys.Timer4Cycle = t;
							if (this.NextEventCycle > t)
								this.NextEventCycle = t;
						}
					}
					else
						this.Pokeys.Timer4Cycle = 8388608;
				}
				else
					this.Pokeys.Poke(addr, data, this.Cycle);
			}
			else if ((addr & 65295) == 54282) {
				int x = this.Cycle % 114;
				this.Cycle += (x <= 106 ? 106 : 220) - x;
			}
			else if ((addr & 65295) == 54287) {
				this.Nmist = this.Cycle < 28292 ? NmiStatus.OnVBlank : NmiStatus.Reset;
			}
			else if ((addr & 65280) == this.ModuleInfo.CovoxAddr) {
				Pokey pokey;
				addr &= 3;
				if (addr == 0 || addr == 3)
					pokey = this.Pokeys.BasePokey;
				else
					pokey = this.Pokeys.ExtraPokey;
				pokey.AddDelta(this.Pokeys, this.Cycle, data - this.Covox[addr] << 17);
				this.Covox[addr] = (byte) data;
			}
			else if ((addr & 65311) == 53279) {
				data &= 8;
				int delta = this.Consol - data << 20;
				this.Pokeys.BasePokey.AddDelta(this.Pokeys, this.Cycle, delta);
				this.Pokeys.ExtraPokey.AddDelta(this.Pokeys, this.Cycle, delta);
				this.Consol = data;
			}
			else
				this.Memory[addr] = (byte) data;
		}
		internal readonly PokeyPair Pokeys = new PokeyPair();

		static void PutLittleEndian(byte[] buffer, int offset, int value)
		{
			buffer[offset] = (byte) value;
			buffer[offset + 1] = (byte) (value >> 8);
			buffer[offset + 2] = (byte) (value >> 16);
			buffer[offset + 3] = (byte) (value >> 24);
		}
		/// <summary>Output sample rate.</summary>
		public const int SampleRate = 44100;

		/// <summary>Changes the playback position.</summary>
		/// <param name="position">The requested absolute position in milliseconds.</param>
		public void Seek(int position)
		{
			int block = MillisecondsToBlocks(position);
			if (block < this.BlocksPlayed)
				this.PlaySong(this.CurrentSong, this.CurrentDuration);
			while (this.BlocksPlayed + this.Pokeys.Samples < block) {
				this.BlocksPlayed += this.Pokeys.Samples;
				this.DoFrame();
			}
			this.Pokeys.SampleIndex = block - this.BlocksPlayed;
			this.BlocksPlayed = block;
		}
		int SilenceCycles;
		int SilenceCyclesCounter;
		internal int TmcPerFrame;
		int TmcPerFrameCounter;
		/// <summary>WAV file header length.</summary>
		public const int WavHeaderLength = 44;
	}

	/// <summary>Information about a music file.</summary>
	public class ASAPInfo
	{

		void AddSong(int playerCalls)
		{
			this.Durations[this.Songs++] = (int) ((long) (playerCalls * this.Fastplay) * 114000 / 1773447);
		}
		string Author;
		internal int Channels;

		int CheckDate()
		{
			int n = this.Date.Length;
			switch (n) {
				case 10:
					if (!this.CheckTwoDateDigits(0) || this.Date[2] != 47)
						return -1;
					goto case 7;
				case 7:
					if (!this.CheckTwoDateDigits(n - 7) || this.Date[n - 5] != 47)
						return -1;
					goto case 4;
				case 4:
					if (!this.CheckTwoDateDigits(n - 4) || !this.CheckTwoDateDigits(n - 2))
						return -1;
					return n;
				default:
					return -1;
			}
		}

		bool CheckTwoDateDigits(int i)
		{
			int d1 = this.Date[i];
			int d2 = this.Date[i + 1];
			return d1 >= 48 && d1 <= 57 && d2 >= 48 && d2 <= 57;
		}
		/// <summary>Short license notice.</summary>
		/// <remarks>Display after the credits.</remarks>
		public const string Copyright = "This program is free software; you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published\nby the Free Software Foundation; either version 2 of the License,\nor (at your option) any later version.";
		internal int CovoxAddr;
		/// <summary>Short credits for ASAP.</summary>
		public const string Credits = "Another Slight Atari Player (C) 2005-2011 Piotr Fusik\nCMC, MPT, TMC, TM2 players (C) 1994-2005 Marcin Lewandowski\nRMT player (C) 2002-2005 Radek Sterba\nDLT player (C) 2009 Marek Konopka\nCMS player (C) 1999 David Spilka\n";
		string Date;
		int DefaultSong;
		readonly int[] Durations = new int[32];
		internal int Fastplay;
		string Filename;

		/// <summary>Returns author's name.</summary>
		/// <remarks>A nickname may be included in parentheses after the real name.
		/// Multiple authors are separated with <c>" &amp; "</c>.
		/// An empty string means the author is unknown.</remarks>
		public string GetAuthor()
		{
			return this.Author;
		}

		/// <summary>Returns 1 for mono or 2 for stereo.</summary>
		public int GetChannels()
		{
			return this.Channels;
		}

		/// <summary>Returns music creation date.</summary>
		/// <remarks>Some of the possible formats are:
		/// <list type="bullet">
		/// <item>YYYY</item>
		/// <item>MM/YYYY</item>
		/// <item>DD/MM/YYYY</item>
		/// <item>YYYY-YYYY</item>
		/// </list>
		/// An empty string means the date is unknown.</remarks>
		public string GetDate()
		{
			return this.Date;
		}

		public int GetDayOfMonth()
		{
			int n = this.CheckDate();
			if (n != 10)
				return -1;
			return this.GetTwoDateDigits(0);
		}

		/// <summary>Returns 0-based index of the "main" song.</summary>
		/// <remarks>The specified song should be played by default.</remarks>
		public int GetDefaultSong()
		{
			return this.DefaultSong;
		}

		/// <summary>Returns length of the specified song.</summary>
		/// <remarks>The result is in milliseconds. -1 means the length is indeterminate.</remarks>
		public int GetDuration(int song)
		{
			return this.Durations[song];
		}

		/// <summary>Returns information whether the specified song loops.</summary>
		/// <remarks>Returns:
		/// <list type="bullet">
		/// <item><see langword="true" /> if the song loops</item>
		/// <item><see langword="false" /> if the song stops</item>
		/// </list>
		/// </remarks>
		public bool GetLoop(int song)
		{
			return this.Loops[song];
		}

		public int GetMonth()
		{
			int n = this.CheckDate();
			if (n < 7)
				return -1;
			return this.GetTwoDateDigits(n - 7);
		}

		static int GetPackedExt(string filename)
		{
			int ext = 0;
			for (int i = filename.Length; --i > 0;) {
				int c = filename[i];
				if (c <= 32 || c > 122)
					return 0;
				if (c == 46)
					return ext | 2105376;
				ext = (ext << 8) + c;
			}
			return 0;
		}

		static int GetRmtInstrumentFrames(byte[] module, int instrument, int volume, int volumeFrame, bool onExtraPokey)
		{
			int addrToOffset = GetWord(module, 2) - 6;
			instrument = GetWord(module, 14) - addrToOffset + (instrument << 1);
			if (module[instrument + 1] == 0)
				return 0;
			instrument = GetWord(module, instrument) - addrToOffset;
			int perFrame = module[12];
			int playerCall = volumeFrame * perFrame;
			int playerCalls = playerCall;
			int index = module[instrument] + 1 + playerCall * 3;
			int indexEnd = module[instrument + 2] + 3;
			int indexLoop = module[instrument + 3];
			if (indexLoop >= indexEnd)
				return 0;
			int volumeSlideDepth = module[instrument + 6];
			int volumeMin = module[instrument + 7];
			if (index >= indexEnd)
				index = (index - indexEnd) % (indexEnd - indexLoop) + indexLoop;
			else {
				do {
					int vol = module[instrument + index];
					if (onExtraPokey)
						vol >>= 4;
					if ((vol & 15) >= CiConstArray_2[volume])
						playerCalls = playerCall + 1;
					playerCall++;
					index += 3;
				}
				while (index < indexEnd);
			}
			if (volumeSlideDepth == 0)
				return playerCalls / perFrame;
			int volumeSlide = 128;
			bool silentLoop = false;
			for (;;) {
				if (index >= indexEnd) {
					if (silentLoop)
						break;
					silentLoop = true;
					index = indexLoop;
				}
				int vol = module[instrument + index];
				if (onExtraPokey)
					vol >>= 4;
				if ((vol & 15) >= CiConstArray_2[volume]) {
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

		/// <summary>Returns number of songs in the file.</summary>
		public int GetSongs()
		{
			return this.Songs;
		}

		/// <summary>Returns music title.</summary>
		/// <remarks>An empty string means the title is unknown.</remarks>
		public string GetTitle()
		{
			return this.Name;
		}

		/// <summary>Returns music title or filename.</summary>
		/// <remarks>If title is unknown returns filename without the path or extension.</remarks>
		public string GetTitleOrFilename()
		{
			return this.Name.Length > 0 ? this.Name : this.Filename;
		}

		int GetTwoDateDigits(int i)
		{
			return (this.Date[i] - 48) * 10 + this.Date[i + 1] - 48;
		}

		static int GetWord(byte[] array, int i)
		{
			return array[i] + (array[i + 1] << 8);
		}

		public int GetYear()
		{
			int n = this.CheckDate();
			if (n < 0)
				return -1;
			return this.GetTwoDateDigits(n - 4) * 100 + this.GetTwoDateDigits(n - 2);
		}

		static bool HasStringAt(byte[] module, int moduleIndex, string s)
		{
			int n = s.Length;
			for (int i = 0; i < n; i++)
				if (module[moduleIndex + i] != s[i])
					return false;
			return true;
		}
		int HeaderLen;
		internal int Init;

		static bool IsDltPatternEnd(byte[] module, int pos, int i)
		{
			for (int ch = 0; ch < 4; ch++) {
				int pattern = module[8198 + (ch << 8) + pos];
				if (pattern < 64) {
					int offset = 6 + (pattern << 7) + (i << 1);
					if ((module[offset] & 128) == 0 && (module[offset + 1] & 128) != 0)
						return true;
				}
			}
			return false;
		}

		static bool IsDltTrackEmpty(byte[] module, int pos)
		{
			return module[8198 + pos] >= 67 && module[8454 + pos] >= 64 && module[8710 + pos] >= 64 && module[8966 + pos] >= 64;
		}

		/// <summary>Checks whether the extension represents a module type supported by ASAP.</summary>
		/// <param name="ext">Filename extension without the leading dot.</param>
		public static bool IsOurExt(string ext)
		{
			return ext.Length == 3 && IsOurPackedExt(ext[0] + (ext[1] << 8) + (ext[2] << 16) | 2105376);
		}

		/// <summary>Checks whether the filename represents a module type supported by ASAP.</summary>
		/// <param name="filename">Filename to check the extension of.</param>
		public static bool IsOurFile(string filename)
		{
			return IsOurPackedExt(GetPackedExt(filename));
		}

		static bool IsOurPackedExt(int ext)
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

		/// <summary>Loads file information.</summary>
		/// <param name="filename">Filename, used to determine the format.</param>
		/// <param name="module">Contents of the file.</param>
		/// <param name="moduleLen">Length of the file.</param>
		public void Load(string filename, byte[] module, int moduleLen)
		{
			this.ParseFile(null, filename, module, moduleLen);
		}

		/// <summary>Loads a native module (anything except SAP) and a 6502 player routine.</summary>
		void LoadNative(ASAP asap, byte[] module, int moduleLen, byte[] playerRoutine)
		{
			if ((module[0] != 255 || module[1] != 255) && (module[0] != 0 || module[1] != 0))
				throw new System.Exception("Invalid two leading bytes of the module");
			this.Music = GetWord(module, 2);
			this.Player = GetWord(playerRoutine, 2);
			int playerLastByte = GetWord(playerRoutine, 4);
			if (this.Music <= playerLastByte)
				throw new System.Exception("Module address conflicts with the player routine");
			int musicLastByte = GetWord(module, 4);
			if (this.Music <= 55295 && musicLastByte >= 53248)
				throw new System.Exception("Module address conflicts with hardware registers");
			int blockLen = musicLastByte + 1 - this.Music;
			if (6 + blockLen != moduleLen) {
				if (this.Type != ASAPModuleType.Rmt || 11 + blockLen > moduleLen)
					throw new System.Exception("Module length doesn't match headers");
				int infoAddr = GetWord(module, 6 + blockLen);
				if (infoAddr != this.Music + blockLen)
					throw new System.Exception("Invalid address of RMT info");
				int infoLen = GetWord(module, 8 + blockLen) + 1 - infoAddr;
				if (10 + blockLen + infoLen != moduleLen)
					throw new System.Exception("Invalid RMT info block");
			}
			if (asap != null) {
				System.Array.Copy(module, 6, asap.Memory, this.Music, blockLen);
				System.Array.Copy(playerRoutine, 6, asap.Memory, this.Player, playerLastByte + 1 - this.Player);
			}
		}
		readonly bool[] Loops = new bool[32];
		/// <summary>Maximum length of a supported input file.</summary>
		/// <remarks>You may assume that files longer than this are not supported by ASAP.</remarks>
		public const int MaxModuleLength = 65000;
		/// <summary>Maximum number of songs in a file.</summary>
		public const int MaxSongs = 32;
		/// <summary>Maximum length of text metadata.</summary>
		public const int MaxTextLength = 127;
		internal int Music;
		string Name;
		internal bool Ntsc;

		void ParseCmc(ASAP asap, byte[] module, int moduleLen, ASAPModuleType type, byte[] playerRoutine)
		{
			if (moduleLen < 774)
				throw new System.Exception("Module too short");
			this.Type = type;
			this.LoadNative(asap, module, moduleLen, playerRoutine);
			if (asap != null && type == ASAPModuleType.Cmr) {
				System.Array.Copy(CiConstArray_1, 0, asap.Memory, 3087, 37);
			}
			int lastPos = 84;
			while (--lastPos >= 0) {
				if (module[518 + lastPos] < 176 || module[603 + lastPos] < 64 || module[688 + lastPos] < 64)
					break;
				if (this.Channels == 2) {
					if (module[774 + lastPos] < 176 || module[859 + lastPos] < 64 || module[944 + lastPos] < 64)
						break;
				}
			}
			this.Songs = 0;
			this.ParseCmcSong(module, 0);
			for (int pos = 0; pos < lastPos && this.Songs < 32; pos++)
				if (module[518 + pos] == 143 || module[518 + pos] == 239)
					this.ParseCmcSong(module, pos + 1);
		}

		void ParseCmcSong(byte[] module, int pos)
		{
			int tempo = module[25];
			int playerCalls = 0;
			int repStartPos = 0;
			int repEndPos = 0;
			int repTimes = 0;
			byte[] seen = new byte[85];
			while (pos >= 0 && pos < 85) {
				if (pos == repEndPos && repTimes > 0) {
					for (int i = 0; i < 85; i++)
						if (seen[i] == 1 || seen[i] == 3)
							seen[i] = 0;
					repTimes--;
					pos = repStartPos;
				}
				if (seen[pos] != 0) {
					if (seen[pos] != 1)
						this.Loops[this.Songs] = true;
					break;
				}
				seen[pos] = 1;
				int p1 = module[518 + pos];
				int p2 = module[603 + pos];
				int p3 = module[688 + pos];
				if (p1 == 254 || p2 == 254 || p3 == 254) {
					pos++;
					continue;
				}
				p1 >>= 4;
				if (p1 == 8)
					break;
				if (p1 == 9) {
					pos = p2;
					continue;
				}
				if (p1 == 10) {
					pos -= p2;
					continue;
				}
				if (p1 == 11) {
					pos += p2;
					continue;
				}
				if (p1 == 12) {
					tempo = p2;
					pos++;
					continue;
				}
				if (p1 == 13) {
					pos++;
					repStartPos = pos;
					repEndPos = pos + p2;
					repTimes = p3 - 1;
					continue;
				}
				if (p1 == 14) {
					this.Loops[this.Songs] = true;
					break;
				}
				p2 = repTimes > 0 ? 3 : 2;
				for (p1 = 0; p1 < 85; p1++)
					if (seen[p1] == 1)
						seen[p1] = (byte) p2;
				playerCalls += tempo * (this.Type == ASAPModuleType.Cm3 ? 48 : 64);
				pos++;
			}
			this.AddSong(playerCalls);
		}

		static int ParseDec(byte[] module, int moduleIndex, int maxVal)
		{
			if (module[moduleIndex] == 13)
				throw new System.Exception("Missing number");
			for (int r = 0;;) {
				int c = module[moduleIndex++];
				if (c == 13)
					return r;
				if (c < 48 || c > 57)
					throw new System.Exception("Invalid number");
				r = 10 * r + c - 48;
				if (r > maxVal)
					throw new System.Exception("Number too big");
			}
		}

		void ParseDlt(ASAP asap, byte[] module, int moduleLen)
		{
			if (moduleLen == 11270) {
				if (asap != null)
					asap.Memory[19456] = 0;
			}
			else if (moduleLen != 11271)
				throw new System.Exception("Invalid module length");
			this.Type = ASAPModuleType.Dlt;
			this.LoadNative(asap, module, moduleLen, CiBinaryResource_dlt_obx);
			if (this.Music != 8192)
				throw new System.Exception("Unsupported module address");
			bool[] seen = new bool[128];
			this.Songs = 0;
			for (int pos = 0; pos < 128 && this.Songs < 32; pos++) {
				if (!seen[pos])
					this.ParseDltSong(module, seen, pos);
			}
			if (this.Songs == 0)
				throw new System.Exception("No songs found");
		}

		void ParseDltSong(byte[] module, bool[] seen, int pos)
		{
			while (pos < 128 && !seen[pos] && IsDltTrackEmpty(module, pos))
				seen[pos++] = true;
			this.SongPos[this.Songs] = (byte) pos;
			int playerCalls = 0;
			bool loop = false;
			int tempo = 6;
			while (pos < 128) {
				if (seen[pos]) {
					loop = true;
					break;
				}
				seen[pos] = true;
				int p1 = module[8198 + pos];
				if (p1 == 64 || IsDltTrackEmpty(module, pos))
					break;
				if (p1 == 65)
					pos = module[8326 + pos];
				else if (p1 == 66)
					tempo = module[8326 + pos++];
				else {
					for (int i = 0; i < 64 && !IsDltPatternEnd(module, pos, i); i++)
						playerCalls += tempo;
					pos++;
				}
			}
			if (playerCalls > 0) {
				this.Loops[this.Songs] = loop;
				this.AddSong(playerCalls);
			}
		}

		/// <summary>Parses a string and returns the number of milliseconds it represents.</summary>
		/// <param name="s">Time in the <c>"mm:ss.xxx"</c> format.</param>
		public static int ParseDuration(string s)
		{
			int i = 0;
			int n = s.Length;
			int d;
			if (i >= n)
				throw new System.Exception("Invalid duration");
			d = s[i] - 48;
			if (d < 0 || d > 9)
				throw new System.Exception("Invalid duration");
			i++;
			int r = d;
			if (i < n) {
				d = s[i] - 48;
				if (d >= 0 && d <= 9) {
					i++;
					r = 10 * r + d;
				}
				if (i < n && s[i] == 58) {
					i++;
					if (i >= n)
						throw new System.Exception("Invalid duration");
					d = s[i] - 48;
					if (d < 0 || d > 5)
						throw new System.Exception("Invalid duration");
					i++;
					r = (6 * r + d) * 10;
					if (i >= n)
						throw new System.Exception("Invalid duration");
					d = s[i] - 48;
					if (d < 0 || d > 9)
						throw new System.Exception("Invalid duration");
					i++;
					r += d;
				}
			}
			r *= 1000;
			if (i >= n)
				return r;
			if (s[i] != 46)
				throw new System.Exception("Invalid duration");
			i++;
			if (i >= n)
				throw new System.Exception("Invalid duration");
			d = s[i] - 48;
			if (d < 0 || d > 9)
				throw new System.Exception("Invalid duration");
			i++;
			r += 100 * d;
			if (i >= n)
				return r;
			d = s[i] - 48;
			if (d < 0 || d > 9)
				throw new System.Exception("Invalid duration");
			i++;
			r += 10 * d;
			if (i >= n)
				return r;
			d = s[i] - 48;
			if (d < 0 || d > 9)
				throw new System.Exception("Invalid duration");
			i++;
			r += d;
			return r;
		}

		internal void ParseFile(ASAP asap, string filename, byte[] module, int moduleLen)
		{
			int len = filename.Length;
			int basename = 0;
			int ext = -1;
			for (int i = len; --i >= 0;) {
				int c = filename[i];
				if (c == 47 || c == 92) {
					basename = i + 1;
					break;
				}
				if (c == 46)
					ext = i;
			}
			if (ext < 0)
				throw new System.Exception("Filename has no extension");
			ext -= basename;
			if (ext > 127)
				ext = 127;
			this.Filename = filename.Substring(basename, ext);
			this.Author = "";
			this.Name = "";
			this.Date = "";
			this.Channels = 1;
			this.Songs = 1;
			this.DefaultSong = 0;
			for (int i = 0; i < 32; i++) {
				this.Durations[i] = -1;
				this.Loops[i] = false;
			}
			this.Ntsc = false;
			this.Fastplay = 312;
			this.Music = -1;
			this.Init = -1;
			this.Player = -1;
			this.CovoxAddr = -1;
			switch (GetPackedExt(filename)) {
				case 7364979:
					this.ParseSap(asap, module, moduleLen);
					return;
				case 6516067:
					this.ParseCmc(asap, module, moduleLen, ASAPModuleType.Cmc, CiBinaryResource_cmc_obx);
					return;
				case 3370339:
					this.ParseCmc(asap, module, moduleLen, ASAPModuleType.Cm3, CiBinaryResource_cm3_obx);
					return;
				case 7499107:
					this.ParseCmc(asap, module, moduleLen, ASAPModuleType.Cmr, CiBinaryResource_cmc_obx);
					return;
				case 7564643:
					this.Channels = 2;
					this.ParseCmc(asap, module, moduleLen, ASAPModuleType.Cms, CiBinaryResource_cms_obx);
					return;
				case 6516068:
					this.Fastplay = 156;
					this.ParseCmc(asap, module, moduleLen, ASAPModuleType.Cmc, CiBinaryResource_cmc_obx);
					return;
				case 7629924:
					this.ParseDlt(asap, module, moduleLen);
					return;
				case 7630957:
					this.ParseMpt(asap, module, moduleLen);
					return;
				case 6582381:
					this.Fastplay = 156;
					this.ParseMpt(asap, module, moduleLen);
					return;
				case 7630194:
					this.ParseRmt(asap, module, moduleLen);
					return;
				case 6516084:
				case 3698036:
					this.ParseTmc(asap, module, moduleLen);
					return;
				case 3304820:
					this.ParseTm2(asap, module, moduleLen);
					return;
				default:
					throw new System.Exception("Unknown filename extension");
			}
		}

		static int ParseHex(byte[] module, int moduleIndex)
		{
			if (module[moduleIndex] == 13)
				throw new System.Exception("Missing number");
			for (int r = 0;;) {
				int c = module[moduleIndex++];
				if (c == 13)
					return r;
				if (r > 4095)
					throw new System.Exception("Number too big");
				r <<= 4;
				if (c >= 48 && c <= 57)
					r += c - 48;
				else if (c >= 65 && c <= 70)
					r += c - 65 + 10;
				else if (c >= 97 && c <= 102)
					r += c - 97 + 10;
				else
					throw new System.Exception("Invalid number");
			}
		}

		void ParseMpt(ASAP asap, byte[] module, int moduleLen)
		{
			if (moduleLen < 464)
				throw new System.Exception("Module too short");
			this.Type = ASAPModuleType.Mpt;
			this.LoadNative(asap, module, moduleLen, CiBinaryResource_mpt_obx);
			int track0Addr = GetWord(module, 2) + 458;
			if (module[454] + (module[458] << 8) != track0Addr)
				throw new System.Exception("Invalid address of the first track");
			int songLen = module[455] + (module[459] << 8) - track0Addr >> 1;
			if (songLen > 254)
				throw new System.Exception("Song too long");
			bool[] globalSeen = new bool[256];
			this.Songs = 0;
			for (int pos = 0; pos < songLen && this.Songs < 32; pos++) {
				if (!globalSeen[pos]) {
					this.SongPos[this.Songs] = (byte) pos;
					this.ParseMptSong(module, globalSeen, songLen, pos);
				}
			}
			if (this.Songs == 0)
				throw new System.Exception("No songs found");
		}

		void ParseMptSong(byte[] module, bool[] globalSeen, int songLen, int pos)
		{
			int addrToOffset = GetWord(module, 2) - 6;
			int tempo = module[463];
			int playerCalls = 0;
			byte[] seen = new byte[256];
			int[] patternOffset = new int[4];
			int[] blankRows = new int[4];
			int[] blankRowsCounter = new int[4];
			while (pos < songLen) {
				if (seen[pos] != 0) {
					if (seen[pos] != 1)
						this.Loops[this.Songs] = true;
					break;
				}
				seen[pos] = 1;
				globalSeen[pos] = true;
				int i = module[464 + pos * 2];
				if (i == 255) {
					pos = module[465 + pos * 2];
					continue;
				}
				int ch;
				for (ch = 3; ch >= 0; ch--) {
					i = module[454 + ch] + (module[458 + ch] << 8) - addrToOffset;
					i = module[i + pos * 2];
					if (i >= 64)
						break;
					i <<= 1;
					i = GetWord(module, 70 + i);
					patternOffset[ch] = i == 0 ? 0 : i - addrToOffset;
					blankRowsCounter[ch] = 0;
				}
				if (ch >= 0)
					break;
				for (i = 0; i < songLen; i++)
					if (seen[i] == 1)
						seen[i] = 2;
				for (int patternRows = module[462]; --patternRows >= 0;) {
					for (ch = 3; ch >= 0; ch--) {
						if (patternOffset[ch] == 0 || --blankRowsCounter[ch] >= 0)
							continue;
						for (;;) {
							i = module[patternOffset[ch]++];
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
				this.AddSong(playerCalls);
		}

		void ParseRmt(ASAP asap, byte[] module, int moduleLen)
		{
			if (moduleLen < 48)
				throw new System.Exception("Module too short");
			if (module[6] != 82 || module[7] != 77 || module[8] != 84 || module[13] != 1)
				throw new System.Exception("Invalid module header");
			int posShift;
			switch (module[9]) {
				case 52:
					posShift = 2;
					break;
				case 56:
					this.Channels = 2;
					posShift = 3;
					break;
				default:
					throw new System.Exception("Unsupported number of channels");
			}
			int perFrame = module[12];
			if (perFrame < 1 || perFrame > 4)
				throw new System.Exception("Unsupported player call rate");
			this.Type = ASAPModuleType.Rmt;
			this.LoadNative(asap, module, moduleLen, this.Channels == 2 ? CiBinaryResource_rmt8_obx : CiBinaryResource_rmt4_obx);
			int songLen = GetWord(module, 4) + 1 - GetWord(module, 20);
			if (posShift == 3 && (songLen & 4) != 0 && module[6 + GetWord(module, 4) - GetWord(module, 2) - 3] == 254)
				songLen += 4;
			songLen >>= posShift;
			if (songLen >= 256)
				throw new System.Exception("Song too long");
			bool[] globalSeen = new bool[256];
			this.Songs = 0;
			for (int pos = 0; pos < songLen && this.Songs < 32; pos++) {
				if (!globalSeen[pos]) {
					this.SongPos[this.Songs] = (byte) pos;
					this.ParseRmtSong(module, globalSeen, songLen, posShift, pos);
				}
			}
			this.Fastplay = 312 / perFrame;
			this.Player = 1536;
			if (this.Songs == 0)
				throw new System.Exception("No songs found");
		}

		void ParseRmtSong(byte[] module, bool[] globalSeen, int songLen, int posShift, int pos)
		{
			int addrToOffset = GetWord(module, 2) - 6;
			int tempo = module[11];
			int frames = 0;
			int songOffset = GetWord(module, 20) - addrToOffset;
			int patternLoOffset = GetWord(module, 16) - addrToOffset;
			int patternHiOffset = GetWord(module, 18) - addrToOffset;
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
					if (seen[pos] != 1)
						this.Loops[this.Songs] = true;
					break;
				}
				seen[pos] = 1;
				globalSeen[pos] = true;
				if (module[songOffset + (pos << posShift)] == 254) {
					pos = module[songOffset + (pos << posShift) + 1];
					continue;
				}
				for (int ch = 0; ch < 1 << posShift; ch++) {
					int p = module[songOffset + (pos << posShift) + ch];
					if (p == 255)
						blankRows[ch] = 256;
					else {
						patternOffset[ch] = patternBegin[ch] = module[patternLoOffset + p] + (module[patternHiOffset + p] << 8) - addrToOffset;
						blankRows[ch] = 0;
					}
				}
				for (int i = 0; i < songLen; i++)
					if (seen[i] == 1)
						seen[i] = 2;
				for (int patternRows = module[10]; --patternRows >= 0;) {
					for (int ch = 0; ch < 1 << posShift; ch++) {
						if (--blankRows[ch] > 0)
							continue;
						for (;;) {
							int i = module[patternOffset[ch]++];
							if ((i & 63) < 62) {
								i += module[patternOffset[ch]++] << 8;
								if ((i & 63) != 61) {
									instrumentNo[ch] = i >> 10;
									instrumentFrame[ch] = frames;
								}
								volumeValue[ch] = i >> 6 & 15;
								volumeFrame[ch] = frames;
								break;
							}
							if (i == 62) {
								blankRows[ch] = module[patternOffset[ch]++];
								break;
							}
							if ((i & 63) == 62) {
								blankRows[ch] = i >> 6;
								break;
							}
							if ((i & 191) == 63) {
								tempo = module[patternOffset[ch]++];
								continue;
							}
							if (i == 191) {
								patternOffset[ch] = patternBegin[ch] + module[patternOffset[ch]];
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
				frame += GetRmtInstrumentFrames(module, instrumentNo[ch], volumeValue[ch], volumeFrame[ch] - frame, ch >= 4);
				if (instrumentFrames < frame)
					instrumentFrames = frame;
			}
			if (frames > instrumentFrames) {
				if (frames - instrumentFrames > 100)
					this.Loops[this.Songs] = false;
				frames = instrumentFrames;
			}
			if (frames > 0)
				this.AddSong(frames);
		}

		void ParseSap(ASAP asap, byte[] module, int moduleLen)
		{
			this.ParseSapHeader(module, moduleLen);
			if (asap == null)
				return;
			System.Array.Clear(asap.Memory, 0, 65536);
			int moduleIndex = this.HeaderLen + 2;
			while (moduleIndex + 5 <= moduleLen) {
				int start_addr = GetWord(module, moduleIndex);
				int block_len = GetWord(module, moduleIndex + 2) + 1 - start_addr;
				if (block_len <= 0 || moduleIndex + block_len > moduleLen)
					throw new System.Exception("Invalid binary block");
				moduleIndex += 4;
				System.Array.Copy(module, moduleIndex, asap.Memory, start_addr, block_len);
				moduleIndex += block_len;
				if (moduleIndex == moduleLen)
					return;
				if (moduleIndex + 7 <= moduleLen && module[moduleIndex] == 255 && module[moduleIndex + 1] == 255)
					moduleIndex += 2;
			}
			throw new System.Exception("Invalid binary block");
		}

		void ParseSapHeader(byte[] module, int moduleLen)
		{
			if (!HasStringAt(module, 0, "SAP\r\n"))
				throw new System.Exception("Missing SAP header");
			this.Fastplay = -1;
			int type = 0;
			int moduleIndex = 5;
			int durationIndex = 0;
			while (module[moduleIndex] != 255) {
				if (moduleIndex + 8 >= moduleLen)
					throw new System.Exception("Missing binary part");
				if (HasStringAt(module, moduleIndex, "AUTHOR ")) {
					int len = ParseText(module, moduleIndex + 7);
					if (len > 0)
						this.Author = System.Text.Encoding.UTF8.GetString(module, moduleIndex + 7 + 1, len);
				}
				else if (HasStringAt(module, moduleIndex, "NAME ")) {
					int len = ParseText(module, moduleIndex + 5);
					if (len > 0)
						this.Name = System.Text.Encoding.UTF8.GetString(module, moduleIndex + 5 + 1, len);
				}
				else if (HasStringAt(module, moduleIndex, "DATE ")) {
					int len = ParseText(module, moduleIndex + 5);
					if (len > 0)
						this.Date = System.Text.Encoding.UTF8.GetString(module, moduleIndex + 5 + 1, len);
				}
				else if (HasStringAt(module, moduleIndex, "SONGS ")) {
					this.Songs = ParseDec(module, moduleIndex + 6, 32);
					if (this.Songs < 1)
						throw new System.Exception("Number too small");
				}
				else if (HasStringAt(module, moduleIndex, "DEFSONG ")) {
					this.DefaultSong = ParseDec(module, moduleIndex + 8, 31);
					if (this.DefaultSong < 0)
						throw new System.Exception("Number too small");
				}
				else if (HasStringAt(module, moduleIndex, "STEREO\r"))
					this.Channels = 2;
				else if (HasStringAt(module, moduleIndex, "NTSC\r"))
					this.Ntsc = true;
				else if (HasStringAt(module, moduleIndex, "TIME ")) {
					if (durationIndex >= 32)
						throw new System.Exception("Too many TIME tags");
					moduleIndex += 5;
					int len;
					for (len = 0; module[moduleIndex + len] != 13; len++) {
					}
					if (len > 5 && HasStringAt(module, moduleIndex + len - 5, " LOOP")) {
						this.Loops[durationIndex] = true;
						len -= 5;
					}
					if (len > 9)
						throw new System.Exception("Invalid TIME tag");
					string s = System.Text.Encoding.UTF8.GetString(module, moduleIndex, len);
					int duration = ParseDuration(s);
					this.Durations[durationIndex++] = duration;
				}
				else if (HasStringAt(module, moduleIndex, "TYPE "))
					type = module[moduleIndex + 5];
				else if (HasStringAt(module, moduleIndex, "FASTPLAY ")) {
					this.Fastplay = ParseDec(module, moduleIndex + 9, 312);
					if (this.Fastplay < 1)
						throw new System.Exception("Number too small");
				}
				else if (HasStringAt(module, moduleIndex, "MUSIC ")) {
					this.Music = ParseHex(module, moduleIndex + 6);
				}
				else if (HasStringAt(module, moduleIndex, "INIT ")) {
					this.Init = ParseHex(module, moduleIndex + 5);
				}
				else if (HasStringAt(module, moduleIndex, "PLAYER ")) {
					this.Player = ParseHex(module, moduleIndex + 7);
				}
				else if (HasStringAt(module, moduleIndex, "COVOX ")) {
					this.CovoxAddr = ParseHex(module, moduleIndex + 6);
					if (this.CovoxAddr != 54784)
						throw new System.Exception("COVOX should be D600");
					this.Channels = 2;
				}
				while (module[moduleIndex++] != 13) {
					if (moduleIndex >= moduleLen)
						throw new System.Exception("Malformed SAP header");
				}
				if (module[moduleIndex++] != 10)
					throw new System.Exception("Malformed SAP header");
			}
			if (this.DefaultSong >= this.Songs)
				throw new System.Exception("DEFSONG too big");
			switch (type) {
				case 66:
					if (this.Player < 0)
						throw new System.Exception("Missing PLAYER tag");
					if (this.Init < 0)
						throw new System.Exception("Missing INIT tag");
					this.Type = ASAPModuleType.SapB;
					break;
				case 67:
					if (this.Player < 0)
						throw new System.Exception("Missing PLAYER tag");
					if (this.Music < 0)
						throw new System.Exception("Missing MUSIC tag");
					this.Type = ASAPModuleType.SapC;
					break;
				case 68:
					if (this.Init < 0)
						throw new System.Exception("Missing INIT tag");
					this.Type = ASAPModuleType.SapD;
					break;
				case 83:
					if (this.Init < 0)
						throw new System.Exception("Missing INIT tag");
					this.Type = ASAPModuleType.SapS;
					this.Fastplay = 78;
					break;
				default:
					throw new System.Exception("Unsupported TYPE");
			}
			if (this.Fastplay < 0)
				this.Fastplay = this.Ntsc ? 262 : 312;
			else if (this.Ntsc && this.Fastplay > 262)
				throw new System.Exception("FASTPLAY too big");
			if (module[moduleIndex + 1] != 255)
				throw new System.Exception("Invalid binary header");
			this.HeaderLen = moduleIndex;
		}

		static int ParseText(byte[] module, int moduleIndex)
		{
			if (module[moduleIndex] != 34)
				throw new System.Exception("Missing quote");
			if (HasStringAt(module, moduleIndex + 1, "<?>\"\r"))
				return 0;
			for (int len = 0;; len++) {
				int c = module[moduleIndex + 1 + len];
				if (c == 34) {
					if (module[moduleIndex + 2 + len] != 13)
						throw new System.Exception("Invalid text tag");
					return len;
				}
				if (c < 32 || c >= 127)
					throw new System.Exception("Invalid character");
			}
		}

		void ParseTm2(ASAP asap, byte[] module, int moduleLen)
		{
			if (moduleLen < 932)
				throw new System.Exception("Module too short");
			this.Type = ASAPModuleType.Tm2;
			this.LoadNative(asap, module, moduleLen, CiBinaryResource_tm2_obx);
			int i = module[37];
			if (i < 1 || i > 4)
				throw new System.Exception("Unsupported player call rate");
			this.Fastplay = 312 / i;
			this.Player = 1280;
			if (module[31] != 0)
				this.Channels = 2;
			int lastPos = 65535;
			for (i = 0; i < 128; i++) {
				int instrAddr = module[134 + i] + (module[774 + i] << 8);
				if (instrAddr != 0 && instrAddr < lastPos)
					lastPos = instrAddr;
			}
			for (i = 0; i < 256; i++) {
				int patternAddr = module[262 + i] + (module[518 + i] << 8);
				if (patternAddr != 0 && patternAddr < lastPos)
					lastPos = patternAddr;
			}
			lastPos -= GetWord(module, 2) + 896;
			if (902 + lastPos >= moduleLen)
				throw new System.Exception("Module too short");
			int c;
			do {
				if (lastPos <= 0)
					throw new System.Exception("No songs found");
				lastPos -= 17;
				c = module[918 + lastPos];
			}
			while (c == 0 || c >= 128);
			this.Songs = 0;
			this.ParseTm2Song(module, 0);
			for (i = 0; i < lastPos && this.Songs < 32; i += 17) {
				c = module[918 + i];
				if (c == 0 || c >= 128)
					this.ParseTm2Song(module, i + 17);
			}
		}

		void ParseTm2Song(byte[] module, int pos)
		{
			int addrToOffset = GetWord(module, 2) - 6;
			int tempo = module[36] + 1;
			int playerCalls = 0;
			int[] patternOffset = new int[8];
			int[] blankRows = new int[8];
			for (;;) {
				int patternRows = module[918 + pos];
				if (patternRows == 0)
					break;
				if (patternRows >= 128) {
					this.Loops[this.Songs] = true;
					break;
				}
				for (int ch = 7; ch >= 0; ch--) {
					int pat = module[917 + pos - 2 * ch];
					patternOffset[ch] = module[262 + pat] + (module[518 + pat] << 8) - addrToOffset;
					blankRows[ch] = 0;
				}
				while (--patternRows >= 0) {
					for (int ch = 7; ch >= 0; ch--) {
						if (--blankRows[ch] >= 0)
							continue;
						for (;;) {
							int i = module[patternOffset[ch]++];
							if (i == 0) {
								patternOffset[ch]++;
								break;
							}
							if (i < 64) {
								if (module[patternOffset[ch]++] >= 128)
									patternOffset[ch]++;
								break;
							}
							if (i < 128) {
								patternOffset[ch]++;
								break;
							}
							if (i == 128) {
								blankRows[ch] = module[patternOffset[ch]++];
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
			this.AddSong(playerCalls);
		}

		void ParseTmc(ASAP asap, byte[] module, int moduleLen)
		{
			if (moduleLen < 464)
				throw new System.Exception("Module too short");
			this.Type = ASAPModuleType.Tmc;
			this.LoadNative(asap, module, moduleLen, CiBinaryResource_tmc_obx);
			this.Channels = 2;
			int i = 0;
			while (module[102 + i] == 0) {
				if (++i >= 64)
					throw new System.Exception("No instruments");
			}
			int lastPos = (module[102 + i] << 8) + module[38 + i] - GetWord(module, 2) - 432;
			if (437 + lastPos >= moduleLen)
				throw new System.Exception("Module too short");
			do {
				if (lastPos <= 0)
					throw new System.Exception("No songs found");
				lastPos -= 16;
			}
			while (module[437 + lastPos] >= 128);
			this.Songs = 0;
			this.ParseTmcSong(module, 0);
			for (i = 0; i < lastPos && this.Songs < 32; i += 16)
				if (module[437 + i] >= 128)
					this.ParseTmcSong(module, i + 16);
			i = module[37];
			if (i < 1 || i > 4)
				throw new System.Exception("Unsupported player call rate");
			if (asap != null)
				asap.TmcPerFrame = i;
			this.Fastplay = 312 / i;
		}

		void ParseTmcSong(byte[] module, int pos)
		{
			int addrToOffset = GetWord(module, 2) - 6;
			int tempo = module[36] + 1;
			int frames = 0;
			int[] patternOffset = new int[8];
			int[] blankRows = new int[8];
			while (module[437 + pos] < 128) {
				for (int ch = 7; ch >= 0; ch--) {
					int pat = module[437 + pos - 2 * ch];
					patternOffset[ch] = module[166 + pat] + (module[294 + pat] << 8) - addrToOffset;
					blankRows[ch] = 0;
				}
				for (int patternRows = 64; --patternRows >= 0;) {
					for (int ch = 7; ch >= 0; ch--) {
						if (--blankRows[ch] >= 0)
							continue;
						for (;;) {
							int i = module[patternOffset[ch]++];
							if (i < 64) {
								patternOffset[ch]++;
								break;
							}
							if (i == 64) {
								i = module[patternOffset[ch]++];
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
			if (module[436 + pos] < 128)
				this.Loops[this.Songs] = true;
			this.AddSong(frames);
		}
		internal int Player;
		internal readonly byte[] SongPos = new byte[32];
		internal int Songs;
		internal ASAPModuleType Type;
		/// <summary>ASAP version as a string.</summary>
		public const string Version = "3.0.0";
		/// <summary>ASAP version - major part.</summary>
		public const int VersionMajor = 3;
		/// <summary>ASAP version - micro part.</summary>
		public const int VersionMicro = 0;
		/// <summary>ASAP version - minor part.</summary>
		public const int VersionMinor = 0;
		/// <summary>Years ASAP was created in.</summary>
		public const string Years = "2005-2011";
		static readonly byte[] CiConstArray_1 = { 92, 86, 80, 77, 71, 68, 65, 62, 56, 53, 136, 127, 121, 115, 108, 103,
			96, 90, 85, 81, 76, 72, 67, 63, 61, 57, 52, 51, 48, 45, 42, 40,
			37, 36, 33, 31, 30 };
		static readonly byte[] CiConstArray_2 = { 16, 8, 4, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 };
		static readonly byte[] CiBinaryResource_cm3_obx = { 255, 255, 0, 5, 223, 12, 76, 18, 11, 76, 120, 5, 76, 203, 7, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 160, 227, 237, 227, 160, 240, 236, 225,
			249, 229, 242, 160, 246, 160, 178, 174, 177, 160, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255,
			255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 128, 128, 128, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 141, 110,
			5, 142, 111, 5, 140, 112, 5, 41, 112, 74, 74, 74, 170, 189, 148, 11,
			141, 169, 5, 189, 149, 11, 141, 170, 5, 169, 3, 141, 15, 210, 216, 165,
			254, 72, 165, 255, 72, 172, 112, 5, 174, 111, 5, 173, 110, 5, 32, 178,
			5, 104, 133, 255, 104, 133, 254, 96, 173, 118, 5, 133, 254, 173, 119, 5,
			133, 255, 160, 0, 138, 240, 28, 177, 254, 201, 143, 240, 4, 201, 239, 208,
			12, 202, 208, 9, 200, 192, 84, 176, 9, 152, 170, 16, 6, 200, 192, 84,
			144, 229, 96, 142, 104, 5, 32, 123, 6, 169, 0, 162, 9, 157, 69, 5,
			202, 16, 250, 141, 103, 5, 169, 1, 141, 113, 5, 169, 255, 141, 106, 5,
			173, 114, 5, 133, 254, 173, 115, 5, 133, 255, 160, 19, 177, 254, 170, 173,
			118, 5, 133, 254, 173, 119, 5, 133, 255, 172, 104, 5, 177, 254, 201, 207,
			208, 13, 152, 24, 105, 85, 168, 177, 254, 48, 15, 170, 76, 52, 6, 201,
			143, 240, 7, 201, 239, 240, 3, 136, 16, 226, 142, 108, 5, 142, 109, 5,
			96, 41, 15, 240, 245, 142, 221, 10, 142, 243, 10, 142, 2, 11, 140, 222,
			10, 140, 244, 10, 140, 3, 11, 96, 142, 114, 5, 134, 254, 140, 115, 5,
			132, 255, 24, 138, 105, 20, 141, 116, 5, 152, 105, 0, 141, 117, 5, 142,
			118, 5, 200, 200, 140, 119, 5, 160, 19, 177, 254, 141, 108, 5, 141, 109,
			5, 162, 8, 169, 0, 141, 113, 5, 157, 0, 210, 224, 3, 176, 8, 157,
			9, 5, 169, 255, 157, 57, 5, 202, 16, 233, 169, 128, 162, 3, 157, 75,
			5, 202, 16, 250, 96, 169, 1, 141, 113, 5, 169, 0, 240, 238, 41, 3,
			201, 3, 240, 240, 224, 64, 176, 236, 192, 26, 176, 232, 170, 169, 128, 157,
			75, 5, 169, 0, 157, 57, 5, 157, 60, 5, 157, 63, 5, 173, 111, 5,
			157, 12, 5, 173, 112, 5, 10, 10, 10, 133, 254, 24, 173, 114, 5, 105,
			48, 72, 173, 115, 5, 105, 1, 168, 104, 24, 101, 254, 157, 97, 5, 152,
			105, 0, 157, 100, 5, 24, 173, 114, 5, 105, 148, 133, 254, 173, 115, 5,
			105, 0, 133, 255, 173, 112, 5, 10, 109, 112, 5, 10, 168, 177, 254, 157,
			79, 5, 200, 177, 254, 157, 82, 5, 41, 7, 141, 110, 5, 200, 177, 254,
			157, 85, 5, 200, 177, 254, 157, 88, 5, 200, 177, 254, 157, 91, 5, 200,
			177, 254, 157, 94, 5, 160, 0, 173, 110, 5, 201, 3, 208, 2, 160, 2,
			201, 7, 208, 2, 160, 4, 185, 178, 11, 133, 254, 185, 179, 11, 133, 255,
			189, 85, 5, 74, 74, 74, 74, 24, 109, 111, 5, 141, 111, 5, 141, 194,
			7, 168, 173, 110, 5, 201, 7, 208, 15, 152, 10, 168, 177, 254, 157, 45,
			5, 200, 140, 111, 5, 76, 131, 7, 177, 254, 157, 45, 5, 189, 85, 5,
			41, 15, 24, 109, 111, 5, 141, 111, 5, 172, 111, 5, 173, 110, 5, 201,
			5, 8, 177, 254, 40, 240, 8, 221, 45, 5, 208, 3, 56, 233, 1, 157,
			48, 5, 189, 79, 5, 72, 41, 3, 168, 185, 184, 11, 157, 54, 5, 104,
			74, 74, 74, 74, 160, 62, 201, 15, 240, 16, 160, 55, 201, 14, 240, 10,
			160, 48, 201, 13, 240, 4, 24, 105, 0, 168, 185, 188, 11, 157, 51, 5,
			96, 216, 165, 252, 72, 165, 253, 72, 165, 254, 72, 165, 255, 72, 173, 113,
			5, 208, 3, 76, 5, 11, 173, 78, 5, 240, 3, 76, 110, 9, 173, 108,
			5, 205, 109, 5, 240, 3, 76, 91, 9, 173, 103, 5, 240, 3, 76, 220,
			8, 162, 2, 188, 75, 5, 48, 3, 157, 75, 5, 157, 69, 5, 202, 16,
			242, 173, 118, 5, 133, 252, 173, 119, 5, 133, 253, 172, 104, 5, 132, 254,
			204, 106, 5, 208, 25, 173, 107, 5, 240, 20, 173, 104, 5, 172, 105, 5,
			140, 104, 5, 206, 107, 5, 208, 232, 141, 104, 5, 168, 16, 226, 162, 0,
			177, 252, 201, 254, 208, 14, 172, 104, 5, 200, 196, 254, 240, 67, 140, 104,
			5, 76, 26, 8, 157, 66, 5, 24, 152, 105, 85, 168, 232, 224, 3, 144,
			223, 172, 104, 5, 177, 252, 16, 122, 201, 255, 240, 118, 74, 74, 74, 41,
			14, 170, 189, 164, 11, 141, 126, 8, 189, 165, 11, 141, 127, 8, 173, 67,
			5, 133, 255, 32, 147, 8, 140, 104, 5, 192, 85, 176, 4, 196, 254, 208,
			143, 164, 254, 140, 104, 5, 76, 5, 11, 32, 148, 6, 160, 255, 96, 48,
			251, 168, 96, 48, 247, 56, 152, 229, 255, 168, 96, 48, 239, 24, 152, 101,
			255, 168, 96, 48, 231, 141, 108, 5, 141, 109, 5, 200, 96, 48, 221, 173,
			68, 5, 48, 216, 141, 107, 5, 200, 140, 105, 5, 24, 152, 101, 255, 141,
			106, 5, 96, 136, 48, 10, 177, 252, 201, 143, 240, 4, 201, 239, 208, 243,
			200, 96, 162, 2, 189, 72, 5, 240, 5, 222, 72, 5, 16, 99, 189, 75,
			5, 208, 94, 188, 66, 5, 192, 64, 176, 87, 173, 116, 5, 133, 252, 173,
			117, 5, 133, 253, 177, 252, 133, 254, 24, 152, 105, 64, 168, 177, 252, 133,
			255, 37, 254, 201, 255, 240, 58, 188, 69, 5, 177, 254, 41, 192, 208, 12,
			177, 254, 41, 63, 157, 15, 5, 254, 69, 5, 16, 235, 201, 64, 208, 19,
			177, 254, 41, 63, 141, 111, 5, 189, 15, 5, 141, 112, 5, 32, 188, 6,
			76, 72, 9, 201, 128, 208, 10, 177, 254, 41, 63, 157, 72, 5, 254, 69,
			5, 202, 16, 144, 174, 103, 5, 232, 224, 48, 144, 2, 162, 0, 142, 103,
			5, 206, 109, 5, 208, 14, 173, 108, 5, 141, 109, 5, 173, 103, 5, 208,
			3, 238, 104, 5, 172, 48, 5, 173, 82, 5, 41, 7, 201, 5, 240, 4,
			201, 6, 208, 1, 136, 140, 39, 5, 160, 0, 201, 5, 240, 4, 201, 6,
			208, 2, 160, 2, 201, 7, 208, 2, 160, 40, 140, 44, 5, 162, 2, 189,
			82, 5, 41, 224, 157, 40, 5, 189, 97, 5, 133, 252, 189, 100, 5, 133,
			253, 189, 57, 5, 201, 255, 240, 54, 201, 15, 208, 32, 189, 63, 5, 240,
			45, 222, 63, 5, 189, 63, 5, 208, 37, 188, 9, 5, 240, 1, 136, 152,
			157, 9, 5, 189, 88, 5, 157, 63, 5, 76, 232, 9, 189, 57, 5, 74,
			168, 177, 252, 144, 4, 74, 74, 74, 74, 41, 15, 157, 9, 5, 188, 45,
			5, 189, 82, 5, 41, 7, 201, 1, 208, 31, 136, 152, 200, 221, 48, 5,
			8, 169, 1, 40, 208, 2, 10, 10, 61, 60, 5, 240, 12, 188, 48, 5,
			192, 255, 208, 5, 169, 0, 157, 9, 5, 152, 157, 36, 5, 169, 1, 141,
			110, 5, 189, 57, 5, 201, 15, 240, 56, 41, 7, 168, 185, 208, 12, 133,
			254, 189, 57, 5, 41, 8, 8, 138, 40, 24, 240, 2, 105, 3, 168, 185,
			91, 5, 37, 254, 240, 27, 189, 51, 5, 157, 36, 5, 142, 110, 5, 202,
			16, 8, 141, 39, 5, 169, 0, 141, 44, 5, 232, 189, 54, 5, 157, 40,
			5, 189, 57, 5, 41, 15, 201, 15, 240, 16, 254, 57, 5, 189, 57, 5,
			201, 15, 208, 6, 189, 88, 5, 157, 63, 5, 189, 75, 5, 16, 10, 189,
			9, 5, 208, 5, 169, 64, 157, 75, 5, 254, 60, 5, 160, 0, 189, 82,
			5, 74, 74, 74, 74, 144, 1, 136, 74, 144, 1, 200, 24, 152, 125, 45,
			5, 157, 45, 5, 189, 48, 5, 201, 255, 208, 2, 160, 0, 24, 152, 125,
			48, 5, 157, 48, 5, 202, 48, 3, 76, 153, 9, 173, 40, 5, 141, 43,
			5, 173, 82, 5, 41, 7, 170, 160, 3, 173, 110, 5, 240, 3, 188, 216,
			12, 152, 72, 185, 188, 12, 8, 41, 127, 170, 152, 41, 3, 10, 168, 189,
			36, 5, 153, 0, 210, 200, 189, 9, 5, 224, 3, 208, 3, 173, 9, 5,
			29, 40, 5, 40, 16, 2, 169, 0, 153, 0, 210, 104, 168, 136, 41, 3,
			208, 207, 160, 8, 173, 44, 5, 153, 0, 210, 24, 104, 133, 255, 104, 133,
			254, 104, 133, 253, 104, 133, 252, 96, 104, 170, 240, 78, 201, 2, 240, 6,
			104, 104, 202, 208, 251, 96, 165, 20, 197, 20, 240, 252, 173, 36, 2, 201,
			137, 208, 7, 173, 37, 2, 201, 11, 240, 230, 173, 36, 2, 141, 146, 11,
			173, 37, 2, 141, 147, 11, 169, 137, 141, 36, 2, 169, 11, 141, 37, 2,
			104, 104, 240, 3, 56, 233, 1, 141, 96, 11, 104, 168, 104, 170, 169, 112,
			32, 120, 5, 169, 0, 162, 0, 76, 120, 5, 165, 20, 197, 20, 240, 252,
			173, 36, 2, 201, 137, 208, 174, 173, 37, 2, 201, 11, 208, 167, 173, 146,
			11, 141, 36, 2, 173, 147, 11, 141, 37, 2, 169, 64, 76, 120, 5, 32,
			203, 7, 144, 3, 32, 120, 11, 76, 255, 255, 178, 5, 221, 5, 168, 6,
			59, 6, 123, 6, 148, 6, 159, 6, 82, 6, 147, 8, 153, 8, 157, 8,
			165, 8, 173, 8, 183, 8, 205, 8, 188, 11, 253, 11, 62, 12, 128, 160,
			32, 64, 255, 241, 228, 215, 203, 192, 181, 170, 161, 152, 143, 135, 127, 120,
			114, 107, 101, 95, 90, 85, 80, 75, 71, 67, 63, 60, 56, 53, 50, 47,
			44, 42, 39, 37, 35, 33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18,
			17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2,
			1, 0, 0, 0, 0, 0, 0, 242, 233, 218, 206, 191, 182, 170, 161, 152,
			143, 137, 128, 122, 113, 107, 101, 95, 0, 86, 80, 103, 96, 90, 85, 81,
			76, 72, 67, 63, 61, 57, 52, 51, 57, 45, 42, 40, 37, 36, 33, 31,
			30, 0, 0, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3,
			2, 1, 0, 0, 56, 11, 140, 10, 0, 10, 106, 9, 232, 8, 106, 8,
			239, 7, 128, 7, 8, 7, 174, 6, 70, 6, 230, 5, 149, 5, 65, 5,
			246, 4, 176, 4, 110, 4, 48, 4, 246, 3, 187, 3, 132, 3, 82, 3,
			34, 3, 244, 2, 200, 2, 160, 2, 122, 2, 85, 2, 52, 2, 20, 2,
			245, 1, 216, 1, 189, 1, 164, 1, 141, 1, 119, 1, 96, 1, 78, 1,
			56, 1, 39, 1, 21, 1, 6, 1, 247, 0, 232, 0, 219, 0, 207, 0,
			195, 0, 184, 0, 172, 0, 162, 0, 154, 0, 144, 0, 136, 0, 127, 0,
			120, 0, 112, 0, 106, 0, 100, 0, 94, 0, 87, 0, 82, 0, 50, 0,
			10, 0, 0, 1, 2, 131, 0, 1, 2, 3, 1, 0, 2, 131, 1, 0,
			2, 3, 1, 2, 128, 3, 128, 64, 32, 16, 8, 4, 2, 1, 3, 3,
			3, 3, 7, 11, 15, 19 };
		static readonly byte[] CiBinaryResource_cmc_obx = { 255, 255, 0, 5, 220, 12, 76, 15, 11, 76, 120, 5, 76, 203, 7, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 160, 227, 237, 227, 160, 240, 236, 225,
			249, 229, 242, 160, 246, 160, 178, 174, 177, 160, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255,
			255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 128, 128, 128, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 141, 110,
			5, 142, 111, 5, 140, 112, 5, 41, 112, 74, 74, 74, 170, 189, 145, 11,
			141, 169, 5, 189, 146, 11, 141, 170, 5, 169, 3, 141, 15, 210, 216, 165,
			254, 72, 165, 255, 72, 172, 112, 5, 174, 111, 5, 173, 110, 5, 32, 178,
			5, 104, 133, 255, 104, 133, 254, 96, 173, 118, 5, 133, 254, 173, 119, 5,
			133, 255, 160, 0, 138, 240, 28, 177, 254, 201, 143, 240, 4, 201, 239, 208,
			12, 202, 208, 9, 200, 192, 84, 176, 9, 152, 170, 16, 6, 200, 192, 84,
			144, 229, 96, 142, 104, 5, 32, 123, 6, 169, 0, 162, 9, 157, 69, 5,
			202, 16, 250, 141, 103, 5, 169, 1, 141, 113, 5, 169, 255, 141, 106, 5,
			173, 114, 5, 133, 254, 173, 115, 5, 133, 255, 160, 19, 177, 254, 170, 173,
			118, 5, 133, 254, 173, 119, 5, 133, 255, 172, 104, 5, 177, 254, 201, 207,
			208, 13, 152, 24, 105, 85, 168, 177, 254, 48, 15, 170, 76, 52, 6, 201,
			143, 240, 7, 201, 239, 240, 3, 136, 16, 226, 142, 108, 5, 142, 109, 5,
			96, 41, 15, 240, 245, 142, 218, 10, 142, 240, 10, 142, 255, 10, 140, 219,
			10, 140, 241, 10, 140, 0, 11, 96, 142, 114, 5, 134, 254, 140, 115, 5,
			132, 255, 24, 138, 105, 20, 141, 116, 5, 152, 105, 0, 141, 117, 5, 142,
			118, 5, 200, 200, 140, 119, 5, 160, 19, 177, 254, 141, 108, 5, 141, 109,
			5, 162, 8, 169, 0, 141, 113, 5, 157, 0, 210, 224, 3, 176, 8, 157,
			9, 5, 169, 255, 157, 57, 5, 202, 16, 233, 169, 128, 162, 3, 157, 75,
			5, 202, 16, 250, 96, 169, 1, 141, 113, 5, 169, 0, 240, 238, 41, 3,
			201, 3, 240, 240, 224, 64, 176, 236, 192, 26, 176, 232, 170, 169, 128, 157,
			75, 5, 169, 0, 157, 57, 5, 157, 60, 5, 157, 63, 5, 173, 111, 5,
			157, 12, 5, 173, 112, 5, 10, 10, 10, 133, 254, 24, 173, 114, 5, 105,
			48, 72, 173, 115, 5, 105, 1, 168, 104, 24, 101, 254, 157, 97, 5, 152,
			105, 0, 157, 100, 5, 24, 173, 114, 5, 105, 148, 133, 254, 173, 115, 5,
			105, 0, 133, 255, 173, 112, 5, 10, 109, 112, 5, 10, 168, 177, 254, 157,
			79, 5, 200, 177, 254, 157, 82, 5, 41, 7, 141, 110, 5, 200, 177, 254,
			157, 85, 5, 200, 177, 254, 157, 88, 5, 200, 177, 254, 157, 91, 5, 200,
			177, 254, 157, 94, 5, 160, 0, 173, 110, 5, 201, 3, 208, 2, 160, 2,
			201, 7, 208, 2, 160, 4, 185, 175, 11, 133, 254, 185, 176, 11, 133, 255,
			189, 85, 5, 74, 74, 74, 74, 24, 109, 111, 5, 141, 111, 5, 141, 194,
			7, 168, 173, 110, 5, 201, 7, 208, 15, 152, 10, 168, 177, 254, 157, 45,
			5, 200, 140, 111, 5, 76, 131, 7, 177, 254, 157, 45, 5, 189, 85, 5,
			41, 15, 24, 109, 111, 5, 141, 111, 5, 172, 111, 5, 173, 110, 5, 201,
			5, 8, 177, 254, 40, 240, 8, 221, 45, 5, 208, 3, 56, 233, 1, 157,
			48, 5, 189, 79, 5, 72, 41, 3, 168, 185, 181, 11, 157, 54, 5, 104,
			74, 74, 74, 74, 160, 62, 201, 15, 240, 16, 160, 55, 201, 14, 240, 10,
			160, 48, 201, 13, 240, 4, 24, 105, 0, 168, 185, 185, 11, 157, 51, 5,
			96, 216, 165, 252, 72, 165, 253, 72, 165, 254, 72, 165, 255, 72, 173, 113,
			5, 208, 3, 76, 2, 11, 173, 78, 5, 240, 3, 76, 107, 9, 173, 108,
			5, 205, 109, 5, 240, 3, 76, 88, 9, 173, 103, 5, 240, 3, 76, 220,
			8, 162, 2, 188, 75, 5, 48, 3, 157, 75, 5, 157, 69, 5, 202, 16,
			242, 173, 118, 5, 133, 252, 173, 119, 5, 133, 253, 172, 104, 5, 132, 254,
			204, 106, 5, 208, 25, 173, 107, 5, 240, 20, 173, 104, 5, 172, 105, 5,
			140, 104, 5, 206, 107, 5, 208, 232, 141, 104, 5, 168, 16, 226, 162, 0,
			177, 252, 201, 254, 208, 14, 172, 104, 5, 200, 196, 254, 240, 67, 140, 104,
			5, 76, 26, 8, 157, 66, 5, 24, 152, 105, 85, 168, 232, 224, 3, 144,
			223, 172, 104, 5, 177, 252, 16, 122, 201, 255, 240, 118, 74, 74, 74, 41,
			14, 170, 189, 161, 11, 141, 126, 8, 189, 162, 11, 141, 127, 8, 173, 67,
			5, 133, 255, 32, 147, 8, 140, 104, 5, 192, 85, 176, 4, 196, 254, 208,
			143, 164, 254, 140, 104, 5, 76, 2, 11, 32, 148, 6, 160, 255, 96, 48,
			251, 168, 96, 48, 247, 56, 152, 229, 255, 168, 96, 48, 239, 24, 152, 101,
			255, 168, 96, 48, 231, 141, 108, 5, 141, 109, 5, 200, 96, 48, 221, 173,
			68, 5, 48, 216, 141, 107, 5, 200, 140, 105, 5, 24, 152, 101, 255, 141,
			106, 5, 96, 136, 48, 10, 177, 252, 201, 143, 240, 4, 201, 239, 208, 243,
			200, 96, 162, 2, 189, 72, 5, 240, 5, 222, 72, 5, 16, 99, 189, 75,
			5, 208, 94, 188, 66, 5, 192, 64, 176, 87, 173, 116, 5, 133, 252, 173,
			117, 5, 133, 253, 177, 252, 133, 254, 24, 152, 105, 64, 168, 177, 252, 133,
			255, 37, 254, 201, 255, 240, 58, 188, 69, 5, 177, 254, 41, 192, 208, 12,
			177, 254, 41, 63, 157, 15, 5, 254, 69, 5, 16, 235, 201, 64, 208, 19,
			177, 254, 41, 63, 141, 111, 5, 189, 15, 5, 141, 112, 5, 32, 188, 6,
			76, 72, 9, 201, 128, 208, 10, 177, 254, 41, 63, 157, 72, 5, 254, 69,
			5, 202, 16, 144, 174, 103, 5, 232, 138, 41, 63, 141, 103, 5, 206, 109,
			5, 208, 14, 173, 108, 5, 141, 109, 5, 173, 103, 5, 208, 3, 238, 104,
			5, 172, 48, 5, 173, 82, 5, 41, 7, 201, 5, 240, 4, 201, 6, 208,
			1, 136, 140, 39, 5, 160, 0, 201, 5, 240, 4, 201, 6, 208, 2, 160,
			2, 201, 7, 208, 2, 160, 40, 140, 44, 5, 162, 2, 189, 82, 5, 41,
			224, 157, 40, 5, 189, 97, 5, 133, 252, 189, 100, 5, 133, 253, 189, 57,
			5, 201, 255, 240, 54, 201, 15, 208, 32, 189, 63, 5, 240, 45, 222, 63,
			5, 189, 63, 5, 208, 37, 188, 9, 5, 240, 1, 136, 152, 157, 9, 5,
			189, 88, 5, 157, 63, 5, 76, 229, 9, 189, 57, 5, 74, 168, 177, 252,
			144, 4, 74, 74, 74, 74, 41, 15, 157, 9, 5, 188, 45, 5, 189, 82,
			5, 41, 7, 201, 1, 208, 31, 136, 152, 200, 221, 48, 5, 8, 169, 1,
			40, 208, 2, 10, 10, 61, 60, 5, 240, 12, 188, 48, 5, 192, 255, 208,
			5, 169, 0, 157, 9, 5, 152, 157, 36, 5, 169, 1, 141, 110, 5, 189,
			57, 5, 201, 15, 240, 56, 41, 7, 168, 185, 205, 12, 133, 254, 189, 57,
			5, 41, 8, 8, 138, 40, 24, 240, 2, 105, 3, 168, 185, 91, 5, 37,
			254, 240, 27, 189, 51, 5, 157, 36, 5, 142, 110, 5, 202, 16, 8, 141,
			39, 5, 169, 0, 141, 44, 5, 232, 189, 54, 5, 157, 40, 5, 189, 57,
			5, 41, 15, 201, 15, 240, 16, 254, 57, 5, 189, 57, 5, 201, 15, 208,
			6, 189, 88, 5, 157, 63, 5, 189, 75, 5, 16, 10, 189, 9, 5, 208,
			5, 169, 64, 157, 75, 5, 254, 60, 5, 160, 0, 189, 82, 5, 74, 74,
			74, 74, 144, 1, 136, 74, 144, 1, 200, 24, 152, 125, 45, 5, 157, 45,
			5, 189, 48, 5, 201, 255, 208, 2, 160, 0, 24, 152, 125, 48, 5, 157,
			48, 5, 202, 48, 3, 76, 150, 9, 173, 40, 5, 141, 43, 5, 173, 82,
			5, 41, 7, 170, 160, 3, 173, 110, 5, 240, 3, 188, 213, 12, 152, 72,
			185, 185, 12, 8, 41, 127, 170, 152, 41, 3, 10, 168, 189, 36, 5, 153,
			0, 210, 200, 189, 9, 5, 224, 3, 208, 3, 173, 9, 5, 29, 40, 5,
			40, 16, 2, 169, 0, 153, 0, 210, 104, 168, 136, 41, 3, 208, 207, 160,
			8, 173, 44, 5, 153, 0, 210, 24, 104, 133, 255, 104, 133, 254, 104, 133,
			253, 104, 133, 252, 96, 104, 170, 240, 78, 201, 2, 240, 6, 104, 104, 202,
			208, 251, 96, 165, 20, 197, 20, 240, 252, 173, 36, 2, 201, 134, 208, 7,
			173, 37, 2, 201, 11, 240, 230, 173, 36, 2, 141, 143, 11, 173, 37, 2,
			141, 144, 11, 169, 134, 141, 36, 2, 169, 11, 141, 37, 2, 104, 104, 240,
			3, 56, 233, 1, 141, 93, 11, 104, 168, 104, 170, 169, 112, 32, 120, 5,
			169, 0, 162, 0, 76, 120, 5, 165, 20, 197, 20, 240, 252, 173, 36, 2,
			201, 134, 208, 174, 173, 37, 2, 201, 11, 208, 167, 173, 143, 11, 141, 36,
			2, 173, 144, 11, 141, 37, 2, 169, 64, 76, 120, 5, 32, 203, 7, 144,
			3, 32, 117, 11, 76, 255, 255, 178, 5, 221, 5, 168, 6, 59, 6, 123,
			6, 148, 6, 159, 6, 82, 6, 147, 8, 153, 8, 157, 8, 165, 8, 173,
			8, 183, 8, 205, 8, 185, 11, 250, 11, 59, 12, 128, 160, 32, 64, 255,
			241, 228, 215, 203, 192, 181, 170, 161, 152, 143, 135, 127, 120, 114, 107, 101,
			95, 90, 85, 80, 75, 71, 67, 63, 60, 56, 53, 50, 47, 44, 42, 39,
			37, 35, 33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18, 17, 16, 15,
			14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 0,
			0, 0, 0, 0, 242, 233, 218, 206, 191, 182, 170, 161, 152, 143, 137, 128,
			122, 113, 107, 101, 95, 0, 86, 80, 103, 96, 90, 85, 81, 76, 72, 67,
			63, 61, 57, 52, 51, 57, 45, 42, 40, 37, 36, 33, 31, 30, 0, 0,
			15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
			0, 56, 11, 140, 10, 0, 10, 106, 9, 232, 8, 106, 8, 239, 7, 128,
			7, 8, 7, 174, 6, 70, 6, 230, 5, 149, 5, 65, 5, 246, 4, 176,
			4, 110, 4, 48, 4, 246, 3, 187, 3, 132, 3, 82, 3, 34, 3, 244,
			2, 200, 2, 160, 2, 122, 2, 85, 2, 52, 2, 20, 2, 245, 1, 216,
			1, 189, 1, 164, 1, 141, 1, 119, 1, 96, 1, 78, 1, 56, 1, 39,
			1, 21, 1, 6, 1, 247, 0, 232, 0, 219, 0, 207, 0, 195, 0, 184,
			0, 172, 0, 162, 0, 154, 0, 144, 0, 136, 0, 127, 0, 120, 0, 112,
			0, 106, 0, 100, 0, 94, 0, 87, 0, 82, 0, 50, 0, 10, 0, 0,
			1, 2, 131, 0, 1, 2, 3, 1, 0, 2, 131, 1, 0, 2, 3, 1,
			2, 128, 3, 128, 64, 32, 16, 8, 4, 2, 1, 3, 3, 3, 3, 7,
			11, 15, 19 };
		static readonly byte[] CiBinaryResource_cms_obx = { 255, 255, 0, 5, 186, 15, 234, 234, 234, 76, 21, 8, 76, 92, 15, 35,
			5, 169, 5, 173, 5, 184, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 128, 128, 128, 128, 128, 128, 0, 0, 0, 0, 0, 0, 255,
			255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 130, 0, 0, 6, 6, 0,
			128, 20, 128, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15,
			15, 0, 0, 0, 0, 0, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0,
			1, 2, 131, 0, 1, 2, 3, 1, 0, 2, 131, 1, 0, 2, 3, 1,
			2, 128, 3, 128, 64, 32, 16, 8, 4, 2, 1, 75, 8, 118, 8, 133,
			9, 19, 9, 80, 9, 110, 9, 124, 9, 26, 9, 128, 160, 32, 64, 255,
			241, 228, 215, 203, 192, 181, 170, 161, 152, 143, 135, 127, 120, 114, 107, 101,
			95, 90, 85, 80, 75, 71, 67, 63, 60, 56, 53, 50, 47, 44, 42, 39,
			37, 35, 33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18, 17, 16, 15,
			14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 0,
			0, 0, 0, 0, 242, 233, 218, 206, 191, 182, 170, 161, 152, 143, 137, 128,
			122, 113, 107, 101, 95, 0, 86, 80, 103, 96, 90, 85, 81, 76, 72, 67,
			63, 61, 57, 52, 51, 57, 45, 42, 40, 37, 36, 33, 31, 30, 0, 0,
			15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
			0, 56, 11, 140, 10, 0, 10, 106, 9, 232, 8, 106, 8, 239, 7, 128,
			7, 8, 7, 174, 6, 70, 6, 230, 5, 149, 5, 65, 5, 246, 4, 176,
			4, 110, 4, 48, 4, 246, 3, 187, 3, 132, 3, 82, 3, 34, 3, 244,
			2, 200, 2, 160, 2, 122, 2, 85, 2, 52, 2, 20, 2, 245, 1, 216,
			1, 189, 1, 164, 1, 141, 1, 119, 1, 96, 1, 78, 1, 56, 1, 39,
			1, 21, 1, 6, 1, 247, 0, 232, 0, 219, 0, 207, 0, 195, 0, 184,
			0, 172, 0, 162, 0, 154, 0, 144, 0, 136, 0, 127, 0, 120, 0, 112,
			0, 106, 0, 100, 0, 94, 0, 87, 0, 82, 0, 50, 0, 10, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 0,
			0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 0,
			0, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 0,
			0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 5, 5, 0,
			0, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5, 6, 6, 0,
			0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0,
			1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 0,
			1, 1, 2, 2, 3, 4, 4, 5, 5, 6, 7, 7, 8, 8, 9, 0,
			1, 1, 2, 3, 3, 4, 5, 5, 6, 7, 7, 8, 9, 9, 10, 0,
			1, 1, 2, 3, 4, 4, 5, 6, 7, 7, 8, 9, 10, 10, 11, 0,
			1, 2, 2, 3, 4, 5, 6, 7, 8, 9, 9, 10, 11, 11, 12, 0,
			1, 2, 3, 4, 5, 5, 6, 7, 8, 9, 10, 10, 11, 12, 13, 0,
			1, 2, 3, 4, 5, 6, 7, 7, 8, 9, 10, 11, 12, 13, 14, 0,
			1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 6,
			12, 12, 12, 18, 12, 28, 12, 38, 12, 50, 12, 79, 12, 233, 5, 42,
			6, 107, 6, 161, 11, 196, 11, 185, 11, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 3, 3, 3, 3, 7, 11, 15, 19, 141, 143, 5, 142, 144,
			5, 140, 145, 5, 41, 112, 74, 74, 74, 170, 169, 3, 141, 15, 210, 189,
			213, 5, 141, 73, 8, 189, 214, 5, 141, 74, 8, 169, 3, 141, 31, 210,
			169, 1, 141, 146, 5, 172, 145, 5, 174, 144, 5, 173, 143, 5, 76, 72,
			8, 173, 147, 5, 133, 252, 173, 148, 5, 133, 253, 160, 0, 138, 240, 28,
			177, 252, 201, 143, 240, 4, 201, 239, 208, 12, 202, 208, 9, 200, 192, 84,
			176, 9, 152, 170, 16, 6, 200, 192, 84, 144, 229, 96, 142, 149, 5, 169,
			0, 162, 5, 157, 17, 5, 157, 23, 5, 157, 29, 5, 202, 16, 244, 141,
			150, 5, 141, 157, 5, 160, 255, 140, 159, 5, 173, 153, 5, 133, 252, 173,
			154, 5, 133, 253, 160, 19, 177, 252, 170, 173, 147, 5, 133, 252, 173, 148,
			5, 133, 253, 172, 149, 5, 152, 72, 169, 15, 141, 169, 5, 141, 170, 5,
			177, 252, 201, 135, 208, 35, 152, 72, 24, 105, 85, 168, 177, 252, 16, 2,
			169, 15, 41, 15, 141, 169, 5, 152, 24, 105, 85, 168, 177, 252, 16, 3,
			173, 169, 5, 41, 15, 141, 170, 5, 104, 76, 243, 8, 177, 252, 201, 143,
			240, 7, 201, 239, 240, 3, 136, 16, 199, 104, 168, 177, 252, 201, 207, 208,
			13, 152, 24, 105, 85, 168, 177, 252, 48, 15, 170, 76, 19, 9, 201, 143,
			240, 7, 201, 239, 240, 3, 136, 16, 226, 142, 151, 5, 142, 152, 5, 96,
			142, 153, 5, 134, 252, 140, 154, 5, 132, 253, 24, 138, 105, 20, 141, 155,
			5, 152, 105, 0, 141, 156, 5, 24, 138, 105, 0, 141, 147, 5, 152, 105,
			2, 141, 148, 5, 160, 19, 177, 252, 141, 151, 5, 141, 152, 5, 162, 3,
			142, 31, 210, 142, 15, 210, 169, 0, 141, 146, 5, 160, 8, 169, 0, 153,
			0, 210, 153, 16, 210, 192, 6, 176, 8, 153, 35, 5, 169, 255, 153, 41,
			5, 136, 16, 233, 169, 128, 162, 5, 157, 29, 5, 202, 16, 250, 141, 157,
			5, 96, 169, 0, 240, 240, 141, 157, 5, 240, 11, 173, 143, 5, 41, 7,
			170, 169, 128, 157, 29, 5, 172, 145, 5, 173, 144, 5, 141, 143, 5, 140,
			145, 5, 169, 0, 157, 83, 5, 157, 41, 5, 157, 77, 5, 152, 10, 10,
			10, 133, 254, 24, 173, 153, 5, 105, 48, 72, 173, 154, 5, 105, 1, 168,
			104, 24, 101, 254, 157, 101, 5, 152, 105, 0, 157, 71, 5, 24, 173, 153,
			5, 105, 148, 133, 252, 173, 154, 5, 105, 0, 133, 253, 173, 145, 5, 10,
			109, 145, 5, 10, 168, 140, 145, 5, 200, 200, 200, 200, 200, 177, 252, 157,
			113, 5, 136, 177, 252, 157, 107, 5, 136, 177, 252, 157, 119, 5, 136, 136,
			177, 252, 157, 59, 5, 160, 0, 41, 7, 201, 3, 208, 2, 160, 2, 201,
			7, 208, 2, 160, 4, 185, 247, 7, 133, 254, 185, 248, 7, 133, 255, 172,
			145, 5, 200, 200, 177, 252, 74, 74, 74, 74, 24, 109, 143, 5, 141, 143,
			5, 141, 159, 10, 168, 189, 59, 5, 41, 7, 201, 7, 208, 15, 152, 10,
			168, 177, 254, 157, 125, 5, 200, 140, 143, 5, 76, 92, 10, 177, 254, 157,
			125, 5, 172, 145, 5, 200, 200, 177, 252, 41, 15, 24, 109, 143, 5, 141,
			143, 5, 172, 143, 5, 189, 59, 5, 41, 7, 201, 5, 8, 177, 254, 40,
			240, 8, 221, 125, 5, 208, 3, 56, 233, 1, 157, 89, 5, 172, 145, 5,
			177, 252, 72, 41, 3, 168, 185, 229, 5, 157, 131, 5, 104, 74, 74, 74,
			74, 160, 62, 201, 15, 240, 16, 160, 55, 201, 14, 240, 10, 160, 48, 201,
			13, 240, 4, 24, 105, 50, 168, 185, 233, 5, 157, 137, 5, 96, 216, 165,
			252, 72, 165, 253, 72, 165, 254, 72, 165, 255, 72, 173, 146, 5, 208, 3,
			76, 47, 15, 173, 157, 5, 240, 3, 76, 225, 12, 173, 152, 5, 205, 151,
			5, 176, 3, 76, 206, 12, 173, 150, 5, 240, 3, 76, 158, 11, 162, 5,
			169, 0, 188, 29, 5, 48, 3, 157, 29, 5, 157, 17, 5, 202, 16, 242,
			173, 147, 5, 133, 252, 173, 148, 5, 133, 253, 172, 149, 5, 140, 161, 5,
			204, 159, 5, 208, 25, 173, 160, 5, 240, 20, 173, 149, 5, 172, 158, 5,
			140, 149, 5, 206, 160, 5, 208, 232, 141, 149, 5, 168, 16, 226, 162, 0,
			177, 252, 201, 254, 240, 28, 157, 53, 5, 230, 253, 177, 252, 198, 253, 201,
			254, 240, 15, 157, 56, 5, 24, 152, 105, 85, 168, 232, 224, 3, 144, 224,
			176, 34, 172, 149, 5, 200, 204, 161, 5, 240, 80, 140, 149, 5, 76, 250,
			10, 104, 41, 14, 170, 189, 253, 7, 141, 135, 11, 189, 254, 7, 141, 136,
			11, 76, 129, 11, 172, 149, 5, 177, 252, 16, 57, 201, 255, 240, 53, 74,
			74, 74, 72, 41, 1, 240, 218, 104, 41, 14, 170, 189, 233, 7, 141, 135,
			11, 189, 234, 7, 141, 136, 11, 173, 54, 5, 133, 254, 32, 134, 11, 140,
			149, 5, 192, 85, 176, 5, 204, 161, 5, 208, 179, 172, 161, 5, 140, 149,
			5, 76, 47, 15, 76, 94, 12, 165, 254, 48, 18, 41, 15, 141, 169, 5,
			173, 55, 5, 16, 3, 173, 169, 5, 41, 15, 141, 170, 5, 200, 96, 165,
			254, 48, 250, 41, 1, 141, 184, 5, 200, 96, 173, 179, 5, 48, 20, 206,
			180, 5, 208, 51, 169, 50, 141, 180, 5, 206, 179, 5, 208, 41, 206, 179,
			5, 200, 96, 165, 254, 48, 214, 141, 180, 5, 238, 180, 5, 165, 254, 48,
			204, 141, 180, 5, 238, 180, 5, 173, 55, 5, 141, 179, 5, 16, 5, 169,
			0, 141, 179, 5, 238, 179, 5, 104, 104, 76, 225, 12, 32, 110, 9, 160,
			255, 96, 165, 254, 48, 249, 168, 96, 165, 254, 48, 243, 56, 152, 229, 254,
			168, 96, 165, 254, 48, 233, 24, 152, 101, 254, 168, 96, 165, 254, 48, 223,
			141, 151, 5, 141, 152, 5, 200, 96, 165, 254, 48, 211, 173, 55, 5, 48,
			206, 200, 140, 158, 5, 24, 152, 101, 254, 141, 159, 5, 173, 55, 5, 141,
			160, 5, 192, 84, 96, 136, 48, 10, 177, 252, 201, 143, 240, 4, 201, 239,
			208, 243, 200, 96, 162, 5, 189, 23, 5, 240, 5, 222, 23, 5, 16, 87,
			189, 29, 5, 208, 82, 188, 53, 5, 201, 64, 176, 75, 173, 155, 5, 133,
			252, 173, 156, 5, 133, 253, 177, 252, 133, 254, 24, 152, 105, 64, 168, 177,
			252, 133, 255, 188, 17, 5, 177, 254, 41, 192, 208, 12, 177, 254, 41, 63,
			157, 47, 5, 254, 17, 5, 16, 235, 201, 64, 208, 13, 177, 254, 41, 63,
			188, 47, 5, 32, 150, 9, 76, 190, 12, 201, 128, 208, 10, 177, 254, 41,
			63, 157, 23, 5, 254, 17, 5, 202, 16, 156, 174, 150, 5, 232, 138, 41,
			63, 141, 150, 5, 206, 152, 5, 208, 14, 173, 151, 5, 141, 152, 5, 173,
			150, 5, 208, 3, 238, 149, 5, 172, 89, 5, 173, 59, 5, 41, 7, 201,
			5, 240, 4, 201, 6, 208, 1, 136, 140, 162, 5, 160, 0, 201, 5, 240,
			4, 201, 6, 208, 2, 160, 2, 201, 7, 208, 2, 160, 40, 140, 164, 5,
			172, 92, 5, 173, 62, 5, 41, 7, 201, 5, 240, 4, 201, 6, 208, 1,
			136, 140, 163, 5, 160, 0, 201, 5, 240, 4, 201, 6, 208, 2, 160, 2,
			201, 7, 208, 2, 160, 40, 140, 165, 5, 162, 5, 189, 59, 5, 41, 224,
			157, 65, 5, 189, 101, 5, 133, 252, 189, 71, 5, 133, 253, 189, 41, 5,
			201, 255, 240, 55, 201, 15, 208, 33, 189, 77, 5, 240, 46, 222, 77, 5,
			189, 77, 5, 208, 38, 188, 35, 5, 240, 1, 136, 152, 157, 35, 5, 189,
			119, 5, 157, 77, 5, 136, 76, 133, 13, 189, 41, 5, 74, 168, 177, 252,
			144, 4, 74, 74, 74, 74, 41, 15, 157, 35, 5, 188, 125, 5, 189, 59,
			5, 41, 7, 201, 1, 208, 31, 136, 152, 200, 221, 89, 5, 8, 169, 1,
			40, 208, 2, 10, 10, 61, 83, 5, 240, 12, 188, 89, 5, 192, 255, 208,
			5, 169, 0, 157, 35, 5, 152, 157, 95, 5, 169, 1, 141, 168, 5, 189,
			41, 5, 201, 15, 240, 76, 41, 7, 168, 185, 205, 5, 133, 254, 189, 41,
			5, 41, 8, 8, 138, 40, 24, 240, 2, 105, 6, 168, 185, 107, 5, 37,
			254, 240, 47, 189, 137, 5, 157, 95, 5, 142, 168, 5, 202, 224, 2, 240,
			15, 224, 255, 208, 22, 141, 162, 5, 169, 0, 141, 164, 5, 76, 5, 14,
			173, 140, 5, 141, 163, 5, 169, 0, 141, 165, 5, 232, 189, 131, 5, 157,
			65, 5, 189, 41, 5, 41, 15, 201, 15, 240, 18, 254, 41, 5, 189, 41,
			5, 41, 15, 201, 15, 208, 6, 189, 119, 5, 157, 77, 5, 189, 29, 5,
			16, 10, 189, 35, 5, 208, 5, 169, 64, 157, 29, 5, 254, 83, 5, 160,
			0, 189, 59, 5, 74, 74, 74, 74, 144, 1, 136, 74, 144, 1, 200, 24,
			152, 125, 125, 5, 157, 125, 5, 189, 89, 5, 201, 255, 208, 2, 160, 0,
			24, 152, 125, 89, 5, 157, 89, 5, 202, 48, 3, 76, 53, 13, 32, 123,
			15, 173, 65, 5, 141, 166, 5, 173, 68, 5, 141, 167, 5, 173, 59, 5,
			41, 7, 32, 181, 15, 152, 72, 185, 185, 5, 8, 41, 127, 170, 152, 41,
			3, 10, 168, 224, 3, 208, 3, 76, 196, 14, 189, 173, 5, 208, 39, 189,
			95, 5, 153, 0, 210, 189, 35, 5, 29, 65, 5, 40, 16, 2, 169, 0,
			153, 1, 210, 104, 168, 136, 41, 3, 240, 3, 76, 127, 14, 173, 164, 5,
			141, 8, 210, 76, 228, 14, 40, 76, 173, 14, 173, 173, 5, 208, 23, 173,
			162, 5, 153, 0, 210, 173, 35, 5, 13, 166, 5, 40, 16, 2, 169, 0,
			153, 1, 210, 76, 173, 14, 40, 76, 173, 14, 173, 62, 5, 41, 7, 32,
			181, 15, 152, 72, 185, 185, 5, 8, 41, 127, 170, 152, 41, 3, 10, 168,
			224, 3, 208, 3, 76, 60, 15, 189, 176, 5, 208, 30, 189, 98, 5, 153,
			16, 210, 189, 38, 5, 29, 68, 5, 40, 16, 2, 169, 0, 153, 17, 210,
			104, 168, 136, 41, 3, 240, 7, 76, 236, 14, 40, 76, 26, 15, 173, 165,
			5, 141, 24, 210, 24, 104, 133, 255, 104, 133, 254, 104, 133, 253, 104, 133,
			252, 96, 173, 176, 5, 208, 23, 173, 163, 5, 153, 16, 210, 173, 38, 5,
			13, 167, 5, 40, 16, 2, 169, 0, 153, 17, 210, 76, 26, 15, 40, 76,
			26, 15, 32, 168, 10, 176, 25, 173, 184, 5, 240, 20, 173, 157, 5, 141,
			183, 5, 169, 1, 141, 157, 5, 32, 168, 10, 173, 183, 5, 141, 157, 5,
			96, 173, 169, 5, 10, 10, 10, 10, 141, 171, 5, 173, 170, 5, 10, 10,
			10, 10, 141, 172, 5, 162, 2, 134, 200, 173, 171, 5, 29, 35, 5, 170,
			189, 233, 6, 166, 200, 157, 35, 5, 173, 172, 5, 29, 38, 5, 170, 189,
			233, 6, 166, 200, 157, 38, 5, 202, 16, 221, 96, 168, 185, 13, 8, 168,
			96 };
		static readonly byte[] CiBinaryResource_dlt_obx = { 255, 255, 0, 4, 70, 12, 255, 241, 228, 215, 203, 192, 181, 170, 161, 152,
			143, 135, 127, 121, 114, 107, 101, 95, 90, 85, 80, 75, 71, 67, 63, 60,
			56, 53, 50, 47, 44, 42, 39, 37, 35, 33, 31, 29, 28, 26, 24, 23,
			22, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 255, 241, 228, 215, 242, 233, 218, 206, 191, 182, 170, 161, 152, 143,
			137, 128, 122, 113, 107, 101, 95, 92, 86, 80, 103, 96, 90, 85, 81, 76,
			72, 67, 63, 61, 57, 52, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30,
			28, 27, 25, 0, 22, 21, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2,
			1, 0, 242, 233, 218, 206, 242, 233, 218, 206, 191, 182, 170, 161, 152, 143,
			137, 128, 122, 113, 107, 101, 95, 92, 86, 80, 103, 96, 90, 85, 81, 76,
			72, 67, 63, 61, 57, 52, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30,
			28, 27, 25, 0, 22, 21, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2,
			1, 0, 242, 233, 218, 206, 255, 241, 228, 216, 202, 192, 181, 171, 162, 153,
			142, 135, 127, 120, 115, 108, 102, 97, 90, 85, 81, 75, 72, 67, 63, 60,
			57, 52, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30, 28, 27, 25, 23,
			22, 21, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 3, 2, 1, 255, 76, 9, 5, 76, 200, 5, 76, 183, 5, 136,
			140, 54, 3, 169, 126, 141, 53, 3, 162, 6, 142, 51, 3, 162, 1, 142,
			52, 3, 32, 51, 5, 32, 95, 5, 32, 163, 5, 32, 139, 5, 169, 1,
			141, 50, 3, 169, 3, 141, 15, 210, 96, 162, 0, 160, 32, 142, 48, 3,
			140, 49, 3, 160, 0, 173, 48, 3, 153, 0, 2, 173, 49, 3, 153, 64,
			2, 173, 48, 3, 24, 105, 128, 141, 48, 3, 144, 3, 238, 49, 3, 200,
			192, 64, 208, 225, 96, 162, 0, 160, 68, 142, 48, 3, 140, 49, 3, 160,
			0, 173, 48, 3, 153, 128, 2, 173, 49, 3, 153, 160, 2, 173, 48, 3,
			24, 105, 64, 141, 48, 3, 144, 3, 238, 49, 3, 200, 192, 32, 208, 225,
			96, 173, 0, 76, 41, 1, 74, 106, 106, 168, 162, 0, 185, 128, 4, 157,
			64, 4, 200, 232, 224, 64, 208, 244, 96, 160, 3, 169, 0, 153, 40, 3,
			153, 32, 3, 153, 36, 3, 153, 44, 3, 136, 16, 241, 96, 169, 0, 141,
			50, 3, 160, 7, 169, 0, 153, 0, 210, 136, 16, 250, 96, 96, 173, 50,
			3, 240, 250, 173, 40, 3, 13, 41, 3, 13, 42, 3, 13, 43, 3, 141,
			8, 210, 174, 36, 3, 172, 32, 3, 142, 0, 210, 140, 1, 210, 174, 37,
			3, 172, 33, 3, 142, 2, 210, 140, 3, 210, 174, 38, 3, 172, 34, 3,
			142, 4, 210, 140, 5, 210, 174, 39, 3, 172, 35, 3, 142, 6, 210, 140,
			7, 210, 206, 52, 3, 208, 74, 173, 51, 3, 141, 52, 3, 238, 53, 3,
			238, 53, 3, 16, 28, 238, 54, 3, 169, 0, 141, 53, 3, 32, 199, 6,
			173, 4, 3, 13, 5, 3, 13, 6, 3, 13, 7, 3, 208, 3, 76, 183,
			5, 173, 4, 3, 240, 3, 32, 97, 7, 173, 5, 3, 240, 3, 32, 192,
			7, 173, 6, 3, 240, 3, 32, 31, 8, 173, 7, 3, 240, 3, 32, 126,
			8, 173, 4, 3, 240, 8, 173, 44, 3, 240, 3, 32, 221, 8, 173, 5,
			3, 240, 8, 173, 45, 3, 240, 3, 32, 206, 9, 173, 6, 3, 240, 8,
			173, 46, 3, 240, 3, 32, 191, 10, 173, 7, 3, 240, 8, 173, 47, 3,
			240, 3, 32, 131, 11, 96, 192, 67, 144, 14, 169, 0, 141, 4, 3, 141,
			32, 3, 141, 40, 3, 76, 230, 6, 192, 66, 208, 15, 189, 128, 64, 141,
			51, 3, 141, 52, 3, 238, 54, 3, 76, 199, 6, 192, 65, 208, 9, 189,
			128, 64, 141, 54, 3, 76, 199, 6, 104, 104, 76, 183, 5, 174, 54, 3,
			188, 0, 64, 192, 64, 176, 191, 189, 128, 64, 141, 24, 3, 185, 0, 2,
			133, 224, 185, 64, 2, 133, 225, 169, 1, 141, 4, 3, 188, 0, 65, 192,
			64, 176, 78, 189, 128, 65, 141, 25, 3, 185, 0, 2, 133, 226, 185, 64,
			2, 133, 227, 169, 1, 141, 5, 3, 188, 0, 66, 192, 64, 176, 63, 189,
			128, 66, 141, 26, 3, 185, 0, 2, 133, 228, 185, 64, 2, 133, 229, 169,
			1, 141, 6, 3, 188, 0, 67, 192, 64, 176, 48, 189, 128, 67, 141, 27,
			3, 185, 0, 2, 133, 230, 185, 64, 2, 133, 231, 169, 1, 141, 7, 3,
			96, 169, 0, 141, 5, 3, 141, 33, 3, 141, 41, 3, 240, 186, 169, 0,
			141, 6, 3, 141, 34, 3, 141, 42, 3, 240, 201, 169, 0, 141, 7, 3,
			141, 35, 3, 141, 43, 3, 96, 172, 53, 3, 177, 224, 48, 11, 200, 177,
			224, 48, 1, 96, 104, 104, 76, 31, 6, 24, 109, 24, 3, 41, 127, 141,
			8, 3, 169, 15, 141, 0, 3, 141, 44, 3, 200, 177, 224, 170, 189, 160,
			2, 133, 233, 133, 241, 133, 249, 189, 128, 2, 133, 232, 73, 16, 133, 240,
			73, 48, 133, 248, 160, 49, 177, 232, 141, 12, 3, 160, 51, 177, 232, 41,
			127, 141, 16, 3, 169, 0, 141, 20, 3, 141, 28, 3, 160, 48, 177, 232,
			41, 213, 141, 40, 3, 96, 172, 53, 3, 177, 226, 48, 11, 200, 177, 226,
			48, 1, 96, 104, 104, 76, 31, 6, 24, 109, 25, 3, 41, 127, 141, 9,
			3, 169, 15, 141, 1, 3, 141, 45, 3, 200, 177, 226, 170, 189, 160, 2,
			133, 235, 133, 243, 133, 251, 189, 128, 2, 133, 234, 73, 16, 133, 242, 73,
			48, 133, 250, 160, 49, 177, 234, 141, 13, 3, 160, 51, 177, 234, 41, 127,
			141, 17, 3, 169, 0, 141, 21, 3, 141, 29, 3, 160, 48, 177, 234, 41,
			131, 141, 41, 3, 96, 172, 53, 3, 177, 228, 48, 11, 200, 177, 228, 48,
			1, 96, 104, 104, 76, 31, 6, 24, 109, 26, 3, 41, 127, 141, 10, 3,
			169, 15, 141, 2, 3, 141, 46, 3, 200, 177, 228, 170, 189, 160, 2, 133,
			237, 133, 245, 133, 253, 189, 128, 2, 133, 236, 73, 16, 133, 244, 73, 48,
			133, 252, 160, 49, 177, 236, 141, 14, 3, 160, 51, 177, 236, 41, 127, 141,
			18, 3, 169, 0, 141, 22, 3, 141, 30, 3, 160, 48, 177, 236, 41, 169,
			141, 42, 3, 96, 172, 53, 3, 177, 230, 48, 11, 200, 177, 230, 48, 1,
			96, 104, 104, 76, 31, 6, 24, 109, 27, 3, 41, 127, 141, 11, 3, 169,
			15, 141, 3, 3, 141, 47, 3, 200, 177, 230, 170, 189, 160, 2, 133, 239,
			133, 247, 133, 255, 189, 128, 2, 133, 238, 73, 16, 133, 246, 73, 48, 133,
			254, 160, 49, 177, 238, 141, 15, 3, 160, 51, 177, 238, 41, 127, 141, 19,
			3, 169, 0, 141, 23, 3, 141, 31, 3, 160, 48, 177, 238, 41, 129, 141,
			43, 3, 96, 172, 0, 3, 48, 70, 177, 232, 141, 32, 3, 177, 240, 208,
			9, 32, 108, 9, 206, 0, 3, 76, 79, 9, 201, 1, 240, 39, 201, 3,
			208, 16, 173, 8, 3, 24, 113, 248, 170, 173, 28, 3, 141, 55, 3, 76,
			24, 9, 173, 28, 3, 24, 113, 248, 141, 55, 3, 174, 8, 3, 32, 150,
			9, 206, 0, 3, 96, 177, 248, 141, 36, 3, 206, 0, 3, 96, 32, 108,
			9, 160, 49, 177, 232, 240, 30, 206, 12, 3, 240, 3, 76, 79, 9, 173,
			32, 3, 41, 15, 240, 11, 206, 32, 3, 177, 232, 141, 12, 3, 76, 79,
			9, 141, 44, 3, 96, 173, 28, 3, 24, 160, 50, 113, 232, 141, 28, 3,
			206, 16, 3, 208, 12, 238, 20, 3, 160, 51, 177, 232, 41, 127, 141, 16,
			3, 96, 173, 20, 3, 41, 3, 24, 105, 52, 168, 177, 232, 170, 160, 51,
			177, 232, 48, 14, 138, 109, 8, 3, 170, 173, 28, 3, 141, 55, 3, 76,
			150, 9, 138, 109, 28, 3, 141, 55, 3, 174, 8, 3, 189, 0, 4, 24,
			109, 55, 3, 141, 36, 3, 173, 40, 3, 41, 4, 208, 1, 96, 172, 0,
			3, 177, 240, 208, 21, 138, 24, 160, 0, 113, 248, 170, 189, 0, 4, 24,
			109, 55, 3, 24, 105, 255, 141, 38, 3, 96, 173, 36, 3, 24, 105, 255,
			141, 38, 3, 96, 172, 1, 3, 48, 70, 177, 234, 141, 33, 3, 177, 242,
			208, 9, 32, 93, 10, 206, 1, 3, 76, 64, 10, 201, 1, 240, 39, 201,
			3, 208, 16, 173, 9, 3, 24, 113, 250, 170, 173, 29, 3, 141, 55, 3,
			76, 9, 10, 173, 29, 3, 24, 113, 250, 141, 55, 3, 174, 9, 3, 32,
			135, 10, 206, 1, 3, 96, 177, 250, 141, 37, 3, 206, 1, 3, 96, 32,
			93, 10, 160, 49, 177, 234, 240, 30, 206, 13, 3, 240, 3, 76, 64, 10,
			173, 33, 3, 41, 15, 240, 11, 206, 33, 3, 177, 234, 141, 13, 3, 76,
			64, 10, 141, 45, 3, 96, 173, 29, 3, 24, 160, 50, 113, 234, 141, 29,
			3, 206, 17, 3, 208, 12, 238, 21, 3, 160, 51, 177, 234, 41, 127, 141,
			17, 3, 96, 173, 21, 3, 41, 3, 24, 105, 52, 168, 177, 234, 170, 160,
			51, 177, 234, 48, 14, 138, 109, 9, 3, 170, 173, 29, 3, 141, 55, 3,
			76, 135, 10, 138, 109, 29, 3, 141, 55, 3, 174, 9, 3, 189, 0, 4,
			24, 109, 55, 3, 141, 37, 3, 173, 41, 3, 41, 2, 208, 1, 96, 172,
			1, 3, 177, 242, 208, 21, 138, 24, 160, 0, 113, 250, 170, 189, 0, 4,
			24, 109, 55, 3, 24, 105, 255, 141, 39, 3, 96, 173, 37, 3, 24, 105,
			255, 141, 39, 3, 96, 172, 2, 3, 48, 70, 177, 236, 141, 34, 3, 177,
			244, 208, 9, 32, 78, 11, 206, 2, 3, 76, 49, 11, 201, 1, 240, 39,
			201, 3, 208, 16, 173, 10, 3, 24, 113, 252, 170, 173, 30, 3, 141, 55,
			3, 76, 250, 10, 173, 30, 3, 24, 113, 252, 141, 55, 3, 174, 10, 3,
			32, 120, 11, 206, 2, 3, 96, 177, 252, 141, 38, 3, 206, 2, 3, 96,
			32, 78, 11, 160, 49, 177, 236, 240, 30, 206, 14, 3, 240, 3, 76, 49,
			11, 173, 34, 3, 41, 15, 240, 11, 206, 34, 3, 177, 236, 141, 14, 3,
			76, 49, 11, 141, 46, 3, 96, 173, 30, 3, 24, 160, 50, 113, 236, 141,
			30, 3, 206, 18, 3, 208, 12, 238, 22, 3, 160, 51, 177, 236, 41, 127,
			141, 18, 3, 96, 173, 22, 3, 41, 3, 24, 105, 52, 168, 177, 236, 170,
			160, 51, 177, 236, 48, 14, 138, 109, 10, 3, 170, 173, 30, 3, 141, 55,
			3, 76, 120, 11, 138, 109, 30, 3, 141, 55, 3, 174, 10, 3, 189, 0,
			4, 24, 109, 55, 3, 141, 38, 3, 96, 172, 3, 3, 48, 70, 177, 238,
			141, 35, 3, 177, 246, 208, 9, 32, 18, 12, 206, 3, 3, 76, 245, 11,
			201, 1, 240, 39, 201, 3, 208, 16, 173, 11, 3, 24, 113, 254, 170, 173,
			31, 3, 141, 55, 3, 76, 190, 11, 173, 31, 3, 24, 113, 254, 141, 55,
			3, 174, 11, 3, 32, 60, 12, 206, 3, 3, 96, 177, 254, 141, 39, 3,
			206, 3, 3, 96, 32, 18, 12, 160, 49, 177, 238, 240, 30, 206, 15, 3,
			240, 3, 76, 245, 11, 173, 35, 3, 41, 15, 240, 11, 206, 35, 3, 177,
			238, 141, 15, 3, 76, 245, 11, 141, 47, 3, 96, 173, 31, 3, 24, 160,
			50, 113, 238, 141, 31, 3, 206, 19, 3, 208, 12, 238, 23, 3, 160, 51,
			177, 238, 41, 127, 141, 19, 3, 96, 173, 23, 3, 41, 3, 24, 105, 52,
			168, 177, 238, 170, 160, 51, 177, 238, 48, 14, 138, 109, 11, 3, 170, 173,
			31, 3, 141, 55, 3, 76, 60, 12, 138, 109, 31, 3, 141, 55, 3, 174,
			11, 3, 189, 0, 4, 24, 109, 55, 3, 141, 39, 3, 96 };
		static readonly byte[] CiBinaryResource_mpt_obx = { 255, 255, 0, 5, 178, 13, 76, 205, 11, 173, 46, 7, 208, 1, 96, 169,
			0, 141, 28, 14, 238, 29, 14, 173, 23, 14, 205, 187, 13, 144, 80, 206,
			21, 14, 240, 3, 76, 197, 5, 162, 0, 142, 23, 14, 169, 0, 157, 237,
			13, 157, 245, 13, 189, 179, 13, 133, 236, 189, 183, 13, 133, 237, 172, 22,
			14, 177, 236, 200, 201, 255, 240, 7, 201, 254, 208, 15, 76, 42, 12, 177,
			236, 48, 249, 10, 168, 140, 22, 14, 76, 59, 5, 157, 233, 13, 177, 236,
			157, 213, 13, 232, 224, 4, 208, 196, 200, 140, 22, 14, 76, 197, 5, 206,
			21, 14, 16, 87, 173, 188, 13, 141, 21, 14, 162, 3, 222, 245, 13, 16,
			68, 189, 233, 13, 10, 168, 185, 255, 255, 133, 236, 200, 185, 255, 255, 133,
			237, 5, 236, 240, 48, 189, 237, 13, 141, 31, 14, 32, 62, 7, 172, 31,
			14, 200, 152, 157, 237, 13, 189, 241, 13, 157, 245, 13, 224, 2, 208, 21,
			189, 197, 13, 73, 15, 10, 10, 10, 10, 105, 69, 141, 161, 13, 169, 10,
			105, 0, 141, 162, 13, 202, 16, 180, 238, 23, 14, 162, 1, 173, 27, 14,
			201, 2, 240, 2, 162, 3, 173, 27, 14, 201, 2, 208, 5, 236, 25, 14,
			240, 3, 76, 118, 6, 181, 240, 61, 114, 6, 240, 18, 160, 40, 177, 236,
			24, 125, 225, 13, 32, 117, 9, 56, 125, 1, 14, 157, 203, 13, 202, 16,
			213, 169, 3, 141, 15, 210, 165, 241, 41, 16, 240, 15, 172, 226, 13, 185,
			198, 9, 141, 201, 13, 185, 5, 10, 141, 202, 13, 173, 201, 13, 141, 0,
			210, 173, 202, 13, 141, 2, 210, 173, 203, 13, 141, 4, 210, 173, 204, 13,
			141, 6, 210, 173, 193, 13, 162, 255, 172, 27, 14, 192, 1, 208, 5, 174,
			25, 14, 240, 3, 141, 1, 210, 173, 194, 13, 224, 1, 240, 3, 141, 3,
			210, 192, 2, 240, 20, 173, 195, 13, 224, 2, 240, 3, 141, 5, 210, 173,
			196, 13, 224, 3, 240, 3, 141, 7, 210, 165, 240, 5, 241, 5, 242, 5,
			243, 13, 28, 14, 141, 8, 210, 96, 4, 2, 0, 0, 189, 217, 13, 133,
			236, 189, 221, 13, 133, 237, 5, 236, 208, 8, 157, 193, 13, 149, 240, 76,
			248, 5, 180, 244, 192, 32, 240, 66, 177, 236, 56, 253, 197, 13, 44, 58,
			7, 240, 2, 41, 240, 157, 193, 13, 200, 177, 236, 141, 30, 14, 200, 148,
			244, 41, 7, 240, 60, 168, 185, 126, 9, 141, 203, 6, 185, 133, 9, 141,
			204, 6, 173, 30, 14, 74, 74, 74, 74, 74, 9, 40, 168, 177, 236, 24,
			32, 255, 255, 169, 0, 149, 240, 76, 248, 5, 189, 9, 14, 240, 18, 222,
			13, 14, 208, 13, 157, 13, 14, 189, 193, 13, 41, 15, 240, 3, 222, 193,
			13, 160, 35, 177, 236, 149, 240, 189, 17, 14, 24, 105, 37, 168, 41, 3,
			157, 17, 14, 136, 177, 236, 125, 209, 13, 157, 225, 13, 32, 119, 9, 157,
			201, 13, 189, 5, 14, 240, 6, 222, 5, 14, 76, 223, 5, 189, 189, 13,
			141, 30, 7, 16, 254, 76, 194, 8, 0, 76, 229, 8, 0, 76, 251, 8,
			0, 76, 21, 9, 0, 76, 37, 9, 0, 76, 56, 9, 0, 76, 66, 9,
			16, 76, 72, 9, 169, 0, 157, 197, 13, 172, 31, 14, 136, 200, 177, 236,
			201, 254, 208, 4, 140, 31, 14, 96, 201, 224, 144, 8, 173, 187, 13, 141,
			23, 14, 208, 233, 201, 208, 144, 10, 41, 15, 141, 188, 13, 141, 21, 14,
			16, 219, 201, 192, 144, 9, 41, 15, 73, 15, 157, 197, 13, 16, 206, 201,
			128, 144, 7, 41, 63, 157, 241, 13, 16, 195, 201, 64, 144, 27, 200, 140,
			31, 14, 41, 31, 157, 229, 13, 10, 168, 185, 255, 255, 157, 217, 13, 200,
			185, 255, 255, 157, 221, 13, 76, 62, 7, 140, 31, 14, 141, 30, 14, 24,
			125, 213, 13, 157, 209, 13, 173, 27, 14, 240, 66, 201, 2, 240, 58, 189,
			229, 13, 201, 31, 208, 55, 173, 30, 14, 56, 233, 1, 41, 15, 168, 177,
			254, 133, 253, 152, 9, 16, 168, 177, 254, 133, 248, 160, 1, 5, 253, 208,
			2, 160, 0, 140, 26, 14, 169, 0, 133, 252, 157, 217, 13, 157, 221, 13,
			138, 10, 141, 24, 14, 142, 25, 14, 96, 224, 2, 176, 99, 189, 217, 13,
			133, 238, 189, 221, 13, 133, 239, 5, 238, 240, 74, 160, 32, 177, 238, 41,
			15, 157, 249, 13, 177, 238, 41, 112, 74, 74, 157, 189, 13, 200, 177, 238,
			10, 10, 72, 41, 63, 157, 5, 14, 104, 41, 192, 157, 205, 13, 200, 177,
			238, 157, 9, 14, 157, 13, 14, 169, 0, 149, 244, 157, 17, 14, 157, 253,
			13, 157, 1, 14, 189, 209, 13, 157, 225, 13, 32, 117, 9, 157, 201, 13,
			236, 25, 14, 240, 1, 96, 160, 255, 140, 25, 14, 200, 140, 26, 14, 96,
			224, 2, 208, 51, 172, 211, 13, 185, 69, 11, 141, 121, 13, 185, 129, 11,
			141, 127, 13, 169, 0, 133, 249, 133, 250, 173, 231, 13, 41, 15, 168, 177,
			254, 133, 251, 152, 9, 16, 168, 177, 254, 141, 137, 13, 5, 251, 208, 6,
			141, 121, 13, 141, 127, 13, 96, 173, 232, 13, 41, 15, 168, 177, 254, 133,
			253, 152, 9, 16, 168, 177, 254, 5, 253, 240, 15, 177, 254, 56, 229, 253,
			133, 248, 169, 0, 133, 252, 169, 141, 208, 2, 169, 173, 141, 97, 13, 141,
			56, 13, 169, 24, 141, 7, 210, 96, 173, 29, 14, 41, 7, 74, 74, 144,
			18, 208, 24, 189, 249, 13, 24, 157, 1, 14, 125, 201, 13, 157, 201, 13,
			76, 223, 5, 169, 0, 157, 1, 14, 76, 223, 5, 189, 201, 13, 56, 253,
			249, 13, 157, 201, 13, 56, 169, 0, 253, 249, 13, 157, 1, 14, 76, 223,
			5, 189, 253, 13, 24, 157, 1, 14, 125, 201, 13, 157, 201, 13, 24, 189,
			253, 13, 125, 249, 13, 157, 253, 13, 76, 223, 5, 189, 225, 13, 56, 253,
			253, 13, 157, 225, 13, 32, 117, 9, 76, 5, 9, 169, 0, 56, 253, 253,
			13, 157, 1, 14, 189, 201, 13, 56, 253, 253, 13, 76, 5, 9, 189, 225,
			13, 24, 125, 253, 13, 76, 28, 9, 32, 85, 9, 76, 208, 8, 32, 85,
			9, 24, 125, 225, 13, 32, 155, 9, 76, 223, 5, 188, 253, 13, 189, 249,
			13, 48, 2, 200, 200, 136, 152, 157, 253, 13, 221, 249, 13, 208, 8, 189,
			249, 13, 73, 255, 157, 249, 13, 189, 253, 13, 96, 41, 63, 29, 205, 13,
			168, 185, 255, 255, 96, 148, 145, 152, 165, 173, 180, 192, 9, 9, 9, 9,
			9, 9, 9, 64, 0, 32, 0, 125, 201, 13, 157, 201, 13, 96, 125, 209,
			13, 157, 225, 13, 32, 117, 9, 157, 201, 13, 96, 157, 201, 13, 189, 141,
			9, 16, 12, 157, 201, 13, 169, 128, 208, 5, 157, 201, 13, 169, 1, 13,
			28, 14, 141, 28, 14, 96, 45, 10, 210, 157, 201, 13, 96, 242, 51, 150,
			226, 56, 140, 0, 106, 232, 106, 239, 128, 8, 174, 70, 230, 149, 65, 246,
			176, 110, 48, 246, 187, 132, 82, 34, 244, 200, 160, 122, 85, 52, 20, 245,
			216, 189, 164, 141, 119, 96, 78, 56, 39, 21, 6, 247, 232, 219, 207, 195,
			184, 172, 162, 154, 144, 136, 127, 120, 112, 106, 100, 94, 13, 13, 12, 11,
			11, 10, 10, 9, 8, 8, 7, 7, 7, 6, 6, 5, 5, 5, 4, 4,
			4, 4, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2, 2, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 22, 22, 23, 23, 23,
			23, 24, 24, 24, 24, 24, 25, 25, 25, 25, 26, 21, 21, 22, 22, 22,
			23, 23, 24, 24, 24, 25, 25, 26, 26, 26, 27, 20, 21, 21, 22, 22,
			23, 23, 24, 24, 24, 25, 25, 26, 26, 27, 27, 20, 20, 21, 21, 22,
			22, 23, 23, 24, 25, 25, 26, 26, 27, 27, 28, 19, 20, 20, 21, 22,
			22, 23, 23, 24, 25, 25, 26, 26, 27, 28, 28, 19, 19, 20, 21, 21,
			22, 23, 23, 24, 25, 25, 26, 27, 27, 28, 29, 18, 19, 20, 20, 21,
			22, 23, 23, 24, 25, 25, 26, 27, 28, 28, 29, 18, 19, 19, 20, 21,
			22, 22, 23, 24, 25, 26, 26, 27, 28, 29, 29, 18, 18, 19, 20, 21,
			22, 22, 23, 24, 25, 26, 26, 27, 28, 29, 30, 17, 18, 19, 20, 21,
			22, 22, 23, 24, 25, 26, 26, 27, 28, 29, 30, 17, 18, 19, 20, 21,
			21, 22, 23, 24, 25, 26, 27, 27, 28, 29, 30, 17, 18, 19, 20, 20,
			21, 22, 23, 24, 25, 26, 27, 28, 28, 29, 30, 17, 18, 19, 19, 20,
			21, 22, 23, 24, 25, 26, 27, 28, 29, 29, 30, 17, 18, 18, 19, 20,
			21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 30, 16, 17, 18, 19, 20,
			21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 34, 36, 38, 41,
			43, 46, 48, 51, 55, 58, 61, 65, 69, 73, 77, 82, 87, 92, 97, 103,
			110, 116, 123, 130, 138, 146, 155, 164, 174, 184, 195, 207, 220, 233, 246, 5,
			21, 37, 55, 73, 93, 113, 135, 159, 184, 210, 237, 11, 42, 75, 110, 147,
			186, 227, 15, 62, 112, 164, 219, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3,
			3, 3, 3, 229, 42, 64, 89, 100, 238, 8, 166, 11, 12, 12, 12, 12,
			12, 13, 13, 142, 50, 7, 140, 54, 7, 41, 7, 168, 185, 189, 11, 141,
			227, 11, 185, 197, 11, 141, 228, 11, 76, 255, 255, 173, 54, 7, 174, 50,
			7, 141, 148, 7, 141, 155, 7, 142, 149, 7, 142, 156, 7, 24, 105, 64,
			141, 129, 5, 141, 135, 5, 144, 1, 232, 142, 130, 5, 142, 136, 5, 24,
			105, 128, 141, 124, 9, 144, 1, 232, 142, 125, 9, 232, 141, 31, 12, 142,
			32, 12, 162, 9, 189, 255, 255, 157, 179, 13, 202, 16, 247, 206, 188, 13,
			169, 0, 141, 46, 7, 162, 98, 157, 189, 13, 202, 16, 250, 162, 8, 157,
			0, 210, 202, 16, 250, 96, 32, 42, 12, 173, 50, 7, 10, 141, 22, 14,
			173, 187, 13, 141, 23, 14, 169, 1, 141, 21, 14, 141, 46, 7, 96, 173,
			54, 7, 133, 254, 173, 50, 7, 133, 255, 96, 173, 54, 7, 41, 3, 170,
			173, 50, 7, 32, 198, 7, 173, 26, 14, 240, 238, 14, 54, 7, 32, 190,
			12, 169, 1, 141, 27, 14, 173, 26, 14, 240, 222, 201, 1, 208, 5, 160,
			0, 238, 26, 14, 177, 252, 174, 24, 14, 74, 74, 74, 74, 9, 16, 141,
			10, 212, 141, 10, 212, 157, 1, 210, 177, 252, 9, 16, 141, 10, 212, 141,
			10, 212, 157, 1, 210, 200, 208, 206, 230, 253, 165, 253, 197, 248, 208, 198,
			140, 26, 14, 96, 144, 21, 169, 234, 141, 153, 12, 141, 154, 12, 141, 155,
			12, 141, 166, 12, 141, 167, 12, 141, 168, 12, 96, 169, 141, 141, 153, 12,
			141, 166, 12, 169, 10, 141, 154, 12, 141, 167, 12, 169, 212, 141, 155, 12,
			141, 168, 12, 96, 169, 0, 141, 26, 14, 173, 50, 7, 74, 32, 190, 12,
			169, 1, 141, 27, 14, 32, 128, 12, 173, 27, 14, 208, 248, 96, 169, 2,
			141, 27, 14, 141, 25, 14, 169, 24, 141, 7, 210, 169, 17, 133, 250, 169,
			13, 133, 251, 169, 173, 141, 97, 13, 141, 56, 13, 160, 0, 140, 121, 13,
			140, 127, 13, 174, 11, 212, 177, 252, 74, 74, 74, 74, 9, 16, 141, 7,
			210, 32, 117, 13, 236, 11, 212, 240, 251, 141, 5, 210, 174, 11, 212, 177,
			252, 230, 252, 208, 16, 230, 253, 198, 248, 208, 10, 169, 173, 141, 97, 13,
			141, 56, 13, 169, 8, 9, 16, 141, 7, 210, 32, 117, 13, 236, 11, 212,
			240, 251, 141, 5, 210, 173, 27, 14, 208, 185, 96, 24, 165, 249, 105, 0,
			133, 249, 165, 250, 105, 0, 133, 250, 144, 15, 230, 251, 165, 251, 201, 0,
			208, 7, 140, 121, 13, 140, 127, 13, 96, 177, 250, 36, 249, 48, 4, 74,
			74, 74, 74, 41, 15, 168, 185, 69, 10, 160, 0, 96, 160, 0, 140, 27,
			14, 140, 26, 14, 136, 140, 25, 14, 96 };
		static readonly byte[] CiBinaryResource_rmt4_obx = { 255, 255, 144, 3, 96, 11, 128, 0, 128, 32, 128, 64, 0, 192, 128, 128,
			128, 160, 0, 192, 64, 192, 0, 1, 5, 11, 21, 0, 1, 255, 255, 1,
			1, 0, 255, 255, 0, 1, 1, 1, 0, 255, 255, 255, 255, 0, 1, 1,
			0, 0, 0, 0, 0, 0, 242, 51, 150, 226, 56, 140, 0, 106, 232, 106,
			239, 128, 8, 174, 70, 230, 149, 65, 246, 176, 110, 48, 246, 187, 132, 82,
			34, 244, 200, 160, 122, 85, 52, 20, 245, 216, 189, 164, 141, 119, 96, 78,
			56, 39, 21, 6, 247, 232, 219, 207, 195, 184, 172, 162, 154, 144, 136, 127,
			120, 112, 106, 100, 94, 0, 191, 182, 170, 161, 152, 143, 137, 128, 242, 230,
			218, 206, 191, 182, 170, 161, 152, 143, 137, 128, 122, 113, 107, 101, 95, 92,
			86, 80, 77, 71, 68, 62, 60, 56, 53, 50, 47, 45, 42, 40, 37, 35,
			33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18, 17, 16, 15, 14, 13,
			12, 11, 10, 9, 8, 7, 255, 241, 228, 216, 202, 192, 181, 171, 162, 153,
			142, 135, 127, 121, 115, 112, 102, 97, 90, 85, 82, 75, 72, 67, 63, 60,
			57, 55, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30, 28, 27, 25, 23,
			22, 21, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 3, 2, 1, 0, 243, 230, 217, 204, 193, 181, 173, 162, 153, 144,
			136, 128, 121, 114, 108, 102, 96, 91, 85, 81, 76, 72, 68, 64, 60, 57,
			53, 50, 47, 45, 42, 40, 37, 35, 33, 31, 29, 28, 26, 24, 23, 22,
			20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5,
			4, 3, 2, 1, 0, 0, 13, 13, 12, 11, 11, 10, 10, 9, 8, 8,
			7, 7, 7, 6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3,
			3, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
			1, 1, 2, 2, 2, 2, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2,
			2, 2, 2, 3, 3, 3, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
			3, 3, 3, 3, 4, 4, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3,
			3, 4, 4, 4, 5, 5, 0, 0, 1, 1, 2, 2, 2, 3, 3, 4,
			4, 4, 5, 5, 6, 6, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4,
			5, 5, 6, 6, 7, 7, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5,
			5, 6, 6, 7, 7, 8, 0, 1, 1, 2, 2, 3, 4, 4, 5, 5,
			6, 7, 7, 8, 8, 9, 0, 1, 1, 2, 3, 3, 4, 5, 5, 6,
			7, 7, 8, 9, 9, 10, 0, 1, 1, 2, 3, 4, 4, 5, 6, 7,
			7, 8, 9, 10, 10, 11, 0, 1, 2, 2, 3, 4, 5, 6, 6, 7,
			8, 9, 10, 10, 11, 12, 0, 1, 2, 3, 3, 4, 5, 6, 7, 8,
			9, 10, 10, 11, 12, 13, 0, 1, 2, 3, 4, 5, 6, 7, 7, 8,
			9, 10, 11, 12, 13, 14, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 76, 15, 6, 76, 252, 7, 76, 33, 8, 76,
			88, 6, 76, 43, 11, 134, 211, 132, 212, 72, 160, 168, 169, 0, 153, 127,
			2, 136, 208, 250, 160, 4, 177, 211, 141, 21, 8, 200, 177, 211, 141, 36,
			3, 200, 177, 211, 141, 5, 8, 141, 38, 3, 160, 8, 177, 211, 153, 195,
			0, 200, 192, 16, 208, 246, 104, 72, 10, 10, 24, 101, 209, 133, 209, 104,
			8, 41, 192, 10, 42, 42, 40, 101, 210, 133, 210, 32, 110, 6, 169, 0,
			141, 8, 210, 160, 3, 140, 15, 210, 160, 8, 153, 0, 210, 136, 16, 250,
			173, 5, 8, 96, 162, 0, 142, 39, 3, 138, 168, 177, 209, 201, 254, 176,
			45, 168, 177, 205, 157, 128, 2, 177, 207, 157, 132, 2, 169, 0, 157, 136,
			2, 169, 1, 157, 140, 2, 169, 128, 157, 180, 2, 232, 224, 4, 208, 217,
			165, 209, 24, 105, 4, 133, 209, 144, 27, 230, 210, 76, 190, 6, 240, 4,
			169, 0, 240, 223, 160, 2, 177, 209, 170, 200, 177, 209, 133, 210, 134, 209,
			162, 0, 240, 181, 173, 36, 3, 141, 22, 7, 162, 255, 232, 222, 140, 2,
			208, 69, 189, 128, 2, 133, 211, 189, 132, 2, 133, 212, 188, 136, 2, 254,
			136, 2, 177, 211, 133, 217, 41, 63, 201, 61, 240, 17, 176, 56, 157, 144,
			2, 157, 16, 3, 200, 177, 211, 74, 41, 126, 157, 180, 2, 169, 1, 157,
			140, 2, 188, 136, 2, 254, 136, 2, 177, 211, 74, 102, 217, 74, 102, 217,
			165, 217, 41, 240, 157, 148, 2, 224, 3, 208, 177, 169, 255, 141, 36, 3,
			141, 37, 3, 76, 101, 7, 201, 63, 240, 27, 165, 217, 41, 192, 240, 9,
			10, 42, 42, 157, 140, 2, 76, 17, 7, 200, 177, 211, 157, 140, 2, 254,
			136, 2, 76, 17, 7, 165, 217, 48, 12, 200, 177, 211, 141, 22, 7, 254,
			136, 2, 76, 214, 6, 201, 255, 240, 9, 200, 177, 211, 157, 136, 2, 76,
			214, 6, 76, 110, 6, 76, 33, 8, 202, 48, 250, 188, 180, 2, 48, 248,
			177, 203, 157, 184, 2, 133, 215, 200, 177, 203, 157, 188, 2, 133, 216, 169,
			1, 157, 20, 3, 168, 177, 215, 157, 4, 3, 200, 177, 215, 157, 196, 2,
			200, 177, 215, 157, 200, 2, 200, 177, 215, 157, 240, 2, 41, 63, 157, 8,
			3, 177, 215, 41, 64, 157, 244, 2, 200, 177, 215, 157, 32, 3, 200, 177,
			215, 157, 208, 2, 200, 177, 215, 157, 216, 2, 200, 177, 215, 157, 220, 2,
			200, 177, 215, 168, 185, 160, 3, 157, 224, 2, 157, 228, 2, 185, 161, 3,
			157, 232, 2, 160, 10, 177, 215, 157, 236, 2, 169, 128, 157, 212, 2, 157,
			180, 2, 10, 157, 204, 2, 157, 156, 2, 168, 177, 215, 157, 0, 3, 105,
			0, 157, 192, 2, 169, 12, 157, 252, 2, 168, 177, 215, 157, 248, 2, 76,
			98, 7, 32, 43, 11, 206, 38, 3, 208, 29, 169, 255, 141, 38, 3, 206,
			37, 3, 208, 19, 238, 39, 3, 173, 39, 3, 201, 255, 240, 3, 76, 190,
			6, 76, 110, 6, 76, 95, 10, 169, 4, 133, 214, 162, 3, 189, 188, 2,
			240, 242, 133, 212, 189, 184, 2, 133, 211, 188, 192, 2, 177, 211, 133, 217,
			200, 177, 211, 133, 218, 200, 177, 211, 133, 219, 200, 152, 221, 196, 2, 144,
			10, 240, 8, 169, 128, 157, 204, 2, 189, 200, 2, 157, 192, 2, 165, 217,
			41, 15, 29, 148, 2, 168, 185, 0, 5, 133, 220, 165, 218, 41, 14, 168,
			185, 144, 3, 133, 213, 165, 220, 25, 145, 3, 157, 28, 3, 189, 220, 2,
			240, 40, 201, 1, 208, 33, 189, 156, 2, 24, 125, 236, 2, 24, 188, 224,
			2, 121, 165, 3, 157, 156, 2, 200, 152, 221, 232, 2, 208, 3, 189, 228,
			2, 157, 224, 2, 76, 164, 8, 222, 220, 2, 188, 0, 3, 192, 13, 144,
			60, 189, 8, 3, 16, 49, 152, 221, 252, 2, 208, 8, 189, 4, 3, 157,
			252, 2, 208, 3, 254, 252, 2, 189, 184, 2, 133, 215, 189, 188, 2, 133,
			216, 188, 252, 2, 177, 215, 188, 244, 2, 240, 4, 24, 125, 248, 2, 157,
			248, 2, 189, 240, 2, 41, 63, 56, 233, 1, 157, 8, 3, 189, 204, 2,
			16, 31, 189, 148, 2, 240, 26, 221, 216, 2, 240, 21, 144, 19, 168, 189,
			212, 2, 24, 125, 208, 2, 157, 212, 2, 144, 6, 152, 233, 16, 157, 148,
			2, 169, 0, 133, 221, 165, 218, 157, 12, 3, 41, 112, 74, 74, 141, 28,
			9, 144, 254, 76, 210, 9, 234, 76, 60, 9, 234, 76, 65, 9, 234, 76,
			75, 9, 234, 76, 87, 9, 234, 76, 102, 9, 234, 76, 169, 9, 234, 76,
			184, 9, 165, 219, 76, 21, 10, 165, 219, 133, 221, 189, 144, 2, 76, 216,
			9, 189, 144, 2, 24, 101, 219, 157, 144, 2, 76, 216, 9, 189, 156, 2,
			24, 101, 219, 157, 156, 2, 189, 144, 2, 76, 216, 9, 189, 240, 2, 16,
			12, 188, 144, 2, 177, 213, 24, 125, 248, 2, 76, 135, 9, 189, 144, 2,
			24, 125, 248, 2, 201, 61, 144, 2, 169, 63, 168, 177, 213, 157, 160, 2,
			164, 219, 208, 3, 157, 164, 2, 152, 74, 74, 74, 74, 157, 168, 2, 157,
			172, 2, 165, 219, 41, 15, 157, 176, 2, 189, 144, 2, 76, 216, 9, 165,
			219, 24, 125, 20, 3, 157, 20, 3, 189, 144, 2, 76, 216, 9, 165, 219,
			201, 128, 240, 6, 157, 144, 2, 76, 216, 9, 189, 28, 3, 9, 240, 157,
			28, 3, 189, 144, 2, 76, 216, 9, 189, 144, 2, 24, 101, 219, 188, 240,
			2, 48, 31, 24, 125, 248, 2, 201, 61, 144, 7, 169, 0, 157, 28, 3,
			169, 63, 157, 16, 3, 168, 177, 213, 24, 125, 156, 2, 24, 101, 221, 76,
			21, 10, 201, 61, 144, 7, 169, 0, 157, 28, 3, 169, 63, 168, 189, 156,
			2, 24, 125, 248, 2, 24, 113, 213, 24, 101, 221, 157, 24, 3, 189, 172,
			2, 240, 50, 222, 172, 2, 208, 45, 189, 168, 2, 157, 172, 2, 189, 164,
			2, 221, 160, 2, 240, 31, 176, 13, 125, 176, 2, 176, 18, 221, 160, 2,
			176, 13, 76, 76, 10, 253, 176, 2, 144, 5, 221, 160, 2, 176, 3, 189,
			160, 2, 157, 164, 2, 165, 218, 41, 1, 240, 10, 189, 164, 2, 24, 125,
			156, 2, 157, 24, 3, 202, 48, 3, 76, 39, 8, 173, 32, 3, 13, 33,
			3, 13, 34, 3, 13, 35, 3, 170, 142, 44, 11, 173, 12, 3, 16, 33,
			173, 28, 3, 41, 15, 240, 26, 173, 24, 3, 24, 109, 20, 3, 141, 26,
			3, 173, 30, 3, 41, 16, 208, 5, 169, 0, 141, 30, 3, 138, 9, 4,
			170, 173, 13, 3, 16, 33, 173, 29, 3, 41, 15, 240, 26, 173, 25, 3,
			24, 109, 21, 3, 141, 27, 3, 173, 31, 3, 41, 16, 208, 5, 169, 0,
			141, 31, 3, 138, 9, 2, 170, 236, 44, 11, 208, 94, 173, 13, 3, 41,
			14, 201, 6, 208, 38, 173, 29, 3, 41, 15, 240, 31, 172, 17, 3, 185,
			192, 3, 141, 24, 3, 185, 192, 4, 141, 25, 3, 173, 28, 3, 41, 16,
			208, 5, 169, 0, 141, 28, 3, 138, 9, 80, 170, 173, 15, 3, 41, 14,
			201, 6, 208, 38, 173, 31, 3, 41, 15, 240, 31, 172, 19, 3, 185, 192,
			3, 141, 26, 3, 185, 192, 4, 141, 27, 3, 173, 30, 3, 41, 16, 208,
			5, 169, 0, 141, 30, 3, 138, 9, 40, 170, 142, 44, 11, 173, 38, 3,
			96, 160, 255, 173, 24, 3, 174, 28, 3, 141, 0, 210, 142, 1, 210, 173,
			25, 3, 174, 29, 3, 141, 2, 210, 142, 3, 210, 173, 26, 3, 174, 30,
			3, 141, 4, 210, 142, 5, 210, 173, 27, 3, 174, 31, 3, 141, 6, 210,
			142, 7, 210, 140, 8, 210, 96 };
		static readonly byte[] CiBinaryResource_rmt8_obx = { 255, 255, 144, 3, 108, 12, 128, 0, 128, 32, 128, 64, 0, 192, 128, 128,
			128, 160, 0, 192, 64, 192, 0, 1, 5, 11, 21, 0, 1, 255, 255, 1,
			1, 0, 255, 255, 0, 1, 1, 1, 0, 255, 255, 255, 255, 0, 1, 1,
			0, 0, 0, 0, 0, 0, 242, 51, 150, 226, 56, 140, 0, 106, 232, 106,
			239, 128, 8, 174, 70, 230, 149, 65, 246, 176, 110, 48, 246, 187, 132, 82,
			34, 244, 200, 160, 122, 85, 52, 20, 245, 216, 189, 164, 141, 119, 96, 78,
			56, 39, 21, 6, 247, 232, 219, 207, 195, 184, 172, 162, 154, 144, 136, 127,
			120, 112, 106, 100, 94, 0, 191, 182, 170, 161, 152, 143, 137, 128, 242, 230,
			218, 206, 191, 182, 170, 161, 152, 143, 137, 128, 122, 113, 107, 101, 95, 92,
			86, 80, 77, 71, 68, 62, 60, 56, 53, 50, 47, 45, 42, 40, 37, 35,
			33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18, 17, 16, 15, 14, 13,
			12, 11, 10, 9, 8, 7, 255, 241, 228, 216, 202, 192, 181, 171, 162, 153,
			142, 135, 127, 121, 115, 112, 102, 97, 90, 85, 82, 75, 72, 67, 63, 60,
			57, 55, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30, 28, 27, 25, 23,
			22, 21, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 3, 2, 1, 0, 243, 230, 217, 204, 193, 181, 173, 162, 153, 144,
			136, 128, 121, 114, 108, 102, 96, 91, 85, 81, 76, 72, 68, 64, 60, 57,
			53, 50, 47, 45, 42, 40, 37, 35, 33, 31, 29, 28, 26, 24, 23, 22,
			20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5,
			4, 3, 2, 1, 0, 0, 13, 13, 12, 11, 11, 10, 10, 9, 8, 8,
			7, 7, 7, 6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3,
			3, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
			1, 1, 2, 2, 2, 2, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2,
			2, 2, 2, 3, 3, 3, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
			3, 3, 3, 3, 4, 4, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3,
			3, 4, 4, 4, 5, 5, 0, 0, 1, 1, 2, 2, 2, 3, 3, 4,
			4, 4, 5, 5, 6, 6, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4,
			5, 5, 6, 6, 7, 7, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5,
			5, 6, 6, 7, 7, 8, 0, 1, 1, 2, 2, 3, 4, 4, 5, 5,
			6, 7, 7, 8, 8, 9, 0, 1, 1, 2, 3, 3, 4, 5, 5, 6,
			7, 7, 8, 9, 9, 10, 0, 1, 1, 2, 3, 4, 4, 5, 6, 7,
			7, 8, 9, 10, 10, 11, 0, 1, 2, 2, 3, 4, 5, 6, 6, 7,
			8, 9, 10, 10, 11, 12, 0, 1, 2, 3, 3, 4, 5, 6, 7, 8,
			9, 10, 10, 11, 12, 13, 0, 1, 2, 3, 4, 5, 6, 7, 7, 8,
			9, 10, 11, 12, 13, 14, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 76, 15, 6, 76, 9, 8, 76, 46, 8, 76,
			92, 6, 76, 2, 12, 134, 211, 132, 212, 72, 160, 0, 152, 153, 0, 2,
			153, 76, 2, 200, 208, 247, 160, 4, 177, 211, 141, 34, 8, 200, 177, 211,
			141, 72, 3, 200, 177, 211, 141, 18, 8, 141, 74, 3, 160, 8, 177, 211,
			153, 195, 0, 200, 192, 16, 208, 246, 104, 72, 10, 10, 10, 24, 101, 209,
			133, 209, 104, 8, 41, 224, 10, 42, 42, 42, 40, 101, 210, 133, 210, 32,
			123, 6, 169, 0, 141, 8, 210, 141, 24, 210, 160, 3, 140, 15, 210, 140,
			31, 210, 160, 8, 153, 0, 210, 153, 16, 210, 136, 16, 247, 173, 18, 8,
			96, 162, 0, 142, 75, 3, 138, 168, 177, 209, 201, 254, 176, 45, 168, 177,
			205, 157, 0, 2, 177, 207, 157, 8, 2, 169, 0, 157, 16, 2, 169, 1,
			157, 24, 2, 169, 128, 157, 104, 2, 232, 224, 8, 208, 217, 165, 209, 24,
			105, 8, 133, 209, 144, 27, 230, 210, 76, 203, 6, 240, 4, 169, 0, 240,
			223, 160, 2, 177, 209, 170, 200, 177, 209, 133, 210, 134, 209, 162, 0, 240,
			181, 173, 72, 3, 141, 35, 7, 162, 255, 232, 222, 24, 2, 208, 69, 189,
			0, 2, 133, 211, 189, 8, 2, 133, 212, 188, 16, 2, 254, 16, 2, 177,
			211, 133, 217, 41, 63, 201, 61, 240, 17, 176, 56, 157, 32, 2, 157, 32,
			3, 200, 177, 211, 74, 41, 126, 157, 104, 2, 169, 1, 157, 24, 2, 188,
			16, 2, 254, 16, 2, 177, 211, 74, 102, 217, 74, 102, 217, 165, 217, 41,
			240, 157, 40, 2, 224, 7, 208, 177, 169, 255, 141, 72, 3, 141, 73, 3,
			76, 114, 7, 201, 63, 240, 27, 165, 217, 41, 192, 240, 9, 10, 42, 42,
			157, 24, 2, 76, 30, 7, 200, 177, 211, 157, 24, 2, 254, 16, 2, 76,
			30, 7, 165, 217, 48, 12, 200, 177, 211, 141, 35, 7, 254, 16, 2, 76,
			227, 6, 201, 255, 240, 9, 200, 177, 211, 157, 16, 2, 76, 227, 6, 76,
			123, 6, 76, 46, 8, 202, 48, 250, 188, 104, 2, 48, 248, 177, 203, 157,
			112, 2, 133, 215, 200, 177, 203, 157, 120, 2, 133, 216, 169, 1, 157, 40,
			3, 168, 177, 215, 157, 8, 3, 200, 177, 215, 157, 136, 2, 200, 177, 215,
			157, 144, 2, 200, 177, 215, 157, 224, 2, 41, 63, 157, 16, 3, 177, 215,
			41, 64, 157, 232, 2, 200, 177, 215, 157, 64, 3, 200, 177, 215, 157, 160,
			2, 200, 177, 215, 157, 176, 2, 200, 177, 215, 157, 184, 2, 200, 177, 215,
			168, 185, 160, 3, 157, 192, 2, 157, 200, 2, 185, 161, 3, 157, 208, 2,
			160, 10, 177, 215, 157, 216, 2, 169, 128, 157, 168, 2, 157, 104, 2, 10,
			157, 152, 2, 157, 56, 2, 168, 177, 215, 157, 0, 3, 105, 0, 157, 128,
			2, 169, 12, 157, 248, 2, 168, 177, 215, 157, 240, 2, 76, 111, 7, 32,
			2, 12, 206, 74, 3, 208, 29, 169, 255, 141, 74, 3, 206, 73, 3, 208,
			19, 238, 75, 3, 173, 75, 3, 201, 255, 240, 3, 76, 203, 6, 76, 123,
			6, 76, 116, 10, 169, 4, 133, 214, 162, 7, 189, 120, 2, 240, 242, 133,
			212, 189, 112, 2, 133, 211, 188, 128, 2, 177, 211, 133, 217, 200, 177, 211,
			133, 218, 200, 177, 211, 133, 219, 200, 152, 221, 136, 2, 144, 10, 240, 8,
			169, 128, 157, 152, 2, 189, 144, 2, 157, 128, 2, 165, 217, 224, 4, 144,
			4, 74, 74, 74, 74, 41, 15, 29, 40, 2, 168, 185, 0, 5, 133, 220,
			165, 218, 41, 14, 168, 185, 144, 3, 133, 213, 165, 220, 25, 145, 3, 157,
			56, 3, 189, 184, 2, 240, 40, 201, 1, 208, 33, 189, 56, 2, 24, 125,
			216, 2, 24, 188, 192, 2, 121, 165, 3, 157, 56, 2, 200, 152, 221, 208,
			2, 208, 3, 189, 200, 2, 157, 192, 2, 76, 185, 8, 222, 184, 2, 188,
			0, 3, 192, 13, 144, 60, 189, 16, 3, 16, 49, 152, 221, 248, 2, 208,
			8, 189, 8, 3, 157, 248, 2, 208, 3, 254, 248, 2, 189, 112, 2, 133,
			215, 189, 120, 2, 133, 216, 188, 248, 2, 177, 215, 188, 232, 2, 240, 4,
			24, 125, 240, 2, 157, 240, 2, 189, 224, 2, 41, 63, 56, 233, 1, 157,
			16, 3, 189, 152, 2, 16, 31, 189, 40, 2, 240, 26, 221, 176, 2, 240,
			21, 144, 19, 168, 189, 168, 2, 24, 125, 160, 2, 157, 168, 2, 144, 6,
			152, 233, 16, 157, 40, 2, 169, 0, 133, 221, 165, 218, 157, 24, 3, 41,
			112, 74, 74, 141, 49, 9, 144, 254, 76, 231, 9, 234, 76, 81, 9, 234,
			76, 86, 9, 234, 76, 96, 9, 234, 76, 108, 9, 234, 76, 123, 9, 234,
			76, 190, 9, 234, 76, 205, 9, 165, 219, 76, 42, 10, 165, 219, 133, 221,
			189, 32, 2, 76, 237, 9, 189, 32, 2, 24, 101, 219, 157, 32, 2, 76,
			237, 9, 189, 56, 2, 24, 101, 219, 157, 56, 2, 189, 32, 2, 76, 237,
			9, 189, 224, 2, 16, 12, 188, 32, 2, 177, 213, 24, 125, 240, 2, 76,
			156, 9, 189, 32, 2, 24, 125, 240, 2, 201, 61, 144, 2, 169, 63, 168,
			177, 213, 157, 64, 2, 164, 219, 208, 3, 157, 72, 2, 152, 74, 74, 74,
			74, 157, 80, 2, 157, 88, 2, 165, 219, 41, 15, 157, 96, 2, 189, 32,
			2, 76, 237, 9, 165, 219, 24, 125, 40, 3, 157, 40, 3, 189, 32, 2,
			76, 237, 9, 165, 219, 201, 128, 240, 6, 157, 32, 2, 76, 237, 9, 189,
			56, 3, 9, 240, 157, 56, 3, 189, 32, 2, 76, 237, 9, 189, 32, 2,
			24, 101, 219, 188, 224, 2, 48, 31, 24, 125, 240, 2, 201, 61, 144, 7,
			169, 0, 157, 56, 3, 169, 63, 157, 32, 3, 168, 177, 213, 24, 125, 56,
			2, 24, 101, 221, 76, 42, 10, 201, 61, 144, 7, 169, 0, 157, 56, 3,
			169, 63, 168, 189, 56, 2, 24, 125, 240, 2, 24, 113, 213, 24, 101, 221,
			157, 48, 3, 189, 88, 2, 240, 50, 222, 88, 2, 208, 45, 189, 80, 2,
			157, 88, 2, 189, 72, 2, 221, 64, 2, 240, 31, 176, 13, 125, 96, 2,
			176, 18, 221, 64, 2, 176, 13, 76, 97, 10, 253, 96, 2, 144, 5, 221,
			64, 2, 176, 3, 189, 64, 2, 157, 72, 2, 165, 218, 41, 1, 240, 10,
			189, 72, 2, 24, 125, 56, 2, 157, 48, 3, 202, 48, 3, 76, 52, 8,
			173, 64, 3, 13, 65, 3, 13, 66, 3, 13, 67, 3, 170, 142, 101, 12,
			173, 24, 3, 16, 33, 173, 56, 3, 41, 15, 240, 26, 173, 48, 3, 24,
			109, 40, 3, 141, 50, 3, 173, 58, 3, 41, 16, 208, 5, 169, 0, 141,
			58, 3, 138, 9, 4, 170, 173, 25, 3, 16, 33, 173, 57, 3, 41, 15,
			240, 26, 173, 49, 3, 24, 109, 41, 3, 141, 51, 3, 173, 59, 3, 41,
			16, 208, 5, 169, 0, 141, 59, 3, 138, 9, 2, 170, 236, 101, 12, 208,
			94, 173, 25, 3, 41, 14, 201, 6, 208, 38, 173, 57, 3, 41, 15, 240,
			31, 172, 33, 3, 185, 192, 3, 141, 48, 3, 185, 192, 4, 141, 49, 3,
			173, 56, 3, 41, 16, 208, 5, 169, 0, 141, 56, 3, 138, 9, 80, 170,
			173, 27, 3, 41, 14, 201, 6, 208, 38, 173, 59, 3, 41, 15, 240, 31,
			172, 35, 3, 185, 192, 3, 141, 50, 3, 185, 192, 4, 141, 51, 3, 173,
			58, 3, 41, 16, 208, 5, 169, 0, 141, 58, 3, 138, 9, 40, 170, 142,
			101, 12, 173, 68, 3, 13, 69, 3, 13, 70, 3, 13, 71, 3, 170, 142,
			3, 12, 173, 28, 3, 16, 33, 173, 60, 3, 41, 15, 240, 26, 173, 52,
			3, 24, 109, 44, 3, 141, 54, 3, 173, 62, 3, 41, 16, 208, 5, 169,
			0, 141, 62, 3, 138, 9, 4, 170, 173, 29, 3, 16, 33, 173, 61, 3,
			41, 15, 240, 26, 173, 53, 3, 24, 109, 45, 3, 141, 55, 3, 173, 63,
			3, 41, 16, 208, 5, 169, 0, 141, 63, 3, 138, 9, 2, 170, 236, 3,
			12, 208, 94, 173, 29, 3, 41, 14, 201, 6, 208, 38, 173, 61, 3, 41,
			15, 240, 31, 172, 37, 3, 185, 192, 3, 141, 52, 3, 185, 192, 4, 141,
			53, 3, 173, 60, 3, 41, 16, 208, 5, 169, 0, 141, 60, 3, 138, 9,
			80, 170, 173, 31, 3, 41, 14, 201, 6, 208, 38, 173, 63, 3, 41, 15,
			240, 31, 172, 39, 3, 185, 192, 3, 141, 54, 3, 185, 192, 4, 141, 55,
			3, 173, 62, 3, 41, 16, 208, 5, 169, 0, 141, 62, 3, 138, 9, 40,
			170, 142, 3, 12, 173, 74, 3, 96, 160, 255, 173, 52, 3, 174, 48, 3,
			141, 16, 210, 142, 0, 210, 173, 60, 3, 174, 56, 3, 141, 17, 210, 142,
			1, 210, 173, 53, 3, 174, 49, 3, 141, 18, 210, 142, 2, 210, 173, 61,
			3, 174, 57, 3, 141, 19, 210, 142, 3, 210, 173, 54, 3, 174, 50, 3,
			141, 20, 210, 142, 4, 210, 173, 62, 3, 174, 58, 3, 141, 21, 210, 142,
			5, 210, 173, 55, 3, 174, 51, 3, 141, 22, 210, 142, 6, 210, 173, 63,
			3, 174, 59, 3, 141, 23, 210, 142, 7, 210, 169, 255, 140, 24, 210, 141,
			8, 210, 96 };
		static readonly byte[] CiBinaryResource_tm2_obx = { 255, 255, 0, 2, 107, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
			1, 1, 2, 2, 2, 2, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2,
			2, 2, 2, 3, 3, 3, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
			3, 3, 3, 3, 4, 4, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3,
			3, 4, 4, 4, 5, 5, 0, 0, 1, 1, 2, 2, 2, 3, 3, 4,
			4, 4, 5, 5, 6, 6, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4,
			5, 5, 6, 6, 7, 7, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5,
			5, 6, 6, 7, 7, 8, 0, 1, 1, 2, 2, 3, 4, 4, 5, 5,
			6, 7, 7, 8, 8, 9, 0, 1, 1, 2, 3, 3, 4, 5, 5, 6,
			7, 7, 8, 9, 9, 10, 0, 1, 1, 2, 3, 4, 4, 5, 6, 7,
			7, 8, 9, 10, 10, 11, 0, 1, 2, 2, 3, 4, 5, 6, 6, 7,
			8, 9, 10, 10, 11, 12, 0, 1, 2, 3, 3, 4, 5, 6, 7, 8,
			9, 10, 10, 11, 12, 13, 0, 1, 2, 3, 4, 5, 6, 7, 7, 8,
			9, 10, 11, 12, 13, 14, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
			10, 11, 12, 13, 14, 15, 0, 241, 228, 215, 203, 192, 181, 170, 161, 152,
			143, 135, 127, 120, 114, 107, 101, 95, 90, 85, 80, 75, 71, 67, 63, 60,
			56, 53, 50, 47, 44, 42, 39, 37, 35, 33, 31, 29, 28, 26, 24, 23,
			22, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 3, 2, 1, 0, 0, 242, 233, 218, 206, 191, 182, 170, 161, 152,
			143, 137, 128, 122, 113, 107, 101, 95, 92, 86, 80, 77, 71, 68, 62, 60,
			56, 53, 50, 47, 45, 42, 40, 37, 35, 33, 31, 29, 28, 26, 24, 23,
			22, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 3, 2, 1, 0, 0, 255, 241, 228, 216, 202, 192, 181, 171, 162,
			153, 142, 135, 127, 121, 115, 112, 102, 97, 90, 85, 82, 75, 72, 67, 63,
			60, 57, 55, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30, 28, 27, 25,
			23, 22, 21, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7,
			6, 5, 4, 3, 2, 1, 0, 243, 230, 217, 204, 193, 181, 173, 162, 153,
			144, 136, 128, 121, 114, 108, 102, 96, 91, 85, 81, 76, 72, 68, 64, 60,
			57, 53, 50, 47, 45, 42, 40, 37, 35, 33, 31, 29, 28, 26, 24, 23,
			22, 20, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6,
			5, 4, 3, 2, 1, 0, 226, 56, 140, 0, 106, 232, 106, 239, 128, 8,
			174, 70, 230, 149, 65, 246, 176, 110, 48, 246, 187, 132, 82, 34, 244, 200,
			160, 122, 85, 52, 20, 245, 216, 189, 164, 141, 119, 96, 78, 56, 39, 21,
			6, 247, 232, 219, 207, 195, 184, 172, 162, 154, 144, 136, 127, 120, 112, 106,
			100, 94, 87, 82, 50, 10, 0, 242, 51, 150, 226, 56, 140, 0, 106, 232,
			106, 239, 128, 8, 174, 70, 230, 149, 65, 246, 176, 110, 48, 246, 187, 132,
			82, 34, 244, 200, 160, 122, 85, 52, 20, 245, 216, 189, 164, 141, 119, 96,
			78, 56, 39, 21, 6, 247, 232, 219, 207, 195, 184, 172, 162, 154, 144, 136,
			127, 120, 112, 106, 100, 94, 11, 11, 10, 10, 9, 8, 8, 7, 7, 7,
			6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3, 3, 2, 2,
			2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 13, 13, 12, 11, 11, 10, 10, 9, 8,
			8, 7, 7, 7, 6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3,
			3, 3, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1,
			1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 76, 228, 13, 76, 227, 6, 76, 159, 8, 1,
			16, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 5,
			6, 7, 0, 1, 2, 3, 4, 2, 0, 0, 4, 2, 0, 0, 0, 16,
			0, 8, 0, 16, 0, 8, 133, 211, 129, 169, 133, 211, 129, 169, 136, 177,
			250, 141, 23, 5, 162, 0, 134, 252, 10, 38, 252, 10, 38, 252, 10, 38,
			252, 10, 38, 252, 109, 23, 5, 144, 2, 230, 252, 24, 105, 0, 133, 250,
			165, 252, 105, 0, 133, 251, 76, 7, 7, 32, 181, 9, 173, 22, 5, 240,
			5, 206, 28, 5, 48, 3, 76, 162, 8, 206, 29, 5, 208, 82, 162, 0,
			238, 23, 5, 173, 25, 5, 133, 250, 173, 26, 5, 133, 251, 160, 16, 177,
			250, 48, 171, 208, 3, 76, 44, 15, 141, 29, 5, 136, 177, 250, 136, 132,
			252, 168, 185, 255, 255, 157, 80, 5, 185, 255, 255, 157, 88, 5, 169, 0,
			157, 112, 5, 157, 96, 5, 164, 252, 177, 250, 157, 104, 5, 232, 136, 16,
			219, 169, 17, 24, 101, 250, 141, 25, 5, 169, 0, 101, 251, 141, 26, 5,
			173, 27, 5, 141, 28, 5, 162, 7, 222, 112, 5, 48, 6, 202, 16, 248,
			76, 162, 8, 189, 80, 5, 133, 250, 189, 88, 5, 133, 251, 188, 96, 5,
			177, 250, 208, 28, 200, 177, 250, 157, 208, 5, 41, 240, 157, 216, 5, 177,
			250, 10, 10, 10, 10, 157, 224, 5, 200, 152, 157, 96, 5, 76, 87, 7,
			201, 64, 176, 79, 125, 104, 5, 157, 152, 5, 200, 177, 250, 16, 37, 41,
			127, 133, 252, 200, 177, 250, 157, 208, 5, 41, 240, 157, 216, 5, 177, 250,
			10, 10, 10, 10, 157, 224, 5, 200, 152, 157, 96, 5, 164, 252, 32, 156,
			15, 76, 87, 7, 168, 254, 96, 5, 254, 96, 5, 189, 208, 5, 41, 240,
			157, 216, 5, 189, 208, 5, 10, 10, 10, 10, 157, 224, 5, 32, 156, 15,
			76, 87, 7, 201, 128, 176, 37, 41, 63, 24, 125, 104, 5, 157, 152, 5,
			200, 177, 250, 157, 208, 5, 41, 240, 157, 216, 5, 177, 250, 10, 10, 10,
			10, 157, 224, 5, 200, 152, 157, 96, 5, 76, 87, 7, 208, 14, 200, 177,
			250, 157, 112, 5, 200, 152, 157, 96, 5, 76, 87, 7, 201, 192, 176, 15,
			41, 63, 24, 125, 104, 5, 157, 152, 5, 254, 96, 5, 76, 87, 7, 201,
			208, 176, 15, 200, 254, 96, 5, 41, 15, 141, 27, 5, 141, 28, 5, 76,
			106, 7, 201, 224, 176, 22, 177, 250, 133, 252, 200, 177, 250, 133, 253, 200,
			152, 157, 96, 5, 165, 252, 32, 14, 14, 76, 87, 7, 201, 240, 176, 46,
			177, 250, 133, 252, 200, 177, 250, 133, 253, 165, 252, 32, 14, 14, 188, 96,
			5, 200, 200, 177, 250, 157, 208, 5, 41, 240, 157, 216, 5, 177, 250, 10,
			10, 10, 10, 157, 224, 5, 200, 152, 157, 96, 5, 76, 87, 7, 201, 255,
			176, 11, 233, 239, 157, 112, 5, 254, 96, 5, 76, 87, 7, 169, 64, 157,
			112, 5, 76, 87, 7, 32, 181, 9, 162, 7, 189, 120, 5, 240, 115, 76,
			217, 10, 189, 14, 5, 240, 14, 169, 0, 157, 32, 5, 157, 40, 5, 202,
			16, 232, 76, 31, 9, 164, 253, 185, 0, 3, 24, 101, 252, 157, 56, 5,
			152, 157, 160, 5, 189, 176, 5, 61, 168, 6, 240, 40, 165, 253, 41, 127,
			168, 185, 0, 4, 24, 101, 252, 157, 55, 5, 185, 128, 4, 105, 0, 157,
			56, 5, 169, 0, 157, 31, 5, 188, 152, 6, 153, 39, 5, 202, 202, 16,
			169, 76, 31, 9, 189, 176, 5, 61, 160, 6, 240, 22, 189, 104, 6, 24,
			101, 253, 157, 162, 5, 168, 185, 0, 3, 24, 101, 252, 56, 101, 254, 157,
			58, 5, 202, 16, 133, 232, 134, 252, 162, 3, 173, 9, 5, 240, 6, 41,
			64, 208, 60, 162, 7, 138, 168, 185, 32, 5, 208, 12, 188, 152, 6, 185,
			40, 5, 208, 4, 138, 168, 169, 0, 25, 168, 5, 157, 48, 5, 185, 56,
			5, 157, 72, 5, 185, 160, 5, 157, 64, 5, 185, 176, 5, 5, 252, 133,
			252, 224, 4, 208, 3, 141, 31, 5, 202, 16, 202, 141, 30, 5, 96, 189,
			32, 5, 29, 168, 5, 157, 48, 5, 189, 44, 5, 29, 172, 5, 157, 52,
			5, 189, 56, 5, 157, 72, 5, 189, 60, 5, 157, 76, 5, 189, 160, 5,
			157, 64, 5, 189, 164, 5, 157, 68, 5, 202, 16, 211, 173, 176, 5, 13,
			177, 5, 13, 178, 5, 13, 179, 5, 141, 30, 5, 173, 180, 5, 13, 181,
			5, 13, 182, 5, 13, 183, 5, 141, 31, 5, 96, 173, 9, 5, 208, 3,
			76, 144, 10, 48, 3, 76, 72, 10, 173, 13, 5, 170, 74, 74, 41, 1,
			168, 185, 30, 5, 141, 56, 210, 138, 41, 4, 168, 185, 56, 5, 141, 48,
			210, 189, 32, 5, 141, 49, 210, 185, 57, 5, 141, 50, 210, 189, 33, 5,
			141, 51, 210, 185, 58, 5, 141, 52, 210, 189, 34, 5, 141, 53, 210, 185,
			59, 5, 141, 54, 210, 189, 35, 5, 141, 55, 210, 173, 12, 5, 170, 74,
			74, 41, 1, 168, 185, 30, 5, 141, 40, 210, 138, 41, 4, 168, 185, 56,
			5, 141, 32, 210, 189, 32, 5, 141, 33, 210, 185, 57, 5, 141, 34, 210,
			189, 33, 5, 141, 35, 210, 185, 58, 5, 141, 36, 210, 189, 34, 5, 141,
			37, 210, 185, 59, 5, 141, 38, 210, 189, 35, 5, 141, 39, 210, 173, 11,
			5, 170, 74, 74, 41, 1, 168, 185, 30, 5, 141, 24, 210, 138, 172, 9,
			5, 16, 2, 41, 4, 168, 185, 56, 5, 141, 16, 210, 189, 32, 5, 141,
			17, 210, 185, 57, 5, 141, 18, 210, 189, 33, 5, 141, 19, 210, 185, 58,
			5, 141, 20, 210, 189, 34, 5, 141, 21, 210, 185, 59, 5, 141, 22, 210,
			189, 35, 5, 141, 23, 210, 173, 10, 5, 170, 74, 74, 41, 1, 168, 185,
			30, 5, 141, 8, 210, 138, 172, 9, 5, 16, 2, 41, 4, 168, 185, 56,
			5, 141, 0, 210, 189, 32, 5, 141, 1, 210, 185, 57, 5, 141, 2, 210,
			189, 33, 5, 141, 3, 210, 185, 58, 5, 141, 4, 210, 189, 34, 5, 141,
			5, 210, 185, 59, 5, 141, 6, 210, 189, 35, 5, 141, 7, 210, 96, 189,
			128, 5, 133, 250, 189, 136, 5, 133, 251, 189, 128, 6, 133, 252, 189, 136,
			6, 133, 253, 189, 144, 6, 133, 254, 189, 184, 5, 221, 192, 5, 144, 12,
			157, 8, 6, 189, 200, 5, 157, 184, 5, 76, 11, 11, 189, 8, 6, 240,
			48, 189, 232, 5, 240, 19, 222, 248, 5, 208, 14, 157, 248, 5, 189, 216,
			5, 240, 6, 56, 233, 16, 157, 216, 5, 189, 240, 5, 240, 19, 222, 0,
			6, 208, 14, 157, 0, 6, 189, 224, 5, 240, 6, 56, 233, 16, 157, 224,
			5, 188, 72, 6, 177, 250, 24, 125, 152, 5, 24, 101, 253, 133, 253, 222,
			88, 6, 16, 57, 189, 80, 6, 157, 88, 6, 189, 96, 6, 240, 30, 24,
			125, 72, 6, 157, 72, 6, 240, 13, 221, 64, 6, 144, 32, 169, 255, 157,
			96, 6, 76, 135, 11, 169, 1, 157, 96, 6, 76, 135, 11, 254, 72, 6,
			189, 64, 6, 221, 72, 6, 176, 5, 169, 0, 157, 72, 6, 169, 19, 24,
			101, 250, 133, 250, 144, 2, 230, 251, 188, 184, 5, 177, 250, 41, 240, 157,
			168, 5, 177, 250, 41, 15, 29, 216, 5, 168, 185, 0, 2, 5, 255, 168,
			185, 0, 2, 157, 32, 5, 188, 184, 5, 200, 177, 250, 41, 15, 29, 224,
			5, 168, 185, 0, 2, 5, 255, 168, 185, 0, 2, 157, 40, 5, 189, 40,
			6, 208, 39, 189, 16, 6, 141, 212, 11, 16, 254, 76, 209, 12, 234, 76,
			108, 12, 234, 76, 167, 12, 234, 76, 212, 12, 234, 76, 1, 13, 234, 76,
			33, 13, 234, 76, 65, 13, 234, 76, 73, 13, 222, 40, 6, 188, 184, 5,
			200, 177, 250, 41, 112, 74, 74, 74, 141, 34, 12, 177, 250, 48, 6, 189,
			112, 6, 76, 18, 12, 189, 120, 6, 61, 176, 6, 157, 176, 5, 200, 200,
			152, 157, 184, 5, 136, 177, 250, 144, 254, 144, 22, 144, 12, 144, 34, 144,
			24, 144, 46, 144, 36, 144, 50, 144, 52, 125, 128, 6, 157, 128, 6, 177,
			250, 24, 101, 252, 133, 252, 76, 172, 8, 125, 136, 6, 157, 136, 6, 177,
			250, 24, 101, 253, 133, 253, 76, 172, 8, 125, 144, 6, 157, 144, 6, 177,
			250, 24, 101, 254, 133, 254, 76, 172, 8, 133, 252, 169, 0, 133, 253, 76,
			172, 8, 189, 32, 6, 41, 3, 74, 144, 10, 208, 25, 189, 24, 6, 24,
			101, 252, 133, 252, 222, 56, 6, 16, 78, 254, 32, 6, 189, 48, 6, 157,
			56, 6, 76, 247, 11, 165, 252, 253, 24, 6, 133, 252, 222, 56, 6, 16,
			54, 254, 32, 6, 189, 48, 6, 157, 56, 6, 76, 247, 11, 188, 32, 6,
			189, 24, 6, 48, 2, 200, 200, 136, 152, 24, 101, 252, 133, 252, 222, 56,
			6, 16, 20, 152, 157, 32, 6, 221, 24, 6, 208, 5, 73, 255, 157, 24,
			6, 189, 48, 6, 157, 56, 6, 76, 247, 11, 188, 32, 6, 189, 24, 6,
			48, 2, 200, 200, 136, 152, 24, 101, 253, 133, 253, 222, 56, 6, 16, 231,
			152, 157, 32, 6, 221, 24, 6, 208, 216, 73, 255, 157, 24, 6, 189, 48,
			6, 157, 56, 6, 76, 247, 11, 189, 32, 6, 24, 101, 252, 133, 252, 222,
			56, 6, 16, 195, 189, 24, 6, 24, 125, 32, 6, 157, 32, 6, 189, 48,
			6, 157, 56, 6, 76, 247, 11, 165, 253, 56, 253, 32, 6, 133, 253, 222,
			56, 6, 16, 163, 189, 24, 6, 24, 125, 32, 6, 157, 32, 6, 189, 48,
			6, 157, 56, 6, 76, 247, 11, 189, 24, 6, 24, 101, 252, 133, 252, 76,
			247, 11, 160, 16, 169, 0, 133, 250, 169, 0, 133, 251, 169, 0, 141, 23,
			5, 138, 240, 63, 177, 250, 240, 2, 16, 1, 202, 169, 17, 24, 101, 250,
			133, 250, 144, 2, 230, 251, 238, 23, 5, 208, 230, 162, 0, 169, 0, 133,
			252, 138, 141, 23, 5, 10, 38, 252, 10, 38, 252, 10, 38, 252, 10, 38,
			252, 109, 23, 5, 144, 2, 230, 252, 24, 105, 0, 133, 250, 165, 252, 105,
			0, 133, 251, 32, 44, 15, 165, 250, 141, 25, 5, 165, 251, 141, 26, 5,
			162, 7, 169, 255, 157, 208, 5, 169, 240, 157, 216, 5, 157, 224, 5, 202,
			16, 240, 169, 3, 141, 15, 210, 141, 31, 210, 141, 47, 210, 141, 63, 210,
			206, 23, 5, 232, 142, 28, 5, 232, 142, 29, 5, 142, 22, 5, 96, 138,
			41, 15, 141, 27, 5, 96, 142, 22, 5, 96, 201, 16, 176, 3, 76, 76,
			13, 201, 32, 144, 136, 201, 48, 176, 3, 76, 133, 15, 201, 64, 144, 223,
			201, 80, 176, 3, 76, 44, 15, 201, 96, 144, 219, 201, 112, 144, 3, 76,
			180, 14, 132, 253, 41, 15, 10, 141, 23, 14, 165, 253, 144, 254, 144, 30,
			144, 56, 144, 89, 144, 96, 144, 26, 144, 28, 144, 30, 144, 32, 144, 34,
			144, 36, 144, 13, 144, 11, 144, 9, 144, 7, 144, 5, 144, 3, 141, 24,
			5, 96, 157, 104, 6, 96, 157, 112, 6, 96, 157, 120, 6, 96, 157, 144,
			6, 96, 157, 128, 6, 96, 157, 136, 6, 96, 41, 112, 74, 74, 157, 16,
			6, 41, 48, 208, 3, 157, 32, 6, 165, 253, 48, 6, 41, 15, 157, 24,
			6, 96, 41, 15, 73, 255, 24, 105, 1, 157, 24, 6, 96, 41, 63, 157,
			48, 6, 157, 56, 6, 96, 41, 128, 10, 42, 157, 96, 6, 165, 253, 41,
			112, 74, 74, 74, 74, 157, 64, 6, 208, 3, 157, 96, 6, 165, 253, 41,
			15, 157, 80, 6, 157, 88, 6, 189, 72, 6, 221, 64, 6, 144, 143, 189,
			64, 6, 240, 2, 233, 1, 157, 72, 6, 96, 132, 250, 134, 251, 160, 25,
			177, 250, 200, 141, 9, 5, 177, 250, 200, 141, 10, 5, 177, 250, 200, 141,
			11, 5, 177, 250, 200, 141, 12, 5, 177, 250, 200, 141, 13, 5, 177, 250,
			141, 27, 5, 165, 250, 73, 128, 48, 1, 232, 141, 172, 15, 142, 173, 15,
			73, 128, 48, 1, 232, 141, 29, 7, 142, 30, 7, 232, 141, 35, 7, 142,
			36, 7, 232, 141, 162, 15, 142, 163, 15, 73, 128, 48, 1, 232, 141, 25,
			5, 141, 215, 6, 141, 79, 13, 141, 148, 13, 142, 26, 5, 142, 221, 6,
			142, 83, 13, 142, 154, 13, 169, 240, 133, 255, 169, 0, 141, 22, 5, 141,
			24, 5, 162, 7, 169, 0, 141, 22, 5, 157, 120, 5, 157, 176, 5, 157,
			32, 5, 157, 40, 5, 157, 48, 5, 157, 48, 210, 157, 32, 210, 157, 16,
			210, 157, 0, 210, 202, 16, 226, 141, 24, 210, 141, 8, 210, 141, 56, 210,
			141, 40, 210, 141, 30, 5, 141, 31, 5, 96, 157, 32, 5, 157, 40, 5,
			157, 48, 5, 157, 176, 5, 96, 152, 157, 208, 5, 41, 240, 157, 216, 5,
			189, 208, 5, 10, 10, 10, 10, 157, 224, 5, 96, 41, 7, 133, 250, 138,
			166, 250, 41, 63, 240, 225, 157, 152, 5, 152, 48, 238, 189, 208, 5, 32,
			117, 15, 169, 0, 157, 120, 5, 185, 255, 255, 240, 190, 157, 136, 5, 133,
			251, 185, 255, 255, 157, 128, 5, 133, 250, 152, 157, 144, 5, 160, 8, 177,
			250, 200, 157, 192, 5, 177, 250, 200, 157, 200, 5, 177, 250, 200, 157, 104,
			6, 177, 250, 200, 157, 112, 6, 177, 250, 200, 157, 120, 6, 177, 250, 200,
			157, 232, 5, 157, 248, 5, 177, 250, 200, 157, 240, 5, 157, 0, 6, 177,
			250, 41, 112, 74, 74, 157, 16, 6, 177, 250, 200, 48, 8, 41, 15, 157,
			24, 6, 76, 9, 16, 41, 15, 73, 255, 24, 105, 1, 157, 24, 6, 177,
			250, 200, 157, 40, 6, 177, 250, 200, 41, 63, 157, 48, 6, 157, 56, 6,
			177, 250, 41, 128, 10, 42, 157, 96, 6, 177, 250, 41, 112, 74, 74, 74,
			74, 157, 64, 6, 208, 3, 157, 96, 6, 177, 250, 136, 41, 15, 157, 80,
			6, 157, 88, 6, 177, 250, 41, 192, 29, 152, 5, 157, 152, 5, 168, 185,
			0, 3, 157, 56, 5, 169, 0, 157, 184, 5, 157, 32, 6, 157, 8, 6,
			157, 72, 6, 157, 128, 6, 157, 136, 6, 157, 144, 6, 169, 1, 157, 120,
			5, 96 };
		static readonly byte[] CiBinaryResource_tmc_obx = { 255, 255, 0, 5, 104, 15, 76, 206, 13, 76, 208, 8, 76, 239, 9, 15,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
			1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2,
			2, 2, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3,
			3, 3, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
			4, 4, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4,
			5, 5, 0, 0, 1, 1, 2, 2, 2, 3, 3, 4, 4, 4, 5, 5,
			6, 6, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
			7, 7, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7,
			7, 8, 0, 1, 1, 2, 2, 3, 4, 4, 5, 5, 6, 7, 7, 8,
			8, 9, 0, 1, 1, 2, 3, 3, 4, 5, 5, 6, 7, 7, 8, 9,
			9, 10, 0, 1, 1, 2, 3, 4, 4, 5, 6, 7, 7, 8, 9, 10,
			10, 11, 0, 1, 2, 2, 3, 4, 5, 6, 6, 7, 8, 9, 10, 10,
			11, 12, 0, 1, 2, 3, 3, 4, 5, 6, 7, 8, 9, 10, 10, 11,
			12, 13, 0, 1, 2, 3, 4, 5, 6, 7, 7, 8, 9, 10, 11, 12,
			13, 14, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
			14, 15, 0, 241, 228, 215, 203, 192, 181, 170, 161, 152, 143, 135, 127, 120,
			114, 107, 101, 95, 90, 85, 80, 75, 71, 67, 63, 60, 56, 53, 50, 47,
			44, 42, 39, 37, 35, 33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18,
			17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2,
			1, 0, 0, 242, 230, 218, 206, 191, 182, 170, 161, 152, 143, 137, 128, 122,
			113, 107, 101, 95, 92, 86, 80, 77, 71, 68, 62, 60, 56, 53, 50, 47,
			45, 42, 40, 37, 35, 33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18,
			17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2,
			1, 0, 0, 255, 241, 228, 216, 202, 192, 181, 171, 162, 153, 142, 135, 127,
			121, 115, 112, 102, 97, 90, 85, 82, 75, 72, 67, 63, 60, 57, 55, 51,
			48, 45, 42, 40, 37, 36, 33, 31, 30, 28, 27, 25, 23, 22, 21, 19,
			18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3,
			2, 1, 0, 243, 230, 217, 204, 193, 181, 173, 162, 153, 144, 136, 128, 121,
			114, 108, 102, 96, 91, 85, 81, 76, 72, 68, 64, 60, 57, 53, 50, 47,
			45, 42, 40, 37, 35, 33, 31, 29, 28, 26, 24, 23, 22, 20, 19, 18,
			17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2,
			1, 0, 0, 242, 51, 150, 226, 56, 140, 0, 106, 232, 106, 239, 128, 8,
			174, 70, 230, 149, 65, 246, 176, 110, 48, 246, 187, 132, 82, 34, 244, 200,
			160, 122, 85, 52, 20, 245, 216, 189, 164, 141, 119, 96, 78, 56, 39, 21,
			6, 247, 232, 219, 207, 195, 184, 172, 162, 154, 144, 136, 127, 120, 112, 106,
			100, 94, 0, 13, 13, 12, 11, 11, 10, 10, 9, 8, 8, 7, 7, 7,
			6, 6, 5, 5, 5, 4, 4, 4, 4, 3, 3, 3, 3, 3, 2, 2,
			2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 5,
			6, 7, 0, 1, 2, 3, 4, 2, 0, 0, 4, 2, 0, 0, 0, 16,
			0, 8, 0, 16, 0, 8, 173, 183, 8, 240, 94, 173, 182, 8, 201, 64,
			144, 90, 206, 181, 8, 240, 3, 76, 239, 9, 162, 7, 169, 0, 157, 196,
			7, 157, 204, 7, 202, 16, 247, 141, 182, 8, 170, 160, 15, 177, 254, 16,
			32, 136, 177, 254, 16, 3, 76, 95, 14, 134, 252, 10, 10, 38, 252, 10,
			38, 252, 10, 38, 252, 105, 0, 133, 254, 165, 252, 105, 0, 133, 255, 144,
			218, 157, 212, 7, 136, 177, 254, 157, 220, 7, 232, 136, 16, 207, 24, 165,
			254, 105, 16, 133, 254, 144, 2, 230, 255, 76, 239, 9, 206, 181, 8, 16,
			248, 238, 182, 8, 173, 180, 8, 141, 181, 8, 162, 7, 222, 204, 7, 48,
			3, 76, 233, 9, 188, 212, 7, 185, 255, 255, 133, 252, 185, 255, 255, 133,
			253, 188, 196, 7, 177, 252, 208, 6, 32, 109, 13, 76, 230, 9, 201, 64,
			176, 18, 125, 220, 7, 157, 228, 7, 32, 109, 13, 188, 42, 5, 32, 188,
			14, 76, 230, 9, 208, 34, 200, 254, 196, 7, 177, 252, 16, 7, 133, 251,
			32, 109, 13, 165, 251, 41, 127, 208, 7, 169, 64, 141, 182, 8, 208, 76,
			141, 180, 8, 141, 181, 8, 208, 68, 201, 128, 176, 43, 41, 63, 125, 220,
			7, 157, 228, 7, 200, 254, 196, 7, 177, 252, 41, 127, 208, 7, 169, 64,
			141, 182, 8, 208, 6, 141, 180, 8, 141, 181, 8, 32, 109, 13, 188, 42,
			5, 32, 188, 14, 76, 230, 9, 201, 192, 176, 12, 41, 63, 157, 42, 5,
			200, 254, 196, 7, 76, 94, 9, 41, 63, 157, 204, 7, 254, 196, 7, 202,
			48, 3, 76, 70, 9, 162, 7, 189, 188, 7, 240, 33, 32, 46, 11, 189,
			50, 5, 61, 192, 8, 240, 22, 160, 71, 177, 252, 24, 125, 34, 5, 157,
			36, 5, 168, 185, 60, 6, 56, 125, 100, 8, 157, 246, 7, 202, 16, 215,
			14, 9, 5, 14, 9, 5, 14, 9, 5, 14, 9, 5, 232, 134, 252, 134,
			253, 162, 7, 138, 168, 185, 252, 7, 208, 12, 188, 184, 8, 185, 4, 8,
			208, 4, 138, 168, 169, 0, 133, 250, 152, 157, 26, 5, 185, 244, 7, 157,
			18, 5, 185, 50, 5, 133, 251, 5, 253, 133, 253, 165, 251, 61, 192, 8,
			240, 6, 185, 246, 7, 157, 20, 5, 165, 251, 61, 200, 8, 240, 18, 185,
			34, 5, 41, 63, 168, 200, 132, 252, 185, 123, 7, 157, 18, 5, 76, 137,
			10, 164, 252, 240, 10, 185, 59, 7, 157, 18, 5, 169, 0, 133, 252, 165,
			250, 13, 9, 5, 168, 185, 60, 5, 188, 26, 5, 25, 236, 7, 157, 10,
			5, 224, 4, 208, 9, 165, 253, 141, 59, 5, 169, 0, 133, 253, 202, 16,
			130, 78, 9, 5, 78, 9, 5, 78, 9, 5, 78, 9, 5, 165, 253, 162,
			3, 142, 31, 210, 142, 15, 210, 174, 22, 5, 172, 18, 5, 142, 16, 210,
			140, 0, 210, 174, 14, 5, 172, 10, 5, 142, 17, 210, 140, 1, 210, 174,
			23, 5, 172, 19, 5, 142, 18, 210, 140, 2, 210, 174, 15, 5, 172, 11,
			5, 142, 19, 210, 140, 3, 210, 174, 24, 5, 172, 20, 5, 142, 20, 210,
			140, 4, 210, 174, 16, 5, 172, 12, 5, 142, 21, 210, 140, 5, 210, 174,
			25, 5, 172, 21, 5, 142, 22, 210, 140, 6, 210, 174, 17, 5, 172, 13,
			5, 142, 23, 210, 140, 7, 210, 141, 58, 5, 174, 59, 5, 142, 24, 210,
			141, 8, 210, 96, 189, 28, 8, 133, 252, 189, 36, 8, 133, 253, 188, 44,
			8, 192, 63, 240, 123, 254, 44, 8, 254, 44, 8, 254, 44, 8, 177, 252,
			41, 240, 157, 236, 7, 177, 252, 41, 15, 56, 253, 12, 8, 16, 2, 169,
			0, 157, 252, 7, 200, 177, 252, 41, 15, 56, 253, 20, 8, 16, 2, 169,
			0, 157, 4, 8, 177, 252, 41, 240, 240, 116, 16, 11, 160, 73, 177, 252,
			188, 44, 8, 136, 136, 16, 2, 169, 0, 157, 50, 5, 177, 252, 41, 112,
			240, 99, 74, 74, 141, 154, 11, 169, 0, 157, 100, 8, 200, 177, 252, 144,
			254, 234, 234, 234, 234, 76, 56, 13, 234, 76, 53, 13, 234, 76, 60, 13,
			234, 76, 74, 13, 234, 76, 84, 13, 234, 76, 95, 13, 234, 76, 81, 13,
			189, 52, 8, 240, 18, 222, 68, 8, 208, 13, 157, 68, 8, 189, 252, 7,
			41, 15, 240, 3, 222, 252, 7, 189, 60, 8, 240, 18, 222, 76, 8, 208,
			13, 157, 76, 8, 189, 4, 8, 41, 15, 240, 3, 222, 4, 8, 160, 72,
			177, 252, 157, 50, 5, 189, 148, 8, 24, 105, 63, 168, 177, 252, 125, 228,
			7, 157, 34, 5, 168, 185, 60, 6, 157, 244, 7, 222, 164, 8, 16, 51,
			189, 156, 8, 157, 164, 8, 189, 172, 8, 240, 24, 24, 125, 148, 8, 157,
			148, 8, 240, 7, 221, 140, 8, 208, 26, 169, 254, 24, 105, 1, 157, 172,
			8, 208, 16, 254, 148, 8, 189, 140, 8, 221, 148, 8, 176, 5, 169, 0,
			157, 148, 8, 189, 116, 8, 240, 4, 222, 116, 8, 96, 189, 108, 8, 133,
			250, 189, 92, 8, 133, 251, 32, 105, 12, 222, 132, 8, 16, 16, 165, 250,
			157, 108, 8, 165, 251, 157, 92, 8, 189, 124, 8, 157, 132, 8, 96, 189,
			84, 8, 141, 112, 12, 16, 254, 76, 167, 12, 234, 76, 144, 12, 234, 76,
			174, 12, 234, 76, 180, 12, 234, 76, 190, 12, 234, 76, 210, 12, 234, 76,
			226, 12, 234, 76, 244, 12, 165, 250, 230, 250, 41, 3, 74, 144, 15, 208,
			71, 165, 251, 157, 100, 8, 24, 125, 244, 7, 157, 244, 7, 96, 169, 0,
			157, 100, 8, 96, 32, 29, 13, 76, 157, 12, 32, 29, 13, 24, 125, 34,
			5, 76, 84, 13, 165, 250, 157, 100, 8, 24, 125, 244, 7, 157, 244, 7,
			165, 250, 24, 101, 251, 133, 250, 96, 189, 34, 5, 56, 229, 250, 157, 34,
			5, 168, 185, 60, 6, 76, 199, 12, 189, 244, 7, 56, 229, 251, 157, 244,
			7, 56, 169, 0, 229, 251, 157, 100, 8, 96, 189, 132, 8, 208, 174, 165,
			251, 16, 16, 189, 4, 8, 240, 165, 189, 252, 7, 201, 15, 240, 158, 254,
			252, 7, 96, 189, 252, 7, 240, 149, 189, 4, 8, 201, 15, 240, 142, 254,
			4, 8, 96, 164, 250, 165, 251, 48, 2, 200, 200, 136, 152, 133, 250, 197,
			251, 208, 6, 165, 251, 73, 255, 133, 251, 152, 96, 125, 244, 7, 157, 244,
			7, 96, 188, 228, 7, 121, 60, 6, 157, 244, 7, 152, 157, 34, 5, 96,
			45, 10, 210, 157, 244, 7, 96, 125, 228, 7, 157, 34, 5, 168, 185, 60,
			6, 157, 244, 7, 96, 157, 34, 5, 168, 189, 244, 7, 121, 60, 6, 157,
			244, 7, 96, 200, 254, 196, 7, 177, 252, 74, 74, 74, 74, 157, 12, 8,
			177, 252, 41, 15, 157, 20, 8, 96, 32, 95, 14, 160, 15, 169, 0, 133,
			254, 169, 0, 133, 255, 138, 240, 46, 177, 254, 16, 1, 202, 24, 165, 254,
			105, 16, 133, 254, 144, 239, 230, 255, 176, 235, 32, 95, 14, 169, 0, 133,
			252, 138, 10, 10, 38, 252, 10, 38, 252, 10, 38, 252, 105, 0, 133, 254,
			165, 252, 105, 0, 133, 255, 169, 64, 141, 182, 8, 169, 1, 141, 181, 8,
			141, 183, 8, 96, 201, 16, 144, 176, 201, 32, 144, 206, 201, 48, 176, 3,
			76, 174, 14, 201, 64, 176, 9, 138, 41, 15, 240, 3, 141, 180, 8, 96,
			201, 80, 144, 113, 201, 96, 176, 6, 169, 0, 141, 183, 8, 96, 201, 112,
			144, 248, 169, 1, 141, 181, 8, 169, 64, 141, 182, 8, 132, 252, 134, 253,
			160, 30, 177, 252, 141, 180, 8, 165, 252, 24, 105, 32, 141, 194, 14, 144,
			1, 232, 142, 195, 14, 24, 105, 64, 141, 202, 14, 144, 1, 232, 142, 203,
			14, 24, 105, 64, 141, 82, 9, 144, 1, 232, 142, 83, 9, 24, 105, 128,
			141, 87, 9, 144, 1, 232, 142, 88, 9, 24, 105, 128, 133, 254, 141, 16,
			9, 141, 136, 13, 141, 183, 13, 144, 1, 232, 134, 255, 142, 22, 9, 142,
			140, 13, 142, 189, 13, 160, 7, 169, 0, 141, 183, 8, 153, 0, 210, 153,
			16, 210, 153, 10, 5, 153, 252, 7, 153, 4, 8, 153, 50, 5, 153, 188,
			7, 136, 16, 232, 141, 8, 210, 141, 24, 210, 141, 58, 5, 141, 59, 5,
			96, 157, 252, 7, 157, 4, 8, 157, 50, 5, 189, 228, 7, 157, 34, 5,
			96, 152, 73, 240, 74, 74, 74, 74, 157, 12, 8, 152, 41, 15, 73, 15,
			157, 20, 8, 96, 41, 7, 133, 252, 138, 166, 252, 41, 63, 240, 226, 157,
			228, 7, 169, 0, 157, 188, 7, 185, 255, 255, 157, 28, 8, 133, 252, 185,
			255, 255, 157, 36, 8, 133, 253, 5, 252, 240, 182, 160, 74, 177, 252, 157,
			52, 8, 157, 68, 8, 200, 177, 252, 157, 60, 8, 157, 76, 8, 200, 177,
			252, 41, 112, 74, 74, 157, 84, 8, 177, 252, 41, 15, 157, 92, 8, 177,
			252, 16, 11, 189, 92, 8, 73, 255, 24, 105, 1, 157, 92, 8, 200, 177,
			252, 157, 116, 8, 200, 177, 252, 41, 63, 157, 124, 8, 157, 132, 8, 200,
			177, 252, 41, 128, 240, 2, 169, 1, 157, 172, 8, 177, 252, 41, 112, 74,
			74, 74, 74, 157, 140, 8, 208, 3, 157, 172, 8, 177, 252, 41, 15, 157,
			156, 8, 157, 164, 8, 136, 177, 252, 41, 192, 24, 125, 228, 7, 157, 228,
			7, 157, 34, 5, 168, 185, 60, 6, 157, 244, 7, 169, 0, 157, 44, 8,
			157, 100, 8, 157, 108, 8, 157, 148, 8, 169, 1, 157, 188, 7, 96 };
	}

	internal enum ASAPModuleType
	{
		SapB,
		SapC,
		SapD,
		SapS,
		Cmc,
		Cm3,
		Cmr,
		Cms,
		Dlt,
		Mpt,
		Rmt,
		Tmc,
		Tm2
	}

	/// <summary>Format of output samples.</summary>
	public enum ASAPSampleFormat
	{
		/// <summary>Unsigned 8-bit.</summary>
		U8,
		/// <summary>Signed 16-bit little-endian.</summary>
		S16LE,
		/// <summary>Signed 16-bit big-endian.</summary>
		S16BE
	}

	internal class Cpu6502
	{
		internal int A;
		internal int C;

		/// <summary>Runs 6502 emulation for the specified number of Atari scanlines.</summary>
		/// <remarks>Each scanline is 114 cycles of which 9 is taken by ANTIC for memory refresh.</remarks>
		internal void DoFrame(ASAP asap, int cycleLimit)
		{
			int pc = this.Pc;
			int nz = this.Nz;
			int a = this.A;
			int x = this.X;
			int y = this.Y;
			int c = this.C;
			int s = this.S;
			int vdi = this.Vdi;
			while (asap.Cycle < cycleLimit) {
				if (asap.Cycle >= asap.NextEventCycle) {
					this.Pc = pc;
					this.S = s;
					asap.HandleEvent();
					pc = this.Pc;
					s = this.S;
					if ((vdi & 4) == 0 && asap.Pokeys.Irqst != 255) {
						asap.Memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						asap.Memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						asap.Memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
						s = s - 1 & 255;
						vdi |= 4;
						pc = asap.Memory[65534] + (asap.Memory[65535] << 8);
						asap.Cycle += 7;
					}
				}
				int data = asap.Memory[pc++];
				asap.Cycle += CiConstArray_1[data];
				int addr;
				switch (data) {
					case 0:
						pc++;
						asap.Memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						asap.Memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						asap.Memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48);
						s = s - 1 & 255;
						vdi |= 4;
						pc = asap.Memory[65534] + (asap.Memory[65535] << 8);
						break;
					case 1:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						nz = a |= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 2:
					case 18:
					case 34:
					case 50:
					case 66:
					case 82:
					case 98:
					case 114:
					case 146:
					case 178:
					case 210:
					case 242:
						pc--;
						asap.Cycle = asap.NextEventCycle;
						break;
					case 5:
						addr = asap.Memory[pc++];
						nz = a |= asap.Memory[addr];
						break;
					case 6:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 8:
						asap.Memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48);
						s = s - 1 & 255;
						break;
					case 9:
						nz = a |= asap.Memory[pc++];
						break;
					case 10:
						c = a >> 7;
						nz = a = a << 1 & 255;
						break;
					case 13:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = a |= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 14:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 16:
						if (nz < 128) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 17:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a |= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 21:
						addr = asap.Memory[pc++] + x & 255;
						nz = a |= asap.Memory[addr];
						break;
					case 22:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 24:
						c = 0;
						break;
					case 25:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a |= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 29:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						nz = a |= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 30:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 32:
						addr = asap.Memory[pc++];
						asap.Memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						asap.Memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						pc = addr + (asap.Memory[pc] << 8);
						break;
					case 33:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						nz = a &= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 36:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						vdi = (vdi & 12) + (nz & 64);
						nz = ((nz & 128) << 1) + (nz & a);
						break;
					case 37:
						addr = asap.Memory[pc++];
						nz = a &= asap.Memory[addr];
						break;
					case 38:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 40:
						s = s + 1 & 255;
						vdi = asap.Memory[256 + s];
						nz = ((vdi & 128) << 1) + (~vdi & 2);
						c = vdi & 1;
						vdi &= 76;
						if ((vdi & 4) == 0 && asap.Pokeys.Irqst != 255) {
							asap.Memory[256 + s] = (byte) (pc >> 8);
							s = s - 1 & 255;
							asap.Memory[256 + s] = (byte) pc;
							s = s - 1 & 255;
							asap.Memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
							s = s - 1 & 255;
							vdi |= 4;
							pc = asap.Memory[65534] + (asap.Memory[65535] << 8);
							asap.Cycle += 7;
						}
						break;
					case 41:
						nz = a &= asap.Memory[pc++];
						break;
					case 42:
						a = (a << 1) + c;
						c = a >> 8;
						nz = a &= 255;
						break;
					case 44:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						vdi = (vdi & 12) + (nz & 64);
						nz = ((nz & 128) << 1) + (nz & a);
						break;
					case 45:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = a &= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 46:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 48:
						if (nz >= 128) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 49:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a &= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 53:
						addr = asap.Memory[pc++] + x & 255;
						nz = a &= asap.Memory[addr];
						break;
					case 54:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 56:
						c = 1;
						break;
					case 57:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a &= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 61:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						nz = a &= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 62:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 64:
						s = s + 1 & 255;
						vdi = asap.Memory[256 + s];
						nz = ((vdi & 128) << 1) + (~vdi & 2);
						c = vdi & 1;
						vdi &= 76;
						s = s + 1 & 255;
						pc = asap.Memory[256 + s];
						s = s + 1 & 255;
						addr = asap.Memory[256 + s];
						pc += addr << 8;
						if ((vdi & 4) == 0 && asap.Pokeys.Irqst != 255) {
							asap.Memory[256 + s] = (byte) (pc >> 8);
							s = s - 1 & 255;
							asap.Memory[256 + s] = (byte) pc;
							s = s - 1 & 255;
							asap.Memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
							s = s - 1 & 255;
							vdi |= 4;
							pc = asap.Memory[65534] + (asap.Memory[65535] << 8);
							asap.Cycle += 7;
						}
						break;
					case 65:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						nz = a ^= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 69:
						addr = asap.Memory[pc++];
						nz = a ^= asap.Memory[addr];
						break;
					case 70:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						break;
					case 72:
						asap.Memory[256 + s] = (byte) a;
						s = s - 1 & 255;
						break;
					case 73:
						nz = a ^= asap.Memory[pc++];
						break;
					case 74:
						c = a & 1;
						nz = a >>= 1;
						break;
					case 76:
						addr = asap.Memory[pc++];
						pc = addr + (asap.Memory[pc] << 8);
						break;
					case 77:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = a ^= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 78:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 80:
						if ((vdi & 64) == 0) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 81:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a ^= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 85:
						addr = asap.Memory[pc++] + x & 255;
						nz = a ^= asap.Memory[addr];
						break;
					case 86:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						break;
					case 88:
						vdi &= 72;
						if ((vdi & 4) == 0 && asap.Pokeys.Irqst != 255) {
							asap.Memory[256 + s] = (byte) (pc >> 8);
							s = s - 1 & 255;
							asap.Memory[256 + s] = (byte) pc;
							s = s - 1 & 255;
							asap.Memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
							s = s - 1 & 255;
							vdi |= 4;
							pc = asap.Memory[65534] + (asap.Memory[65535] << 8);
							asap.Cycle += 7;
						}
						break;
					case 89:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a ^= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 93:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						nz = a ^= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 94:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 96:
						s = s + 1 & 255;
						pc = asap.Memory[256 + s];
						s = s + 1 & 255;
						addr = asap.Memory[256 + s];
						pc += (addr << 8) + 1;
						break;
					case 97:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 101:
						addr = asap.Memory[pc++];
						data = asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 102:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						break;
					case 104:
						s = s + 1 & 255;
						a = asap.Memory[256 + s];
						nz = a;
						break;
					case 105:
						data = asap.Memory[pc++];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 106:
						nz = (c << 7) + (a >> 1);
						c = a & 1;
						a = nz;
						break;
					case 108:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if ((addr & 255) == 255)
							pc = asap.Memory[addr] + (asap.Memory[addr - 255] << 8);
						else
							pc = asap.Memory[addr] + (asap.Memory[addr + 1] << 8);
						break;
					case 109:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 110:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 112:
						if ((vdi & 64) != 0) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 113:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 117:
						addr = asap.Memory[pc++] + x & 255;
						data = asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 118:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						break;
					case 120:
						vdi |= 4;
						break;
					case 121:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 125:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 126:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 129:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, a);
						else
							asap.Memory[addr] = (byte) a;
						break;
					case 132:
						addr = asap.Memory[pc++];
						asap.Memory[addr] = (byte) y;
						break;
					case 133:
						addr = asap.Memory[pc++];
						asap.Memory[addr] = (byte) a;
						break;
					case 134:
						addr = asap.Memory[pc++];
						asap.Memory[addr] = (byte) x;
						break;
					case 136:
						nz = y = y - 1 & 255;
						break;
					case 138:
						nz = a = x;
						break;
					case 140:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, y);
						else
							asap.Memory[addr] = (byte) y;
						break;
					case 141:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, a);
						else
							asap.Memory[addr] = (byte) a;
						break;
					case 142:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, x);
						else
							asap.Memory[addr] = (byte) x;
						break;
					case 144:
						if (c == 0) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 145:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, a);
						else
							asap.Memory[addr] = (byte) a;
						break;
					case 148:
						addr = asap.Memory[pc++] + x & 255;
						asap.Memory[addr] = (byte) y;
						break;
					case 149:
						addr = asap.Memory[pc++] + x & 255;
						asap.Memory[addr] = (byte) a;
						break;
					case 150:
						addr = asap.Memory[pc++] + y & 255;
						asap.Memory[addr] = (byte) x;
						break;
					case 152:
						nz = a = y;
						break;
					case 153:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, a);
						else
							asap.Memory[addr] = (byte) a;
						break;
					case 154:
						s = x;
						break;
					case 157:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, a);
						else
							asap.Memory[addr] = (byte) a;
						break;
					case 160:
						nz = y = asap.Memory[pc++];
						break;
					case 161:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						nz = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 162:
						nz = x = asap.Memory[pc++];
						break;
					case 164:
						addr = asap.Memory[pc++];
						nz = y = asap.Memory[addr];
						break;
					case 165:
						addr = asap.Memory[pc++];
						nz = a = asap.Memory[addr];
						break;
					case 166:
						addr = asap.Memory[pc++];
						nz = x = asap.Memory[addr];
						break;
					case 168:
						nz = y = a;
						break;
					case 169:
						nz = a = asap.Memory[pc++];
						break;
					case 170:
						nz = x = a;
						break;
					case 172:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = y = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 173:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 174:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = x = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 176:
						if (c != 0) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 177:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 180:
						addr = asap.Memory[pc++] + x & 255;
						nz = y = asap.Memory[addr];
						break;
					case 181:
						addr = asap.Memory[pc++] + x & 255;
						nz = a = asap.Memory[addr];
						break;
					case 182:
						addr = asap.Memory[pc++] + y & 255;
						nz = x = asap.Memory[addr];
						break;
					case 184:
						vdi &= 12;
						break;
					case 185:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 186:
						nz = x = s;
						break;
					case 188:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						nz = y = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 189:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						nz = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 190:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = x = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 192:
						nz = asap.Memory[pc++];
						c = y >= nz ? 1 : 0;
						nz = y - nz & 255;
						break;
					case 193:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 196:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = y >= nz ? 1 : 0;
						nz = y - nz & 255;
						break;
					case 197:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 198:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 200:
						nz = y = y + 1 & 255;
						break;
					case 201:
						nz = asap.Memory[pc++];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 202:
						nz = x = x - 1 & 255;
						break;
					case 204:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = y >= nz ? 1 : 0;
						nz = y - nz & 255;
						break;
					case 205:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 206:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 208:
						if ((nz & 255) != 0) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 209:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 213:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 214:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 216:
						vdi &= 68;
						break;
					case 217:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 221:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 222:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 224:
						nz = asap.Memory[pc++];
						c = x >= nz ? 1 : 0;
						nz = x - nz & 255;
						break;
					case 225:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 228:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = x >= nz ? 1 : 0;
						nz = x - nz & 255;
						break;
					case 229:
						addr = asap.Memory[pc++];
						data = asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 230:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 232:
						nz = x = x + 1 & 255;
						break;
					case 233:
					case 235:
						data = asap.Memory[pc++];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 234:
					case 26:
					case 58:
					case 90:
					case 122:
					case 218:
					case 250:
						break;
					case 236:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						c = x >= nz ? 1 : 0;
						nz = x - nz & 255;
						break;
					case 237:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 238:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 240:
						if ((nz & 255) == 0) {
							addr = (sbyte) asap.Memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								asap.Cycle++;
							asap.Cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 241:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 245:
						addr = asap.Memory[pc++] + x & 255;
						data = asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 246:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						asap.Memory[addr] = (byte) nz;
						break;
					case 248:
						vdi |= 8;
						break;
					case 249:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 253:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							asap.Cycle++;
						data = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 254:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						break;
					case 3:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 4:
					case 68:
					case 100:
					case 20:
					case 52:
					case 84:
					case 116:
					case 212:
					case 244:
					case 128:
					case 130:
					case 137:
					case 194:
					case 226:
						pc++;
						break;
					case 7:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 11:
					case 43:
						nz = a &= asap.Memory[pc++];
						c = nz >> 7;
						break;
					case 12:
						pc += 2;
						break;
					case 15:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 19:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 23:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 27:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 28:
					case 60:
					case 92:
					case 124:
					case 220:
					case 252:
						if (asap.Memory[pc++] + x >= 256)
							asap.Cycle++;
						pc++;
						break;
					case 31:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a |= nz;
						break;
					case 35:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 39:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 47:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 51:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 55:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 59:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 63:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a &= nz;
						break;
					case 67:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 71:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 75:
						a &= asap.Memory[pc++];
						c = a & 1;
						nz = a >>= 1;
						break;
					case 79:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 83:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 87:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 91:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 95:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						nz = a ^= nz;
						break;
					case 99:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 103:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 107:
						data = a & asap.Memory[pc++];
						nz = a = (data >> 1) + (c << 7);
						vdi = (vdi & 12) + ((a ^ data) & 64);
						if ((vdi & 8) == 0)
							c = data >> 7;
						else {
							if ((data & 15) >= 5)
								a = (a & 240) + (a + 6 & 15);
							if (data >= 80) {
								a = a + 96 & 255;
								c = 1;
							}
							else
								c = 0;
						}
						break;
					case 111:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 115:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 119:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 123:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 127:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10) {
									tmp += al < 26 ? 6 : -10;
									if (nz != 0)
										nz = (tmp & 128) + 1;
								}
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 131:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						data = a & x;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, data);
						else
							asap.Memory[addr] = (byte) data;
						break;
					case 135:
						addr = asap.Memory[pc++];
						data = a & x;
						asap.Memory[addr] = (byte) data;
						break;
					case 139:
						data = asap.Memory[pc++];
						a &= (data | 239) & x;
						nz = a & data;
						break;
					case 143:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						data = a & x;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, data);
						else
							asap.Memory[addr] = (byte) data;
						break;
					case 147:
						{
							addr = asap.Memory[pc++];
							int hi = asap.Memory[addr + 1 & 255];
							addr = asap.Memory[addr];
							data = hi + 1 & a & x;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								asap.PokeHardware(addr, data);
							else
								asap.Memory[addr] = (byte) data;
						}
						break;
					case 151:
						addr = asap.Memory[pc++] + y & 255;
						data = a & x;
						asap.Memory[addr] = (byte) data;
						break;
					case 155:
						s = a & x;
						{
							addr = asap.Memory[pc++];
							int hi = asap.Memory[pc++];
							data = hi + 1 & s;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								asap.PokeHardware(addr, data);
							else
								asap.Memory[addr] = (byte) data;
						}
						break;
					case 156:
						{
							addr = asap.Memory[pc++];
							int hi = asap.Memory[pc++];
							data = hi + 1 & y;
							addr += x;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								asap.PokeHardware(addr, data);
							else
								asap.Memory[addr] = (byte) data;
						}
						break;
					case 158:
						{
							addr = asap.Memory[pc++];
							int hi = asap.Memory[pc++];
							data = hi + 1 & x;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								asap.PokeHardware(addr, data);
							else
								asap.Memory[addr] = (byte) data;
						}
						break;
					case 159:
						{
							addr = asap.Memory[pc++];
							int hi = asap.Memory[pc++];
							data = hi + 1 & a & x;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								asap.PokeHardware(addr, data);
							else
								asap.Memory[addr] = (byte) data;
						}
						break;
					case 163:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						nz = x = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 167:
						addr = asap.Memory[pc++];
						nz = x = a = asap.Memory[addr];
						break;
					case 171:
						nz = x = a &= asap.Memory[pc++];
						break;
					case 175:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						nz = x = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 179:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = x = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 183:
						addr = asap.Memory[pc++] + y & 255;
						nz = x = a = asap.Memory[addr];
						break;
					case 187:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = x = a = s &= (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 191:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							asap.Cycle++;
						nz = x = a = (addr & 63744) == 53248 ? asap.PeekHardware(addr) : asap.Memory[addr];
						break;
					case 195:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 199:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 203:
						nz = asap.Memory[pc++];
						x &= a;
						c = x >= nz ? 1 : 0;
						nz = x = x - nz & 255;
						break;
					case 207:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 211:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 215:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 219:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 223:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 227:
						addr = asap.Memory[pc++] + x & 255;
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 231:
						addr = asap.Memory[pc++];
						nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 239:
						addr = asap.Memory[pc++];
						addr += asap.Memory[pc++] << 8;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 243:
						addr = asap.Memory[pc++];
						addr = asap.Memory[addr] + (asap.Memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 247:
						addr = asap.Memory[pc++] + x & 255;
						nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 251:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 255:
						addr = asap.Memory[pc++];
						addr = addr + (asap.Memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							asap.Cycle--;
							nz = asap.PeekHardware(addr);
							asap.PokeHardware(addr, nz);
							asap.Cycle++;
						}
						else
							nz = asap.Memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							asap.PokeHardware(addr, nz);
						else
							asap.Memory[addr] = (byte) nz;
						data = nz;
						{
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
				}
			}
			this.Pc = pc;
			this.Nz = nz;
			this.A = a;
			this.X = x;
			this.Y = y;
			this.C = c;
			this.S = s;
			this.Vdi = vdi;
		}
		internal int Nz;
		internal int Pc;
		internal int S;
		internal int Vdi;
		internal int X;
		internal int Y;
		static readonly int[] CiConstArray_1 = { 7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
			2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
			2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
			2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
			2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
			2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
			2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7 };
	}

	internal enum NmiStatus
	{
		Reset,
		OnVBlank,
		WasVBlank
	}

	internal class Pokey
	{

		internal void AddDelta(PokeyPair pokeys, int cycle, int delta)
		{
			this.DeltaBuffer[(cycle * 44100 + pokeys.SampleOffset) / pokeys.MainClock] += delta;
		}
		internal int Audc1;
		internal int Audc2;
		internal int Audc3;
		internal int Audc4;
		internal int Audctl;
		internal int Audf1;
		internal int Audf2;
		internal int Audf3;
		internal int Audf4;
		internal int Delta1;
		internal int Delta2;
		internal int Delta3;
		internal int Delta4;
		internal readonly int[] DeltaBuffer = new int[888];
		internal int DivCycles;

		internal void EndFrame(PokeyPair pokeys, int cycle)
		{
			this.GenerateUntilCycle(pokeys, cycle);
			this.PolyIndex += cycle;
			int m = (this.Audctl & 128) != 0 ? 237615 : 60948015;
			if (this.PolyIndex >= 2 * m)
				this.PolyIndex -= m;
			if (this.TickCycle1 != 8388608)
				this.TickCycle1 -= cycle;
			if (this.TickCycle2 != 8388608)
				this.TickCycle2 -= cycle;
			if (this.TickCycle3 != 8388608)
				this.TickCycle3 -= cycle;
			if (this.TickCycle4 != 8388608)
				this.TickCycle4 -= cycle;
		}

		/// <summary>Fills <c>DeltaBuffer</c> up to <c>cycleLimit</c> basing on current Audf/Audc/AudcTL values.</summary>
		internal void GenerateUntilCycle(PokeyPair pokeys, int cycleLimit)
		{
			for (;;) {
				int cycle = cycleLimit;
				if (cycle > this.TickCycle1)
					cycle = this.TickCycle1;
				if (cycle > this.TickCycle2)
					cycle = this.TickCycle2;
				if (cycle > this.TickCycle3)
					cycle = this.TickCycle3;
				if (cycle > this.TickCycle4)
					cycle = this.TickCycle4;
				if (cycle == cycleLimit)
					break;
				if (cycle == this.TickCycle3) {
					this.TickCycle3 += this.PeriodCycles3;
					if ((this.Audctl & 4) != 0 && this.Delta1 > 0 && this.Mute1 == 0) {
						this.Delta1 = -this.Delta1;
						this.AddDelta(pokeys, cycle, this.Delta1);
					}
					if (this.Init) {
						switch (this.Audc3 >> 4) {
							case 10:
							case 14:
								this.Out3 ^= 1;
								this.Delta3 = -this.Delta3;
								this.AddDelta(pokeys, cycle, this.Delta3);
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + this.PolyIndex - 2;
						int newOut = this.Out3;
						switch (this.Audc3 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((this.Audctl & 128) != 0)
										newOut = pokeys.Poly9Lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newOut ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newOut = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((this.Audctl & 128) != 0)
									newOut = pokeys.Poly9Lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newOut ^= 1;
								break;
							case 12:
								newOut = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newOut != this.Out3) {
							this.Out3 = newOut;
							this.Delta3 = -this.Delta3;
							this.AddDelta(pokeys, cycle, this.Delta3);
						}
					}
				}
				if (cycle == this.TickCycle4) {
					this.TickCycle4 += this.PeriodCycles4;
					if ((this.Audctl & 8) != 0)
						this.TickCycle3 = cycle + this.ReloadCycles3;
					if ((this.Audctl & 2) != 0 && this.Delta2 > 0 && this.Mute2 == 0) {
						this.Delta2 = -this.Delta2;
						this.AddDelta(pokeys, cycle, this.Delta2);
					}
					if (this.Init) {
						switch (this.Audc4 >> 4) {
							case 10:
							case 14:
								this.Out4 ^= 1;
								this.Delta4 = -this.Delta4;
								this.AddDelta(pokeys, cycle, this.Delta4);
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + this.PolyIndex - 3;
						int newOut = this.Out4;
						switch (this.Audc4 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((this.Audctl & 128) != 0)
										newOut = pokeys.Poly9Lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newOut ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newOut = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((this.Audctl & 128) != 0)
									newOut = pokeys.Poly9Lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newOut ^= 1;
								break;
							case 12:
								newOut = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newOut != this.Out4) {
							this.Out4 = newOut;
							this.Delta4 = -this.Delta4;
							this.AddDelta(pokeys, cycle, this.Delta4);
						}
					}
				}
				if (cycle == this.TickCycle1) {
					this.TickCycle1 += this.PeriodCycles1;
					if ((this.Skctl & 136) == 8)
						this.TickCycle2 = cycle + this.PeriodCycles2;
					if (this.Init) {
						switch (this.Audc1 >> 4) {
							case 10:
							case 14:
								this.Out1 ^= 1;
								this.Delta1 = -this.Delta1;
								this.AddDelta(pokeys, cycle, this.Delta1);
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + this.PolyIndex - 0;
						int newOut = this.Out1;
						switch (this.Audc1 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((this.Audctl & 128) != 0)
										newOut = pokeys.Poly9Lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newOut ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newOut = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((this.Audctl & 128) != 0)
									newOut = pokeys.Poly9Lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newOut ^= 1;
								break;
							case 12:
								newOut = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newOut != this.Out1) {
							this.Out1 = newOut;
							this.Delta1 = -this.Delta1;
							this.AddDelta(pokeys, cycle, this.Delta1);
						}
					}
				}
				if (cycle == this.TickCycle2) {
					this.TickCycle2 += this.PeriodCycles2;
					if ((this.Audctl & 16) != 0)
						this.TickCycle1 = cycle + this.ReloadCycles1;
					else if ((this.Skctl & 8) != 0)
						this.TickCycle1 = cycle + this.PeriodCycles1;
					if (this.Init) {
						switch (this.Audc2 >> 4) {
							case 10:
							case 14:
								this.Out2 ^= 1;
								this.Delta2 = -this.Delta2;
								this.AddDelta(pokeys, cycle, this.Delta2);
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + this.PolyIndex - 1;
						int newOut = this.Out2;
						switch (this.Audc2 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((this.Audctl & 128) != 0)
										newOut = pokeys.Poly9Lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newOut ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newOut = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((this.Audctl & 128) != 0)
									newOut = pokeys.Poly9Lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newOut = pokeys.Poly17Lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newOut ^= 1;
								break;
							case 12:
								newOut = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newOut != this.Out2) {
							this.Out2 = newOut;
							this.Delta2 = -this.Delta2;
							this.AddDelta(pokeys, cycle, this.Delta2);
						}
					}
				}
			}
		}
		internal bool Init;

		internal void Initialize()
		{
			this.Audf1 = 0;
			this.Audf2 = 0;
			this.Audf3 = 0;
			this.Audf4 = 0;
			this.Audc1 = 0;
			this.Audc2 = 0;
			this.Audc3 = 0;
			this.Audc4 = 0;
			this.Audctl = 0;
			this.Skctl = 3;
			this.Init = false;
			this.DivCycles = 28;
			this.PeriodCycles1 = 28;
			this.PeriodCycles2 = 28;
			this.PeriodCycles3 = 28;
			this.PeriodCycles4 = 28;
			this.ReloadCycles1 = 28;
			this.ReloadCycles3 = 28;
			this.PolyIndex = 60948015;
			this.TickCycle1 = 8388608;
			this.TickCycle2 = 8388608;
			this.TickCycle3 = 8388608;
			this.TickCycle4 = 8388608;
			this.Mute1 = 1;
			this.Mute2 = 1;
			this.Mute3 = 1;
			this.Mute4 = 1;
			this.Out1 = 0;
			this.Out2 = 0;
			this.Out3 = 0;
			this.Out4 = 0;
			this.Delta1 = 0;
			this.Delta2 = 0;
			this.Delta3 = 0;
			this.Delta4 = 0;
			System.Array.Clear(this.DeltaBuffer, 0, 888);
		}

		internal bool IsSilent()
		{
			return ((this.Audc1 | this.Audc2 | this.Audc3 | this.Audc4) & 15) == 0;
		}

		internal void Mute(int mask)
		{
			if ((mask & 1) != 0) {
				this.Mute1 |= 4;
				this.TickCycle1 = 8388608;
			}
			else {
				this.Mute1 &= ~4;
				if (this.TickCycle1 == 8388608 && this.Mute1 == 0)
					this.TickCycle1 = 0;
			}
			if ((mask & 2) != 0) {
				this.Mute2 |= 4;
				this.TickCycle2 = 8388608;
			}
			else {
				this.Mute2 &= ~4;
				if (this.TickCycle2 == 8388608 && this.Mute2 == 0)
					this.TickCycle2 = 0;
			}
			if ((mask & 4) != 0) {
				this.Mute3 |= 4;
				this.TickCycle3 = 8388608;
			}
			else {
				this.Mute3 &= ~4;
				if (this.TickCycle3 == 8388608 && this.Mute3 == 0)
					this.TickCycle3 = 0;
			}
			if ((mask & 8) != 0) {
				this.Mute4 |= 4;
				this.TickCycle4 = 8388608;
			}
			else {
				this.Mute4 &= ~4;
				if (this.TickCycle4 == 8388608 && this.Mute4 == 0)
					this.TickCycle4 = 0;
			}
		}
		internal int Mute1;
		internal int Mute2;
		internal int Mute3;
		internal int Mute4;
		int Out1;
		int Out2;
		int Out3;
		int Out4;
		internal int PeriodCycles1;
		internal int PeriodCycles2;
		internal int PeriodCycles3;
		internal int PeriodCycles4;
		internal int PolyIndex;
		internal int ReloadCycles1;
		internal int ReloadCycles3;
		internal int Skctl;
		internal int TickCycle1;
		internal int TickCycle2;
		internal int TickCycle3;
		internal int TickCycle4;
		static readonly byte[] CiConstArray_1 = { 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
		static readonly byte[] CiConstArray_2 = { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0,
			1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };
	}

	internal class PokeyPair
	{
		public PokeyPair()
		{
			int reg = 511;
			for (int i = 0; i < 511; i++) {
				reg = (((reg >> 5 ^ reg) & 1) << 8) + (reg >> 1);
				this.Poly9Lookup[i] = (byte) reg;
			}
			reg = 131071;
			for (int i = 0; i < 16385; i++) {
				reg = (((reg >> 5 ^ reg) & 255) << 9) + (reg >> 8);
				this.Poly17Lookup[i] = (byte) (reg >> 1);
			}
		}
		internal readonly Pokey BasePokey = new Pokey();

		internal int EndFrame(int cycle)
		{
			this.BasePokey.EndFrame(this, cycle);
			if (this.ExtraPokeyMask != 0)
				this.ExtraPokey.EndFrame(this, cycle);
			this.SampleOffset += cycle * 44100;
			this.SampleIndex = 0;
			this.Samples = this.SampleOffset / this.MainClock;
			this.SampleOffset %= this.MainClock;
			return this.Samples;
		}
		internal readonly Pokey ExtraPokey = new Pokey();
		internal int ExtraPokeyMask;

		/// <summary>Fills buffer with samples from <c>DeltaBuffer</c>.</summary>
		internal int Generate(byte[] buffer, int bufferOffset, int blocks, ASAPSampleFormat format)
		{
			int i = this.SampleIndex;
			int samples = this.Samples;
			int accLeft = this.IirAccLeft;
			int accRight = this.IirAccRight;
			if (blocks < samples - i)
				samples = i + blocks;
			else
				blocks = samples - i;
			for (; i < samples; i++) {
				accLeft += this.BasePokey.DeltaBuffer[i] - (accLeft * 3 >> 10);
				int sample = accLeft >> 10;
				if (sample < -32767)
					sample = -32767;
				else if (sample > 32767)
					sample = 32767;
				switch (format) {
					case ASAPSampleFormat.U8:
						buffer[bufferOffset++] = (byte) ((sample >> 8) + 128);
						break;
					case ASAPSampleFormat.S16LE:
						buffer[bufferOffset++] = (byte) sample;
						buffer[bufferOffset++] = (byte) (sample >> 8);
						break;
					case ASAPSampleFormat.S16BE:
						buffer[bufferOffset++] = (byte) (sample >> 8);
						buffer[bufferOffset++] = (byte) sample;
						break;
				}
				if (this.ExtraPokeyMask != 0) {
					accRight += this.ExtraPokey.DeltaBuffer[i] - (accRight * 3 >> 10);
					sample = accRight >> 10;
					if (sample < -32767)
						sample = -32767;
					else if (sample > 32767)
						sample = 32767;
					switch (format) {
						case ASAPSampleFormat.U8:
							buffer[bufferOffset++] = (byte) ((sample >> 8) + 128);
							break;
						case ASAPSampleFormat.S16LE:
							buffer[bufferOffset++] = (byte) sample;
							buffer[bufferOffset++] = (byte) (sample >> 8);
							break;
						case ASAPSampleFormat.S16BE:
							buffer[bufferOffset++] = (byte) (sample >> 8);
							buffer[bufferOffset++] = (byte) sample;
							break;
					}
				}
			}
			if (i == this.Samples) {
				accLeft += this.BasePokey.DeltaBuffer[i];
				accRight += this.ExtraPokey.DeltaBuffer[i];
			}
			this.SampleIndex = i;
			this.IirAccLeft = accLeft;
			this.IirAccRight = accRight;
			return blocks;
		}

		internal int GetRandom(int addr, int cycle)
		{
			Pokey pokey = (addr & this.ExtraPokeyMask) != 0 ? this.ExtraPokey : this.BasePokey;
			if (pokey.Init)
				return 255;
			int i = cycle + pokey.PolyIndex;
			if ((pokey.Audctl & 128) != 0)
				return this.Poly9Lookup[i % 511];
			else {
				i %= 131071;
				int j = i >> 3;
				i &= 7;
				return (this.Poly17Lookup[j] >> i) + (this.Poly17Lookup[j + 1] << 8 - i) & 255;
			}
		}
		int IirAccLeft;
		int IirAccRight;

		internal void Initialize(int mainClock, bool stereo)
		{
			this.MainClock = mainClock;
			this.ExtraPokeyMask = stereo ? 16 : 0;
			this.Timer1Cycle = 8388608;
			this.Timer2Cycle = 8388608;
			this.Timer4Cycle = 8388608;
			this.Irqst = 255;
			this.BasePokey.Initialize();
			this.ExtraPokey.Initialize();
			this.SampleOffset = 0;
			this.SampleIndex = 0;
			this.Samples = 0;
			this.IirAccLeft = 0;
			this.IirAccRight = 0;
		}
		internal int Irqst;

		internal bool IsSilent()
		{
			return this.BasePokey.IsSilent() && this.ExtraPokey.IsSilent();
		}
		internal int MainClock;

		internal void Poke(int addr, int data, int cycle)
		{
			Pokey pokey = (addr & this.ExtraPokeyMask) != 0 ? this.ExtraPokey : this.BasePokey;
			switch (addr & 15) {
				case 0:
					if (data == pokey.Audf1)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audf1 = data;
					switch (pokey.Audctl & 80) {
						case 0:
							pokey.PeriodCycles1 = pokey.DivCycles * (data + 1);
							break;
						case 16:
							pokey.PeriodCycles2 = pokey.DivCycles * (data + (pokey.Audf2 << 8) + 1);
							pokey.ReloadCycles1 = pokey.DivCycles * (data + 1);
							if (pokey.PeriodCycles2 <= 112 && (pokey.Audc2 >> 4 == 10 || pokey.Audc2 >> 4 == 14)) {
								pokey.Mute2 |= 1;
								pokey.TickCycle2 = 8388608;
							}
							else {
								pokey.Mute2 &= ~1;
								if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
									pokey.TickCycle2 = cycle;
							}
							break;
						case 64:
							pokey.PeriodCycles1 = data + 4;
							break;
						case 80:
							pokey.PeriodCycles2 = data + (pokey.Audf2 << 8) + 7;
							pokey.ReloadCycles1 = data + 4;
							if (pokey.PeriodCycles2 <= 112 && (pokey.Audc2 >> 4 == 10 || pokey.Audc2 >> 4 == 14)) {
								pokey.Mute2 |= 1;
								pokey.TickCycle2 = 8388608;
							}
							else {
								pokey.Mute2 &= ~1;
								if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
									pokey.TickCycle2 = cycle;
							}
							break;
					}
					if (pokey.PeriodCycles1 <= 112 && (pokey.Audc1 >> 4 == 10 || pokey.Audc1 >> 4 == 14)) {
						pokey.Mute1 |= 1;
						pokey.TickCycle1 = 8388608;
					}
					else {
						pokey.Mute1 &= ~1;
						if (pokey.TickCycle1 == 8388608 && pokey.Mute1 == 0)
							pokey.TickCycle1 = cycle;
					}
					break;
				case 1:
					if (data == pokey.Audc1)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audc1 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pokey.Mute1 & 4) == 0)
							pokey.AddDelta(this, cycle, pokey.Delta1 > 0 ? data - pokey.Delta1 : data);
						pokey.Delta1 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pokey.PeriodCycles1 <= 112 && (pokey.Audc1 >> 4 == 10 || pokey.Audc1 >> 4 == 14)) {
							pokey.Mute1 |= 1;
							pokey.TickCycle1 = 8388608;
						}
						else {
							pokey.Mute1 &= ~1;
							if (pokey.TickCycle1 == 8388608 && pokey.Mute1 == 0)
								pokey.TickCycle1 = cycle;
						}
						if (pokey.Delta1 > 0) {
							if ((pokey.Mute1 & 4) == 0)
								pokey.AddDelta(this, cycle, data - pokey.Delta1);
							pokey.Delta1 = data;
						}
						else
							pokey.Delta1 = -data;
					}
					break;
				case 2:
					if (data == pokey.Audf2)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audf2 = data;
					switch (pokey.Audctl & 80) {
						case 0:
						case 64:
							pokey.PeriodCycles2 = pokey.DivCycles * (data + 1);
							break;
						case 16:
							pokey.PeriodCycles2 = pokey.DivCycles * (pokey.Audf1 + (data << 8) + 1);
							break;
						case 80:
							pokey.PeriodCycles2 = pokey.Audf1 + (data << 8) + 7;
							break;
					}
					if (pokey.PeriodCycles2 <= 112 && (pokey.Audc2 >> 4 == 10 || pokey.Audc2 >> 4 == 14)) {
						pokey.Mute2 |= 1;
						pokey.TickCycle2 = 8388608;
					}
					else {
						pokey.Mute2 &= ~1;
						if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
							pokey.TickCycle2 = cycle;
					}
					break;
				case 3:
					if (data == pokey.Audc2)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audc2 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pokey.Mute2 & 4) == 0)
							pokey.AddDelta(this, cycle, pokey.Delta2 > 0 ? data - pokey.Delta2 : data);
						pokey.Delta2 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pokey.PeriodCycles2 <= 112 && (pokey.Audc2 >> 4 == 10 || pokey.Audc2 >> 4 == 14)) {
							pokey.Mute2 |= 1;
							pokey.TickCycle2 = 8388608;
						}
						else {
							pokey.Mute2 &= ~1;
							if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
								pokey.TickCycle2 = cycle;
						}
						if (pokey.Delta2 > 0) {
							if ((pokey.Mute2 & 4) == 0)
								pokey.AddDelta(this, cycle, data - pokey.Delta2);
							pokey.Delta2 = data;
						}
						else
							pokey.Delta2 = -data;
					}
					break;
				case 4:
					if (data == pokey.Audf3)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audf3 = data;
					switch (pokey.Audctl & 40) {
						case 0:
							pokey.PeriodCycles3 = pokey.DivCycles * (data + 1);
							break;
						case 8:
							pokey.PeriodCycles4 = pokey.DivCycles * (data + (pokey.Audf4 << 8) + 1);
							pokey.ReloadCycles3 = pokey.DivCycles * (data + 1);
							if (pokey.PeriodCycles4 <= 112 && (pokey.Audc4 >> 4 == 10 || pokey.Audc4 >> 4 == 14)) {
								pokey.Mute4 |= 1;
								pokey.TickCycle4 = 8388608;
							}
							else {
								pokey.Mute4 &= ~1;
								if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
									pokey.TickCycle4 = cycle;
							}
							break;
						case 32:
							pokey.PeriodCycles3 = data + 4;
							break;
						case 40:
							pokey.PeriodCycles4 = data + (pokey.Audf4 << 8) + 7;
							pokey.ReloadCycles3 = data + 4;
							if (pokey.PeriodCycles4 <= 112 && (pokey.Audc4 >> 4 == 10 || pokey.Audc4 >> 4 == 14)) {
								pokey.Mute4 |= 1;
								pokey.TickCycle4 = 8388608;
							}
							else {
								pokey.Mute4 &= ~1;
								if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
									pokey.TickCycle4 = cycle;
							}
							break;
					}
					if (pokey.PeriodCycles3 <= 112 && (pokey.Audc3 >> 4 == 10 || pokey.Audc3 >> 4 == 14)) {
						pokey.Mute3 |= 1;
						pokey.TickCycle3 = 8388608;
					}
					else {
						pokey.Mute3 &= ~1;
						if (pokey.TickCycle3 == 8388608 && pokey.Mute3 == 0)
							pokey.TickCycle3 = cycle;
					}
					break;
				case 5:
					if (data == pokey.Audc3)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audc3 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pokey.Mute3 & 4) == 0)
							pokey.AddDelta(this, cycle, pokey.Delta3 > 0 ? data - pokey.Delta3 : data);
						pokey.Delta3 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pokey.PeriodCycles3 <= 112 && (pokey.Audc3 >> 4 == 10 || pokey.Audc3 >> 4 == 14)) {
							pokey.Mute3 |= 1;
							pokey.TickCycle3 = 8388608;
						}
						else {
							pokey.Mute3 &= ~1;
							if (pokey.TickCycle3 == 8388608 && pokey.Mute3 == 0)
								pokey.TickCycle3 = cycle;
						}
						if (pokey.Delta3 > 0) {
							if ((pokey.Mute3 & 4) == 0)
								pokey.AddDelta(this, cycle, data - pokey.Delta3);
							pokey.Delta3 = data;
						}
						else
							pokey.Delta3 = -data;
					}
					break;
				case 6:
					if (data == pokey.Audf4)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audf4 = data;
					switch (pokey.Audctl & 40) {
						case 0:
						case 32:
							pokey.PeriodCycles4 = pokey.DivCycles * (data + 1);
							break;
						case 8:
							pokey.PeriodCycles4 = pokey.DivCycles * (pokey.Audf3 + (data << 8) + 1);
							break;
						case 40:
							pokey.PeriodCycles4 = pokey.Audf3 + (data << 8) + 7;
							break;
					}
					if (pokey.PeriodCycles4 <= 112 && (pokey.Audc4 >> 4 == 10 || pokey.Audc4 >> 4 == 14)) {
						pokey.Mute4 |= 1;
						pokey.TickCycle4 = 8388608;
					}
					else {
						pokey.Mute4 &= ~1;
						if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
							pokey.TickCycle4 = cycle;
					}
					break;
				case 7:
					if (data == pokey.Audc4)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audc4 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pokey.Mute4 & 4) == 0)
							pokey.AddDelta(this, cycle, pokey.Delta4 > 0 ? data - pokey.Delta4 : data);
						pokey.Delta4 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pokey.PeriodCycles4 <= 112 && (pokey.Audc4 >> 4 == 10 || pokey.Audc4 >> 4 == 14)) {
							pokey.Mute4 |= 1;
							pokey.TickCycle4 = 8388608;
						}
						else {
							pokey.Mute4 &= ~1;
							if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
								pokey.TickCycle4 = cycle;
						}
						if (pokey.Delta4 > 0) {
							if ((pokey.Mute4 & 4) == 0)
								pokey.AddDelta(this, cycle, data - pokey.Delta4);
							pokey.Delta4 = data;
						}
						else
							pokey.Delta4 = -data;
					}
					break;
				case 8:
					if (data == pokey.Audctl)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Audctl = data;
					pokey.DivCycles = (data & 1) != 0 ? 114 : 28;
					switch (data & 80) {
						case 0:
							pokey.PeriodCycles1 = pokey.DivCycles * (pokey.Audf1 + 1);
							pokey.PeriodCycles2 = pokey.DivCycles * (pokey.Audf2 + 1);
							break;
						case 16:
							pokey.PeriodCycles1 = pokey.DivCycles << 8;
							pokey.PeriodCycles2 = pokey.DivCycles * (pokey.Audf1 + (pokey.Audf2 << 8) + 1);
							pokey.ReloadCycles1 = pokey.DivCycles * (pokey.Audf1 + 1);
							break;
						case 64:
							pokey.PeriodCycles1 = pokey.Audf1 + 4;
							pokey.PeriodCycles2 = pokey.DivCycles * (pokey.Audf2 + 1);
							break;
						case 80:
							pokey.PeriodCycles1 = 256;
							pokey.PeriodCycles2 = pokey.Audf1 + (pokey.Audf2 << 8) + 7;
							pokey.ReloadCycles1 = pokey.Audf1 + 4;
							break;
					}
					if (pokey.PeriodCycles1 <= 112 && (pokey.Audc1 >> 4 == 10 || pokey.Audc1 >> 4 == 14)) {
						pokey.Mute1 |= 1;
						pokey.TickCycle1 = 8388608;
					}
					else {
						pokey.Mute1 &= ~1;
						if (pokey.TickCycle1 == 8388608 && pokey.Mute1 == 0)
							pokey.TickCycle1 = cycle;
					}
					if (pokey.PeriodCycles2 <= 112 && (pokey.Audc2 >> 4 == 10 || pokey.Audc2 >> 4 == 14)) {
						pokey.Mute2 |= 1;
						pokey.TickCycle2 = 8388608;
					}
					else {
						pokey.Mute2 &= ~1;
						if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
							pokey.TickCycle2 = cycle;
					}
					switch (data & 40) {
						case 0:
							pokey.PeriodCycles3 = pokey.DivCycles * (pokey.Audf3 + 1);
							pokey.PeriodCycles4 = pokey.DivCycles * (pokey.Audf4 + 1);
							break;
						case 8:
							pokey.PeriodCycles3 = pokey.DivCycles << 8;
							pokey.PeriodCycles4 = pokey.DivCycles * (pokey.Audf3 + (pokey.Audf4 << 8) + 1);
							pokey.ReloadCycles3 = pokey.DivCycles * (pokey.Audf3 + 1);
							break;
						case 32:
							pokey.PeriodCycles3 = pokey.Audf3 + 4;
							pokey.PeriodCycles4 = pokey.DivCycles * (pokey.Audf4 + 1);
							break;
						case 40:
							pokey.PeriodCycles3 = 256;
							pokey.PeriodCycles4 = pokey.Audf3 + (pokey.Audf4 << 8) + 7;
							pokey.ReloadCycles3 = pokey.Audf3 + 4;
							break;
					}
					if (pokey.PeriodCycles3 <= 112 && (pokey.Audc3 >> 4 == 10 || pokey.Audc3 >> 4 == 14)) {
						pokey.Mute3 |= 1;
						pokey.TickCycle3 = 8388608;
					}
					else {
						pokey.Mute3 &= ~1;
						if (pokey.TickCycle3 == 8388608 && pokey.Mute3 == 0)
							pokey.TickCycle3 = cycle;
					}
					if (pokey.PeriodCycles4 <= 112 && (pokey.Audc4 >> 4 == 10 || pokey.Audc4 >> 4 == 14)) {
						pokey.Mute4 |= 1;
						pokey.TickCycle4 = 8388608;
					}
					else {
						pokey.Mute4 &= ~1;
						if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
							pokey.TickCycle4 = cycle;
					}
					if (pokey.Init && (data & 64) == 0) {
						pokey.Mute1 |= 2;
						pokey.TickCycle1 = 8388608;
					}
					else {
						pokey.Mute1 &= ~2;
						if (pokey.TickCycle1 == 8388608 && pokey.Mute1 == 0)
							pokey.TickCycle1 = cycle;
					}
					if (pokey.Init && (data & 80) != 80) {
						pokey.Mute2 |= 2;
						pokey.TickCycle2 = 8388608;
					}
					else {
						pokey.Mute2 &= ~2;
						if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
							pokey.TickCycle2 = cycle;
					}
					if (pokey.Init && (data & 32) == 0) {
						pokey.Mute3 |= 2;
						pokey.TickCycle3 = 8388608;
					}
					else {
						pokey.Mute3 &= ~2;
						if (pokey.TickCycle3 == 8388608 && pokey.Mute3 == 0)
							pokey.TickCycle3 = cycle;
					}
					if (pokey.Init && (data & 40) != 40) {
						pokey.Mute4 |= 2;
						pokey.TickCycle4 = 8388608;
					}
					else {
						pokey.Mute4 &= ~2;
						if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
							pokey.TickCycle4 = cycle;
					}
					break;
				case 9:
					if (pokey.TickCycle1 != 8388608)
						pokey.TickCycle1 = cycle + pokey.PeriodCycles1;
					if (pokey.TickCycle2 != 8388608)
						pokey.TickCycle2 = cycle + pokey.PeriodCycles2;
					if (pokey.TickCycle3 != 8388608)
						pokey.TickCycle3 = cycle + pokey.PeriodCycles3;
					if (pokey.TickCycle4 != 8388608)
						pokey.TickCycle4 = cycle + pokey.PeriodCycles4;
					break;
				case 15:
					if (data == pokey.Skctl)
						break;
					pokey.GenerateUntilCycle(this, cycle);
					pokey.Skctl = data;
					bool init = (data & 3) == 0;
					if (pokey.Init && !init)
						pokey.PolyIndex = ((pokey.Audctl & 128) != 0 ? 237614 : 60948014) - cycle;
					pokey.Init = init;
					if (pokey.Init && (pokey.Audctl & 64) == 0) {
						pokey.Mute1 |= 2;
						pokey.TickCycle1 = 8388608;
					}
					else {
						pokey.Mute1 &= ~2;
						if (pokey.TickCycle1 == 8388608 && pokey.Mute1 == 0)
							pokey.TickCycle1 = cycle;
					}
					if (pokey.Init && (pokey.Audctl & 80) != 80) {
						pokey.Mute2 |= 2;
						pokey.TickCycle2 = 8388608;
					}
					else {
						pokey.Mute2 &= ~2;
						if (pokey.TickCycle2 == 8388608 && pokey.Mute2 == 0)
							pokey.TickCycle2 = cycle;
					}
					if (pokey.Init && (pokey.Audctl & 32) == 0) {
						pokey.Mute3 |= 2;
						pokey.TickCycle3 = 8388608;
					}
					else {
						pokey.Mute3 &= ~2;
						if (pokey.TickCycle3 == 8388608 && pokey.Mute3 == 0)
							pokey.TickCycle3 = cycle;
					}
					if (pokey.Init && (pokey.Audctl & 40) != 40) {
						pokey.Mute4 |= 2;
						pokey.TickCycle4 = 8388608;
					}
					else {
						pokey.Mute4 &= ~2;
						if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
							pokey.TickCycle4 = cycle;
					}
					if ((data & 16) != 0) {
						pokey.Mute3 |= 8;
						pokey.TickCycle3 = 8388608;
					}
					else {
						pokey.Mute3 &= ~8;
						if (pokey.TickCycle3 == 8388608 && pokey.Mute3 == 0)
							pokey.TickCycle3 = cycle;
					}
					if ((data & 16) != 0) {
						pokey.Mute4 |= 8;
						pokey.TickCycle4 = 8388608;
					}
					else {
						pokey.Mute4 &= ~8;
						if (pokey.TickCycle4 == 8388608 && pokey.Mute4 == 0)
							pokey.TickCycle4 = cycle;
					}
					break;
				default:
					break;
			}
		}
		internal readonly byte[] Poly17Lookup = new byte[16385];
		internal readonly byte[] Poly9Lookup = new byte[511];
		internal int SampleIndex;
		internal int SampleOffset;
		internal int Samples;

		internal void StartFrame()
		{
			System.Array.Clear(this.BasePokey.DeltaBuffer, 0, 888);
			if (this.ExtraPokeyMask != 0)
				System.Array.Clear(this.ExtraPokey.DeltaBuffer, 0, 888);
		}
		internal int Timer1Cycle;
		internal int Timer2Cycle;
		internal int Timer4Cycle;
	}
}
