/*
 * wasap.c - Another Slight Atari Player for Win32 systems
 *
 * Copyright (C) 2005-2007  Piotr Fusik
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
#include <shellapi.h>

#include "asap.h"
#include "resource.h"

#define APP_TITLE        "WASAP"
#define WND_CLASS_NAME   "WASAP"
#define BUFFERED_BLOCKS  4096

static int frequency = 44100;
static int use_16bit = 1;
static int quality = 1;
static const ASAP_ModuleInfo *module_info;
static int current_song;

static HWND hWnd;

static char *Util_stpcpy(char *dest, const char *src)
{
	size_t len = strlen(src);
	memcpy(dest, src, len + 1);
	return dest + len;
}

static char *Util_utoa(char *dest, int x)
{
	char tmpbuf[16];
	char *p = tmpbuf + 15;
	*p = '\0';
	do {
		*--p = x % 10 + '0';
		x /= 10;
	} while (x > 0);
	return Util_stpcpy(dest, p);
}


/* WaveOut ---------------------------------------------------------------- */

/* double-buffering, *2 for 16-bit, *2 for stereo */
static unsigned char buffer[2][BUFFERED_BLOCKS * 2 * 2];
static HWAVEOUT hwo = INVALID_HANDLE_VALUE;
static WAVEHDR wh[2] = {
	{ buffer[0], 0, 0, 0, 0, 0, NULL, 0 },
	{ buffer[1], 0, 0, 0, 0, 0, NULL, 0 },
};
static BOOL playing = FALSE;

static void WaveOut_Stop(void)
{
	if (playing) {
		playing = FALSE;
		waveOutReset(hwo);
	}
}

static void WaveOut_Write(LPWAVEHDR pwh)
{
	if (playing) {
		int len = ASAP_Generate(pwh->lpData, pwh->dwBufferLength);
		if (len < (int) pwh->dwBufferLength
		 || waveOutWrite(hwo, pwh, sizeof(WAVEHDR)) != MMSYSERR_NOERROR) {
			// calling StopPlayback() here causes a deadlock
			PostMessage(hWnd, WM_COMMAND, IDM_STOP, 0);
		}
	}
}

static void CALLBACK WaveOut_Proc(HWAVEOUT hwo2, UINT uMsg, DWORD dwInstance,
                                  DWORD dwParam1, DWORD dwParam2)
{
	if (uMsg == WOM_DONE)
		WaveOut_Write((LPWAVEHDR) dwParam1);
}

static int WaveOut_Open(int frequency, int use_16bit, int channels)
{
	WAVEFORMATEX wfx;
	wfx.wFormatTag = WAVE_FORMAT_PCM;
	wfx.nChannels = channels;
	wfx.nSamplesPerSec = frequency;
	wfx.nBlockAlign = channels << use_16bit;
	wfx.nAvgBytesPerSec = frequency * wfx.nBlockAlign;
	wfx.wBitsPerSample = 8 << use_16bit;
	wfx.cbSize = 0;
	if (waveOutOpen(&hwo, WAVE_MAPPER, &wfx, (DWORD) WaveOut_Proc, 0,
	                CALLBACK_FUNCTION) != MMSYSERR_NOERROR)
		return FALSE;
	wh[1].dwBufferLength = wh[0].dwBufferLength = BUFFERED_BLOCKS * wfx.nBlockAlign;
	if (waveOutPrepareHeader(hwo, &wh[0], sizeof(wh[0])) != MMSYSERR_NOERROR
	 || waveOutPrepareHeader(hwo, &wh[1], sizeof(wh[1])) != MMSYSERR_NOERROR)
		return FALSE;
	return TRUE;
}

static void WaveOut_Start(void)
{
	playing = TRUE;
	WaveOut_Write(&wh[0]);
	WaveOut_Write(&wh[1]);
}

static void WaveOut_Close(void)
{
	if (hwo == INVALID_HANDLE_VALUE)
		return;
	WaveOut_Stop();
	if (wh[0].dwFlags & WHDR_PREPARED)
		waveOutUnprepareHeader(hwo, &wh[0], sizeof(wh[0]));
	if (wh[1].dwFlags & WHDR_PREPARED)
		waveOutUnprepareHeader(hwo, &wh[1], sizeof(wh[1]));
	waveOutClose(hwo);
	hwo = INVALID_HANDLE_VALUE;
}


/* Tray ------------------------------------------------------------------- */

