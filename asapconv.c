/*
 * asapconv.c - converter of ASAP-supported formats
 *
 * Copyright (C) 2005-2010  Piotr Fusik
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

#include "asap.h"

static const char *output_arg = NULL;
static int song = -1;
static ASAP_SampleFormat sample_format = ASAP_FORMAT_S16_LE;
static int duration = -1;
static int mute_mask = 0;
static const char *tag_author = NULL;
static const char *tag_name = NULL;
static const char *tag_date = NULL;

static void print_help(void)
{
	printf(
		"Usage: asapconv [OPTIONS] INPUTFILE...\n"
		"Each INPUTFILE must be in a supported format:\n"
		"SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8 or TM2.\n"
		"Output EXT must be one of the above or WAV or RAW.\n"
		"Options:\n"
		"-o FILE.EXT --output=FILE.EXT  Write to the specified file\n"
		"-o .EXT     --output=.EXT      Use input file path and name\n"
		"-o DIR/.EXT --output=DIR/.EXT  Write to the specified directory\n"
		"-o -.EXT    --output=-.EXT     Write to standard output\n"
		"-h          --help             Display this information\n"
		"-v          --version          Display version information\n"
		"Options for WAV and RAW output:\n"
		"-s SONG     --song=SONG        Select subsong number (zero-based)\n"
		"-t TIME     --time=TIME        Set output length (MM:SS format)\n"
		"-b          --byte-samples     Output 8-bit samples\n"
		"-w          --word-samples     Output 16-bit samples (default)\n"
		"-m CHANNELS --mute=CHANNELS    Mute POKEY channels (1-8)\n"
		"Options for SAP output:\n"
		"-a \"TEXT\"   --author=\"TEXT\"    Set author name\n"
		"-n \"TEXT\"   --name=\"TEXT\"      Set music name\n"
		"-d \"TEXT\"   --date=\"TEXT\"      Set music creation date (DD/MM/YYYY format)\n"
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
		if (song >= ASAP_SONGS_MAX)
			fatal_error("maximum subsong number is %d", ASAP_SONGS_MAX - 1);
	} while (*s != '\0');
}

static void set_time(const char *s)
{
	duration = ASAP_ParseDuration(s);
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

static const char *set_tag(const char *tag, const char *s)
{
	if (strlen(s) >= ASAP_INFO_CHARS - 1)
		fatal_error("%s too long", tag);
	return s;
}

static void get_song(const char *input_file, const ASAP_ModuleInfo *module_info)
{
	if (song < 0)
		song = module_info->default_song;
	if (song >= module_info->songs) {
		fatal_error("you have requested subsong %d ...\n"
			"... but %s contains only %d subsongs",
			song, input_file, module_info->songs);
	}
}

static void convert_to_wav(const char *input_file, const byte *module, int module_len, const char *output_file, FILE *fp, abool output_header)
{
	static ASAP_State asap;
	int n_bytes;
	static byte buffer[8192];
	abool opened = FALSE;

	if (!ASAP_Load(&asap, input_file, module, module_len))
		fatal_error("%s: unsupported file", input_file);
	get_song(input_file, &asap.module_info);
	if (duration < 0) {
		duration = asap.module_info.durations[song];
		if (duration < 0)
			duration = 180 * 1000;
	}
	ASAP_PlaySong(&asap, song, duration);
	ASAP_MutePokeyChannels(&asap, mute_mask);

	if (fp == NULL) {
		fp = fopen(output_file, "wb");
		if (fp == NULL)
			fatal_error("cannot write %s", output_file);
		opened = TRUE;
	}
	if (output_header) {
		ASAP_GetWavHeader(&asap, buffer, sample_format);
		fwrite(buffer, 1, ASAP_WAV_HEADER_BYTES, fp);
	}
	do {
		n_bytes = ASAP_Generate(&asap, buffer, sizeof(buffer), sample_format);
		if (fwrite(buffer, 1, n_bytes, fp) != n_bytes) {
			fclose(fp);
			fatal_error("error writing to %s", output_file);
		}
	} while (n_bytes == sizeof(buffer));
	if (opened)
		fclose(fp);
}

static void convert_module(const char *input_file, const byte *module, int module_len, const char *output_file, FILE *fp, const char *output_ext)
{
	const char *input_ext;
	ASAP_ModuleInfo module_info;
	const char *possible_ext;
	static byte out_module[ASAP_MODULE_MAX];
	int out_module_len;
	abool opened = FALSE;

	input_ext = strrchr(input_file, '.');
	if (input_ext == NULL)
		fatal_error("%s: missing extension", input_file);
	input_ext++;
	if (!ASAP_GetModuleInfo(&module_info, input_file, module, module_len))
		fatal_error("%s: unsupported file", input_file);
	if (tag_author != NULL)
		strcpy(module_info.author, tag_author);
	if (tag_name != NULL) {
		strcpy(module_info.name, tag_name);
		tag_name = NULL;
	}
	if (tag_date != NULL)
		strcpy(module_info.date, tag_date);
	if (duration >= 0) {
		get_song(input_file, &module_info);
		module_info.durations[song] = duration;
	}

	if (strcasecmp(input_ext, output_ext) == 0 && ASAP_CanSetModuleInfo(input_file))
		out_module_len = ASAP_SetModuleInfo(&module_info, module, module_len, out_module);
	else {
		possible_ext = ASAP_CanConvert(input_file, &module_info, module, module_len);
		if (possible_ext == NULL)
			fatal_error("%s: cannot convert", input_file);
		if (strcasecmp(output_ext, possible_ext) != 0)
			fatal_error("%s: can convert to .%s but not .%s", input_file, possible_ext, output_ext);
		out_module_len = ASAP_Convert(input_file, &module_info, module, module_len, out_module);
	}
	if (out_module_len < 0)
		fatal_error("%s: conversion error", input_file);

	if (fp == NULL) {
		fp = fopen(output_file, "wb");
		if (fp == NULL)
			fatal_error("cannot write %s", output_file);
		opened = TRUE;
	}
	fwrite(out_module, 1, out_module_len, fp);
	if (opened)
		fclose(fp);
}

static void process_file(const char *input_file)
{
	FILE *fp;
	static byte module[ASAP_MODULE_MAX];
	int module_len;
	const char *output_ext;
	static char output_file_buffer[FILENAME_MAX];
	const char *output_file;

	if (output_arg == NULL)
		fatal_error("the -o/--output option is mandatory");
	fp = fopen(input_file, "rb");
	if (fp == NULL)
		fatal_error("cannot open %s", input_file);
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);

	fp = NULL;
	output_ext = strrchr(output_arg, '.');
	if (output_ext == NULL)
		fatal_error("missing .EXT in -o/--output");
	if (output_ext == output_arg) {
		if (strlen(input_file) >= FILENAME_MAX)
			fatal_error("filename too long");
		strcpy(output_file_buffer, input_file);
		ASAP_ChangeExt(output_file_buffer, output_ext + 1);
		output_file = output_file_buffer;
	}
	else if (output_ext == output_arg + 1 && output_arg[0] == '-') {
		output_file = "stdout";
#ifdef __WIN32
		_setmode(_fileno(stdout), _O_BINARY);
#endif
		fp = stdout;
	}
	else if (output_ext[-1] == '/' || output_ext[-1] == '\\') {
		const char *base_name = input_file;
		const char *p;
		for (p = input_file; *p != '\0'; p++)
			if (*p == '/' || *p == '\\')
				base_name = p + 1;
		if (output_ext - output_arg + strlen(base_name) >= FILENAME_MAX)
			fatal_error("filename too long");
		memcpy(output_file_buffer, output_arg, output_ext - output_arg);
		strcpy(output_file_buffer + (output_ext - output_arg), base_name);
		ASAP_ChangeExt(output_file_buffer, output_ext + 1);
		output_file = output_file_buffer;
	}
	else
		output_file = output_arg;

	output_ext++; /* skip the dot */
	if (strcasecmp(output_ext, "wav") == 0)
		convert_to_wav(input_file, module, module_len, output_file, fp, TRUE);
	else if (strcasecmp(output_ext, "raw") == 0)
		convert_to_wav(input_file, module, module_len, output_file, fp, FALSE);
	else
		convert_module(input_file, module, module_len, output_file, fp, output_ext);

	if (output_arg == output_file)
		output_arg = NULL;
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
			sample_format = ASAP_FORMAT_U8;
		else if (is_opt('w') || strcmp(arg, "--word-samples") == 0)
			sample_format = ASAP_FORMAT_S16_LE;
		else if (is_opt('m'))
			set_mute_mask(argv[++i]);
		else if (strncmp(arg, "--mute=", 7) == 0)
			set_mute_mask(arg + 7);
		else if (is_opt('a'))
			tag_author = set_tag("author", argv[++i]);
		else if (strncmp(arg, "--author=", 9) == 0)
			tag_author = set_tag("author", arg + 9);
		else if (is_opt('n'))
			tag_name = set_tag("name", argv[++i]);
		else if (strncmp(arg, "--name=", 7) == 0)
			tag_name = set_tag("name", arg + 7);
		else if (is_opt('d'))
			tag_date = set_tag("date", argv[++i]);
		else if (strncmp(arg, "--date=", 7) == 0)
			tag_date = set_tag("date", arg + 7);
		else if (is_opt('h') || strcmp(arg, "--help") == 0) {
			print_help();
			options_error = NULL;
		}
		else if (is_opt('v') || strcmp(arg, "--version") == 0) {
			printf("asapconv " ASAP_VERSION "\n");
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
