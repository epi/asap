/*
 * asap.h - public interface of the ASAP engine
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

#ifndef _ASAP_H_
#define _ASAP_H_

#ifdef __cplusplus
extern "C" {
#endif

/* ASAP version. */
#define ASAP_VERSION_MAJOR   0
#define ASAP_VERSION_MINOR   3
#define ASAP_VERSION_MICRO   0
#define ASAP_VERSION         "0.3.0"

/* Short credits of the ASAP engine. */
#define ASAP_YEARS           "2005-2007"
#define ASAP_CREDITS \
   "Another Slight Atari Player (C) 2005-2007 Piotr Fusik\n" \
   "6502 and POKEY sound emulation (C) 1995-2006 Atari800 development team\n" \
   "CMC, MPT, TMC players (C) 1994-2005 Marcin Lewandowski\n" \
   "RMT player (C) 2002-2005 Radek Sterba\n"

/* Short GPL notice.
   Display after the credits. */
#define ASAP_COPYRIGHT \
   "This program is free software; you can redistribute it and/or modify\n" \
   "it under the terms of the GNU General Public License as published\n" \
   "by the Free Software Foundation; either version 2 of the License,\n" \
   "or (at your option) any later version."

/* Output formats. */
/* unsigned char */
#define AUDIO_FORMAT_U8      0
/* signed short, little-endian */
#define AUDIO_FORMAT_S16_LE  1
/* signed short, big-endian */
#define AUDIO_FORMAT_S16_BE  2
/* signed short, machine's native endian convention */
#ifdef WORDS_BIGENDIAN
#define AUDIO_FORMAT_S16_NE  AUDIO_FORMAT_S16_BE
#else
#define AUDIO_FORMAT_S16_NE  AUDIO_FORMAT_S16_LE
#endif

/* Maximum length of a supported input file.
   You can assume that files longer than this are not supported by ASAP. */
#define ASAP_MODULE_MAX      65000

/* Information about a file. */
typedef struct {
   char author[128];    /* author's name */
   char name[128];      /* title */
   char date[128];      /* creation date */
   char all_info[512];  /* the above information formatted in multiple lines */
   int channels;        /* 1 for mono or 2 for stereo */
   int songs;           /* number of subsongs */
   int default_song;    /* 0-based index of the "main" subsong */
   int durations[32];   /* lengths of songs, in milliseconds, -1 = unspecified */
   int loops[32];       /* whether songs repeat (1) or not (0) */
} ASAP_ModuleInfo;

/* Checks whether the extension of the passed filename is known to ASAP.
   Does no file operations. You can call this function anytime. */
int ASAP_IsOurFile(const char *filename);

/* Gets basic information about a module.
   "filename" determines the file format.
   "module" is the data (the contents of the file).
   "module_len" is the number of data bytes.
   "module_info" is the structure where the information is returned.
   ASAP_GetModuleInfo() returns non-zero on success.
   You can call this function anytime. */
int ASAP_GetModuleInfo(const char *filename, const unsigned char *module,
                       int module_len, ASAP_ModuleInfo *module_info);

/* A helper function. Parses the string in the "mm:ss.xxx" format
   and returns the number of milliseconds or -1 if an error occurs. */
int ASAP_ParseDuration(const char *duration);

/* Initializes ASAP.
   "frequency" is sample rate in Hz (for example 44100).
   "audio_format" is the format of generated samples (see values above).
   "quality" 0 means Ron Fries' pokeysnd,
   1..3 mean Michael Borisov's mzpokeysnd with different filters.
   You must call this function before any of the following functions. */
#ifndef APOKEYSND
void ASAP_Initialize(int frequency, int audio_format, int quality);
#endif

/* Loads a module into ASAP.
   "filename" determines the file format.
   "module" is the data (the contents of the file).
   "module_len" is the number of data bytes.
   ASAP does not make copies of the passed pointers. You can overwrite
   or free "filename" and "module" once this function returns.
   On success, ASAP_Load() returns a pointer to a static structure.
   If NULL is returned, you must not call the following functions. */
const ASAP_ModuleInfo *ASAP_Load(const char *filename,
                                 const unsigned char *module,
                                 int module_len);

/* Prepares ASAP to play the specified song of the loaded module.
   "song" is a zero-based index which must be less than the "songs" field
   of the ASAP_ModuleInfo structure.
   "seconds" is playback time in milliseconds - use durations[song]
   unless you want to override it. -1 means indefinitely. */
void ASAP_PlaySong(int song, int duration);

/* Rewinds the current song.
   "position" is the requested absolute position in milliseconds. */
void ASAP_Seek(int position);

/* Fills in the specified buffer with generated samples.
   "buffer" is a buffer for samples, managed outside ASAP.
   "buffer_len" is the length of this buffer in bytes.
   ASAP_Generate() returns number of bytes actually written
   (less than buffer_len if reached the end).
   You must call ASAP_PlaySong() before this function.
   Normally you use a buffer of a few kilobytes or less,
   and call ASAP_Generate() in a loop or via a callback. */
int ASAP_Generate(void *buffer, int buffer_len);

#ifdef __cplusplus
}
#endif

#endif
