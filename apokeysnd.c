/*
 * apokeysnd.c - another POKEY sound emulator
 *
 * Copyright (C) 2007-2008  Piotr Fusik
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

#ifndef JAVA
#include <string.h>
#endif

#include "asap_internal.h"

#define ULTRASOUND_CYCLES  112

#define MUTE_FREQUENCY     1
#define MUTE_INIT          2
#define MUTE_USER          4

CONST_LOOKUP(byte, poly4_lookup) =
	{ 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
CONST_LOOKUP(byte, poly5_lookup) =
	{ 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1,
	  0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };

FILE_FUNC void init_state(PokeyState PTR ps)
{
	PS audctl = 0;
	PS init = FALSE;
	PS poly_index = 15 * 31 * 131071;
	PS div_cycles = 28;
	PS mute1 = MUTE_FREQUENCY | MUTE_USER;
	PS mute2 = MUTE_FREQUENCY | MUTE_USER;
	PS mute3 = MUTE_FREQUENCY | MUTE_USER;
	PS mute4 = MUTE_FREQUENCY | MUTE_USER;
	PS audf1 = 0;
	PS audf2 = 0;
	PS audf3 = 0;
	PS audf4 = 0;
	PS audc1 = 0;
	PS audc2 = 0;
	PS audc3 = 0;
	PS audc4 = 0;
	PS tick_cycle1 = NEVER;
	PS tick_cycle2 = NEVER;
	PS tick_cycle3 = NEVER;
	PS tick_cycle4 = NEVER;
	PS period_cycles1 = 28;
	PS period_cycles2 = 28;
	PS period_cycles3 = 28;
	PS period_cycles4 = 28;
	PS reload_cycles1 = 28;
	PS reload_cycles3 = 28;
	PS out1 = 0;
	PS out2 = 0;
	PS out3 = 0;
	PS out4 = 0;
	PS delta1 = 0;
	PS delta2 = 0;
	PS delta3 = 0;
	PS delta4 = 0;
	PS skctl = 3;
	ZERO_ARRAY(PS delta_buffer);
}

ASAP_FUNC void PokeySound_Initialize(ASAP_State PTR as)
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
	init_state(ADDRESSOF AS base_pokey);
	init_state(ADDRESSOF AS extra_pokey);
}

#define CYCLE_TO_SAMPLE(cycle)  (((cycle) * ASAP_SAMPLE_RATE + AS sample_offset) / ASAP_MAIN_CLOCK)

#define DO_TICK(ch) \
	if (PS init) { \
		switch (PS audc##ch >> 4) { \
		case 10: \
		case 14: \
			PS out##ch ^= 1; \
			PS delta_buffer[CYCLE_TO_SAMPLE(cycle)] += PS delta##ch = -PS delta##ch; \
			break; \
		default: \
			break; \
		} \
	} \
	else { \
		int poly = cycle + PS poly_index - (ch - 1); \
		int newout = PS out##ch; \
		switch (PS audc##ch >> 4) { \
		case 0: \
			if (poly5_lookup[poly % 31] != 0) { \
				if ((PS audctl & 0x80) != 0) \
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
		case 4: \
			if (poly5_lookup[poly % 31] != 0) \
				newout = poly4_lookup[poly % 15]; \
			break; \
		case 8: \
			if ((PS audctl & 0x80) != 0) \
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
		if (newout != PS out##ch) { \
			PS out##ch = newout; \
			PS delta_buffer[CYCLE_TO_SAMPLE(cycle)] += PS delta##ch = -PS delta##ch; \
		} \
	}

FILE_FUNC void generate(ASAP_State PTR as, PokeyState PTR ps, int current_cycle)
{
	for (;;) {
		int cycle = current_cycle;
		if (cycle > PS tick_cycle1)
			cycle = PS tick_cycle1;
		if (cycle > PS tick_cycle2)
			cycle = PS tick_cycle2;
		if (cycle > PS tick_cycle3)
			cycle = PS tick_cycle3;
		if (cycle > PS tick_cycle4)
			cycle = PS tick_cycle4;
		if (cycle == current_cycle)
			break;
		if (cycle == PS tick_cycle3) {
			PS tick_cycle3 += PS period_cycles3;
			if ((PS audctl & 4) != 0 && PS delta1 > 0 && PS mute1 == 0)
				PS delta_buffer[CYCLE_TO_SAMPLE(cycle)] += PS delta1 = -PS delta1;
			DO_TICK(3);
		}
		if (cycle == PS tick_cycle4) {
			PS tick_cycle4 += PS period_cycles4;
			if ((PS audctl & 8) != 0)
				PS tick_cycle3 = cycle + PS reload_cycles3;
			if ((PS audctl & 2) != 0 && PS delta2 > 0 && PS mute2 == 0)
				PS delta_buffer[CYCLE_TO_SAMPLE(cycle)] += PS delta2 = -PS delta2;
			DO_TICK(4);
		}
		if (cycle == PS tick_cycle1) {
			PS tick_cycle1 += PS period_cycles1;
			if ((PS skctl & 0x88) == 8)
				PS tick_cycle2 = cycle + PS period_cycles2;
			DO_TICK(1);
		}
		if (cycle == PS tick_cycle2) {
			PS tick_cycle2 += PS period_cycles2;
			if ((PS audctl & 0x10) != 0)
				PS tick_cycle1 = cycle + PS reload_cycles1;
			else if ((PS skctl & 8) != 0)
				PS tick_cycle1 = cycle + PS period_cycles1;
			DO_TICK(2);
		}
	}
}

#define MUTE_CHANNEL(ch, cond, mask) \
	if (cond) { \
		PS mute##ch |= mask; \
		PS tick_cycle##ch = NEVER; \
	} \
	else { \
		PS mute##ch &= ~mask; \
		if (PS tick_cycle##ch == NEVER && PS mute##ch == 0) \
			PS tick_cycle##ch = AS cycle; \
	}

#define DO_ULTRASOUND(ch) \
	MUTE_CHANNEL(ch, PS period_cycles##ch <= ULTRASOUND_CYCLES && (PS audc##ch >> 4 == 10 || PS audc##ch >> 4 == 14), MUTE_FREQUENCY)

#define DO_AUDC(ch) \
	if (data == PS audc##ch) \
		break; \
	generate(as, ps, AS cycle); \
	PS audc##ch = data; \
	if ((data & 0x10) != 0) { \
		data &= 0xf; \
		if ((PS mute##ch & MUTE_USER) == 0) \
			PS delta_buffer[CYCLE_TO_SAMPLE(AS cycle)] \
				+= PS delta##ch > 0 ? data - PS delta##ch : data; \
		PS delta##ch = data; \
	} \
	else { \
		data &= 0xf; \
		DO_ULTRASOUND(ch); \
		if (PS delta##ch > 0) { \
			if ((PS mute##ch & MUTE_USER) == 0) \
				PS delta_buffer[CYCLE_TO_SAMPLE(AS cycle)] \
					+= data - PS delta##ch; \
			PS delta##ch = data; \
		} \
		else \
			PS delta##ch = -data; \
	} \
	break;

#define DO_INIT(ch, cond) \
	MUTE_CHANNEL(ch, PS init && cond, MUTE_INIT)

ASAP_FUNC void PokeySound_PutByte(ASAP_State PTR as, int addr, int data)
{
	PokeyState PTR ps = (addr & AS extra_pokey_mask) != 0
		? ADDRESSOF AS extra_pokey : ADDRESSOF AS base_pokey;
	switch (addr & 0xf) {
	case 0x00:
		if (data == PS audf1)
			break;
		generate(as, ps, AS cycle);
		PS audf1 = data;
		switch (PS audctl & 0x50) {
		case 0x00:
			PS period_cycles1 = PS div_cycles * (data + 1);
			break;
		case 0x10:
			PS period_cycles2 = PS div_cycles * (data + 256 * PS audf2 + 1);
			PS reload_cycles1 = PS div_cycles * (data + 1);
			DO_ULTRASOUND(2);
			break;
		case 0x40:
			PS period_cycles1 = data + 4;
			break;
		case 0x50:
			PS period_cycles2 = data + 256 * PS audf2 + 7;
			PS reload_cycles1 = data + 4;
			DO_ULTRASOUND(2);
			break;
		}
		DO_ULTRASOUND(1);
		break;
	case 0x01:
		DO_AUDC(1)
	case 0x02:
		if (data == PS audf2)
			break;
		generate(as, ps, AS cycle);
		PS audf2 = data;
		switch (PS audctl & 0x50) {
		case 0x00:
		case 0x40:
			PS period_cycles2 = PS div_cycles * (data + 1);
			break;
		case 0x10:
			PS period_cycles2 = PS div_cycles * (PS audf1 + 256 * data + 1);
			break;
		case 0x50:
			PS period_cycles2 = PS audf1 + 256 * data + 7;
			break;
		}
		DO_ULTRASOUND(2);
		break;
	case 0x03:
		DO_AUDC(2)
	case 0x04:
		if (data == PS audf3)
			break;
		generate(as, ps, AS cycle);
		PS audf3 = data;
		switch (PS audctl & 0x28) {
		case 0x00:
			PS period_cycles3 = PS div_cycles * (data + 1);
			break;
		case 0x08:
			PS period_cycles4 = PS div_cycles * (data + 256 * PS audf4 + 1);
			PS reload_cycles3 = PS div_cycles * (data + 1);
			DO_ULTRASOUND(4);
			break;
		case 0x20:
			PS period_cycles3 = data + 4;
			break;
		case 0x28:
			PS period_cycles4 = data + 256 * PS audf4 + 7;
			PS reload_cycles3 = data + 4;
			DO_ULTRASOUND(4);
			break;
		}
		DO_ULTRASOUND(3);
		break;
	case 0x05:
		DO_AUDC(3)
	case 0x06:
		if (data == PS audf4)
			break;
		generate(as, ps, AS cycle);
		PS audf4 = data;
		switch (PS audctl & 0x28) {
		case 0x00:
		case 0x20:
			PS period_cycles4 = PS div_cycles * (data + 1);
			break;
		case 0x08:
			PS period_cycles4 = PS div_cycles * (PS audf3 + 256 * data + 1);
			break;
		case 0x28:
			PS period_cycles4 = PS audf3 + 256 * data + 7;
			break;
		}
		DO_ULTRASOUND(4);
		break;
	case 0x07:
		DO_AUDC(4)
	case 0x08:
		if (data == PS audctl)
			break;
		generate(as, ps, AS cycle);
		PS audctl = data;
		PS div_cycles = ((data & 1) != 0) ? 114 : 28;
		/* TODO: tick_cycles */
		switch (data & 0x50) {
		case 0x00:
			PS period_cycles1 = PS div_cycles * (PS audf1 + 1);
			PS period_cycles2 = PS div_cycles * (PS audf2 + 1);
			break;
		case 0x10:
			PS period_cycles1 = PS div_cycles * 256;
			PS period_cycles2 = PS div_cycles * (PS audf1 + 256 * PS audf2 + 1);
			PS reload_cycles1 = PS div_cycles * (PS audf1 + 1);
			break;
		case 0x40:
			PS period_cycles1 = PS audf1 + 4;
			PS period_cycles2 = PS div_cycles * (PS audf2 + 1);
			break;
		case 0x50:
			PS period_cycles1 = 256;
			PS period_cycles2 = PS audf1 + 256 * PS audf2 + 7;
			PS reload_cycles1 = PS audf1 + 4;
			break;
		}
		DO_ULTRASOUND(1);
		DO_ULTRASOUND(2);
		switch (data & 0x28) {
		case 0x00:
			PS period_cycles3 = PS div_cycles * (PS audf3 + 1);
			PS period_cycles4 = PS div_cycles * (PS audf4 + 1);
			break;
		case 0x08:
			PS period_cycles3 = PS div_cycles * 256;
			PS period_cycles4 = PS div_cycles * (PS audf3 + 256 * PS audf4 + 1);
			PS reload_cycles3 = PS div_cycles * (PS audf3 + 1);
			break;
		case 0x20:
			PS period_cycles3 = PS audf3 + 4;
			PS period_cycles4 = PS div_cycles * (PS audf4 + 1);
			break;
		case 0x28:
			PS period_cycles3 = 256;
			PS period_cycles4 = PS audf3 + 256 * PS audf4 + 7;
			PS reload_cycles3 = PS audf3 + 4;
			break;
		}
		DO_ULTRASOUND(3);
		DO_ULTRASOUND(4);
		break;
	case 0x09:
		/* TODO: STIMER */
		break;
	case 0x0f:
		PS skctl = data;
		PS init = ((data & 3) == 0);
		DO_INIT(1, (PS audctl & 0x40) == 0);
		DO_INIT(2, (PS audctl & 0x50) != 0x50);
		DO_INIT(3, (PS audctl & 0x20) == 0);
		DO_INIT(4, (PS audctl & 0x28) != 0x28);
		break;
	default:
		break;
	}
}

