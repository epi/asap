/*
 * wasap.c - Another Slight Atari Player for Win32 systems
 *
 * Copyright (C) 2005  Piotr Fusik
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
#include <shellapi.h>

#include "asap.h"
#include "resource.h"

#define APP_TITLE        "WASAP"
#define WND_CLASS_NAME   "WASAP"
#define FREQUENCY        44100
#define QUALITY_DEFAULT  1


static char *Util_stpcpy(char *dest, const char *src)
{
	size_t len = strlen(src);
	memcpy(dest, src, len + 1);
	return dest + len;
}

static char *Util_utoa(char *dest, unsigned int x)
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

#define BUFFER_SIZE 4096
static HWAVEOUT hwo;
static WAVEHDR wh[2];
static unsigned char buffer[2][BUFFER_SIZE];
static int playing = FALSE;

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
		ASAP_Generate(pwh->lpData, BUFFER_SIZE);
		if (waveOutWrite(hwo, pwh, sizeof(WAVEHDR)) != MMSYSERR_NOERROR)
			WaveOut_Stop();
	}
}

static void CALLBACK WaveOut_Proc(HWAVEOUT hwo2, UINT uMsg, DWORD dwInstance,
                                 DWORD dwParam1, DWORD dwParam2)
{
	if (uMsg == WOM_DONE)
		WaveOut_Write((LPWAVEHDR) dwParam1);
}

static int WaveOut_Init(unsigned int frequency)
{
	WAVEFORMATEX wfx = { WAVE_FORMAT_PCM, 1, frequency, frequency, 1, 8, 0 };
	if (waveOutOpen(&hwo, WAVE_MAPPER, &wfx, (DWORD) WaveOut_Proc, 0,
	                CALLBACK_FUNCTION) != MMSYSERR_NOERROR)
		return FALSE;
	memset(wh, 0, sizeof(wh));
	wh[0].lpData = buffer[0];
	wh[0].dwBufferLength = BUFFER_SIZE;
	wh[1].lpData = buffer[1];
	wh[1].dwBufferLength = BUFFER_SIZE;
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

static void WaveOut_Exit(void)
{
	waveOutUnprepareHeader(hwo, &wh[1], sizeof(wh[1]));
	waveOutUnprepareHeader(hwo, &wh[0], sizeof(wh[0]));
	waveOutClose(hwo);
}


/* Tray ------------------------------------------------------------------- */

static unsigned int songs = 0;
static unsigned int cursong = 0;
static char strFileTitle[MAX_PATH] = "";

#define MYWM_NOTIFYICON  (WM_APP + 1)

static void Tray_Add(HWND hWnd, HICON hIcon)
{
	NOTIFYICONDATA nid;
	nid.cbSize = sizeof(NOTIFYICONDATA);
	nid.hWnd = hWnd;
	nid.uID = 0;
	nid.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;
	nid.uCallbackMessage = MYWM_NOTIFYICON;
	nid.hIcon = hIcon;
	strcpy(nid.szTip, APP_TITLE);
	Shell_NotifyIcon(NIM_ADD, &nid);
}

static void Tray_Modify(HWND hWnd, HICON hIcon)
{
	NOTIFYICONDATA nid;
	char *p;
	nid.cbSize = sizeof(NOTIFYICONDATA);
	nid.hWnd = hWnd;
	nid.uID = 0;
	nid.uFlags = NIF_ICON | NIF_TIP;
	nid.hIcon = hIcon;
	/* we need to be careful because szTip is only 64 characters */
	/* 8 */
	p = Util_stpcpy(nid.szTip, APP_TITLE);
	if (songs > 0) {
		/* 2 */
		*p++ = ':';
		*p++ = ' ';
		/* max 33 */
		if (strlen(strFileTitle) <= 33)
			p = Util_stpcpy(p, strFileTitle);
		else {
			memcpy(p, strFileTitle, 30);
			p = Util_stpcpy(p + 30, "...");
		}
		if (songs > 1) {
			/* 7 */
			p = Util_stpcpy(p, " (song ");
			/* max 3 */
			p = Util_utoa(p, cursong + 1);
			/* 4 */
			p = Util_stpcpy(p, " of ");
			/* max 3 */
			p = Util_utoa(p, songs);
			/* 2 */
			p[0] = ')';
			p[1] = '\0';
		}
	}
	Shell_NotifyIcon(NIM_MODIFY, &nid);
}

static void Tray_Delete(HWND hWnd)
{
	NOTIFYICONDATA nid;
	nid.cbSize = sizeof(NOTIFYICONDATA);
	nid.hWnd = hWnd;
	nid.uID = 0;
	Shell_NotifyIcon(NIM_DELETE, &nid);
}


/* GUI -------------------------------------------------------------------- */

static HICON hStopIcon;
static HICON hPlayIcon;
static HMENU hTrayMenu;
static HMENU hSongMenu;
static HMENU hQualityMenu;

static void SetSongs(unsigned int newsongs)
{
	if (songs < newsongs) {
		do {
			char tmp_buf[16];
			Util_utoa(tmp_buf, songs + 1);
			AppendMenu(hSongMenu, MF_ENABLED | MF_STRING,
			           IDM_SONG1 + songs, tmp_buf);
		} while (++songs < newsongs);
	}
	else if (songs > newsongs) {
		do
			DeleteMenu(hSongMenu, --songs, MF_BYPOSITION);
		while (songs > newsongs);
	}
}

static void PlaySong(HWND hWnd, unsigned int n)
{
	CheckMenuRadioItem(hSongMenu, 0, songs - 1, n, MF_BYPOSITION);
	cursong = n;
	ASAP_PlaySong(n);
	Tray_Modify(hWnd, hPlayIcon);
	WaveOut_Start();
}

