/*
 * asapplug.c - ASAP plugin for Audacious
 *
 * Copyright (C) 2010-2013  Piotr Fusik
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
#include <audacious/input.h>
#include <audacious/plugin.h>
#include <libaudcore/audstrings.h>

#include "asap.h"

static ASAP *asap;

static bool_t plugin_init(void)
{
	asap = ASAP_New();
	return asap != NULL;
}

static void plugin_cleanup(void)
{
	ASAP_Delete(asap);
}

static bool_t is_our_file_from_vfs(const char *filename, VFSFile *file)
{
	return ASAPInfo_IsOurFile(filename);
}

static int load_module(const char *filename, VFSFile *file, unsigned char *module)
{
	int module_len;
	if (file != NULL)
		return vfs_fread(module, 1, ASAPInfo_MAX_MODULE_LENGTH, file);
	file = vfs_fopen(filename, "rb");
	if (file == NULL)
		return -1;
	module_len = vfs_fread(module, 1, ASAPInfo_MAX_MODULE_LENGTH, file);
	vfs_fclose(file);
	return module_len;
}

static char *filename_split_subtune(const char *filename, int *song)
{
	const char *sub;
	uri_parse(filename, NULL, NULL, &sub, song);
	return str_nget(filename, sub - filename);
}

static void tuple_set_nonblank(Tuple *tuple, int nfield, const char *value)
{
	if (value[0] != '\0')
		tuple_set_str(tuple, nfield, value);
}

static Tuple *probe_for_tuple(const char *filename, VFSFile *file)
{
	int song = -1;
	unsigned char module[ASAPInfo_MAX_MODULE_LENGTH];
	int module_len;
	ASAPInfo *info = NULL;
	Tuple *tuple = NULL;
	int songs;
	int duration;
	int year;

	char *real_filename = filename_split_subtune(filename, &song);
	if (real_filename != NULL)
		filename = real_filename;
	module_len = load_module(filename, file, module);
	if (module_len > 0) {
		info = ASAPInfo_New();
		if (info != NULL && ASAPInfo_Load(info, filename, module, module_len))
			tuple = tuple_new_from_filename(filename);
	}
	str_unref(real_filename);
	if (tuple == NULL) {
		ASAPInfo_Delete(info);
		return NULL;
	}

	tuple_set_nonblank(tuple, FIELD_ARTIST, ASAPInfo_GetAuthor(info));
	tuple_set_nonblank(tuple, FIELD_TITLE, ASAPInfo_GetTitleOrFilename(info));
	tuple_set_nonblank(tuple, FIELD_DATE, ASAPInfo_GetDate(info));
	tuple_set_str(tuple, FIELD_CODEC, "ASAP");
	songs = ASAPInfo_GetSongs(info);
	if (song > 0) {
		tuple_set_int(tuple, FIELD_SUBSONG_ID, song);
		tuple_set_int(tuple, FIELD_SUBSONG_NUM, songs);
		song--;
	}
	else {
		if (songs > 1)
			tuple_set_subtunes(tuple, songs, NULL);
		song = ASAPInfo_GetDefaultSong(info);
	}
	duration = ASAPInfo_GetDuration(info, song);
	if (duration > 0)
		tuple_set_int(tuple, FIELD_LENGTH, duration);
	year = ASAPInfo_GetYear(info);
	if (year > 0)
		tuple_set_int(tuple, FIELD_YEAR, year);
	ASAPInfo_Delete(info);
	return tuple;
}

static bool_t play_start(const char *filename, VFSFile *file)
{
	int song = -1;
	unsigned char module[ASAPInfo_MAX_MODULE_LENGTH];
	int module_len;
	bool_t ok;
	const ASAPInfo *info;
	int channels;

	char *real_filename = filename_split_subtune(filename, &song);
	if (real_filename != NULL)
		filename = real_filename;
	module_len = load_module(filename, file, module);
	ok = module_len > 0 && ASAP_Load(asap, filename, module, module_len);
	str_unref(real_filename);
	if (!ok)
		return FALSE;

	info = ASAP_GetInfo(asap);
	channels = ASAPInfo_GetChannels(info);
	if (song > 0)
		song--;
	else
		song = ASAPInfo_GetDefaultSong(info);
	if (!ASAP_PlaySong(asap, song, ASAPInfo_GetDuration(info, song)))
		return FALSE;

	if (!aud_input_open_audio(FMT_S16_LE, ASAP_SAMPLE_RATE, channels))
		return FALSE;

	while (!aud_input_check_stop()) {
		int time = aud_input_check_seek();
		if (time >= 0)
			ASAP_Seek(asap, time);
		static unsigned char buffer[4096];
		int len = ASAP_Generate(asap, buffer, sizeof(buffer), ASAPSampleFormat_S16_L_E);
		if (len <= 0)
			break;
		aud_input_write_audio(buffer, len);
	}

	return TRUE;
}

#pragma GCC diagnostic ignored "-Wunused-result"
static void write_byte(void *obj, int data)
{
	VFSFile *file = (VFSFile *) obj;
	const char buf[1] = { data };
	vfs_fwrite(buf, 1, 1, file);
}

static bool_t update_song_tuple(const char *filename, VFSFile *file, const Tuple *tuple)
{
	/* read file */
	unsigned char module[ASAPInfo_MAX_MODULE_LENGTH];
	int module_len = load_module(filename, file, module);
	ASAPInfo *info;
	int year;
	ByteWriter bw;
	bool_t ok;
	if (module_len <= 0)
		return FALSE;
	info = ASAPInfo_New();
	if (info == NULL)
		return FALSE;
	if (!ASAPInfo_Load(info, filename, module, module_len)) {
		ASAPInfo_Delete(info);
		return FALSE;
	}

	/* apply new tags */
	char *s = tuple_get_str(tuple, FIELD_ARTIST);
	if (s != NULL) {
		if (!ASAPInfo_SetAuthor(info, s)) {
			str_unref(s);
			ASAPInfo_Delete(info);
			return FALSE;
		}
		str_unref(s);
	}
	else
		ASAPInfo_SetAuthor(info, "");
	s = tuple_get_str(tuple, FIELD_TITLE);
	if (s != NULL) {
		if (!ASAPInfo_SetTitle(info, s)) {
			str_unref(s);
			ASAPInfo_Delete(info);
			return FALSE;
		}
		str_unref(s);
	}
	else
		ASAPInfo_SetTitle(info, "");
	year = tuple_get_int(tuple, FIELD_YEAR);
	if (year == 0)
		year = -1;
	/* check if year changed so that we don't lose other date parts */
	if (year != ASAPInfo_GetYear(info)) {
		if (year <= 0)
			ASAPInfo_SetDate(info, "");
		else {
			char d[16];
			sprintf(d, "%d", year);
			ASAPInfo_SetDate(info, d);
		}
	}

	/* write file */
	vfs_fseek(file, 0, SEEK_SET);
	bw.obj = file;
	bw.func = write_byte;
	ok = ASAPWriter_Write(filename, bw, info, module, module_len, TRUE) && vfs_ftruncate(file, vfs_ftell(file)) == 0;
	ASAPInfo_Delete(info);
	return ok;
}

static const char *exts[] = { "sap", "cmc", "cm3", "cmr", "cms", "dmc", "dlt", "mpt", "mpd", "rmt", "tmc", "tm8", "tm2", "fc", NULL };

AUD_INPUT_PLUGIN
(
	.name = "ASAP",
	.init = plugin_init,
	.cleanup = plugin_cleanup,
	.about_text = "ASAP " ASAPInfo_VERSION "\n" ASAPInfo_CREDITS "\n" ASAPInfo_COPYRIGHT,
	.have_subtune = TRUE,
	.extensions = exts,
	.is_our_file_from_vfs = is_our_file_from_vfs,
	.probe_for_tuple = probe_for_tuple,
	.play = play_start,
	.update_song_tuple = update_song_tuple
)
