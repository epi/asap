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

#define FREQUENCY          44100
#define BITS_PER_SAMPLE    16
#define QUALITY            1
#define BUFFERED_BLOCKS    576
#define SUPPORT_SUBSONGS   1

#include "foobar2000/SDK/componentversion.h"
#include "foobar2000/SDK/input.h"
#if SUPPORT_SUBSONGS
#include "foobar2000/SDK/playlist_loader.h"
#endif

#include "asap.h"

static unsigned int channels;
static unsigned int buffered_bytes;

static int open_asap(const char *filename, reader *r)
{
	::buffered_bytes = 0;
	static bool initialized = false;
	if (!initialized) {
		::ASAP_Initialize(FREQUENCY,
			BITS_PER_SAMPLE == 8 ? AUDIO_FORMAT_U8 : AUDIO_FORMAT_S16_NE,
			QUALITY);
		initialized = true;
	}
	static unsigned char module[ASAP_MODULE_MAX];
	unsigned int module_len;
#if SUPPORT_SUBSONGS
	if (r == NULL) {
		r = file::g_open_read(filename);
		if (r == NULL)
			return 0;
		module_len = r->read(module, sizeof(module));
		r->reader_release();
	}
	else
#endif
		module_len = r->read(module, sizeof(module));
	return ::ASAP_Load(filename, module, module_len);
}

class input_asap : public input
{
public:

	virtual bool test_filename(const char *full_path, const char *extension)
	{
		return ::ASAP_IsOurFile(full_path) ? true : false;
	}

	virtual set_info_t set_info(reader *r, const file_info *info)
	{
		return SET_INFO_FAILURE;
	}

	virtual bool open(reader *r, file_info *info, unsigned flags)
	{
		if (!::open_asap(info->get_file_path(), r))
			return false;
		if ((flags & OPEN_FLAG_DECODE) == 0)
			return true;
		::ASAP_PlaySong(
#if SUPPORT_SUBSONGS
			info->get_subsong_index()
#else
			::ASAP_GetDefSong()
#endif
		);
		::channels = ::ASAP_GetChannels();
		::buffered_bytes = BUFFERED_BLOCKS * ::channels * (BITS_PER_SAMPLE / 8);
		return true;
	}

	virtual bool can_seek()
	{
		return false;
	}

	virtual bool seek(double seconds)
	{
		return false;
	}

	virtual int run(audio_chunk *chunk)
	{
		if (::buffered_bytes == 0)
			return 0;
		static
#if BITS_PER_SAMPLE == 8
			unsigned char
#else
			short int
#endif
			buffer[BUFFERED_BLOCKS * 2];

		::ASAP_Generate(buffer, ::buffered_bytes);
		chunk->set_data_fixedpoint(buffer, ::buffered_bytes, FREQUENCY,
		                           ::channels, BITS_PER_SAMPLE);
		return 1;
	}
};

static service_factory_single_t<input,input_asap> foo;

DECLARE_COMPONENT_VERSION("ASAP", ASAP_VERSION, ASAP_CREDITS "\n" ASAP_COPYRIGHT);

static const char * const names_and_masks[][2] = {
	{ "Slight Atari Player (*.sap)", "*.SAP" },
	{ "Chaos Music Composer (*.cmc;*.cmr;*.dmc)", "*.CMC;*.CMR;*.DMC" },
	{ "Music ProTracker (*.mpt;*.mpd)", "*.MPT;*.MPD" },
	{ "Raster Music Tracker (*.rmt)", "*.RMT" },
#ifdef STEREO_SOUND
	{ "Theta Music Composer 1.x (*.tmc;*.tm8)", "*.TMC;*.TM8" },
#else
	{ "Theta Music Composer 1.x (*.tmc)", "*.TMC" },
#endif
	{ "Theta Music Composer 2.x (*.tm2)", "*.TM2" }
};

#define N_FILE_TYPES (sizeof(names_and_masks) / sizeof(names_and_masks[0]))

class input_file_type_asap : public service_impl_single_t<input_file_type>
{
public:

	virtual unsigned get_count()
	{
		return N_FILE_TYPES;
	}

	virtual bool get_name(unsigned idx, string_base &out)
	{
		if (idx < N_FILE_TYPES) {
			out = ::names_and_masks[idx][0];
			return true;
		}
		return false;
	}

	virtual bool get_mask(unsigned idx, string_base &out)
	{
		if (idx < N_FILE_TYPES) {
			out = ::names_and_masks[idx][1];
			return true;
		}
		return false;
	}
};

static service_factory_single_t<input_file_type,input_file_type_asap> foo2;

#if SUPPORT_SUBSONGS

class track_indexer_asap : public track_indexer
{
public:

	virtual int get_tracks(const char *filename, callback *out, reader *r)
	{
		if (!::ASAP_IsOurFile(filename))
			return 0;
		if (::open_asap(filename, r) == 0)
			return 0;
		unsigned int d = ::ASAP_GetDefSong();
		out->on_entry(make_playable_location(filename, (int) d));
		unsigned int n = ::ASAP_GetSongs();
		unsigned int i;
		for (i = 0; i < n; i++)
			if (i != d)
				out->on_entry(make_playable_location(filename, (int) i));
		return 1;
	}
};

static service_factory_single_t<track_indexer,track_indexer_asap> foo3;

#endif /* SUPPORT_SUBSONGS */
