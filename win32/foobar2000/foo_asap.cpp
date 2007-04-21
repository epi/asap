/*
 * foo_asap.cpp - ASAP plugin for foobar2000 0.9.x
 *
 * Copyright (C) 2006-2007  Piotr Fusik
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

#include "foobar2000/SDK/foobar2000.h"

#include "asap.h"
#include "settings.h"


/* Configuration --------------------------------------------------------- */

static const GUID song_length_guid =
	{ 0x810e12f0, 0xa695, 0x42d2, { 0xab, 0xc0, 0x14, 0x1e, 0xe5, 0xf3, 0xb3, 0xb7 } };
static cfg_int song_length(song_length_guid, -1);

static INT_PTR CALLBACK settings_dialog_proc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	WORD wCtrl;
	switch (uMsg) {
	case WM_INITDIALOG:
		if (song_length.get_value() < 0) {
			CheckRadioButton(hDlg, IDC_UNLIMITED, IDC_LIMITED, IDC_UNLIMITED);
			SetDlgItemInt(hDlg, IDC_LIMIT, DEFAULT_SONG_LENGTH, FALSE);
			EnableWindow(GetDlgItem(hDlg, IDC_LIMIT), FALSE);
		}
		else {
			CheckRadioButton(hDlg, IDC_UNLIMITED, IDC_LIMITED, IDC_LIMITED);
			SetDlgItemInt(hDlg, IDC_LIMIT, (UINT) song_length, FALSE);
			EnableWindow(GetDlgItem(hDlg, IDC_LIMIT), TRUE);
		}
		return TRUE;
	case WM_COMMAND:
		wCtrl = LOWORD(wParam);
		switch (wCtrl) {
		case IDC_UNLIMITED:
		case IDC_LIMITED:
			CheckRadioButton(hDlg, IDC_UNLIMITED, IDC_LIMITED, wCtrl);
			EnableWindow(GetDlgItem(hDlg, IDC_LIMIT), wCtrl == IDC_LIMITED);
			if (wCtrl == IDC_UNLIMITED)
				song_length = -1;
			else {
				HWND hLimit = GetDlgItem(hDlg, IDC_LIMIT);
				SetFocus(hLimit);
				SendMessage(hLimit, EM_SETSEL, 0, -1);
				UINT limit = GetDlgItemInt(hDlg, IDC_LIMIT, NULL, FALSE);
				if (limit != 0U)
					song_length = (int) limit;
			}
			return TRUE;
		case IDC_LIMIT:
			if (HIWORD(wParam) == EN_CHANGE && IsDlgButtonChecked(hDlg, IDC_LIMITED) == BST_CHECKED) {
				UINT limit = GetDlgItemInt(hDlg, IDC_LIMIT, NULL, FALSE);
				if (limit != 0U)
					song_length = (int) limit;
			}
			return TRUE;
		default:
			break;
		}
	default:
		break;
	}
	return FALSE;
}

class preferences_page_asap : public preferences_page
{
public:
	virtual HWND create(HWND parent)
	{
		return CreateDialog(core_api::get_my_instance(), MAKEINTRESOURCE(IDD_SETTINGS), parent, ::settings_dialog_proc);
	}

	virtual const char *get_name()
	{
		return "ASAP";
	}

	virtual GUID get_guid()
	{
		static const GUID a_guid =
			{ 0xf7c0a763, 0x7c20, 0x4b64, { 0x92, 0xbf, 0x11, 0xe5, 0x5d, 0x8, 0xe5, 0x53 } };
		return a_guid;
	}

	virtual GUID get_parent_guid()
	{
		return guid_input;
	}

	virtual bool reset_query()
	{
		return true;
	}

	virtual void reset()
	{
		song_length = -1;
	}
};

static service_factory_single_t<preferences_page_asap> g_preferences_page_asap_factory;


/* Decoding -------------------------------------------------------------- */

static unsigned int channels;
static unsigned int blocks_played;

class input_asap
{
	service_ptr_t<file> m_file;

public:

	static bool g_is_our_content_type(const char *p_content_type)
	{
		return false;
	}

	static bool g_is_our_path(const char *p_path, const char *p_extension)
	{
		return ::ASAP_IsOurFile(p_path) != 0;
	}

