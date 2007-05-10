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

#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "asap.h"

void call_6502_player(ASAP_State *as);

static abool detect_time = FALSE;
static int scan_player_calls;
static int silence_player_calls;
static int loop_check_player_calls;
static int loop_min_player_calls;
static byte *registers_dump;

static ASAP_State asap;
static abool dump = FALSE;

#define FEATURE_CHECK          1
#define FEATURE_15_KHZ         2
#define FEATURE_HIPASS_FILTER  4
#define FEATURE_LOW_OF_16_BIT  8
#define FEATURE_9_BIT_POLY     16
static int features = 0;

static void print_help(void)
{
	printf(
		"Usage: asapscan COMMAND INPUTFILE\n"
		"Commands:\n"
		"-d  Dump POKEY registers\n"
		"-f  List POKEY features used\n"
		"-t  Detect silence and loops\n"
	);
}

static abool store_pokey(byte *p, PokeyState *ps)
{
	abool is_silence = TRUE;
	p[0] = ps->audf1;
	p[1] = ps->audc1;
	if ((ps->audc1 & 0xf) != 0)
		is_silence = FALSE;
	p[2] = ps->audf2;
	p[3] = ps->audc2;
	if ((ps->audc2 & 0xf) != 0)
		is_silence = FALSE;
	p[4] = ps->audf3;
	p[5] = ps->audc3;
	if ((ps->audc3 & 0xf) != 0)
		is_silence = FALSE;
	p[6] = ps->audf4;
	p[7] = ps->audc4;
	if ((ps->audc4 & 0xf) != 0)
		is_silence = FALSE;
	p[8] = ps->audctl;
	return is_silence;
}

static void print_pokey(PokeyState *ps)
{
	printf(
		"%02X %02X  %02X %02X  %02X %02X  %02X %02X  %02X",
		ps->audf1, ps->audc1, ps->audf2, ps->audc2,
		ps->audf3, ps->audc3, ps->audf4, ps->audc4, ps->audctl
	);
}

static int seconds_to_player_calls(int seconds)
{
	return (int) (seconds * 1773447.0 / 114.0 / asap.sap_fastplay);
}

static int player_calls_to_milliseconds(int player_calls)
{
	return (int) ceil(player_calls * asap.sap_fastplay * 114.0 * 1000 / 1773447.0);
}

void scan_song(int song)
{
	int i;
	int silence_run = 0;
	int loop_bytes = 18 * loop_check_player_calls;
	ASAP_PlaySong(&asap, song, -1);
	for (i = 0; i < scan_player_calls; i++) {
		byte *p = registers_dump + 18 * i;
		abool is_silence;
		call_6502_player(&asap);
		is_silence = store_pokey(p, &asap.base_pokey);
		is_silence &= store_pokey(p + 9, &asap.extra_pokey);
		if (dump) {
			printf("%6.2f: ", i * asap.sap_fastplay * 114.0 / 1773447.0);
			print_pokey(&asap.base_pokey);
			if (asap.module_info.channels == 2) {
				printf("  |  ");
				print_pokey(&asap.extra_pokey);
			}
			printf("\n");
		}
		if (features != 0) {
			int c1 = asap.base_pokey.audctl;
			int c2 = asap.extra_pokey.audctl;
			if (((c1 | c2) & 1) != 0)
				features |= FEATURE_15_KHZ;
			if (((c1 | c2) & 6) != 0)
				features |= FEATURE_HIPASS_FILTER;
			if (((c1 & 0x40) != 0 && (asap.base_pokey.audc1 & 0xf) != 0)
			|| ((c1 & 0x20) != 0 && (asap.base_pokey.audc3 & 0xf) != 0))
				features |= FEATURE_LOW_OF_16_BIT;
			if (((c1 | c2) & 0x80) != 0)
				features |= FEATURE_9_BIT_POLY;
		}
		if (detect_time) {
			if (is_silence) {
				silence_run++;
				if (silence_run >= silence_player_calls) {
					int duration = player_calls_to_milliseconds(i - silence_run);
					printf("TIME %02d:%02d.%02d\n", duration / 60000, duration / 1000 % 60, duration / 10 % 100);
					return;
				}
			}
			else
				silence_run = 0;
			if (i > loop_check_player_calls) {
				byte *q;
				if (memcmp(p - loop_bytes - 18, p - loop_bytes, loop_bytes) == 0) {
					/* POKEY registers do not change - probably an ultrasound */
					int duration = player_calls_to_milliseconds(i - loop_check_player_calls);
					printf("TIME %02d:%02d.%02d\n", duration / 60000, duration / 1000 % 60, duration / 10 % 100);
					return;
				}
				for (q = registers_dump; q < p - loop_bytes - 18 * loop_min_player_calls; q += 18) {
					if (memcmp(q, p - loop_bytes, loop_bytes) == 0) {
						int duration = player_calls_to_milliseconds(i - loop_check_player_calls);
						printf("TIME %02d:%02d.%02d LOOP\n", duration / 60000, duration / 1000 % 60, duration / 10 % 100);
						return;
					}
				}
			}
		}
	}
	if (detect_time)
		printf("No silence or loop detected in song %d\n", song);
}

int main(int argc, char *argv[])
{
	const char *input_file;
	int scan_seconds = 15 * 60;
	int silence_seconds = 5;
	int loop_check_seconds = 3 * 60;
	int loop_min_seconds = 5;
	FILE *fp;
	static byte module[ASAP_MODULE_MAX];
	int module_len;
	int song;
	if (argc != 3) {
		print_help();
		return 1;
	}
	if (strcmp(argv[1], "-d") == 0)
		dump = TRUE;
	else if (strcmp(argv[1], "-f") == 0)
		features = 1;
	else if (strcmp(argv[1], "-t") == 0)
		detect_time = TRUE;
	else {
		print_help();
		return 1;
	}
	input_file = argv[2];
	fp = fopen(input_file, "rb");
	if (fp == NULL) {
		fprintf(stderr, "asapscan: cannot open %s\n", argv[2]);
		return 1;
	}
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);
	if (!ASAP_Load(&asap, input_file, module, module_len)) {
		fprintf(stderr, "asapscan: %s: format not supported\n", input_file);
		return 1;
	}
	scan_player_calls = seconds_to_player_calls(scan_seconds);
	silence_player_calls = seconds_to_player_calls(silence_seconds);
	loop_check_player_calls = seconds_to_player_calls(loop_check_seconds);
	loop_min_player_calls = seconds_to_player_calls(loop_min_seconds);
	registers_dump = malloc(scan_player_calls * 18);
	if (registers_dump == NULL) {
		fprintf(stderr, "asapscan: out of memory\n");
		return 1;
	}
	for (song = 0; song < asap.module_info.songs; song++)
		scan_song(song);
	free(registers_dump);
	if (features != 0) {
		if ((features & FEATURE_15_KHZ) != 0)
			printf("15 kHz clock\n");
		if ((features & FEATURE_HIPASS_FILTER) != 0)
			printf("Hi-pass filter\n");
		if ((features & FEATURE_LOW_OF_16_BIT) != 0)
			printf("Low byte of 16-bit counter\n");
		if ((features & FEATURE_9_BIT_POLY) != 0)
			printf("9-bit poly\n");
	}
	return 0;
}
