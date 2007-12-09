/*
 * asap.c - ASAP engine
 *
 * Copyright (C) 2005-2007  Piotr Fusik
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
#ifndef JAVA
#include "players.h"
#endif

#define CMR_BASS_TABLE_OFFSET  0x70f

CONST_LOOKUP byte cmr_bass_table[] = {
	0x5C, 0x56, 0x50, 0x4D, 0x47, 0x44, 0x41, 0x3E,
	0x38, 0x35, (byte) 0x88, 0x7F, 0x79, 0x73, 0x6C, 0x67,
	0x60, 0x5A, 0x55, 0x51, 0x4C, 0x48, 0x43, 0x3F,
	0x3D, 0x39, 0x34, 0x33, 0x30, 0x2D, 0x2A, 0x28,
	0x25, 0x24, 0x21, 0x1F, 0x1E
};

ASAP_FUNC int ASAP_GetByte(ASAP_State PTR as, int addr)
{
	switch (addr & 0xff0f) {
	case 0xd20a:
		return PokeySound_GetRandom(as, addr);
	case 0xd20e:
		if ((addr & AS extra_pokey_mask) != 0)
			return 0xff;
		return AS irqst;
	case 0xd20f:
		return 0xff;
	case 0xd40b:
		return AS scanline_number >> 1;
	default:
		return dGetByte(addr);
	}
}

ASAP_FUNC void ASAP_PutByte(ASAP_State PTR as, int addr, int data)
{
	if ((addr >> 8) == 0xd2) {
		if ((addr & (AS extra_pokey_mask + 0xf)) == 0xe) {
			AS irqst |= data ^ 0xff;
#define SET_TIMER_IRQ(ch) \
			if ((data & AS irqst & ch) != 0) { \
				if (AS timer##ch##_cycle == NEVER) { \
					int t = AS base_pokey.tick_cycle##ch; \
					while (t < AS cycle) \
						t += AS base_pokey.period_cycles##ch ; \
					AS timer##ch##_cycle = t; \
					if (AS nearest_event_cycle > t) \
						AS nearest_event_cycle = t; \
				} \
			} \
			else \
				AS timer##ch##_cycle = NEVER;
			SET_TIMER_IRQ(1);
			SET_TIMER_IRQ(2);
			SET_TIMER_IRQ(4);
		}
		else
			PokeySound_PutByte(as, addr, data);
	}
	else if ((addr & 0xff0f) == 0xd40a) {
		if (AS cycle <= AS next_scanline_cycle - 8)
			AS cycle = AS next_scanline_cycle - 8;
		else
			AS cycle = AS next_scanline_cycle + 106;
	}
	else
		dPutByte(addr, data);
}

#define MAX_SONGS  32

CONST_LOOKUP int perframe2fastplay[] = { 312, 312 / 2, 312 / 3, 312 / 4 };

FILE_FUNC abool load_native(ASAP_State PTR as, const byte module[], int module_len,
                            ASAP_Player player, char type)
{
	int player_last_byte;
	int block_len;
	if (UBYTE(module[0]) != 0xff || UBYTE(module[1]) != 0xff)
		return FALSE;
#ifdef JAVA
	try {
		player.read();
		player.read();
		AS sap_player = player.read();
		AS sap_player += player.read() << 8;
		player_last_byte = player.read();
		player_last_byte += player.read() << 8;
	} catch (IOException e) {
		throw new RuntimeException();
	}
#else
	AS sap_player = UBYTE(player[2]) + (UBYTE(player[3]) << 8);
	player_last_byte = UBYTE(player[4]) + (UBYTE(player[5]) << 8);
#endif
	AS sap_music = UBYTE(module[2]) + (UBYTE(module[3]) << 8);
	if (AS sap_music <= player_last_byte)
		return FALSE;
	block_len = UBYTE(module[4]) + (UBYTE(module[5]) << 8) + 1 - AS sap_music;
	if (6 + block_len != module_len) {
		int info_addr;
		int info_len;
		if (type != 'r' || 11 + block_len > module_len)
			return FALSE;
		/* allow optional info for Raster Music Tracker */
		info_addr = UBYTE(module[6 + block_len]) + (UBYTE(module[7 + block_len]) << 8);
		if (info_addr != AS sap_music + block_len)
			return FALSE;
		info_len = UBYTE(module[8 + block_len]) + (UBYTE(module[9 + block_len]) << 8) + 1 - info_addr;
		if (10 + block_len + info_len != module_len)
			return FALSE;
	}
	COPY_ARRAY(AS memory, AS sap_music, module, 6, block_len);
#ifdef JAVA
	int addr = AS sap_player;
	do {
		int i;
		try {
			i = player.read(AS memory, addr, player_last_byte + 1 - addr);
		} catch (IOException e) {
			throw new RuntimeException();
		}
		if (i <= 0)
			throw new RuntimeException();
		addr += i;
	} while (addr <= player_last_byte);
#else
	COPY_ARRAY(AS memory, AS sap_player, player, 6, player_last_byte + 1 - AS sap_player);
#endif
	AS sap_type = type;
	return TRUE;
}

FILE_FUNC void set_song_duration(ASAP_ModuleInfo PTR module_info, int fastplay, int player_calls)
{
	MODULE_INFO durations[MODULE_INFO songs] = (int) (player_calls * fastplay * 114000.0 / 1773447);
	MODULE_INFO songs++;
}

#define SEEN_THIS_CALL  1
#define SEEN_BEFORE     2
#define SEEN_REPEAT     3

