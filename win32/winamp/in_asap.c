/*
 * in_asap.c - ASAP plugin for Winamp
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
#include <string.h>

#include "in2.h"

#include "asap.h"

#define FREQUENCY        44100
#define QUALITY_DEFAULT  1
#define BUFFER_SIZE      576

#if 0

// This is used in Winamp examples to disable C runtime in order to produce smaller DLL.
// Currently it doesn't work here, because the following CRT functions are used:
// _fltused, _ftol, floor, rand, free, malloc

BOOL WINAPI _DllMainCRTStartup(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
	return TRUE;
}

#endif

extern In_Module mod;

static char current_filename[MAX_PATH] = "";

static HANDLE thread_handle = NULL;

static volatile int thread_run = FALSE;

static int paused = 0;

static void config(HWND hwndParent)
{
	// TODO
}

static void about(HWND hwndParent)
{
	MessageBox(hwndParent, ASAP_CREDITS ASAP_COPYRIGHT, "About ASAP Winamp plugin " ASAP_VERSION, MB_OK);
}

static void init(void)
{
	ASAP_Initialize(FREQUENCY, QUALITY_DEFAULT);
}

static void quit(void)
{
}

static void getFileInfo(char *file, char *title, int *length_in_ms)
{
	if (file == NULL || file[0] == '\0')
		file = current_filename;
	if (title != NULL) {
		const char *dirsep;
		dirsep = strrchr(file, '\\');
		strcpy(title, dirsep != NULL ? dirsep + 1 : file);
	}
	if (length_in_ms != NULL)
		*length_in_ms = -1000;
}

static int infoBox(char *file, HWND hwndParent)
{
	// TODO
	return 0;
}

static int isOurFile(char *fn)
{
	return 0;
}

static DWORD WINAPI playThread(LPVOID dummy)
{
	while (thread_run) {
		if (mod.outMod->CanWrite() >= BUFFER_SIZE /* << mod.dsp_isactive() */) {
			static unsigned char buffer[BUFFER_SIZE /* * 2 */];
			int t;
			ASAP_Generate(buffer, BUFFER_SIZE);
			t = mod.outMod->GetWrittenTime();
			mod.SAAddPCMData(buffer, 1, 8, t);
			mod.VSAAddPCMData(buffer, 1, 8, t);
			// t = mod.dsp_dosamples((short int *) buffer, BUFFER_SIZE, 8, 1, FREQUENCY);
			mod.outMod->Write(buffer, BUFFER_SIZE /* t */);
		}
		else
			Sleep(20);
	}
	return 0;
}

static int play(char *fn)
{
	const char *dot;
	HANDLE fh;
	static unsigned char module[65000];
	DWORD module_len;
	int maxlatency;
	DWORD threadId;
	strcpy(current_filename, fn);
	dot = strrchr(fn, '.');
	if (dot == NULL)
		return 1;
	fh = CreateFile(fn, GENERIC_READ, 0, NULL, OPEN_EXISTING,
	                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (fh == INVALID_HANDLE_VALUE)
		return -1;
	if (!ReadFile(fh, module, sizeof(module), &module_len, NULL)) {
		CloseHandle(fh);
		return -1;
	}
	CloseHandle(fh);
	if (!ASAP_Load(dot + 1, module, (unsigned int) module_len))
		return 1;
	ASAP_PlaySong(ASAP_GetDefSong());
	maxlatency = mod.outMod->Open(FREQUENCY, 1, 8, -1, -1);
	if (maxlatency < 0)
		return 1;
	mod.SetInfo(8, FREQUENCY / 1000, 1, 1);
	mod.SAVSAInit(maxlatency, FREQUENCY);
	mod.VSASetInfo(FREQUENCY, 1);
	mod.outMod->SetVolume(-666);
	thread_run = TRUE;
	thread_handle = CreateThread(NULL, 0, playThread, NULL, 0, &threadId);
	return thread_handle != NULL ? 0 : 1;
}

static void pause(void)
{
	paused = 1;
	mod.outMod->Pause(1);
}

static void unPause(void)
{
	paused = 0;
	mod.outMod->Pause(0);
}

static int isPaused(void)
{
	return paused;
}

static void stop(void)
{
	if (thread_handle != NULL) {
		thread_run = FALSE;
		// wait max 30 seconds
		if (WaitForSingleObject(thread_handle, 30 * 1000) == WAIT_TIMEOUT)
			TerminateThread(thread_handle, 0);
		CloseHandle(thread_handle);
		thread_handle = NULL;
	}
	mod.outMod->Close();
	// mod.SAVSADeInit();
}

static int getLength(void)
{
	return -1000;
}

static int getOutputTime(void)
{
	return mod.outMod->GetOutputTime();
}

static void setOutputTime(int time_in_ms)
{
}

static void setVolume(int volume)
{
	mod.outMod->SetVolume(volume);
}

static void setPan(int pan)
{
	mod.outMod->SetPan(pan);
}

static void eqSet(int on, char data[10], int preamp)
{
}

static In_Module mod = {
	IN_VER,
	"ASAP " ASAP_VERSION,
	0, 0, // filled by Winamp
	"sap\0Slight Atari Player (*.SAP)\0"
	"cmc\0Chaos Music Composer (*.CMC)\0"
	"cmr\0Chaos Music Composer / Rzog (*.CMR)\0"
	"mpt\0Music ProTracker (*.MPT)\0"
	"tmc\0Theta Music Composer (*.TMC)\0",
	0,    // is_seekable
	1,    // UsesOutputPlug
	config,
	about,
	init,
	quit,
	getFileInfo,
	infoBox,
	isOurFile,
	play,
	pause,
	unPause,
	isPaused,
	stop,
	getLength,
	getOutputTime,
	setOutputTime,
	setVolume,
	setPan,
	0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // filled by Winamp
	eqSet,
	NULL, // SetInfo
	NULL  // filled by Winamp
};

__declspec(dllexport) In_Module *winampGetInModule2(void)
{
	return &mod;
}
