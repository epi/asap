// Generated automatically with "cito". Do not edit.
package net.sf.asap;

final class PokeyPair
{
	public PokeyPair()
	{
		int reg = 511;
		for (int i = 0; i < 511; i++) {
			reg = (((reg >> 5 ^ reg) & 1) << 8) + (reg >> 1);
			this.poly9Lookup[i] = (byte) reg;
		}
		reg = 131071;
		for (int i = 0; i < 16385; i++) {
			reg = (((reg >> 5 ^ reg) & 255) << 9) + (reg >> 8);
			this.poly17Lookup[i] = (byte) (reg >> 1);
		}
	}
	final Pokey basePokey = new Pokey();

	int endFrame(int cycle)
	{
		this.basePokey.endFrame(this, cycle);
		if (this.extraPokeyMask != 0)
			this.extraPokey.endFrame(this, cycle);
		this.sampleOffset += cycle * 44100;
		this.sampleIndex = 0;
		this.samples = this.sampleOffset / this.mainClock;
		this.sampleOffset %= this.mainClock;
		return this.samples;
	}
	final Pokey extraPokey = new Pokey();
	int extraPokeyMask;

	/**
	 * Fills buffer with samples from <code>DeltaBuffer</code>.
	 */
	int generate(byte[] buffer, int bufferOffset, int blocks, int format)
	{
		int i = this.sampleIndex;
		int samples = this.samples;
		int accLeft = this.iirAccLeft;
		int accRight = this.iirAccRight;
		if (blocks < samples - i)
			samples = i + blocks;
		else
			blocks = samples - i;
		for (; i < samples; i++) {
			accLeft += this.basePokey.deltaBuffer[i] - (accLeft * 3 >> 10);
			int sample = accLeft >> 10;
			if (sample < -32767)
				sample = -32767;
			else if (sample > 32767)
				sample = 32767;
			switch (format) {
				case ASAPSampleFormat.U8:
					buffer[bufferOffset++] = (byte) ((sample >> 8) + 128);
					break;
				case ASAPSampleFormat.S16_L_E:
					buffer[bufferOffset++] = (byte) sample;
					buffer[bufferOffset++] = (byte) (sample >> 8);
					break;
				case ASAPSampleFormat.S16_B_E:
					buffer[bufferOffset++] = (byte) (sample >> 8);
					buffer[bufferOffset++] = (byte) sample;
					break;
			}
			if (this.extraPokeyMask != 0) {
				accRight += this.extraPokey.deltaBuffer[i] - (accRight * 3 >> 10);
				sample = accRight >> 10;
				if (sample < -32767)
					sample = -32767;
				else if (sample > 32767)
					sample = 32767;
				switch (format) {
					case ASAPSampleFormat.U8:
						buffer[bufferOffset++] = (byte) ((sample >> 8) + 128);
						break;
					case ASAPSampleFormat.S16_L_E:
						buffer[bufferOffset++] = (byte) sample;
						buffer[bufferOffset++] = (byte) (sample >> 8);
						break;
					case ASAPSampleFormat.S16_B_E:
						buffer[bufferOffset++] = (byte) (sample >> 8);
						buffer[bufferOffset++] = (byte) sample;
						break;
				}
			}
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

	int getRandom(int addr, int cycle)
	{
		Pokey pokey = (addr & this.extraPokeyMask) != 0 ? this.extraPokey : this.basePokey;
		if (pokey.init)
			return 255;
		int i = cycle + pokey.polyIndex;
		if ((pokey.audctl & 128) != 0)
			return this.poly9Lookup[i % 511] & 0xff;
		else {
			i %= 131071;
			int j = i >> 3;
			i &= 7;
			return ((this.poly17Lookup[j] & 0xff) >> i) + ((this.poly17Lookup[j + 1] & 0xff) << 8 - i) & 255;
		}
	}
	int iirAccLeft;
	int iirAccRight;

	void initialize(int mainClock, boolean stereo)
	{
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
	int irqst;

	boolean isSilent()
	{
		return this.basePokey.isSilent() && this.extraPokey.isSilent();
	}
	int mainClock;

	void poke(int addr, int data, int cycle)
	{
		Pokey pokey = (addr & this.extraPokeyMask) != 0 ? this.extraPokey : this.basePokey;
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
				boolean init = (data & 3) == 0;
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
	final byte[] poly17Lookup = new byte[16385];
	final byte[] poly9Lookup = new byte[511];
	int sampleIndex;
	int sampleOffset;
	int samples;

	void startFrame()
	{
		clear(this.basePokey.deltaBuffer);
		if (this.extraPokeyMask != 0)
			clear(this.extraPokey.deltaBuffer);
	}
	int timer1Cycle;
	int timer2Cycle;
	int timer4Cycle;
	private static void clear(int[] array)
	{
		for (int i = 0; i < array.length; i++)
			array[i] = 0;
	}
}
