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

#include "config.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "asap.h"
#include "asap_internal.h"
#include "acpu.h"
#ifdef APOKEYSND
#include "apokeysnd.h"
#else
#include "pokey.h"
#include "pokeysnd.h"
#endif

#include "players.h"

#define CMR_BASS_TABLE_OFFSET  0x70f

static const unsigned char cmr_bass_table[] = {
	0x5C, 0x56, 0x50, 0x4D, 0x47, 0x44, 0x41, 0x3E,
	0x38, 0x35, 0x88, 0x7F, 0x79, 0x73, 0x6C, 0x67,
	0x60, 0x5A, 0x55, 0x51, 0x4C, 0x48, 0x43, 0x3F,
	0x3D, 0x39, 0x34, 0x33, 0x30, 0x2D, 0x2A, 0x28,
	0x25, 0x24, 0x21, 0x1F, 0x1E
};

/* main clock in Hz, PAL (FREQ_17_EXACT is for NTSC!) */
#define ASAP_MAIN_CLOCK  1773447U

UBYTE memory[65536 + 2];

static CpuState cpu_state;

static char sap_type;
static UWORD sap_player;
static UWORD sap_music;
static UWORD sap_init;
int sap_fastplay;
static ASAP_ModuleInfo loaded_module_info;

#ifndef APOKEYSND

/* structures to hold the 9 pokey control bytes */
UBYTE AUDF[4 * MAXPOKEYS];	/* AUDFx (D200, D202, D204, D206) */
UBYTE AUDC[4 * MAXPOKEYS];	/* AUDCx (D201, D203, D205, D207) */
UBYTE AUDCTL[MAXPOKEYS];	/* AUDCTL (D208) */
int Base_mult[MAXPOKEYS];		/* selects either 64Khz or 15Khz clock mult */

UBYTE poly9_lookup[511];
UBYTE poly17_lookup[16385];
static ULONG random_scanline_counter;

static int enable_stereo = 0;

#ifndef SOUND_GAIN /* sound gain can be pre-defined in the configure/Makefile */
#define SOUND_GAIN 4
#endif

