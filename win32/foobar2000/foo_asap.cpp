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

#include "foobar2000/SDK/foobar2000.h"

#include "asap.h"
#include "settings.h"


/* Configuration --------------------------------------------------------- */

static const GUID song_length_guid =
	{ 0x810e12f0, 0xa695, 0x42d2, { 0xab, 0xc0, 0x14, 0x1e, 0xe5, 0xf3, 0xb3, 0xb7 } };
static cfg_int song_length(song_length_guid, -1);

static const GUID silence_seconds_guid =
	{ 0x40170cb0, 0xc18c, 0x4f97, { 0xaa, 0x6, 0xdb, 0xe7, 0x45, 0xb0, 0x7e, 0x1d } };
static cfg_int silence_seconds(silence_seconds_guid, -1);

static const GUID play_loops_guid =
	{ 0xf08d12f8, 0x58d6, 0x49fc, { 0xae, 0xc5, 0xf4, 0xd0, 0x2f, 0xb2, 0x20, 0xaf } };
static cfg_bool play_loops(play_loops_guid, false);

static void enableTimeInput(HWND hDlg, BOOL enable)
{
	EnableWindow(GetDlgItem(hDlg, IDC_MINUTES), enable);
	EnableWindow(GetDlgItem(hDlg, IDC_SECONDS), enable);
}

static void setFocusAndSelect(HWND hDlg, int nID)
{
	HWND hWnd = GetDlgItem(hDlg, nID);
	SetFocus(hWnd);
	SendMessage(hWnd, EM_SETSEL, 0, -1);
}

static void getTimeInput(HWND hDlg)
{
	BOOL minutesTranslated;
	BOOL secondsTranslated;
	UINT minutes = GetDlgItemInt(hDlg, IDC_MINUTES, &minutesTranslated, FALSE);
	UINT seconds = GetDlgItemInt(hDlg, IDC_SECONDS, &secondsTranslated, FALSE);
	if (minutesTranslated && secondsTranslated)
		song_length = (int) (60 * minutes + seconds);
}

static void getSilenceInput(HWND hDlg)
{
	BOOL translated;
	UINT seconds = GetDlgItemInt(hDlg, IDC_SILSECONDS, &translated, FALSE);
	if (translated)
		silence_seconds = (int) seconds;
}

static INT_PTR CALLBACK settings_dialog_proc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	WORD wCtrl;
	switch (uMsg) {
	case WM_INITDIALOG:
		if (song_length <= 0) {
			CheckRadioButton(hDlg, IDC_UNLIMITED, IDC_LIMITED, IDC_UNLIMITED);
			SetDlgItemInt(hDlg, IDC_MINUTES, DEFAULT_SONG_LENGTH / 60, FALSE);
			SetDlgItemInt(hDlg, IDC_SECONDS, DEFAULT_SONG_LENGTH % 60, FALSE);
			enableTimeInput(hDlg, FALSE);
		}
		else {
			CheckRadioButton(hDlg, IDC_UNLIMITED, IDC_LIMITED, IDC_LIMITED);
			SetDlgItemInt(hDlg, IDC_MINUTES, (UINT) song_length / 60, FALSE);
			SetDlgItemInt(hDlg, IDC_SECONDS, (UINT) song_length % 60, FALSE);
			enableTimeInput(hDlg, TRUE);
		}
		if (silence_seconds <= 0) {
			CheckDlgButton(hDlg, IDC_SILENCE, BST_UNCHECKED);
			SetDlgItemInt(hDlg, IDC_SILSECONDS, DEFAULT_SILENCE_SECONDS, FALSE);
			EnableWindow(GetDlgItem(hDlg, IDC_SILSECONDS), FALSE);
		}
		else {
			CheckDlgButton(hDlg, IDC_SILENCE, BST_CHECKED);
			SetDlgItemInt(hDlg, IDC_SILSECONDS, (UINT) silence_seconds, FALSE);
			EnableWindow(GetDlgItem(hDlg, IDC_SILSECONDS), TRUE);
		}
		CheckRadioButton(hDlg, IDC_LOOPS, IDC_NOLOOPS, play_loops ? IDC_LOOPS : IDC_NOLOOPS);
		return TRUE;
	case WM_COMMAND:
		wCtrl = LOWORD(wParam);
		BOOL enabled;
		switch (wCtrl) {
		case IDC_UNLIMITED:
		case IDC_LIMITED:
			enabled = (wCtrl == IDC_LIMITED);
			enableTimeInput(hDlg, enabled);
			if (!enabled)
				song_length = -1;
			else {
				setFocusAndSelect(hDlg, IDC_MINUTES);
				getTimeInput(hDlg);
			}
			return TRUE;
		case IDC_SILENCE:
			enabled = (IsDlgButtonChecked(hDlg, IDC_SILENCE) == BST_CHECKED);
			EnableWindow(GetDlgItem(hDlg, IDC_SILSECONDS), enabled);
			if (!enabled)
				silence_seconds = -1;
			else {
				setFocusAndSelect(hDlg, IDC_SILSECONDS);
				getSilenceInput(hDlg);
			}
			return TRUE;
		case IDC_MINUTES:
		case IDC_SECONDS:
			if (HIWORD(wParam) == EN_CHANGE && IsDlgButtonChecked(hDlg, IDC_LIMITED) == BST_CHECKED)
				getTimeInput(hDlg);
			return TRUE;
		case IDC_SILSECONDS:
			if (HIWORD(wParam) == EN_CHANGE && IsDlgButtonChecked(hDlg, IDC_SILENCE) == BST_CHECKED)
				getSilenceInput(hDlg);
			return TRUE;
		case IDC_LOOPS:
		case IDC_NOLOOPS:
			play_loops = (IsDlgButtonChecked(hDlg, IDC_LOOPS) == BST_CHECKED);
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
		play_loops = false;
	}
};