	void open(service_ptr_t<file> p_filehint, const char *p_path, t_input_open_reason p_reason, abort_callback &p_abort)
	{
		if (p_reason != input_open_info_read && p_reason != input_open_decode)
			throw exception_io_unsupported_format();
		if (p_filehint.is_empty())
			filesystem::g_open(p_filehint, p_path, filesystem::open_mode_read, p_abort);
		m_file = p_filehint;
		static bool initialized = false;
		if (!initialized) {
			::ASAP_Initialize(FREQUENCY,
				BITS_PER_SAMPLE == 8 ? AUDIO_FORMAT_U8 : AUDIO_FORMAT_S16_NE,
				QUALITY);
			initialized = true;
		}
		static unsigned char module[ASAP_MODULE_MAX];
		unsigned int module_len;
		module_len = m_file->read(module, sizeof(module), p_abort);
		if (!::ASAP_Load(p_path, module, module_len))
			throw exception_io_unsupported_format();
		::channels = ::ASAP_GetChannels();
	}

	t_uint32 get_subsong_count()
	{
		return ::ASAP_GetSongs();
	}

	t_uint32 get_subsong(t_uint32 p_index)
	{
		return p_index;
	}

	void get_info(t_uint32 p_subsong, file_info &p_info, abort_callback &p_abort)
	{
		int length = song_length.get_value();
		if (length > 0)
			p_info.set_length(length);
		p_info.info_set_int("channels", ::channels);
	}

	t_filestats get_file_stats(abort_callback &p_abort)
	{
		return m_file->get_stats(p_abort);
	}

	void decode_initialize(t_uint32 p_subsong, unsigned p_flags, abort_callback &p_abort)
	{
		::ASAP_PlaySong(p_subsong);
		::blocks_played = 0;
	}

	bool decode_run(audio_chunk &p_chunk, abort_callback &p_abort)
	{
		int length = ::song_length.get_value();
		int blocks_to_play;
		unsigned int buffered_bytes;
		if (length < 0)
			blocks_to_play = BUFFERED_BLOCKS;
		else {
			blocks_to_play = length * FREQUENCY - ::blocks_played;
			if (blocks_to_play <= 0)
				return false;
			if (blocks_to_play > BUFFERED_BLOCKS)
				blocks_to_play = BUFFERED_BLOCKS;
		}
		buffered_bytes = (unsigned int) blocks_to_play * ::channels * (BITS_PER_SAMPLE / 8);
		static
#if BITS_PER_SAMPLE == 8
			unsigned char
#else
			short int
#endif
			buffer[BUFFERED_BLOCKS * 2];

		::ASAP_Generate(buffer, buffered_bytes);
		p_chunk.set_data_fixedpoint(buffer, buffered_bytes, FREQUENCY,
		                            ::channels, BITS_PER_SAMPLE,
		                            ::channels == 2 ? audio_chunk::channel_config_stereo
		                                            : audio_chunk::channel_config_mono);
		::blocks_played += blocks_to_play;
		return true;
	}

	void decode_seek(double p_seconds, abort_callback& p_abort)
	{
	}

	bool decode_can_seek()
	{
		return false;
	}

	bool decode_get_dynamic_info(file_info &p_out, double &p_timestamp_delta)
	{
		return false;
	}

	bool decode_get_dynamic_info_track(file_info &p_out, double &p_timestamp_delta)
	{
		return false;
	}

	void decode_on_idle(abort_callback &p_abort)
	{
		m_file->on_idle(p_abort);
	}

	void retag_set_info(t_uint32 p_subsong, const file_info &p_info, abort_callback &p_abort)
	{
	}

	void retag_commit(abort_callback &p_abort)
	{
	}
};

static input_factory_t<input_asap> g_input_asap_factory;


/* File types ------------------------------------------------------------ */

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

	virtual bool get_name(unsigned idx, pfc::string_base &out)
	{
		if (idx < N_FILE_TYPES) {
			out = ::names_and_masks[idx][0];
			return true;
		}
		return false;
	}

	virtual bool get_mask(unsigned idx, pfc::string_base &out)
	{
		if (idx < N_FILE_TYPES) {
			out = ::names_and_masks[idx][1];
			return true;
		}
		return false;
	}

	virtual bool is_associatable(unsigned idx)
	{
		return true;
	}
};

static service_factory_single_t<input_file_type_asap> g_input_file_type_asap_factory;

DECLARE_COMPONENT_VERSION("ASAP", ASAP_VERSION, ASAP_CREDITS "\n" ASAP_COPYRIGHT);
