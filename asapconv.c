/*
 * asapconv.c - converter of ASAP-supported formats
 *
 * Copyright (C) 2005-2011  Piotr Fusik
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

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#ifdef __WIN32
#include <fcntl.h>
#endif

#ifdef HAVE_LIBMP3LAME
#define SAMPLE_FORMATS "WAV, RAW or MP3"
#ifndef HAVE_LIBMP3LAME_DLL
#include <lame.h>
#endif
#else
#define SAMPLE_FORMATS "WAV or RAW"
#endif

#include "asapci.h"

static const char *output_arg = NULL;
static int song = -1;
static ASAPSampleFormat sample_format = ASAPSampleFormat_S16_L_E;
static int duration = -1;
static int mute_mask = 0;
static const char *tag_author = NULL;
static const char *tag_name = NULL;
static const char *tag_date = NULL;
#ifdef HAVE_LIBMP3LAME
static cibool tag = FALSE;
#endif
static char output_file[FILENAME_MAX];

static void print_help(void)
{
	printf(
		"Usage: asapconv [OPTIONS] INPUTFILE...\n"
		"Each INPUTFILE must be in a supported format:\n"
		"SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8 or TM2.\n"
		"Output EXT must be one of the above or " SAMPLE_FORMATS ".\n"
		"In FILE and DIR you can use the following placeholders:\n"
		"%%a (author), %%n (music name) and %%d (music creation date).\n"
		"Options:\n"
		"-o FILE.EXT --output=FILE.EXT  Write to the specified file\n"
		"-o .EXT     --output=.EXT      Use input file path and name\n"
		"-o DIR/.EXT --output=DIR/.EXT  Write to the specified directory\n"
		"-o -.EXT    --output=-.EXT     Write to standard output\n"
		"-a \"TEXT\"   --author=\"TEXT\"    Set author name\n"
		"-n \"TEXT\"   --name=\"TEXT\"      Set music name\n"
		"-d \"TEXT\"   --date=\"TEXT\"      Set music creation date (DD/MM/YYYY format)\n"
		"-h          --help             Display this information\n"
		"-v          --version          Display version information\n"
		"Options for " SAMPLE_FORMATS " output:\n"
		"-s SONG     --song=SONG        Select subsong number (zero-based)\n"
		"-t TIME     --time=TIME        Set output length (MM:SS format)\n"
		"-m CHANNELS --mute=CHANNELS    Mute POKEY channels (1-8)\n"
#ifdef HAVE_LIBMP3LAME
		"Options for WAV or RAW output:\n"
#endif
		"-b          --byte-samples     Output 8-bit samples\n"
		"-w          --word-samples     Output 16-bit samples (default)\n"
#ifdef HAVE_LIBMP3LAME
		"Options for MP3 output:\n"
		"            --tag              Output ID3 tag\n"
#endif
		"Options for SAP output:\n"
		"-s SONG     --song=SONG        Select subsong to set length of\n"
		"-t TIME     --time=TIME        Set subsong length (MM:SS format)\n"
	);
}

static void fatal_error(const char *format, ...)
{
	va_list args;
	va_start(args, format);
	fprintf(stderr, "asapconv: ");
	vfprintf(stderr, format, args);
	fputc('\n', stderr);
	va_end(args);
	exit(1);
}

static void set_song(const char *s)
{
	song = 0;
	do {
		if (*s < '0' || *s > '9')
			fatal_error("subsong number must be an integer");
		song = 10 * song + *s++ - '0';
		if (song >= ASAPInfo_MAX_SONGS)
			fatal_error("maximum subsong number is %d", ASAPInfo_MAX_SONGS - 1);
	} while (*s != '\0');
}

static void set_time(const char *s)
{
	duration = ASAPInfo_ParseDuration(s);
	if (duration <= 0)
		fatal_error("invalid time format");
}

static void set_mute_mask(const char *s)
{
	int mask = 0;
	while (*s != '\0') {
		if (*s >= '1' && *s <= '8')
			mask |= 1 << (*s - '1');
		s++;
	}
	mute_mask = mask;
}

static void apply_tags(const char *input_file, ASAPInfo *info)
{
	if (tag_author != NULL) {
		if (!ASAPInfo_SetAuthor(info, tag_author))
			fatal_error("invalid author");
	}
	if (tag_name != NULL) {
		if (!ASAPInfo_SetTitle(info, tag_name))
			fatal_error("invalid music name");
		tag_name = NULL;
	}
	if (tag_date != NULL) {
		if (!ASAPInfo_SetDate(info, tag_date))
			fatal_error("invalid date");
	}

	if (song < 0)
		song = ASAPInfo_GetDefaultSong(info);
	else if (song >= ASAPInfo_GetSongs(info)) {
		fatal_error("you have requested subsong %d ...\n"
			"... but %s contains only %d subsongs",
			song, input_file, ASAPInfo_GetSongs(info));
	}
}

static ASAP *load_module(const char *input_file, const unsigned char *module, int module_len)
{
	ASAP *asap = ASAP_New();
	ASAPInfo *info;
	if (asap == NULL)
		fatal_error("out of memory");
	if (!ASAP_Load(asap, input_file, module, module_len))
		fatal_error("%s: unsupported file", input_file);
	info = (ASAPInfo *) ASAP_GetInfo(asap); /* FIXME: avoid cast */
	apply_tags(input_file, info);
	if (duration < 0) {
		duration = ASAPInfo_GetDuration(info, song);
		if (duration < 0)
			duration = 180 * 1000;
	}
	ASAP_PlaySong(asap, song, duration);
	ASAP_MutePokeyChannels(asap, mute_mask);
	return asap;
}