FILE_FUNC void parse_cmc_song(ASAP_ModuleInfo PTR module_info, const byte module[],
                              int fastplay, int pos)
{
	int tempo = UBYTE(module[0x19]);
	int player_calls = 0;
	int rep_start_pos = 0;
	int rep_end_pos = 0;
	int rep_times = 0;
	byte seen NEW_ARRAY(byte, 0x55);
	INIT_BYTE_ARRAY(seen);
	while (pos >= 0 && pos < 0x55) {
		int p1;
		int p2;
		int p3;
		if (pos == rep_end_pos && rep_times > 0) {
			for (p1 = 0; p1 < 0x55; p1++)
				if (seen[p1] == SEEN_THIS_CALL || seen[p1] == SEEN_REPEAT)
					seen[p1] = 0;
			rep_times--;
			pos = rep_start_pos;
		}
		if (seen[pos] != 0) {
			if (seen[pos] != SEEN_THIS_CALL)
				MODULE_INFO loops[MODULE_INFO songs] = TRUE;
			break;
		}
		seen[pos] = SEEN_THIS_CALL;
		p1 = UBYTE(module[0x206 + pos]);
		p2 = UBYTE(module[0x25b + pos]);
		p3 = UBYTE(module[0x2b0 + pos]);
		if (p1 == 0xfe || p2 == 0xfe || p3 == 0xfe) {
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
		if (p1 == 0xa) {
			pos -= p2;
			continue;
		}
		if (p1 == 0xb) {
			pos += p2;
			continue;
		}
		if (p1 == 0xc) {
			tempo = p2;
			pos++;
			continue;
		}
		if (p1 == 0xd) {
			pos++;
			rep_start_pos = pos;
			rep_end_pos = pos + p2;
			rep_times = p3 - 1;
			continue;
		}
		if (p1 == 0xe) {
			MODULE_INFO loops[MODULE_INFO songs] = TRUE;
			break;
		}
		p2 = rep_times > 0 ? SEEN_REPEAT : SEEN_BEFORE;
		for (p1 = 0; p1 < 0x55; p1++)
			if (seen[p1] == SEEN_THIS_CALL)
				seen[p1] = (byte) p2;
		player_calls += tempo << 6;
		pos++;
	}
	set_song_duration(module_info, fastplay, player_calls);
}

FILE_FUNC abool parse_cmc(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len, abool cmr, int fastplay)
{
	int last_pos;
	int pos;
	if (module_len < 0x306)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, PLAYER_OBX(cmc), 'C'))
			return FALSE;
		if (cmr)
			COPY_ARRAY(AS memory, 0x500 + CMR_BASS_TABLE_OFFSET, cmr_bass_table, 0, sizeof(cmr_bass_table));
		AS sap_fastplay = fastplay;
	}
	/* auto-detect number of subsongs */
	last_pos = 0x54;
	while (--last_pos >= 0) {
		if (UBYTE(module[0x206 + last_pos]) < 0xb0
		 || UBYTE(module[0x25b + last_pos]) < 0x40
		 || UBYTE(module[0x2b0 + last_pos]) < 0x40)
			break;
	}
	MODULE_INFO songs = 0;
	parse_cmc_song(module_info, module, fastplay, 0);
	for (pos = 0; pos < last_pos && MODULE_INFO songs < MAX_SONGS; pos++)
		if (UBYTE(module[0x206 + pos]) == 0x8f || UBYTE(module[0x206 + pos]) == 0xef)
			parse_cmc_song(module_info, module, fastplay, pos + 1);
	return TRUE;
}

FILE_FUNC void parse_mpt_song(ASAP_ModuleInfo PTR module_info, const byte module[],
                              abool global_seen[], int song_len, int fastplay, int pos)
{
	int addr_to_offset = UBYTE(module[2]) + (UBYTE(module[3]) << 8) - 6;
	int tempo = UBYTE(module[0x1cf]);
	int player_calls = 0;
	byte seen NEW_ARRAY(byte, 256);
	int pattern_offset NEW_ARRAY(int, 4);
	int blank_rows NEW_ARRAY(int, 4);
	int blank_rows_counter NEW_ARRAY(int, 4);
	INIT_BYTE_ARRAY(seen);
	blank_rows[3] = blank_rows[2] = blank_rows[1] = blank_rows[0] = 0;
	while (pos < song_len) {
		int i;
		int ch;
		int pattern_rows;
		if (seen[pos] != 0) {
			if (seen[pos] != SEEN_THIS_CALL)
				MODULE_INFO loops[MODULE_INFO songs] = TRUE;
			break;
		}
		seen[pos] = SEEN_THIS_CALL;
		global_seen[pos] = TRUE;
		i = UBYTE(module[0x1d0 + pos * 2]);
		if (i == 0xff) {
			pos = UBYTE(module[0x1d1 + pos * 2]);
			continue;
		}
		for (ch = 3; ch >= 0; ch--) {
			i = UBYTE(module[0x1c6 + ch]) + (UBYTE(module[0x1ca + ch]) << 8) - addr_to_offset;
			i = UBYTE(module[i + pos * 2]);
			if (i >= 0x40)
				break;
			i <<= 1;
			i = UBYTE(module[0x46 + i]) + (UBYTE(module[0x47 + i]) << 8);
			pattern_offset[ch] = i == 0 ? 0 : i - addr_to_offset;
			blank_rows_counter[ch] = 0;
		}
		if (ch >= 0)
			break;
		for (i = 0; i < song_len; i++)
			if (seen[i] == SEEN_THIS_CALL)
				seen[i] = SEEN_BEFORE;
		for (pattern_rows = UBYTE(module[0x1ce]); --pattern_rows >= 0; ) {
			for (ch = 3; ch >= 0; ch--) {
				if (pattern_offset[ch] == 0 || --blank_rows_counter[ch] >= 0)
					continue;
				for (;;) {
					i = UBYTE(module[pattern_offset[ch]++]);
					if (i < 0x40 || i == 0xfe)
						break;
					if (i < 0x80)
						continue;
					if (i < 0xc0) {
						blank_rows[ch] = i - 0x80;
						continue;
					}
					if (i < 0xd0)
						continue;
					if (i < 0xe0) {
						tempo = i - 0xcf;
						continue;
					}
					pattern_rows = 0;
				}
				blank_rows_counter[ch] = blank_rows[ch];
			}
			player_calls += tempo;
		}
		pos++;
	}
	if (player_calls > 0)
		set_song_duration(module_info, fastplay, player_calls);
}

