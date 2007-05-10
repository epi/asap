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

#include <string.h>

#include "asap_internal.h"
#include "players.h"

#define CMR_BASS_TABLE_OFFSET  0x70f

CONST_LOOKUP byte cmr_bass_table[] = {
	0x5C, 0x56, 0x50, 0x4D, 0x47, 0x44, 0x41, 0x3E,
	0x38, 0x35, 0x88, 0x7F, 0x79, 0x73, 0x6C, 0x67,
	0x60, 0x5A, 0x55, 0x51, 0x4C, 0x48, 0x43, 0x3F,
	0x3D, 0x39, 0x34, 0x33, 0x30, 0x2D, 0x2A, 0x28,
	0x25, 0x24, 0x21, 0x1F, 0x1E
};

#define REAL_CYCLE  (AS cycle + AS cycle / 105 * 9)

int ASAP_GetByte(ASAP_State *as, int addr)
{
	switch (addr & 0xff0f) {
	case 0xd20a:
		return PokeySound_GetRandom(as, addr, REAL_CYCLE);
	case 0xd20e:
		return AS irqst;
	case 0xd40b:
		return AS cycle / (2 * 105) % 156;
	default:
		return dGetByte(addr);
	}
}

void ASAP_PutByte(ASAP_State *as, int addr, int data)
{
	if ((addr >> 8) == 0xd2) {
		if ((addr & (AS module_info.channels == 1 ? 0xf : 0x1f)) == 0xe) {
			AS irqst |= data ^ 0xff;
#define SET_TIMER_IRQ(ch) \
			if ((data & AS irqst & ch) != 0 && AS timer##ch##_cycle == NEVER) { \
				int t = AS base_pokey.tick_cycle##ch; \
				while (t < REAL_CYCLE) \
					t += AS base_pokey.period_cycles##ch ; \
				t = t * 105 / 114; \
				AS timer##ch##_cycle = t; \
				if (AS nearest_event_cycle > t) \
					AS nearest_event_cycle = t; \
			} \
			else \
				AS timer##ch##_cycle = NEVER;
			SET_TIMER_IRQ(1);
			SET_TIMER_IRQ(2);
			SET_TIMER_IRQ(4);
		}
		else
			PokeySound_PutByte(as, addr, data, REAL_CYCLE);
	}
	else if ((addr & 0xff0f) == 0xd40a) {
		int cycle = AS cycle % 105 + 9;
		if (cycle <= 106)
			AS cycle += 106 - cycle;
		else
			AS cycle += 105 + 106 - cycle;
	}
	else
		dPutByte(addr, data);
}

#define MAX_DURATIONS  (sizeof(module_info->durations) / sizeof(module_info->durations[0]))

CONST_LOOKUP int perframe2fastplay[] = { 312, 312 / 2, 312 / 3, 312 / 4 };

static abool load_native(ASAP_State *as, const byte *module, int module_len,
                         const byte *player, char type)
{
	int player_last_byte;
	int block_len;
	if (module[0] != 0xff || module[1] != 0xff)
		return FALSE;
	AS sap_player = player[2] + (player[3] << 8);
	player_last_byte = player[4] + (player[5] << 8);
	AS sap_music = module[2] + (module[3] << 8);
	if (AS sap_music <= player_last_byte)
		return FALSE;
	block_len = module[4] + (module[5] << 8) + 1 - AS sap_music;
	if (6 + block_len != module_len) {
		int info_addr;
		int info_len;
		if (type != 'r' || 11 + block_len > module_len)
			return FALSE;
		/* allow optional info for Raster Music Tracker */
		info_addr = module[6 + block_len] + (module[7 + block_len] << 8);
		if (info_addr != AS sap_music + block_len)
			return FALSE;
		info_len = module[8 + block_len] + (module[9 + block_len] << 8) + 1 - info_addr;
		if (10 + block_len + info_len != module_len)
			return FALSE;
	}
	memcpy(AS memory + AS sap_music, module + 6, block_len);
	memcpy(AS memory + AS sap_player, player + 6, player_last_byte + 1 - AS sap_player);
	AS sap_type = type;
	return TRUE;
}