static int set_output_file_from_pattern(const char *pattern, int pattern_len, const ASAPInfo *info)
{
	int pattern_index;
	int output_file_len = 0;
	for (pattern_index = 0; pattern_index < pattern_len; pattern_index++) {
		char c = pattern[pattern_index];
		if (c == '%') {
			const char *tag;
			c = pattern[++pattern_index];
			switch (c) {
			case 'a':
				tag = ASAPInfo_GetAuthor(info);
				break;
			case 'n':
				tag = ASAPInfo_GetTitleOrFilename(info);
				break;
			case 'd':
				tag = ASAPInfo_GetDate(info);
				break;
			case '%':
				tag = "%";
				break;
			default:
				fatal_error("unrecognized %%%c", c);
				return 0;
			}
			while (*tag != '\0') {
				if (output_file_len >= sizeof(output_file) - 1)
					fatal_error("filename too long");
				c = *tag++;
				switch (c) {
				case '<':
				case '>':
				case ':':
				case '/':
				case '\\':
				case '|':
				case '?':
				case '*':
					c = '_';
					break;
				default:
					break;
				}
				output_file[output_file_len++] = c;
			}
		}
		else {
			if (output_file_len >= sizeof(output_file) - 1)
				fatal_error("filename too long");
			output_file[output_file_len++] = c;
		}
	}
	output_file[output_file_len] = '\0';
	return output_file_len;
}

static void set_output_file_from_elements(int path_len, const char *filename, const char *ext)
{
	/* assume ext is as long as in filename */
	if (path_len + strlen(filename) >= sizeof(output_file))
		fatal_error("filename too long");
	sprintf(output_file + path_len, "%.*s%s", (int) (strrchr(filename, '.') - filename), filename, ext);
}