FILE_FUNC abool parse_mpt(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len, int fastplay)
{
	int track0_addr;
	int pos;
	int song_len;
	/* seen[i] == TRUE if the track position i is already processed */
	abool global_seen NEW_ARRAY(abool, 256);
	if (module_len < 0x1d0)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, PLAYER_OBX(mpt), 'm'))
			return FALSE;
		AS sap_fastplay = fastplay;
	}
	track0_addr = UBYTE(module[2]) + (UBYTE(module[3]) << 8) + 0x1ca;
	if (UBYTE(module[0x1c6]) + (UBYTE(module[0x1ca]) << 8) != track0_addr)
		return FALSE;
	/* Calculate the length of the first track. Address of the second track minus
	   address of the first track equals the length of the first track in bytes.
	   Divide by two to get number of track positions. */
	song_len = (UBYTE(module[0x1c7]) + (UBYTE(module[0x1cb]) << 8) - track0_addr) >> 1;
	if (song_len > 0xfe)
		return FALSE;
	INIT_BOOL_ARRAY(global_seen);
	MODULE_INFO songs = 0;
	for (pos = 0; pos < song_len && MODULE_INFO songs < MAX_SONGS; pos++) {
		if (!global_seen[pos]) {
			if (as != NULL)
				AS song_pos[MODULE_INFO songs] = (byte) pos;
			parse_mpt_song(module_info, module, global_seen, song_len, fastplay, pos);
		}
	}
	return MODULE_INFO songs != 0;
}

FILE_FUNC abool parse_rmt(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int i;
	int module_start;
	int song_start;
	int song_last_byte;
	int song_index;
	int song_len;
	int pos_shift;
	abool seen NEW_ARRAY(abool, 256);
	if (module_len < 0x30 || module[6] != 'R' || module[7] != 'M'
	 || module[8] != 'T' || module[13] != 1)
		return FALSE;
	switch (module[9]) {
	case '4':
		pos_shift = 2;
		break;
	case '8':
		MODULE_INFO channels = 2;
		pos_shift = 3;
		break;
	default:
		return FALSE;
	}
	i = module[12];
	if (i < 1 || i > 4)
		return FALSE;
	if (as != NULL) {
		AS sap_fastplay = perframe2fastplay[i - 1];
		if (!load_native(as, module, module_len,
			MODULE_INFO channels == 2 ? PLAYER_OBX(rmt8) : PLAYER_OBX(rmt4), 'r'))
			return FALSE;
		AS sap_player = 0x600;
	}
	/* auto-detect number of subsongs */
	module_start = UBYTE(module[2]) + (UBYTE(module[3]) << 8);
	song_start = UBYTE(module[20]) + (UBYTE(module[21]) << 8);
	song_last_byte = UBYTE(module[4]) + (UBYTE(module[5]) << 8);
	if (song_start <= module_start || song_start >= song_last_byte)
		return FALSE;
	song_index = 6 + song_start - module_start;
	song_len = (song_last_byte + 1 - song_start) >> pos_shift;
	if (song_len > 0xfe)
		song_len = 0xfe;
	INIT_BOOL_ARRAY(seen);
	MODULE_INFO songs = 0;
	for (i = 0; i < song_len; i++) {
		int j;
		if (seen[i])
			continue;
		j = i;
		do {
			seen[j] = TRUE;
			if (UBYTE(module[song_index + (j << pos_shift)]) != 0xfe)
				break;
			j = UBYTE(module[song_index + (j << pos_shift) + 1]);
		} while (j < song_len && !seen[j]);
		if (as != NULL)
			AS song_pos[MODULE_INFO songs] = (byte) j;
		MODULE_INFO songs++;
		j++;
		while (j < song_len && !seen[j]) {
			seen[j] = TRUE;
			if (UBYTE(module[song_index + (j << pos_shift)]) != 0xfe)
				j++;
			else
				j = UBYTE(module[song_index + (j << pos_shift) + 1]);
		}
	}
	return MODULE_INFO songs != 0;
}

FILE_FUNC void parse_tmc_song(ASAP_ModuleInfo PTR module_info, const byte module[],
                              int pos)
{
	int addr_to_offset = UBYTE(module[2]) + (UBYTE(module[3]) << 8) - 6;
	int tempo = UBYTE(module[0x24]) + 1;
	int player_calls = 0;
	int pattern_offset NEW_ARRAY(int, 8);
	int blank_rows NEW_ARRAY(int, 8);
	while (UBYTE(module[0x1a6 + 15 + pos]) < 0x80) {
		int ch;
		int pattern_rows;
		for (ch = 7; ch >= 0; ch--) {
			int pat = UBYTE(module[0x1a6 + 15 + pos - 2 * ch]);
			pattern_offset[ch] = UBYTE(module[0xa6 + pat]) + (UBYTE(module[0x126 + pat]) << 8) - addr_to_offset;
			blank_rows[ch] = 0;
		}
		for (pattern_rows = 64; --pattern_rows >= 0; ) {
			for (ch = 7; ch >= 0; ch--) {
				if (--blank_rows[ch] >= 0)
					continue;
				for (;;) {
					int i = UBYTE(module[pattern_offset[ch]++]);
					if (i < 0x40) {
						pattern_offset[ch]++;
						break;
					}
					if (i == 0x40) {
						i = UBYTE(module[pattern_offset[ch]++]);
						if ((i & 0x7f) == 0)
							pattern_rows = 0;
						else
							tempo = (i & 0x7f) + 1;
						if (i >= 0x80)
							pattern_offset[ch]++;
						break;
					}
					if (i < 0x80) {
						i = module[pattern_offset[ch]++] & 0x7f;
						if (i == 0)
							pattern_rows = 0;
						else
							tempo = i + 1;
						pattern_offset[ch]++;
						break;
					}
					if (i < 0xc0)
						continue;
					blank_rows[ch] = i - 0xbf;
					break;
				}
			}
			player_calls += tempo;
		}
		pos += 16;
	}
	if (UBYTE(module[0x1a6 + 14 + pos]) < 0x80)
		MODULE_INFO loops[MODULE_INFO songs] = TRUE;
	set_song_duration(module_info, 312, player_calls);
}