static abool parse_cmc(ASAP_State *as, ASAP_ModuleInfo *module_info,
                       const byte *module, int module_len, abool cmr)
{
	int pos;
	if (module_len < 0x306)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, cmc_obx, 'C'))
			return FALSE;
		if (cmr)
			memcpy(AS memory + 0x500 + CMR_BASS_TABLE_OFFSET, cmr_bass_table, sizeof(cmr_bass_table));
	}
	/* auto-detect number of subsongs */
	pos = 0x54;
	while (--pos >= 0) {
		if (module[0x206 + pos] < 0xfe
		 || module[0x25b + pos] < 0xfe
		 || module[0x2b0 + pos] < 0xfe)
			break;
	}
	while (--pos >= 0) {
		if (module[0x206 + pos] == 0x8f || module[0x206 + pos] == 0xef)
			module_info->songs++;
	}
	return TRUE;
}

static abool parse_mpt(ASAP_State *as, ASAP_ModuleInfo *module_info,
                       const byte *module, int module_len)
{
	int track0_addr;
	int i;
	int song_len;
	/* seen[i] == TRUE if the track position i is already processed */
	abool seen[256];
	if (module_len < 0x1d0)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, mpt_obx, 'm'))
			return FALSE;
	}
	track0_addr = module[2] + (module[3] << 8) + 0x1ca;
	/* do not auto-detect number of subsongs if the address
	   of the first track is non-standard */
	if (module[0x1c6] + (module[0x1ca] << 8) != track0_addr) {
		if (as != NULL)
			AS song_pos[0] = 0;
		return TRUE;
	}
	/* Calculate the length of the first track. Address of the second track minus
	   address of the first track equals the length of the first track in bytes.
	   Divide by two to get number of track positions. */
	song_len = (module[0x1c7] + (module[0x1cb] << 8) - track0_addr) >> 1;
	if (song_len > 0xfe)
		return FALSE;
	memset(seen, FALSE, sizeof(seen));
	module_info->songs = 0;
	for (i = 0; i < song_len; i++) {
		int j;
		int c;
		if (seen[i])
			continue;
		j = i;
		/* follow jump commands until a pattern or a stop command is found */
		do {
			seen[j] = TRUE;
			c = module[0x1d0 + j * 2];
			if (c != 0xff)
				break;
			j = module[0x1d1 + j * 2];
		} while (j < song_len && !seen[j]);
		/* if no pattern found then this is not a subsong */
		if (c >= 64)
			continue;
		/* found subsong */
		if (as != NULL)
			AS song_pos[module_info->songs] = (byte) j;
		module_info->songs++;
		j++;
		/* follow this subsong */
		while (j < song_len && !seen[j]) {
			seen[j] = TRUE;
			c = module[0x1d0 + j * 2];
			if (c < 64)
				j++;
			else if (c == 0xff)
				j = module[0x1d1 + j * 2];
			else
				break;
		}
	}
	return module_info->songs != 0;
}

static abool parse_rmt(ASAP_State *as, ASAP_ModuleInfo *module_info,
                       const byte *module, int module_len)
{
	int i;
	int module_start;
	int song_start;
	int song_last_byte;
	const byte *song;
	int song_len;
	int pos_shift;
	abool seen[256];
	if (module_len < 0x30 || module[6] != 'R' || module[7] != 'M'
	 || module[8] != 'T' || module[13] != 1)
		return FALSE;
	switch (module[9]) {
	case '4':
		pos_shift = 2;
		break;
	case '8':
		module_info->channels = 2;
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
			module_info->channels == 2 ? rmt8_obx : rmt4_obx, 'r'))
			return FALSE;
		AS sap_player = 0x600;
	}
	/* auto-detect number of subsongs */
	module_start = module[2] + (module[3] << 8);
	song_start = module[20] + (module[21] << 8);
	song_last_byte = module[4] + (module[5] << 8);
	if (song_start <= module_start || song_start >= song_last_byte)
		return FALSE;
	song = module + 6 + song_start - module_start;
	song_len = (song_last_byte + 1 - song_start) >> pos_shift;
	if (song_len > 0xfe)
		song_len = 0xfe;
	memset(seen, FALSE, sizeof(seen));
	module_info->songs = 0;
	for (i = 0; i < song_len; i++) {
		int j;
		if (seen[i])
			continue;
		j = i;
		do {
			seen[j] = TRUE;
			if (song[j << pos_shift] != 0xfe)
				break;
			j = song[(j << pos_shift) + 1];
		} while (j < song_len && !seen[j]);
		if (as != NULL)
			AS song_pos[module_info->songs] = (byte) j;
		module_info->songs++;
		j++;
		while (j < song_len && !seen[j]) {
			seen[j] = TRUE;
			if (song[j << pos_shift] != 0xfe)
				j++;
			else
				j = song[(j << pos_shift) + 1];
		}
	}
	return module_info->songs != 0;
}

