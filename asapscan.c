/*
 * asapscan.c - Atari 8-bit music analyzer
 *
 * Copyright (C) 2007-2019  Piotr Fusik
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
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifdef _WIN32
#include <windows.h>
#endif

#include "asap-asapscan.h"

static bool detect_time = false;
static int scan_frames;
static int silence_frames;
static int loop_check_frames;
static int loop_min_frames;
static int frame;
static unsigned char *registers_dump;
#define HASH_BITS  8
static int hash_first[1 << HASH_BITS];
static int *hash_next;
static int hash_last[1 << HASH_BITS];

static ASAP *asap;
static bool dump = false;
static bool fingerprint = false;
static bool long_fingerprint = false;

#define FEATURE_CHECK          1
#define FEATURE_15_KHZ         2
#define FEATURE_HIPASS_FILTER  4
#define FEATURE_LOW_OF_16_BIT  8
#define FEATURE_9_BIT_POLY     16
#define FEATURE_ULTRASOUND     32
static int features = 0;

#define CPU_TRACE_PRINT        1
#define CPU_TRACE_UNOFFICIAL   2
#define CPU_TRACE_PC_TIME      4
static int cpu_trace = 0;
static void trace_cpu(void);

#define MEM_TRACE_FILE         1
#define MEM_TRACE_STORE        2
#define MEM_TRACE_LOAD         4
#define MEM_TRACE_LOAD_UNINIT  8
#define MEM_TRACE_PUSH         16
#define MEM_TRACE_PULL         32
#define MEM_TRACE_ALL          63
#define MEM_TRACE_HW           128

static int mem_trace = 0;
static void trace_mem(int op, unsigned short addr, unsigned char data);
static void trace_mem_range(int op, unsigned short addr, unsigned short length);
static unsigned char mem_access[65537];

static int print_time_at_pc = -1;

static bool acid = false;
static int exit_code = 0;

#include "asap-asapscan.c"

#define CYCLES_PER_FRAME (asap->moduleInfo.ntsc ? 262 * 114 : 312 * 114)
#define MAIN_CLOCK (asap->moduleInfo.ntsc ? 1789772 : 1773447)

static int seconds_to_frames(int seconds)
{
	return (int) ((double) seconds * MAIN_CLOCK / CYCLES_PER_FRAME);
}

static int frames_to_milliseconds(int frames)
{
	return (int) ceil(frames * 1000.0 * CYCLES_PER_FRAME / MAIN_CLOCK);
}

static void print_time(int frames, bool loop)
{
	int duration = frames_to_milliseconds(frames);
	printf("TIME %02d:%02d.%02d%s\n", duration / 60000, duration / 1000 % 60, duration / 10 % 100, loop ? " LOOP" : "");
}

static const char cpu_mnemonics[256][10] = {
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

#define CPU_OPCODE_UNOFFICIAL  1
#define CPU_OPCODE_USED        2
static char cpu_opcodes[256] = {
	1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1,
	0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1,
	1, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 0, 1, 1,
	0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1,
	0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1,
	0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1
};

static void show_instruction(int pc)
{
	int addr = pc;
	int opcode = asap->cpu.memory[pc++];
	const char *mnemonic = cpu_mnemonics[opcode];
	for (const char *p = mnemonic + 3; *p != '\0'; p++) {
		if (*p == '1') {
			int value = asap->cpu.memory[pc];
			printf("%04X: %02X %02X     %.*s$%02X%s\n",
			       addr, opcode, value, (int) (p - mnemonic), mnemonic, value, p + 1);
			return;
		}
		if (*p == '2') {
			int lo = asap->cpu.memory[pc];
			int hi = asap->cpu.memory[pc + 1];
			printf("%04X: %02X %02X %02X  %.*s$%02X%02X%s\n",
			       addr, opcode, lo, hi, (int) (p - mnemonic), mnemonic, hi, lo, p + 1);
			return;
		}
		if (*p == '0') {
			int offset = asap->cpu.memory[pc++];
			int target = (pc + (signed char) offset) & 0xffff;
			printf("%04X: %02X %02X     %.4s$%04X\n", addr, opcode, offset, mnemonic, target);
			return;
		}
	}
	printf("%04X: %02X        %s\n", addr, opcode, mnemonic);
}

static void trace_cpu(void)
{
	int pc = asap->cpu.pc;
	if ((cpu_trace & CPU_TRACE_PRINT) != 0) {
		int nz = asap->cpu.nz;
		int vdi = asap->cpu.vdi;
		printf("%3d %3d A=%02X X=%02X Y=%02X S=%02X P=%c%c*-%c%c%c%c PC=",
			asap->cpu.cycle / 114, asap->cpu.cycle % 114,
			asap->cpu.a, asap->cpu.x, asap->cpu.y, asap->cpu.s,
			nz >= 0x80 ? 'N' : '-', (vdi & 0x40) != 0 ? 'V' : '-', (vdi & 8) != 0 ? 'D' : '-',
			(vdi & 4) != 0 ? 'I' : '-', (nz & 0xff) == 0 ? 'Z' : '-', asap->cpu.c != 0 ? 'C' : '-');
		show_instruction(pc);
	}
	if (pc == print_time_at_pc)
		print_time(frame, true);
	if (pc != 0xd200 && pc != 0xd203) /* don't count 0xd2 used by Do6502Init() and Call6502() */
		cpu_opcodes[asap->cpu.memory[pc]] |= CPU_OPCODE_USED;
}

