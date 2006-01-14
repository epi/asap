/*
 * foo_asap.cpp - ASAP plugin for foobar2000
 *
 * Copyright (C) 2006  Piotr Fusik
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

#include "foobar2000/SDK/componentversion.h"
#include "foobar2000/SDK/input.h"

#include "asap.h"

#define FREQUENCY          44100
#define BITS_PER_SAMPLE    16
#define QUALITY            1
#define BUFFERED_BLOCKS    576

static unsigned int channels;
static unsigned int buffered_bytes;

static
#if BITS_PER_SAMPLE == 8
	unsigned char
#else
	short int
#endif
	buffer[BUFFERED_BLOCKS * 2];

class input_asap : public input
{
public:
	bool test_filename(const char *full_path, const char *extension);
	set_info_t set_info(reader *r, const file_info *info) { return SET_INFO_FAILURE; }
	bool open(reader *r, file_info *info, unsigned flags);
	bool can_seek() { return false; }
	bool seek(double seconds) { return false; }
	int run(audio_chunk *chunk);
};

#define EXT(c1, c2, c3) (((c1 << 16) + (c2 << 8) + c3) | 0x202020)

bool input_asap::test_filename(const char *full_path, const char *extension)
{
	int ext = 0;
	while (*extension > ' ')
		ext = (ext << 8) + (*extension++ & 0xff);
	if (*extension != '\0')
		return false;
	switch (ext | 0x202020) {
	case EXT('C', 'M', 'C'):
	case EXT('C', 'M', 'R'):
	case EXT('D', 'M', 'C'):
	case EXT('M', 'P', 'D'):
	case EXT('M', 'P', 'T'):
	case EXT('R', 'M', 'T'):
	case EXT('S', 'A', 'P'):
	case EXT('T', 'M', 'C'):
#ifdef STEREO_SOUND
	case EXT('T', 'M', '8'):
#endif
		return true;
	default:
		return false;
	}
}

static bool initialized = false;

bool input_asap::open(reader *r, file_info *info, unsigned flags)
{
	::buffered_bytes = 0;
	if (!::initialized) {
		::ASAP_Initialize(FREQUENCY,
			BITS_PER_SAMPLE == 8 ? AUDIO_FORMAT_U8 : AUDIO_FORMAT_S16_NE,
			QUALITY);
		::initialized = true;
	}
	static unsigned char module[65000];
	unsigned int module_len = r->read(module, sizeof(module));
	if (!::ASAP_Load(info->get_file_path(), module, module_len))
		return false;
	if ((flags & OPEN_FLAG_DECODE) == 0)
		return true;
	::ASAP_PlaySong(::ASAP_GetDefSong());
	::channels = ::ASAP_GetChannels();
	::buffered_bytes = BUFFERED_BLOCKS * ::channels * (BITS_PER_SAMPLE / 8);
	return true;
}

int input_asap::run(audio_chunk *chunk)
{
	if (::buffered_bytes == 0)
		return 0;
	::ASAP_Generate(::buffer, ::buffered_bytes);
	chunk->set_data_fixedpoint(::buffer, ::buffered_bytes, FREQUENCY, ::channels, BITS_PER_SAMPLE);
	return 1;
}

static service_factory_t<input,input_asap> foo;

DECLARE_COMPONENT_VERSION("ASAP", ASAP_VERSION, ASAP_CREDITS "\n" ASAP_COPYRIGHT);

#define ASAP_FILE_TYPE(ext, desc) namespace asap_ ## ext { DECLARE_FILE_TYPE(desc " (*." #ext ")", "*." #ext); }

ASAP_FILE_TYPE(SAP, "Slight Atari Player");
ASAP_FILE_TYPE(CMC, "Chaos Music Composer");
ASAP_FILE_TYPE(CMR, "Chaos Music Composer / Rzog");
ASAP_FILE_TYPE(DMC, "DoublePlay Chaos Music Composer");
ASAP_FILE_TYPE(MPT, "Music ProTracker");
ASAP_FILE_TYPE(MPD, "Music ProTracker DoublePlay");
ASAP_FILE_TYPE(RMT, "Raster Music Tracker");
ASAP_FILE_TYPE(TMC, "Theta Music Composer");
#ifdef STEREO_SOUND
ASAP_FILE_TYPE(TM8, "Theta Music Composer 8-channel");
#endif
