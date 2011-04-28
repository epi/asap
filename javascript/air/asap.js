// Generated automatically with "cito". Do not edit.

function ASAP()
{
	this.blocksPlayed = 0;
	this.consol = 0;
	this.covox = new Array(4);
	this.cpu = new Cpu6502();
	this.currentDuration = 0;
	this.currentSong = 0;
	this.cycle = 0;
	this.memory = new Array(65536);
	this.moduleInfo = new ASAPInfo();
	this.nextEventCycle = 0;
	this.nextPlayerCycle = 0;
	this.nextScanlineCycle = 0;
	this.nmist = NmiStatus.RESET;
	this.pokeys = new PokeyPair();
	this.silenceCycles = 0;
	this.silenceCyclesCounter = 0;
	this.tmcPerFrameCounter = 0;
}

ASAP.prototype.call6502 = function(addr) {
	this.memory[53760] = 32;
	this.memory[53761] = addr & 0xff;
	this.memory[53762] = addr >> 8;
	this.memory[53763] = 210;
	this.cpu.pc = 53760;
}

ASAP.prototype.call6502Player = function() {
	var player = this.moduleInfo.player;
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
				this.memory[256 + this.cpu.s] = this.cpu.pc >> 8;
				this.cpu.s = this.cpu.s - 1 & 255;
				this.memory[256 + this.cpu.s] = this.cpu.pc & 0xff;
				this.cpu.s = this.cpu.s - 1 & 255;
				this.memory[53760] = 8;
				this.memory[53761] = 72;
				this.memory[53762] = 138;
				this.memory[53763] = 72;
				this.memory[53764] = 138;
				this.memory[53765] = 72;
				this.memory[53766] = 32;
				this.memory[53767] = player & 0xff;
				this.memory[53768] = player >> 8;
				this.memory[53769] = 104;
				this.memory[53770] = 168;
				this.memory[53771] = 104;
				this.memory[53772] = 170;
				this.memory[53773] = 104;
				this.memory[53774] = 64;
				this.cpu.pc = 53760;
			}
			break;
		case ASAPModuleType.SAP_S:
			var i = this.memory[69] - 1;
			this.memory[69] = i & 0xff;
			if (i == 0)
				this.memory[45179] = this.memory[45179] + 1 & 0xff;
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
				this.tmcPerFrameCounter = this.memory[this.moduleInfo.music + 31];
				this.call6502(player + 3);
			}
			else
				this.call6502(player + 6);
			break;
	}
}

ASAP.prototype.detectSilence = function(seconds) {
	this.silenceCycles = seconds * this.pokeys.mainClock;
}

