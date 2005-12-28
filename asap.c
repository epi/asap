/*
 * asap.c - ASAP engine
 *
 * Copyright (C) 2005  Piotr Fusik
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

#include "asap_internal.h"
#include "cpu.h"
#include "pokey.h"
#include "pokeysnd.h"

#include "players.h"

#define CMR_BASS_TABLE_OFFSET  0x70d

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

int xpos = 0;
int xpos_limit = 0;
UBYTE wsync_halt = 0;

/* structures to hold the 9 pokey control bytes */
UBYTE AUDF[4 * MAXPOKEYS];	/* AUDFx (D200, D202, D204, D206) */
UBYTE AUDC[4 * MAXPOKEYS];	/* AUDCx (D201, D203, D205, D207) */
UBYTE AUDCTL[MAXPOKEYS];	/* AUDCTL (D208) */
int Base_mult[MAXPOKEYS];		/* selects either 64Khz or 15Khz clock mult */

UBYTE poly9_lookup[511];
UBYTE poly17_lookup[16385];
static ULONG random_scanline_counter;

#ifndef SOUND_GAIN /* sound gain can be pre-defined in the configure/Makefile */
#define SOUND_GAIN 4
#endif

static void POKEY_PutByte(UWORD addr, UBYTE byte)
{
#ifdef STEREO_SOUND
	addr &= stereo_enabled ? 0x1f : 0x0f;
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

UBYTE ASAP_GetByte(UWORD addr)
{
	unsigned int i;
	switch (addr & 0xff0f) {
	case 0xd20a:
		i = random_scanline_counter + (unsigned int) xpos + (unsigned int) xpos / LINE_C * DMAR;
		if (AUDCTL[0] & POLY9)
			return poly9_lookup[i % POLY9_SIZE];
		else {
			const UBYTE *ptr;
			i %= POLY17_SIZE;
			ptr = poly17_lookup + (i >> 3);
			i &= 7;
			return (UBYTE) ((ptr[0] >> i) + (ptr[1] << (8 - i)));
		}
	case 0xd40b:
		return (UBYTE) ((unsigned int) xpos / (unsigned int) (2 * (LINE_C - DMAR)) % 156U);
	default:
		return dGetByte(addr);
	}
}

void ASAP_PutByte(UWORD addr, UBYTE byte)
{
	/* TODO: implement WSYNC */
#if 0
	if ((addr >> 8) == 0xd2)
		POKEY_PutByte(addr, byte);
	else if ((addr & 0xff0f) == 0xd40a) {
		if (xpos <= WSYNC_C && xpos_limit >= WSYNC_C)
			xpos = WSYNC_C;
		else {
			wsync_halt = TRUE;
			xpos = xpos_limit;
		}
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
	xpos = xpos_limit;
}

static unsigned int sample_frequency;

void ASAP_Initialize(unsigned int frequency, unsigned int quality)
{
	sample_frequency = frequency;
	POKEY_Initialise();
	if (quality == 0)
		enable_new_pokey = FALSE;
	else {
		Pokey_set_mzquality(quality - 1);
		enable_new_pokey = TRUE;
	}
	Pokey_sound_init(ASAP_MAIN_CLOCK, (uint16) frequency, 1, 0);
}

static char sap_type;
static UWORD sap_player;
static UWORD sap_music;
static UWORD sap_init;
static unsigned int sap_songs;
static unsigned int sap_defsong;
static unsigned int sap_fastplay;

/* This array maps subsong numbers to track positions.
   Note: the original MPT supports up to 128 track positions, therefore up to 64
   subsongs (each subsong must have at least one pattern entry and a jump/stop).
   We additionally support here the modified MPT ("10.5") which supports
   up to 254 track positions, even though the player doesn't support it yet. */
static UBYTE mpt_song_pos[127];

static unsigned int tmc_per_frame;
static unsigned int tmc_per_frame_counter;

static unsigned int sampleclocks;
static unsigned int sampleclocks_per_player;

static const unsigned int perframe2fastplay[] = { 312U, 312U / 2U, 312U / 3U, 312U / 4U };

static int load_native(const unsigned char *module, unsigned int module_len,
                       const unsigned char *player, unsigned int player_len,
                       unsigned int player_addr, char type)
{
	int block_len;
	if (module[0] != 0xff || module[1] != 0xff)
		return FALSE;
	sap_music = module[2] + (module[3] << 8);
	if (sap_music < player_addr + player_len)
		return FALSE;
	block_len = module[4] + (module[5] << 8) + 1 - sap_music;
	if ((unsigned int) (6 + block_len) != module_len) {
		UWORD info_addr;
		int info_len;
		if (type != 'r' || (unsigned int) (11 + block_len) > module_len)
			return FALSE;
		/* allow optional info for Raster Music Tracker */
		info_addr = module[6 + block_len] + (module[7 + block_len] << 8);
		if (info_addr != sap_music + block_len)
			return FALSE;
		info_len = module[8 + block_len] + (module[9 + block_len] << 8) + 1 - info_addr;
		if ((unsigned int) (10 + block_len + info_len) != module_len)
			return FALSE;
	}
	memcpy(memory + sap_music, module + 6, block_len);
	memcpy(memory + player_addr, player, player_len);
	sap_type = type;
	sap_player = player_addr;
	return TRUE;
}

static int load_cmc(const unsigned char *module, unsigned int module_len, int cmr)
{
	int pos;
	if (module_len < 0x300)
		return FALSE;
	if (!load_native(module, module_len, cmc_0500_raw_data, sizeof(cmc_0500_raw_data), 0x500, 'C'))
		return FALSE;
	if (cmr)
		memcpy(memory + 0x500 + CMR_BASS_TABLE_OFFSET, cmr_bass_table, sizeof(cmr_bass_table));
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
			sap_songs++;
	}
	return TRUE;
}

static int load_mpt(const unsigned char *module, unsigned int module_len)
{
	if (module_len < 0x1d0)
		return FALSE;
	if (!load_native(module, module_len, mpt_0500_raw_data, sizeof(mpt_0500_raw_data), 0x500, 'm'))
		return FALSE;
	/* auto-detect number of subsongs - only if the address of the first track is standard */
	if (module[0x1c6] + (module[0x1ca] << 8) == sap_music + 0x1ca) {
		/* we look for jump/stop commands only in the first track,
		   even though the players allow them in any track */
		int i;
		int song_len;
		/* seen[i] == TRUE if the track position i is already processed */
		UBYTE seen[256];
		/* Calculate the length of the first track. Address of the second track minus
		   address of the first track equals the length of the first track in bytes.
		   Divide by two to get number of track positions. */
		song_len = (module[0x1c7] + (module[0x1cb] << 8) - sap_music - 0x1ca) >> 1;
		if (song_len > 0xfe)
			return FALSE;
		memset(seen, FALSE, sizeof(seen));
		sap_songs = 0;
		for (i = 0; i < song_len; i++) {
			int j;
			UBYTE c;
			if (seen[i])
				continue;
			j = i;
			do {
				seen[j] = TRUE;
				c = module[0x1d0 + j * 2];
				if (c != 0xff)
					break;
				j = module[0x1d1 + j * 2];
			} while (j < song_len && !seen[j]);
			if (c >= 64)
				continue;
			mpt_song_pos[sap_songs++] = (UBYTE) j;
			j++;
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
		return sap_songs != 0;
	}
	mpt_song_pos[0] = 0;
	return TRUE;
}

static int load_rmt(const unsigned char *module, unsigned int module_len)
{
	unsigned int i;
	if (module_len < 0x30 || memcmp(module + 6, "RMT4", 4) != 0)
		return FALSE;
	i = module[12];
	if (i < 1 || i > 4)
		return FALSE;
	sap_fastplay = perframe2fastplay[i - 1];
	if (!load_native(module, module_len, rmt_0390_raw_data, sizeof(rmt_0390_raw_data), 0x390, 'r'))
		return FALSE;
	/* TODO: detect subsongs */
	sap_player = 0x600;
	return TRUE;
}

static int load_tmc(const unsigned char *module, unsigned int module_len)
{
	unsigned int i;
	if (module_len < 0x1d0)
		return FALSE;
	if (!load_native(module, module_len, tmc_0500_raw_data, sizeof(tmc_0500_raw_data), 0x500, 't'))
		return FALSE;
	tmc_per_frame = module[37];
	if (tmc_per_frame < 1 || tmc_per_frame > 4)
		return FALSE;
	sap_fastplay = perframe2fastplay[tmc_per_frame - 1];
	i = 0;
	/* find first instrument */
	while (module[0x66 + i] == 0) {
		if (++i >= 64)
			return FALSE; /* no instrument */
	}
	i = (module[0x66 + i] << 8) + module[0x26 + i] - sap_music - 1 + 6;
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
			sap_songs++;
		i -= 16;
	}
	return TRUE;
}

static int tag_matches(const char *tag, const UBYTE *sap_ptr, const UBYTE *sap_end)
{
	size_t len = strlen(tag);
	return (sap_ptr + len + 8 < sap_end) && memcmp(tag, sap_ptr, len) == 0;
}

static int parse_hex(const UBYTE **ps, UWORD *retval)
{
	int chars = 0;
	*retval = 0;
	while (**ps != 0x0d) {
		char c;
		if (++chars > 4)
			return FALSE;
		c = (char) *(*ps)++;
		*retval <<= 4;
		if (c >= '0' && c <= '9')
			*retval += c - '0';
		else if (c >= 'A' && c <= 'F')
			*retval += c - 'A' + 10;
		else if (c >= 'a' && c <= 'f')
			*retval += c - 'a' + 10;
		else
			return FALSE;
	}
	return chars != 0;
}

static int parse_dec(const UBYTE **ps, unsigned int *retval)
{
	int chars = 0;
	*retval = 0;
	while (**ps != 0x0d) {
		char c;
		if (++chars > 3)
			return FALSE;
		c = (char) *(*ps)++;
		*retval *= 10;
		if (c >= '0' && c <= '9')
			*retval += c - '0';
		else
			return FALSE;
	}
	return chars != 0;
}

static int load_sap(const UBYTE *sap_ptr, const UBYTE * const sap_end)
{
	if (!tag_matches("SAP", sap_ptr, sap_end))
		return FALSE;
	sap_type = '?';
	sap_player = 0xffff;
	sap_music = 0xffff;
	sap_init = 0xffff;
	sap_ptr += 3;
	for (;;) {
		if (sap_ptr + 8 >= sap_end || sap_ptr[0] != 0x0d || sap_ptr[1] != 0x0a)
			return FALSE;
		sap_ptr += 2;
		if (sap_ptr[0] == 0xff)
			break;
		if (tag_matches("TYPE ", sap_ptr, sap_end)) {
			sap_ptr += 5;
			sap_type = *sap_ptr++;
		}
		else if (tag_matches("PLAYER ", sap_ptr, sap_end)) {
			sap_ptr += 7;
			if (!parse_hex(&sap_ptr, &sap_player))
				return FALSE;
		}
		else if (tag_matches("MUSIC ", sap_ptr, sap_end)) {
			sap_ptr += 6;
			if (!parse_hex(&sap_ptr, &sap_music))
				return FALSE;
		}
		else if (tag_matches("INIT ", sap_ptr, sap_end)) {
			sap_ptr += 5;
			if (!parse_hex(&sap_ptr, &sap_init))
				return FALSE;
		}
		else if (tag_matches("SONGS ", sap_ptr, sap_end)) {
			sap_ptr += 6;
			if (!parse_dec(&sap_ptr, &sap_songs) || sap_songs < 1 || sap_songs > 255)
				return FALSE;
		}
		else if (tag_matches("DEFSONG ", sap_ptr, sap_end)) {
			sap_ptr += 8;
			if (!parse_dec(&sap_ptr, &sap_defsong))
				return FALSE;
		}
		else if (tag_matches("FASTPLAY ", sap_ptr, sap_end)) {
			sap_ptr += 9;
			if (!parse_dec(&sap_ptr, &sap_fastplay) || sap_fastplay < 1 || sap_fastplay > 312)
				return FALSE;
		}
		else if (tag_matches("STEREO", sap_ptr, sap_end))
			return FALSE;
		/* ignore unknown tag*/
		while (sap_ptr[0] != 0x0d) {
			sap_ptr++;
			if (sap_ptr >= sap_end)
				return FALSE;
		}
	}
	if (sap_defsong >= sap_songs)
		return FALSE;
	switch (sap_type) {
	case 'B':
		if (sap_player == 0xffff || sap_init == 0xffff)
			return FALSE;
		break;
	case 'C':
		if (sap_player == 0xffff || sap_music == 0xffff)
			return FALSE;
		break;
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

#define EXT(c1, c2, c3) ((c1 << 16) + (c2 << 8) + c3)

int ASAP_Load(const char *format, const unsigned char *module, unsigned int module_len)
{
	int ext;
	ext = 0;
	while (*format != '\0') {
		if (ext > 0xffff)
			return FALSE; /* fourth character */
		ext = (ext << 8) + (*format++ & 0xff);
	}
	sap_songs = 1;
	sap_defsong = 0;
	sap_fastplay = 312;
	switch (ext & 0xdfdfdf) {
	case EXT('C', 'M', 'C'):
		return load_cmc(module, module_len, FALSE);
	case EXT('C', 'M', 'R'):
		return load_cmc(module, module_len, TRUE);
	case EXT('D', 'M', 'C'):
		sap_fastplay = 156;
		return load_cmc(module, module_len, FALSE);
	case EXT('M', 'P', 'D'):
		sap_fastplay = 156;
		return load_mpt(module, module_len);
	case EXT('M', 'P', 'T'):
		return load_mpt(module, module_len);
	case EXT('R', 'M', 'T'):
		return load_rmt(module, module_len);
	case EXT('S', 'A', 'P'):
		return load_sap(module, module + module_len);
	case EXT('T', 'M', 'C'):
		return load_tmc(module, module_len);
	default:
		return FALSE;
	}
}

unsigned int ASAP_GetSongs(void)
{
	return sap_songs;
}

unsigned int ASAP_GetDefSong(void)
{
	return sap_defsong;
}

static void call_6502(UWORD addr, int max_scanlines)
{
	regPC = addr;
	/* put a CIM at 0xd20a and a return address on stack */
	dPutByte(0xd20a, 0xd2);
	dPutByte(0x01fe, 0x09);
	dPutByte(0x01ff, 0xd2);
	regS = 0xfd;
	xpos = 0;
	GO(max_scanlines * (LINE_C - DMAR));
}

void ASAP_PlaySong(unsigned int song)
{
	UWORD addr;
	for (addr = _AUDF1; addr <= _STIMER; addr++)
		POKEY_PutByte(addr, 0);
	sampleclocks = 0;
	sampleclocks_per_player = 114U * sap_fastplay * sample_frequency;
	regP = 0x30;
	switch (sap_type) {
	case 'B':
		regA = (UBYTE) song;
		regX = 0x00;
		regY = 0x00;
		/* 5 frames should be enough */
		call_6502(sap_init, 5 * 312);
		break;
	case 'C':
		regA = 0x70;
		regX = (UBYTE) sap_music;
		regY = (UBYTE) (sap_music >> 8);
		call_6502((UWORD) (sap_player + 3), 5 * 312);
		regA = 0x00;
		regX = (UBYTE) song;
		call_6502((UWORD) (sap_player + 3), 5 * 312);
		break;
	case 'm':
		regA = 0x00;
		regX = (UBYTE) (sap_music >> 8);
		regY = (UBYTE) sap_music;
		call_6502(sap_player, 5 * 312);
		regA = 0x02;
		regX = mpt_song_pos[song];
		call_6502(sap_player, 5 * 312);
		break;
	case 'r':
		regA = 0x00;
		regX = (UBYTE) sap_music;
		regY = (UBYTE) (sap_music >> 8);
		call_6502(sap_player, 5 * 312);
		break;
	case 't':
		regA = 0x70;
		regX = (UBYTE) (sap_music >> 8);
		regY = (UBYTE) sap_music;
		call_6502(sap_player, 5 * 312);
		regA = 0x00;
		regX = (UBYTE) song;
		call_6502(sap_player, 5 * 312);
		tmc_per_frame_counter = 1;
		break;
	}
}

void ASAP_Generate(void *buffer, unsigned int buffer_len)
{
	for (;;) {
		unsigned int samples = sampleclocks / ASAP_MAIN_CLOCK;
		if (samples != 0U) {
			if (samples > buffer_len)
				samples = buffer_len;
			sampleclocks -= samples * ASAP_MAIN_CLOCK;
			Pokey_process(buffer, samples);
			buffer_len -= samples;
			if (buffer_len == 0U)
				return;
			buffer = (void *) ((unsigned char *) buffer + samples);
		}
		switch (sap_type) {
		case 'B':
			call_6502(sap_player, sap_fastplay);
			break;
		case 'C':
			call_6502((UWORD) (sap_player + 6), sap_fastplay);
			break;
		case 'm':
		case 'r':
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
		random_scanline_counter = (random_scanline_counter + LINE_C * sap_fastplay)
		                          % ((AUDCTL[0] & POLY9) ? POLY9_SIZE : POLY17_SIZE);
		sampleclocks += sampleclocks_per_player;
	}
}
