/*
 * asapscan.c - Atari 8-bit music analyzer
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
#include "asap_internal.h"

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

abool cpu_trace = FALSE;

static const char cpu_instructions[256][10] = {
	"BRK", "ORA (1,X)", "CIM", "ASO (1,X)", "NOP 1", "ORA 1", "ASL 1", "ASO 1",
	"PHP", "ORA #1", "ASL", "ANC #1", "NOP 2", "ORA 2", "ASL 2", "ASO 2",
	"BPL 0", "ORA (1),Y", "CIM", "ASO (1),Y", "NOP 1,X", "ORA 1,X", "ASL 1,X", "ASO 1,X",
	"CLC", "ORA 2,Y", "NOP !", "ASO 2,Y", "NOP 2,X", "ORA 2,X", "ASL 2,X", "ASO 2,X",
	"JSR 2", "AND (1,X)", "CIM", "RLA (1,X)", "BIT 1", "AND 1", "ROL 1", "RLA 1",
	"PLP", "AND #1", "ROL", "ANC #1", "BIT 2", "AND 2", "ROL 2", "RLA 2",
	"BMI 0", "AND (1),Y", "CIM", "RLA (1),Y", "NOP 1,X", "AND 1,X", "ROL 1,X", "RLA 1,X",
	"SEC", "AND 2,Y", "NOP !", "RLA 2,Y", "NOP 2,X", "AND 2,X", "ROL 2,X", "RLA 2,X",

	"RTI", "EOR (1,X)", "CIM", "LSE (1,X)", "NOP 1", "EOR 1", "LSR 1", "LSE 1",
	"PHA", "EOR #1", "LSR", "ALR #1", "JMP 2", "EOR 2", "LSR 2", "LSE 2",
	"BVC 0", "EOR (1),Y", "CIM", "LSE (1),Y", "NOP 1,X", "EOR 1,X", "LSR 1,X", "LSE 1,X",
	"CLI", "EOR 2,Y", "NOP !", "LSE 2,Y", "NOP 2,X", "EOR 2,X", "LSR 2,X", "LSE 2,X",
	"RTS", "ADC (1,X)", "CIM", "RRA (1,X)", "NOP 1", "ADC 1", "ROR 1", "RRA 1",
	"PLA", "ADC #1", "ROR", "ARR #1", "JMP (2)", "ADC 2", "ROR 2", "RRA 2",
	"BVS 0", "ADC (1),Y", "CIM", "RRA (1),Y", "NOP 1,X", "ADC 1,X", "ROR 1,X", "RRA 1,X",
	"SEI", "ADC 2,Y", "NOP !", "RRA 2,Y", "NOP 2,X", "ADC 2,X", "ROR 2,X", "RRA 2,X",

	"NOP #1", "STA (1,X)", "NOP #1", "SAX (1,X)", "STY 1", "STA 1", "STX 1", "SAX 1",
	"DEY", "NOP #1", "TXA", "ANE #1", "STY 2", "STA 2", "STX 2", "SAX 2",
	"BCC 0", "STA (1),Y", "CIM", "SHA (1),Y", "STY 1,X", "STA 1,X", "STX 1,Y", "SAX 1,Y",
	"TYA", "STA 2,Y", "TXS", "SHS 2,Y", "SHY 2,X", "STA 2,X", "SHX 2,Y", "SHA 2,Y",
	"LDY #1", "LDA (1,X)", "LDX #1", "LAX (1,X)", "LDY 1", "LDA 1", "LDX 1", "LAX 1",
	"TAY", "LDA #1", "TAX", "ANX #1", "LDY 2", "LDA 2", "LDX 2", "LAX 2",
	"BCS 0", "LDA (1),Y", "CIM", "LAX (1),Y", "LDY 1,X", "LDA 1,X", "LDX 1,Y", "LAX 1,X",
	"CLV", "LDA 2,Y", "TSX", "LAS 2,Y", "LDY 2,X", "LDA 2,X", "LDX 2,Y", "LAX 2,Y",

	"CPY #1", "CMP (1,X)", "NOP #1", "DCM (1,X)", "CPY 1", "CMP 1", "DEC 1", "DCM 1",
	"INY", "CMP #1", "DEX", "SBX #1", "CPY 2", "CMP 2", "DEC 2", "DCM 2",
	"BNE 0", "CMP (1),Y", "CIM", "DCM (1),Y", "NOP 1,X", "CMP 1,X", "DEC 1,X", "DCM 1,X",
	"CLD", "CMP 2,Y", "NOP !", "DCM 2,Y", "NOP 2,X", "CMP 2,X", "DEC 2,X", "DCM 2,X",

	"CPX #1", "SBC (1,X)", "NOP #1", "INS (1,X)", "CPX 1", "SBC 1", "INC 1", "INS 1",
	"INX", "SBC #1", "NOP", "SBC #1 !", "CPX 2", "SBC 2", "INC 2", "INS 2",
	"BEQ 0", "SBC (1),Y", "CIM", "INS (1),Y", "NOP 1,X", "SBC 1,X", "INC 1,X", "INS 1,X",
	"SED", "SBC 2,Y", "NOP !", "INS 2,Y", "NOP 2,X", "SBC 2,X", "INC 2,X", "INS 2,X"
};

static void show_instruction(const ASAP_State *as, int pc)
{
	int addr = pc;
	int insn;
	const char *mnemonic;
	const char *p;

	insn = dGetByte(pc++);
	mnemonic = cpu_instructions[insn];
	for (p = mnemonic + 3; *p != '\0'; p++) {
		if (*p == '1') {
			int value = dGetByte(pc);
			printf("%04X: %02X %02X     %.*s$%02X%s\n",
			       addr, insn, value, (int) (p - mnemonic), mnemonic, dGetByte(pc), p + 1);
			return;
		}
		if (*p == '2') {
			int lo = dGetByte(pc);
			int hi = dGetByte(pc + 1);
			printf("%04X: %02X %02X %02X  %.*s$%02X%02X%s\n",
			       addr, insn, lo, hi, (int) (p - mnemonic), mnemonic, hi, lo, p + 1);
			return;
		}
		if (*p == '0') {
			int offset = dGetByte(pc++);
			int target = (pc + (signed char) offset) & 0xffff;
			printf("%04X: %02X %02X     %.4s$%04X\n", addr, insn, offset, mnemonic, target);
			return;
		}
	}
	printf("%04X: %02X        %s\n", addr, insn, mnemonic);
}

void print_cpu_state(const ASAP_State *as, int pc, int a, int x, int y, int s, int nz, int vdi, int c)
{
	printf("%3d %3d A=%02X X=%02X Y=%02X S=%02X P=%c%c*-%c%c%c%c PC=",
		as->scanline_number, as->cycle + 114 - as->next_scanline_cycle, a, x, y, s,
		nz >= 0x80 ? 'N' : '-', (vdi & V_FLAG) != 0 ? 'V' : '-', (vdi & D_FLAG) != 0 ? 'D' : '-',
		(vdi & I_FLAG) != 0 ? 'I' : '-', (nz & 0xff) == 0 ? 'Z' : '-', c != 0 ? 'C' : '-');
	show_instruction(as, pc);
}

static void print_help(void)
{
	printf(
		"Usage: asapscan COMMAND INPUTFILE\n"
		"Commands:\n"
		"-d  Dump POKEY registers\n"
		"-f  List POKEY features used\n"
		"-t  Detect silence and loops\n"
		"-c  Dump 6502 trace\n"
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
					int duration = player_calls_to_milliseconds(i + 1 - silence_run);
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
	else if (strcmp(argv[1], "-c") == 0)
		cpu_trace = TRUE;
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
