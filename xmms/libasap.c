/*
 * libasap.c - ASAP plugin for XMMS
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

#include "config.h"
#include <string.h>
#include <pthread.h>
#ifdef USE_STDIO
#include <stdio.h>
#else
#include <fcntl.h>
#include <unistd.h>
#endif

#include <xmms/plugin.h>
#include <xmms/util.h>

#include "asap.h"

#define FREQUENCY          44100
#define BITS_PER_SAMPLE    16
#define QUALITY            1
#define BUFFERED_BLOCKS    512

static unsigned int channels;
static unsigned int buffered_bytes;

static InputPlugin mod;

static pthread_t thread_handle;

static volatile int thread_run = FALSE;

static void asap_init(void)
{
	ASAP_Initialize(FREQUENCY,
		BITS_PER_SAMPLE == 8 ? AUDIO_FORMAT_U8 : AUDIO_FORMAT_S16_NE, QUALITY);
}

static int asap_is_our_file(char *filename)
{
	return ASAP_IsOurFile(filename);
}

static void *asap_play_thread(void *arg)
{
	for (;;) {
		static
#if BITS_PER_SAMPLE == 8
			unsigned char
#else
			short int
#endif
			buffer[BUFFERED_BLOCKS * 2];
		ASAP_Generate(buffer, buffered_bytes);
		mod.add_vis_pcm(mod.output->written_time(),
			BITS_PER_SAMPLE == 8 ? FMT_U8 : FMT_S16_NE,
			channels, buffered_bytes, buffer);
		while (thread_run && mod.output->buffer_free() < buffered_bytes)
			xmms_usleep(20000);
		if (!thread_run)
			break;
		mod.output->write_audio(buffer, buffered_bytes);
	}
	mod.output->buffer_free();
	mod.output->buffer_free();
	pthread_exit(NULL);
}

static void asap_play_file(char *filename)
{
	static unsigned char module[ASAP_MODULE_MAX];
	unsigned int module_len;
#ifdef USE_STDIO
	FILE *fp;
	fp = fopen(filename, "rb");
	if (fp == NULL)
		return;
	module_len = fread(module, 1, sizeof(module), fp);
	fclose(fp);
#else
	int fd;
	fd = open(filename, O_RDONLY);
	if (fd == -1)
		return;
	module_len = (unsigned int) read(fd, module, sizeof(module));
	close(fd);
	if ((int) module_len < 0)
		return;
#endif
	if (!ASAP_Load(filename, module, module_len))
		return;
	ASAP_PlaySong(ASAP_GetDefSong());
	channels = ASAP_GetChannels();
	buffered_bytes = BUFFERED_BLOCKS * channels * (BITS_PER_SAMPLE / 8);

	if (!mod.output->open_audio(BITS_PER_SAMPLE == 8 ? FMT_U8 : FMT_S16_NE,
		FREQUENCY, channels))
		return;

	mod.set_info(NULL, -1, BITS_PER_SAMPLE * 1000, FREQUENCY, channels);
	thread_run = TRUE;
	pthread_create(&thread_handle, NULL, asap_play_thread, NULL);
}

static void asap_pause(short paused)
{
	mod.output->pause(paused);
}

static void asap_stop(void)
{
	if (thread_run) {
		thread_run = FALSE;
		pthread_join(thread_handle, NULL);
		mod.output->close_audio();
	}
}

static int asap_get_time(void)
{
	if (!thread_run || !mod.output->buffer_playing())
		return -1;
	return mod.output->output_time();
}

static InputPlugin mod = {
	NULL, NULL,
	"ASAP " ASAP_VERSION,
	asap_init,
	NULL,
	NULL,
	asap_is_our_file,
	NULL,
	asap_play_file,
	asap_stop,
	asap_pause,
	NULL,
	NULL,
	asap_get_time,
	NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL
};

InputPlugin *get_iplugin_info(void)
{
	return &mod;
}
