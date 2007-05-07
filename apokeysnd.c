/*
 * apokeysnd.c - another POKEY sound emulator
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

#include <string.h>

#include "apokeysnd.h"

#define SAMPLE_RATE  44100
#define MAIN_CLOCK   1773447

typedef struct {
	int audctl;
	int poly_index;
	int div_cycles;
	int audf1;
	int audf2;
	int audf3;
	int audf4;
	int audc1;
	int audc2;
	int audc3;
	int audc4;
	int tick_cycle1;
	int tick_cycle2;
	int tick_cycle3;
	int tick_cycle4;
	int period_cycles1;
	int period_cycles2;
	int period_cycles3;
	int period_cycles4;
	int reload_cycles1;
	int reload_cycles3;
	int out1;
	int out2;
	int out3;
	int out4;
	int delta1;
	int delta2;
	int delta3;
	int delta4;
	char delta_buffer[1024];
} PokeyState;

static PokeyState pokey_states[2];

static const unsigned char poly4_lookup[15] =
	{ 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
static const unsigned char poly5_lookup[31] =
	{ 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1,
	  0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };
static unsigned char poly9_lookup[511];
static unsigned char poly17_lookup[16385];

static int enable_stereo;
unsigned char atari_sound[2048];
int atari_sound_len;
static int sample_offset;
static int iir_acc[2];

static void init_state(PokeyState *ps)
{
	ps->audctl = 0;
	ps->poly_index = 0;
	ps->div_cycles = 28;
	ps->audf1 = 0;
	ps->audf2 = 0;
	ps->audf3 = 0;
	ps->audf4 = 0;
	ps->audc1 = 0;
	ps->audc2 = 0;
	ps->audc3 = 0;
	ps->audc4 = 0;
	ps->tick_cycle1 = 0;
	ps->tick_cycle2 = 0;
	ps->tick_cycle3 = 0;
	ps->tick_cycle4 = 0;
	ps->period_cycles1 = 28;
	ps->period_cycles2 = 28;
	ps->period_cycles3 = 28;
	ps->period_cycles4 = 28;
	ps->reload_cycles1 = 28;
	ps->reload_cycles3 = 28;
	ps->out1 = 0;
	ps->out2 = 0;
	ps->out3 = 0;
	ps->out4 = 0;
	ps->delta1 = 0;
	ps->delta2 = 0;
	ps->delta3 = 0;
	ps->delta4 = 0;
	memset(ps->delta_buffer, 0, sizeof(ps->delta_buffer));
}

void PokeySound_Initialize(int stereo)
{
	static int poly9_17_initialized = 0;
	enable_stereo = stereo;
	if (!poly9_17_initialized) {
		int i;
		int reg;
		poly9_17_initialized = 1;
		reg = 0x1ff;
		for (i = 0; i < 511; i++) {
			reg = ((((reg >> 5) ^ reg) & 1) << 8) + (reg >> 1);
			poly9_lookup[i] = (unsigned char) reg;
		}
		reg = 0x1ffff;
		for (i = 0; i < 16385; i++) {
			reg = ((((reg >> 5) ^ reg) & 0xff) << 9) + (reg >> 8);
			poly17_lookup[i] = (unsigned char) (reg >> 1);
		}
	}
	init_state(pokey_states);
	init_state(pokey_states + 1);
}

#define CYCLE_TO_SAMPLE(cycle)  (((cycle) * SAMPLE_RATE + sample_offset) / MAIN_CLOCK)

#define DO_TICK(ch) \
	poly = cycle + ps->poly_index - (ch - 1); \
	newout = ps->out##ch; \
	switch (ps->audc##ch >> 4) { \
	case 0: \
		if (poly5_lookup[poly % 31]) { \
			if ((ps->audctl & 0x80) != 0) \
				newout = poly9_lookup[poly % 511] & 1; \
			else { \
				poly %= 131071; \
				newout = (poly17_lookup[poly >> 3] >> (poly & 7)) & 1; \
			} \
		} \
		break; \
	case 2: \
	case 6: \
		newout ^= poly5_lookup[poly % 31]; \
		break; \
	case 4:\
		if (poly5_lookup[poly % 31]) \
			newout = poly4_lookup[poly % 15]; \
		break; \
	case 8: \
		if ((ps->audctl & 0x80) != 0) \
			newout = poly9_lookup[poly % 511] & 1; \
		else { \
			poly %= 131071; \
			newout = (poly17_lookup[poly >> 3] >> (poly & 7)) & 1; \
		} \
		break; \
	case 10: \
	case 14: \
		newout ^= 1; \
		break; \
	case 12: \
		newout = poly4_lookup[poly % 15]; \
		break; \
	default: \
		break; \
	} \
	if (newout != ps->out##ch) { \
		ps->out##ch = newout; \
		ps->delta_buffer[CYCLE_TO_SAMPLE(cycle)] += ps->delta##ch = -ps->delta##ch; \
	}

static void generate(PokeyState *ps, int current_cycle)
{
	for (;;) {
		int cycle = current_cycle;
		int poly;
		int newout;
		if (cycle > ps->tick_cycle1)
			cycle = ps->tick_cycle1;
		if (cycle > ps->tick_cycle2)
			cycle = ps->tick_cycle2;
		if (cycle > ps->tick_cycle3)
			cycle = ps->tick_cycle3;
		if (cycle > ps->tick_cycle4)
			cycle = ps->tick_cycle4;
		if (cycle == current_cycle)
			break;
		if (cycle == ps->tick_cycle1) {
			ps->tick_cycle1 += ps->period_cycles1;
			DO_TICK(1);
		}
		if (cycle == ps->tick_cycle2) {
			ps->tick_cycle2 += ps->period_cycles2;
			if ((ps->audctl & 0x10) != 0)
				ps->tick_cycle1 += ps->reload_cycles1;
			DO_TICK(2);
		}
		if (cycle == ps->tick_cycle3) {
			ps->tick_cycle3 += ps->period_cycles3;
			if ((ps->audctl & 4) != 0 && ps->delta1 > 0)
				ps->delta_buffer[CYCLE_TO_SAMPLE(cycle)] += ps->delta1 = -ps->delta1;
			DO_TICK(3);
		}
		if (cycle == ps->tick_cycle4) {
			ps->tick_cycle4 += ps->period_cycles4;
			if ((ps->audctl & 8) != 0)
				ps->tick_cycle3 += ps->reload_cycles3;
			if ((ps->audctl & 2) != 0 && ps->delta2 > 0)
				ps->delta_buffer[CYCLE_TO_SAMPLE(cycle)] += ps->delta2 = -ps->delta2;
			DO_TICK(4);
		}
	}
}

#define DO_AUDC(ch) \
	if (data == ps->audc##ch) \
		break; \
	generate(ps, current_cycle); \
	ps->audc##ch = data; \
	if ((data & 0x10) != 0) { \
		data &= 0xf; \
		ps->delta_buffer[CYCLE_TO_SAMPLE(current_cycle)] \
			+= ps->delta##ch > 0 ? data - ps->delta##ch : data; \
		ps->delta##ch = data; \
	} \
	else { \
		data &= 0xf; \
		if (ps->delta##ch > 0) { \
			ps->delta_buffer[CYCLE_TO_SAMPLE(current_cycle)] \
				+= data - ps->delta##ch; \
			ps->delta##ch = data; \
		} \
		else \
			ps->delta##ch = -data; \
	} \
	break;

void PokeySound_PutByte(int addr, int data, int current_cycle)
{
	PokeyState *ps = (addr & 0x10) != 0 && enable_stereo ? pokey_states + 1 : pokey_states;
	switch (addr & 0xf) {
	case 0x00:
		if (data == ps->audf1)
			break;
		generate(ps, current_cycle);
		ps->audf1 = data;
		switch (ps->audctl & 0x50) {
		case 0x00:
			ps->period_cycles1 = ps->div_cycles * (data + 1);
			break;
		case 0x10:
			ps->period_cycles2 = ps->div_cycles * (data + 256 * ps->audf2 + 1);
			ps->reload_cycles1 = ps->div_cycles * (data + 1);
			break;
		case 0x40:
			ps->period_cycles1 = data + 4;
			break;
		case 0x50:
			ps->period_cycles2 = data + 256 * ps->audf2 + 7;
			ps->reload_cycles1 = data + 7;
			break;
		}
		break;
	case 0x01:
		DO_AUDC(1);
	case 0x02:
		if (data == ps->audf2)
			break;
		generate(ps, current_cycle);
		ps->audf2 = data;
		switch (ps->audctl & 0x50) {
		case 0x00:
		case 0x40:
			ps->period_cycles2 = ps->div_cycles * (data + 1);
			break;
		case 0x10:
			ps->period_cycles2 = ps->div_cycles * (ps->audf1 + 256 * data + 1);
			break;
		case 0x50:
			ps->period_cycles2 = ps->audf1 + 256 * data + 7;
			break;
		}
		break;
	case 0x03:
		DO_AUDC(2);
	case 0x04:
		if (data == ps->audf3)
			break;
		generate(ps, current_cycle);
		ps->audf3 = data;
		switch (ps->audctl & 0x28) {
		case 0x00:
			ps->period_cycles3 = ps->div_cycles * (data + 1);
			break;
		case 0x08:
			ps->period_cycles4 = ps->div_cycles * (data + 256 * ps->audf4 + 1);
			ps->reload_cycles3 = ps->div_cycles * (data + 1);
			break;
		case 0x20:
			ps->period_cycles3 = data + 4;
			break;
		case 0x28:
			ps->period_cycles4 = data + 256 * ps->audf4 + 7;
			ps->reload_cycles3 = data + 7;
			break;
		}
		break;
	case 0x05:
		DO_AUDC(3);
	case 0x06:
		if (data == ps->audf4)
			break;
		generate(ps, current_cycle);
		ps->audf4 = data;
		switch (ps->audctl & 0x28) {
		case 0x00:
		case 0x20:
			ps->period_cycles4 = ps->div_cycles * (data + 1);
			break;
		case 0x08:
			ps->period_cycles4 = ps->div_cycles * (ps->audf3 + 256 * data + 1);
			break;
		case 0x28:
			ps->period_cycles4 = ps->audf3 + 256 * data + 7;
			break;
		}
		break;
	case 0x07:
		DO_AUDC(4);
	case 0x08:
		if (data == ps->audctl)
			break;
		generate(ps, current_cycle);
		ps->audctl = data;
		ps->div_cycles = ((data & 1) != 0) ? 114 : 28;
		/* TODO: tick_cycles */
		switch (ps->audctl & 0x50) {
		case 0x00:
			ps->period_cycles1 = ps->div_cycles * (ps->audf1 + 1);
			ps->period_cycles2 = ps->div_cycles * (ps->audf2 + 1);
			break;
		case 0x10:
			ps->period_cycles1 = ps->div_cycles * 256;
			ps->period_cycles2 = ps->div_cycles * (ps->audf1 + 256 * ps->audf2 + 1);
			ps->reload_cycles1 = ps->div_cycles * (ps->audf1 + 1);
			break;
		case 0x40:
			ps->period_cycles1 = ps->audf1 + 4;
			ps->period_cycles2 = ps->div_cycles * (ps->audf2 + 1);
			break;
		case 0x50:
			ps->period_cycles1 = 256;
			ps->period_cycles2 = ps->audf1 + 256 * ps->audf2 + 7;
			ps->reload_cycles1 = ps->audf1 + 7;
			break;
		}
		switch (ps->audctl & 0x28) {
		case 0x00:
			ps->period_cycles3 = ps->div_cycles * (ps->audf3 + 1);
			ps->period_cycles4 = ps->div_cycles * (ps->audf4 + 1);
			break;
		case 0x08:
			ps->period_cycles3 = ps->div_cycles * 256;
			ps->period_cycles4 = ps->div_cycles * (ps->audf3 + 256 * ps->audf4 + 1);
			ps->reload_cycles3 = ps->div_cycles * (ps->audf3 + 1);
			break;
		case 0x20:
			ps->period_cycles3 = ps->audf3 + 4;
			ps->period_cycles4 = ps->div_cycles * (ps->audf4 + 1);
			break;
		case 0x28:
			ps->period_cycles3 = 256;
			ps->period_cycles4 = ps->audf3 + 256 * ps->audf4 + 7;
			ps->reload_cycles3 = ps->audf3 + 7;
			break;
		}
		break;
	case 0x09:
		/* TODO: STIMER */
		break;
	case 0x0f:
		/* TODO: SKCTLS */
		break;
	default:
		break;
	}
}

