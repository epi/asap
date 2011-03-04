// Generated automatically with "cito". Do not edit.
#include <stdlib.h>
#include <string.h>
#include "pokey.h"
typedef struct Pokey Pokey;

struct Pokey {
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
	int deltaBuffer[888];
	int divCycles;
	bool init;
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
};
static void Pokey_AddDelta(Pokey *self, PokeyPair const *pokeys, int cycle, int delta);
static void Pokey_EndFrame(Pokey *self, PokeyPair const *pokeys, int cycle);
static void Pokey_GenerateUntilCycle(Pokey *self, PokeyPair const *pokeys, int cycleLimit);
static void Pokey_Initialize(Pokey *self);

struct PokeyPair {
	Pokey basePokey;
	Pokey extraPokey;
	int extraPokeyMask;
	int iirAccLeft;
	int iirAccRight;
	int irqst;
	int mainClock;
	unsigned char poly17Lookup[16385];
	unsigned char poly9Lookup[511];
	int sampleIndex;
	int sampleOffset;
	int samples;
	int timer1Cycle;
	int timer2Cycle;
	int timer4Cycle;
};
static void PokeyPair_Construct(PokeyPair *self);

static void Pokey_AddDelta(Pokey *self, PokeyPair const *pokeys, int cycle, int delta)
{
	self->deltaBuffer[(cycle * 44100 + pokeys->sampleOffset) / pokeys->mainClock] += delta;
}

static void Pokey_EndFrame(Pokey *self, PokeyPair const *pokeys, int cycle)
{
	Pokey_GenerateUntilCycle(self, pokeys, cycle);
	self->polyIndex += cycle;
	int m = (self->audctl & 128) != 0 ? 237615 : 60948015;
	if (self->polyIndex >= 2 * m)
		self->polyIndex -= m;
	if (self->tickCycle1 != 8388608)
		self->tickCycle1 -= cycle;
	if (self->tickCycle2 != 8388608)
		self->tickCycle2 -= cycle;
	if (self->tickCycle3 != 8388608)
		self->tickCycle3 -= cycle;
	if (self->tickCycle4 != 8388608)
		self->tickCycle4 -= cycle;
}