static void POKEY_PutByte(UWORD addr, UBYTE byte)
{
#ifdef STEREO_SOUND
	addr &= enable_stereo ? 0x1f : 0x0f;
#else
	addr &= 0x0f;
#endif
	switch (addr) {
	case _AUDC1:
		AUDC[CHAN1] = byte;
		Update_pokey_sound(_AUDC1, byte, 0, SOUND_GAIN);
		break;
	case _AUDC2:
		AUDC[CHAN2] = byte;
		Update_pokey_sound(_AUDC2, byte, 0, SOUND_GAIN);
		break;
	case _AUDC3:
		AUDC[CHAN3] = byte;
		Update_pokey_sound(_AUDC3, byte, 0, SOUND_GAIN);
		break;
	case _AUDC4:
		AUDC[CHAN4] = byte;
		Update_pokey_sound(_AUDC4, byte, 0, SOUND_GAIN);
		break;
	case _AUDCTL:
		AUDCTL[0] = byte;

		/* determine the base multiplier for the 'div by n' calculations */
		if (byte & CLOCK_15)
			Base_mult[0] = DIV_15;
		else
			Base_mult[0] = DIV_64;

		Update_pokey_sound(_AUDCTL, byte, 0, SOUND_GAIN);
		break;
	case _AUDF1:
		AUDF[CHAN1] = byte;
		Update_pokey_sound(_AUDF1, byte, 0, SOUND_GAIN);
		break;
	case _AUDF2:
		AUDF[CHAN2] = byte;
		Update_pokey_sound(_AUDF2, byte, 0, SOUND_GAIN);
		break;
	case _AUDF3:
		AUDF[CHAN3] = byte;
		Update_pokey_sound(_AUDF3, byte, 0, SOUND_GAIN);
		break;
	case _AUDF4:
		AUDF[CHAN4] = byte;
		Update_pokey_sound(_AUDF4, byte, 0, SOUND_GAIN);
		break;
	case _STIMER:
		Update_pokey_sound(_STIMER, byte, 0, SOUND_GAIN);
		break;
#ifdef STEREO_SOUND
	case _AUDC1 + _POKEY2:
		AUDC[CHAN1 + CHIP2] = byte;
		Update_pokey_sound(_AUDC1, byte, 1, SOUND_GAIN);
		break;
	case _AUDC2 + _POKEY2:
		AUDC[CHAN2 + CHIP2] = byte;
		Update_pokey_sound(_AUDC2, byte, 1, SOUND_GAIN);
		break;
	case _AUDC3 + _POKEY2:
		AUDC[CHAN3 + CHIP2] = byte;
		Update_pokey_sound(_AUDC3, byte, 1, SOUND_GAIN);
		break;
	case _AUDC4 + _POKEY2:
		AUDC[CHAN4 + CHIP2] = byte;
		Update_pokey_sound(_AUDC4, byte, 1, SOUND_GAIN);
		break;
	case _AUDCTL + _POKEY2:
		AUDCTL[1] = byte;
		/* determine the base multiplier for the 'div by n' calculations */
		if (byte & CLOCK_15)
			Base_mult[1] = DIV_15;
		else
			Base_mult[1] = DIV_64;

		Update_pokey_sound(_AUDCTL, byte, 1, SOUND_GAIN);
		break;
	case _AUDF1 + _POKEY2:
		AUDF[CHAN1 + CHIP2] = byte;
		Update_pokey_sound(_AUDF1, byte, 1, SOUND_GAIN);
		break;
	case _AUDF2 + _POKEY2:
		AUDF[CHAN2 + CHIP2] = byte;
		Update_pokey_sound(_AUDF2, byte, 1, SOUND_GAIN);
		break;
	case _AUDF3 + _POKEY2:
		AUDF[CHAN3 + CHIP2] = byte;
		Update_pokey_sound(_AUDF3, byte, 1, SOUND_GAIN);
		break;
	case _AUDF4 + _POKEY2:
		AUDF[CHAN4 + CHIP2] = byte;
		Update_pokey_sound(_AUDF4, byte, 1, SOUND_GAIN);
		break;
	case _STIMER + _POKEY2:
		Update_pokey_sound(_STIMER, byte, 1, SOUND_GAIN);
		break;
#endif
	default:
		break;
	}
}

static void POKEY_Initialise(void)
{
	int i;
	ULONG reg;

	for (i = 0; i < (MAXPOKEYS * 4); i++) {
		AUDC[i] = 0;
		AUDF[i] = 0;
	}

	for (i = 0; i < MAXPOKEYS; i++) {
		AUDCTL[i] = 0;
		Base_mult[i] = DIV_64;
	}

	/* initialise poly9_lookup */
	reg = 0x1ff;
	for (i = 0; i < 511; i++) {
		reg = ((((reg >> 5) ^ reg) & 1) << 8) + (reg >> 1);
		poly9_lookup[i] = (UBYTE) reg;
	}
	/* initialise poly17_lookup */
	reg = 0x1ffff;
	for (i = 0; i < 16385; i++) {
		reg = ((((reg >> 5) ^ reg) & 0xff) << 9) + (reg >> 8);
		poly17_lookup[i] = (UBYTE) (reg >> 1);
	}

	random_scanline_counter = 0;
}

#endif /* APOKEYSND */

#define REAL_CYCLE (cpu_state.cycle + cpu_state.cycle / (LINE_C - DMAR) * DMAR)

