/*
 * in_asap.c - ASAP plugin for XMMS
 *
 * Copyright (C) 2006  Piotr Fusik
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
#include <string.h>
#include <pthread.h>

#include <xmms/plugin.h>
#include <xmms/util.h>

#include "asap.h"

#define FREQUENCY        44100
#define QUALITY_DEFAULT  1
#define BUFFER_SIZE      512

extern InputPlugin mod;

static pthread_t thread_handle;

static volatile int thread_run = FALSE;

static void init(void)
{
	ASAP_Initialize(FREQUENCY, QUALITY_DEFAULT);
}

#define EXT(c1, c2, c3) ((c1 << 16) + (c2 << 8) + c3)

static int is_our_file(char *filename)
{
	const char *p;
	int ext;
	p = strrchr(filename, '.');
	if (p == NULL)
		return FALSE;
	ext = 0;
	while (*++p != '\0') {
		if (ext > 0xffff)
			return FALSE; /* fourth character */
		ext = (ext << 8) + (*p & 0xff);
	}
	switch (ext & 0xdfdfdf) {
	case EXT('C', 'M', 'C'):
	case EXT('C', 'M', 'R'):
	case EXT('D', 'M', 'C'):
	case EXT('M', 'P', 'D'):
	case EXT('M', 'P', 'T'):
	case EXT('R', 'M', 'T'):
	case EXT('S', 'A', 'P'):
	case EXT('T', 'M', 'C'):
		return TRUE;
	default:
		return FALSE;
	}
}

static void *play_thread(void *arg)
{
	for (;;) {
		static unsigned char buffer[BUFFER_SIZE];
		ASAP_Generate(buffer, BUFFER_SIZE);
		mod.add_vis_pcm(mod.output->written_time(), FMT_U8, 1, BUFFER_SIZE, buffer);
		while (thread_run && mod.output->buffer_free() < BUFFER_SIZE)
			xmms_usleep(20000);
		if (!thread_run)
			break;
		mod.output->write_audio(buffer, BUFFER_SIZE);
	}
	mod.output->buffer_free();
	mod.output->buffer_free();
	pthread_exit(NULL);
}

static void play_file(char *filename)
{
	const char *dot;
	FILE *fp;
	static unsigned char module[65000];
	unsigned int module_len;
	dot = strrchr(filename, '.');
	if (dot == NULL)
		return;
	fp = fopen(filename, "rb");
	if (fp == NULL)
		return;
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);
	if (!ASAP_Load(dot + 1, module, module_len))
		return;
	ASAP_PlaySong(ASAP_GetDefSong());

	if (!mod.output->open_audio(FMT_U8, FREQUENCY, 1))
		return;

	mod.set_info(NULL, -1, 8000, FREQUENCY, 1);
	thread_run = TRUE;
	pthread_create(&thread_handle, NULL, play_thread, NULL);
}

static void pause(short paused)
{
	mod.output->pause(paused);
}

static void stop(void)
{
	if (thread_run) {
		thread_run = FALSE;
		pthread_join(thread_handle, NULL);
		mod.output->close_audio();
	}
}

static int get_time(void)
{
	if (!thread_run || !mod.output->buffer_playing())
		return -1;
	return mod.output->output_time();
}

static InputPlugin mod = {
	NULL, NULL,
	"ASAP " ASAP_VERSION,
	init,
	NULL,
	NULL,
	is_our_file,
	NULL,
	play_file,
	stop,
	pause,
	NULL,
	NULL,
	get_time,
	NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
};

InputPlugin *get_iplugin_info(void)
{
	return &mod;
}
