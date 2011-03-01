// Generated automatically with "cito". Do not edit.
package net.sf.asap;

final class Pokey
{

	void addDelta(PokeyPair pokeys, int cycle, int delta)
	{
		this.deltaBuffer[(cycle * 44100 + pokeys.sampleOffset) / pokeys.mainClock] += delta;
	}
	int audc1;
	int audc2;
	int audc3;
	int audc4;
	int audctl;
	int audf1;
	int audf2;
	int audf3;
	int audf4;
	int delta1;
	int delta2;
	int delta3;
	int delta4;
	final int[] deltaBuffer = new int[888];
	int divCycles;

	void endFrame(PokeyPair pokeys, int cycle)
	{
		generateUntilCycle(pokeys, cycle);
		this.polyIndex += cycle;
		int m = (this.audctl & 128) != 0 ? 237615 : 60948015;
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

	/**
	 * Fills <code>DeltaBuffer</code> up to <code>cycleLimit</code> basing on current Audf/Audc/AudcTL values.
	 */
	void generateUntilCycle(PokeyPair pokeys, int cycleLimit)
	{
		for (;;) {
			int cycle = cycleLimit;
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
					addDelta(pokeys, cycle, this.delta1);
				}
				if (this.init) {
					switch (this.audc3 >> 4) {
						case 10:
						case 14:
							this.out3 ^= 1;
							this.delta3 = -this.delta3;
							addDelta(pokeys, cycle, this.delta3);
							break;
						default:
							break;
					}
				}
				else {
					int poly = cycle + this.polyIndex - 2;
					int newOut = this.out3;
					switch (this.audc3 >> 4) {
						case 0:
							if (CI_CONST_ARRAY_2[poly % 31] != 0) {
								if ((this.audctl & 128) != 0)
									newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
								else {
									poly %= 131071;
									newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
								}
							}
							break;
						case 2:
						case 6:
							newOut ^= CI_CONST_ARRAY_2[poly % 31] & 0xff;
							break;
						case 4:
							if (CI_CONST_ARRAY_2[poly % 31] != 0)
								newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						case 8:
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
							else {
								poly %= 131071;
								newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
							}
							break;
						case 10:
						case 14:
							newOut ^= 1;
							break;
						case 12:
							newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						default:
							break;
					}
					if (newOut != this.out3) {
						this.out3 = newOut;
						this.delta3 = -this.delta3;
						addDelta(pokeys, cycle, this.delta3);
					}
				}
			}
			if (cycle == this.tickCycle4) {
				this.tickCycle4 += this.periodCycles4;
				if ((this.audctl & 8) != 0)
					this.tickCycle3 = cycle + this.reloadCycles3;
				if ((this.audctl & 2) != 0 && this.delta2 > 0 && this.mute2 == 0) {
					this.delta2 = -this.delta2;
					addDelta(pokeys, cycle, this.delta2);
				}
				if (this.init) {
					switch (this.audc4 >> 4) {
						case 10:
						case 14:
							this.out4 ^= 1;
							this.delta4 = -this.delta4;
							addDelta(pokeys, cycle, this.delta4);
							break;
						default:
							break;
					}
				}
				else {
					int poly = cycle + this.polyIndex - 3;
					int newOut = this.out4;
					switch (this.audc4 >> 4) {
						case 0:
							if (CI_CONST_ARRAY_2[poly % 31] != 0) {
								if ((this.audctl & 128) != 0)
									newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
								else {
									poly %= 131071;
									newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
								}
							}
							break;
						case 2:
						case 6:
							newOut ^= CI_CONST_ARRAY_2[poly % 31] & 0xff;
							break;
						case 4:
							if (CI_CONST_ARRAY_2[poly % 31] != 0)
								newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						case 8:
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
							else {
								poly %= 131071;
								newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
							}
							break;
						case 10:
						case 14:
							newOut ^= 1;
							break;
						case 12:
							newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						default:
							break;
					}
					if (newOut != this.out4) {
						this.out4 = newOut;
						this.delta4 = -this.delta4;
						addDelta(pokeys, cycle, this.delta4);
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
							addDelta(pokeys, cycle, this.delta1);
							break;
						default:
							break;
					}
				}
				else {
					int poly = cycle + this.polyIndex - 0;
					int newOut = this.out1;
					switch (this.audc1 >> 4) {
						case 0:
							if (CI_CONST_ARRAY_2[poly % 31] != 0) {
								if ((this.audctl & 128) != 0)
									newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
								else {
									poly %= 131071;
									newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
								}
							}
							break;
						case 2:
						case 6:
							newOut ^= CI_CONST_ARRAY_2[poly % 31] & 0xff;
							break;
						case 4:
							if (CI_CONST_ARRAY_2[poly % 31] != 0)
								newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						case 8:
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
							else {
								poly %= 131071;
								newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
							}
							break;
						case 10:
						case 14:
							newOut ^= 1;
							break;
						case 12:
							newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						default:
							break;
					}
					if (newOut != this.out1) {
						this.out1 = newOut;
						this.delta1 = -this.delta1;
						addDelta(pokeys, cycle, this.delta1);
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
							addDelta(pokeys, cycle, this.delta2);
							break;
						default:
							break;
					}
				}
				else {
					int poly = cycle + this.polyIndex - 1;
					int newOut = this.out2;
					switch (this.audc2 >> 4) {
						case 0:
							if (CI_CONST_ARRAY_2[poly % 31] != 0) {
								if ((this.audctl & 128) != 0)
									newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
								else {
									poly %= 131071;
									newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
								}
							}
							break;
						case 2:
						case 6:
							newOut ^= CI_CONST_ARRAY_2[poly % 31] & 0xff;
							break;
						case 4:
							if (CI_CONST_ARRAY_2[poly % 31] != 0)
								newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						case 8:
							if ((this.audctl & 128) != 0)
								newOut = pokeys.poly9Lookup[poly % 511] & 0xff & 1;
							else {
								poly %= 131071;
								newOut = (pokeys.poly17Lookup[poly >> 3] & 0xff) >> (poly & 7) & 1;
							}
							break;
						case 10:
						case 14:
							newOut ^= 1;
							break;
						case 12:
							newOut = CI_CONST_ARRAY_1[poly % 15] & 0xff;
							break;
						default:
							break;
					}
					if (newOut != this.out2) {
						this.out2 = newOut;
						this.delta2 = -this.delta2;
						addDelta(pokeys, cycle, this.delta2);
					}
				}
			}
		}
	}
	boolean init;

	void initialize()
	{
		this.audctl = 0;
		this.init = false;
		this.polyIndex = 60948015;
		this.divCycles = 28;
		this.mute1 = 5;
		this.mute2 = 5;
		this.mute3 = 5;
		this.mute4 = 5;
		this.audf1 = 0;
		this.audf2 = 0;
		this.audf3 = 0;
		this.audf4 = 0;
		this.audc1 = 0;
		this.audc2 = 0;
		this.audc3 = 0;
		this.audc4 = 0;
		this.tickCycle1 = 8388608;
		this.tickCycle2 = 8388608;
		this.tickCycle3 = 8388608;
		this.tickCycle4 = 8388608;
		this.periodCycles1 = 28;
		this.periodCycles2 = 28;
		this.periodCycles3 = 28;
		this.periodCycles4 = 28;
		this.reloadCycles1 = 28;
		this.reloadCycles3 = 28;
		this.out1 = 0;
		this.out2 = 0;
		this.out3 = 0;
		this.out4 = 0;
		this.delta1 = 0;
		this.delta2 = 0;
		this.delta3 = 0;
		this.delta4 = 0;
		this.skctl = 3;
		clear(this.deltaBuffer);
	}

	boolean isSilent()
	{
		return ((this.audc1 | this.audc2 | this.audc3 | this.audc4) & 15) == 0;
	}

	void mute(int mask)
	{
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
	int mute1;
	int mute2;
	int mute3;
	int mute4;
	int out1;
	int out2;
	int out3;
	int out4;
	int periodCycles1;
	int periodCycles2;
	int periodCycles3;
	int periodCycles4;
	int polyIndex;
	int reloadCycles1;
	int reloadCycles3;
	int skctl;
	int tickCycle1;
	int tickCycle2;
	int tickCycle3;
	int tickCycle4;
	private static void clear(int[] array)
	{
		for (int i = 0; i < array.length; i++)
			array[i] = 0;
	}
	static final byte[] CI_CONST_ARRAY_1 = { 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
	static final byte[] CI_CONST_ARRAY_2 = { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0,
		1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };
}