ASAP_FUNC int PokeySound_GetRandom(ASAP_State PTR as, int addr)
{
	PokeyState PTR ps = (addr & AS extra_pokey_mask) != 0
		? ADDRESSOF AS extra_pokey : ADDRESSOF AS base_pokey;
	int i;
	if (PS init)
		return 0xff;
	i = AS cycle + PS poly_index;
	if ((PS audctl & 0x80) != 0)
		return AS poly9_lookup[i % 511];
	else {
		int j;
		i %= 131071;
		j = i >> 3;
		i &= 7;
		return ((AS poly17_lookup[j] >> i) + (AS poly17_lookup[j + 1] << (8 - i))) & 0xff;
	}
}

FILE_FUNC void end_frame(ASAP_State PTR as, PokeyState PTR ps, int cycle_limit)
{
	int m;
	generate(as, ps, cycle_limit);
	PS poly_index += cycle_limit;
	m = ((PS audctl & 0x80) != 0) ? 15 * 31 * 511 : 15 * 31 * 131071;
	if (PS poly_index >= 2 * m)
		PS poly_index -= m;
	if (PS tick_cycle1 != NEVER)
		PS tick_cycle1 -= cycle_limit;
	if (PS tick_cycle2 != NEVER)
		PS tick_cycle2 -= cycle_limit;
	if (PS tick_cycle3 != NEVER)
		PS tick_cycle3 -= cycle_limit;
	if (PS tick_cycle4 != NEVER)
		PS tick_cycle4 -= cycle_limit;
}