FILE_FUNC abool parse_tmc(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int i;
	int last_pos;
	if (module_len < 0x1d0)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, PLAYER_OBX(tmc), 't'))
			return FALSE;
		AS tmc_per_frame = module[0x25];
		if (AS tmc_per_frame < 1 || AS tmc_per_frame > 4)
			return FALSE;
		AS sap_fastplay = perframe2fastplay[AS tmc_per_frame - 1];
	}
	MODULE_INFO channels = 2;
	i = 0;
	/* find first instrument */
	while (module[0x66 + i] == 0) {
		if (++i >= 64)
			return FALSE; /* no instrument */
	}
	last_pos = (UBYTE(module[0x66 + i]) << 8) + UBYTE(module[0x26 + i])
		- UBYTE(module[2]) - (UBYTE(module[3]) << 8) - 0x1b0;
	if (0x1b5 + last_pos >= module_len)
		return FALSE;
	/* skip trailing jumps */
	do {
		if (last_pos <= 0)
			return FALSE; /* no pattern to play */
		last_pos -= 16;
	} while (UBYTE(module[0x1b5 + last_pos]) >= 0x80);
	MODULE_INFO songs = 0;
	parse_tmc_song(module_info, module, 0);
	for (i = 0; i < last_pos && MODULE_INFO songs < MAX_SONGS; i += 16)
		if (UBYTE(module[0x1b5 + i]) >= 0x80)
			parse_tmc_song(module_info, module, i + 16);
	return TRUE;
}

FILE_FUNC void parse_tm2_song(ASAP_ModuleInfo PTR module_info, const byte module[],
                              int fastplay, int pos)
{
	int addr_to_offset = UBYTE(module[2]) + (UBYTE(module[3]) << 8) - 6;
	int tempo = UBYTE(module[0x24]) + 1;
	int player_calls = 0;
	int pattern_offset NEW_ARRAY(int, 8);
	int blank_rows NEW_ARRAY(int, 8);
	for (;;) {
		int ch;
		int pattern_rows = UBYTE(module[0x386 + 16 + pos]);
		if (pattern_rows == 0)
			break;
		if (pattern_rows >= 0x80) {
			MODULE_INFO loops[MODULE_INFO songs] = TRUE;
			break;
		}
		for (ch = 7; ch >= 0; ch--) {
			int pat = UBYTE(module[0x386 + 15 + pos - 2 * ch]);
			pattern_offset[ch] = UBYTE(module[0x106 + pat]) + (UBYTE(module[0x206 + pat]) << 8) - addr_to_offset;
			blank_rows[ch] = 0;
		}
		while (--pattern_rows >= 0) {
			for (ch = 7; ch >= 0; ch--) {
				if (--blank_rows[ch] >= 0)
					continue;
				for (;;) {
					int i = UBYTE(module[pattern_offset[ch]++]);
					if (i == 0) {
						pattern_offset[ch]++;
						break;
					}
					if (i < 0x40) {
						if (UBYTE(module[pattern_offset[ch]++]) >= 0x80)
							pattern_offset[ch]++;
						break;
					}
					if (i < 0x80) {
						pattern_offset[ch]++;
						break;
					}
					if (i == 0x80) {
						blank_rows[ch] = UBYTE(module[pattern_offset[ch]++]);
						break;
					}
					if (i < 0xc0)
						break;
					if (i < 0xd0) {
						tempo = i - 0xbf;
						continue;
					}
					if (i < 0xe0) {
						pattern_offset[ch]++;
						break;
					}
					if (i < 0xf0) {
						pattern_offset[ch] += 2;
						break;
					}
					if (i < 0xff) {
						blank_rows[ch] = i - 0xf0;
						break;
					}
					blank_rows[ch] = 64;
					break;
				}
			}
			player_calls += tempo;
		}
		pos += 17;
	}
	set_song_duration(module_info, fastplay, player_calls);
}

FILE_FUNC abool parse_tm2(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int i;
	int fastplay;
	int last_pos;
	int c;
	if (module_len < 0x3a4)
		return FALSE;
	i = module[0x25];
	if (i < 1 || i > 4)
		return FALSE;
	fastplay = perframe2fastplay[i - 1];
	if (as != NULL) {
		AS sap_fastplay = fastplay;
		if (!load_native(as, module, module_len, PLAYER_OBX(tm2), 'T'))
			return FALSE;
		AS sap_player = 0x500;
	}
	if (module[0x1f] != 0)
		MODULE_INFO channels = 2;
	last_pos = 0xffff;
	for (i = 0; i < 0x80; i++) {
		int instr_addr = UBYTE(module[0x86 + i]) + (UBYTE(module[0x306 + i]) << 8);
		if (instr_addr != 0 && instr_addr < last_pos)
			last_pos = instr_addr;
	}
	for (i = 0; i < 0x100; i++) {
		int pattern_addr = UBYTE(module[0x106 + i]) + (UBYTE(module[0x206 + i]) << 8);
		if (pattern_addr != 0 && pattern_addr < last_pos)
			last_pos = pattern_addr;
	}
	last_pos -= UBYTE(module[2]) + (UBYTE(module[3]) << 8) + 0x380;
	if (0x386 + last_pos >= module_len)
		return FALSE;
	/* skip trailing stop/jump commands */
	do {
		if (last_pos <= 0)
			return FALSE;
		last_pos -= 17;
		c = UBYTE(module[0x386 + 16 + last_pos]);
	} while (c == 0 || c >= 0x80);
	MODULE_INFO songs = 0;
	parse_tm2_song(module_info, module, fastplay, 0);
	for (i = 0; i < last_pos && MODULE_INFO songs < MAX_SONGS; i += 17) {
		c = UBYTE(module[0x386 + 16 + i]);
		if (c == 0 || c >= 0x80)
			parse_tm2_song(module_info, module, fastplay, i + 17);
	}
	return TRUE;
}

#ifndef JAVA

