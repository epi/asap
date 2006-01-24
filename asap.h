/*
 * asap.h - public interface of the ASAP engine
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

#ifndef _ASAP_H_
#define _ASAP_H_

#ifdef __cplusplus
extern "C" {
#endif

/* ASAP version. */
#define ASAP_VERSION "0.2.1"

/* Short ASAP credits.
   If you are porting ASAP then display your name, too. */
#define ASAP_CREDITS \
   "Another Slight Atari Player (C) 2005-2006 Piotr Fusik\n" \
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

/* Initializes ASAP.
   "frequency" is sample rate in Hz (for example 44100).
   "audio_format" is the format of generated samples (see values above).
   "quality" 0 means Ron Fries' pokeysnd,
   1..3 mean Michael Borisov's mzpokeysnd with different filters.
   You must call this function before any other ASAP function. */
void ASAP_Initialize(unsigned int frequency, unsigned int audio_format,
                     unsigned int quality);

/* Loads a module.
   "filename" is required to detect file format by the filename extension.
   You can pass a fake filename, but the extension must match the format
   of "module".
   "module" is the data (the contents of the file).
   ASAP uses "filename" and "module" only inside this function.
   "module_len" is the number of data bytes.
   Returns non-zero on success.
   If zero is returned, you must not call any of the following functions. */
int ASAP_Load(const char *filename, const unsigned char *module,
              unsigned int module_len);

/* Returns number of channels for the loaded module.
   1 means mono and 2 means stereo. */
unsigned int ASAP_GetChannels(void);

/* Returns number of songs in the loaded module.
   ASAP supports multiple songs per file. */
unsigned int ASAP_GetSongs(void);

/* Returns zero-based number of the default song.
   This corresponds to the "DEFSONG" tag value in a SAP file
   and is zero for other formats. */
unsigned int ASAP_GetDefSong(void);

/* Prepares ASAP to play the specified song of the loaded module.
   "song" is zero-based and must be less that the value returned
   by ASAP_GetSongs(). Normally, after successful ASAP_Load()
   you should use the value of ASAP_GetDefSong(). */
void ASAP_PlaySong(unsigned int song);

/* Fills in the specified buffer with generated samples.
   "buffer" is a buffer for samples, managed outside ASAP.
   "buffer_len" is the length of this buffer in bytes.
   You must call ASAP_PlaySong() before this function.
   Normally you use a buffer of a few kilobytes or less,
   and call ASAP_Generate() in a loop or via a callback. */
void ASAP_Generate(void *buffer, unsigned int buffer_len);

#ifdef __cplusplus
}
#endif

#endif
