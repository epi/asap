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

static ASAP_State asap;

__declspec(dllexport) abool asapLoad(const char *filename, int *channels, int *duration)
{
	HANDLE fh;
	byte module[ASAP_MODULE_MAX];
	int module_len;
	int song;
	fh = CreateFile(filename, GENERIC_READ, 0, NULL, OPEN_EXISTING,
	                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (fh == INVALID_HANDLE_VALUE)
		return FALSE;
	if (!ReadFile(fh, module, ASAP_MODULE_MAX, &module_len, NULL)) {
		CloseHandle(fh);
		return FALSE;
	}
	CloseHandle(fh);
	if (!ASAP_Load(&asap, filename, module, module_len))
		return FALSE;
	*channels = asap.module_info.channels;
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