static void print_unofficial_mnemonic(int opcode)
{
	const char *mnemonic = cpu_mnemonics[opcode];
	for (const char *p = mnemonic + 3; *p != '\0'; p++) {
		if (*p == '1') {
			printf("%02X: %.*s$xx%s\n", opcode, (int) (p - mnemonic), mnemonic, p + 1);
			return;
		}
		if (*p == '2') {
			printf("%02X: %.*s$xxxx%s\n", opcode, (int) (p - mnemonic), mnemonic, p + 1);
			return;
		}
		/* there are no undocumented branches ('0') */
	}
	printf("%02X: %s\n", opcode, mnemonic);
}

static void trace_mem(int op, unsigned short addr, unsigned char data)
{
	if ((addr & 0xf800) != 0xd000) {
		if (op & MEM_TRACE_LOAD) {
			if ((mem_access[addr] & (MEM_TRACE_FILE | MEM_TRACE_STORE | MEM_TRACE_PUSH)) == 0)
				mem_access[addr] |= MEM_TRACE_LOAD_UNINIT;
			else
				mem_access[addr] |= MEM_TRACE_LOAD;
		}
		mem_access[addr] |= op & ~(MEM_TRACE_LOAD | MEM_TRACE_LOAD_UNINIT);
	}
	// fputc(mem_access[addr] + 32, stderr);//printf("%d %d %d\n", op, addr, data);
}

static void trace_mem_range(int op, unsigned short addr, unsigned short length)
{
	while (length--)
		mem_access[addr++] |= op;
}

static void print_mem_ranges(void)
{
	static const char *const labels[] = {
		"file data", "store", "load", "load uninitialized", "push", "pull"
	};
	for (int mask = 0; mask <= MEM_TRACE_ALL; mask++) {
		bool printed = false;
		int start = -1;
		for (int addr = 0; addr < 65536; addr++) {
			if (mem_access[addr] == mask) {
				if (start < 0)
					start = addr;
				if (mem_access[addr + 1] != mask) {
					if (!printed) {
						if (mask == 0) {
							printf("Unused memory regions:\n");
							printed = true;
						}
						else {
							printf("Memory regions used for: ");
							for (int b = 0; b < 6; b++) {
								if (mask & (1 << b)) {
									if (printed)
										printf(", ");
									printf("%s", labels[b]);
									printed = true;
								}
							}
							printf("\n");
						}
					}
					if (addr == start)
						printf("%04x\n", addr);
					else
						printf("%04x-%04x\n", start, addr);
					start = -1;
				}
			}
		}
	}
}