static abool parse_tmc(ASAP_State *as, ASAP_ModuleInfo *module_info,
                       const byte *module, int module_len)
{
	int i;
	if (module_len < 0x1d0)
		return FALSE;
	if (as != NULL) {
		if (!load_native(as, module, module_len, tmc_obx, 't'))
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
	i = (module[0x66 + i] << 8) + module[0x26 + i]
		- module[2] - (module[3] << 8) - 1 + 6;
	if (i >= module_len)
		return FALSE;
	/* skip trailing jumps */
	do {
		if (i <= 0x1b5)
			return FALSE; /* no pattern to play */
		i -= 16;
	} while (module[i] >= 0x80);
	while (i >= 0x1b5) {
		if (module[i] >= 0x80)
			module_info->songs++;
		i -= 16;
	}
	return TRUE;
}

static abool parse_tm2(ASAP_State *as, ASAP_ModuleInfo *module_info,
                       const byte *module, int module_len)
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
		if (!load_native(as, module, module_len, tm2_obx, 'T'))
			return FALSE;
		AS sap_player = 0x500;
	}
	if (module[0x1f] != 0)
		module_info->channels = 2;
	song_end = 0xffff;
	for (i = 0; i < 0x80; i++) {
		int instr_addr = module[0x86 + i] + (module[0x306 + i] << 8);
		if (instr_addr != 0 && instr_addr < song_end)
			song_end = instr_addr;
	}
	for (i = 0; i < 0x100; i++) {
		int pattern_addr = module[0x106 + i] + (module[0x206 + i] << 8);
		if (pattern_addr != 0 && pattern_addr < song_end)
			song_end = pattern_addr;
	}
	song_start = module[2] + (module[3] << 8) + 0x380;
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
		c = module[0x386 + 16 + i];
	} while (c == 0 || c >= 0x80);
	/* count stop/jump commands */
	while (i > 0) {
		i -= 17;
		c = module[0x386 + 16 + i];
		if (c == 0 || c >= 0x80)
			module_info->songs++;
	}
	return TRUE;
}

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

static char *my_stpcpy(char *dest, const char *src)
{
	size_t len = strlen(src);
	memcpy(dest, src, len);
	return dest + len;
}