static abool parse_hex(int *retval, const char *p)
{
	int r = 0;
	do {
		char c = *p;
		if (r > 0xfff)
			return FALSE;
		r <<= 4;
		if (c >= '0' && c <= '9')
			r += c - '0';
		else if (c >= 'A' && c <= 'F')
			r += c - 'A' + 10;
		else if (c >= 'a' && c <= 'f')
			r += c - 'a' + 10;
		else
			return FALSE;
	} while (*++p != '\0');
	*retval = r;
	return TRUE;
}

static abool parse_dec(int *retval, const char *p, int minval, int maxval)
{
	int r = 0;
	do {
		char c = *p;
		if (c >= '0' && c <= '9')
			r = 10 * r + c - '0';
		else
			return FALSE;
		if (r > maxval)
			return FALSE;
	} while (*++p != '\0');
	if (r < minval)
		return FALSE;
	*retval = r;
	return TRUE;
}

static abool parse_text(char *retval, const char *p)
{
	int i;
	if (*p != '"')
		return FALSE;
	p++;
	if (p[0] == '<' && p[1] == '?' && p[2] == '>' && p[3] == '"')
		return TRUE;
	i = 0;
	while (*p != '"') {
		if (i >= 127)
			return FALSE;
		if (*p == '\0')
			return FALSE;
		retval[i++] = *p++;
	}
	retval[i] = '\0';
	return TRUE;
}

int ASAP_ParseDuration(const char *s)
{
	int r;
	if (*s < '0' || *s > '9')
		return -1;
	r = *s++ - '0';
	if (*s >= '0' && *s <= '9')
		r = 10 * r + *s++ - '0';
	if (*s == ':') {
		s++;
		if (*s < '0' || *s > '5')
			return -1;
		r = 60 * r + (*s++ - '0') * 10;
		if (*s < '0' || *s > '9')
			return -1;
		r += *s++ - '0';
	}
	r *= 1000;
	if (*s != '.')
		return r;
	s++;
	if (*s < '0' || *s > '9')
		return r;
	r += 100 * (*s++ - '0');
	if (*s < '0' || *s > '9')
		return r;
	r += 10 * (*s++ - '0');
	if (*s < '0' || *s > '9')
		return r;
	r += *s - '0';
	return r;
}

static char *two_digits(char *s, int x)
{
	s[0] = '0' + x / 10;
	s[1] = '0' + x % 10;
	return s + 2;
}

void ASAP_DurationToString(char *s, int duration)
{
	if (duration >= 0) {
		int seconds = duration / 1000;
		int minutes = seconds / 60;
		s = two_digits(s, minutes);
		*s++ = ':';
		s = two_digits(s, seconds % 60);
		duration %= 1000;
		if (duration != 0) {
			*s++ = '.';
			s = two_digits(s, duration / 10);
			duration %= 10;
			if (duration != 0)
				*s++ = '0' + duration;
		}
	}
	*s = '\0';
}

#endif /* JAVA */

FILE_FUNC abool parse_sap(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int module_index = 0;
	abool sap_signature = FALSE;
	int duration_index = 0;
	if (as != NULL) {
		AS sap_type = '?';
		AS sap_player = 0xffff;
		AS sap_music = 0xffff;
		AS sap_init = 0xffff;
	}
	for (;;) {
		char line NEW_ARRAY(char, 256);
		int i;
#ifndef JAVA
		char *p;
#endif
		if (module_index + 8 >= module_len)
			return FALSE;
		if (UBYTE(module[module_index]) == 0xff)
			break;
		i = 0;
		while (module[module_index] != 0x0d) {
			line[i++] = (char) module[module_index++];
			if (module_index >= module_len || i >= sizeof(line) - 1)
				return FALSE;
		}
		if (++module_index >= module_len || module[module_index++] != 0x0a)
			return FALSE;

#ifdef JAVA
		String tag = new String(line, 0, i);
		String arg = null;
		i = tag.indexOf(' ');
		if (i >= 0) {
			arg = tag.substring(i + 1);
			tag = tag.substring(0, i);
		}
#define TAG_IS(t)               tag.equals(t)
#define CHAR_ARG                arg.charAt(0)
#define SET_HEX(v)              v = Integer.parseInt(arg, 16)
#define SET_DEC(v, min, max)    do { v = Integer.parseInt(arg); if (v < min || v > max) return FALSE; } while (FALSE)
#define SET_TEXT(v)             v = arg.substring(1, arg.length() - 1)
#define DURATION_ARG            parseDuration(arg)
#define ARG_CONTAINS(t)         (arg.indexOf(t) >= 0)
#else
		line[i] = '\0';
		for (p = line; *p != '\0'; p++) {
			if (*p == ' ') {
				*p++ = '\0';
				break;
			}
		}
#define TAG_IS(t)               (strcmp(line, t) == 0)
#define CHAR_ARG                *p
#define SET_HEX(v)              do { if (!parse_hex(&v, p)) return FALSE; } while (FALSE)
#define SET_DEC(v, min, max)    do { if (!parse_dec(&v, p, min, max)) return FALSE; } while (FALSE)
#define SET_TEXT(v)             do { if (!parse_text(v, p)) return FALSE; } while (FALSE)
#define DURATION_ARG            ASAP_ParseDuration(p)
#define ARG_CONTAINS(t)         (strstr(p, t) != NULL)
#endif

		if (TAG_IS("SAP"))
			sap_signature = TRUE;
		if (!sap_signature)
			return FALSE;
		if (as != NULL) {
			if (TAG_IS("TYPE"))
				AS sap_type = CHAR_ARG;
			else if (TAG_IS("PLAYER"))
				SET_HEX(AS sap_player);
			else if (TAG_IS("MUSIC"))
				SET_HEX(AS sap_music);
			else if (TAG_IS("INIT"))
				SET_HEX(AS sap_init);
			else if (TAG_IS("FASTPLAY"))
				SET_DEC(AS sap_fastplay, 1, 312);
		}
		if (TAG_IS("AUTHOR"))
			SET_TEXT(MODULE_INFO author);
		else if (TAG_IS("NAME"))
			SET_TEXT(MODULE_INFO name);
		else if (TAG_IS("DATE"))
			SET_TEXT(MODULE_INFO date);
		else if (TAG_IS("SONGS"))
			SET_DEC(MODULE_INFO songs, 1, 32);
		else if (TAG_IS("DEFSONG"))
			SET_DEC(MODULE_INFO default_song, 0, 31);
		else if (TAG_IS("STEREO"))
			MODULE_INFO channels = 2;
		else if (TAG_IS("TIME")) {
			int duration = DURATION_ARG;
			if (duration < 0 || duration_index >= MAX_SONGS)
				return FALSE;
			MODULE_INFO durations[duration_index] = duration;
			if (ARG_CONTAINS("LOOP"))
				MODULE_INFO loops[duration_index] = TRUE;
			duration_index++;
		}
	}
	if (MODULE_INFO default_song >= MODULE_INFO songs)
		return FALSE;
	if (as == NULL)
		return TRUE;
	switch (AS sap_type) {
	case 'B':
	case 'D':
		if (AS sap_player == 0xffff || AS sap_init == 0xffff)
			return FALSE;
		break;
	case 'C':
		if (AS sap_player == 0xffff || AS sap_music == 0xffff)
			return FALSE;
		break;
	case 'S':
		if (AS sap_init == 0xffff)
			return FALSE;
		AS sap_fastplay = 78;
		break;
	default:
		return FALSE;
	}
	if (UBYTE(module[module_index + 1]) != 0xff)
		return FALSE;
	ZERO_ARRAY(AS memory);
	module_index += 2;
	while (module_index + 5 <= module_len) {
		int start_addr = UBYTE(module[module_index]) + (UBYTE(module[module_index + 1]) << 8);
		int block_len = UBYTE(module[module_index + 2]) + (UBYTE(module[module_index + 3]) << 8) + 1 - start_addr;
		if (block_len <= 0 || module_index + block_len > module_len)
			return FALSE;
		module_index += 4;
		COPY_ARRAY(AS memory, start_addr, module, module_index, block_len);
		module_index += block_len;
		if (module_index == module_len)
			return TRUE;
		if (module_index + 7 <= module_len
		 && UBYTE(module[module_index]) == 0xff && UBYTE(module[module_index + 1]) == 0xff)
			module_index += 2;
	}
	return FALSE;
}