static char strFile[MAX_PATH] = "";
#define MYWM_NOTIFYICON  (WM_APP + 1)
static NOTIFYICONDATA nid = {
	sizeof(NOTIFYICONDATA),
	NULL,
	0,
	NIF_ICON | NIF_MESSAGE | NIF_TIP,
	MYWM_NOTIFYICON,
	NULL,
	APP_TITLE
};
static UINT taskbarCreatedMessage;

static void Tray_Modify(HICON hIcon)
{
	char *p;
	nid.hIcon = hIcon;
	/* we need to be careful because szTip is only 64 characters */
	/* 8 */
	p = Util_stpcpy(nid.szTip, APP_TITLE);
	if (module_info != NULL) {
		const char *pb;
		const char *pe;
		for (pe = strFile; *pe != '\0'; pe++);
		for (pb = pe; pb > strFile && pb[-1] != '\\' && pb[-1] != '/'; pb--);
		/* 2 */
		*p++ = ':';
		*p++ = ' ';
		/* max 33 */
		if (pe - pb <= 33)
			p = Util_stpcpy(p, pb);
		else {
			memcpy(p, pb, 30);
			p = Util_stpcpy(p + 30, "...");
		}
		if (current_song >= 0 && module_info->songs > 1) {
			/* 7 */
			p = Util_stpcpy(p, " (song ");
			/* max 3 */
			p = Util_utoa(p, current_song + 1);
			/* 4 */
			p = Util_stpcpy(p, " of ");
			/* max 3 */
			p = Util_utoa(p, module_info->songs);
			/* 2 */
			p[0] = ')';
			p[1] = '\0';
		}
	}
	Shell_NotifyIcon(NIM_MODIFY, &nid);
}


/* GUI -------------------------------------------------------------------- */

static HICON hStopIcon;
static HICON hPlayIcon;
static HMENU hTrayMenu;
static HMENU hSongMenu;
static HMENU hQualityMenu;

static void ClearSongsMenu(void)
{
	int n = GetMenuItemCount(hSongMenu);
	while (--n >= 0)
		DeleteMenu(hSongMenu, n, MF_BYPOSITION);
}

static void SetSongsMenu(int n)
{
	int i;
	for (i = 1; i <= n; i++) {
		char tmp_buf[16];
		Util_utoa(tmp_buf, i);
		AppendMenu(hSongMenu, MF_ENABLED | MF_STRING,
		           IDM_SONG1 + i - 1, tmp_buf);
	}
}

static void PlaySong(int n)
{
	int duration;
	if (module_info == NULL)
		return;
	CheckMenuRadioItem(hSongMenu, 0, module_info->songs - 1, n, MF_BYPOSITION);
	current_song = n;
	duration = module_info->durations[n];
	if (module_info->loops[n])
		duration = 0;
	ASAP_PlaySong(n, duration);
	Tray_Modify(hPlayIcon);
	WaveOut_Start();
}

static void StopPlayback(void)
{
	current_song = -1;
	WaveOut_Stop();
	Tray_Modify(hStopIcon);
}

static void UnloadFile(void)
{
	if (module_info == NULL)
		return;
	EnableMenuItem(hTrayMenu, IDM_FILE_INFO, MF_BYCOMMAND | MF_GRAYED);
	ClearSongsMenu();
	StopPlayback();
	module_info = NULL;
}