static abool parse_sap(ASAP_State *as, ASAP_ModuleInfo *module_info,
                       const byte *sap_ptr, const byte * const sap_end)
{
	char *p;
	abool sap_signature = FALSE;
	int duration_index = 0;
	if (as != NULL) {
		AS sap_type = '?';
		AS sap_player = 0xffff;
		AS sap_music = 0xffff;
		AS sap_init = 0xffff;
	}
	for (;;) {
		char line[256];
		if (sap_ptr + 8 >= sap_end)
			return FALSE;
		if (*sap_ptr == 0xff)
			break;
		p = line;
		while (*sap_ptr != 0x0d) {
			*p++ = (char) *sap_ptr++;
			if (sap_ptr >= sap_end || p >= line + sizeof(line) - 1)
				return FALSE;
		}
		if (++sap_ptr >= sap_end || *sap_ptr++ != 0x0a)
			return FALSE;
		*p = '\0';
		for (p = line; *p != '\0'; p++) {
			if (*p == ' ') {
				*p++ = '\0';
				break;
			}
		}
		if (strcmp(line, "SAP") == 0)
			sap_signature = TRUE;
		if (!sap_signature)
			return FALSE;
		if (as != NULL) {
			if (strcmp(line, "TYPE") == 0) {
				AS sap_type = *p;
			}
			else if (strcmp(line, "PLAYER") == 0) {
				if (!parse_hex(&AS sap_player, p))
					return FALSE;
			}
			else if (strcmp(line, "MUSIC") == 0) {
				if (!parse_hex(&AS sap_music, p))
					return FALSE;
			}
			else if (strcmp(line, "INIT") == 0) {
				if (!parse_hex(&AS sap_init, p))
					return FALSE;
			}
			else if (strcmp(line, "FASTPLAY") == 0) {
				if (!parse_dec(&AS sap_fastplay, p, 1, 312))
					return FALSE;
			}
		}
		if (strcmp(line, "AUTHOR") == 0) {
			if (!parse_text(module_info->author, p))
				return FALSE;
		}
		else if (strcmp(line, "NAME") == 0) {
			if (!parse_text(module_info->name, p))
				return FALSE;
		}
		else if (strcmp(line, "DATE") == 0) {
			if (!parse_text(module_info->date, p))
				return FALSE;
		}
		else if (strcmp(line, "SONGS") == 0) {
			if (!parse_dec(&module_info->songs, p, 1, 255))
				return FALSE;
		}
		else if (strcmp(line, "DEFSONG") == 0) {
			if (!parse_dec(&module_info->default_song, p, 0, 254))
				return FALSE;
		}
		else if (strcmp(line, "STEREO") == 0)
			module_info->channels = 2;
		else if (strcmp(line, "TIME") == 0) {
			int duration = ASAP_ParseDuration(p);
			if (duration < 0 || duration_index >= MAX_DURATIONS)
				return FALSE;
			module_info->durations[duration_index] = duration;
			if (strstr(p, "LOOP") != NULL)
				module_info->loops[duration_index] = TRUE;
			duration_index++;
		}
	}
	if (module_info->default_song >= module_info->songs)
		return FALSE;
	p = my_stpcpy(module_info->all_info, "Author: ");
	p = my_stpcpy(p, module_info->author);
	p = my_stpcpy(p, "\nName: ");
	p = my_stpcpy(p, module_info->name);
	p = my_stpcpy(p, "\nDate: ");
	p = my_stpcpy(p, module_info->date);
	*p++ = '\n';
	*p = '\0';
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
	if (sap_ptr[1] != 0xff)
		return FALSE;
	memset(AS memory, 0, sizeof(AS memory));
	sap_ptr += 2;
	while (sap_ptr + 5 <= sap_end) {
		int start_addr = sap_ptr[0] + (sap_ptr[1] << 8);
		int block_len = sap_ptr[2] + (sap_ptr[3] << 8) + 1 - start_addr;
		if (block_len <= 0 || sap_ptr + block_len > sap_end)
			return FALSE;
		sap_ptr += 4;
		memcpy(AS memory + start_addr, sap_ptr, block_len);
		sap_ptr += block_len;
		if (sap_ptr == sap_end)
			return TRUE;
		if (sap_ptr + 7 <= sap_end && sap_ptr[0] == 0xff && sap_ptr[1] == 0xff)
			sap_ptr += 2;
	}
	return FALSE;
}

#define ASAP_EXT(c1, c2, c3) (((c1) + ((c2) << 8) + ((c3) << 16)) | 0x202020)

static int get_packed_ext(const char *filename)
{
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
}

abool ASAP_IsOurFile(const char *filename)
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

abool parse_file(ASAP_State *as, ASAP_ModuleInfo *module_info,
                 const char *filename, const byte *module, int module_len)
{
	int i;
	strcpy(module_info->author, "<?>");
	strcpy(module_info->name, "<?>");
	strcpy(module_info->date, "<?>");
	module_info->channels = 1;
	module_info->songs = 1;
	module_info->default_song = 0;
	for (i = 0; i < MAX_DURATIONS; i++) {
		module_info->durations[i] = -1;
		module_info->loops[i] = FALSE;
	}
	switch (get_packed_ext(filename)) {
	case ASAP_EXT('C', 'M', 'C'):
		return parse_cmc(as, module_info, module, module_len, FALSE);
	case ASAP_EXT('C', 'M', 'R'):
		return parse_cmc(as, module_info, module, module_len, TRUE);
	case ASAP_EXT('D', 'M', 'C'):
		if (as != NULL)
			AS sap_fastplay = 156;
		return parse_cmc(as, module_info, module, module_len, FALSE);
	case ASAP_EXT('M', 'P', 'D'):
		if (as != NULL)
			AS sap_fastplay = 156;
		return parse_mpt(as, module_info, module, module_len);
	case ASAP_EXT('M', 'P', 'T'):
		return parse_mpt(as, module_info, module, module_len);
	case ASAP_EXT('R', 'M', 'T'):
		return parse_rmt(as, module_info, module, module_len);
	case ASAP_EXT('S', 'A', 'P'):
		return parse_sap(as, module_info, module, module + module_len);
	case ASAP_EXT('T', 'M', '2'):
		return parse_tm2(as, module_info, module, module_len);
	case ASAP_EXT('T', 'M', '8'):
		module_info->channels = 2;
		return parse_tmc(as, module_info, module, module_len);
	case ASAP_EXT('T', 'M', 'C'):
		return parse_tmc(as, module_info, module, module_len);
	default:
		return FALSE;
	}
}