static FILE *open_output_file(const char *input_file, const ASAPInfo *info)
{
	const char *output_ext = strrchr(output_arg, '.');
	FILE *fp;
	if (output_ext == output_arg) {
		/* .EXT */
		set_output_file_from_elements(0, input_file, output_ext);
	}
	else if (output_ext == output_arg + 1 && output_arg[0] == '-') {
		/* -.EXT */
		strcpy(output_file, "stdout");
#ifdef __WIN32
		_setmode(_fileno(stdout), _O_BINARY);
#endif
		return stdout;
	}
	else if (output_ext[-1] == '/' || output_ext[-1] == '\\') {
		/* DIR/.EXT */
		int path_len = set_output_file_from_pattern(output_arg, output_ext - output_arg, info);
		const char *base_name = input_file;
		const char *p;
		for (p = input_file; *p != '\0'; p++)
			if (*p == '/' || *p == '\\')
				base_name = p + 1;
		set_output_file_from_elements(path_len, base_name, output_ext);
	}
	else {
		/* FILE.EXT */
		set_output_file_from_pattern(output_arg, strlen(output_arg), info);
	}
	fp = fopen(output_file, "wb");
	if (fp == NULL)
		fatal_error("cannot write %s", output_file);
	return fp;
}

static void write_output_file(FILE *fp, unsigned char *buffer, int n_bytes)
{
	if (fwrite(buffer, 1, n_bytes, fp) != n_bytes) {
		fclose(fp);
		fatal_error("error writing to %s", output_file);
	}
}

static void convert_to_wav(const char *input_file, const unsigned char *module, int module_len, cibool output_header)
{
	ASAP *asap = load_module(input_file, module, module_len);
	FILE *fp = open_output_file(input_file, ASAP_GetInfo(asap));
	int n_bytes;
	static unsigned char buffer[8192];

	if (output_header) {
		ASAP_GetWavHeader(asap, buffer, sample_format);
		fwrite(buffer, 1, ASAP_WAV_HEADER_LENGTH, fp);
	}
	do {
		n_bytes = ASAP_Generate(asap, buffer, sizeof(buffer), sample_format);
		write_output_file(fp, buffer, n_bytes);
	} while (n_bytes == sizeof(buffer));
	if (fp != stdout)
		fclose(fp);
}

#ifdef HAVE_LIBMP3LAME

#ifdef HAVE_LIBMP3LAME_DLL

#include <windows.h>

typedef struct lame_global_struct *lame_global_flags;
typedef lame_global_flags *(*plame_init)(void);
typedef int (*plame_set_num_samples)(lame_global_flags *, unsigned long);
typedef int (*plame_set_in_samplerate)(lame_global_flags *, int);
typedef int (*plame_set_num_channels)(lame_global_flags *, int);
typedef int (*plame_init_params)(lame_global_flags *);
typedef int (*plame_encode_buffer_interleaved)(lame_global_flags *, short int[], int, unsigned char *, int);
typedef int (*plame_encode_flush)(lame_global_flags *, unsigned char *, int);
typedef int (*plame_close)(lame_global_flags *);
typedef int (*pid3tag_init)(lame_global_flags *);
typedef int (*pid3tag_set_title)(lame_global_flags *, const char *);
typedef int (*pid3tag_set_artist)(lame_global_flags *, const char *);
typedef int (*pid3tag_set_year)(lame_global_flags *, const char *);
typedef int (*pid3tag_set_genre)(lame_global_flags *, const char *);
#define LAME_OKAY 0

static FARPROC lame_proc(HMODULE lame_dll, const char *name)
{
	FARPROC proc = GetProcAddress(lame_dll, name);
	if (proc == NULL) {
		char dll_name[FILENAME_MAX];
		GetModuleFileName(lame_dll, dll_name, FILENAME_MAX);
		fatal_error("%s not found in %s", name, dll_name);
	}
	return proc;
}

#define LAME_FUNC(name) p##name name = (p##name) lame_proc(lame_dll, #name)

#endif

