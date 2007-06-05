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
		return AS irqst;
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

#define MAX_DURATIONS  32

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

FILE_FUNC abool parse_cmc(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len, abool cmr)
{
	int pos;
	if (module_len < 0x306)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, PLAYER_OBX(cmc), 'C'))
			return FALSE;
		if (cmr)
			COPY_ARRAY(AS memory, 0x500 + CMR_BASS_TABLE_OFFSET, cmr_bass_table, 0, sizeof(cmr_bass_table));
	}
	/* auto-detect number of subsongs */
	pos = 0x54;
	while (--pos >= 0) {
		if (UBYTE(module[0x206 + pos]) < 0xfe
		 || UBYTE(module[0x25b + pos]) < 0xfe
		 || UBYTE(module[0x2b0 + pos]) < 0xfe)
			break;
	}
	while (--pos >= 0) {
		if (UBYTE(module[0x206 + pos]) == 0x8f || UBYTE(module[0x206 + pos]) == 0xef)
			MODULE_INFO songs++;
	}
	return TRUE;
}

FILE_FUNC abool parse_mpt(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int track0_addr;
	int i;
	int song_len;
	/* seen[i] == TRUE if the track position i is already processed */
	abool seen NEW_ARRAY(abool, 256);
	if (module_len < 0x1d0)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, PLAYER_OBX(mpt), 'm'))
			return FALSE;
	}
	track0_addr = UBYTE(module[2]) + (UBYTE(module[3]) << 8) + 0x1ca;
	/* do not auto-detect number of subsongs if the address
	   of the first track is non-standard */
	if (UBYTE(module[0x1c6]) + (UBYTE(module[0x1ca]) << 8) != track0_addr) {
		if (as != NULL)
			AS song_pos[0] = 0;
		return TRUE;
	}
	/* Calculate the length of the first track. Address of the second track minus
	   address of the first track equals the length of the first track in bytes.
	   Divide by two to get number of track positions. */
	song_len = (UBYTE(module[0x1c7]) + (UBYTE(module[0x1cb]) << 8) - track0_addr) >> 1;
	if (song_len > 0xfe)
		return FALSE;
	INIT_BOOL_ARRAY(seen);
	MODULE_INFO songs = 0;
	for (i = 0; i < song_len; i++) {
		int j;
		int c;
		if (seen[i])
			continue;
		j = i;
		/* follow jump commands until a pattern or a stop command is found */
		do {
			seen[j] = TRUE;
			c = UBYTE(module[0x1d0 + j * 2]);
			if (c != 0xff)
				break;
			j = UBYTE(module[0x1d1 + j * 2]);
		} while (j < song_len && !seen[j]);
		/* if no pattern found then this is not a subsong */
		if (c >= 64)
			continue;
		/* found subsong */
		if (as != NULL)
			AS song_pos[MODULE_INFO songs] = (byte) j;
		MODULE_INFO songs++;
		j++;
		/* follow this subsong */
		while (j < song_len && !seen[j]) {
			seen[j] = TRUE;
			c = UBYTE(module[0x1d0 + j * 2]);
			if (c < 64)
				j++;
			else if (c == 0xff)
				j = UBYTE(module[0x1d1 + j * 2]);
			else
				break;
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

FILE_FUNC abool parse_tmc(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int i;
	if (module_len < 0x1d0)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, PLAYER_OBX(tmc), 't'))
			return FALSE;
		AS tmc_per_frame = module[37];
		if (AS tmc_per_frame < 1 || AS tmc_per_frame > 4)
			return FALSE;
		AS sap_fastplay = perframe2fastplay[AS tmc_per_frame - 1];
	}
	i = 0;
	/* find first instrument */
	while (module[0x66 + i] == 0) {
		if (++i >= 64)
			return FALSE; /* no instrument */
	}
	i = (UBYTE(module[0x66 + i]) << 8) + UBYTE(module[0x26 + i])
		- UBYTE(module[2]) - (UBYTE(module[3]) << 8) - 1 + 6;
	if (i >= module_len)
		return FALSE;
	/* skip trailing jumps */
	do {
		if (i <= 0x1b5)
			return FALSE; /* no pattern to play */
		i -= 16;
	} while (UBYTE(module[i]) >= 0x80);
	while (i >= 0x1b5) {
		if (UBYTE(module[i]) >= 0x80)
			MODULE_INFO songs++;
		i -= 16;
	}
	return TRUE;
}

