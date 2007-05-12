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
#define ASAP_VERSION_MINOR   4
#define ASAP_VERSION_MICRO   0
#define ASAP_VERSION         "0.4.0"

/* Short credits of the ASAP engine. */
#define ASAP_YEARS           "2005-2007"
#define ASAP_CREDITS \
	"Another Slight Atari Player (C) 2005-2007 Piotr Fusik\n" \
	"CMC, MPT, TMC players (C) 1994-2005 Marcin Lewandowski\n" \
	"RMT player (C) 2002-2005 Radek Sterba\n"

/* Short GPL notice.
   Display after the credits. */
#define ASAP_COPYRIGHT \
	"This program is free software; you can redistribute it and/or modify\n" \
	"it under the terms of the GNU General Public License as published\n" \
	"by the Free Software Foundation; either version 2 of the License,\n" \
	"or (at your option) any later version."

/* Useful type definitions. */
#ifndef FALSE
#define FALSE  0
#endif
#ifndef TRUE
#define TRUE   1
#endif
typedef int abool;
typedef unsigned char byte;

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
	abool loops[32];     /* whether songs repeat or not */
} ASAP_ModuleInfo;

/* POKEY state.
   Not for use outside the ASAP engine. */
typedef struct {
	int audctl;
	int poly_index;
	int div_cycles;
	int audf1;
	int audf2;
	int audf3;
	int audf4;
	int audc1;
	int audc2;
	int audc3;
	int audc4;
	int tick_cycle1;
	int tick_cycle2;
	int tick_cycle3;
	int tick_cycle4;
	int period_cycles1;
	int period_cycles2;
	int period_cycles3;
	int period_cycles4;
	int reload_cycles1;
	int reload_cycles3;
	int out1;
	int out2;
	int out3;
	int out4;
	int delta1;
	int delta2;
	int delta3;
	int delta4;
	char delta_buffer[1024];
} PokeyState;

/* Player state.
   Only module_info is meant to be read outside the ASAP engine. */
typedef struct {
	int cycle;
	int cpu_pc;
	int cpu_a;
	int cpu_x;
	int cpu_y;
	int cpu_s;
	int cpu_nz;
	int cpu_c;
	int cpu_vdi;
	int nearest_event_cycle;
	int timer1_cycle;
	int timer2_cycle;
	int timer4_cycle;
	int irqst;
	PokeyState base_pokey;
	PokeyState extra_pokey;
	int sample_offset;
	int sample_index;
	int samples;
	int iir_acc_left;
	int iir_acc_right;
	ASAP_ModuleInfo module_info;
	char sap_type;
	int sap_player;
	int sap_music;
	int sap_init;
	int sap_fastplay;
	int tmc_per_frame;
	int tmc_per_frame_counter;
	int current_song;
	int current_duration;
	int blocks_played;
	byte song_pos[128];
	byte poly9_lookup[511];
	byte poly17_lookup[16385];
	byte memory[65536];
} ASAP_State;

/* Maximum length of a supported input file.
   You can assume that files longer than this are not supported by ASAP. */
#define ASAP_MODULE_MAX   65000

/* Output sample rate. */
#define ASAP_SAMPLE_RATE  44100

/* Output formats. */
typedef enum {
	ASAP_FORMAT_U8 = 8,       /* unsigned char */
	ASAP_FORMAT_S16_LE = 16,  /* signed short, little-endian */
	ASAP_FORMAT_S16_BE = -16  /* signed short, big-endian */
} ASAP_SampleFormat;

/* Checks whether the extension of the passed filename is known to ASAP. */
abool ASAP_IsOurFile(const char *filename);

/* Gets information about a module.
   "filename" determines file format.
   "module" is the music data (contents of the file).
   "module_len" is the number of data bytes.
   "module_info" is the structure where the information is returned.
   ASAP_GetModuleInfo() returns true on success. */
abool ASAP_GetModuleInfo(ASAP_ModuleInfo *module_info, const char *filename,
                         const byte module[], int module_len);

/* A helper function. Parses the string in the "mm:ss.xxx" format
   and returns the number of milliseconds or -1 if an error occurs. */
int ASAP_ParseDuration(const char *duration);

/* Loads music data.
   "as" is the destination structure.
   "filename" determines file format.
   "module" is the music data (contents of the file).
   "module_len" is the number of data bytes.
   ASAP does not make copies of the passed pointers. You can overwrite
   or free "filename" and "module" once this function returns.
   ASAP_Load() returns true on success.
   If false is returned, the structure is invalid and you cannot
   call the following functions. */
abool ASAP_Load(ASAP_State *as, const char *filename,
                const byte module[], int module_len);

/* Prepares ASAP to play the specified song of the loaded module.
   "as" is ASAP state initialized by ASAP_Load().
   "song" is a zero-based index which must be less than the "songs" field
   of the ASAP_ModuleInfo structure.
   "duration" is playback time in milliseconds - use durations[song]
   unless you want to override it. -1 means indefinitely. */
void ASAP_PlaySong(ASAP_State *as, int song, int duration);

/* Rewinds the current song.
   "as" is ASAP state initialized by ASAP_PlaySong().
   "position" is the requested absolute position in milliseconds. */
void ASAP_Seek(ASAP_State *as, int position);

/* Fills the specified buffer with generated samples.
   "as" is ASAP state initialized by ASAP_PlaySong().
   "buffer" is the destination buffer.
   "buffer_len" is the length of this buffer in bytes.
   "format" is the format of samples.
   ASAP_Generate() returns number of bytes actually written
   (less than buffer_len if reached the end of the song).
   Normally you use a buffer of a few kilobytes or less,
   and call ASAP_Generate() in a loop or via a callback. */
int ASAP_Generate(ASAP_State *as, void *buffer, int buffer_len,
                  ASAP_SampleFormat format);

#ifdef __cplusplus
}
#endif

#endif