static void Pokey_GenerateUntilCycle(Pokey *self, PokeyPair const *pokeys, int cycleLimit)
{
	for (;;) {
		int cycle = cycleLimit;
		if (cycle > self->tickCycle1)
			cycle = self->tickCycle1;
		if (cycle > self->tickCycle2)
			cycle = self->tickCycle2;
		if (cycle > self->tickCycle3)
			cycle = self->tickCycle3;
		if (cycle > self->tickCycle4)
			cycle = self->tickCycle4;
		if (cycle == cycleLimit)
			break;
		static const unsigned char poly4Lookup[15] = { 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
		static const unsigned char poly5Lookup[31] = { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0,
			1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };
		if (cycle == self->tickCycle3) {
			self->tickCycle3 += self->periodCycles3;
			if ((self->audctl & 4) != 0 && self->delta1 > 0 && self->mute1 == 0) {
				self->delta1 = -self->delta1;
				Pokey_AddDelta(self, pokeys, cycle, self->delta1);
			}
			if (self->init) {
				switch (self->audc3 >> 4) {
					case 10:
					case 14:
						self->out3 ^= 1;
						self->delta3 = -self->delta3;
						Pokey_AddDelta(self, pokeys, cycle, self->delta3);
						break;
					default:
						break;
				}
			}
			else {
				int poly = cycle + self->polyIndex - 2;
				int newOut = self->out3;
				switch (self->audc3 >> 4) {
					case 0:
						if (poly5Lookup[poly % 31] != 0) {
							if ((self->audctl & 128) != 0)
								newOut = pokeys->poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= poly5Lookup[poly % 31];
						break;
					case 4:
						if (poly5Lookup[poly % 31] != 0)
							newOut = poly4Lookup[poly % 15];
						break;
					case 8:
						if ((self->audctl & 128) != 0)
							newOut = pokeys->poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = poly4Lookup[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != self->out3) {
					self->out3 = newOut;
					self->delta3 = -self->delta3;
					Pokey_AddDelta(self, pokeys, cycle, self->delta3);
				}
			}
		}
		if (cycle == self->tickCycle4) {
			self->tickCycle4 += self->periodCycles4;
			if ((self->audctl & 8) != 0)
				self->tickCycle3 = cycle + self->reloadCycles3;
			if ((self->audctl & 2) != 0 && self->delta2 > 0 && self->mute2 == 0) {
				self->delta2 = -self->delta2;
				Pokey_AddDelta(self, pokeys, cycle, self->delta2);
			}
			if (self->init) {
				switch (self->audc4 >> 4) {
					case 10:
					case 14:
						self->out4 ^= 1;
						self->delta4 = -self->delta4;
						Pokey_AddDelta(self, pokeys, cycle, self->delta4);
						break;
					default:
						break;
				}
			}
			else {
				int poly = cycle + self->polyIndex - 3;
				int newOut = self->out4;
				switch (self->audc4 >> 4) {
					case 0:
						if (poly5Lookup[poly % 31] != 0) {
							if ((self->audctl & 128) != 0)
								newOut = pokeys->poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= poly5Lookup[poly % 31];
						break;
					case 4:
						if (poly5Lookup[poly % 31] != 0)
							newOut = poly4Lookup[poly % 15];
						break;
					case 8:
						if ((self->audctl & 128) != 0)
							newOut = pokeys->poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = poly4Lookup[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != self->out4) {
					self->out4 = newOut;
					self->delta4 = -self->delta4;
					Pokey_AddDelta(self, pokeys, cycle, self->delta4);
				}
			}
		}
		if (cycle == self->tickCycle1) {
			self->tickCycle1 += self->periodCycles1;
			if ((self->skctl & 136) == 8)
				self->tickCycle2 = cycle + self->periodCycles2;
			if (self->init) {
				switch (self->audc1 >> 4) {
					case 10:
					case 14:
						self->out1 ^= 1;
						self->delta1 = -self->delta1;
						Pokey_AddDelta(self, pokeys, cycle, self->delta1);
						break;
					default:
						break;
				}
			}
			else {
				int poly = cycle + self->polyIndex - 0;
				int newOut = self->out1;
				switch (self->audc1 >> 4) {
					case 0:
						if (poly5Lookup[poly % 31] != 0) {
							if ((self->audctl & 128) != 0)
								newOut = pokeys->poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= poly5Lookup[poly % 31];
						break;
					case 4:
						if (poly5Lookup[poly % 31] != 0)
							newOut = poly4Lookup[poly % 15];
						break;
					case 8:
						if ((self->audctl & 128) != 0)
							newOut = pokeys->poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = poly4Lookup[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != self->out1) {
					self->out1 = newOut;
					self->delta1 = -self->delta1;
					Pokey_AddDelta(self, pokeys, cycle, self->delta1);
				}
			}
		}
		if (cycle == self->tickCycle2) {
			self->tickCycle2 += self->periodCycles2;
			if ((self->audctl & 16) != 0)
				self->tickCycle1 = cycle + self->reloadCycles1;
			else if ((self->skctl & 8) != 0)
				self->tickCycle1 = cycle + self->periodCycles1;
			if (self->init) {
				switch (self->audc2 >> 4) {
					case 10:
					case 14:
						self->out2 ^= 1;
						self->delta2 = -self->delta2;
						Pokey_AddDelta(self, pokeys, cycle, self->delta2);
						break;
					default:
						break;
				}
			}
			else {
				int poly = cycle + self->polyIndex - 1;
				int newOut = self->out2;
				switch (self->audc2 >> 4) {
					case 0:
						if (poly5Lookup[poly % 31] != 0) {
							if ((self->audctl & 128) != 0)
								newOut = pokeys->poly9Lookup[poly % 511] & 1;
							else {
								poly %= 131071;
								newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
							}
						}
						break;
					case 2:
					case 6:
						newOut ^= poly5Lookup[poly % 31];
						break;
					case 4:
						if (poly5Lookup[poly % 31] != 0)
							newOut = poly4Lookup[poly % 15];
						break;
					case 8:
						if ((self->audctl & 128) != 0)
							newOut = pokeys->poly9Lookup[poly % 511] & 1;
						else {
							poly %= 131071;
							newOut = pokeys->poly17Lookup[poly >> 3] >> (poly & 7) & 1;
						}
						break;
					case 10:
					case 14:
						newOut ^= 1;
						break;
					case 12:
						newOut = poly4Lookup[poly % 15];
						break;
					default:
						break;
				}
				if (newOut != self->out2) {
					self->out2 = newOut;
					self->delta2 = -self->delta2;
					Pokey_AddDelta(self, pokeys, cycle, self->delta2);
				}
			}
		}
	}
}

static void Pokey_Initialize(Pokey *self)
{
	self->audf1 = 0;
	self->audf2 = 0;
	self->audf3 = 0;
	self->audf4 = 0;
	self->audc1 = 0;
	self->audc2 = 0;
	self->audc3 = 0;
	self->audc4 = 0;
	self->audctl = 0;
	self->skctl = 3;
	self->init = false;
	self->divCycles = 28;
	self->periodCycles1 = 28;
	self->periodCycles2 = 28;
	self->periodCycles3 = 28;
	self->periodCycles4 = 28;
	self->reloadCycles1 = 28;
	self->reloadCycles3 = 28;
	self->polyIndex = 60948015;
	self->tickCycle1 = 8388608;
	self->tickCycle2 = 8388608;
	self->tickCycle3 = 8388608;
	self->tickCycle4 = 8388608;
	self->mute1 = 1;
	self->mute2 = 1;
	self->mute3 = 1;
	self->mute4 = 1;
	self->out1 = 0;
	self->out2 = 0;
	self->out3 = 0;
	self->out4 = 0;
	self->delta1 = 0;
	self->delta2 = 0;
	self->delta3 = 0;
	self->delta4 = 0;
	memset(self->deltaBuffer, 0, sizeof(self->deltaBuffer));
}

static void PokeyPair_Construct(PokeyPair *self)
{
	int reg = 511;
	for (int i = 0; i < 511; i++) {
		reg = (((reg >> 5 ^ reg) & 1) << 8) + (reg >> 1);
		self->poly9Lookup[i] = (unsigned char) reg;
	}
	reg = 131071;
	for (int i = 0; i < 16385; i++) {
		reg = (((reg >> 5 ^ reg) & 255) << 9) + (reg >> 8);
		self->poly17Lookup[i] = (unsigned char) (reg >> 1);
	}
}

PokeyPair *PokeyPair_New(void)
{
	PokeyPair *self = malloc(sizeof(PokeyPair));
	if (self != NULL)
		PokeyPair_Construct(self);
	return self;
}

void PokeyPair_Delete(PokeyPair *self)
{
	free(self);
}

int PokeyPair_EndFrame(PokeyPair *self, int cycle)
{
	Pokey_EndFrame(&self->basePokey, self, cycle);
	if (self->extraPokeyMask != 0)
		Pokey_EndFrame(&self->extraPokey, self, cycle);
	self->sampleOffset += cycle * 44100;
	self->sampleIndex = 0;
	self->samples = self->sampleOffset / self->mainClock;
	self->sampleOffset %= self->mainClock;
	return self->samples;
}

int PokeyPair_Generate(PokeyPair *self, unsigned char *buffer, int bufferOffset, int blocks, ASAPSampleFormat format)
{
	int i = self->sampleIndex;
	int samples = self->samples;
	int accLeft = self->iirAccLeft;
	int accRight = self->iirAccRight;
	if (blocks < samples - i)
		samples = i + blocks;
	else
		blocks = samples - i;
	for (; i < samples; i++) {
		accLeft += self->basePokey.deltaBuffer[i] - (accLeft * 3 >> 10);
		int sample = accLeft >> 10;
		if (sample < -32767)
			sample = -32767;
		else if (sample > 32767)
			sample = 32767;
		switch (format) {
			case ASAPSampleFormat_U8:
				buffer[bufferOffset++] = (sample >> 8) + 128;
				break;
			case ASAPSampleFormat_S16_L_E:
				buffer[bufferOffset++] = (unsigned char) sample;
				buffer[bufferOffset++] = (unsigned char) (sample >> 8);
				break;
			case ASAPSampleFormat_S16_B_E:
				buffer[bufferOffset++] = (unsigned char) (sample >> 8);
				buffer[bufferOffset++] = (unsigned char) sample;
				break;
		}
		if (self->extraPokeyMask != 0) {
			accRight += self->extraPokey.deltaBuffer[i] - (accRight * 3 >> 10);
			sample = accRight >> 10;
			if (sample < -32767)
				sample = -32767;
			else if (sample > 32767)
				sample = 32767;
			switch (format) {
				case ASAPSampleFormat_U8:
					buffer[bufferOffset++] = (sample >> 8) + 128;
					break;
				case ASAPSampleFormat_S16_L_E:
					buffer[bufferOffset++] = (unsigned char) sample;
					buffer[bufferOffset++] = (unsigned char) (sample >> 8);
					break;
				case ASAPSampleFormat_S16_B_E:
					buffer[bufferOffset++] = (unsigned char) (sample >> 8);
					buffer[bufferOffset++] = (unsigned char) sample;
					break;
			}
		}
	}
	if (i == self->samples) {
		accLeft += self->basePokey.deltaBuffer[i];
		accRight += self->extraPokey.deltaBuffer[i];
	}
	self->sampleIndex = i;
	self->iirAccLeft = accLeft;
	self->iirAccRight = accRight;
	return bufferOffset;
}

int PokeyPair_GetRandom(PokeyPair const *self, int addr, int cycle)
{
	Pokey const *pokey = (addr & self->extraPokeyMask) != 0 ? &self->extraPokey : &self->basePokey;
	if (pokey->init)
		return 255;
	int i = cycle + pokey->polyIndex;
	if ((pokey->audctl & 128) != 0)
		return self->poly9Lookup[i % 511];
	else {
		i %= 131071;
		int j = i >> 3;
		i &= 7;
		return (self->poly17Lookup[j] >> i) + (self->poly17Lookup[j + 1] << 8 - i) & 255;
	}
}

void PokeyPair_Initialize(PokeyPair *self, int mainClock, bool stereo)
{
	self->mainClock = mainClock;
	self->extraPokeyMask = stereo ? 16 : 0;
	self->timer1Cycle = 8388608;
	self->timer2Cycle = 8388608;
	self->timer4Cycle = 8388608;
	self->irqst = 255;
	Pokey_Initialize(&self->basePokey);
	Pokey_Initialize(&self->extraPokey);
	self->sampleOffset = 0;
	self->sampleIndex = 0;
	self->samples = 0;
	self->iirAccLeft = 0;
	self->iirAccRight = 0;
}

void PokeyPair_Poke(PokeyPair *self, int addr, int data, int cycle)
{
	Pokey *pokey = (addr & self->extraPokeyMask) != 0 ? &self->extraPokey : &self->basePokey;
	switch (addr & 15) {
		case 0:
			if (data == pokey->audf1)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audf1 = data;
			switch (pokey->audctl & 80) {
				case 0:
					pokey->periodCycles1 = pokey->divCycles * (data + 1);
					break;
				case 16:
					pokey->periodCycles2 = pokey->divCycles * (data + (pokey->audf2 << 8) + 1);
					pokey->reloadCycles1 = pokey->divCycles * (data + 1);
					if (pokey->periodCycles2 <= 112 && (pokey->audc2 >> 4 == 10 || pokey->audc2 >> 4 == 14)) {
						pokey->mute2 |= 1;
						pokey->tickCycle2 = 8388608;
					}
					else {
						pokey->mute2 &= ~1;
						if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
							pokey->tickCycle2 = cycle;
					}
					break;
				case 64:
					pokey->periodCycles1 = data + 4;
					break;
				case 80:
					pokey->periodCycles2 = data + (pokey->audf2 << 8) + 7;
					pokey->reloadCycles1 = data + 4;
					if (pokey->periodCycles2 <= 112 && (pokey->audc2 >> 4 == 10 || pokey->audc2 >> 4 == 14)) {
						pokey->mute2 |= 1;
						pokey->tickCycle2 = 8388608;
					}
					else {
						pokey->mute2 &= ~1;
						if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
							pokey->tickCycle2 = cycle;
					}
					break;
			}
			if (pokey->periodCycles1 <= 112 && (pokey->audc1 >> 4 == 10 || pokey->audc1 >> 4 == 14)) {
				pokey->mute1 |= 1;
				pokey->tickCycle1 = 8388608;
			}
			else {
				pokey->mute1 &= ~1;
				if (pokey->tickCycle1 == 8388608 && pokey->mute1 == 0)
					pokey->tickCycle1 = cycle;
			}
			break;
		case 1:
			if (data == pokey->audc1)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audc1 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey->mute1 & 4) == 0)
					Pokey_AddDelta(pokey, self, cycle, pokey->delta1 > 0 ? data - pokey->delta1 : data);
				pokey->delta1 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey->periodCycles1 <= 112 && (pokey->audc1 >> 4 == 10 || pokey->audc1 >> 4 == 14)) {
					pokey->mute1 |= 1;
					pokey->tickCycle1 = 8388608;
				}
				else {
					pokey->mute1 &= ~1;
					if (pokey->tickCycle1 == 8388608 && pokey->mute1 == 0)
						pokey->tickCycle1 = cycle;
				}
				if (pokey->delta1 > 0) {
					if ((pokey->mute1 & 4) == 0)
						Pokey_AddDelta(pokey, self, cycle, data - pokey->delta1);
					pokey->delta1 = data;
				}
				else
					pokey->delta1 = -data;
			}
			break;
		case 2:
			if (data == pokey->audf2)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audf2 = data;
			switch (pokey->audctl & 80) {
				case 0:
				case 64:
					pokey->periodCycles2 = pokey->divCycles * (data + 1);
					break;
				case 16:
					pokey->periodCycles2 = pokey->divCycles * (pokey->audf1 + (data << 8) + 1);
					break;
				case 80:
					pokey->periodCycles2 = pokey->audf1 + (data << 8) + 7;
					break;
			}
			if (pokey->periodCycles2 <= 112 && (pokey->audc2 >> 4 == 10 || pokey->audc2 >> 4 == 14)) {
				pokey->mute2 |= 1;
				pokey->tickCycle2 = 8388608;
			}
			else {
				pokey->mute2 &= ~1;
				if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
					pokey->tickCycle2 = cycle;
			}
			break;
		case 3:
			if (data == pokey->audc2)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audc2 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey->mute2 & 4) == 0)
					Pokey_AddDelta(pokey, self, cycle, pokey->delta2 > 0 ? data - pokey->delta2 : data);
				pokey->delta2 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey->periodCycles2 <= 112 && (pokey->audc2 >> 4 == 10 || pokey->audc2 >> 4 == 14)) {
					pokey->mute2 |= 1;
					pokey->tickCycle2 = 8388608;
				}
				else {
					pokey->mute2 &= ~1;
					if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
						pokey->tickCycle2 = cycle;
				}
				if (pokey->delta2 > 0) {
					if ((pokey->mute2 & 4) == 0)
						Pokey_AddDelta(pokey, self, cycle, data - pokey->delta2);
					pokey->delta2 = data;
				}
				else
					pokey->delta2 = -data;
			}
			break;
		case 4:
			if (data == pokey->audf3)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audf3 = data;
			switch (pokey->audctl & 40) {
				case 0:
					pokey->periodCycles3 = pokey->divCycles * (data + 1);
					break;
				case 8:
					pokey->periodCycles4 = pokey->divCycles * (data + (pokey->audf4 << 8) + 1);
					pokey->reloadCycles3 = pokey->divCycles * (data + 1);
					if (pokey->periodCycles4 <= 112 && (pokey->audc4 >> 4 == 10 || pokey->audc4 >> 4 == 14)) {
						pokey->mute4 |= 1;
						pokey->tickCycle4 = 8388608;
					}
					else {
						pokey->mute4 &= ~1;
						if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
							pokey->tickCycle4 = cycle;
					}
					break;
				case 32:
					pokey->periodCycles3 = data + 4;
					break;
				case 40:
					pokey->periodCycles4 = data + (pokey->audf4 << 8) + 7;
					pokey->reloadCycles3 = data + 4;
					if (pokey->periodCycles4 <= 112 && (pokey->audc4 >> 4 == 10 || pokey->audc4 >> 4 == 14)) {
						pokey->mute4 |= 1;
						pokey->tickCycle4 = 8388608;
					}
					else {
						pokey->mute4 &= ~1;
						if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
							pokey->tickCycle4 = cycle;
					}
					break;
			}
			if (pokey->periodCycles3 <= 112 && (pokey->audc3 >> 4 == 10 || pokey->audc3 >> 4 == 14)) {
				pokey->mute3 |= 1;
				pokey->tickCycle3 = 8388608;
			}
			else {
				pokey->mute3 &= ~1;
				if (pokey->tickCycle3 == 8388608 && pokey->mute3 == 0)
					pokey->tickCycle3 = cycle;
			}
			break;
		case 5:
			if (data == pokey->audc3)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audc3 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey->mute3 & 4) == 0)
					Pokey_AddDelta(pokey, self, cycle, pokey->delta3 > 0 ? data - pokey->delta3 : data);
				pokey->delta3 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey->periodCycles3 <= 112 && (pokey->audc3 >> 4 == 10 || pokey->audc3 >> 4 == 14)) {
					pokey->mute3 |= 1;
					pokey->tickCycle3 = 8388608;
				}
				else {
					pokey->mute3 &= ~1;
					if (pokey->tickCycle3 == 8388608 && pokey->mute3 == 0)
						pokey->tickCycle3 = cycle;
				}
				if (pokey->delta3 > 0) {
					if ((pokey->mute3 & 4) == 0)
						Pokey_AddDelta(pokey, self, cycle, data - pokey->delta3);
					pokey->delta3 = data;
				}
				else
					pokey->delta3 = -data;
			}
			break;
		case 6:
			if (data == pokey->audf4)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audf4 = data;
			switch (pokey->audctl & 40) {
				case 0:
				case 32:
					pokey->periodCycles4 = pokey->divCycles * (data + 1);
					break;
				case 8:
					pokey->periodCycles4 = pokey->divCycles * (pokey->audf3 + (data << 8) + 1);
					break;
				case 40:
					pokey->periodCycles4 = pokey->audf3 + (data << 8) + 7;
					break;
			}
			if (pokey->periodCycles4 <= 112 && (pokey->audc4 >> 4 == 10 || pokey->audc4 >> 4 == 14)) {
				pokey->mute4 |= 1;
				pokey->tickCycle4 = 8388608;
			}
			else {
				pokey->mute4 &= ~1;
				if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
					pokey->tickCycle4 = cycle;
			}
			break;
		case 7:
			if (data == pokey->audc4)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audc4 = data;
			if ((data & 16) != 0) {
				data = (data & 15) << 20;
				if ((pokey->mute4 & 4) == 0)
					Pokey_AddDelta(pokey, self, cycle, pokey->delta4 > 0 ? data - pokey->delta4 : data);
				pokey->delta4 = data;
			}
			else {
				data = (data & 15) << 20;
				if (pokey->periodCycles4 <= 112 && (pokey->audc4 >> 4 == 10 || pokey->audc4 >> 4 == 14)) {
					pokey->mute4 |= 1;
					pokey->tickCycle4 = 8388608;
				}
				else {
					pokey->mute4 &= ~1;
					if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
						pokey->tickCycle4 = cycle;
				}
				if (pokey->delta4 > 0) {
					if ((pokey->mute4 & 4) == 0)
						Pokey_AddDelta(pokey, self, cycle, data - pokey->delta4);
					pokey->delta4 = data;
				}
				else
					pokey->delta4 = -data;
			}
			break;
		case 8:
			if (data == pokey->audctl)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->audctl = data;
			pokey->divCycles = (data & 1) != 0 ? 114 : 28;
			switch (data & 80) {
				case 0:
					pokey->periodCycles1 = pokey->divCycles * (pokey->audf1 + 1);
					pokey->periodCycles2 = pokey->divCycles * (pokey->audf2 + 1);
					break;
				case 16:
					pokey->periodCycles1 = pokey->divCycles << 8;
					pokey->periodCycles2 = pokey->divCycles * (pokey->audf1 + (pokey->audf2 << 8) + 1);
					pokey->reloadCycles1 = pokey->divCycles * (pokey->audf1 + 1);
					break;
				case 64:
					pokey->periodCycles1 = pokey->audf1 + 4;
					pokey->periodCycles2 = pokey->divCycles * (pokey->audf2 + 1);
					break;
				case 80:
					pokey->periodCycles1 = 256;
					pokey->periodCycles2 = pokey->audf1 + (pokey->audf2 << 8) + 7;
					pokey->reloadCycles1 = pokey->audf1 + 4;
					break;
			}
			if (pokey->periodCycles1 <= 112 && (pokey->audc1 >> 4 == 10 || pokey->audc1 >> 4 == 14)) {
				pokey->mute1 |= 1;
				pokey->tickCycle1 = 8388608;
			}
			else {
				pokey->mute1 &= ~1;
				if (pokey->tickCycle1 == 8388608 && pokey->mute1 == 0)
					pokey->tickCycle1 = cycle;
			}
			if (pokey->periodCycles2 <= 112 && (pokey->audc2 >> 4 == 10 || pokey->audc2 >> 4 == 14)) {
				pokey->mute2 |= 1;
				pokey->tickCycle2 = 8388608;
			}
			else {
				pokey->mute2 &= ~1;
				if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
					pokey->tickCycle2 = cycle;
			}
			switch (data & 40) {
				case 0:
					pokey->periodCycles3 = pokey->divCycles * (pokey->audf3 + 1);
					pokey->periodCycles4 = pokey->divCycles * (pokey->audf4 + 1);
					break;
				case 8:
					pokey->periodCycles3 = pokey->divCycles << 8;
					pokey->periodCycles4 = pokey->divCycles * (pokey->audf3 + (pokey->audf4 << 8) + 1);
					pokey->reloadCycles3 = pokey->divCycles * (pokey->audf3 + 1);
					break;
				case 32:
					pokey->periodCycles3 = pokey->audf3 + 4;
					pokey->periodCycles4 = pokey->divCycles * (pokey->audf4 + 1);
					break;
				case 40:
					pokey->periodCycles3 = 256;
					pokey->periodCycles4 = pokey->audf3 + (pokey->audf4 << 8) + 7;
					pokey->reloadCycles3 = pokey->audf3 + 4;
					break;
			}
			if (pokey->periodCycles3 <= 112 && (pokey->audc3 >> 4 == 10 || pokey->audc3 >> 4 == 14)) {
				pokey->mute3 |= 1;
				pokey->tickCycle3 = 8388608;
			}
			else {
				pokey->mute3 &= ~1;
				if (pokey->tickCycle3 == 8388608 && pokey->mute3 == 0)
					pokey->tickCycle3 = cycle;
			}
			if (pokey->periodCycles4 <= 112 && (pokey->audc4 >> 4 == 10 || pokey->audc4 >> 4 == 14)) {
				pokey->mute4 |= 1;
				pokey->tickCycle4 = 8388608;
			}
			else {
				pokey->mute4 &= ~1;
				if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
					pokey->tickCycle4 = cycle;
			}
			if (pokey->init && (data & 64) == 0) {
				pokey->mute1 |= 2;
				pokey->tickCycle1 = 8388608;
			}
			else {
				pokey->mute1 &= ~2;
				if (pokey->tickCycle1 == 8388608 && pokey->mute1 == 0)
					pokey->tickCycle1 = cycle;
			}
			if (pokey->init && (data & 80) != 80) {
				pokey->mute2 |= 2;
				pokey->tickCycle2 = 8388608;
			}
			else {
				pokey->mute2 &= ~2;
				if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
					pokey->tickCycle2 = cycle;
			}
			if (pokey->init && (data & 32) == 0) {
				pokey->mute3 |= 2;
				pokey->tickCycle3 = 8388608;
			}
			else {
				pokey->mute3 &= ~2;
				if (pokey->tickCycle3 == 8388608 && pokey->mute3 == 0)
					pokey->tickCycle3 = cycle;
			}
			if (pokey->init && (data & 40) != 40) {
				pokey->mute4 |= 2;
				pokey->tickCycle4 = 8388608;
			}
			else {
				pokey->mute4 &= ~2;
				if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
					pokey->tickCycle4 = cycle;
			}
			break;
		case 9:
			if (pokey->tickCycle1 != 8388608)
				pokey->tickCycle1 = cycle + pokey->periodCycles1;
			if (pokey->tickCycle2 != 8388608)
				pokey->tickCycle2 = cycle + pokey->periodCycles2;
			if (pokey->tickCycle3 != 8388608)
				pokey->tickCycle3 = cycle + pokey->periodCycles3;
			if (pokey->tickCycle4 != 8388608)
				pokey->tickCycle4 = cycle + pokey->periodCycles4;
			break;
		case 15:
			if (data == pokey->skctl)
				break;
			Pokey_GenerateUntilCycle(pokey, self, cycle);
			pokey->skctl = data;
			bool init = (data & 3) == 0;
			if (pokey->init && !init)
				pokey->polyIndex = ((pokey->audctl & 128) != 0 ? 237614 : 60948014) - cycle;
			pokey->init = init;
			if (pokey->init && (pokey->audctl & 64) == 0) {
				pokey->mute1 |= 2;
				pokey->tickCycle1 = 8388608;
			}
			else {
				pokey->mute1 &= ~2;
				if (pokey->tickCycle1 == 8388608 && pokey->mute1 == 0)
					pokey->tickCycle1 = cycle;
			}
			if (pokey->init && (pokey->audctl & 80) != 80) {
				pokey->mute2 |= 2;
				pokey->tickCycle2 = 8388608;
			}
			else {
				pokey->mute2 &= ~2;
				if (pokey->tickCycle2 == 8388608 && pokey->mute2 == 0)
					pokey->tickCycle2 = cycle;
			}
			if (pokey->init && (pokey->audctl & 32) == 0) {
				pokey->mute3 |= 2;
				pokey->tickCycle3 = 8388608;
			}
			else {
				pokey->mute3 &= ~2;
				if (pokey->tickCycle3 == 8388608 && pokey->mute3 == 0)
					pokey->tickCycle3 = cycle;
			}
			if (pokey->init && (pokey->audctl & 40) != 40) {
				pokey->mute4 |= 2;
				pokey->tickCycle4 = 8388608;
			}
			else {
				pokey->mute4 &= ~2;
				if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
					pokey->tickCycle4 = cycle;
			}
			if ((data & 16) != 0) {
				pokey->mute3 |= 8;
				pokey->tickCycle3 = 8388608;
			}
			else {
				pokey->mute3 &= ~8;
				if (pokey->tickCycle3 == 8388608 && pokey->mute3 == 0)
					pokey->tickCycle3 = cycle;
			}
			if ((data & 16) != 0) {
				pokey->mute4 |= 8;
				pokey->tickCycle4 = 8388608;
			}
			else {
				pokey->mute4 &= ~8;
				if (pokey->tickCycle4 == 8388608 && pokey->mute4 == 0)
					pokey->tickCycle4 = cycle;
			}
			break;
		default:
			break;
	}
}

void PokeyPair_StartFrame(PokeyPair *self)
{
	memset(self->basePokey.deltaBuffer, 0, sizeof(self->basePokey.deltaBuffer));
	if (self->extraPokeyMask != 0)
		memset(self->extraPokey.deltaBuffer, 0, sizeof(self->extraPokey.deltaBuffer));
}
