/*
 * asapscan.c - SAP song length detector
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

#include "config.h"
#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "asap.h"

#ifndef TRUE
#define TRUE 1
#endif
#ifndef FALSE
#define FALSE 0
#endif

extern unsigned char AUDF[8];
extern unsigned char AUDC[8];
extern unsigned char AUDCTL[2];
extern int sap_fastplay;
void call_6502_player(void);

static int dump = FALSE;
static int scan_player_calls;
static int silence_player_calls;
static int loop_player_calls;
static unsigned char *registers_dump;

static int seconds_to_player_calls(int seconds)
{
	return (int) (seconds * 1773447.0 / 114.0 / sap_fastplay);
}

static int player_calls_to_seconds(int player_calls)
{
	return (int) ceil(player_calls * sap_fastplay * 114.0 / 1773447.0);
}

void scan_song(int song)
{
	int i;
	int silence_run = 0;
	ASAP_PlaySong(song, 0);
	for (i = 0; i < scan_player_calls; i++) {
		unsigned char *p = registers_dump + 18 * i;
		int j;
		int is_silence = TRUE;
		call_6502_player();
		for (j = 0; j < 8; j++) {
			p[j] = AUDF[j];
			p[8 + j] = AUDC[j];
			if ((AUDC[j] & 0xf) != 0)
				is_silence = FALSE;
		}
		p[16] = AUDCTL[0];
		p[17] = AUDCTL[1];
		if (dump) {
			printf(
				"%6.2f: %02X %02X  %02X %02X  %02X %02X  %02X %02X  %02X  |  "
				"%02X %02X  %02X %02X  %02X %02X  %02X %02X  %02X\n", i * sap_fastplay * 114.0 / 1773447.0,
				AUDF[0], AUDC[0], AUDF[1], AUDC[1], AUDF[2], AUDC[2], AUDF[3], AUDC[3], AUDCTL[0],
				AUDF[4], AUDC[4], AUDF[5], AUDC[5], AUDF[6], AUDC[6], AUDF[7], AUDC[7], AUDCTL[1]
			);
		}
		if (is_silence) {
			silence_run++;
			if (silence_run >= silence_player_calls) {
				int seconds = player_calls_to_seconds(i - silence_run);
				printf("TIME %02d:%02d\n", seconds / 60, seconds % 60);
				return;
			}
		}
		else
			silence_run = 0;
		if (i >= 2 * loop_player_calls) {
			if (memcmp(registers_dump, p - 18 * loop_player_calls, 18 * loop_player_calls) == 0) {
				int seconds = player_calls_to_seconds(i - loop_player_calls);
				printf("TIME %02d:%02d LOOP\n", seconds / 60, seconds % 60);
				return;
			}
		}
	}
	printf("No silence or loop detected\n");
}

int main(int argc, char *argv[])
{
	int scan_seconds = 15 * 60;
	int silence_seconds = 10;
	int loop_seconds = 5 * 60;
	FILE *fp;
	static unsigned char module[ASAP_MODULE_MAX];
	int module_len;
	const ASAP_ModuleInfo *module_info;
	int song;
	if (argc != 2 || strcmp(argv[1], "--h") == 0 || strcmp(argv[1], "--help") == 0) {
		printf("Usage: asapscan INPUTFILE\n");
		return 1;
	}
	fp = fopen(argv[1], "rb");
	if (fp == NULL) {
		fprintf(stderr, "asapscan: cannot open %s\n", argv[1]);
		return 1;
	}
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);
	ASAP_Initialize(44100, AUDIO_FORMAT_U8, 0);
	module_info = ASAP_Load(argv[1], module, module_len);
	if (module_info == NULL) {
		fprintf(stderr, "asapscan: %s: format not supported\n", argv[1]);
		return 1;
	}
	scan_player_calls = seconds_to_player_calls(scan_seconds);
	silence_player_calls = seconds_to_player_calls(silence_seconds);
	loop_player_calls = seconds_to_player_calls(loop_seconds);
	registers_dump = malloc(scan_player_calls * 18);
	if (registers_dump == NULL) {
		fprintf(stderr, "asapscan: out of memory\n");
		return 1;
	}
	for (song = 0; song < module_info->songs; song++)
		scan_song(song);
	free(registers_dump);
	return 0;
}