static void convert_to_mp3(const char *input_file, const unsigned char *module, int module_len)
{
	ASAP *asap = load_module(input_file, module, module_len);
	const ASAPInfo *info = ASAP_GetInfo(asap);
	int channels = ASAPInfo_GetChannels(info);
	FILE *fp;
	int n_bytes;
	static unsigned char buffer[8192];
	lame_global_flags *lame = NULL;
	static unsigned char mp3buf[4096 * 5 / 4 + 7200];
	int mp3_bytes;

#ifdef HAVE_LIBMP3LAME_DLL
	HMODULE lame_dll;
	lame_dll = LoadLibrary("libmp3lame.dll");
	if (lame_dll == NULL) {
		lame_dll = LoadLibrary("lame_enc.dll");
		if (lame_dll == NULL)
			fatal_error("libmp3lame.dll and lame_enc.dll not found");
	}
	LAME_FUNC(lame_init);
	LAME_FUNC(lame_set_num_samples);
	LAME_FUNC(lame_set_in_samplerate);
	LAME_FUNC(lame_set_num_channels);
	LAME_FUNC(lame_init_params);
	LAME_FUNC(lame_encode_buffer_interleaved);
	LAME_FUNC(lame_encode_flush);
	LAME_FUNC(lame_close);
#endif
	lame = lame_init();
	if (lame == NULL)
		fatal_error("lame_init failed");
	if (lame_set_num_samples(lame, duration * (ASAP_SAMPLE_RATE / 100) / 10) != LAME_OKAY
	 || lame_set_in_samplerate(lame, ASAP_SAMPLE_RATE) != LAME_OKAY
	 || lame_set_num_channels(lame, channels) != LAME_OKAY)
		fatal_error("lame_set_* failed");
	if (lame_init_params(lame) != LAME_OKAY)
		fatal_error("lame_init_params failed");
	if (tag) {
#ifdef HAVE_LIBMP3LAME_DLL
		LAME_FUNC(id3tag_init);
		LAME_FUNC(id3tag_set_title);
		LAME_FUNC(id3tag_set_artist);
		LAME_FUNC(id3tag_set_year);
		LAME_FUNC(id3tag_set_genre);
#endif
		const char *s;
		int year;
		id3tag_init(lame);
		s = ASAPInfo_GetTitle(info);
		if (s[0] != '\0')
			id3tag_set_title(lame, s);
		s = ASAPInfo_GetAuthor(info);
		if (s[0] != '\0')
			id3tag_set_artist(lame, s);
		year = ASAPInfo_GetYear(info);
		if (year > 0) {
			char year_string[16];
			sprintf(year_string, "%d", year);
			id3tag_set_year(lame, year_string);
		}
		id3tag_set_genre(lame, "Electronic");
	}
	fp = open_output_file(input_file, info);
	do {
		static short pcm[8192];
		int i;
		short *p = pcm;
		n_bytes = ASAP_Generate(asap, buffer, sizeof(buffer), ASAPSampleFormat_S16_L_E);
		for (i = 0; i < n_bytes; i += 2) {
			*p++ = buffer[i] + (buffer[i + 1] << 8);
			if (channels == 1)
				p++;
		}
		mp3_bytes = lame_encode_buffer_interleaved(lame, pcm, n_bytes >> channels, mp3buf, sizeof(mp3buf));
		if (mp3_bytes < 0)
			fatal_error("lame_encode_buffer_interleaved failed");
		write_output_file(fp, mp3buf, mp3_bytes);
	} while (n_bytes == sizeof(buffer));
	mp3_bytes = lame_encode_flush(lame, mp3buf, sizeof(mp3buf));
	if (mp3_bytes < 0)
		fatal_error("lame_encode_flush failed");
	write_output_file(fp, mp3buf, mp3_bytes);
	lame_close(lame);
	if (fp != stdout)
		fclose(fp);
}

#endif /* HAVE_LIBMP3LAME */

static void write_byte(void *obj, int data)
{
	putc(data, (FILE *) obj);
}

static void convert_to_module(const char *input_file, const unsigned char *module, int module_len, const char *output_ext)
{
	const char *input_ext;
	ASAPInfo *info;
	FILE *fp;
	ByteWriter bw;

	input_ext = strrchr(input_file, '.');
	if (input_ext == NULL)
		fatal_error("%s: missing extension", input_file);
	input_ext++;
	info = ASAPInfo_New();
	if (info == NULL)
		fatal_error("out of memory");
	if (!ASAPInfo_Load(info, input_file, module, module_len))
		fatal_error("%s: unsupported file", input_file);
	apply_tags(input_file, info);
	if (duration >= 0)
		ASAPInfo_SetDurationAndLoop(info, song, duration, ASAPInfo_GetLoop(info, song));

	fp = open_output_file(input_file, info);
	bw.obj = fp;
	bw.func = write_byte;
	/* FIXME: stdout */
	if (!ASAPWriter_Write(output_file, bw, info, module, module_len))
		fatal_error("%s: conversion error", input_file);
	if (fp != stdout)
		fclose(fp);
}

