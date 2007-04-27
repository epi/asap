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
		"-o FILE     --output=FILE      Set output WAV file name\n"
		"-s SONG     --song=SONG        Select subsong number (zero-based)\n"
		"-t TIME     --time=TIME        Set output length MM:SS\n"
		"-r FREQ     --rate=FREQ        Set sample rate in Hz (default: 44100)\n"
		"-q QUALITY  --quality=QUALITY  Set sound quality 0-3 (default: 1)\n"
		"-b          --byte-samples     Output 8-bit samples\n"
		"-w          --word-samples     Output 16-bit samples (default)\n"
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

static void print_error(const char *format, ...)
{
	va_list args;
	va_start(args, format);
	fprintf(stderr, "asap2wav: ");
	vfprintf(stderr, format, args);
	fputc('\n', stderr);
	va_end(args);
}

static const char *output_file = NULL;
static int song = -1;
static int quality = 1;
static int frequency = 44100;
static int seconds = -1;
static int use_16bit = 1;

static int set_output(const char *s)
{
	output_file = s;
	return 0;
}

static int set_dec(const char *s, int *result, const char *name,
                   int minval, int maxval)
{
	int newval = 0;
	do {
		if (*s < '0' || *s > '9') {
			print_error("%s must be an integer", name);
			return 1;
		}
		newval = 10 * newval + *s++ - '0';
		if (newval > maxval) {
			print_error("maximum %s is %d", name, maxval);
			return 1;
		}
	} while (*s != '\0');
	if (newval < minval) {
		print_error("minimum %s is %d", name, minval);
		return 1;
	}
	*result = newval;
	return 0;
}

static int set_song(const char *s)
{
	return set_dec(s, &song, "subsong number", 0, 127);
}

static int set_quality(const char *s)
{
	return set_dec(s, &quality, "quality", 0, 3);
}

static int set_frequency(const char *s)
{
	return set_dec(s, &frequency, "sample rate", 4000, 65535);
}

static int set_time(const char *s)
{
	seconds = ASAP_ParseDuration(s);
	if (seconds == 0) {
		print_error("invalid time format");
		return 1;
	}
	return 0;
}

static void set_byte_samples(void)
{
	use_16bit = 0;
}

static void set_word_samples(void)
{
	use_16bit = 1;
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

typedef struct {
	const char *name;
	void (*void_func)(void);
	int (*arg_func)(const char *s);
} command_line_option;

int main(int argc, char *argv[])
{
	int i;
	for (i = 1; i < argc; i++) {
		const char *arg = argv[i];
		if (arg[0] == '-') {
			static const command_line_option opts[] = {
				{ "output=", NULL, set_output },
				{ "song=", NULL, set_song },
				{ "time=", NULL, set_time },
				{ "rate=", NULL, set_frequency },
				{ "quality=", NULL, set_quality },
				{ "byte-samples", set_byte_samples, NULL },
				{ "word-samples", set_word_samples, NULL },
				{ "help", print_help, NULL },
				{ "version", print_version, NULL }
			};
			const command_line_option *p = opts;
			for (;;) {
				if (arg[1] == p->name[0] && arg[2] == '\0') {
					if (p->void_func != NULL)
						p->void_func();
					else {
						if (++i >= argc) {
							print_error("missing argument for '-%c'", arg[1]);
							return 1;
						}
						if (p->arg_func(argv[i]))
							return 1;
					}
					break;
				}
				if (arg[1] == '-') {
					if (p->void_func != NULL) {
						if (strcmp(arg + 2, p->name) == 0) {
							p->void_func();
							break;
						}
					}
					else {
						size_t len = strlen(p->name);
						if (strncmp(arg + 2, p->name, len) == 0) {
							if (p->arg_func(arg + 2 + len))
								return 1;
							break;
						}
					}
				}
				if (++p >= opts + sizeof(opts)) {
					print_error("unknown option: %s", arg);
					return 1;
				}
			}
		}
		else {
			FILE *fp;
			static unsigned char module[ASAP_MODULE_MAX];
			int module_len;
			const ASAP_ModuleInfo *module_info;
			int block_size;
			int bytes_per_second;
			int n_bytes;
			static unsigned char buffer[8192];
			if (strlen(arg) >= FILENAME_MAX) {
				print_error("filename too long");
				return 1;
			}
			fp = fopen(arg, "rb");
			if (fp == NULL) {
				print_error("cannot open %s", arg);
				return 1;
			}
			module_len = fread(module, 1, sizeof(module), fp);
			fclose(fp);
			ASAP_Initialize(frequency, use_16bit ? AUDIO_FORMAT_S16_LE : AUDIO_FORMAT_U8, quality);
			module_info = ASAP_Load(arg, module, module_len);
			if (module_info == NULL) {
				print_error("%s: format not supported", arg);
				return 1;
			}
			if (song < 0)
				song = module_info->default_song;
			if (song >= module_info->songs) {
				print_error("you have requested subsong %d ...", song);
				print_error("... but %s contains only %d subsongs", arg, module_info->songs);
				return 1;
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
				dot = strrchr(arg, '.');
				sprintf(output_default, "%.*s.wav", (int) (dot - arg), arg);
				output_file = output_default;
			}
			fp = fopen(output_file, "wb");
			if (fp == NULL) {
				print_error("cannot write %s", output_file);
				return 1;
			}
			block_size = module_info->channels << use_16bit;
			bytes_per_second = frequency * block_size;
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
			do {
				n_bytes = ASAP_Generate(buffer, sizeof(buffer));
				if (fwrite(buffer, 1, n_bytes, fp) != n_bytes) {
					fclose(fp);
					print_error("error writing to %s", output_file);
					return 1;
				}
			} while (n_bytes == sizeof(buffer));
			fclose(fp);
			output_file = NULL;
			song = -1;
			seconds = -1;
			no_input_files = FALSE;
		}
	}
	if (no_input_files) {
		print_help();
		return 1;
	}
	return 0;
}