static void print_help(void)
{
	printf(
		"Usage: asapscan COMMAND [OPTIONS] INPUTFILE\n"
		"Commands:\n"
		"-d          Dump POKEY registers\n"
		"-f          List POKEY features used\n"
		"-t          Detect silence and loops\n"
		"-p          Calculate fingerprint\n"
		"-l          Calculate hash representation (fingerprint is a substring of this)\n"
		"-c          Dump 6502 trace\n"
		"-u          List used unofficial 6502 instructions and BRK\n"
		"-m          List used memory regions by access type\n"
		"-b HEXADDR  Print time the given instruction reached\n"
		"-a          Run Acid800 test\n"
		"-v          Display version information\n"
		"Options:\n"
		"-s SONG     Process the specified subsong (zero-based)\n"
	);
}

static bool store_pokey(unsigned char *p, Pokey *pokey)
{
	bool is_silence = true;
	for (int i = 0; i < 4; i++) {
		if ((pokey->channels[i].audc & 0xf) != 0) {
			is_silence = false;
			p[i * 2] = pokey->channels[i].audf;
			p[i * 2 + 1] = pokey->channels[i].audc;
		}
		else {
			p[i * 2] = 0;
			p[i * 2 + 1] = 0;
		}
	}
	p[8] = pokey->audctl;
	return is_silence;
}

static bool store_pokeys(int frame)
{
	unsigned char *p = registers_dump + 18 * frame;
	bool is_silence = store_pokey(p, &asap->pokeys.basePokey);
	is_silence &= store_pokey(p + 9, &asap->pokeys.extraPokey);
	return is_silence;
}

static bool has_loop_at(int first_frame, int second_frame)
{
	return memcmp(registers_dump + 18 * first_frame, registers_dump + 18 * second_frame, 18 * loop_check_frames) == 0;
}

static int get_hash(int player_call)
{
	for (int i = 1; i < 9; i += 2) {
		if ((registers_dump[18 * player_call + i] & 0xe0) == 0xe0)
			registers_dump[18 * player_call + i] &= 0xbf;
		if ((registers_dump[18 * player_call + i + 9] & 0xe0) == 0xe0)
			registers_dump[18 * player_call + i + 9] &= 0xbf;
	}
	int hash = 0;
	for (int i = 0; i < 18; i++)
		hash += registers_dump[18 * player_call + i];
	return hash;
}

static int get_byte_hash(int frame)
{
	int res = get_hash(frame);
	res = (res & 0xff) + (res >> 8);
	res = (res & 0xff) + (res >> 8);
	return res;
}

static void compute_entrophy(int frames)
{
	if (long_fingerprint) {
		for (int i = 0; i < frame; i++)
			printf("%02x", get_byte_hash(i));
		printf("\n");
	}
	else {
		int entrophy_counters[256] = { 0 };
		int emaxvalue = 0;
		int emaxframe = 0;
		int evalue = 0;

		for (int i = 0; i < frames; i++) {
			int v = get_byte_hash(i);
			if (entrophy_counters[v] == 0)
				evalue++;
			entrophy_counters[v]++;

#define ENTROPHY_LEN 32
			if (i >= ENTROPHY_LEN) {
				v = get_byte_hash(i - ENTROPHY_LEN);
				entrophy_counters[v]--;
				if (entrophy_counters[v] == 0)
					evalue--;

				if (emaxvalue < evalue) {
					emaxvalue = evalue;
					emaxframe = i - ENTROPHY_LEN;
				}
			}
		}

		for (int i = 0; i < ENTROPHY_LEN; i++)
			printf("%02x", get_byte_hash(i + emaxframe));
		printf("\n");
	}
}

static void print_pokey(const Pokey *pokey)
{
	for (int i = 0; i < 4; i++)
		printf("%02X %02X  ", pokey->channels[i].audf, pokey->channels[i].audc);
	printf("%02X", pokey->audctl);
}

static bool is_ultrasound(const PokeyChannel *ch)
{
	if (ch->periodCycles > 112)
		return false;
	if ((ch->audc & 0xf) == 0)
		return false;
	return (ch->audc & 0xb0) == 0xa0;
}

