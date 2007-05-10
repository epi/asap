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

#include "asap_internal.h"

static const byte poly4_lookup[15] =
	{ 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
static const byte poly5_lookup[31] =
	{ 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1,
	  0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };

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

void PokeySound_Initialize(ASAP_State *as)
{
	int i;
	int reg;
	reg = 0x1ff;
	for (i = 0; i < 511; i++) {
		reg = ((((reg >> 5) ^ reg) & 1) << 8) + (reg >> 1);
		AS poly9_lookup[i] = (byte) reg;
	}
	reg = 0x1ffff;
	for (i = 0; i < 16385; i++) {
		reg = ((((reg >> 5) ^ reg) & 0xff) << 9) + (reg >> 8);
		AS poly17_lookup[i] = (byte) (reg >> 1);
	}
	AS sample_offset = 0;
	AS sample_index = 0;
	AS samples = 0;
	AS iir_acc_left = 0;
	AS iir_acc_right = 0;
	init_state(&AS base_pokey);
	init_state(&AS extra_pokey);
}

#define CYCLE_TO_SAMPLE(cycle)  (((cycle) * ASAP_SAMPLE_RATE + AS sample_offset) / ASAP_MAIN_CLOCK)

#define DO_TICK(ch) \
	poly = cycle + ps->poly_index - (ch - 1); \
	newout = ps->out##ch; \
	switch (ps->audc##ch >> 4) { \
	case 0: \
		if (poly5_lookup[poly % 31]) { \
			if ((ps->audctl & 0x80) != 0) \
				newout = AS poly9_lookup[poly % 511] & 1; \
			else { \
				poly %= 131071; \
				newout = (AS poly17_lookup[poly >> 3] >> (poly & 7)) & 1; \
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
			newout = AS poly9_lookup[poly % 511] & 1; \
		else { \
			poly %= 131071; \
			newout = (AS poly17_lookup[poly >> 3] >> (poly & 7)) & 1; \
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

static void generate(ASAP_State *as, PokeyState *ps, int current_cycle)
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
				ps->tick_cycle1 = cycle + ps->reload_cycles1;
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
				ps->tick_cycle3 = cycle + ps->reload_cycles3;
			if ((ps->audctl & 2) != 0 && ps->delta2 > 0)
				ps->delta_buffer[CYCLE_TO_SAMPLE(cycle)] += ps->delta2 = -ps->delta2;
			DO_TICK(4);
		}
	}
}

#define DO_AUDC(ch) \
	if (data == ps->audc##ch) \
		break; \
	generate(as, ps, current_cycle); \
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

void PokeySound_PutByte(ASAP_State *as, int addr, int data, int current_cycle)
{
	PokeyState *ps = (addr & 0x10) != 0 && AS module_info.channels == 2 ? &AS extra_pokey : &AS base_pokey;
	switch (addr & 0xf) {
	case 0x00:
		if (data == ps->audf1)
			break;
		generate(as, ps, current_cycle);
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
			ps->reload_cycles1 = data + 4;
			break;
		}
		break;
	case 0x01:
		DO_AUDC(1);
	case 0x02:
		if (data == ps->audf2)
			break;
		generate(as, ps, current_cycle);
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
		generate(as, ps, current_cycle);
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
			ps->reload_cycles3 = data + 4;
			break;
		}
		break;
	case 0x05:
		DO_AUDC(3);
	case 0x06:
		if (data == ps->audf4)
			break;
		generate(as, ps, current_cycle);
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
		generate(as, ps, current_cycle);
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
			ps->reload_cycles1 = ps->audf1 + 4;
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
			ps->reload_cycles3 = ps->audf3 + 4;
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

int PokeySound_GetRandom(ASAP_State *as, int addr, int current_cycle)
{
	PokeyState *ps = (addr & 0x10) != 0 && AS module_info.channels == 2 ? &AS extra_pokey : &AS base_pokey;
	int i = current_cycle + ps->poly_index;
	if ((ps->audctl & 0x80) != 0)
		return AS poly9_lookup[i % 511];
	else {
		int j;
		i %= 131071;
		j = i >> 3;
		i &= 7;
		return ((AS poly17_lookup[j] >> i) + (AS poly17_lookup[j + 1] << (8 - i))) & 0xff;
	}
}

static void end_frame(ASAP_State *as, PokeyState *ps, int current_cycle)
{
	int m;
	generate(as, ps, current_cycle);
	ps->poly_index += current_cycle;
	m = ((ps->audctl & 0x80) != 0) ? 15 * 31 * 511 : 15 * 31 * 131071;
	if (ps->poly_index >= m)
		ps->poly_index -= m;
	ps->tick_cycle1 -= current_cycle;
	ps->tick_cycle2 -= current_cycle;
	ps->tick_cycle3 -= current_cycle;
	ps->tick_cycle4 -= current_cycle;
}

void PokeySound_StartFrame(ASAP_State *as)
{
	memset(AS base_pokey.delta_buffer, 0, sizeof(AS base_pokey.delta_buffer));
	if (AS module_info.channels == 2)
		memset(AS extra_pokey.delta_buffer, 0, sizeof(AS extra_pokey.delta_buffer));
}

void PokeySound_EndFrame(ASAP_State *as, int current_cycle)
{
	end_frame(as, &AS base_pokey, current_cycle);
	if (AS module_info.channels == 2)
		end_frame(as, &AS extra_pokey, current_cycle);
	AS sample_offset += current_cycle * ASAP_SAMPLE_RATE;
	AS sample_index = 0;
	AS samples = AS sample_offset / ASAP_MAIN_CLOCK;
	AS sample_offset %= ASAP_MAIN_CLOCK;
}

int PokeySound_Generate(ASAP_State *as, byte *buffer, int blocks, ASAP_SampleFormat format)
{
	int sample_index = AS sample_index;
	int acc_left = AS iir_acc_left;
	int acc_right = AS iir_acc_right;
	int i;
	if (blocks > AS samples - sample_index)
		blocks = AS samples - sample_index;
	for (i = 0; i < blocks; i++) {
		int sample;
		acc_left += (AS base_pokey.delta_buffer[sample_index + i] << 10) - (acc_left >> 8);
		sample = acc_left;
#define STORE_SAMPLE \
		if (sample < -32767) \
			sample = -32767; \
		else if (sample > 32767) \
			sample = 32767; \
		switch (format) { \
		case ASAP_FORMAT_U8: \
			*buffer++ = (byte) ((sample >> 8) + 128); \
			break; \
		case ASAP_FORMAT_S16_LE: \
			*buffer++ = (byte) sample; \
			*buffer++ = (byte) (sample >> 8); \
			break; \
		case ASAP_FORMAT_S16_BE: \
			*buffer++ = (byte) (sample >> 8); \
			*buffer++ = (byte) sample; \
			break; \
		}
		STORE_SAMPLE;
		if (AS module_info.channels == 2) {
			acc_right += (AS extra_pokey.delta_buffer[sample_index + i] << 10) - (acc_right >> 8);
			sample = acc_right;
			STORE_SAMPLE;
		}
	}
	AS sample_index += blocks;
	AS iir_acc_left = acc_left;
	AS iir_acc_right = acc_right;
	return blocks;
}
