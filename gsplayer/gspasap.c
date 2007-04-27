/*
 * gspasap.c - ASAP plugin for GSPlayer
 *
 * Copyright (C) 2007  Piotr Fusik
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
#include <windows.h>
#include <tchar.h>

#include "mapplugin.h"

#include "asap.h"

#define FREQUENCY        44100
#define BITS_PER_SAMPLE  8
#define QUALITY          0

static void WINAPI asapInit()
{
	ASAP_Initialize(FREQUENCY,
		BITS_PER_SAMPLE == 8 ? AUDIO_FORMAT_U8 : AUDIO_FORMAT_S16_NE,
		QUALITY);
}

static void WINAPI asapQuit()
{
}

static DWORD WINAPI asapGetFunc()
{
	return PLUGIN_FUNC_DECFILE;
}

static BOOL WINAPI asapGetPluginName(LPTSTR pszName)
{
	_tcscpy(pszName, _T("ASAP plug-in"));
	return TRUE;
}

static BOOL WINAPI asapSetEqualizer(MAP_PLUGIN_EQ *pEQ)
{
	return FALSE;
}

static void WINAPI asapShowConfigDlg(HWND hwndParent)
{
}

static LPCTSTR exts[] = {
	_T("sap"), _T("Slight Atari Player (*.sap)"),
	_T("cmc"), _T("Chaos Music Composer (*.cmc)"),
	_T("cmr"), _T("Chaos Music Composer / Rzog (*.cmr)"),
	_T("dmc"), _T("DoublePlay Chaos Music Composer (*.dmc)"),
	_T("mpt"), _T("Music ProTracker (*.mpt)"),
	_T("mpd"), _T("Music ProTracker DoublePlay (*.mpd)"),
	_T("rmt"), _T("Raster Music Tracker (*.rmt)"),
	_T("tmc"), _T("Theta Music Composer 1.x 4-channel (*.tmc)"),
#ifdef STEREO_SOUND
	_T("tm8"), _T("Theta Music Composer 1.x 8-channel (*.tm8)"),
#endif
	_T("tm2"), _T("Theta Music Composer 2.x (*.tm2)")
};
#define N_EXTS (sizeof(exts) / sizeof(exts[0]) / 2)

static int WINAPI asapGetFileExtCount()
{
	return N_EXTS;
}

static BOOL WINAPI asapGetFileExt(int nIndex, LPTSTR pszExt, LPTSTR pszExtDesc)
{
	if (nIndex >= N_EXTS)
		return FALSE;
	_tcscpy(pszExt, exts[nIndex * 2]);
	_tcscpy(pszExtDesc, exts[nIndex * 2 + 1]);
	return TRUE;
}

static BOOL WINAPI asapIsValidFile(LPCTSTR pszFile)
{
#ifdef _UNICODE
	char szFile[MAX_PATH];
	if (WideCharToMultiByte(CP_ACP, 0, pszFile, -1, szFile, MAX_PATH, NULL, NULL) <= 0)
		return FALSE;
	return ASAP_IsOurFile(szFile);
#else
	return ASAP_IsOurFile(pszFile);
#endif
}

static BOOL WINAPI asapOpenFile(LPCTSTR pszFile, MAP_PLUGIN_FILE_INFO *pInfo)
{
	HANDLE fh;
	static unsigned char module[ASAP_MODULE_MAX];
	DWORD module_len;
	const ASAP_ModuleInfo *module_info;
	int duration;
#ifdef _UNICODE
	char szFile[MAX_PATH];
	if (WideCharToMultiByte(CP_ACP, 0, pszFile, -1, szFile, MAX_PATH, NULL, NULL) <= 0)
		return FALSE;
#endif
	fh = CreateFile(pszFile, GENERIC_READ, 0, NULL, OPEN_EXISTING,
	                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (fh == INVALID_HANDLE_VALUE)
		return FALSE;
	if (!ReadFile(fh, module, sizeof(module), &module_len, NULL)) {
		CloseHandle(fh);
		return FALSE;
	}
	CloseHandle(fh);
#ifdef _UNICODE
	module_info = ASAP_Load(szFile, module, module_len);
#else
	module_info = ASAP_Load(pszFile, module, module_len);
#endif
	pInfo->nChannels = module_info->channels;
	pInfo->nSampleRate = FREQUENCY;
	pInfo->nBitsPerSample = BITS_PER_SAMPLE;
	pInfo->nAvgBitrate = 8;
	duration = module_info->durations[module_info->default_song];
	pInfo->nDuration = duration * 1000;
	ASAP_PlaySong(module_info->default_song, duration);
	return TRUE;
}

static long WINAPI asapSeekFile(long lTime)
{
	return 0;
}

static BOOL WINAPI asapStartDecodeFile()
{
	return TRUE;
}

static int WINAPI asapDecodeFile(WAVEHDR *pHdr)
{
	int len = ASAP_Generate(pHdr->lpData, pHdr->dwBufferLength);
	return len < (int) pHdr->dwBufferLength ? PLUGIN_RET_EOF : PLUGIN_RET_SUCCESS;
}

static void WINAPI asapStopDecodeFile()
{
}

static void WINAPI asapCloseFile()
{
}

static BOOL WINAPI asapGetTag(MAP_PLUGIN_FILETAG *pTag)
{
	// TODO
	return FALSE;
}

static BOOL WINAPI asapGetFileTag(LPCTSTR pszFile, MAP_PLUGIN_FILETAG *pTag)
{
	// TODO
	return FALSE;
}

static BOOL WINAPI asapOpenStreaming(LPBYTE pbBuf, DWORD cbBuf, MAP_PLUGIN_STREMING_INFO *pInfo)
{
	return FALSE;
}

static int WINAPI asapDecodeStreaming(LPBYTE pbInBuf, DWORD cbInBuf, DWORD *pcbInUsed, WAVEHDR *pHdr)
{
	return PLUGIN_RET_ERROR;
}

static void WINAPI asapCloseStreaming()
{
}

static MAP_DEC_PLUGIN plugin = {
	PLUGIN_DEC_VER,
	sizeof(TCHAR),
	0,
	asapInit,
	asapQuit,
	asapGetFunc,
	asapGetPluginName,
	asapSetEqualizer,
	asapShowConfigDlg,
	asapGetFileExtCount,
	asapGetFileExt,
	asapIsValidFile,
	asapOpenFile,
	asapSeekFile,
	asapStartDecodeFile,
	asapDecodeFile,
	asapStopDecodeFile,
	asapCloseFile,
	asapGetTag,
	asapGetFileTag,
	asapOpenStreaming,
	asapDecodeStreaming,
	asapCloseStreaming
};

__declspec(dllexport) MAP_DEC_PLUGIN * WINAPI mapGetDecoder()
{
	return &plugin;
}
