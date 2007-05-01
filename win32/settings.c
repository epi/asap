/*
 * settings.c - settings dialog box for Winamp and GSPlayer plugins
 *
 * Copyright (C) 2007  Piotr Fusik
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

#include "asap.h"
#include "settings.h"

static int song_length = -1;
static BOOL play_loops = FALSE;

static void enableTimeInput(HWND hDlg, BOOL enable)
{
	EnableWindow(GetDlgItem(hDlg, IDC_MINUTES), enable);
	EnableWindow(GetDlgItem(hDlg, IDC_SECONDS), enable);
}

static INT_PTR CALLBACK settingsDialogProc(HWND hDlg, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
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
		CheckRadioButton(hDlg, IDC_LOOPS, IDC_NOLOOPS, play_loops ? IDC_LOOPS : IDC_NOLOOPS);
		return TRUE;
	case WM_COMMAND:
		if (HIWORD(wParam) == BN_CLICKED) {
			WORD wCtrl = LOWORD(wParam);
			switch (wCtrl) {
			case IDC_UNLIMITED:
			case IDC_LIMITED:
				CheckRadioButton(hDlg, IDC_UNLIMITED, IDC_LIMITED, wCtrl);
				enableTimeInput(hDlg, wCtrl == IDC_LIMITED);
				if (wCtrl == IDC_LIMITED) {
					HWND hMinutes = GetDlgItem(hDlg, IDC_MINUTES);
					SetFocus(hMinutes);
					SendMessage(hMinutes, EM_SETSEL, 0, -1);
				}
				return TRUE;
			case IDC_LOOPS:
			case IDC_NOLOOPS:
				CheckRadioButton(hDlg, IDC_LOOPS, IDC_NOLOOPS, wCtrl);
				return TRUE;
			case IDOK:
				if (IsDlgButtonChecked(hDlg, IDC_UNLIMITED) == BST_CHECKED)
					song_length = -1;
				else {
					BOOL minutesTranslated;
					BOOL secondsTranslated;
					UINT minutes = GetDlgItemInt(hDlg, IDC_MINUTES, &minutesTranslated, FALSE);
					UINT seconds = GetDlgItemInt(hDlg, IDC_SECONDS, &secondsTranslated, FALSE);
					if (!minutesTranslated || !secondsTranslated) {
						MessageBox(hDlg, "Invalid number", "Error", MB_OK | MB_ICONERROR);
						return TRUE;
					}
					song_length = (int) (60 * minutes + seconds);
				}
				play_loops = (IsDlgButtonChecked(hDlg, IDC_LOOPS) == BST_CHECKED);
				// FALLTHROUGH
			case IDCANCEL:
				EndDialog(hDlg, wCtrl);
				return TRUE;
			}
		}
		break;
	default:
		break;
	}
	return FALSE;
}

void settingsDialog(HINSTANCE hInstance, HWND hwndParent)
{
	DialogBox(hInstance, MAKEINTRESOURCE(IDD_SETTINGS), hwndParent, settingsDialogProc);
}

int getSubsongSeconds(const ASAP_ModuleInfo *module_info, int song)
{
	int seconds = module_info->durations[song];
	if (seconds <= 0)
		seconds = song_length;
	else if (play_loops && module_info->loops[song])
		seconds = song_length;
	return seconds;
}