ASAP_FUNC void PokeySound_StartFrame(ASAP_State PTR as)
{
	ZERO_ARRAY(AS base_pokey.delta_buffer);
	if (AS extra_pokey_mask != 0)
		ZERO_ARRAY(AS extra_pokey.delta_buffer);
}

ASAP_FUNC void PokeySound_EndFrame(ASAP_State PTR as, int current_cycle)
{
	end_frame(as, ADDRESSOF AS base_pokey, current_cycle);
	if (AS extra_pokey_mask != 0)
		end_frame(as, ADDRESSOF AS extra_pokey, current_cycle);
	AS sample_offset += current_cycle * ASAP_SAMPLE_RATE;
	AS sample_index = 0;
	AS samples = AS sample_offset / ASAP_MAIN_CLOCK;
	AS sample_offset %= ASAP_MAIN_CLOCK;
}

ASAP_FUNC int PokeySound_Generate(ASAP_State PTR as, byte buffer[], int buffer_offset, int blocks, ASAP_SampleFormat format)
{
	int i = AS sample_index;
	int samples = AS samples;
	int acc_left = AS iir_acc_left;
	int acc_right = AS iir_acc_right;
	if (blocks < samples - i)
		samples = i + blocks;
	else
		blocks = samples - i;
	for (; i < samples; i++) {
		int sample;
		acc_left += (AS base_pokey.delta_buffer[i] << 20) - (acc_left * 3 >> 10);
		sample = acc_left >> 10;
#define STORE_SAMPLE \
		if (sample < -32767) \
			sample = -32767; \
		else if (sample > 32767) \
			sample = 32767; \
		switch (format) { \
		case ASAP_FORMAT_U8: \
			buffer[buffer_offset++] = (byte) ((sample >> 8) + 128); \
			break; \
		case ASAP_FORMAT_S16_LE: \
			buffer[buffer_offset++] = (byte) sample; \
			buffer[buffer_offset++] = (byte) (sample >> 8); \
			break; \
		case ASAP_FORMAT_S16_BE: \
			buffer[buffer_offset++] = (byte) (sample >> 8); \
			buffer[buffer_offset++] = (byte) sample; \
			break; \
		}
		STORE_SAMPLE;
		if (AS extra_pokey_mask != 0) {
			acc_right += (AS extra_pokey.delta_buffer[i] << 20) - (acc_right * 3 >> 10);
			sample = acc_right >> 10;
			STORE_SAMPLE;
		}
	}
	if (i == AS samples) {
		acc_left += AS base_pokey.delta_buffer[i] << 20;
		acc_right += AS extra_pokey.delta_buffer[i] << 20;
	}
	AS sample_index = i;
	AS iir_acc_left = acc_left;
	AS iir_acc_right = acc_right;
	return blocks;
}

ASAP_FUNC abool PokeySound_IsSilent(const PokeyState PTR ps)
{
	return ((PS audc1 | PS audc2 | PS audc3 | PS audc4) & 0xf) == 0;
}

ASAP_FUNC void PokeySound_Mute(const ASAP_State PTR as, PokeyState PTR ps, int mask)
{
	MUTE_CHANNEL(1, (mask & 1) != 0, MUTE_USER);
	MUTE_CHANNEL(2, (mask & 2) != 0, MUTE_USER);
	MUTE_CHANNEL(3, (mask & 4) != 0, MUTE_USER);
	MUTE_CHANNEL(4, (mask & 8) != 0, MUTE_USER);
}