static int opening = FALSE;

static void LoadFile(HWND hWnd, int reopen)
{
	static char strFile[MAX_PATH] = "";
	static OPENFILENAME ofn = {
		sizeof(OPENFILENAME),
		NULL,
		0,
		"All supported\0"
		"*.sap;*.cmc;*.cmr;*.dmc;*.mpt;*.mpd;*.tmc\0"
		"Slight Atari Player (*.sap)\0"
		"*.sap\0"
		"Chaos Music Composer (*.cmc;*.cmr;*.dmc)\0"
		"*.cmc;*.cmr;*.dmc\0"
		"Music ProTracker (*.mpt;*.mpd)\0"
		"*.mpt;*.mpd\0"
		"Theta Music Composer (*.tmc)\0"
		"*.tmc\0"
		"\0",
		NULL,
		0,
		1,
		strFile,
		MAX_PATH,
		strFileTitle,
		MAX_PATH,
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
	if ((reopen || GetOpenFileName(&ofn)) && ofn.nFileExtension > 0
	 && strFile[ofn.nFileExtension - 1] == '.') {
		HANDLE fh;
		static unsigned char module[65000];
		DWORD module_len;
		fh = CreateFile(strFile, GENERIC_READ, 0, NULL, OPEN_EXISTING,
		                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN,
		                NULL);
		if (fh != INVALID_HANDLE_VALUE) {
			if (ReadFile(fh, module, sizeof(module), &module_len, NULL)) {
				CloseHandle(fh);
				WaveOut_Stop();
				if (ASAP_Load(strFile + ofn.nFileExtension, module,
					(unsigned int) module_len)) {
					SetSongs(ASAP_GetSongs());
					PlaySong(hWnd, reopen ? cursong : ASAP_GetDefSong());
				}
				else {
					SetSongs(0);
					Tray_Modify(hWnd, hStopIcon);
					MessageBox(hWnd, "Format not supported", APP_TITLE,
							   MB_OK | MB_ICONERROR);
				}
			}
			else
				CloseHandle(fh);
		}
	}
	opening = FALSE;
}

static void Reinitialize(HWND hWnd, unsigned int quality, int reopen)
{
	CheckMenuRadioItem(hQualityMenu, 0, 3, quality, MF_BYPOSITION);
	ASAP_Initialize(FREQUENCY, quality);
	LoadFile(hWnd, reopen);
}

static LRESULT CALLBACK MainWndProc(HWND hWnd, UINT msg, WPARAM wParam,
                                    LPARAM lParam)
{
	UINT idc;
	POINT pt;
	switch (msg) {
	case WM_COMMAND:
		if (opening)
			break;
		idc = LOWORD(wParam);
		switch (idc) {
		case IDM_OPEN:
			LoadFile(hWnd, FALSE);
			break;
		case IDM_STOP:
			WaveOut_Stop();
			Tray_Modify(hWnd, hStopIcon);
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
			if (idc >= IDM_SONG1 && idc < IDM_SONG1 + songs) {
				WaveOut_Stop();
				PlaySong(hWnd, idc - IDM_SONG1);
			}
			else if (idc >= IDM_QUALITY_RF && idc <= IDM_QUALITY_MB3) {
				int reopen = FALSE;
				if (songs > 0) {
					WaveOut_Stop();
					SetSongs(0);
					Tray_Modify(hWnd, hStopIcon);
					reopen = TRUE;
				}
				Reinitialize(hWnd, idc - IDM_QUALITY_RF, reopen);
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
			LoadFile(hWnd, FALSE);
			break;
		case WM_RBUTTONDOWN:
			GetCursorPos(&pt);
			SetForegroundWindow(hWnd);
			TrackPopupMenu(hTrayMenu,
				TPM_RIGHTALIGN | TPM_BOTTOMALIGN | TPM_RIGHTBUTTON,
				pt.x, pt.y, 0, hWnd, NULL);
			PostMessage(hWnd, WM_NULL, 0, 0);
			break;
		default:
			break;
		}
		break;
	default:
		return DefWindowProc(hWnd, msg, wParam, lParam);
	}
	return 0;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
                   LPSTR lpCmdLine, int nCmdShow)
{
	WNDCLASS wc;
	HWND hWnd;
	HMENU hMainMenu;
	MSG msg;

	hWnd = FindWindow(WND_CLASS_NAME, NULL);
	if (hWnd != NULL) {
		HWND hChild = GetLastActivePopup(hWnd);
		if (hChild != hWnd)
			SetForegroundWindow(hChild);
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

	if (!WaveOut_Init(FREQUENCY)) {
		MessageBox(hWnd, "Error initalizing WaveOut", APP_TITLE,
		           MB_OK | MB_ICONERROR);
		return 1;
	}

	hStopIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_STOP));
	hPlayIcon = LoadIcon(hInstance, MAKEINTRESOURCE(IDI_PLAY));
	hMainMenu = LoadMenu(hInstance, MAKEINTRESOURCE(IDR_TRAYMENU));
	hTrayMenu = GetSubMenu(hMainMenu, 0);
	hSongMenu = CreatePopupMenu();
	InsertMenu(hTrayMenu, 1, MF_BYPOSITION | MF_ENABLED | MF_STRING | MF_POPUP,
	           (UINT_PTR) hSongMenu, "So&ng");
	hQualityMenu = GetSubMenu(hTrayMenu, 3);
	SetMenuDefaultItem(hTrayMenu, 0, TRUE);
	Tray_Add(hWnd, hStopIcon);
	Reinitialize(hWnd, QUALITY_DEFAULT, FALSE);
	while (GetMessage(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}
	WaveOut_Exit();
	Tray_Delete(hWnd);
	DestroyMenu(hMainMenu);
	return 0;
}