static void LoadFile(void)
{
	HANDLE fh;
	static unsigned char module[ASAP_MODULE_MAX];
	DWORD module_len;
	fh = CreateFile(strFile, GENERIC_READ, 0, NULL, OPEN_EXISTING,
	                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (fh == INVALID_HANDLE_VALUE)
		return;
	if (!ReadFile(fh, module, sizeof(module), &module_len, NULL)) {
		CloseHandle(fh);
		return;
	}
	CloseHandle(fh);
	UnloadFile();
	module_info = ASAP_Load(strFile, module, module_len);
	if (module_info != NULL) {
		if (!WaveOut_Open(frequency, use_16bit, module_info->channels)) {
			module_info = NULL;
			MessageBox(hWnd, "Error initalizing WaveOut", APP_TITLE,
					   MB_OK | MB_ICONERROR);
			return;
		}
		EnableMenuItem(hTrayMenu, IDM_FILE_INFO, MF_BYCOMMAND | MF_ENABLED);
		SetSongsMenu(module_info->songs);
		PlaySong(module_info->default_song);
	}
	else {
		MessageBox(hWnd, "Unsupported file format", APP_TITLE,
		           MB_OK | MB_ICONERROR);
	}
}

static int opening = FALSE;

static void SelectAndLoadFile(void)
{
	static OPENFILENAME ofn = {
		sizeof(OPENFILENAME),
		NULL,
		0,
		"All supported\0"
		"*.sap;*.cmc;*.cmr;*.dmc;*.mpt;*.mpd;*.rmt;*.tmc;*.tm8;*.tm2\0"
		"Slight Atari Player (*.sap)\0"
		"*.sap\0"
		"Chaos Music Composer (*.cmc;*.cmr;*.dmc)\0"
		"*.cmc;*.cmr;*.dmc\0"
		"Music ProTracker (*.mpt;*.mpd)\0"
		"*.mpt;*.mpd\0"
		"Raster Music Tracker (*.rmt)\0"
		"*.rmt\0"
		"Theta Music Composer 1.x (*.tmc;*.tm8)\0"
		"*.tmc;*.tm8\0"
		"Theta Music Composer 2.x (*.tm2)\0"
		"*.tm2\0"
		"\0",
		NULL,
		0,
		1,
		strFile,
		MAX_PATH,
		NULL,
		0,
		NULL,
		"Select Atari 8-bit music",
		OFN_ENABLESIZING | OFN_EXPLORER | OFN_HIDEREADONLY
			| OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST,
		0,
		0,
		NULL,
		0,
		NULL,
		NULL
	};
	opening = TRUE;
	ofn.hwndOwner = hWnd;
	if (GetOpenFileName(&ofn))
		LoadFile();
	opening = FALSE;
}

static void ApplyQuality(void)
{
	int reopen = FALSE;
	if (module_info != NULL) {
		UnloadFile();
		reopen = TRUE;
	}
	CheckMenuRadioItem(hQualityMenu, IDM_44100_HZ, IDM_48000_HZ,
		frequency == 44100 ? IDM_44100_HZ : IDM_48000_HZ, MF_BYCOMMAND);
	CheckMenuRadioItem(hQualityMenu, IDM_8BIT, IDM_16BIT,
		IDM_8BIT + use_16bit, MF_BYCOMMAND);
	CheckMenuRadioItem(hQualityMenu, IDM_QUALITY_RF, IDM_QUALITY_MB3,
		IDM_QUALITY_RF + quality, MF_BYCOMMAND);
	ASAP_Initialize(frequency, use_16bit ? AUDIO_FORMAT_S16_LE : AUDIO_FORMAT_U8, quality);
	if (reopen)
		LoadFile();
}

static LRESULT CALLBACK MainWndProc(HWND hWnd, UINT msg, WPARAM wParam,
                                    LPARAM lParam)
{
	int idc;
	POINT pt;
	PCOPYDATASTRUCT pcds;
	switch (msg) {
	case WM_COMMAND:
		if (opening)
			break;
		idc = LOWORD(wParam);
		switch (idc) {
		case IDM_OPEN:
			SelectAndLoadFile();
			break;
		case IDM_STOP:
			StopPlayback();
			break;
		case IDM_FILE_INFO:
			if (module_info != NULL)
				MessageBox(hWnd, module_info->all_info, "File information", MB_OK | MB_ICONINFORMATION);
			break;
		case IDM_ABOUT:
			MessageBox(hWnd,
				ASAP_CREDITS
				"WASAP icons (C) 2005 Lukasz Sychowicz\n\n"
				ASAP_COPYRIGHT,
				APP_TITLE " " ASAP_VERSION,
				MB_OK | MB_ICONINFORMATION);
			break;
		case IDM_EXIT:
			PostQuitMessage(0);
			break;
		default:
			if (module_info != NULL && idc >= IDM_SONG1 && idc < IDM_SONG1 + module_info->songs) {
				StopPlayback();
				PlaySong(idc - IDM_SONG1);
			}
			else if (idc >= IDM_QUALITY_RF && idc <= IDM_QUALITY_MB3) {
				quality = idc - IDM_QUALITY_RF;
				ApplyQuality();
			}
			else if (idc >= IDM_44100_HZ && idc <= IDM_48000_HZ) {
				frequency = idc == IDM_44100_HZ ? 44100 : 48000;
				ApplyQuality();
			}
			else if (idc >= IDM_8BIT && idc <= IDM_16BIT) {
				use_16bit = idc - IDM_8BIT;
				ApplyQuality();
			}
			break;
		}
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	case MYWM_NOTIFYICON:
		if (opening) {
			SetForegroundWindow(GetLastActivePopup(hWnd));
			break;
		}
		switch (lParam) {
		case WM_LBUTTONDOWN:
			SelectAndLoadFile();
			break;
		case WM_MBUTTONDOWN:
			if (module_info == NULL || module_info->songs <= 1)
				break;
			/* FALLTHROUGH */
		case WM_RBUTTONDOWN:
			GetCursorPos(&pt);
			SetForegroundWindow(hWnd);
			TrackPopupMenu(lParam == WM_MBUTTONDOWN ? hSongMenu : hTrayMenu,
				TPM_RIGHTALIGN | TPM_BOTTOMALIGN | TPM_RIGHTBUTTON,
				pt.x, pt.y, 0, hWnd, NULL);
			PostMessage(hWnd, WM_NULL, 0, 0);
			break;
		default:
			break;
		}
		break;
	case WM_COPYDATA:
		pcds = (PCOPYDATASTRUCT) lParam;
		if (pcds->dwData == 'O' && pcds->cbData <= sizeof(strFile)) {
			memcpy(strFile, pcds->lpData, pcds->cbData);
			LoadFile();
		}
		break;
	default:
		if (msg == taskbarCreatedMessage)
			Shell_NotifyIcon(NIM_ADD, &nid);
		return DefWindowProc(hWnd, msg, wParam, lParam);
	}
	return 0;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
                   LPSTR lpCmdLine, int nCmdShow)
{
	char *pb;
	char *pe;
	WNDCLASS wc;
	HMENU hMainMenu;
	MSG msg;

	for (pb = lpCmdLine; *pb == ' ' || *pb == '\t'; pb++);
	for (pe = pb; *pe != '\0'; pe++);
	while (--pe > pb && (*pe == ' ' || *pe == '\t'));
	/* Now pb and pe point at respectively the first and last
	   non-blank character in lpCmdLine. If pb > pe then the command line
	   is blank. */
	if (*pb == '"' && *pe == '"')
		pb++;
	else
		pe++;
	*pe = '\0';
	/* Now pb contains the filename, if any, specified on the command line. */

	hWnd = FindWindow(WND_CLASS_NAME, NULL);
	if (hWnd != NULL) {
		/* as instance of WASAP is already running */
		if (*pb != '\0') {
			/* pass the filename */
			COPYDATASTRUCT cds = { 'O', (DWORD) (pe + 1 - pb), pb };
			SendMessage(hWnd, WM_COPYDATA, (WPARAM) NULL, (LPARAM) &cds);
		}
		else {
			/* bring the open dialog to top */
			HWND hChild = GetLastActivePopup(hWnd);
			if (hChild != hWnd)
				SetForegroundWindow(hChild);
		}
		return 0;
	}

	wc.style = CS_OWNDC | CS_VREDRAW | CS_HREDRAW;
	wc.lpfnWndProc = MainWndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = hInstance;
	wc.hIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_APP));
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = (HBRUSH) (COLOR_WINDOW + 1);
	wc.lpszMenuName = NULL;
	wc.lpszClassName = WND_CLASS_NAME;
	RegisterClass(&wc);

	hWnd = CreateWindow(WND_CLASS_NAME,
		APP_TITLE,
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		CW_USEDEFAULT,
		NULL,
		NULL,
		hInstance,
		NULL
	);

	hStopIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_STOP));
	hPlayIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_PLAY));
	hMainMenu = LoadMenu(hInstance, MAKEINTRESOURCE(IDR_TRAYMENU));
	hTrayMenu = GetSubMenu(hMainMenu, 0);
	hSongMenu = CreatePopupMenu();
	InsertMenu(hTrayMenu, 1, MF_BYPOSITION | MF_ENABLED | MF_STRING | MF_POPUP,
	           (UINT_PTR) hSongMenu, "So&ng");
	hQualityMenu = GetSubMenu(hTrayMenu, 4);
	SetMenuDefaultItem(hTrayMenu, 0, TRUE);
	nid.hWnd = hWnd;
	nid.hIcon = hStopIcon;
	Shell_NotifyIcon(NIM_ADD, &nid);
	taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
	ApplyQuality();
	if (*pb != '\0') {
		memcpy(strFile, pb, pe + 1 - pb);
		LoadFile();
	}
	else
		SelectAndLoadFile();
	while (GetMessage(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}
	WaveOut_Close();
	Shell_NotifyIcon(NIM_DELETE, &nid);
	DestroyMenu(hMainMenu);
	return 0;
}