static void scan_song(int song)
{
	if (!ASAP_PlaySong(asap, song, -1)) {
		fprintf(stderr, "asapscan: PlaySong failed\n");
		return;
	}
	int silence_run = 0;
	int running_hash = 0;
	if (acid)
		scan_frames = seconds_to_frames(ASAPInfo_GetDuration(&asap->moduleInfo, song) / 1000);
	for (int i = 0; i < 1 << HASH_BITS; i++)
		hash_first[i] = -1;
	for (frame = 0; frame < scan_frames; frame++) {
		ASAP_Do6502Frame(asap);
		if (dump) {
			printf("%6.2f: ", (double) frame * CYCLES_PER_FRAME / MAIN_CLOCK);
			print_pokey(&asap->pokeys.basePokey);
			if (asap->moduleInfo.channels == 2) {
				printf("  |  ");
				print_pokey(&asap->pokeys.extraPokey);
			}
			printf("\n");
		}
		if (features != 0) {
			int c1 = asap->pokeys.basePokey.audctl;
			int c2 = asap->pokeys.extraPokey.audctl;
			if (((c1 | c2) & 1) != 0)
				features |= FEATURE_15_KHZ;
			if (((c1 | c2) & 6) != 0)
				features |= FEATURE_HIPASS_FILTER;
			if (((c1 & 0x40) != 0 && (asap->pokeys.basePokey.channels[0].audc & 0xf) != 0)
			|| ((c1 & 0x20) != 0 && (asap->pokeys.basePokey.channels[2].audc & 0xf) != 0))
				features |= FEATURE_LOW_OF_16_BIT;
			if (((c1 | c2) & 0x80) != 0)
				features |= FEATURE_9_BIT_POLY;
			for (int i = 0; i < 4; i++) {
				if (is_ultrasound(&asap->pokeys.basePokey.channels[i])) {
					features |= FEATURE_ULTRASOUND;
					break;
				}
			}
		}
		if (detect_time) {
			if (store_pokeys(frame)) {
				silence_run++;
				if (silence_run >= silence_frames && /* do not trigger at the initial silence */ silence_run < frame) {
					if (fingerprint)
						compute_entrophy(frame + 1 - silence_run);
					else
						print_time(frame + 1 - silence_run, false);
					return;
				}
			}
			else
				silence_run = 0;
			if (frame >= loop_check_frames) {
				int second_frame = frame - loop_check_frames;
				running_hash &= (1 << HASH_BITS) - 1;
				/* Now running_hash is for the last loop_check_player_calls player calls before player_call. */
				for (int first_frame = hash_first[running_hash]; first_frame >= 0; first_frame = hash_next[first_frame]) {
					if (has_loop_at(first_frame, second_frame)) {
						int loop_len = second_frame - first_frame;
						if (loop_len >= loop_min_frames) {
							if (fingerprint)
								compute_entrophy(second_frame);
							else
								print_time(second_frame, true);
							return;
						}
						if (loop_len == 1) {
							/* POKEY registers do not change - probably an ultrasound */
							if (fingerprint)
								compute_entrophy(first_frame);
							else
								print_time(first_frame, false);
							return;
						}
					}
				}
				/* Insert into hashtable. */
				if (hash_first[running_hash] >= 0)
					hash_next[hash_last[running_hash]] = second_frame;
				else
					hash_first[running_hash] = second_frame;
				hash_next[second_frame] = -1;
				hash_last[running_hash] = second_frame;
				/* Update running_hash. */
				running_hash -= get_hash(second_frame);
			}
			running_hash += get_hash(frame);
		}
	}
	if (detect_time) {
		if (fingerprint)
			compute_entrophy(loop_check_frames);
		else
			printf("No silence or loop detected in song %d\n", song);
	}
	if (acid) {
#ifdef _WIN32
		HANDLE so = GetStdHandle(STD_OUTPUT_HANDLE);
		CONSOLE_SCREEN_BUFFER_INFO csbi;
		GetConsoleScreenBufferInfo(so, &csbi);
#define set_color(x) SetConsoleTextAttribute(so, x)
#else
#define set_color(x)
#endif
		int i;
		for (i = 0x1000; i <= 0x17ff; i++) {
			unsigned char c = asap->cpu.memory[i];
			if (c == 0)
				break;
			if (memcmp(asap->cpu.memory + i, "Pass", 4) == 0)
				set_color((csbi.wAttributes & ~0xf) | 10);
			else if (memcmp(asap->cpu.memory + i, "FAIL", 4) == 0) {
				exit_code = 1;
				set_color((csbi.wAttributes & ~0xf) | 12);
			}
			else if (memcmp(asap->cpu.memory + i, "Skipped", 7) == 0) {
				exit_code = 1;
				set_color((csbi.wAttributes & ~0xf) | 14);
			}
			if (c == 0x9b)
				c = '\n';
			putchar(c);
		}
		if (asap->cpu.memory[i - 1] != 0x9b) {
			set_color((csbi.wAttributes & ~0xf) | 13);
			printf("NO RESPONSE\n");
			exit_code = 1;
		}
		set_color(csbi.wAttributes);
	}
}

