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
	if (!ASAP_Load(szFile, module, (unsigned int) module_len))
		return FALSE;
#else
	if (!ASAP_Load(pszFile, module, (unsigned int) module_len))
		return FALSE;
#endif
	pInfo->nChannels = ASAP_GetChannels();
	pInfo->nSampleRate = FREQUENCY;
	pInfo->nBitsPerSample = BITS_PER_SAMPLE;
	pInfo->nAvgBitrate = 8;
	pInfo->nDuration = 0;
	ASAP_PlaySong(ASAP_GetDefSong());
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
	ASAP_Generate(pHdr->lpData, pHdr->dwBufferLength);
	return PLUGIN_RET_SUCCESS;
}

static void WINAPI asapStopDecodeFile()
{
}

static void WINAPI asapCloseFile()
{
}

static BOOL WINAPI asapGetTag(MAP_PLUGIN_FILETAG *pTag)
{
	return FALSE;
}

static BOOL WINAPI asapGetFileTag(LPCTSTR pszFile, MAP_PLUGIN_FILETAG *pTag)
{
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
