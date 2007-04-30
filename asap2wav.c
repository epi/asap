/*
 * asap2wav.c - converter of ASAP-supported formats to WAV files
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
#include <stdarg.h>

#include "asap.h"

#ifndef TRUE
#define TRUE 1
#endif
#ifndef FALSE
#define FALSE 0
#endif

static int no_input_files = TRUE;

static void print_help(void)
{
	printf(
		"Usage: asap2wav [OPTIONS] INPUTFILE...\n"
		"Each INPUTFILE must be in a supported format:\n"
#ifdef STEREO_SOUND
		"SAP, CMC, CMR, DMC, MPT, MPD, RMT, TMC, TM8 or TM2.\n"
#else
		"SAP, CMC, CMR, DMC, MPT, MPD, RMT, TMC or TM2.\n"
#endif
		"Options:\n"
		"-o FILE     --output=FILE      Set output file name\n"
		"-o -        --output=-         Write to standard output\n"
		"-s SONG     --song=SONG        Select subsong number (zero-based)\n"
		"-t TIME     --time=TIME        Set output length MM:SS\n"
		"-r FREQ     --rate=FREQ        Set sample rate in Hz (default: 44100)\n"
		"-q QUALITY  --quality=QUALITY  Set sound quality 0-3 (default: 1)\n"
		"-b          --byte-samples     Output 8-bit samples\n"
		"-w          --word-samples     Output 16-bit samples (default)\n"
		"            --raw              Output raw audio (no WAV header)\n"
		"-h          --help             Display this information\n"
		"-v          --version          Display version information\n"
	);
	no_input_files = FALSE;
}

static void print_version(void)
{
	printf("ASAP2WAV " ASAP_VERSION "\n");
	no_input_files = FALSE;
}

static void fatal_error(const char *format, ...)
{
	va_list args;
	va_start(args, format);
	fprintf(stderr, "asap2wav: ");
	vfprintf(stderr, format, args);
	fputc('\n', stderr);
	va_end(args);
	exit(1);
}

static const char *output_file = NULL;
static int output_header = TRUE;
static int song = -1;
static int quality = 1;
static int frequency = 44100;
static int seconds = -1;
static int use_16bit = TRUE;

static void set_dec(const char *s, int *result, const char *name,
                    int minval, int maxval)
{
	int newval = 0;
	do {
		if (*s < '0' || *s > '9')
			fatal_error("%s must be an integer", name);
		newval = 10 * newval + *s++ - '0';
		if (newval > maxval)
			fatal_error("maximum %s is %d", name, maxval);
	} while (*s != '\0');
	if (newval < minval)
		fatal_error("minimum %s is %d", name, minval);
	*result = newval;
}

static void set_time(const char *s)
{
	seconds = ASAP_ParseDuration(s);
	if (seconds == 0)
		fatal_error("invalid time format");
}

/* write 16-bit word as little endian */
static void fput16(int x, FILE *fp)
{
	fputc(x & 0xff, fp);
	fputc((x >> 8) & 0xff, fp);
}

/* write 32-bit word as little endian */
static void fput32(int x, FILE *fp)
{
	fputc(x & 0xff, fp);
	fputc((x >> 8) & 0xff, fp);
	fputc((x >> 16) & 0xff, fp);
	fputc((x >> 24) & 0xff, fp);
}

