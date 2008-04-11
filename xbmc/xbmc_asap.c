/*
 * xbmc_asap.c - ASAP plugin for XBMC
 *
 * Copyright (C) 2008  Piotr Fusik
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

#include <windows.h>

#include "asap.h"

typedef struct {
	char author[128];
	char name[128];
	int year;
	int month;
	int day;
	int channels;
	int duration;
} ASAP_SongInfo;

static ASAP_State asap;

static abool loadModule(const char *filename, byte *module, int *module_len)
{
	HANDLE fh;
	abool ok;
	fh = CreateFile(filename, GENERIC_READ, 0, NULL, OPEN_EXISTING,
	                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (fh == INVALID_HANDLE_VALUE)
		return FALSE;
	ok = ReadFile(fh, module, ASAP_MODULE_MAX, module_len, NULL);
	CloseHandle(fh);
	return ok;
}

static abool getModuleInfo(const char *filename, ASAP_ModuleInfo *module_info)
{
	byte module[ASAP_MODULE_MAX];
	int module_len;
	if (!loadModule(filename, module, &module_len))
		return FALSE;
	return ASAP_GetModuleInfo(module_info, filename, module, module_len);
}

static int getTwoDigits(const char *s)
{
	if (s[0] < '0' || s[0] > '9' || s[1] < '0' || s[1] > '9')
		return -1;
	return 10 * (s[0] - '0') + s[1] - '0';
}

static void parseDate(const char *s, ASAP_SongInfo *song_info)
{
	int x;
	int y;
	song_info->year = 0;
	song_info->month = 0;
	song_info->day = 0;
	x = getTwoDigits(s);
	if (x <= 0)
		return;
	if (s[2] == '/') {
		s += 3;
		y = getTwoDigits(s);
		if (y <= 0)
			return;
		if (s[2] == '/') {
			song_info->day = x;
			s += 3;
			x = y;
			y = getTwoDigits(s);
			if (y <= 0)
				return;
		}
		song_info->month = x;
		x = y;
	}
	y = getTwoDigits(s + 2);
	if (y < 0 || s[4] != '\0')
		return;
	song_info->year = 100 * x + y;
}

__declspec(dllexport) int asapGetSongs(const char *filename)
{
	ASAP_ModuleInfo module_info;
	if (!getModuleInfo(filename, &module_info))
		return 0;
	return module_info.songs;
}

__declspec(dllexport) abool asapGetInfo(const char *filename, int song, ASAP_SongInfo *song_info)
{
	ASAP_ModuleInfo module_info;
	if (!getModuleInfo(filename, &module_info))
		return FALSE;
	if (song < 0)
		song = module_info.default_song;
	strcpy(song_info->author, module_info.author);
	strcpy(song_info->name, module_info.name);
	parseDate(module_info.date, song_info);
	song_info->channels = module_info.channels;
	song_info->duration = module_info.durations[song];
	return TRUE;
}

__declspec(dllexport) abool asapLoad(const char *filename, int song, int *channels, int *duration)
{
	byte module[ASAP_MODULE_MAX];
	int module_len;
	if (!loadModule(filename, module, &module_len))
		return FALSE;
	if (!ASAP_Load(&asap, filename, module, module_len))
		return FALSE;
	*channels = asap.module_info.channels;
	if (song < 0)
		song = asap.module_info.default_song;
	*duration = asap.module_info.durations[song];
	ASAP_PlaySong(&asap, song, *duration);
	return TRUE;
}

__declspec(dllexport) void asapSeek(int position)
{
	ASAP_Seek(&asap, position);
}

__declspec(dllexport) int asapGenerate(void *buffer, int buffer_len)
{
	return ASAP_Generate(&asap, buffer, buffer_len, ASAP_FORMAT_S16_LE);
}