abool ASAP_GetModuleInfo(ASAP_ModuleInfo *module_info, const char *filename,
                         const byte *module, int module_len)
{
	return parse_file(NULL, module_info, filename, module, module_len);
}

abool ASAP_Load(ASAP_State *as, const char *filename,
                const byte *module, int module_len)
{
	AS sap_fastplay = 312;
	return parse_file(as, &as->module_info, filename, module, module_len);
}

static void call_6502(ASAP_State *as, int addr, int max_scanlines)
{
	AS cpu_pc = addr;
	/* put a CIM at 0xd20a and a return address on stack */
	dPutByte(0xd20a, 0xd2);
	dPutByte(0x01fe, 0x09);
	dPutByte(0x01ff, 0xd2);
	AS cpu_s = 0xfd;
	AS cycle = 0;
	Cpu_Run(as, max_scanlines * 105);
}

/* 50 Atari frames for the initialization routine - some SAPs are self-extracting. */
#define SCANLINES_FOR_INIT  (50 * 312)

static void call_6502_init(ASAP_State *as, int addr, int a, int x, int y)
{
	AS cpu_a = a & 0xff;
	AS cpu_x = x & 0xff;
	AS cpu_y = y & 0xff;
	call_6502(as, addr, SCANLINES_FOR_INIT);
}

void ASAP_PlaySong(ASAP_State *as, int song, int duration)
{
	AS current_song = song;
	AS current_duration = duration;
	AS blocks_played = 0;
	PokeySound_Initialize(as);
	AS cpu_nz = 0;
	AS cpu_c = 0;
	AS cpu_vdi = I_FLAG;
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
}

void call_6502_player(ASAP_State *as)
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
		AS cycle = 0;
		Cpu_Run(as, AS sap_fastplay * 105);
		break;
	case 'S':
		AS cycle = 0;
		Cpu_Run(as, AS sap_fastplay * 105);
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
	PokeySound_EndFrame(as, 114 * AS sap_fastplay);
}

static int milliseconds_to_blocks(int milliseconds)
{
	return milliseconds * (ASAP_SAMPLE_RATE / 100) / 10;
}

void ASAP_Seek(ASAP_State *as, int position)
{
	int block = milliseconds_to_blocks(position);
	if (block < AS blocks_played)
		ASAP_PlaySong(as, AS current_song, AS current_duration);
	while (AS blocks_played + AS samples - AS sample_offset < block) {
		AS blocks_played += AS samples - AS sample_offset;
		call_6502_player(as);
	}
	AS sample_offset += block - AS blocks_played;
	AS blocks_played = block;
}

int ASAP_Generate(ASAP_State *as, void *buffer, int buffer_len,
                  ASAP_SampleFormat format)
{
	int block_shift = (AS module_info.channels - 1) + (format != ASAP_FORMAT_U8 ? 1 : 0);
	int buffer_blocks = buffer_len >> block_shift;
	int remaining_blocks;
	if (AS current_duration > 0) {
		int total_blocks = milliseconds_to_blocks(AS current_duration);
		if (buffer_blocks > total_blocks - AS blocks_played)
			buffer_blocks = total_blocks - AS blocks_played;
	}
	if (buffer_blocks == 0)
		return 0;
	remaining_blocks = buffer_blocks;
	for (;;) {
		int blocks = PokeySound_Generate(as, buffer, remaining_blocks, format);
		AS blocks_played += blocks;
		remaining_blocks -= blocks;
		if (remaining_blocks == 0)
			return buffer_blocks << block_shift;
		buffer = (byte *) buffer + (blocks << block_shift);
		call_6502_player(as);
	}
}