#define ASAP_EXT(c1, c2, c3) (((c1) + ((c2) << 8) + ((c3) << 16)) | 0x202020)

FILE_FUNC int get_packed_ext(STRING filename)
{
#ifdef JAVA
	int i = filename.length();
	int ext = 0;
	while (--i > 0) {
		if (filename.charAt(i) == '.')
			return ext | 0x202020;
		ext = (ext << 8) + filename.charAt(i);
	}
	return 0;
#else
	const char *p;
	int ext;
	for (p = filename; *p != '\0'; p++);
	ext = 0;
	for (;;) {
		if (--p <= filename || *p <= ' ')
			return 0; /* no filename extension or invalid character */
		if (*p == '.')
			return ext | 0x202020;
		ext = (ext << 8) + (*p & 0xff);
	}
#endif
}

FILE_FUNC abool is_our_ext(int ext)
{
	switch (ext) {
	case ASAP_EXT('C', 'M', 'C'):
	case ASAP_EXT('C', 'M', 'R'):
	case ASAP_EXT('D', 'M', 'C'):
	case ASAP_EXT('M', 'P', 'D'):
	case ASAP_EXT('M', 'P', 'T'):
	case ASAP_EXT('R', 'M', 'T'):
	case ASAP_EXT('S', 'A', 'P'):
	case ASAP_EXT('T', 'M', '2'):
	case ASAP_EXT('T', 'M', '8'):
	case ASAP_EXT('T', 'M', 'C'):
		return TRUE;
	default:
		return FALSE;
	}
}

ASAP_FUNC abool ASAP_IsOurFile(STRING filename)
{
	int ext = get_packed_ext(filename);
	return is_our_ext(ext);
}

ASAP_FUNC abool ASAP_IsOurExt(STRING ext)
{
#ifdef JAVA
	return ext.length() == 3
		&& is_our_ext(ASAP_EXT(ext.charAt(0), ext.charAt(1), ext.charAt(2)));
#else
	return ext[0] > ' ' && ext[1] > ' ' && ext[2] > ' ' && ext[3] == '\0'
		&& is_our_ext(ASAP_EXT(ext[0], ext[1], ext[2]));
#endif
}

FILE_FUNC abool parse_file(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                           STRING filename, const byte module[], int module_len)
{
	int i;
#ifdef JAVA
	int basename = 0;
	int ext = -1;
	for (i = 0; i < filename.length(); i++) {
		int c = filename.charAt(i);
		if (c == '/' || c == '\\')
			basename = i + 1;
		else if (c == '.')
			ext = i;
	}
	if (ext < 0)
		ext = i;
	module_info.author = "";
	module_info.name = filename.substring(basename, ext);
	module_info.date = "";
#else
	const char *p;
	const char *basename = filename;
	const char *ext = NULL;
	for (p = filename; *p != '\0'; p++) {
		if (*p == '/' || *p == '\\')
			basename = p + 1;
		else if (*p == '.')
			ext = p;
	}
	if (ext == NULL)
		ext = p;
	module_info->author[0] = '\0';
	i = ext - basename;
	memcpy(module_info->name, basename, i);
	module_info->name[i] = '\0';
	module_info->date[0] = '\0';
#endif
	MODULE_INFO channels = 1;
	MODULE_INFO songs = 1;
	MODULE_INFO default_song = 0;
	for (i = 0; i < MAX_SONGS; i++) {
		MODULE_INFO durations[i] = -1;
		MODULE_INFO loops[i] = FALSE;
	}
	switch (get_packed_ext(filename)) {
	case ASAP_EXT('C', 'M', 'C'):
		return parse_cmc(as, module_info, module, module_len, FALSE, 312);
	case ASAP_EXT('C', 'M', 'R'):
		return parse_cmc(as, module_info, module, module_len, TRUE, 312);
	case ASAP_EXT('D', 'M', 'C'):
		return parse_cmc(as, module_info, module, module_len, FALSE, 156);
	case ASAP_EXT('M', 'P', 'D'):
		return parse_mpt(as, module_info, module, module_len, 156);
	case ASAP_EXT('M', 'P', 'T'):
		return parse_mpt(as, module_info, module, module_len, 312);
	case ASAP_EXT('R', 'M', 'T'):
		return parse_rmt(as, module_info, module, module_len);
	case ASAP_EXT('S', 'A', 'P'):
		return parse_sap(as, module_info, module, module_len);
	case ASAP_EXT('T', 'M', '2'):
		return parse_tm2(as, module_info, module, module_len);
	case ASAP_EXT('T', 'M', '8'):
	case ASAP_EXT('T', 'M', 'C'):
		return parse_tmc(as, module_info, module, module_len);
	default:
		break;
	}
	return FALSE;
}