FILE_FUNC abool parse_tm2(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                          const byte module[], int module_len)
{
	int i;
	int song_end;
	int c;
	int song_start;
	if (module_len < 0x3a4)
		return FALSE;
	if (as != NULL) {
		i = module[0x25];
		if (i < 1 || i > 4)
			return FALSE;
		AS sap_fastplay = perframe2fastplay[i - 1];
		if (!load_native(as, module, module_len, PLAYER_OBX(tm2), 'T'))
			return FALSE;
		AS sap_player = 0x500;
	}
	if (module[0x1f] != 0)
		MODULE_INFO channels = 2;
	song_end = 0xffff;
	for (i = 0; i < 0x80; i++) {
		int instr_addr = UBYTE(module[0x86 + i]) + (UBYTE(module[0x306 + i]) << 8);
		if (instr_addr != 0 && instr_addr < song_end)
			song_end = instr_addr;
	}
	for (i = 0; i < 0x100; i++) {
		int pattern_addr = UBYTE(module[0x106 + i]) + (UBYTE(module[0x206 + i]) << 8);
		if (pattern_addr != 0 && pattern_addr < song_end)
			song_end = pattern_addr;
	}
	song_start = UBYTE(module[2]) + (UBYTE(module[3]) << 8) + 0x380;
	if (song_end < song_start + 2 * 17)
		return FALSE;
	i = song_end - song_start;
	if (0x386 + i >= module_len)
		return FALSE;
	i -= i % 17;
	/* skip trailing stop/jump commands */
	do {
		if (i == 0)
			return FALSE;
		i -= 17;
		c = UBYTE(module[0x386 + 16 + i]);
	} while (c == 0 || c >= 0x80);
	/* count stop/jump commands */
	while (i > 0) {
		i -= 17;
		c = UBYTE(module[0x386 + 16 + i]);
		if (c == 0 || c >= 0x80)
			MODULE_INFO songs++;
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

int ASAP_ParseDuration(const char *duration)
{
	int r;
	if (*duration < '0' || *duration > '9')
		return -1;
	r = *duration++ - '0';
	if (*duration >= '0' && *duration <= '9')
		r = 10 * r + *duration++ - '0';
	if (*duration == ':') {
		duration++;
		if (*duration < '0' || *duration > '5')
			return -1;
		r = 60 * r + (*duration++ - '0') * 10;
		if (*duration < '0' || *duration > '9')
			return -1;
		r += *duration++ - '0';
	}
	r *= 1000;
	if (*duration != '.')
		return r;
	duration++;
	if (*duration < '0' || *duration > '9')
		return r;
	r += 100 * (*duration++ - '0');
	if (*duration < '0' || *duration > '9')
		return r;
	r += 10 * (*duration++ - '0');
	if (*duration < '0' || *duration > '9')
		return r;
	r += *duration - '0';
	return r;
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
			if (duration < 0 || duration_index >= MAX_DURATIONS)
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

ASAP_FUNC abool ASAP_IsOurFile(STRING filename)
{
	switch (get_packed_ext(filename)) {
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

FILE_FUNC abool parse_file(ASAP_State PTR as, ASAP_ModuleInfo PTR module_info,
                           STRING filename, const byte module[], int module_len)
{
	abool r;
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
	for (i = 0; i < MAX_DURATIONS; i++) {
		MODULE_INFO durations[i] = -1;
		MODULE_INFO loops[i] = FALSE;
	}
	switch (get_packed_ext(filename)) {
	case ASAP_EXT('C', 'M', 'C'):
		r = parse_cmc(as, module_info, module, module_len, FALSE);
		break;
	case ASAP_EXT('C', 'M', 'R'):
		r = parse_cmc(as, module_info, module, module_len, TRUE);
		break;
	case ASAP_EXT('D', 'M', 'C'):
		if (as != NULL)
			AS sap_fastplay = 156;
		r = parse_cmc(as, module_info, module, module_len, FALSE);
		break;
	case ASAP_EXT('M', 'P', 'D'):
		if (as != NULL)
			AS sap_fastplay = 156;
		r = parse_mpt(as, module_info, module, module_len);
		break;
	case ASAP_EXT('M', 'P', 'T'):
		r = parse_mpt(as, module_info, module, module_len);
		break;
	case ASAP_EXT('R', 'M', 'T'):
		r = parse_rmt(as, module_info, module, module_len);
		break;
	case ASAP_EXT('S', 'A', 'P'):
		r = parse_sap(as, module_info, module, module_len);
		break;
	case ASAP_EXT('T', 'M', '2'):
		r = parse_tm2(as, module_info, module, module_len);
		break;
	case ASAP_EXT('T', 'M', '8'):
	case ASAP_EXT('T', 'M', 'C'):
		MODULE_INFO channels = 2;
		r = parse_tmc(as, module_info, module, module_len);
		break;
	default:
		return FALSE;
	}
	return r;
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
