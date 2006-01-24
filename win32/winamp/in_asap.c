/*
 * in_asap.c - ASAP plugin for Winamp
 *
 * Copyright (C) 2005-2006  Piotr Fusik
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
#include <string.h>

#include "in2.h"

#include "asap.h"

#define FREQUENCY          44100
#define BITS_PER_SAMPLE    16
#define QUALITY            1
// This is a magic size for Winamp, better do not modify it
#define BUFFERED_BLOCKS    576
// Winamp's equalizer works only with 16-bit samples and sounds awfully,
// probably because of the DC offset of the generated sound
#define SUPPORT_EQUALIZER  0

static unsigned int channels;
static unsigned int buffered_bytes;

#if 0

// This is used in Winamp examples to disable C runtime in order to produce smaller DLL.
// Currently it doesn't work here, because the following CRT functions are used:
// _fltused, _ftol, floor, rand, free, malloc

BOOL WINAPI _DllMainCRTStartup(HANDLE hInst, ULONG ul_reason_for_call, LPVOID lpReserved)
{
	return TRUE;
}

#endif

static In_Module mod;

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
	MessageBox(hwndParent, ASAP_CREDITS "\n" ASAP_COPYRIGHT,
		"About ASAP Winamp plugin " ASAP_VERSION, MB_OK);
}

static void init(void)
{
	ASAP_Initialize(FREQUENCY,
		BITS_PER_SAMPLE == 8 ? AUDIO_FORMAT_U8 : AUDIO_FORMAT_S16_NE, QUALITY);
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
		if (mod.outMod->CanWrite() >= (int) buffered_bytes
#if SUPPORT_EQUALIZER
			<< mod.dsp_isactive()
#endif
		) {
			static
#if BITS_PER_SAMPLE == 8
				unsigned char
#else
				short int
#endif
				buffer[BUFFERED_BLOCKS * 2
#if SUPPORT_EQUALIZER
				* 2
#endif
				];
			int t;
			ASAP_Generate(buffer, buffered_bytes);
			t = mod.outMod->GetWrittenTime();
			mod.SAAddPCMData(buffer, channels, BITS_PER_SAMPLE, t);
			mod.VSAAddPCMData(buffer, channels, BITS_PER_SAMPLE, t);
#if SUPPORT_EQUALIZER
			t = mod.dsp_dosamples((short int *) buffer, BUFFERED_BLOCKS,
				BITS_PER_SAMPLE, channels, FREQUENCY) * channels * (BITS_PER_SAMPLE / 8);
			mod.outMod->Write((char *) buffer, t);
#else
			mod.outMod->Write((char *) buffer, buffered_bytes);
#endif
		}
		else
			Sleep(20);
	}
	return 0;
}

static int play(char *fn)
{
	HANDLE fh;
	static unsigned char module[65000];
	DWORD module_len;
	int maxlatency;
	DWORD threadId;
	strcpy(current_filename, fn);
	fh = CreateFile(fn, GENERIC_READ, 0, NULL, OPEN_EXISTING,
	                FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
	if (fh == INVALID_HANDLE_VALUE)
		return -1;
	if (!ReadFile(fh, module, sizeof(module), &module_len, NULL)) {
		CloseHandle(fh);
		return -1;
	}
	CloseHandle(fh);
	if (!ASAP_Load(fn, module, (unsigned int) module_len))
		return 1;
	ASAP_PlaySong(ASAP_GetDefSong());
	channels = ASAP_GetChannels();
	buffered_bytes = BUFFERED_BLOCKS * channels * (BITS_PER_SAMPLE / 8);
	maxlatency = mod.outMod->Open(FREQUENCY, channels, BITS_PER_SAMPLE, -1, -1);
	if (maxlatency < 0)
		return 1;
	mod.SetInfo(BITS_PER_SAMPLE, FREQUENCY / 1000, channels, 1);
	mod.SAVSAInit(maxlatency, FREQUENCY);
	// the order of VSASetInfo's arguments in in2.h is wrong!
	// http://forums.winamp.com/showthread.php?postid=1841035
	mod.VSASetInfo(FREQUENCY, channels);
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
	mod.SAVSADeInit();
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
	"dmc\0DoublePlay Chaos Music Composer (*.DMC)\0"
	"mpt\0Music ProTracker (*.MPT)\0"
	"mpd\0Music ProTracker DoublePlay (*.MPD)\0"
	"rmt\0Raster Music Tracker (*.RMT)\0"
	"tmc\0Theta Music Composer 1.x 4-channel (*.TMC)\0"
#ifdef STEREO_SOUND
	"tm8\0Theta Music Composer 1.x 8-channel (*.TM8)\0"
#endif
	"tm2\0Theta Music Composer 2.x (*.TM2)\0"
	,
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