ASAP_FUNC abool ASAP_GetModuleInfo(ASAP_ModuleInfo PTR module_info, STRING filename,
                                   const byte module[], int module_len)
{
	return parse_file(NULL, module_info, filename, module, module_len);
}

ASAP_FUNC abool ASAP_Load(ASAP_State PTR as, STRING filename,
                          const byte module[], int module_len)
{
	AS sap_fastplay = 312;
	AS silence_cycles = 0;
	return parse_file(as, ADDRESSOF AS module_info, filename, module, module_len);
}

ASAP_FUNC void ASAP_DetectSilence(ASAP_State PTR as, int seconds)
{
	AS silence_cycles = seconds * ASAP_MAIN_CLOCK;
}

FILE_FUNC void call_6502(ASAP_State PTR as, int addr, int max_scanlines)
{
	AS cpu_pc = addr;
	/* put a CIM at 0xd20a and a return address on stack */
	dPutByte(0xd20a, 0xd2);
	dPutByte(0x01fe, 0x09);
	dPutByte(0x01ff, 0xd2);
	AS cpu_s = 0xfd;
	Cpu_RunScanlines(as, max_scanlines);
}

/* 50 Atari frames for the initialization routine - some SAPs are self-extracting. */
#define SCANLINES_FOR_INIT  (50 * 312)

FILE_FUNC void call_6502_init(ASAP_State PTR as, int addr, int a, int x, int y)
{
	AS cpu_a = a & 0xff;
	AS cpu_x = x & 0xff;
	AS cpu_y = y & 0xff;
	call_6502(as, addr, SCANLINES_FOR_INIT);
}

ASAP_FUNC void ASAP_PlaySong(ASAP_State PTR as, int song, int duration)
{
	AS current_song = song;
	AS current_duration = duration;
	AS blocks_played = 0;
	AS silence_cycles_counter = AS silence_cycles;
	AS extra_pokey_mask = AS module_info.channels > 1 ? 0x10 : 0;
	PokeySound_Initialize(as);
	AS cycle = 0;
	AS cpu_nz = 0;
	AS cpu_c = 0;
	AS cpu_vdi = 0;
	AS scanline_number = 0;
	AS next_scanline_cycle = 0;
	AS timer1_cycle = NEVER;
	AS timer2_cycle = NEVER;
	AS timer4_cycle = NEVER;
	AS irqst = 0xff;
	switch (AS sap_type) {
	case 'B':
		call_6502_init(as, AS sap_init, song, 0, 0);
		break;
	case 'C':
		call_6502_init(as, AS sap_player + 3, 0x70, AS sap_music, AS sap_music >> 8);
		call_6502_init(as, AS sap_player + 3, 0x00, song, 0);
		break;
	case 'D':
	case 'S':
		AS cpu_a = song;
		AS cpu_x = 0x00;
		AS cpu_y = 0x00;
		AS cpu_s = 0xff;
		AS cpu_pc = AS sap_init;
		break;
	case 'm':
		call_6502_init(as, AS sap_player, 0x00, AS sap_music >> 8, AS sap_music);
		call_6502_init(as, AS sap_player, 0x02, AS song_pos[song], 0);
		break;
	case 'r':
		call_6502_init(as, AS sap_player, AS song_pos[song], AS sap_music, AS sap_music >> 8);
		break;
	case 't':
	case 'T':
		call_6502_init(as, AS sap_player, 0x70, AS sap_music >> 8, AS sap_music);
		call_6502_init(as, AS sap_player, 0x00, song, 0);
		AS tmc_per_frame_counter = 1;
		break;
	}
	ASAP_MutePokeyChannels(as, 0);
}

ASAP_FUNC void ASAP_MutePokeyChannels(ASAP_State PTR as, int mask)
{
	PokeySound_Mute(as, ADDRESSOF AS base_pokey, mask);
	PokeySound_Mute(as, ADDRESSOF AS extra_pokey, mask >> 4);
}