static void process_file(const char *input_file)
{
	FILE *fp;
	static unsigned char module[ASAPInfo_MAX_MODULE_LENGTH];
	int module_len;
	const char *output_ext;

	if (output_arg == NULL)
		fatal_error("the -o/--output option is mandatory");
	fp = fopen(input_file, "rb");
	if (fp == NULL)
		fatal_error("cannot open %s", input_file);
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);

	output_ext = strrchr(output_arg, '.');
	if (output_ext == NULL)
		fatal_error("missing .EXT in -o/--output");
	output_ext++;
	if (strcasecmp(output_ext, "wav") == 0)
		convert_to_wav(input_file, module, module_len, TRUE);
	else if (strcasecmp(output_ext, "raw") == 0)
		convert_to_wav(input_file, module, module_len, FALSE);
	else if (strcasecmp(output_ext, "mp3") == 0) {
#ifdef HAVE_LIBMP3LAME
		convert_to_mp3(input_file, module, module_len);
#else
		fatal_error("this build of asapconv doesn't support MP3");
#endif
	}
	else
		convert_to_module(input_file, module, module_len, output_ext);

	song = -1;
	duration = -1;
}

int main(int argc, char *argv[])
{
	const char *options_error = "no input files";
	int i;
	for (i = 1; i < argc; i++) {
		const char *arg = argv[i];
		if (arg[0] != '-') {
			process_file(arg);
			options_error = NULL;
			continue;
		}
		options_error = "options must be specified before the input file";
#define is_opt(c)  (arg[1] == c && arg[2] == '\0')
		if (is_opt('o'))
			output_arg = argv[++i];
		else if (strncmp(arg, "--output=", 9) == 0)
			output_arg = arg + 9;
		else if (is_opt('s'))
			set_song(argv[++i]);
		else if (strncmp(arg, "--song=", 7) == 0)
			set_song(arg + 7);
		else if (is_opt('t'))
			set_time(argv[++i]);
		else if (strncmp(arg, "--time=", 7) == 0)
			set_time(arg + 7);
		else if (is_opt('b') || strcmp(arg, "--byte-samples") == 0)
			sample_format = ASAPSampleFormat_U8;
		else if (is_opt('w') || strcmp(arg, "--word-samples") == 0)
			sample_format = ASAPSampleFormat_S16_L_E;
		else if (is_opt('m'))
			set_mute_mask(argv[++i]);
		else if (strncmp(arg, "--mute=", 7) == 0)
			set_mute_mask(arg + 7);
		else if (is_opt('a'))
			tag_author = argv[++i];
		else if (strncmp(arg, "--author=", 9) == 0)
			tag_author = arg + 9;
		else if (is_opt('n'))
			tag_name = argv[++i];
		else if (strncmp(arg, "--name=", 7) == 0)
			tag_name = arg + 7;
		else if (is_opt('d'))
			tag_date = argv[++i];
		else if (strncmp(arg, "--date=", 7) == 0)
			tag_date = arg + 7;
#ifdef HAVE_LIBMP3LAME
		else if (strcmp(arg, "--tag") == 0)
			tag = TRUE;
#endif
		else if (is_opt('h') || strcmp(arg, "--help") == 0) {
			print_help();
			options_error = NULL;
		}
		else if (is_opt('v') || strcmp(arg, "--version") == 0) {
			printf("asapconv " ASAPInfo_VERSION "\n");
			options_error = NULL;
		}
		else
			fatal_error("unknown option: %s", arg);
	}
	if (options_error != NULL) {
		fprintf(stderr, "asapconv: %s\n", options_error);
		print_help();
		return 1;
	}
	return 0;
}