UBYTE ASAP_GetByte(UWORD addr)
{
#ifndef APOKEYSND
	unsigned int i;
#endif
	switch (addr & 0xff0f) {
	case 0xd20a:
#ifdef APOKEYSND
		return (UBYTE) PokeySound_GetRandom(addr, REAL_CYCLE);
#else
		i = random_scanline_counter + REAL_CYCLE;
		if (AUDCTL[0] & POLY9)
			return poly9_lookup[i % POLY9_SIZE];
		else {
			const UBYTE *ptr;
			i %= POLY17_SIZE;
			ptr = poly17_lookup + (i >> 3);
			i &= 7;
			return (UBYTE) ((ptr[0] >> i) + (ptr[1] << (8 - i)));
		}
#endif
	case 0xd20e:
		return cpu_state.irqst;
	case 0xd40b:
		return (UBYTE) ((unsigned int) cpu_state.cycle / (unsigned int) (2 * (LINE_C - DMAR)) % 156U);
	default:
		return dGetByte(addr);
	}
}

void ASAP_PutByte(UWORD addr, UBYTE byte)
{
#ifdef APOKEYSND
	if ((addr >> 8) == 0xd2) {
		if ((addr & (loaded_module_info.channels == 1 ? 0xf : 0x1f)) == 0xe) {
			cpu_state.irqst |= byte ^ 0xff;
#define SET_TIMER_IRQ(ch) \
			if ((byte & cpu_state.irqst & ch) != 0 && cpu_state.timer##ch##_cycle == NEVER) { \
				int t = pokey_states[0].tick_cycle##ch; \
				while (t < REAL_CYCLE) \
					t += pokey_states[0].period_cycles##ch ; \
				t = t * (LINE_C - DMAR) / LINE_C; \
				cpu_state.timer##ch##_cycle = t; \
				if (cpu_state.nearest_event_cycle > t) \
					cpu_state.nearest_event_cycle = t; \
			} \
			else \
				cpu_state.timer##ch##_cycle = NEVER;
			SET_TIMER_IRQ(1);
			SET_TIMER_IRQ(2);
			SET_TIMER_IRQ(4);
		}
		else
			PokeySound_PutByte(addr, byte, REAL_CYCLE);
	}
	else if ((addr & 0xff0f) == 0xd40a) {
		int cycle = cpu_state.cycle % (LINE_C - DMAR) + DMAR;
		if (cycle <= WSYNC_C)
			cpu_state.cycle += WSYNC_C - cycle;
		else
			cpu_state.cycle += LINE_C - DMAR + WSYNC_C - cycle;
	}
	else
		dPutByte(addr, byte);
#else
	POKEY_PutByte(addr, byte);
#endif
}

/* We use CIM opcode to return from a subroutine to ASAP */
void ASAP_CIM(void)
{
	cpu_state.cycle = 0x7fffffff;
}

#ifdef APOKEYSND

#define block_rate     44100

#else

static int block_rate;
static int sample_format;
static int sample_16bit;

void ASAP_Initialize(int frequency, int audio_format, int quality)
{
	block_rate = frequency;
	sample_format = audio_format;
	sample_16bit = audio_format == AUDIO_FORMAT_U8 ? 0 : 1;
	enable_stereo = 5; /* force Pokey_sound_init() in ASAP_PlaySong() */
	POKEY_Initialise();
	if (quality == 0)
		enable_new_pokey = FALSE;
	else {
		Pokey_set_mzquality(quality - 1);
		enable_new_pokey = TRUE;
	}
}

#endif /* APOKEYSND */

#define MAX_DURATIONS  (sizeof(module_info->durations) / sizeof(module_info->durations[0]))

/* This array maps subsong numbers to track positions for MPT and RMT formats. */
static UBYTE song_pos[128];

static int tmc_per_frame;
static int tmc_per_frame_counter;

static int current_song;
static int current_duration;
#ifdef APOKEYSND
static int bytes_played;
static int atari_sound_index;
#else
static int blocks_played;
static int blockcycles_till_player;
static int blockcycles_per_player;
#endif

static const int perframe2fastplay[] = { 312, 312 / 2, 312 / 3, 312 / 4 };