static service_factory_single_t<preferences_page_asap> g_preferences_page_asap_factory;


/* Decoding -------------------------------------------------------------- */

class input_asap
{
	service_ptr_t<file> m_file;
	byte module[ASAP_MODULE_MAX];
	ASAP_ModuleInfo module_info;
	ASAP_State asap;

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
		int module_len = m_file->read(module, sizeof(module), p_abort);
		if (!::ASAP_GetModuleInfo(&module_info, p_path, module, module_len))
			throw exception_io_unsupported_format();
		if (p_reason == input_open_info_read)
			return;
		if (!::ASAP_Load(&asap, p_path, module, module_len))
			throw exception_io_unsupported_format();
	}

	t_uint32 get_subsong_count()
	{
		return module_info.songs;
	}

	t_uint32 get_subsong(t_uint32 p_index)
	{
		return p_index;
	}

	void get_info(t_uint32 p_subsong, file_info &p_info, abort_callback &p_abort)
	{
		int duration = module_info.durations[p_subsong];
		if (duration < 0)
			duration = 1000 *  song_length;
		if (play_loops && module_info.loops[p_subsong])
			duration = 1000 * song_length;
		if (duration >= 0)
			p_info.set_length(duration / 1000.0);
		p_info.info_set_int("channels", module_info.channels);
		p_info.info_set_int("subsongs", module_info.songs);
		p_info.meta_set("composer", module_info.author);
		p_info.meta_set("title", module_info.name);
		p_info.meta_set("date", module_info.date);
	}

	t_filestats get_file_stats(abort_callback &p_abort)
	{
		return m_file->get_stats(p_abort);
	}

	void decode_initialize(t_uint32 p_subsong, unsigned p_flags, abort_callback &p_abort)
	{
		int duration = module_info.durations[p_subsong];
		if (duration < 0) {
			if (silence_seconds > 0)
				::ASAP_DetectSilence(&asap, silence_seconds);
			duration = 1000 * song_length;
		}
		if (play_loops && module_info.loops[p_subsong])
			duration = 1000 * song_length;
		::ASAP_PlaySong(&asap, p_subsong, duration);
	}

	bool decode_run(audio_chunk &p_chunk, abort_callback &p_abort)
	{
		int channels = module_info.channels;
		int buffered_bytes = BUFFERED_BLOCKS * channels * (BITS_PER_SAMPLE / 8);
		static
#if BITS_PER_SAMPLE == 8
			byte
#else
			short
#endif
			buffer[BUFFERED_BLOCKS * 2];

		buffered_bytes = ::ASAP_Generate(&asap, buffer, buffered_bytes,
			(ASAP_SampleFormat) BITS_PER_SAMPLE);
		if (buffered_bytes == 0)
			return false;
		p_chunk.set_data_fixedpoint(buffer, buffered_bytes, ASAP_SAMPLE_RATE,
			channels, BITS_PER_SAMPLE,
			channels == 2 ? audio_chunk::channel_config_stereo : audio_chunk::channel_config_mono);
		return true;
	}

	void decode_seek(double p_seconds, abort_callback& p_abort)
	{
		ASAP_Seek(&asap, (int) (p_seconds * 1000));
	}

	bool decode_can_seek()
	{
		return true;
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
	{ "Theta Music Composer 1.x (*.tmc;*.tm8)", "*.TMC;*.TM8" },
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
