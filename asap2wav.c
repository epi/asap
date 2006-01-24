/*
 * asap2wav.c - converter of ASAP-supported formats to WAV files
 *
 * Copyright (C) 2005-2006  Piotr Fusik
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
static unsigned int song = 1000; /* default */
static unsigned int quality = 1;
static unsigned int frequency = 44100;
static unsigned int seconds = 180;
static unsigned int use_16bit = 1;

static int set_output(const char *s)
{
	output_file = s;
	return 0;
}

static int set_dec(const char *s, unsigned int *result, const char *name,
                   unsigned int minval, unsigned int maxval)
{
	unsigned int newval = 0;
	while (*s != '\0') {
		if (*s < '0' || *s > '9') {
			print_error("%s must be an integer", name);
			return 1;
		}
		newval = 10 * newval + *s++ - '0';
		if (newval > maxval) {
			print_error("maximum %s is %u", name, maxval);
			return 1;
		}
	}
	if (newval < minval) {
		print_error("minimum %s is %u", name, minval);
		return 1;
	}
	*result = newval;
	return 0;
}

static int set_song(const char *s)
{
	return set_dec(s, &song, "subsong number", 0, 255);
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
	unsigned int newmin;
	const char *p;
	if (s[0] < '0' || s[0] > '9') {
		print_error("invalid time format");
		return 1;
	}
	newmin = s[0] - '0';
	p = s + 1;
	if (*p >= '0' && *p <= '9')
		newmin = 10 * newmin + *p++ - '0';
	if (*p == ':') {
		unsigned int newsec;
		if (newmin > 59) {
			print_error("maximum time is 59:59");
			return 1;
		}
		if (set_dec(p + 1, &newsec, "SS", newmin == 0 ? 1 : 0, 59))
			return 1;
		seconds = 60 * newmin + newsec;
		return 0;
	}
	return set_dec(s, &seconds, "time", 1, 3599);
}

/* write 16-bit word as little endian */
static void fput16(unsigned int x, FILE *fp)
{
	fputc(x & 0xff, fp);
	fputc((x >> 8) & 0xff, fp);
}

/* write 32-bit word as little endian */
static void fput32(unsigned int x, FILE *fp)
{
	fputc(x & 0xff, fp);
	fputc((x >> 8) & 0xff, fp);
	fputc((x >> 16) & 0xff, fp);
	fputc((x >> 24) & 0xff, fp);
}

int main(int argc, char *argv[])
{
	static const struct {
		const char name[9];
		int (*func)(const char *s);
	} param_opts[] = {
		{ "output=", set_output },
		{ "song=", set_song },
		{ "quality=", set_quality },
		{ "time=", set_time },
		{ "rate=", set_frequency }
	};
	int i;
	int files_processed = 0;
	for (i = 1; i < argc; i++) {
		const char *arg = argv[i];
		if (arg[0] == '-') {
			int j;
			for (j = 0; j < sizeof(param_opts) / sizeof(param_opts[0]); j++) {
				if (arg[1] == param_opts[j].name[0] && arg[2] == '\0') {
					if (++i >= argc) {
						print_error("missing argument for '-%c'", arg[1]);
						return 1;
					}
					if (param_opts[j].func(argv[i]))
						return 1;
					break;
				}
				if (arg[1] == '-') {
					size_t len = strlen(param_opts[j].name);
					if (strncmp(arg + 2, param_opts[j].name, len) == 0) {
						if (param_opts[j].func(arg + 2 + len))
							return 1;
						break;
					}
				}
			}
			if (j < sizeof(param_opts) / sizeof(param_opts[0]))
				continue;
			if (strcmp(arg, "-b") == 0 || strcmp(arg, "--byte-samples") == 0) {
				use_16bit = 0;
				continue;
			}
			if (strcmp(arg, "-w") == 0 || strcmp(arg, "--word-samples") == 0) {
				use_16bit = 1;
				continue;
			}
			if (strcmp(arg, "-h") == 0 || strcmp(arg, "--help") == 0) {
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
					"-t TIME     --time=TIME        Set output length MM:SS (default: 03:00)\n"
					"-r FREQ     --rate=FREQ        Set sample rate in Hz (default: 44100)\n"
					"-q QUALITY  --quality=QUALITY  Set sound quality 0-3 (default: 1)\n"
					"-b          --byte-samples     Output 8-bit samples\n"
					"-w          --word-samples     Output 16-bit samples (default)\n"
					"-h          --help             Display this information and exit\n"
					"-v          --version          Display version information and exit\n"
				);
				return 0;
			}
			if (strcmp(arg, "-v") == 0 || strcmp(arg, "--version") == 0) {
				printf("ASAP2WAV " ASAP_VERSION "\n");
				return 0;
			}
			print_error("unknown option: %s", arg);
			return 1;
		}
		else {
			FILE *fp;
			static unsigned char module[65000];
			unsigned int module_len;
			unsigned int channels;
			unsigned int block_size;
			unsigned int bytes_per_second;
			unsigned int n_bytes;
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
			if (!ASAP_Load(arg, module, module_len)) {
				print_error("%s: format not supported", arg);
				return 1;
			}
			if (song > 255)
				ASAP_PlaySong(ASAP_GetDefSong());
			else if (song < ASAP_GetSongs()) {
				ASAP_PlaySong(song);
				/* back to default */
				song = 1000;
			}
			else {
				print_error("you have requested subsong %u ...", song);
				print_error("... but %s contains only %u subsongs", arg, ASAP_GetSongs());
				return 1;
			}
			if (output_file == NULL) {
				const char *dot;
				static char output_default[FILENAME_MAX + 5]; /* max. original name + ".wav" + '\0' */
				dot = strrchr(arg, '.');
				sprintf(output_default, "%.*s.wav", (int) (dot - arg), arg);
				output_file = output_default;
			}
			fp = fopen(output_file, "wb");
			if (fp == NULL) {
				print_error("cannot write %s", output_file);
				return 1;
			}
			channels = ASAP_GetChannels();
			block_size = channels << use_16bit;
			bytes_per_second = frequency * block_size;
			n_bytes = seconds * bytes_per_second;
			fwrite("RIFF", 1, 4, fp);
			fput32(n_bytes + 36, fp);
			fwrite("WAVEfmt \x10\0\0\0\1\0", 1, 14, fp);
			fput16(channels, fp);
			fput32(frequency, fp);
			fput32(bytes_per_second, fp);
			fput16(block_size, fp);
			fput16(8 << use_16bit, fp);
			fwrite("data", 1, 4, fp);
			fput32(n_bytes, fp);
			while (n_bytes > sizeof(buffer)) {
				ASAP_Generate(buffer, sizeof(buffer));
				if (fwrite(buffer, 1, sizeof(buffer), fp) != sizeof(buffer)) {
					fclose(fp);
					print_error("error writing to %s", output_file);
					return 1;
				}
				n_bytes -= sizeof(buffer);
			}
			ASAP_Generate(buffer, n_bytes);
			fwrite(buffer, 1, n_bytes, fp);
			fclose(fp);
			output_file = NULL;
			files_processed++;
		}
	}
	if (files_processed == 0) {
		print_error("no input files; try: asap2wav --help");
		return 1;
	}
	return 0;
}