static int load_native(const unsigned char *module, int module_len,
                       const unsigned char *player, char type)
{
	UWORD player_last_byte;
	int block_len;
	if (module[0] != 0xff || module[1] != 0xff)
		return FALSE;
	sap_player = player[2] + (player[3] << 8);
	player_last_byte = player[4] + (player[5] << 8);
	sap_music = module[2] + (module[3] << 8);
	if (sap_music <= player_last_byte)
		return FALSE;
	block_len = module[4] + (module[5] << 8) + 1 - sap_music;
	if (6 + block_len != module_len) {
		UWORD info_addr;
		int info_len;
		if (type != 'r' || 11 + block_len > module_len)
			return FALSE;
		/* allow optional info for Raster Music Tracker */
		info_addr = module[6 + block_len] + (module[7 + block_len] << 8);
		if (info_addr != sap_music + block_len)
			return FALSE;
		info_len = module[8 + block_len] + (module[9 + block_len] << 8) + 1 - info_addr;
		if (10 + block_len + info_len != module_len)
			return FALSE;
	}
	memcpy(memory + sap_music, module + 6, block_len);
	memcpy(memory + sap_player, player + 6, player_last_byte + 1 - sap_player);
	sap_type = type;
	return TRUE;
}

static int load_cmc(const unsigned char *module, int module_len,
                    ASAP_ModuleInfo *module_info, int cmr)
{
	int pos;
	if (module_len < 0x306)
		return FALSE;
	if (module_info == &loaded_module_info) {
		if (!load_native(module, module_len, cmc_obx, 'C'))
			return FALSE;
		if (cmr)
			memcpy(memory + 0x500 + CMR_BASS_TABLE_OFFSET, cmr_bass_table, sizeof(cmr_bass_table));
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

static int load_mpt(const unsigned char *module, int module_len,
                    ASAP_ModuleInfo *module_info)
{
	int track0_addr;
	int i;
	int song_len;
	/* seen[i] == TRUE if the track position i is already processed */
	UBYTE seen[256];
	if (module_len < 0x1d0)
		return FALSE;
	if (module_info == &loaded_module_info) {
		if (!load_native(module, module_len, mpt_obx, 'm'))
			return FALSE;
	}
	track0_addr = module[2] + (module[3] << 8) + 0x1ca;
	/* do not auto-detect number of subsongs if the address
	   of the first track is non-standard */
	if (module[0x1c6] + (module[0x1ca] << 8) != track0_addr) {
		if (module_info == &loaded_module_info)
			song_pos[0] = 0;
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
		UBYTE c;
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
		if (module_info == &loaded_module_info)
			song_pos[module_info->songs] = (UBYTE) j;
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

static int load_rmt(const unsigned char *module, int module_len,
                    ASAP_ModuleInfo *module_info)
{
	int i;
	UWORD module_start;
	UWORD song_start;
	UWORD song_last_byte;
	const unsigned char *song;
	int song_len;
	int pos_shift;
	UBYTE seen[256];
	if (module_len < 0x30 || module[6] != 'R' || module[7] != 'M'
	 || module[8] != 'T' || module[13] != 1)
		return FALSE;
	switch (module[9]) {
	case '4':
		pos_shift = 2;
		break;
#ifdef STEREO_SOUND
	case '8':
		module_info->channels = 2;
		pos_shift = 3;
		break;
#endif
	default:
		return FALSE;
	}
	i = module[12];
	if (i < 1 || i > 4)
		return FALSE;
	if (module_info == &loaded_module_info) {
		sap_fastplay = perframe2fastplay[i - 1];
		if (!load_native(module, module_len, module_info->channels == 2 ? rmt8_obx : rmt4_obx, 'r'))
			return FALSE;
		sap_player = 0x600;
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
		if (module_info == &loaded_module_info)
			song_pos[module_info->songs] = (UBYTE) j;
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

static int load_tmc(const unsigned char *module, int module_len,
					ASAP_ModuleInfo *module_info)
{
	int i;
	if (module_len < 0x1d0)
		return FALSE;
	if (module_info == &loaded_module_info) {
		if (!load_native(module, module_len, tmc_obx, 't'))
			return FALSE;
		tmc_per_frame = module[37];
		if (tmc_per_frame < 1 || tmc_per_frame > 4)
			return FALSE;
		sap_fastplay = perframe2fastplay[tmc_per_frame - 1];
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

static int load_tm2(const unsigned char *module, int module_len,
                    ASAP_ModuleInfo *module_info)
{
	int i;
	int song_end;
	int c;
	if (module_len < 0x3a4)
		return FALSE;
	if (module_info == &loaded_module_info) {
		i = module[0x25];
		if (i < 1 || i > 4)
			return FALSE;
		sap_fastplay = perframe2fastplay[i - 1];
		if (!load_native(module, module_len, tm2_obx, 'T'))
			return FALSE;
		sap_player = 0x500;
	}
	/* TODO: quadrophonic */
	if (module[0x1f] != 0)
#ifdef STEREO_SOUND
		module_info->channels = 2;
#else
		return FALSE;
#endif
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
	if (song_end < sap_music + 0x380 + 2 * 17)
		return FALSE;
	i = song_end - sap_music - 0x380;
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

static int parse_hex(UWORD *retval, const char *p)
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
	*retval = (UWORD) r;
	return TRUE;
}

static int parse_dec(int *retval, const char *p, int minval, int maxval)
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

static int parse_text(char *retval, const char *p)
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

static int load_sap(const UBYTE *sap_ptr, const UBYTE * const sap_end,
                    ASAP_ModuleInfo *module_info)
{
	char *p;
	int sap_signature = FALSE;
	int duration_index = 0;
	if (module_info == &loaded_module_info) {
		sap_type = '?';
		sap_player = 0xffff;
		sap_music = 0xffff;
		sap_init = 0xffff;
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
		if (module_info == &loaded_module_info) {
			if (strcmp(line, "TYPE") == 0) {
				sap_type = *p;
			}
			else if (strcmp(line, "PLAYER") == 0) {
				if (!parse_hex(&sap_player, p))
					return FALSE;
			}
			else if (strcmp(line, "MUSIC") == 0) {
				if (!parse_hex(&sap_music, p))
					return FALSE;
			}
			else if (strcmp(line, "INIT") == 0) {
				if (!parse_hex(&sap_init, p))
					return FALSE;
			}
			else if (strcmp(line, "FASTPLAY") == 0) {
				if (!parse_dec(&sap_fastplay, p, 1, 312))
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
		else if (strcmp(line, "STEREO") == 0) {
#ifdef STEREO_SOUND
			module_info->channels = 2;
#else
			return FALSE;
#endif
		}
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
	if (module_info != &loaded_module_info)
		return TRUE;
	switch (sap_type) {
	case 'B':
#ifdef APOKEYSND
	case 'D':
#endif
		if (sap_player == 0xffff || sap_init == 0xffff)
			return FALSE;
		break;
	case 'C':
		if (sap_player == 0xffff || sap_music == 0xffff)
			return FALSE;
		break;
#ifdef APOKEYSND
	case 'S':
		if (sap_init == 0xffff)
			return FALSE;
		sap_fastplay = 78;
		break;
#endif
	default:
		return FALSE;
	}
	if (sap_ptr[1] != 0xff)
		return FALSE;
	memset(memory, 0, sizeof(memory));
	sap_ptr += 2;
	while (sap_ptr + 5 <= sap_end) {
		int start_addr = sap_ptr[0] + (sap_ptr[1] << 8);
		int block_len = sap_ptr[2] + (sap_ptr[3] << 8) + 1 - start_addr;
		if (block_len <= 0 || sap_ptr + block_len > sap_end)
			return FALSE;
		sap_ptr += 4;
		memcpy(memory + start_addr, sap_ptr, block_len);
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

int ASAP_IsOurFile(const char *filename)
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
#ifdef STEREO_SOUND
	case ASAP_EXT('T', 'M', '8'):
#endif
	case ASAP_EXT('T', 'M', 'C'):
		return TRUE;
	default:
		return FALSE;
	}
}

int ASAP_GetModuleInfo(const char *filename, const unsigned char *module,
                       int module_len, ASAP_ModuleInfo *module_info)
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
		return load_cmc(module, module_len, module_info, FALSE);
	case ASAP_EXT('C', 'M', 'R'):
		return load_cmc(module, module_len, module_info, TRUE);
	case ASAP_EXT('D', 'M', 'C'):
		if (module_info == &loaded_module_info)
			sap_fastplay = 156;
		return load_cmc(module, module_len, module_info, FALSE);
	case ASAP_EXT('M', 'P', 'D'):
		if (module_info == &loaded_module_info)
			sap_fastplay = 156;
		return load_mpt(module, module_len, module_info);
	case ASAP_EXT('M', 'P', 'T'):
		return load_mpt(module, module_len, module_info);
	case ASAP_EXT('R', 'M', 'T'):
		return load_rmt(module, module_len, module_info);
	case ASAP_EXT('S', 'A', 'P'):
		return load_sap(module, module + module_len, module_info);
	case ASAP_EXT('T', 'M', '2'):
		return load_tm2(module, module_len, module_info);
#ifdef STEREO_SOUND
	case ASAP_EXT('T', 'M', '8'):
		module_info->channels = 2;
		return load_tmc(module, module_len, module_info);
#endif
	case ASAP_EXT('T', 'M', 'C'):
		return load_tmc(module, module_len, module_info);
	default:
		return FALSE;
	}
}

const ASAP_ModuleInfo *ASAP_Load(const char *filename,
                                 const unsigned char *module,
                                 int module_len)
{
	sap_fastplay = 312;
	if (ASAP_GetModuleInfo(filename, module, module_len, &loaded_module_info))
		return &loaded_module_info;
	else
		return NULL;
}

static void call_6502(UWORD addr, int max_scanlines)
{
	cpu_state.pc = addr;
	/* put a CIM at 0xd20a and a return address on stack */
	dPutByte(0xd20a, 0xd2);
	dPutByte(0x01fe, 0x09);
	dPutByte(0x01ff, 0xd2);
	cpu_state.s = 0xfd;
	cpu_state.cycle = 0;
	Cpu_Run(&cpu_state, max_scanlines * (LINE_C - DMAR));
}

/* 50 Atari frames for the initialization routine - some SAPs are self-extracting. */
#define SCANLINES_FOR_INIT  (50 * 312)

static void call_6502_init(UWORD addr, int a, int x, int y)
{
	cpu_state.a = a & 0xff;
	cpu_state.x = x & 0xff;
	cpu_state.y = y & 0xff;
	call_6502(addr, SCANLINES_FOR_INIT);
}

void ASAP_PlaySong(int song, int duration)
{
#ifndef APOKEYSND
	UWORD addr;
#endif
	current_song = song;
	current_duration = duration;
#ifdef APOKEYSND
	bytes_played = 0;
	atari_sound_len = 0;
	atari_sound_index = 0;
	PokeySound_Initialize(loaded_module_info.channels - 1);
#else
	blocks_played = 0;
	blockcycles_till_player = 0;
	blockcycles_per_player = 114U * sap_fastplay * block_rate;
	if ((1 << enable_stereo) != loaded_module_info.channels) {
		Pokey_sound_init(ASAP_MAIN_CLOCK, (uint16) block_rate,
			loaded_module_info.channels, sample_16bit ? SND_BIT16 : 0);
		enable_stereo = loaded_module_info.channels == 2 ? 1 : 0;
	}
	for (addr = _AUDF1; addr <= _STIMER; addr++)
		POKEY_PutByte(addr, 0);
	if (enable_stereo)
		for (addr = _AUDF1 + _POKEY2; addr <= _STIMER + _POKEY2; addr++)
			POKEY_PutByte(addr, 0);
#endif
	cpu_state.nz = 0;
	cpu_state.c = 0;
	cpu_state.vdi = I_FLAG;
	cpu_state.timer1_cycle = NEVER;
	cpu_state.timer2_cycle = NEVER;
	cpu_state.timer4_cycle = NEVER;
	cpu_state.irqst = 0xff;
	switch (sap_type) {
	case 'B':
		call_6502_init(sap_init, song, 0, 0);
		break;
	case 'C':
		call_6502_init((UWORD) (sap_player + 3), 0x70, sap_music, sap_music >> 8);
		call_6502_init((UWORD) (sap_player + 3), 0x00, song, 0);
		break;
#ifdef APOKEYSND
	case 'D':
	case 'S':
		cpu_state.a = song;
		cpu_state.x = 0x00;
		cpu_state.y = 0x00;
		cpu_state.s = 0xff;
		cpu_state.pc = sap_init;
		break;
#endif
	case 'm':
		call_6502_init(sap_player, 0x00, sap_music >> 8, sap_music);
		call_6502_init(sap_player, 0x02, song_pos[song], 0);
		break;
	case 'r':
		call_6502_init(sap_player, song_pos[song], sap_music, sap_music >> 8);
		break;
	case 't':
	case 'T':
		call_6502_init(sap_player, 0x70, sap_music >> 8, sap_music);
		call_6502_init(sap_player, 0x00, song, 0);
		tmc_per_frame_counter = 1;
		break;
	}
}

void call_6502_player(void)
{
	switch (sap_type) {
	case 'B':
		call_6502(sap_player, sap_fastplay);
		break;
	case 'C':
		call_6502((UWORD) (sap_player + 6), sap_fastplay);
		break;
#ifdef APOKEYSND
	case 'D':
#define PUSH_ON_6502_STACK(x)  dPutByte(0x100 + cpu_state.s, x); cpu_state.s = (cpu_state.s - 1) & 0xff
#define RETURN_FROM_PLAYER_ADDR  0xd200
		/* save 6502 state on 6502 stack */
		PUSH_ON_6502_STACK(cpu_state.pc >> 8);
		PUSH_ON_6502_STACK(cpu_state.pc & 0xff);
		PUSH_ON_6502_STACK(((cpu_state.nz | (cpu_state.nz >> 1)) & N_FLAG) + cpu_state.vdi + ((cpu_state.nz & 0xff) == 0 ? Z_FLAG : 0) + cpu_state.c + 0x20);
		PUSH_ON_6502_STACK(cpu_state.a);
		PUSH_ON_6502_STACK(cpu_state.x);
		PUSH_ON_6502_STACK(cpu_state.y);
		/* RTS will jump to 6502 code that restores the state */
		PUSH_ON_6502_STACK((RETURN_FROM_PLAYER_ADDR - 1) >> 8);
		PUSH_ON_6502_STACK((RETURN_FROM_PLAYER_ADDR - 1) & 0xff);
		dPutByte(RETURN_FROM_PLAYER_ADDR, 0x68);     /* PLA */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 1, 0xa8); /* TAY */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 2, 0x68); /* PLA */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 3, 0xaa); /* TAX */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 4, 0x68); /* PLA */
		dPutByte(RETURN_FROM_PLAYER_ADDR + 5, 0x40); /* RTI */
		cpu_state.pc = sap_player;
		cpu_state.cycle = 0;
		Cpu_Run(&cpu_state, sap_fastplay * (LINE_C - DMAR));
		break;
	case 'S':
		cpu_state.cycle = 0;
		Cpu_Run(&cpu_state, sap_fastplay * (LINE_C - DMAR));
		{
			int i = dGetByte(0x45) - 1;
			dPutByte(0x45, i);
			if (i == 0)
				dPutByte(0xb07b, dGetByte(0xb07b) + 1);
		}
		break;
#endif
	case 'm':
	case 'r':
	case 'T':
		call_6502((UWORD) (sap_player + 3), sap_fastplay);
		break;
	case 't':
		if (--tmc_per_frame_counter <= 0) {
			tmc_per_frame_counter = tmc_per_frame;
			call_6502((UWORD) (sap_player + 3), sap_fastplay);
		}
		else
			call_6502((UWORD) (sap_player + 6), sap_fastplay);
		break;
	}
#ifdef APOKEYSND
	PokeySound_Flush(114 * sap_fastplay);
#else
	random_scanline_counter = (random_scanline_counter + LINE_C * sap_fastplay)
	                          % ((AUDCTL[0] & POLY9) ? POLY9_SIZE : POLY17_SIZE);
#endif
}

static int milliseconds_to_blocks(int milliseconds)
{
	return (int) ((double) milliseconds * block_rate / 1000);
}

#ifdef APOKEYSND

int ASAP_Generate(void *buffer, int buffer_len)
{
	int remaining_bytes;
	if (current_duration > 0) {
		int total_bytes = milliseconds_to_blocks(current_duration)
			<< (loaded_module_info.channels - 1);
		if (bytes_played + buffer_len > total_bytes)
			buffer_len = total_bytes - bytes_played;
	}
	remaining_bytes = buffer_len;
	while (remaining_bytes > 0) {
		int bytes = atari_sound_len - atari_sound_index;
		if (bytes >= remaining_bytes) {
			memcpy(buffer, atari_sound + atari_sound_index, remaining_bytes);
			atari_sound_index += remaining_bytes;
			break;
		}
		memcpy(buffer, atari_sound + atari_sound_index, bytes);
		buffer = (void *) ((unsigned char *) buffer + bytes);
		remaining_bytes -= bytes;
		call_6502_player();
		atari_sound_index = 0;
	}
	bytes_played += buffer_len;
	return buffer_len;
}

#else

static int cpu_process(int blocks)
{
	int ready_blocks;
	while (blockcycles_till_player < ASAP_MAIN_CLOCK) {
		call_6502_player();
		blockcycles_till_player += blockcycles_per_player;
	}
	ready_blocks = blockcycles_till_player / ASAP_MAIN_CLOCK;
	if (blocks > ready_blocks)
		blocks = ready_blocks;
	blocks_played += blocks;
	blockcycles_till_player -= blocks * ASAP_MAIN_CLOCK;
	return blocks;
}

void ASAP_Seek(int position)
{
	int block = milliseconds_to_blocks(position);
	if (block < blocks_played)
		ASAP_PlaySong(current_song, current_duration);
	while (blocks_played < block)
		cpu_process(block - blocks_played);
}

/* swap bytes in non-native words if necessary */
static void fix_endianess(void *buffer, int samples)
{
	if (sample_format ==
#ifdef WORDS_BIGENDIAN
		AUDIO_FORMAT_S16_LE
#else
		AUDIO_FORMAT_S16_BE
#endif
		) {
		unsigned char *p = (unsigned char *) buffer;
		int n = samples;
		do {
			unsigned char t = p[0];
			p[0] = p[1];
			p[1] = t;
			p += 2;
		} while (--n != 0);
	}
}

int ASAP_Generate(void *buffer, int buffer_len)
{
	int buffer_blocks = buffer_len >> (sample_16bit + enable_stereo);
	int remaining_blocks;
	if (current_duration > 0) {
		int total_blocks = milliseconds_to_blocks(current_duration);
		if (blocks_played + buffer_blocks > total_blocks)
			buffer_blocks = total_blocks - blocks_played;
	}
	remaining_blocks = buffer_blocks;
	while (remaining_blocks > 0) {
		int blocks = cpu_process(remaining_blocks);
		int samples = blocks << enable_stereo;
		Pokey_process(buffer, samples);
		fix_endianess(buffer, samples);
		buffer = (void *) ((unsigned char *) buffer +
			(samples << sample_16bit));
		remaining_blocks -= blocks;
	}
	return buffer_blocks << (sample_16bit + enable_stereo);
}

#endif /* APOKEYSND */
