/* Generated automatically with "cito". Do not edit. */
#ifndef _ASAPCI_H_
#define _ASAPCI_H_
typedef int cibool;
#ifndef TRUE
#define TRUE 1
#endif
#ifndef FALSE
#define FALSE 0
#endif
#ifdef __cplusplus
extern "C" {
#endif
typedef struct ASAP ASAP;
typedef struct ASAPInfo ASAPInfo;

typedef enum {
	ASAPSampleFormat_U8,
	ASAPSampleFormat_S16_L_E,
	ASAPSampleFormat_S16_B_E
}
ASAPSampleFormat;
typedef struct 
{
	void *obj;
	void (*func)(void *obj, int data);
}
ByteWriter;
typedef struct 
{
	void *obj;
	void (*func)(void *obj, const char *s);
}
StringConsumer;
ASAP *ASAP_New(void);
void ASAP_Delete(ASAP *self);
void ASAP_DetectSilence(ASAP *self, int seconds);
int ASAP_Generate(ASAP *self, unsigned char *buffer, int bufferLen, ASAPSampleFormat format);
int ASAP_GetBlocksPlayed(ASAP const *self);
ASAPInfo const *ASAP_GetInfo(ASAP const *self);
int ASAP_GetPokeyChannelVolume(ASAP const *self, int channel);
int ASAP_GetPosition(ASAP const *self);
void ASAP_GetWavHeader(ASAP const *self, unsigned char *buffer, ASAPSampleFormat format);
cibool ASAP_Load(ASAP *self, const char *filename, unsigned char const *module, int moduleLen);
void ASAP_MutePokeyChannels(ASAP *self, int mask);
cibool ASAP_PlaySong(ASAP *self, int song, int duration);
#define ASAP_SAMPLE_RATE  44100
cibool ASAP_Seek(ASAP *self, int position);
#define ASAP_WAV_HEADER_LENGTH  44
ASAPInfo *ASAPInfo_New(void);
void ASAPInfo_Delete(ASAPInfo *self);
#define ASAPInfo_COPYRIGHT  "This program is free software; you can redistribute it and/or modify\nit under the terms of the GNU General Public License as published\nby the Free Software Foundation; either version 2 of the License,\nor (at your option) any later version."
#define ASAPInfo_CREDITS  "Another Slight Atari Player (C) 2005-2011 Piotr Fusik\nCMC, MPT, TMC, TM2 players (C) 1994-2005 Marcin Lewandowski\nRMT player (C) 2002-2005 Radek Sterba\nDLT player (C) 2009 Marek Konopka\nCMS player (C) 1999 David Spilka\n"
const char *ASAPInfo_GetAuthor(ASAPInfo const *self);
int ASAPInfo_GetChannels(ASAPInfo const *self);
const char *ASAPInfo_GetDate(ASAPInfo const *self);
int ASAPInfo_GetDayOfMonth(ASAPInfo const *self);
int ASAPInfo_GetDefaultSong(ASAPInfo const *self);
int ASAPInfo_GetDuration(ASAPInfo const *self, int song);
const char *ASAPInfo_GetExtDescription(const char *ext);
cibool ASAPInfo_GetLoop(ASAPInfo const *self, int song);
int ASAPInfo_GetMonth(ASAPInfo const *self);
const char *ASAPInfo_GetOriginalModuleExt(ASAPInfo const *self, unsigned char const *module, int moduleLen);
int ASAPInfo_GetSongs(ASAPInfo const *self);
const char *ASAPInfo_GetTitle(ASAPInfo const *self);
const char *ASAPInfo_GetTitleOrFilename(ASAPInfo const *self);
int ASAPInfo_GetYear(ASAPInfo const *self);
cibool ASAPInfo_IsNtsc(ASAPInfo const *self);
cibool ASAPInfo_IsOurExt(const char *ext);
cibool ASAPInfo_IsOurFile(const char *filename);
cibool ASAPInfo_Load(ASAPInfo *self, const char *filename, unsigned char const *module, int moduleLen);
#define ASAPInfo_MAX_MODULE_LENGTH  65000
#define ASAPInfo_MAX_SONGS  32
#define ASAPInfo_MAX_TEXT_LENGTH  127
int ASAPInfo_ParseDuration(const char *s);
cibool ASAPInfo_SetAuthor(ASAPInfo *self, const char *value);
cibool ASAPInfo_SetDate(ASAPInfo *self, const char *value);
cibool ASAPInfo_SetDuration(ASAPInfo *self, int song, int duration);
cibool ASAPInfo_SetLoop(ASAPInfo *self, int song, cibool loop);
cibool ASAPInfo_SetTitle(ASAPInfo *self, const char *value);
#define ASAPInfo_VERSION  "3.0.0"
#define ASAPInfo_VERSION_MAJOR  3
#define ASAPInfo_VERSION_MICRO  0
#define ASAPInfo_VERSION_MINOR  0
#define ASAPInfo_YEARS  "2005-2011"
int ASAPWriter_DurationToString(unsigned char *result, int value);
void ASAPWriter_EnumSaveExts(StringConsumer output, ASAPInfo const *info, unsigned char const *module, int moduleLen);
#define ASAPWriter_MAX_DURATION_LENGTH  9
cibool ASAPWriter_Write(const char *targetFilename, ByteWriter w, ASAPInfo const *info, unsigned char const *module, int moduleLen);
#ifdef __cplusplus
}
#endif
#endif