static void process_file(const char *input_file)
{
	FILE *fp;
	static unsigned char module[ASAP_MODULE_MAX];
	int module_len;
	const ASAP_ModuleInfo *module_info;
	int n_bytes;
	static unsigned char buffer[8192];
	if (strlen(input_file) >= FILENAME_MAX)
		fatal_error("filename too long");
	fp = fopen(input_file, "rb");
	if (fp == NULL)
		fatal_error("cannot open %s", input_file);
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);
	ASAP_Initialize(frequency, use_16bit ? AUDIO_FORMAT_S16_LE : AUDIO_FORMAT_U8, quality);
	module_info = ASAP_Load(input_file, module, module_len);
	if (module_info == NULL)
		fatal_error("%s: format not supported", input_file);
	if (song < 0)
		song = module_info->default_song;
	if (song >= module_info->songs) {
		fatal_error("you have requested subsong %d ...\n"
			"... but %s contains only %d subsongs",
			song, input_file, module_info->songs);
	}
	if (seconds <= 0) {
		seconds = module_info->durations[song];
		if (seconds <= 0)
			seconds = 180;
	}
	ASAP_PlaySong(song, seconds);
	if (output_file == NULL) {
		const char *dot;
		static char output_default[FILENAME_MAX];
		/* we are sure to find a dot because ASAP_Load()
		   accepts only filenames with an extension */
		dot = strrchr(input_file, '.');
		sprintf(output_default, "%.*s.%s", (int) (dot - input_file), input_file,
			output_header ? "wav" : "raw");
		output_file = output_default;
	}
	if (output_file[0] == '-' && output_file[1] == '\0')
		fp = stdout;
	else {
		fp = fopen(output_file, "wb");
		if (fp == NULL)
			fatal_error("cannot write %s", output_file);
	}
	if (output_header) {
		int block_size = module_info->channels << use_16bit;
		int bytes_per_second = frequency * block_size;
		n_bytes = seconds * bytes_per_second;
		fwrite("RIFF", 1, 4, fp);
		fput32(n_bytes + 36, fp);
		fwrite("WAVEfmt \x10\0\0\0\1\0", 1, 14, fp);
		fput16(module_info->channels, fp);
		fput32(frequency, fp);
		fput32(bytes_per_second, fp);
		fput16(block_size, fp);
		fput16(8 << use_16bit, fp);
		fwrite("data", 1, 4, fp);
		fput32(n_bytes, fp);
	}
	do {
		n_bytes = ASAP_Generate(buffer, sizeof(buffer));
		if (fwrite(buffer, 1, n_bytes, fp) != n_bytes) {
			fclose(fp);
			fatal_error("error writing to %s", output_file);
		}
	} while (n_bytes == sizeof(buffer));
	if (!(output_file[0] == '-' && output_file[1] == '\0'))
		fclose(fp);
	output_file = NULL;
	song = -1;
	seconds = -1;
	no_input_files = FALSE;
}

int main(int argc, char *argv[])
{
	int i;
	for (i = 1; i < argc; i++) {
		const char *arg = argv[i];
		if (arg[0] != '-')
			process_file(arg);
		else if (arg[1] == 'o' && arg[2] == '\0')
			output_file = argv[++i];
		else if (strncmp(arg, "--output=", 9) == 0)
			output_file = arg + 9;
		else if (arg[1] == 's' && arg[2] == '\0')
			set_dec(argv[++i], &song, "subsong number", 0, 127);
		else if (strncmp(arg, "--song=", 7) == 0)
			set_dec(arg + 7, &song, "subsong number", 0, 127);
		else if (arg[1] == 't' && arg[2] == '\0')
			set_time(argv[++i]);
		else if (strncmp(arg, "--time=", 7) == 0)
			set_time(arg + 7);
		else if (arg[1] == 'r' && arg[2] == '\0')
			set_dec(argv[++i], &frequency, "sample rate", 4000, 65535);
		else if (strncmp(arg, "--rate=", 7) == 0)
			set_dec(arg + 7, &frequency, "sample rate", 4000, 65535);
		else if (arg[1] == 'q' && arg[2] == '\0')
			set_dec(argv[++i], &quality, "quality", 0, 3);
		else if (strncmp(arg, "--quality=", 10) == 0)
			set_dec(arg + 10, &quality, "quality", 0, 3);
		else if ((arg[1] == 'b' && arg[2] == '\0')
			|| strcmp(arg, "--byte-samples") == 0)
			use_16bit = FALSE;
		else if ((arg[1] == 'w' && arg[2] == '\0')
			|| strcmp(arg, "--word-samples") == 0)
			use_16bit = TRUE;
		else if (strcmp(arg, "--raw") == 0)
			output_header = FALSE;
		else if ((arg[1] == 'h' && arg[2] == '\0')
			|| strcmp(arg, "--help") == 0)
			print_help();
		else if ((arg[1] == 'v' && arg[2] == '\0')
			|| strcmp(arg, "--version") == 0)
			print_version();
		else
			fatal_error("unknown option: %s", arg);
	}
	if (no_input_files) {
		print_help();
		return 1;
	}
	return 0;
}