ASAP.prototype.do6502Frame = function() {
	this.nextEventCycle = 0;
	this.nextScanlineCycle = 0;
	this.nmist = this.nmist == NmiStatus.RESET ? NmiStatus.ON_V_BLANK : NmiStatus.WAS_V_BLANK;
	var cycles = this.moduleInfo.ntsc ? 29868 : 35568;
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

ASAP.prototype.do6502Init = function(pc, a, x, y) {
	this.cpu.pc = pc;
	this.cpu.a = a & 255;
	this.cpu.x = x & 255;
	this.cpu.y = y & 255;
	this.memory[53760] = 210;
	this.memory[510] = 255;
	this.memory[511] = 209;
	this.cpu.s = 253;
	for (var frame = 0; frame < 50; frame++) {
		this.do6502Frame();
		if (this.cpu.pc == 53760)
			return;
	}
	throw "INIT routine didn't return";
}

ASAP.prototype.doFrame = function() {
	this.pokeys.startFrame();
	var cycles = this.do6502Frame();
	this.pokeys.endFrame(cycles);
	return cycles;
}

ASAP.prototype.generate = function(buffer, bufferLen, format) {
	return this.generateAt(buffer, 0, bufferLen, format);
}

ASAP.prototype.generateAt = function(buffer, bufferOffset, bufferLen, format) {
	if (this.silenceCycles > 0 && this.silenceCyclesCounter <= 0)
		return 0;
	var blockShift = 0;
	var bufferBlocks = bufferLen >> blockShift;
	if (this.currentDuration > 0) {
		var totalBlocks = ASAP.millisecondsToBlocks(this.currentDuration);
		if (bufferBlocks > totalBlocks - this.blocksPlayed)
			bufferBlocks = totalBlocks - this.blocksPlayed;
	}
	var block = 0;
	for (;;) {
		var blocks = this.pokeys.generate(buffer, bufferOffset + (block << blockShift), bufferBlocks - block, format);
		this.blocksPlayed += blocks;
		block += blocks;
		if (block >= bufferBlocks)
			break;
		var cycles = this.doFrame();
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

ASAP.prototype.getBlocksPlayed = function() {
	return this.blocksPlayed;
}

ASAP.prototype.getInfo = function() {
	return this.moduleInfo;
}

ASAP.prototype.getPokeyChannelVolume = function(channel) {
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

ASAP.prototype.getPosition = function() {
	return Math.floor(this.blocksPlayed * 10 / 441);
}

ASAP.prototype.getWavHeader = function(buffer, format) {
	var use16bit = format != ASAPSampleFormat.U8 ? 1 : 0;
	var blockSize = this.moduleInfo.channels << use16bit;
	var bytesPerSecond = 44100 * blockSize;
	var totalBlocks = ASAP.millisecondsToBlocks(this.currentDuration);
	var nBytes = (totalBlocks - this.blocksPlayed) * blockSize;
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
	buffer[22] = this.moduleInfo.channels;
	buffer[23] = 0;
	ASAP.putLittleEndian(buffer, 24, 44100);
	ASAP.putLittleEndian(buffer, 28, bytesPerSecond);
	buffer[32] = blockSize;
	buffer[33] = 0;
	buffer[34] = 8 << use16bit;
	buffer[35] = 0;
	buffer[36] = 100;
	buffer[37] = 97;
	buffer[38] = 116;
	buffer[39] = 97;
	ASAP.putLittleEndian(buffer, 40, nBytes);
}

ASAP.prototype.handleEvent = function() {
	var cycle = this.cycle;
	if (cycle >= this.nextScanlineCycle) {
		if (cycle - this.nextScanlineCycle < 50)
			this.cycle = cycle += 9;
		this.nextScanlineCycle += 114;
		if (cycle >= this.nextPlayerCycle) {
			this.call6502Player();
			this.nextPlayerCycle += 114 * this.moduleInfo.fastplay;
		}
	}
	var nextEventCycle = this.nextScanlineCycle;
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

ASAP.prototype.load = function(filename, module, moduleLen) {
	this.silenceCycles = 0;
	this.moduleInfo.load(filename, module, moduleLen);
	var playerRoutine = ASAP6502.getPlayerRoutine(this.moduleInfo);
	if (playerRoutine != null) {
		var player = ASAPInfo.getWord(playerRoutine, 2);
		var playerLastByte = ASAPInfo.getWord(playerRoutine, 4);
		if (this.moduleInfo.music <= playerLastByte)
			throw "Module address conflicts with the player routine";
		this.memory[19456] = 0;
		Ci.copyArray(module, 6, this.memory, this.moduleInfo.music, moduleLen - 6);
		Ci.copyArray(playerRoutine, 6, this.memory, player, playerLastByte + 1 - player);
		if (this.moduleInfo.player < 0)
			this.moduleInfo.player = player;
		return;
	}
	Ci.clearArray(this.memory, 0);
	var moduleIndex = this.moduleInfo.headerLen + 2;
	while (moduleIndex + 5 <= moduleLen) {
		var startAddr = ASAPInfo.getWord(module, moduleIndex);
		var blockLen = ASAPInfo.getWord(module, moduleIndex + 2) + 1 - startAddr;
		if (blockLen <= 0 || moduleIndex + blockLen > moduleLen)
			throw "Invalid binary block";
		moduleIndex += 4;
		Ci.copyArray(module, moduleIndex, this.memory, startAddr, blockLen);
		moduleIndex += blockLen;
		if (moduleIndex == moduleLen)
			return;
		if (moduleIndex + 7 <= moduleLen && module[moduleIndex] == 255 && module[moduleIndex + 1] == 255)
			moduleIndex += 2;
	}
	throw "Invalid binary block";
}

ASAP.millisecondsToBlocks = function(milliseconds) {
	return Math.floor(milliseconds * 441 / 10);
}

ASAP.prototype.mutePokeyChannels = function(mask) {
	this.pokeys.basePokey.mute(mask);
	this.pokeys.extraPokey.mute(mask >> 4);
}

ASAP.prototype.peekHardware = function(addr) {
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
			return Math.floor(this.cycle / 228);
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
			return this.memory[addr];
	}
}

ASAP.prototype.playSong = function(song, duration) {
	if (song < 0 || song >= this.moduleInfo.songs)
		throw "Song number out of range";
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
	this.covox[0] = 128;
	this.covox[1] = 128;
	this.covox[2] = 128;
	this.covox[3] = 128;
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
			this.do6502Init(this.moduleInfo.player + 256, 0, 0, this.moduleInfo.songPos[song]);
			break;
		case ASAPModuleType.MPT:
			this.do6502Init(this.moduleInfo.player, 0, this.moduleInfo.music >> 8, this.moduleInfo.music);
			this.do6502Init(this.moduleInfo.player, 2, this.moduleInfo.songPos[song], 0);
			break;
		case ASAPModuleType.RMT:
			this.do6502Init(this.moduleInfo.player, this.moduleInfo.songPos[song], this.moduleInfo.music, this.moduleInfo.music >> 8);
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

ASAP.prototype.pokeHardware = function(addr, data) {
	if (addr >> 8 == 210) {
		if ((addr & this.pokeys.extraPokeyMask + 15) == 14) {
			this.pokeys.irqst |= data ^ 255;
			if ((data & this.pokeys.irqst & 1) != 0) {
				if (this.pokeys.timer1Cycle == 8388608) {
					var t = this.pokeys.basePokey.tickCycle1;
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
					var t = this.pokeys.basePokey.tickCycle2;
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
					var t = this.pokeys.basePokey.tickCycle4;
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
		var x = this.cycle % 114;
		this.cycle += (x <= 106 ? 106 : 220) - x;
	}
	else if ((addr & 65295) == 54287) {
		this.nmist = this.cycle < 28292 ? NmiStatus.ON_V_BLANK : NmiStatus.RESET;
	}
	else if ((addr & 65280) == this.moduleInfo.covoxAddr) {
		var pokey;
		addr &= 3;
		if (addr == 0 || addr == 3)
			pokey = this.pokeys.basePokey;
		else
			pokey = this.pokeys.extraPokey;
		pokey.addDelta(this.pokeys, this.cycle, data - this.covox[addr] << 17);
		this.covox[addr] = data;
	}
	else if ((addr & 65311) == 53279) {
		data &= 8;
		var delta = this.consol - data << 20;
		this.pokeys.basePokey.addDelta(this.pokeys, this.cycle, delta);
		this.pokeys.extraPokey.addDelta(this.pokeys, this.cycle, delta);
		this.consol = data;
	}
	else
		this.memory[addr] = data;
}

ASAP.putLittleEndian = function(buffer, offset, value) {
	buffer[offset] = value & 0xff;
	buffer[offset + 1] = value >> 8 & 0xff;
	buffer[offset + 2] = value >> 16 & 0xff;
	buffer[offset + 3] = value >> 24 & 0xff;
}
ASAP.SAMPLE_RATE = 44100;

ASAP.prototype.seek = function(position) {
	var block = ASAP.millisecondsToBlocks(position);
	if (block < this.blocksPlayed)
		this.playSong(this.currentSong, this.currentDuration);
	while (this.blocksPlayed + this.pokeys.samples < block) {
		this.blocksPlayed += this.pokeys.samples;
		this.doFrame();
	}
	this.pokeys.sampleIndex = block - this.blocksPlayed;
	this.blocksPlayed = block;
}
ASAP.WAV_HEADER_LENGTH = 44;

function ASAP6502()
{
}

ASAP6502.getPlayerRoutine = function(info) {
	switch (info.type) {
		case ASAPModuleType.CMC:
			return ASAP6502.CI_BINARY_RESOURCE_CMC_OBX;
		case ASAPModuleType.CM3:
			return ASAP6502.CI_BINARY_RESOURCE_CM3_OBX;
		case ASAPModuleType.CMR:
			return ASAP6502.CI_BINARY_RESOURCE_CMR_OBX;
		case ASAPModuleType.CMS:
			return ASAP6502.CI_BINARY_RESOURCE_CMS_OBX;
		case ASAPModuleType.DLT:
			return ASAP6502.CI_BINARY_RESOURCE_DLT_OBX;
		case ASAPModuleType.MPT:
			return ASAP6502.CI_BINARY_RESOURCE_MPT_OBX;
		case ASAPModuleType.RMT:
			return info.channels == 1 ? ASAP6502.CI_BINARY_RESOURCE_RMT4_OBX : ASAP6502.CI_BINARY_RESOURCE_RMT8_OBX;
		case ASAPModuleType.TMC:
			return ASAP6502.CI_BINARY_RESOURCE_TMC_OBX;
		case ASAPModuleType.TM2:
			return ASAP6502.CI_BINARY_RESOURCE_TM2_OBX;
		default:
			return null;
	}
}
ASAP6502.CI_BINARY_RESOURCE_CM3_OBX = [ 255, 255, 0, 5, 223, 12, 76, 18, 11, 76, 120, 5, 76, 203, 7, 0,
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
	3, 3, 7, 11, 15, 19 ];
ASAP6502.CI_BINARY_RESOURCE_CMC_OBX = [ 255, 255, 0, 5, 220, 12, 76, 15, 11, 76, 120, 5, 76, 203, 7, 0,
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
	11, 15, 19 ];
ASAP6502.CI_BINARY_RESOURCE_CMR_OBX = [ 255, 255, 0, 5, 220, 12, 76, 15, 11, 76, 120, 5, 76, 203, 7, 0,
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
	122, 113, 107, 101, 95, 92, 86, 80, 77, 71, 68, 65, 62, 56, 53, 136,
	127, 121, 115, 108, 103, 96, 90, 85, 81, 76, 72, 67, 63, 61, 57, 52,
	51, 48, 45, 42, 40, 37, 36, 33, 31, 30, 5, 4, 3, 2, 1, 0,
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
	11, 15, 19 ];
ASAP6502.CI_BINARY_RESOURCE_CMS_OBX = [ 255, 255, 0, 5, 186, 15, 234, 234, 234, 76, 21, 8, 76, 92, 15, 35,
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
	96 ];
ASAP6502.CI_BINARY_RESOURCE_DLT_OBX = [ 255, 255, 0, 4, 70, 12, 255, 241, 228, 215, 203, 192, 181, 170, 161, 152,
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
	11, 3, 189, 0, 4, 24, 109, 55, 3, 141, 39, 3, 96 ];
ASAP6502.CI_BINARY_RESOURCE_MPT_OBX = [ 255, 255, 0, 5, 178, 13, 76, 205, 11, 173, 46, 7, 208, 1, 96, 169,
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
	14, 140, 26, 14, 136, 140, 25, 14, 96 ];
ASAP6502.CI_BINARY_RESOURCE_RMT4_OBX = [ 255, 255, 144, 3, 96, 11, 128, 0, 128, 32, 128, 64, 0, 192, 128, 128,
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
	142, 7, 210, 140, 8, 210, 96 ];
ASAP6502.CI_BINARY_RESOURCE_RMT8_OBX = [ 255, 255, 144, 3, 108, 12, 128, 0, 128, 32, 128, 64, 0, 192, 128, 128,
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
	8, 210, 96 ];
ASAP6502.CI_BINARY_RESOURCE_TM2_OBX = [ 255, 255, 0, 2, 107, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
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
	5, 96 ];
ASAP6502.CI_BINARY_RESOURCE_TMC_OBX = [ 255, 255, 0, 5, 104, 15, 76, 206, 13, 76, 208, 8, 76, 239, 9, 15,
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
	157, 100, 8, 157, 108, 8, 157, 148, 8, 169, 1, 157, 188, 7, 96 ];

function ASAPInfo()
{
	this.author = null;
	this.channels = 0;
	this.covoxAddr = 0;
	this.date = null;
	this.defaultSong = 0;
	this.durations = new Array(32);
	this.fastplay = 0;
	this.filename = null;
	this.headerLen = 0;
	this.init = 0;
	this.loops = new Array(32);
	this.music = 0;
	this.name = null;
	this.ntsc = false;
	this.player = 0;
	this.songPos = new Array(32);
	this.songs = 0;
	this.type = ASAPModuleType.SAP_B;
}

ASAPInfo.prototype.addSong = function(playerCalls) {
	this.durations[this.songs++] = Math.floor(playerCalls * this.fastplay * 114000 / 1773447);
}

ASAPInfo.prototype.checkDate = function() {
	var n = this.date.length;
	switch (n) {
		case 10:
			if (!this.checkTwoDateDigits(0) || this.date.charCodeAt(2) != 47)
				return -1;
		case 7:
			if (!this.checkTwoDateDigits(n - 7) || this.date.charCodeAt(n - 5) != 47)
				return -1;
		case 4:
			if (!this.checkTwoDateDigits(n - 4) || !this.checkTwoDateDigits(n - 2))
				return -1;
			return n;
		default:
			return -1;
	}
}

ASAPInfo.prototype.checkTwoDateDigits = function(i) {
	var d1 = this.date.charCodeAt(i);
	var d2 = this.date.charCodeAt(i + 1);
	return d1 >= 48 && d1 <= 57 && d2 >= 48 && d2 <= 57;
}

ASAPInfo.checkValidChar = function(c) {
	if (c < 32 || c > 122 || c == 34 || c == 96)
		throw "Invalid character";
}

ASAPInfo.checkValidText = function(s) {
	var n = s.length;
	if (n > 127)
		throw "Text too long";
	for (var i = 0; i < n; i++)
		ASAPInfo.checkValidChar(s.charCodeAt(i));
}
ASAPInfo.COPYRIGHT = "This program is free software; you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published\nby the Free Software Foundation; either version 2 of the License,\nor (at your option) any later version.";
ASAPInfo.CREDITS = "Another Slight Atari Player (C) 2005-2011 Piotr Fusik\nCMC, MPT, TMC, TM2 players (C) 1994-2005 Marcin Lewandowski\nRMT player (C) 2002-2005 Radek Sterba\nDLT player (C) 2009 Marek Konopka\nCMS player (C) 1999 David Spilka\n";

ASAPInfo.prototype.getAuthor = function() {
	return this.author;
}

ASAPInfo.prototype.getChannels = function() {
	return this.channels;
}

ASAPInfo.prototype.getDate = function() {
	return this.date;
}

ASAPInfo.prototype.getDayOfMonth = function() {
	var n = this.checkDate();
	if (n != 10)
		return -1;
	return this.getTwoDateDigits(0);
}

ASAPInfo.prototype.getDefaultSong = function() {
	return this.defaultSong;
}

ASAPInfo.prototype.getDuration = function(song) {
	return this.durations[song];
}

ASAPInfo.getExtDescription = function(ext) {
	if (ext.length != 3)
		throw "Unknown extension";
	switch (ext.charCodeAt(0) + (ext.charCodeAt(1) << 8) + (ext.charCodeAt(2) << 16) | 2105376) {
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
			throw "Unknown extension";
	}
}

ASAPInfo.prototype.getLoop = function(song) {
	return this.loops[song];
}

ASAPInfo.prototype.getMonth = function() {
	var n = this.checkDate();
	if (n < 7)
		return -1;
	return this.getTwoDateDigits(n - 7);
}

ASAPInfo.prototype.getOriginalModuleExt = function(module, moduleLen) {
	switch (this.type) {
		case ASAPModuleType.SAP_B:
			if ((this.init == 1019 || this.init == 1017) && this.player == 1283)
				return "dlt";
			if (this.init == 1267 || this.init == 62707 || this.init == 1263)
				return this.fastplay == 156 ? "mpd" : "mpt";
			if (this.init == 3200)
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

ASAPInfo.getPackedExt = function(filename) {
	var ext = 0;
	for (var i = filename.length; --i > 0;) {
		var c = filename.charCodeAt(i);
		if (c <= 32 || c > 122)
			return 0;
		if (c == 46)
			return ext | 2105376;
		ext = (ext << 8) + c;
	}
	return 0;
}

ASAPInfo.getRmtInstrumentFrames = function(module, instrument, volume, volumeFrame, onExtraPokey) {
	var addrToOffset = ASAPInfo.getWord(module, 2) - 6;
	instrument = ASAPInfo.getWord(module, 14) - addrToOffset + (instrument << 1);
	if (module[instrument + 1] == 0)
		return 0;
	instrument = ASAPInfo.getWord(module, instrument) - addrToOffset;
	var perFrame = module[12];
	var playerCall = volumeFrame * perFrame;
	var playerCalls = playerCall;
	var index = module[instrument] + 1 + playerCall * 3;
	var indexEnd = module[instrument + 2] + 3;
	var indexLoop = module[instrument + 3];
	if (indexLoop >= indexEnd)
		return 0;
	var volumeSlideDepth = module[instrument + 6];
	var volumeMin = module[instrument + 7];
	if (index >= indexEnd)
		index = (index - indexEnd) % (indexEnd - indexLoop) + indexLoop;
	else {
		do {
			var vol = module[instrument + index];
			if (onExtraPokey)
				vol >>= 4;
			if ((vol & 15) >= ASAPInfo.CI_CONST_ARRAY_1[volume])
				playerCalls = playerCall + 1;
			playerCall++;
			index += 3;
		}
		while (index < indexEnd);
	}
	if (volumeSlideDepth == 0)
		return Math.floor(playerCalls / perFrame);
	var volumeSlide = 128;
	var silentLoop = false;
	for (;;) {
		if (index >= indexEnd) {
			if (silentLoop)
				break;
			silentLoop = true;
			index = indexLoop;
		}
		var vol = module[instrument + index];
		if (onExtraPokey)
			vol >>= 4;
		if ((vol & 15) >= ASAPInfo.CI_CONST_ARRAY_1[volume]) {
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
	return Math.floor(playerCalls / perFrame);
}

ASAPInfo.prototype.getSongs = function() {
	return this.songs;
}

ASAPInfo.prototype.getTitle = function() {
	return this.name;
}

ASAPInfo.prototype.getTitleOrFilename = function() {
	return this.name.length > 0 ? this.name : this.filename;
}

ASAPInfo.prototype.getTwoDateDigits = function(i) {
	return (this.date.charCodeAt(i) - 48) * 10 + this.date.charCodeAt(i + 1) - 48;
}

ASAPInfo.getWord = function(array, i) {
	return array[i] + (array[i + 1] << 8);
}

ASAPInfo.prototype.getYear = function() {
	var n = this.checkDate();
	if (n < 0)
		return -1;
	return this.getTwoDateDigits(n - 4) * 100 + this.getTwoDateDigits(n - 2);
}

ASAPInfo.hasStringAt = function(module, moduleIndex, s) {
	var n = s.length;
	for (var i = 0; i < n; i++)
		if (module[moduleIndex + i] != s.charCodeAt(i))
			return false;
	return true;
}

ASAPInfo.isDltPatternEnd = function(module, pos, i) {
	for (var ch = 0; ch < 4; ch++) {
		var pattern = module[8198 + (ch << 8) + pos];
		if (pattern < 64) {
			var offset = 6 + (pattern << 7) + (i << 1);
			if ((module[offset] & 128) == 0 && (module[offset + 1] & 128) != 0)
				return true;
		}
	}
	return false;
}

ASAPInfo.isDltTrackEmpty = function(module, pos) {
	return module[8198 + pos] >= 67 && module[8454 + pos] >= 64 && module[8710 + pos] >= 64 && module[8966 + pos] >= 64;
}

ASAPInfo.prototype.isNtsc = function() {
	return this.ntsc;
}

ASAPInfo.isOurExt = function(ext) {
	return ext.length == 3 && ASAPInfo.isOurPackedExt(ext.charCodeAt(0) + (ext.charCodeAt(1) << 8) + (ext.charCodeAt(2) << 16) | 2105376);
}

ASAPInfo.isOurFile = function(filename) {
	return ASAPInfo.isOurPackedExt(ASAPInfo.getPackedExt(filename));
}

ASAPInfo.isOurPackedExt = function(ext) {
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

ASAPInfo.prototype.load = function(filename, module, moduleLen) {
	var len = filename.length;
	var basename = 0;
	var ext = -1;
	for (var i = len; --i >= 0;) {
		var c = filename.charCodeAt(i);
		if (c == 47 || c == 92) {
			basename = i + 1;
			break;
		}
		if (c == 46)
			ext = i;
	}
	if (ext < 0)
		throw "Filename has no extension";
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
	for (var i = 0; i < 32; i++) {
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
			throw "Unknown filename extension";
	}
}
ASAPInfo.MAX_MODULE_LENGTH = 65000;
ASAPInfo.MAX_SONGS = 32;
ASAPInfo.MAX_TEXT_LENGTH = 127;

ASAPInfo.prototype.parseCmc = function(module, moduleLen, type) {
	if (moduleLen < 774)
		throw "Module too short";
	this.type = type;
	this.parseModule(module, moduleLen);
	var lastPos = 84;
	while (--lastPos >= 0) {
		if (module[518 + lastPos] < 176 || module[603 + lastPos] < 64 || module[688 + lastPos] < 64)
			break;
		if (this.channels == 2) {
			if (module[774 + lastPos] < 176 || module[859 + lastPos] < 64 || module[944 + lastPos] < 64)
				break;
		}
	}
	this.songs = 0;
	this.parseCmcSong(module, 0);
	for (var pos = 0; pos < lastPos && this.songs < 32; pos++)
		if (module[518 + pos] == 143 || module[518 + pos] == 239)
			this.parseCmcSong(module, pos + 1);
}

ASAPInfo.prototype.parseCmcSong = function(module, pos) {
	var tempo = module[25];
	var playerCalls = 0;
	var repStartPos = 0;
	var repEndPos = 0;
	var repTimes = 0;
	var seen = new Array(85);
	Ci.clearArray(seen, 0);
	while (pos >= 0 && pos < 85) {
		if (pos == repEndPos && repTimes > 0) {
			for (var i = 0; i < 85; i++)
				if (seen[i] == 1 || seen[i] == 3)
					seen[i] = 0;
			repTimes--;
			pos = repStartPos;
		}
		if (seen[pos] != 0) {
			if (seen[pos] != 1)
				this.loops[this.songs] = true;
			break;
		}
		seen[pos] = 1;
		var p1 = module[518 + pos];
		var p2 = module[603 + pos];
		var p3 = module[688 + pos];
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
			if (seen[p1] == 1)
				seen[p1] = p2;
		playerCalls += tempo * (this.type == ASAPModuleType.CM3 ? 48 : 64);
		pos++;
	}
	this.addSong(playerCalls);
}

ASAPInfo.parseDec = function(module, moduleIndex, maxVal) {
	if (module[moduleIndex] == 13)
		throw "Missing number";
	for (var r = 0;;) {
		var c = module[moduleIndex++];
		if (c == 13)
			return r;
		if (c < 48 || c > 57)
			throw "Invalid number";
		r = 10 * r + c - 48;
		if (r > maxVal)
			throw "Number too big";
	}
}

ASAPInfo.prototype.parseDlt = function(module, moduleLen) {
	if (moduleLen != 11270 && moduleLen != 11271)
		throw "Invalid module length";
	this.type = ASAPModuleType.DLT;
	this.parseModule(module, moduleLen);
	if (this.music != 8192)
		throw "Unsupported module address";
	var seen = new Array(128);
	Ci.clearArray(seen, false);
	this.songs = 0;
	for (var pos = 0; pos < 128 && this.songs < 32; pos++) {
		if (!seen[pos])
			this.parseDltSong(module, seen, pos);
	}
	if (this.songs == 0)
		throw "No songs found";
}

ASAPInfo.prototype.parseDltSong = function(module, seen, pos) {
	while (pos < 128 && !seen[pos] && ASAPInfo.isDltTrackEmpty(module, pos))
		seen[pos++] = true;
	this.songPos[this.songs] = pos;
	var playerCalls = 0;
	var loop = false;
	var tempo = 6;
	while (pos < 128) {
		if (seen[pos]) {
			loop = true;
			break;
		}
		seen[pos] = true;
		var p1 = module[8198 + pos];
		if (p1 == 64 || ASAPInfo.isDltTrackEmpty(module, pos))
			break;
		if (p1 == 65)
			pos = module[8326 + pos];
		else if (p1 == 66)
			tempo = module[8326 + pos++];
		else {
			for (var i = 0; i < 64 && !ASAPInfo.isDltPatternEnd(module, pos, i); i++)
				playerCalls += tempo;
			pos++;
		}
	}
	if (playerCalls > 0) {
		this.loops[this.songs] = loop;
		this.addSong(playerCalls);
	}
}

ASAPInfo.parseDuration = function(s) {
	var i = 0;
	var n = s.length;
	var d;
	if (i >= n)
		throw "Invalid duration";
	d = s.charCodeAt(i) - 48;
	if (d < 0 || d > 9)
		throw "Invalid duration";
	i++;
	var r = d;
	if (i < n) {
		d = s.charCodeAt(i) - 48;
		if (d >= 0 && d <= 9) {
			i++;
			r = 10 * r + d;
		}
		if (i < n && s.charCodeAt(i) == 58) {
			i++;
			if (i >= n)
				throw "Invalid duration";
			d = s.charCodeAt(i) - 48;
			if (d < 0 || d > 5)
				throw "Invalid duration";
			i++;
			r = (6 * r + d) * 10;
			if (i >= n)
				throw "Invalid duration";
			d = s.charCodeAt(i) - 48;
			if (d < 0 || d > 9)
				throw "Invalid duration";
			i++;
			r += d;
		}
	}
	r *= 1000;
	if (i >= n)
		return r;
	if (s.charCodeAt(i) != 46)
		throw "Invalid duration";
	i++;
	if (i >= n)
		throw "Invalid duration";
	d = s.charCodeAt(i) - 48;
	if (d < 0 || d > 9)
		throw "Invalid duration";
	i++;
	r += 100 * d;
	if (i >= n)
		return r;
	d = s.charCodeAt(i) - 48;
	if (d < 0 || d > 9)
		throw "Invalid duration";
	i++;
	r += 10 * d;
	if (i >= n)
		return r;
	d = s.charCodeAt(i) - 48;
	if (d < 0 || d > 9)
		throw "Invalid duration";
	i++;
	r += d;
	return r;
}

ASAPInfo.parseHex = function(module, moduleIndex) {
	if (module[moduleIndex] == 13)
		throw "Missing number";
	for (var r = 0;;) {
		var c = module[moduleIndex++];
		if (c == 13)
			return r;
		if (r > 4095)
			throw "Number too big";
		r <<= 4;
		if (c >= 48 && c <= 57)
			r += c - 48;
		else if (c >= 65 && c <= 70)
			r += c - 65 + 10;
		else if (c >= 97 && c <= 102)
			r += c - 97 + 10;
		else
			throw "Invalid number";
	}
}

ASAPInfo.prototype.parseModule = function(module, moduleLen) {
	if ((module[0] != 255 || module[1] != 255) && (module[0] != 0 || module[1] != 0))
		throw "Invalid two leading bytes of the module";
	this.music = ASAPInfo.getWord(module, 2);
	var musicLastByte = ASAPInfo.getWord(module, 4);
	if (this.music <= 55295 && musicLastByte >= 53248)
		throw "Module address conflicts with hardware registers";
	var blockLen = musicLastByte + 1 - this.music;
	if (6 + blockLen != moduleLen) {
		if (this.type != ASAPModuleType.RMT || 11 + blockLen > moduleLen)
			throw "Module length doesn't match headers";
		var infoAddr = ASAPInfo.getWord(module, 6 + blockLen);
		if (infoAddr != this.music + blockLen)
			throw "Invalid address of RMT info";
		var infoLen = ASAPInfo.getWord(module, 8 + blockLen) + 1 - infoAddr;
		if (10 + blockLen + infoLen != moduleLen)
			throw "Invalid RMT info block";
	}
}

ASAPInfo.prototype.parseMpt = function(module, moduleLen) {
	if (moduleLen < 464)
		throw "Module too short";
	this.type = ASAPModuleType.MPT;
	this.parseModule(module, moduleLen);
	var track0Addr = ASAPInfo.getWord(module, 2) + 458;
	if (module[454] + (module[458] << 8) != track0Addr)
		throw "Invalid address of the first track";
	var songLen = module[455] + (module[459] << 8) - track0Addr >> 1;
	if (songLen > 254)
		throw "Song too long";
	var globalSeen = new Array(256);
	Ci.clearArray(globalSeen, false);
	this.songs = 0;
	for (var pos = 0; pos < songLen && this.songs < 32; pos++) {
		if (!globalSeen[pos]) {
			this.songPos[this.songs] = pos;
			this.parseMptSong(module, globalSeen, songLen, pos);
		}
	}
	if (this.songs == 0)
		throw "No songs found";
}

ASAPInfo.prototype.parseMptSong = function(module, globalSeen, songLen, pos) {
	var addrToOffset = ASAPInfo.getWord(module, 2) - 6;
	var tempo = module[463];
	var playerCalls = 0;
	var seen = new Array(256);
	Ci.clearArray(seen, 0);
	var patternOffset = new Array(4);
	var blankRows = new Array(4);
	Ci.clearArray(blankRows, 0);
	var blankRowsCounter = new Array(4);
	while (pos < songLen) {
		if (seen[pos] != 0) {
			if (seen[pos] != 1)
				this.loops[this.songs] = true;
			break;
		}
		seen[pos] = 1;
		globalSeen[pos] = true;
		var i = module[464 + pos * 2];
		if (i == 255) {
			pos = module[465 + pos * 2];
			continue;
		}
		var ch;
		for (ch = 3; ch >= 0; ch--) {
			i = module[454 + ch] + (module[458 + ch] << 8) - addrToOffset;
			i = module[i + pos * 2];
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
			if (seen[i] == 1)
				seen[i] = 2;
		for (var patternRows = module[462]; --patternRows >= 0;) {
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
		this.addSong(playerCalls);
}

ASAPInfo.prototype.parseRmt = function(module, moduleLen) {
	if (moduleLen < 48)
		throw "Module too short";
	if (module[6] != 82 || module[7] != 77 || module[8] != 84 || module[13] != 1)
		throw "Invalid module header";
	var posShift;
	switch (module[9]) {
		case 52:
			posShift = 2;
			break;
		case 56:
			this.channels = 2;
			posShift = 3;
			break;
		default:
			throw "Unsupported number of channels";
	}
	var perFrame = module[12];
	if (perFrame < 1 || perFrame > 4)
		throw "Unsupported player call rate";
	this.type = ASAPModuleType.RMT;
	this.parseModule(module, moduleLen);
	var songLen = ASAPInfo.getWord(module, 4) + 1 - ASAPInfo.getWord(module, 20);
	if (posShift == 3 && (songLen & 4) != 0 && module[6 + ASAPInfo.getWord(module, 4) - ASAPInfo.getWord(module, 2) - 3] == 254)
		songLen += 4;
	songLen >>= posShift;
	if (songLen >= 256)
		throw "Song too long";
	var globalSeen = new Array(256);
	Ci.clearArray(globalSeen, false);
	this.songs = 0;
	for (var pos = 0; pos < songLen && this.songs < 32; pos++) {
		if (!globalSeen[pos]) {
			this.songPos[this.songs] = pos;
			this.parseRmtSong(module, globalSeen, songLen, posShift, pos);
		}
	}
	this.fastplay = Math.floor(312 / perFrame);
	this.player = 1536;
	if (this.songs == 0)
		throw "No songs found";
}

ASAPInfo.prototype.parseRmtSong = function(module, globalSeen, songLen, posShift, pos) {
	var addrToOffset = ASAPInfo.getWord(module, 2) - 6;
	var tempo = module[11];
	var frames = 0;
	var songOffset = ASAPInfo.getWord(module, 20) - addrToOffset;
	var patternLoOffset = ASAPInfo.getWord(module, 16) - addrToOffset;
	var patternHiOffset = ASAPInfo.getWord(module, 18) - addrToOffset;
	var seen = new Array(256);
	Ci.clearArray(seen, 0);
	var patternBegin = new Array(8);
	var patternOffset = new Array(8);
	var blankRows = new Array(8);
	var instrumentNo = new Array(8);
	Ci.clearArray(instrumentNo, 0);
	var instrumentFrame = new Array(8);
	Ci.clearArray(instrumentFrame, 0);
	var volumeValue = new Array(8);
	Ci.clearArray(volumeValue, 0);
	var volumeFrame = new Array(8);
	Ci.clearArray(volumeFrame, 0);
	while (pos < songLen) {
		if (seen[pos] != 0) {
			if (seen[pos] != 1)
				this.loops[this.songs] = true;
			break;
		}
		seen[pos] = 1;
		globalSeen[pos] = true;
		if (module[songOffset + (pos << posShift)] == 254) {
			pos = module[songOffset + (pos << posShift) + 1];
			continue;
		}
		for (var ch = 0; ch < 1 << posShift; ch++) {
			var p = module[songOffset + (pos << posShift) + ch];
			if (p == 255)
				blankRows[ch] = 256;
			else {
				patternOffset[ch] = patternBegin[ch] = module[patternLoOffset + p] + (module[patternHiOffset + p] << 8) - addrToOffset;
				blankRows[ch] = 0;
			}
		}
		for (var i = 0; i < songLen; i++)
			if (seen[i] == 1)
				seen[i] = 2;
		for (var patternRows = module[10]; --patternRows >= 0;) {
			for (var ch = 0; ch < 1 << posShift; ch++) {
				if (--blankRows[ch] > 0)
					continue;
				for (;;) {
					var i = module[patternOffset[ch]++];
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
	var instrumentFrames = 0;
	for (var ch = 0; ch < 1 << posShift; ch++) {
		var frame = instrumentFrame[ch];
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

ASAPInfo.prototype.parseSap = function(module, moduleLen) {
	if (!ASAPInfo.hasStringAt(module, 0, "SAP\r\n"))
		throw "Missing SAP header";
	this.fastplay = -1;
	var type = 0;
	var moduleIndex = 5;
	var durationIndex = 0;
	while (module[moduleIndex] != 255) {
		if (moduleIndex + 8 >= moduleLen)
			throw "Missing binary part";
		if (ASAPInfo.hasStringAt(module, moduleIndex, "AUTHOR ")) {
			var len = ASAPInfo.parseText(module, moduleIndex + 7);
			if (len > 0)
				this.author = Ci.bytesToString(module, moduleIndex + 7 + 1, len);
		}
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "NAME ")) {
			var len = ASAPInfo.parseText(module, moduleIndex + 5);
			if (len > 0)
				this.name = Ci.bytesToString(module, moduleIndex + 5 + 1, len);
		}
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "DATE ")) {
			var len = ASAPInfo.parseText(module, moduleIndex + 5);
			if (len > 0)
				this.date = Ci.bytesToString(module, moduleIndex + 5 + 1, len);
		}
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "SONGS ")) {
			this.songs = ASAPInfo.parseDec(module, moduleIndex + 6, 32);
			if (this.songs < 1)
				throw "Number too small";
		}
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "DEFSONG ")) {
			this.defaultSong = ASAPInfo.parseDec(module, moduleIndex + 8, 31);
			if (this.defaultSong < 0)
				throw "Number too small";
		}
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "STEREO\r"))
			this.channels = 2;
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "NTSC\r"))
			this.ntsc = true;
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "TIME ")) {
			if (durationIndex >= 32)
				throw "Too many TIME tags";
			moduleIndex += 5;
			var len;
			for (len = 0; module[moduleIndex + len] != 13; len++) {
			}
			if (len > 5 && ASAPInfo.hasStringAt(module, moduleIndex + len - 5, " LOOP")) {
				this.loops[durationIndex] = true;
				len -= 5;
			}
			if (len > 9)
				throw "Invalid TIME tag";
			var s = Ci.bytesToString(module, moduleIndex, len);
			var duration = ASAPInfo.parseDuration(s);
			this.durations[durationIndex++] = duration;
		}
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "TYPE "))
			type = module[moduleIndex + 5];
		else if (ASAPInfo.hasStringAt(module, moduleIndex, "FASTPLAY ")) {
			this.fastplay = ASAPInfo.parseDec(module, moduleIndex + 9, 312);
			if (this.fastplay < 1)
				throw "Number too small";
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
				throw "COVOX should be D600";
			this.channels = 2;
		}
		while (module[moduleIndex++] != 13) {
			if (moduleIndex >= moduleLen)
				throw "Malformed SAP header";
		}
		if (module[moduleIndex++] != 10)
			throw "Malformed SAP header";
	}
	if (this.defaultSong >= this.songs)
		throw "DEFSONG too big";
	switch (type) {
		case 66:
			if (this.player < 0)
				throw "Missing PLAYER tag";
			if (this.init < 0)
				throw "Missing INIT tag";
			this.type = ASAPModuleType.SAP_B;
			break;
		case 67:
			if (this.player < 0)
				throw "Missing PLAYER tag";
			if (this.music < 0)
				throw "Missing MUSIC tag";
			this.type = ASAPModuleType.SAP_C;
			break;
		case 68:
			if (this.init < 0)
				throw "Missing INIT tag";
			this.type = ASAPModuleType.SAP_D;
			break;
		case 83:
			if (this.init < 0)
				throw "Missing INIT tag";
			this.type = ASAPModuleType.SAP_S;
			if (this.fastplay < 0)
				this.fastplay = 78;
			break;
		default:
			throw "Unsupported TYPE";
	}
	if (this.fastplay < 0)
		this.fastplay = this.ntsc ? 262 : 312;
	else if (this.ntsc && this.fastplay > 262)
		throw "FASTPLAY too big";
	if (module[moduleIndex + 1] != 255)
		throw "Invalid binary header";
	this.headerLen = moduleIndex;
}

ASAPInfo.parseText = function(module, moduleIndex) {
	if (module[moduleIndex] != 34)
		throw "Missing quote";
	if (ASAPInfo.hasStringAt(module, moduleIndex + 1, "<?>\"\r"))
		return 0;
	for (var len = 0;; len++) {
		var c = module[moduleIndex + 1 + len];
		if (c == 34 && module[moduleIndex + 2 + len] == 13)
			return len;
		ASAPInfo.checkValidChar(c);
	}
}

ASAPInfo.prototype.parseTm2 = function(module, moduleLen) {
	if (moduleLen < 932)
		throw "Module too short";
	this.type = ASAPModuleType.TM2;
	this.parseModule(module, moduleLen);
	var i = module[37];
	if (i < 1 || i > 4)
		throw "Unsupported player call rate";
	this.fastplay = Math.floor(312 / i);
	this.player = 1280;
	if (module[31] != 0)
		this.channels = 2;
	var lastPos = 65535;
	for (i = 0; i < 128; i++) {
		var instrAddr = module[134 + i] + (module[774 + i] << 8);
		if (instrAddr != 0 && instrAddr < lastPos)
			lastPos = instrAddr;
	}
	for (i = 0; i < 256; i++) {
		var patternAddr = module[262 + i] + (module[518 + i] << 8);
		if (patternAddr != 0 && patternAddr < lastPos)
			lastPos = patternAddr;
	}
	lastPos -= ASAPInfo.getWord(module, 2) + 896;
	if (902 + lastPos >= moduleLen)
		throw "Module too short";
	var c;
	do {
		if (lastPos <= 0)
			throw "No songs found";
		lastPos -= 17;
		c = module[918 + lastPos];
	}
	while (c == 0 || c >= 128);
	this.songs = 0;
	this.parseTm2Song(module, 0);
	for (i = 0; i < lastPos && this.songs < 32; i += 17) {
		c = module[918 + i];
		if (c == 0 || c >= 128)
			this.parseTm2Song(module, i + 17);
	}
}

ASAPInfo.prototype.parseTm2Song = function(module, pos) {
	var addrToOffset = ASAPInfo.getWord(module, 2) - 6;
	var tempo = module[36] + 1;
	var playerCalls = 0;
	var patternOffset = new Array(8);
	var blankRows = new Array(8);
	for (;;) {
		var patternRows = module[918 + pos];
		if (patternRows == 0)
			break;
		if (patternRows >= 128) {
			this.loops[this.songs] = true;
			break;
		}
		for (var ch = 7; ch >= 0; ch--) {
			var pat = module[917 + pos - 2 * ch];
			patternOffset[ch] = module[262 + pat] + (module[518 + pat] << 8) - addrToOffset;
			blankRows[ch] = 0;
		}
		while (--patternRows >= 0) {
			for (var ch = 7; ch >= 0; ch--) {
				if (--blankRows[ch] >= 0)
					continue;
				for (;;) {
					var i = module[patternOffset[ch]++];
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
	this.addSong(playerCalls);
}

ASAPInfo.prototype.parseTmc = function(module, moduleLen) {
	if (moduleLen < 464)
		throw "Module too short";
	this.type = ASAPModuleType.TMC;
	this.parseModule(module, moduleLen);
	this.channels = 2;
	var i = 0;
	while (module[102 + i] == 0) {
		if (++i >= 64)
			throw "No instruments";
	}
	var lastPos = (module[102 + i] << 8) + module[38 + i] - ASAPInfo.getWord(module, 2) - 432;
	if (437 + lastPos >= moduleLen)
		throw "Module too short";
	do {
		if (lastPos <= 0)
			throw "No songs found";
		lastPos -= 16;
	}
	while (module[437 + lastPos] >= 128);
	this.songs = 0;
	this.parseTmcSong(module, 0);
	for (i = 0; i < lastPos && this.songs < 32; i += 16)
		if (module[437 + i] >= 128)
			this.parseTmcSong(module, i + 16);
	i = module[37];
	if (i < 1 || i > 4)
		throw "Unsupported player call rate";
	this.fastplay = Math.floor(312 / i);
}

ASAPInfo.prototype.parseTmcSong = function(module, pos) {
	var addrToOffset = ASAPInfo.getWord(module, 2) - 6;
	var tempo = module[36] + 1;
	var frames = 0;
	var patternOffset = new Array(8);
	var blankRows = new Array(8);
	while (module[437 + pos] < 128) {
		for (var ch = 7; ch >= 0; ch--) {
			var pat = module[437 + pos - 2 * ch];
			patternOffset[ch] = module[166 + pat] + (module[294 + pat] << 8) - addrToOffset;
			blankRows[ch] = 0;
		}
		for (var patternRows = 64; --patternRows >= 0;) {
			for (var ch = 7; ch >= 0; ch--) {
				if (--blankRows[ch] >= 0)
					continue;
				for (;;) {
					var i = module[patternOffset[ch]++];
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
		this.loops[this.songs] = true;
	this.addSong(frames);
}

ASAPInfo.prototype.setAuthor = function(value) {
	ASAPInfo.checkValidText(value);
	this.author = value;
}

ASAPInfo.prototype.setDate = function(value) {
	ASAPInfo.checkValidText(value);
	this.date = value;
}

ASAPInfo.prototype.setDuration = function(song, duration) {
	if (song < 0 || song >= this.songs)
		throw "Song out of range";
	this.durations[song] = duration;
}

ASAPInfo.prototype.setLoop = function(song, loop) {
	if (song < 0 || song >= this.songs)
		throw "Song out of range";
	this.loops[song] = loop;
}

ASAPInfo.prototype.setTitle = function(value) {
	ASAPInfo.checkValidText(value);
	this.name = value;
}
ASAPInfo.VERSION = "3.0.0";
ASAPInfo.VERSION_MAJOR = 3;
ASAPInfo.VERSION_MICRO = 0;
ASAPInfo.VERSION_MINOR = 0;
ASAPInfo.YEARS = "2005-2011";
ASAPInfo.CI_CONST_ARRAY_1 = [ 16, 8, 4, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 ];

var ASAPModuleType = {
	SAP_B : 0,
	SAP_C : 1,
	SAP_D : 2,
	SAP_S : 3,
	CMC : 4,
	CM3 : 5,
	CMR : 6,
	CMS : 7,
	DLT : 8,
	MPT : 9,
	RMT : 10,
	TMC : 11,
	TM2 : 12
}

var ASAPSampleFormat = {
	U8 : 0,
	S16_L_E : 1,
	S16_B_E : 2
}

function Cpu6502()
{
	this.a = 0;
	this.c = 0;
	this.nz = 0;
	this.pc = 0;
	this.s = 0;
	this.vdi = 0;
	this.x = 0;
	this.y = 0;
}

Cpu6502.prototype.doFrame = function(asap, cycleLimit) {
	var pc = this.pc;
	var nz = this.nz;
	var a = this.a;
	var x = this.x;
	var y = this.y;
	var c = this.c;
	var s = this.s;
	var vdi = this.vdi;
	while (asap.cycle < cycleLimit) {
		if (asap.cycle >= asap.nextEventCycle) {
			this.pc = pc;
			this.s = s;
			asap.handleEvent();
			pc = this.pc;
			s = this.s;
			if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
				asap.memory[256 + s] = pc >> 8;
				s = s - 1 & 255;
				asap.memory[256 + s] = pc & 0xff;
				s = s - 1 & 255;
				asap.memory[256 + s] = ((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32;
				s = s - 1 & 255;
				vdi |= 4;
				pc = asap.memory[65534] + (asap.memory[65535] << 8);
				asap.cycle += 7;
			}
		}
		var data = asap.memory[pc++];
		asap.cycle += Cpu6502.CI_CONST_ARRAY_1[data];
		var addr;
		switch (data) {
			case 0:
				pc++;
				asap.memory[256 + s] = pc >> 8;
				s = s - 1 & 255;
				asap.memory[256 + s] = pc & 0xff;
				s = s - 1 & 255;
				asap.memory[256 + s] = ((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48;
				s = s - 1 & 255;
				vdi |= 4;
				pc = asap.memory[65534] + (asap.memory[65535] << 8);
				break;
			case 1:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
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
				asap.cycle = asap.nextEventCycle;
				break;
			case 5:
				addr = asap.memory[pc++];
				nz = a |= asap.memory[addr];
				break;
			case 6:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				asap.memory[addr] = nz;
				break;
			case 8:
				asap.memory[256 + s] = ((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48;
				s = s - 1 & 255;
				break;
			case 9:
				nz = a |= asap.memory[pc++];
				break;
			case 10:
				c = a >> 7;
				nz = a = a << 1 & 255;
				break;
			case 13:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 14:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 16:
				if (nz < 128) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 17:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 21:
				addr = asap.memory[pc++] + x & 255;
				nz = a |= asap.memory[addr];
				break;
			case 22:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				asap.memory[addr] = nz;
				break;
			case 24:
				c = 0;
				break;
			case 25:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 29:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 30:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 32:
				addr = asap.memory[pc++];
				asap.memory[256 + s] = pc >> 8;
				s = s - 1 & 255;
				asap.memory[256 + s] = pc & 0xff;
				s = s - 1 & 255;
				pc = addr + (asap.memory[pc] << 8);
				break;
			case 33:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 36:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				vdi = (vdi & 12) + (nz & 64);
				nz = ((nz & 128) << 1) + (nz & a);
				break;
			case 37:
				addr = asap.memory[pc++];
				nz = a &= asap.memory[addr];
				break;
			case 38:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				asap.memory[addr] = nz;
				break;
			case 40:
				s = s + 1 & 255;
				vdi = asap.memory[256 + s];
				nz = ((vdi & 128) << 1) + (~vdi & 2);
				c = vdi & 1;
				vdi &= 76;
				if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
					asap.memory[256 + s] = pc >> 8;
					s = s - 1 & 255;
					asap.memory[256 + s] = pc & 0xff;
					s = s - 1 & 255;
					asap.memory[256 + s] = ((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32;
					s = s - 1 & 255;
					vdi |= 4;
					pc = asap.memory[65534] + (asap.memory[65535] << 8);
					asap.cycle += 7;
				}
				break;
			case 41:
				nz = a &= asap.memory[pc++];
				break;
			case 42:
				a = (a << 1) + c;
				c = a >> 8;
				nz = a &= 255;
				break;
			case 44:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				vdi = (vdi & 12) + (nz & 64);
				nz = ((nz & 128) << 1) + (nz & a);
				break;
			case 45:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 46:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 48:
				if (nz >= 128) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 49:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 53:
				addr = asap.memory[pc++] + x & 255;
				nz = a &= asap.memory[addr];
				break;
			case 54:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				asap.memory[addr] = nz;
				break;
			case 56:
				c = 1;
				break;
			case 57:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 61:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 62:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 64:
				s = s + 1 & 255;
				vdi = asap.memory[256 + s];
				nz = ((vdi & 128) << 1) + (~vdi & 2);
				c = vdi & 1;
				vdi &= 76;
				s = s + 1 & 255;
				pc = asap.memory[256 + s];
				s = s + 1 & 255;
				addr = asap.memory[256 + s];
				pc += addr << 8;
				if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
					asap.memory[256 + s] = pc >> 8;
					s = s - 1 & 255;
					asap.memory[256 + s] = pc & 0xff;
					s = s - 1 & 255;
					asap.memory[256 + s] = ((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32;
					s = s - 1 & 255;
					vdi |= 4;
					pc = asap.memory[65534] + (asap.memory[65535] << 8);
					asap.cycle += 7;
				}
				break;
			case 65:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 69:
				addr = asap.memory[pc++];
				nz = a ^= asap.memory[addr];
				break;
			case 70:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				break;
			case 72:
				asap.memory[256 + s] = a;
				s = s - 1 & 255;
				break;
			case 73:
				nz = a ^= asap.memory[pc++];
				break;
			case 74:
				c = a & 1;
				nz = a >>= 1;
				break;
			case 76:
				addr = asap.memory[pc++];
				pc = addr + (asap.memory[pc] << 8);
				break;
			case 77:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 78:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 80:
				if ((vdi & 64) == 0) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 81:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 85:
				addr = asap.memory[pc++] + x & 255;
				nz = a ^= asap.memory[addr];
				break;
			case 86:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				break;
			case 88:
				vdi &= 72;
				if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
					asap.memory[256 + s] = pc >> 8;
					s = s - 1 & 255;
					asap.memory[256 + s] = pc & 0xff;
					s = s - 1 & 255;
					asap.memory[256 + s] = ((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32;
					s = s - 1 & 255;
					vdi |= 4;
					pc = asap.memory[65534] + (asap.memory[65535] << 8);
					asap.cycle += 7;
				}
				break;
			case 89:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 93:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 94:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 96:
				s = s + 1 & 255;
				pc = asap.memory[256 + s];
				s = s + 1 & 255;
				addr = asap.memory[256 + s];
				pc += (addr << 8) + 1;
				break;
			case 97:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				data = asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				nz = asap.memory[addr] + (c << 8);
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				break;
			case 104:
				s = s + 1 & 255;
				a = asap.memory[256 + s];
				nz = a;
				break;
			case 105:
				data = asap.memory[pc++];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if ((addr & 255) == 255)
					pc = asap.memory[addr] + (asap.memory[addr - 255] << 8);
				else
					pc = asap.memory[addr] + (asap.memory[addr + 1] << 8);
				break;
			case 109:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 112:
				if ((vdi & 64) != 0) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 113:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++] + x & 255;
				data = asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr] + (c << 8);
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				break;
			case 120:
				vdi |= 4;
				break;
			case 121:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 129:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, a);
				else
					asap.memory[addr] = a;
				break;
			case 132:
				addr = asap.memory[pc++];
				asap.memory[addr] = y;
				break;
			case 133:
				addr = asap.memory[pc++];
				asap.memory[addr] = a;
				break;
			case 134:
				addr = asap.memory[pc++];
				asap.memory[addr] = x;
				break;
			case 136:
				nz = y = y - 1 & 255;
				break;
			case 138:
				nz = a = x;
				break;
			case 140:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, y);
				else
					asap.memory[addr] = y;
				break;
			case 141:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, a);
				else
					asap.memory[addr] = a;
				break;
			case 142:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, x);
				else
					asap.memory[addr] = x;
				break;
			case 144:
				if (c == 0) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 145:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, a);
				else
					asap.memory[addr] = a;
				break;
			case 148:
				addr = asap.memory[pc++] + x & 255;
				asap.memory[addr] = y;
				break;
			case 149:
				addr = asap.memory[pc++] + x & 255;
				asap.memory[addr] = a;
				break;
			case 150:
				addr = asap.memory[pc++] + y & 255;
				asap.memory[addr] = x;
				break;
			case 152:
				nz = a = y;
				break;
			case 153:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, a);
				else
					asap.memory[addr] = a;
				break;
			case 154:
				s = x;
				break;
			case 157:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, a);
				else
					asap.memory[addr] = a;
				break;
			case 160:
				nz = y = asap.memory[pc++];
				break;
			case 161:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 162:
				nz = x = asap.memory[pc++];
				break;
			case 164:
				addr = asap.memory[pc++];
				nz = y = asap.memory[addr];
				break;
			case 165:
				addr = asap.memory[pc++];
				nz = a = asap.memory[addr];
				break;
			case 166:
				addr = asap.memory[pc++];
				nz = x = asap.memory[addr];
				break;
			case 168:
				nz = y = a;
				break;
			case 169:
				nz = a = asap.memory[pc++];
				break;
			case 170:
				nz = x = a;
				break;
			case 172:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = y = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 173:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 174:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = x = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 176:
				if (c != 0) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 177:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 180:
				addr = asap.memory[pc++] + x & 255;
				nz = y = asap.memory[addr];
				break;
			case 181:
				addr = asap.memory[pc++] + x & 255;
				nz = a = asap.memory[addr];
				break;
			case 182:
				addr = asap.memory[pc++] + y & 255;
				nz = x = asap.memory[addr];
				break;
			case 184:
				vdi &= 12;
				break;
			case 185:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 186:
				nz = x = s;
				break;
			case 188:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				nz = y = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 189:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 190:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = x = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 192:
				nz = asap.memory[pc++];
				c = y >= nz ? 1 : 0;
				nz = y - nz & 255;
				break;
			case 193:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 196:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = y >= nz ? 1 : 0;
				nz = y - nz & 255;
				break;
			case 197:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 198:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				nz = nz - 1 & 255;
				asap.memory[addr] = nz;
				break;
			case 200:
				nz = y = y + 1 & 255;
				break;
			case 201:
				nz = asap.memory[pc++];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 202:
				nz = x = x - 1 & 255;
				break;
			case 204:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = y >= nz ? 1 : 0;
				nz = y - nz & 255;
				break;
			case 205:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 206:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 208:
				if ((nz & 255) != 0) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 209:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 213:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 214:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				nz = nz - 1 & 255;
				asap.memory[addr] = nz;
				break;
			case 216:
				vdi &= 68;
				break;
			case 217:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 221:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 222:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 224:
				nz = asap.memory[pc++];
				c = x >= nz ? 1 : 0;
				nz = x - nz & 255;
				break;
			case 225:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = x >= nz ? 1 : 0;
				nz = x - nz & 255;
				break;
			case 229:
				addr = asap.memory[pc++];
				data = asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				nz = nz + 1 & 255;
				asap.memory[addr] = nz;
				break;
			case 232:
				nz = x = x + 1 & 255;
				break;
			case 233:
			case 235:
				data = asap.memory[pc++];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				c = x >= nz ? 1 : 0;
				nz = x - nz & 255;
				break;
			case 237:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 240:
				if ((nz & 255) == 0) {
					addr = (asap.memory[pc] ^ 128) - 128;
					pc++;
					addr += pc;
					if ((addr ^ pc) >> 8 != 0)
						asap.cycle++;
					asap.cycle++;
					pc = addr;
					break;
				}
				pc++;
				break;
			case 241:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++] + x & 255;
				data = asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				nz = nz + 1 & 255;
				asap.memory[addr] = nz;
				break;
			case 248:
				vdi |= 8;
				break;
			case 249:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if ((addr & 255) < x)
					asap.cycle++;
				data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				break;
			case 3:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
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
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				asap.memory[addr] = nz;
				nz = a |= nz;
				break;
			case 11:
			case 43:
				nz = a &= asap.memory[pc++];
				c = nz >> 7;
				break;
			case 12:
				pc += 2;
				break;
			case 15:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a |= nz;
				break;
			case 19:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a |= nz;
				break;
			case 23:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				asap.memory[addr] = nz;
				nz = a |= nz;
				break;
			case 27:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a |= nz;
				break;
			case 28:
			case 60:
			case 92:
			case 124:
			case 220:
			case 252:
				if (asap.memory[pc++] + x >= 256)
					asap.cycle++;
				pc++;
				break;
			case 31:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz >> 7;
				nz = nz << 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a |= nz;
				break;
			case 35:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 39:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 47:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 51:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 55:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 59:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 63:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = (nz << 1) + c;
				c = nz >> 8;
				nz &= 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a &= nz;
				break;
			case 67:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 71:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 75:
				a &= asap.memory[pc++];
				c = a & 1;
				nz = a >>= 1;
				break;
			case 79:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 83:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 87:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 91:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 95:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				nz = a ^= nz;
				break;
			case 99:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				nz = asap.memory[addr] + (c << 8);
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				data = a & asap.memory[pc++];
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
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr] + (c << 8);
				c = nz & 1;
				nz >>= 1;
				asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz += c << 8;
				c = nz & 1;
				nz >>= 1;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a + data + c;
					nz = tmp & 255;
					if ((vdi & 8) == 0) {
						vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
						c = tmp >> 8;
						a = nz;
					}
					else {
						var al = (a & 15) + (data & 15) + c;
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
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				data = a & x;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, data);
				else
					asap.memory[addr] = data;
				break;
			case 135:
				addr = asap.memory[pc++];
				data = a & x;
				asap.memory[addr] = data;
				break;
			case 139:
				data = asap.memory[pc++];
				a &= (data | 239) & x;
				nz = a & data;
				break;
			case 143:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				data = a & x;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, data);
				else
					asap.memory[addr] = data;
				break;
			case 147:
				{
					addr = asap.memory[pc++];
					var hi = asap.memory[addr + 1 & 255];
					addr = asap.memory[addr];
					data = hi + 1 & a & x;
					addr += y;
					if (addr >= 256)
						hi = data - 1;
					addr += hi << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = data;
				}
				break;
			case 151:
				addr = asap.memory[pc++] + y & 255;
				data = a & x;
				asap.memory[addr] = data;
				break;
			case 155:
				s = a & x;
				{
					addr = asap.memory[pc++];
					var hi = asap.memory[pc++];
					data = hi + 1 & s;
					addr += y;
					if (addr >= 256)
						hi = data - 1;
					addr += hi << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = data;
				}
				break;
			case 156:
				{
					addr = asap.memory[pc++];
					var hi = asap.memory[pc++];
					data = hi + 1 & y;
					addr += x;
					if (addr >= 256)
						hi = data - 1;
					addr += hi << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = data;
				}
				break;
			case 158:
				{
					addr = asap.memory[pc++];
					var hi = asap.memory[pc++];
					data = hi + 1 & x;
					addr += y;
					if (addr >= 256)
						hi = data - 1;
					addr += hi << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = data;
				}
				break;
			case 159:
				{
					addr = asap.memory[pc++];
					var hi = asap.memory[pc++];
					data = hi + 1 & a & x;
					addr += y;
					if (addr >= 256)
						hi = data - 1;
					addr += hi << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = data;
				}
				break;
			case 163:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 167:
				addr = asap.memory[pc++];
				nz = x = a = asap.memory[addr];
				break;
			case 171:
				nz = x = a &= asap.memory[pc++];
				break;
			case 175:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 179:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 183:
				addr = asap.memory[pc++] + y & 255;
				nz = x = a = asap.memory[addr];
				break;
			case 187:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = x = a = s &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 191:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if ((addr & 255) < y)
					asap.cycle++;
				nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr];
				break;
			case 195:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 199:
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				nz = nz - 1 & 255;
				asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 203:
				nz = asap.memory[pc++];
				x &= a;
				c = x >= nz ? 1 : 0;
				nz = x = x - nz & 255;
				break;
			case 207:
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 211:
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 215:
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				nz = nz - 1 & 255;
				asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 219:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 223:
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz - 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				c = a >= nz ? 1 : 0;
				nz = a - nz & 255;
				break;
			case 227:
				addr = asap.memory[pc++] + x & 255;
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8);
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				nz = asap.memory[addr];
				nz = nz + 1 & 255;
				asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr += asap.memory[pc++] << 8;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr = asap.memory[addr] + (asap.memory[addr + 1 & 255] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++] + x & 255;
				nz = asap.memory[addr];
				nz = nz + 1 & 255;
				asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + y & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
				addr = asap.memory[pc++];
				addr = addr + (asap.memory[pc++] << 8) + x & 65535;
				if (addr >> 8 == 210) {
					asap.cycle--;
					nz = asap.peekHardware(addr);
					asap.pokeHardware(addr, nz);
					asap.cycle++;
				}
				else
					nz = asap.memory[addr];
				nz = nz + 1 & 255;
				if ((addr & 63744) == 53248)
					asap.pokeHardware(addr, nz);
				else
					asap.memory[addr] = nz;
				data = nz;
				{
					var tmp = a - data - 1 + c;
					var al = (a & 15) - (data & 15) - 1 + c;
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
	this.pc = pc;
	this.nz = nz;
	this.a = a;
	this.x = x;
	this.y = y;
	this.c = c;
	this.s = s;
	this.vdi = vdi;
}
Cpu6502.CI_CONST_ARRAY_1 = [ 7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
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
	2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7 ];

var NmiStatus = {
	RESET : 0,
	ON_V_BLANK : 1,
	WAS_V_BLANK : 2
}

function Pokey()
{
	this.audc1 = 0;
	this.audc2 = 0;
	this.audc3 = 0;
	this.audc4 = 0;
	this.audctl = 0;
	this.audf1 = 0;
	this.audf2 = 0;
	this.audf3 = 0;
	this.audf4 = 0;
	this.delta1 = 0;
	this.delta2 = 0;
	this.delta3 = 0;
	this.delta4 = 0;
	this.deltaBuffer = new Array(888);
	this.divCycles = 0;
	this.init = false;
	this.mute1 = 0;
	this.mute2 = 0;
	this.mute3 = 0;
	this.mute4 = 0;
	this.out1 = 0;
	this.out2 = 0;
	this.out3 = 0;
	this.out4 = 0;
	this.periodCycles1 = 0;
	this.periodCycles2 = 0;
	this.periodCycles3 = 0;
	this.periodCycles4 = 0;
	this.polyIndex = 0;
	this.reloadCycles1 = 0;
	this.reloadCycles3 = 0;
	this.skctl = 0;
	this.tickCycle1 = 0;
	this.tickCycle2 = 0;
	this.tickCycle3 = 0;
	this.tickCycle4 = 0;
}

Pokey.prototype.addDelta = function(pokeys, cycle, delta) {
	this.deltaBuffer[Math.floor((cycle * 44100 + pokeys.sampleOffset) / pokeys.mainClock)] += delta;
}

Pokey.prototype.endFrame = function(pokeys, cycle) {
	this.generateUntilCycle(pokeys, cycle);
	this.polyIndex += cycle;
	var m = (this.audctl & 128) != 0 ? 237615 : 60948015;
	if (this.polyIndex >= 2 * m)
		this.polyIndex -= m;
	if (this.tickCycle1 != 8388608)
		this.tickCycle1 -= cycle;
	if (this.tickCycle2 != 8388608)
		this.tickCycle2 -= cycle;
	if (this.tickCycle3 != 8388608)
		this.tickCycle3 -= cycle;
	if (this.tickCycle4 != 8388608)
		this.tickCycle4 -= cycle;
}

Pokey.prototype.generateUntilCycle = function(pokeys, cycleLimit) {
	for (;;) {
		var cycle = cycleLimit;
		if (cycle > this.tickCycle1)
			cycle = this.tickCycle1;
		if (cycle > this.tickCycle2)
			cycle = this.tickCycle2;
		if (cycle > this.tickCycle3)
			cycle = this.tickCycle3;
		if (cycle > this.tickCycle4)
			cycle = this.tickCycle4;
		if (cycle == cycleLimit)
			break;
		if (cycle == this.tickCycle3) {
			this.tickCycle3 += this.periodCycles3;
			if ((this.audctl & 4) != 0 && this.delta1 > 0 && this.mute1 == 0) {
				this.delta1 = -this.delta1;
				this.addDelta(pokeys, cycle, this.delta1);
			}
			if (this.init) {
				switch (this.audc3 >> 4) {
					case 10:
					case 14:
						this.out3 ^= 1;
						this.delta3 = -this.delta3;
						this.addDelta(pokeys, cycle, this.delta3);
						break;
					default:
						break;
				}
			}
			else {
				var poly = cycle + this.polyIndex - 2;
				var newOut = this.out3;
				switch (this.audc3 >> 4) {
					case 0:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0) {
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= Pokey.CI_CONST_ARRAY_2[poly % 31];
						break;
					case 4:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0)
							newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					case 8:
						if ((this.audctl & 128) != 0)
							newOut = pokeys.poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != this.out3) {
					this.out3 = newOut;
					this.delta3 = -this.delta3;
					this.addDelta(pokeys, cycle, this.delta3);
				}
			}
		}
		if (cycle == this.tickCycle4) {
			this.tickCycle4 += this.periodCycles4;
			if ((this.audctl & 8) != 0)
				this.tickCycle3 = cycle + this.reloadCycles3;
			if ((this.audctl & 2) != 0 && this.delta2 > 0 && this.mute2 == 0) {
				this.delta2 = -this.delta2;
				this.addDelta(pokeys, cycle, this.delta2);
			}
			if (this.init) {
				switch (this.audc4 >> 4) {
					case 10:
					case 14:
						this.out4 ^= 1;
						this.delta4 = -this.delta4;
						this.addDelta(pokeys, cycle, this.delta4);
						break;
					default:
						break;
				}
			}
			else {
				var poly = cycle + this.polyIndex - 3;
				var newOut = this.out4;
				switch (this.audc4 >> 4) {
					case 0:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0) {
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= Pokey.CI_CONST_ARRAY_2[poly % 31];
						break;
					case 4:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0)
							newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					case 8:
						if ((this.audctl & 128) != 0)
							newOut = pokeys.poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != this.out4) {
					this.out4 = newOut;
					this.delta4 = -this.delta4;
					this.addDelta(pokeys, cycle, this.delta4);
				}
			}
		}
		if (cycle == this.tickCycle1) {
			this.tickCycle1 += this.periodCycles1;
			if ((this.skctl & 136) == 8)
				this.tickCycle2 = cycle + this.periodCycles2;
			if (this.init) {
				switch (this.audc1 >> 4) {
					case 10:
					case 14:
						this.out1 ^= 1;
						this.delta1 = -this.delta1;
						this.addDelta(pokeys, cycle, this.delta1);
						break;
					default:
						break;
				}
			}
			else {
				var poly = cycle + this.polyIndex - 0;
				var newOut = this.out1;
				switch (this.audc1 >> 4) {
					case 0:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0) {
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= Pokey.CI_CONST_ARRAY_2[poly % 31];
						break;
					case 4:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0)
							newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					case 8:
						if ((this.audctl & 128) != 0)
							newOut = pokeys.poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != this.out1) {
					this.out1 = newOut;
					this.delta1 = -this.delta1;
					this.addDelta(pokeys, cycle, this.delta1);
				}
			}
		}
		if (cycle == this.tickCycle2) {
			this.tickCycle2 += this.periodCycles2;
			if ((this.audctl & 16) != 0)
				this.tickCycle1 = cycle + this.reloadCycles1;
			else if ((this.skctl & 8) != 0)
				this.tickCycle1 = cycle + this.periodCycles1;
			if (this.init) {
				switch (this.audc2 >> 4) {
					case 10:
					case 14:
						this.out2 ^= 1;
						this.delta2 = -this.delta2;
						this.addDelta(pokeys, cycle, this.delta2);
						break;
					default:
						break;
				}
			}
			else {
				var poly = cycle + this.polyIndex - 1;
				var newOut = this.out2;
				switch (this.audc2 >> 4) {
					case 0:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0) {
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= Pokey.CI_CONST_ARRAY_2[poly % 31];
						break;
					case 4:
						if (Pokey.CI_CONST_ARRAY_2[poly % 31] != 0)
							newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					case 8:
						if ((this.audctl & 128) != 0)
							newOut = pokeys.poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys.poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = Pokey.CI_CONST_ARRAY_1[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != this.out2) {
					this.out2 = newOut;
					this.delta2 = -this.delta2;
					this.addDelta(pokeys, cycle, this.delta2);
				}
			}
		}
	}
}

Pokey.prototype.initialize = function() {
	this.audf1 = 0;
	this.audf2 = 0;
	this.audf3 = 0;
	this.audf4 = 0;
	this.audc1 = 0;
	this.audc2 = 0;
	this.audc3 = 0;
	this.audc4 = 0;
	this.audctl = 0;
	this.skctl = 3;
	this.init = false;
	this.divCycles = 28;
	this.periodCycles1 = 28;
	this.periodCycles2 = 28;
	this.periodCycles3 = 28;
	this.periodCycles4 = 28;
	this.reloadCycles1 = 28;
	this.reloadCycles3 = 28;
	this.polyIndex = 60948015;
	this.tickCycle1 = 8388608;
	this.tickCycle2 = 8388608;
	this.tickCycle3 = 8388608;
	this.tickCycle4 = 8388608;
	this.mute1 = 1;
	this.mute2 = 1;
	this.mute3 = 1;
	this.mute4 = 1;
	this.out1 = 0;
	this.out2 = 0;
	this.out3 = 0;
	this.out4 = 0;
	this.delta1 = 0;
	this.delta2 = 0;
	this.delta3 = 0;
	this.delta4 = 0;
	Ci.clearArray(this.deltaBuffer, 0);
}

Pokey.prototype.isSilent = function() {
	return ((this.audc1 | this.audc2 | this.audc3 | this.audc4) & 15) == 0;
}

Pokey.prototype.mute = function(mask) {
	if ((mask & 1) != 0) {
		this.mute1 |= 4;
		this.tickCycle1 = 8388608;
	}
	else {
		this.mute1 &= ~4;
		if (this.tickCycle1 == 8388608 && this.mute1 == 0)
			this.tickCycle1 = 0;
	}
	if ((mask & 2) != 0) {
		this.mute2 |= 4;
		this.tickCycle2 = 8388608;
	}
	else {
		this.mute2 &= ~4;
		if (this.tickCycle2 == 8388608 && this.mute2 == 0)
			this.tickCycle2 = 0;
	}
	if ((mask & 4) != 0) {
		this.mute3 |= 4;
		this.tickCycle3 = 8388608;
	}
	else {
		this.mute3 &= ~4;
		if (this.tickCycle3 == 8388608 && this.mute3 == 0)
			this.tickCycle3 = 0;
	}
	if ((mask & 8) != 0) {
		this.mute4 |= 4;
		this.tickCycle4 = 8388608;
	}
	else {
		this.mute4 &= ~4;
		if (this.tickCycle4 == 8388608 && this.mute4 == 0)
			this.tickCycle4 = 0;
	}
}
Pokey.CI_CONST_ARRAY_1 = [ 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 ];
Pokey.CI_CONST_ARRAY_2 = [ 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0,
	1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 ];

function PokeyPair()
{
	this.basePokey = new Pokey();
	this.extraPokey = new Pokey();
	this.extraPokeyMask = 0;
	this.iirAccLeft = 0;
	this.iirAccRight = 0;
	this.irqst = 0;
	this.mainClock = 0;
	this.poly17Lookup = new Array(16385);
	this.poly9Lookup = new Array(511);
	this.sampleIndex = 0;
	this.sampleOffset = 0;
	this.samples = 0;
	this.timer1Cycle = 0;
	this.timer2Cycle = 0;
	this.timer4Cycle = 0;
	var reg = 511;
	for (var i = 0; i < 511; i++) {
		reg = (((reg >> 5 ^ reg) & 1) << 8) + (reg >> 1);
		this.poly9Lookup[i] = reg & 0xff;
	}
	reg = 131071;
	for (var i = 0; i < 16385; i++) {
		reg = (((reg >> 5 ^ reg) & 255) << 9) + (reg >> 8);
		this.poly17Lookup[i] = reg >> 1 & 0xff;
	}
}

PokeyPair.prototype.endFrame = function(cycle) {
	this.basePokey.endFrame(this, cycle);
	if (this.extraPokeyMask != 0)
		this.extraPokey.endFrame(this, cycle);
	this.sampleOffset += cycle * 44100;
	this.sampleIndex = 0;
	this.samples = Math.floor(this.sampleOffset / this.mainClock);
	this.sampleOffset %= this.mainClock;
	return this.samples;
}

PokeyPair.prototype.generate = function(buffer, bufferOffset, blocks, format) {
	var i = this.sampleIndex;
	var samples = this.samples;
	var accLeft = this.iirAccLeft;
	var accRight = this.iirAccRight;
	if (blocks < samples - i)
		samples = i + blocks;
	else
		blocks = samples - i;
	for (; i < samples; i++) {
		accLeft += this.basePokey.deltaBuffer[i] - (accLeft * 3 >> 10);
		
				var sample
//#if ACTIONSCRIPT
//					: Number
//#endif
					= accLeft / 33553408;
				buffer.writeFloat(sample);
				if (this.extraPokeyMask != 0) {
					accRight += this.extraPokey.deltaBuffer[i] - (accRight * 3 >> 10);
					sample = accRight / 33553408;
				}
				buffer.writeFloat(sample);
			}
	if (i == this.samples) {
		accLeft += this.basePokey.deltaBuffer[i];
		accRight += this.extraPokey.deltaBuffer[i];
	}
	this.sampleIndex = i;
	this.iirAccLeft = accLeft;
	this.iirAccRight = accRight;
	return blocks;
}

PokeyPair.prototype.getRandom = function(addr, cycle) {
	var pokey = (addr & this.extraPokeyMask) != 0 ? this.extraPokey : this.basePokey;
	if (pokey.init)
		return 255;
	var i = cycle + pokey.polyIndex;
	if ((pokey.audctl & 128) != 0)
		return this.poly9Lookup[i % 511];
	i %= 131071;
	var j = i >> 3;
	i &= 7;
	return (this.poly17Lookup[j] >> i) + (this.poly17Lookup[j + 1] << 8 - i) & 255;
}

PokeyPair.prototype.initialize = function(mainClock, stereo) {
	this.mainClock = mainClock;
	this.extraPokeyMask = stereo ? 16 : 0;
	this.timer1Cycle = 8388608;
	this.timer2Cycle = 8388608;
	this.timer4Cycle = 8388608;
	this.irqst = 255;
	this.basePokey.initialize();
	this.extraPokey.initialize();
	this.sampleOffset = 0;
	this.sampleIndex = 0;
	this.samples = 0;
	this.iirAccLeft = 0;
	this.iirAccRight = 0;
}

PokeyPair.prototype.isSilent = function() {
	return this.basePokey.isSilent() && this.extraPokey.isSilent();
}

PokeyPair.prototype.poke = function(addr, data, cycle) {
	var pokey = (addr & this.extraPokeyMask) != 0 ? this.extraPokey : this.basePokey;
	switch (addr & 15) {
		case 0:
			if (data == pokey.audf1)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audf1 = data;
			switch (pokey.audctl & 80) {
				case 0:
					pokey.periodCycles1 = pokey.divCycles * (data + 1);
					break;
				case 16:
					pokey.periodCycles2 = pokey.divCycles * (data + (pokey.audf2 << 8) + 1);
					pokey.reloadCycles1 = pokey.divCycles * (data + 1);
					if (pokey.periodCycles2 <= 112 && (pokey.audc2 >> 4 == 10 || pokey.audc2 >> 4 == 14)) {
						pokey.mute2 |= 1;
						pokey.tickCycle2 = 8388608;
					}
					else {
						pokey.mute2 &= ~1;
						if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
							pokey.tickCycle2 = cycle;
					}
					break;
				case 64:
					pokey.periodCycles1 = data + 4;
					break;
				case 80:
					pokey.periodCycles2 = data + (pokey.audf2 << 8) + 7;
					pokey.reloadCycles1 = data + 4;
					if (pokey.periodCycles2 <= 112 && (pokey.audc2 >> 4 == 10 || pokey.audc2 >> 4 == 14)) {
						pokey.mute2 |= 1;
						pokey.tickCycle2 = 8388608;
					}
					else {
						pokey.mute2 &= ~1;
						if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
							pokey.tickCycle2 = cycle;
					}
					break;
			}
			if (pokey.periodCycles1 <= 112 && (pokey.audc1 >> 4 == 10 || pokey.audc1 >> 4 == 14)) {
				pokey.mute1 |= 1;
				pokey.tickCycle1 = 8388608;
			}
			else {
				pokey.mute1 &= ~1;
				if (pokey.tickCycle1 == 8388608 && pokey.mute1 == 0)
					pokey.tickCycle1 = cycle;
			}
			break;
		case 1:
			if (data == pokey.audc1)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audc1 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey.mute1 & 4) == 0)
					pokey.addDelta(this, cycle, pokey.delta1 > 0 ? data - pokey.delta1 : data);
				pokey.delta1 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey.periodCycles1 <= 112 && (pokey.audc1 >> 4 == 10 || pokey.audc1 >> 4 == 14)) {
					pokey.mute1 |= 1;
					pokey.tickCycle1 = 8388608;
				}
				else {
					pokey.mute1 &= ~1;
					if (pokey.tickCycle1 == 8388608 && pokey.mute1 == 0)
						pokey.tickCycle1 = cycle;
				}
				if (pokey.delta1 > 0) {
					if ((pokey.mute1 & 4) == 0)
						pokey.addDelta(this, cycle, data - pokey.delta1);
					pokey.delta1 = data;
				}
				else
					pokey.delta1 = -data;
			}
			break;
		case 2:
			if (data == pokey.audf2)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audf2 = data;
			switch (pokey.audctl & 80) {
				case 0:
				case 64:
					pokey.periodCycles2 = pokey.divCycles * (data + 1);
					break;
				case 16:
					pokey.periodCycles2 = pokey.divCycles * (pokey.audf1 + (data << 8) + 1);
					break;
				case 80:
					pokey.periodCycles2 = pokey.audf1 + (data << 8) + 7;
					break;
			}
			if (pokey.periodCycles2 <= 112 && (pokey.audc2 >> 4 == 10 || pokey.audc2 >> 4 == 14)) {
				pokey.mute2 |= 1;
				pokey.tickCycle2 = 8388608;
			}
			else {
				pokey.mute2 &= ~1;
				if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
					pokey.tickCycle2 = cycle;
			}
			break;
		case 3:
			if (data == pokey.audc2)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audc2 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey.mute2 & 4) == 0)
					pokey.addDelta(this, cycle, pokey.delta2 > 0 ? data - pokey.delta2 : data);
				pokey.delta2 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey.periodCycles2 <= 112 && (pokey.audc2 >> 4 == 10 || pokey.audc2 >> 4 == 14)) {
					pokey.mute2 |= 1;
					pokey.tickCycle2 = 8388608;
				}
				else {
					pokey.mute2 &= ~1;
					if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
						pokey.tickCycle2 = cycle;
				}
				if (pokey.delta2 > 0) {
					if ((pokey.mute2 & 4) == 0)
						pokey.addDelta(this, cycle, data - pokey.delta2);
					pokey.delta2 = data;
				}
				else
					pokey.delta2 = -data;
			}
			break;
		case 4:
			if (data == pokey.audf3)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audf3 = data;
			switch (pokey.audctl & 40) {
				case 0:
					pokey.periodCycles3 = pokey.divCycles * (data + 1);
					break;
				case 8:
					pokey.periodCycles4 = pokey.divCycles * (data + (pokey.audf4 << 8) + 1);
					pokey.reloadCycles3 = pokey.divCycles * (data + 1);
					if (pokey.periodCycles4 <= 112 && (pokey.audc4 >> 4 == 10 || pokey.audc4 >> 4 == 14)) {
						pokey.mute4 |= 1;
						pokey.tickCycle4 = 8388608;
					}
					else {
						pokey.mute4 &= ~1;
						if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
							pokey.tickCycle4 = cycle;
					}
					break;
				case 32:
					pokey.periodCycles3 = data + 4;
					break;
				case 40:
					pokey.periodCycles4 = data + (pokey.audf4 << 8) + 7;
					pokey.reloadCycles3 = data + 4;
					if (pokey.periodCycles4 <= 112 && (pokey.audc4 >> 4 == 10 || pokey.audc4 >> 4 == 14)) {
						pokey.mute4 |= 1;
						pokey.tickCycle4 = 8388608;
					}
					else {
						pokey.mute4 &= ~1;
						if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
							pokey.tickCycle4 = cycle;
					}
					break;
			}
			if (pokey.periodCycles3 <= 112 && (pokey.audc3 >> 4 == 10 || pokey.audc3 >> 4 == 14)) {
				pokey.mute3 |= 1;
				pokey.tickCycle3 = 8388608;
			}
			else {
				pokey.mute3 &= ~1;
				if (pokey.tickCycle3 == 8388608 && pokey.mute3 == 0)
					pokey.tickCycle3 = cycle;
			}
			break;
		case 5:
			if (data == pokey.audc3)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audc3 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey.mute3 & 4) == 0)
					pokey.addDelta(this, cycle, pokey.delta3 > 0 ? data - pokey.delta3 : data);
				pokey.delta3 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey.periodCycles3 <= 112 && (pokey.audc3 >> 4 == 10 || pokey.audc3 >> 4 == 14)) {
					pokey.mute3 |= 1;
					pokey.tickCycle3 = 8388608;
				}
				else {
					pokey.mute3 &= ~1;
					if (pokey.tickCycle3 == 8388608 && pokey.mute3 == 0)
						pokey.tickCycle3 = cycle;
				}
				if (pokey.delta3 > 0) {
					if ((pokey.mute3 & 4) == 0)
						pokey.addDelta(this, cycle, data - pokey.delta3);
					pokey.delta3 = data;
				}
				else
					pokey.delta3 = -data;
			}
			break;
		case 6:
			if (data == pokey.audf4)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audf4 = data;
			switch (pokey.audctl & 40) {
				case 0:
				case 32:
					pokey.periodCycles4 = pokey.divCycles * (data + 1);
					break;
				case 8:
					pokey.periodCycles4 = pokey.divCycles * (pokey.audf3 + (data << 8) + 1);
					break;
				case 40:
					pokey.periodCycles4 = pokey.audf3 + (data << 8) + 7;
					break;
			}
			if (pokey.periodCycles4 <= 112 && (pokey.audc4 >> 4 == 10 || pokey.audc4 >> 4 == 14)) {
				pokey.mute4 |= 1;
				pokey.tickCycle4 = 8388608;
			}
			else {
				pokey.mute4 &= ~1;
				if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
					pokey.tickCycle4 = cycle;
			}
			break;
		case 7:
			if (data == pokey.audc4)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audc4 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey.mute4 & 4) == 0)
					pokey.addDelta(this, cycle, pokey.delta4 > 0 ? data - pokey.delta4 : data);
				pokey.delta4 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey.periodCycles4 <= 112 && (pokey.audc4 >> 4 == 10 || pokey.audc4 >> 4 == 14)) {
					pokey.mute4 |= 1;
					pokey.tickCycle4 = 8388608;
				}
				else {
					pokey.mute4 &= ~1;
					if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
						pokey.tickCycle4 = cycle;
				}
				if (pokey.delta4 > 0) {
					if ((pokey.mute4 & 4) == 0)
						pokey.addDelta(this, cycle, data - pokey.delta4);
					pokey.delta4 = data;
				}
				else
					pokey.delta4 = -data;
			}
			break;
		case 8:
			if (data == pokey.audctl)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.audctl = data;
			pokey.divCycles = (data & 1) != 0 ? 114 : 28;
			switch (data & 80) {
				case 0:
					pokey.periodCycles1 = pokey.divCycles * (pokey.audf1 + 1);
					pokey.periodCycles2 = pokey.divCycles * (pokey.audf2 + 1);
					break;
				case 16:
					pokey.periodCycles1 = pokey.divCycles << 8;
					pokey.periodCycles2 = pokey.divCycles * (pokey.audf1 + (pokey.audf2 << 8) + 1);
					pokey.reloadCycles1 = pokey.divCycles * (pokey.audf1 + 1);
					break;
				case 64:
					pokey.periodCycles1 = pokey.audf1 + 4;
					pokey.periodCycles2 = pokey.divCycles * (pokey.audf2 + 1);
					break;
				case 80:
					pokey.periodCycles1 = 256;
					pokey.periodCycles2 = pokey.audf1 + (pokey.audf2 << 8) + 7;
					pokey.reloadCycles1 = pokey.audf1 + 4;
					break;
			}
			if (pokey.periodCycles1 <= 112 && (pokey.audc1 >> 4 == 10 || pokey.audc1 >> 4 == 14)) {
				pokey.mute1 |= 1;
				pokey.tickCycle1 = 8388608;
			}
			else {
				pokey.mute1 &= ~1;
				if (pokey.tickCycle1 == 8388608 && pokey.mute1 == 0)
					pokey.tickCycle1 = cycle;
			}
			if (pokey.periodCycles2 <= 112 && (pokey.audc2 >> 4 == 10 || pokey.audc2 >> 4 == 14)) {
				pokey.mute2 |= 1;
				pokey.tickCycle2 = 8388608;
			}
			else {
				pokey.mute2 &= ~1;
				if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
					pokey.tickCycle2 = cycle;
			}
			switch (data & 40) {
				case 0:
					pokey.periodCycles3 = pokey.divCycles * (pokey.audf3 + 1);
					pokey.periodCycles4 = pokey.divCycles * (pokey.audf4 + 1);
					break;
				case 8:
					pokey.periodCycles3 = pokey.divCycles << 8;
					pokey.periodCycles4 = pokey.divCycles * (pokey.audf3 + (pokey.audf4 << 8) + 1);
					pokey.reloadCycles3 = pokey.divCycles * (pokey.audf3 + 1);
					break;
				case 32:
					pokey.periodCycles3 = pokey.audf3 + 4;
					pokey.periodCycles4 = pokey.divCycles * (pokey.audf4 + 1);
					break;
				case 40:
					pokey.periodCycles3 = 256;
					pokey.periodCycles4 = pokey.audf3 + (pokey.audf4 << 8) + 7;
					pokey.reloadCycles3 = pokey.audf3 + 4;
					break;
			}
			if (pokey.periodCycles3 <= 112 && (pokey.audc3 >> 4 == 10 || pokey.audc3 >> 4 == 14)) {
				pokey.mute3 |= 1;
				pokey.tickCycle3 = 8388608;
			}
			else {
				pokey.mute3 &= ~1;
				if (pokey.tickCycle3 == 8388608 && pokey.mute3 == 0)
					pokey.tickCycle3 = cycle;
			}
			if (pokey.periodCycles4 <= 112 && (pokey.audc4 >> 4 == 10 || pokey.audc4 >> 4 == 14)) {
				pokey.mute4 |= 1;
				pokey.tickCycle4 = 8388608;
			}
			else {
				pokey.mute4 &= ~1;
				if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
					pokey.tickCycle4 = cycle;
			}
			if (pokey.init && (data & 64) == 0) {
				pokey.mute1 |= 2;
				pokey.tickCycle1 = 8388608;
			}
			else {
				pokey.mute1 &= ~2;
				if (pokey.tickCycle1 == 8388608 && pokey.mute1 == 0)
					pokey.tickCycle1 = cycle;
			}
			if (pokey.init && (data & 80) != 80) {
				pokey.mute2 |= 2;
				pokey.tickCycle2 = 8388608;
			}
			else {
				pokey.mute2 &= ~2;
				if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
					pokey.tickCycle2 = cycle;
			}
			if (pokey.init && (data & 32) == 0) {
				pokey.mute3 |= 2;
				pokey.tickCycle3 = 8388608;
			}
			else {
				pokey.mute3 &= ~2;
				if (pokey.tickCycle3 == 8388608 && pokey.mute3 == 0)
					pokey.tickCycle3 = cycle;
			}
			if (pokey.init && (data & 40) != 40) {
				pokey.mute4 |= 2;
				pokey.tickCycle4 = 8388608;
			}
			else {
				pokey.mute4 &= ~2;
				if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
					pokey.tickCycle4 = cycle;
			}
			break;
		case 9:
			if (pokey.tickCycle1 != 8388608)
				pokey.tickCycle1 = cycle + pokey.periodCycles1;
			if (pokey.tickCycle2 != 8388608)
				pokey.tickCycle2 = cycle + pokey.periodCycles2;
			if (pokey.tickCycle3 != 8388608)
				pokey.tickCycle3 = cycle + pokey.periodCycles3;
			if (pokey.tickCycle4 != 8388608)
				pokey.tickCycle4 = cycle + pokey.periodCycles4;
			break;
		case 15:
			if (data == pokey.skctl)
				break;
			pokey.generateUntilCycle(this, cycle);
			pokey.skctl = data;
			var init = (data & 3) == 0;
			if (pokey.init && !init)
				pokey.polyIndex = ((pokey.audctl & 128) != 0 ? 237614 : 60948014) - cycle;
			pokey.init = init;
			if (pokey.init && (pokey.audctl & 64) == 0) {
				pokey.mute1 |= 2;
				pokey.tickCycle1 = 8388608;
			}
			else {
				pokey.mute1 &= ~2;
				if (pokey.tickCycle1 == 8388608 && pokey.mute1 == 0)
					pokey.tickCycle1 = cycle;
			}
			if (pokey.init && (pokey.audctl & 80) != 80) {
				pokey.mute2 |= 2;
				pokey.tickCycle2 = 8388608;
			}
			else {
				pokey.mute2 &= ~2;
				if (pokey.tickCycle2 == 8388608 && pokey.mute2 == 0)
					pokey.tickCycle2 = cycle;
			}
			if (pokey.init && (pokey.audctl & 32) == 0) {
				pokey.mute3 |= 2;
				pokey.tickCycle3 = 8388608;
			}
			else {
				pokey.mute3 &= ~2;
				if (pokey.tickCycle3 == 8388608 && pokey.mute3 == 0)
					pokey.tickCycle3 = cycle;
			}
			if (pokey.init && (pokey.audctl & 40) != 40) {
				pokey.mute4 |= 2;
				pokey.tickCycle4 = 8388608;
			}
			else {
				pokey.mute4 &= ~2;
				if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
					pokey.tickCycle4 = cycle;
			}
			if ((data & 16) != 0) {
				pokey.mute3 |= 8;
				pokey.tickCycle3 = 8388608;
			}
			else {
				pokey.mute3 &= ~8;
				if (pokey.tickCycle3 == 8388608 && pokey.mute3 == 0)
					pokey.tickCycle3 = cycle;
			}
			if ((data & 16) != 0) {
				pokey.mute4 |= 8;
				pokey.tickCycle4 = 8388608;
			}
			else {
				pokey.mute4 &= ~8;
				if (pokey.tickCycle4 == 8388608 && pokey.mute4 == 0)
					pokey.tickCycle4 = cycle;
			}
			break;
		default:
			break;
	}
}

PokeyPair.prototype.startFrame = function() {
	Ci.clearArray(this.basePokey.deltaBuffer, 0);
	if (this.extraPokeyMask != 0)
		Ci.clearArray(this.extraPokey.deltaBuffer, 0);
}
var Ci = {
	copyArray : function(sa, soffset, da, doffset, length) {
		for (var i = 0; i < length; i++)
			da[doffset + i] = sa[soffset + i];
	},
	bytesToString : function(a, offset, length) {
		var s = "";
		for (var i = 0; i < length; i++)
			s += String.fromCharCode(a[offset + i]);
		return s;
	},
	clearArray : function(a, value) {
		for (var i = 0; i < a.length; i++)
			a[i] = value;
	}
};