int PokeySound_GetRandom(int addr, int current_cycle)
{
	PokeyState *ps = (addr & 0x10) != 0 && enable_stereo ? pokey_states + 1 : pokey_states;
	int i = current_cycle + ps->poly_index;
	if ((ps->audctl & 0x80) != 0)
		return poly9_lookup[i % 511];
	else {
		int j;
		i %= 131071;
		j = i >> 3;
		i &= 7;
		return ((poly17_lookup[j] >> i) + (poly17_lookup[j + 1] << (8 - i))) & 0xff;
	}
}

static void mix_mono(int samples)
{
	int i;
	int acc = iir_acc[0];
	for (i = 0; i < samples; i++) {
		int sample;
		acc += (pokey_states[0].delta_buffer[i] << 10) - (acc >> 8);
		sample = 128 + (acc >> 8);
		if (sample < 0)
			sample = 0;
		else if (sample > 255)
			sample = 255;
		atari_sound[i] = (unsigned char) sample;
	}
	iir_acc[0] = acc;
	atari_sound_len = samples;
}

static void mix_stereo(int samples)
{
	int i;
	int acc0 = iir_acc[0];
	int acc1 = iir_acc[1];
	for (i = 0; i < samples; i++) {
		int sample;
		acc0 += (pokey_states[0].delta_buffer[i] << 10) - (acc0 >> 8);
		sample = 128 + (acc0 >> 8);
		if (sample < 0)
			sample = 0;
		else if (sample > 255)
			sample = 255;
		atari_sound[2 * i] = (unsigned char) sample;
		acc1 += (pokey_states[1].delta_buffer[i] << 10) - (acc1 >> 8);
		sample = 128 + (acc1 >> 8);
		if (sample < 0)
			sample = 0;
		else if (sample > 255)
			sample = 255;
		atari_sound[2 * i + 1] = (unsigned char) sample;
	}
	iir_acc[0] = acc0;
	iir_acc[1] = acc1;
	atari_sound_len = 2 * samples;
}

static void flush(PokeyState *ps, int current_cycle)
{
	int m;
	generate(ps, current_cycle);
	ps->poly_index += current_cycle;
	m = ((ps->audctl & 0x80) != 0) ? 15 * 31 * 511 : 15 * 31 * 131071;
	if (ps->poly_index >= m)
		ps->poly_index -= m;
	ps->tick_cycle1 -= current_cycle;
	ps->tick_cycle2 -= current_cycle;
	ps->tick_cycle3 -= current_cycle;
	ps->tick_cycle4 -= current_cycle;
}

void PokeySound_Flush(int current_cycle)
{
	int samples;
	flush(pokey_states, current_cycle);
	if (enable_stereo)
		flush(pokey_states + 1, current_cycle);
	sample_offset += current_cycle * SAMPLE_RATE;
	samples = sample_offset / MAIN_CLOCK;
	sample_offset %= MAIN_CLOCK;
	if (enable_stereo) {
		mix_stereo(samples);
		memset(pokey_states[1].delta_buffer, 0, sizeof(pokey_states[1].delta_buffer));
	}
	else
		mix_mono(samples);
	memset(pokey_states[0].delta_buffer, 0, sizeof(pokey_states[0].delta_buffer));
}