ASAP_FUNC abool call_6502_player(ASAP_State PTR as)
{
	int s;
	PokeySound_StartFrame(as);
	switch (AS sap_type) {
	case 'B':
		call_6502(as, AS sap_player, AS sap_fastplay);
		break;
	case 'C':
		call_6502(as, AS sap_player + 6, AS sap_fastplay);
		break;
	case 'D':
		s = AS cpu_s;
#define PUSH_ON_6502_STACK(x)  dPutByte(0x100 + s, x); s = (s - 1) & 0xff
#define RETURN_FROM_PLAYER_ADDR  0xd200
		/* save 6502 state on 6502 stack */
		PUSH_ON_6502_STACK(AS cpu_pc >> 8);
		PUSH_ON_6502_STACK(AS cpu_pc & 0xff);
		PUSH_ON_6502_STACK(((AS cpu_nz | (AS cpu_nz >> 1)) & 0x80) + AS cpu_vdi + \
			((AS cpu_nz & 0xff) == 0 ? Z_FLAG : 0) + AS cpu_c + 0x20);
		PUSH_ON_6502_STACK(AS cpu_a);
		PUSH_ON_6502_STACK(AS cpu_x);
		PUSH_ON_6502_STACK(AS cpu_y);
		/* RTS will jump to 6502 code that restores the state */
		PUSH_ON_6502_STACK((RETURN_FROM_PLAYER_ADDR - 1) >> 8);
		PUSH_ON_6502_STACK((RETURN_FROM_PLAYER_ADDR - 1) & 0xff);
		AS cpu_s = s;
		dPutByte(RETURN_FROM_PLAYER_ADDR, 0x68);     /* PLA */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 1, 0xa8); /* TAY */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 2, 0x68); /* PLA */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 3, 0xaa); /* TAX */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 4, 0x68); /* PLA */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 5, 0x40); /* RTI */
		AS cpu_pc = AS sap_player;
		Cpu_RunScanlines(as, AS sap_fastplay);
		break;
	case 'S':
		Cpu_RunScanlines(as, AS sap_fastplay);
		{
			int i = dGetByte(0x45) - 1;
			dPutByte(0x45, i);
			if (i == 0)
				dPutByte(0xb07b, dGetByte(0xb07b) + 1);
		}
		break;
	case 'm':
	case 'r':
	case 'T':
		call_6502(as, AS sap_player + 3, AS sap_fastplay);
		break;
	case 't':
		if (--AS tmc_per_frame_counter <= 0) {
			AS tmc_per_frame_counter = AS tmc_per_frame;
			call_6502(as, AS sap_player + 3, AS sap_fastplay);
		}
		else
			call_6502(as, AS sap_player + 6, AS sap_fastplay);
		break;
	}
	PokeySound_EndFrame(as, AS sap_fastplay * 114);
	if (AS silence_cycles > 0) {
		if (PokeySound_IsSilent(ADDRESSOF AS base_pokey)
		 && PokeySound_IsSilent(ADDRESSOF AS extra_pokey)) {
			AS silence_cycles_counter -= AS sap_fastplay * 114;
			if (AS silence_cycles_counter <= 0)
				return FALSE;
		}
		else
			AS silence_cycles_counter = AS silence_cycles;
	}
	return TRUE;
}

FILE_FUNC int milliseconds_to_blocks(int milliseconds)
{
	return milliseconds * (ASAP_SAMPLE_RATE / 100) / 10;
}

ASAP_FUNC void ASAP_Seek(ASAP_State PTR as, int position)
{
	int block = milliseconds_to_blocks(position);
	if (block < AS blocks_played)
		ASAP_PlaySong(as, AS current_song, AS current_duration);
	while (AS blocks_played + AS samples - AS sample_index < block) {
		AS blocks_played += AS samples - AS sample_index;
		call_6502_player(as);
	}
	AS sample_index += block - AS blocks_played;
	AS blocks_played = block;
}

ASAP_FUNC int ASAP_Generate(ASAP_State PTR as, VOIDPTR buffer, int buffer_len,
                            ASAP_SampleFormat format)
{
	int block_shift;
	int buffer_blocks;
	int block;
	if (AS silence_cycles > 0 && AS silence_cycles_counter <= 0)
		return 0;
	block_shift = (AS module_info.channels - 1) + (format != ASAP_FORMAT_U8 ? 1 : 0);
	buffer_blocks = buffer_len >> block_shift;
	if (AS current_duration > 0) {
		int total_blocks = milliseconds_to_blocks(AS current_duration);
		if (buffer_blocks > total_blocks - AS blocks_played)
			buffer_blocks = total_blocks - AS blocks_played;
	}
	block = 0;
	do {
		int blocks = PokeySound_Generate(as, buffer, block << block_shift, buffer_blocks - block, format);
		AS blocks_played += blocks;
		block += blocks;
	} while (block < buffer_blocks && call_6502_player(as));
	return block << block_shift;
}

#ifndef JAVA

abool ASAP_CanSetModuleInfo(const char *filename)
{
	int ext = get_packed_ext(filename);
	return ext == ASAP_EXT('S', 'A', 'P');
}

static byte *put_string(byte *dest, const char *str)
{
	while (*str != '\0')
		*dest++ = *str++;
	return dest;
}

static byte *put_tag(byte *dest, const char *tag, const char *value)
{
	dest = put_string(dest, tag);
	if (value != NULL) {
		*dest++ = ' ';
		*dest++ = '"';
		if (*value == '\0')
			value = "<?>";
		while (*value != '\0') {
			if (*value < ' ' || *value > 'z' || *value == '"' || *value == '`')
				return NULL;
			*dest++ = *value++;
		}
		*dest++ = '"';
	}
	*dest++ = '\x0d';
	*dest++ = '\x0a';
	return dest;
}

int ASAP_SetModuleInfo(const ASAP_ModuleInfo *module_info, const byte module[],
                       int module_len, byte out_module[])
{
	byte *p = out_module;
	int i;
	int song;
	if (memcmp(module, "SAP\r\n", 5) != 0)
		return -1;
	i = 5;
	p = put_tag(p, "SAP", NULL);
	p = put_tag(p, "AUTHOR", module_info->author);
	if (p == NULL)
		return -1;
	p = put_tag(p, "NAME", module_info->name);
	if (p == NULL)
		return -1;
	p = put_tag(p, "DATE", module_info->date);
	if (p == NULL)
		return -1;
	while (i < module_len && module[i] != 0xff) {
		if (memcmp(module + i, "AUTHOR ", 7) == 0
		 || memcmp(module + i, "NAME ", 5) == 0
		 || memcmp(module + i, "DATE ", 5) == 0
		 || memcmp(module + i, "TIME ", 5) == 0) {
			while (i < module_len && module[i++] != 0x0a);
		}
		else {
			int b;
			do {
				b = module[i++];
				*p++ = b;
			} while (i < module_len && b != 0x0a);
		}
	}
	for (song = 0; song < module_info->songs; song++) {
		if (module_info->durations[song] < 0)
			break;
		p = put_string(p, "TIME ");
		ASAP_DurationToString(p, module_info->durations[song]);
		while (*p != '\0')
			p++;
		if (module_info->loops[song])
			p = put_string(p, " LOOP");
		*p++ = '\x0d';
		*p++ = '\x0a';
	}
	module_len -= i;
	memcpy(p, module + i, module_len);
	p += module_len;
	return p - out_module;
}

#endif /* JAVA */
