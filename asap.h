// Generated automatically with "cito". Do not edit.
#pragma once
#include <stdbool.h>
#include <stdint.h>
#ifdef __cplusplus
extern "C" {
#endif
typedef struct ASAP ASAP;
typedef struct ASAPInfo ASAPInfo;
typedef struct ASAPWriter ASAPWriter;

typedef enum {
	ASAPSampleFormat_U8,
	ASAPSampleFormat_S16_L_E,
	ASAPSampleFormat_S16_B_E
} ASAPSampleFormat;
ASAP *ASAP_New(void);
void ASAP_Delete(ASAP *self);
#define ASAP_SAMPLE_RATE 44100
void ASAP_DetectSilence(ASAP *self, int seconds);
bool ASAP_Load(ASAP *self, const char *filename, uint8_t const *module, int moduleLen);
const ASAPInfo *ASAP_GetInfo(const ASAP *self);
void ASAP_MutePokeyChannels(ASAP *self, int mask);
bool ASAP_PlaySong(ASAP *self, int song, int duration);
int ASAP_GetBlocksPlayed(const ASAP *self);
int ASAP_GetPosition(const ASAP *self);
bool ASAP_SeekSample(ASAP *self, int block);
bool ASAP_Seek(ASAP *self, int position);
int ASAP_GetWavHeader(const ASAP *self, uint8_t *buffer, ASAPSampleFormat format, bool metadata);
int ASAP_Generate(ASAP *self, uint8_t *buffer, int bufferLen, ASAPSampleFormat format);
int ASAP_GetPokeyChannelVolume(const ASAP *self, int channel);
ASAPInfo *ASAPInfo_New(void);
void ASAPInfo_Delete(ASAPInfo *self);
#define ASAPInfo_VERSION_MAJOR 4
#define ASAPInfo_VERSION_MINOR 0
#define ASAPInfo_VERSION_MICRO 0
#define ASAPInfo_VERSION "4.0.0"
#define ASAPInfo_YEARS "2005-2019"
#define ASAPInfo_CREDITS "Another Slight Atari Player (C) 2005-2019 Piotr Fusik\nCMC, MPT, TMC, TM2 players (C) 1994-2005 Marcin Lewandowski\nRMT player (C) 2002-2005 Radek Sterba\nDLT player (C) 2009 Marek Konopka\nCMS player (C) 1999 David Spilka\nFC player (C) 2011 Jerzy Kut\n"
#define ASAPInfo_COPYRIGHT "This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version."
#define ASAPInfo_MAX_MODULE_LENGTH 65000
#define ASAPInfo_MAX_TEXT_LENGTH 127
#define ASAPInfo_MAX_SONGS 32
const char *ASAPInfo_GetAuthor(const ASAPInfo *self);
bool ASAPInfo_SetAuthor(ASAPInfo *self, const char *value);
const char *ASAPInfo_GetTitle(const ASAPInfo *self);
bool ASAPInfo_SetTitle(ASAPInfo *self, const char *value);
const char *ASAPInfo_GetTitleOrFilename(const ASAPInfo *self);
const char *ASAPInfo_GetDate(const ASAPInfo *self);
bool ASAPInfo_SetDate(ASAPInfo *self, const char *value);
int ASAPInfo_GetYear(const ASAPInfo *self);
int ASAPInfo_GetMonth(const ASAPInfo *self);
int ASAPInfo_GetDayOfMonth(const ASAPInfo *self);
int ASAPInfo_GetChannels(const ASAPInfo *self);
int ASAPInfo_GetSongs(const ASAPInfo *self);
int ASAPInfo_GetDefaultSong(const ASAPInfo *self);
bool ASAPInfo_SetDefaultSong(ASAPInfo *self, int song);
int ASAPInfo_GetDuration(const ASAPInfo *self, int song);
bool ASAPInfo_SetDuration(ASAPInfo *self, int song, int duration);
bool ASAPInfo_GetLoop(const ASAPInfo *self, int song);
bool ASAPInfo_SetLoop(ASAPInfo *self, int song, bool loop);
bool ASAPInfo_IsNtsc(const ASAPInfo *self);
int ASAPInfo_GetTypeLetter(const ASAPInfo *self);
int ASAPInfo_GetPlayerRateScanlines(const ASAPInfo *self);
int ASAPInfo_GetPlayerRateHz(const ASAPInfo *self);
int ASAPInfo_GetMusicAddress(const ASAPInfo *self);
bool ASAPInfo_SetMusicAddress(ASAPInfo *self, int address);
int ASAPInfo_GetInitAddress(const ASAPInfo *self);
int ASAPInfo_GetPlayerAddress(const ASAPInfo *self);
int ASAPInfo_GetCovoxAddress(const ASAPInfo *self);
int ASAPInfo_GetSapHeaderLength(const ASAPInfo *self);
int ASAPInfo_GetInstrumentNamesOffset(const ASAPInfo *self, uint8_t const *module, int moduleLen);
int ASAPInfo_ParseDuration(const char *s);
bool ASAPInfo_IsOurFile(const char *filename);
bool ASAPInfo_IsOurExt(const char *ext);
bool ASAPInfo_Load(ASAPInfo *self, const char *filename, uint8_t const *module, int moduleLen);
const char *ASAPInfo_GetExtDescription(const char *ext);
const char *ASAPInfo_GetOriginalModuleExt(const ASAPInfo *self, uint8_t const *module, int moduleLen);
ASAPWriter *ASAPWriter_New(void);
void ASAPWriter_Delete(ASAPWriter *self);
#define ASAPWriter_MAX_SAVE_EXTS 3
#define ASAPWriter_MAX_DURATION_LENGTH 9
int ASAPWriter_GetSaveExts(const char **exts, const ASAPInfo *info, uint8_t const *module, int moduleLen);
int ASAPWriter_DurationToString(uint8_t *result, int value);
void ASAPWriter_SetOutput(ASAPWriter *self, uint8_t *output, int startIndex, int endIndex);
int ASAPWriter_Write(ASAPWriter *self, const char *targetFilename, const ASAPInfo *info, uint8_t const *module, int moduleLen, bool tag);

#ifdef __cplusplus
}
#endif