int main(int argc, char **argv)
{
	const char *input_file = NULL;
	int song = -1;
	int scan_seconds = 15 * 60;
	int silence_seconds = 5;
	int loop_check_seconds = 3 * 60;
	int loop_min_seconds = 5;

	for (int i = 1; i < argc; i++) {
		if (strcmp(argv[i], "-d") == 0)
			dump = true;
		else if (strcmp(argv[i], "-p") == 0)
			detect_time = fingerprint = true;
		else if (strcmp(argv[i], "-l") == 0)
			detect_time = fingerprint = long_fingerprint = true;
		else if (strcmp(argv[i], "-f") == 0)
			features = FEATURE_CHECK;
		else if (strcmp(argv[i], "-t") == 0)
			detect_time = true;
		else if (strcmp(argv[i], "-c") == 0)
			cpu_trace |= CPU_TRACE_PRINT;
		else if (strcmp(argv[i], "-u") == 0)
			cpu_trace |= CPU_TRACE_UNOFFICIAL;
		else if (strcmp(argv[i], "-b") == 0) {
			print_time_at_pc = (int) strtol(argv[++i], NULL, 16);
			cpu_trace |= CPU_TRACE_PC_TIME;
		}
		else if (strcmp(argv[i], "-m") == 0)
			mem_trace = MEM_TRACE_ALL;
		else if (strcmp(argv[i], "-s") == 0)
			song = atoi(argv[++i]);
		else if (strcmp(argv[i], "-a") == 0)
			acid = true;
		else if (strcmp(argv[i], "-v") == 0) {
			printf("asapscan " ASAPInfo_VERSION "\n");
			return 0;
		}
		else {
			if (input_file != NULL) {
				print_help();
				return 1;
			}
			input_file = argv[i];
		}
	}
	if (dump + features + detect_time + cpu_trace + acid + mem_trace == 0 || input_file == NULL) {
		print_help();
		return 1;
	}
	FILE *fp = fopen(input_file, "rb");
	if (fp == NULL) {
		fprintf(stderr, "asapscan: cannot open %s\n", input_file);
		return 1;
	}
	static unsigned char module[ASAPInfo_MAX_MODULE_LENGTH];
	int module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);
	asap = ASAP_New();
	if (!ASAP_Load(asap, input_file, module, module_len)) {
		fprintf(stderr, "asapscan: %s: format not supported\n", input_file);
		return 1;
	}
	scan_frames = seconds_to_frames(scan_seconds);
	silence_frames = seconds_to_frames(silence_seconds);
	loop_check_frames = seconds_to_frames(loop_check_seconds);
	loop_min_frames = seconds_to_frames(loop_min_seconds);
	registers_dump = malloc(scan_frames * 18);
	hash_next = malloc(scan_frames * sizeof(hash_next[0]));
	if (registers_dump == NULL || hash_next == NULL) {
		fprintf(stderr, "asapscan: out of memory\n");
		return 1;
	}
	if (song >= 0)
		scan_song(song);
	else
		for (song = 0; song < asap->moduleInfo.songs; song++)
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
		if ((features & FEATURE_ULTRASOUND) != 0)
			printf("Ultrasound\n");
	}
	if ((cpu_trace & CPU_TRACE_UNOFFICIAL) != 0) {
		for (int i = 0; i < 256; i++) {
			if (cpu_opcodes[i] == (CPU_OPCODE_UNOFFICIAL | CPU_OPCODE_USED))
				print_unofficial_mnemonic(i);
		}
	}
	if (mem_trace) {
		mem_access[65536] = MEM_TRACE_HW;
		trace_mem_range(MEM_TRACE_HW, 0xd000, 0x800);
		print_mem_ranges();
	}
	return exit_code;
}
