/*
 * ASAPShellEx.cpp - ASAP Column Handler shell extension
 *
 * Copyright (C) 2010  Piotr Fusik
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

#include <malloc.h>
#include <stdio.h>
#define _WIN32_IE 0x500
#include <shlobj.h>

/* missing in MinGW */
extern "C" const FMTID FMTID_SummaryInformation =
	{ 0xf29f85e0, 0x4ff9, 0x1068, { 0xab, 0x91, 0x08, 0x00, 0x2b, 0x27, 0xb3, 0xd9 } };
extern "C" const FMTID FMTID_MUSIC =
	{ 0x56a3372e, 0xce9c, 0x11d2, { 0x9f, 0x0e, 0x00, 0x60, 0x97, 0xc6, 0x86, 0xf6 } };
#define PIDSI_ARTIST          2
#define PIDSI_YEAR            5
extern "C" const FMTID FMTID_AudioSummaryInformation =
	{ 0x64440490, 0x4C8B, 0x11D1, { 0x8B, 0x70, 0x08, 0x00, 0x36, 0xB1, 0x1A, 0x03 } };
#define PIDASI_TIMELENGTH     3
#define PIDASI_CHANNEL_COUNT  7
#define SHCDF_UPDATEITEM      1
/* end missing in MinGW */

#include "asap.h"

static HINSTANCE g_hDll;
static BOOL g_isXPorLater;
	
#define CLSID_ASAPColumnHandler_str "{5AE26367-B5CF-444D-B163-2CBC99B41287}"
static const GUID CLSID_ASAPColumnHandler =
	{ 0x5ae26367, 0xb5cf, 0x444d, { 0xb1, 0x63, 0x2c, 0xbc, 0x99, 0xb4, 0x12, 0x87 } };

class CMyLock
{
	PCRITICAL_SECTION m_pLock;
public:
	CMyLock(PCRITICAL_SECTION pLock)
	{
		m_pLock = pLock;
		EnterCriticalSection(pLock);
	}
	~CMyLock()
	{
		LeaveCriticalSection(m_pLock);
	}
};

class CASAPColumnHandler : public IColumnProvider
{
	LONG m_cRef;
	CRITICAL_SECTION m_lock;
	WCHAR m_filename[MAX_PATH];
	BOOL m_hasInfo;
	ASAP_ModuleInfo m_info;

	static void SetColumnTitle(SHCOLUMNINFO *psci, LPCWSTR title)
	{
		lstrcpyW(psci->wszTitle, title);
		lstrcpyW(psci->wszDescription, title);
	}

	static HRESULT SetString(VARIANT *pvarData, const char *s)
	{
		OLECHAR str[ASAP_INFO_CHARS];
		int i = 0;
		char c;
		do {
			c = s[i];
			str[i++] = c;
		} while (c != '\0');
		pvarData->vt = VT_BSTR;
		pvarData->bstrVal = SysAllocString(str);
		return pvarData->bstrVal != NULL ? S_OK : E_OUTOFMEMORY;
	}

public:
	CASAPColumnHandler() : m_cRef(1), m_hasInfo(FALSE)
	{
		InitializeCriticalSection(&m_lock);
		m_filename[0] = '\0';
	}

	virtual ~CASAPColumnHandler()
	{
		DeleteCriticalSection(&m_lock);
	}

	STDMETHODIMP QueryInterface(REFIID riid, void **ppv)
	{
		if (riid == IID_IUnknown || riid == IID_IColumnProvider) {
			*ppv = (IColumnProvider *) this;
			AddRef();
			return S_OK;
		}
		*ppv = NULL;
		return E_NOINTERFACE;
	}

	STDMETHODIMP_(ULONG) AddRef()
	{
		return InterlockedIncrement(&m_cRef);
	}

	STDMETHODIMP_(ULONG) Release()
	{
		ULONG r = InterlockedDecrement(&m_cRef);
		if (r == 0)
			delete this;
		return r;
	}

	STDMETHODIMP Initialize(LPCSHCOLUMNINIT psci)
	{
		return S_OK;
	}

	STDMETHODIMP GetColumnInfo(DWORD dwIndex, SHCOLUMNINFO *psci)
	{
		psci->vt = VT_LPWSTR;
		psci->fmt = LVCFMT_LEFT;
		psci->csFlags = SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW;
		switch (dwIndex) {
		case 0:
			psci->scid.fmtid = FMTID_SummaryInformation;
			psci->scid.pid = PIDSI_TITLE;
			psci->cChars = 25;
			SetColumnTitle(psci, L"Title");
			return S_OK;
		case 1:
			psci->scid.fmtid = FMTID_SummaryInformation;
			psci->scid.pid = PIDSI_AUTHOR;
			psci->cChars = 25;
			SetColumnTitle(psci, L"Author");
			return S_OK;
		case 2:
			psci->scid.fmtid = FMTID_MUSIC;
			psci->scid.pid = PIDSI_ARTIST;
			psci->cChars = 25;
			SetColumnTitle(psci, L"Artist");
			return S_OK;
		case 3:
			psci->scid.fmtid = FMTID_MUSIC;
			psci->scid.pid = PIDSI_YEAR;
			psci->cChars = 4;
			SetColumnTitle(psci, L"Year");
			return S_OK;
		case 4:
			psci->scid.fmtid = FMTID_AudioSummaryInformation;
			psci->scid.pid = PIDASI_TIMELENGTH;
			psci->cChars = 8;
			SetColumnTitle(psci, L"Duration");
			return S_OK;
		case 5:
			psci->scid.fmtid = FMTID_AudioSummaryInformation;
			psci->scid.pid = PIDASI_CHANNEL_COUNT;
			psci->cChars = 9;
			psci->csFlags = SHCOLSTATE_TYPE_INT | SHCOLSTATE_SLOW;
			SetColumnTitle(psci, L"Channels");
			return S_OK;
		case 6:
			psci->scid.fmtid = CLSID_ASAPColumnHandler;
			psci->scid.pid = 1;
			psci->cChars = 8;
			psci->csFlags = SHCOLSTATE_TYPE_INT | SHCOLSTATE_SLOW;
			SetColumnTitle(psci, L"Subsongs");
			return S_OK;
		default:
			return S_FALSE;
		}
	}

	STDMETHODIMP GetItemData(LPCSHCOLUMNID pscid, LPCSHCOLUMNDATA pscd, VARIANT *pvarData)
	{
		if ((pscd->dwFileAttributes & (FILE_ATTRIBUTE_DIRECTORY | FILE_ATTRIBUTE_OFFLINE)) != 0)
			return S_FALSE;
		char ext[4];
		for (int i = 0; i < 3; i++) {
			WCHAR c = pscd->pwszExt[1 + i];
			if (c <= ' ' || c > 'z')
				return S_FALSE;
			ext[i] = (char) c;
		}
		ext[3] = '\0';
		if (!ASAP_IsOurExt(ext))
			return S_FALSE;

		CMyLock lck(&m_lock);
		if ((pscd->dwFlags & SHCDF_UPDATEITEM) != 0 || lstrcmpW(m_filename, pscd->wszFile) != 0) {
			lstrcpyW(m_filename, pscd->wszFile);
			m_hasInfo = FALSE;

			int cch = lstrlenW(pscd->wszFile) + 1;
			char *filename = (char *) alloca(cch * 2);
			if (filename == NULL)
				return E_OUTOFMEMORY;
			if (WideCharToMultiByte(CP_ACP, 0, pscd->wszFile, -1, filename, cch, NULL, NULL) <= 0)
				return HRESULT_FROM_WIN32(GetLastError());

			HANDLE fh = CreateFile(filename, GENERIC_READ, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, NULL);
			if (fh == INVALID_HANDLE_VALUE)
				return HRESULT_FROM_WIN32(GetLastError());
			byte module[ASAP_MODULE_MAX];
			int module_len;
			if (!ReadFile(fh, module, ASAP_MODULE_MAX, (LPDWORD) &module_len, NULL)) {
				HRESULT hr = HRESULT_FROM_WIN32(GetLastError());
				CloseHandle(fh);
				return hr;
			}
			CloseHandle(fh);
			m_hasInfo = ASAP_GetModuleInfo(&m_info, filename, module, module_len);
		}
		if (!m_hasInfo)
			return S_FALSE;

		if (pscid->fmtid == FMTID_SummaryInformation) {
			if (pscid->pid == PIDSI_TITLE)
				return SetString(pvarData, m_info.name);
			if (pscid->pid == PIDSI_AUTHOR)
				return SetString(pvarData, m_info.author);
		}
		else if (pscid->fmtid == FMTID_MUSIC) {
			if (pscid->pid == PIDSI_ARTIST)
				return SetString(pvarData, m_info.author);
			if (pscid->pid == PIDSI_YEAR) {
				char year[5];
				return SetString(pvarData, ASAP_DateToYear(m_info.date, year) ? year : "");
			}
		}
		else if (pscid->fmtid == FMTID_AudioSummaryInformation) {
			if (pscid->pid == PIDASI_TIMELENGTH) {
				int duration = m_info.durations[m_info.default_song];
				if (g_isXPorLater) {
					pvarData->vt = VT_UI8;
					pvarData->ullVal = 10000ULL * duration;
					return S_OK;
				}
				else {
					if (duration < 0)
						return SetString(pvarData, "");
					duration /= 1000;
					char timeStr[16];
					sprintf(timeStr, "%.2d:%.2d:%.2d", duration / 3600, duration / 60 % 60, duration % 60);
					return SetString(pvarData, timeStr);
				}
			}
			if (pscid->pid == PIDASI_CHANNEL_COUNT) {
				pvarData->vt = VT_INT;
				pvarData->intVal = m_info.channels;
				return S_OK;
			}
		}
		else if (pscid->fmtid == CLSID_ASAPColumnHandler) {
			if (pscid->pid == 1) {
				pvarData->vt = VT_INT;
				pvarData->intVal = m_info.songs;
				return S_OK;
			}
		}
		return S_FALSE;
	}
};

class CASAPColumnHandlerFactory : public IClassFactory
{
public:
	STDMETHODIMP QueryInterface(REFIID riid, void **ppv)
	{
		if (riid == IID_IUnknown || riid == IID_IClassFactory) {
			*ppv = (IClassFactory *) this;
			return S_OK;
		}
		*ppv = NULL;
		return E_NOINTERFACE;
	}

	STDMETHODIMP_(ULONG) AddRef()
	{
		return 1;
	}

	STDMETHODIMP_(ULONG) Release()
	{
		return 1;
	}

	STDMETHODIMP CreateInstance(LPUNKNOWN punkOuter, REFIID riid, void **ppv)
	{
		*ppv = NULL;
		if (punkOuter != NULL)
			return CLASS_E_NOAGGREGATION;
		LPUNKNOWN punk = new CASAPColumnHandler;
		if (punk == NULL)
			return E_OUTOFMEMORY;
		HRESULT hr = punk->QueryInterface(riid, ppv);
		punk->Release();
		return hr;
	}

	STDMETHODIMP LockServer(BOOL fLock)
	{
		return E_FAIL;
	};
};

STDAPI_(BOOL) DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	if (dwReason == DLL_PROCESS_ATTACH) {
		g_hDll = hInstance;
		OSVERSIONINFO osvi;
		osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
		GetVersionEx(&osvi);
		g_isXPorLater = (osvi.dwPlatformId == VER_PLATFORM_WIN32_NT
			&& ((osvi.dwMajorVersion == 5 && osvi.dwMinorVersion >= 1)
				|| osvi.dwMajorVersion > 5));
	}
	return TRUE;
}

STDAPI DllRegisterServer(void)
{
	HKEY hk1;
	if (RegCreateKeyEx(HKEY_CLASSES_ROOT, "CLSID\\" CLSID_ASAPColumnHandler_str, 0, NULL, 0, KEY_WRITE, NULL, &hk1, NULL) != ERROR_SUCCESS)
		return E_FAIL;
	HKEY hk2;
	if (RegCreateKeyEx(hk1, "InProcServer32", 0, NULL, 0, KEY_WRITE, NULL, &hk2, NULL) != ERROR_SUCCESS) {
		RegCloseKey(hk1);
		return E_FAIL;
	}
	char szModulePath[MAX_PATH];
	DWORD nModulePathLen = GetModuleFileName(g_hDll, szModulePath, MAX_PATH);
	static const char szThreadingModel[] = "Both";
	if (RegSetValueEx(hk2, NULL, 0, REG_SZ, (CONST BYTE *) szModulePath, nModulePathLen) != ERROR_SUCCESS
	 || RegSetValueEx(hk2, "ThreadingModel", 0, REG_SZ, (CONST BYTE *) szThreadingModel, sizeof(szThreadingModel)) != ERROR_SUCCESS) {
		RegCloseKey(hk2);
		RegCloseKey(hk1);
		return E_FAIL;
	}
	RegCloseKey(hk2);
	RegCloseKey(hk1);
	if (RegCreateKeyEx(HKEY_CLASSES_ROOT, "Folder\\shellex\\ColumnHandlers\\" CLSID_ASAPColumnHandler_str, 0, NULL, 0, KEY_WRITE, NULL, &hk1, NULL) != ERROR_SUCCESS)
		return E_FAIL;
	RegCloseKey(hk1);
	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", 0, KEY_SET_VALUE, &hk1) != ERROR_SUCCESS)
		return E_FAIL;
	static const char szDescription[] = "ASAP Column Handler";
	if (RegSetValueEx(hk1, CLSID_ASAPColumnHandler_str, 0, REG_SZ, (CONST BYTE *) szDescription, sizeof(szDescription)) != ERROR_SUCCESS) {
		RegCloseKey(hk1);
		return E_FAIL;
	}
	RegCloseKey(hk1);
	return S_OK;
}

STDAPI DllUnregisterServer(void)
{
	HKEY hk1;
	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", 0, KEY_SET_VALUE, &hk1) == ERROR_SUCCESS) {
		RegDeleteValue(hk1, CLSID_ASAPColumnHandler_str);
		RegCloseKey(hk1);
	}
	RegDeleteKey(HKEY_CLASSES_ROOT, "Folder\\shellex\\ColumnHandlers\\" CLSID_ASAPColumnHandler_str);
	RegDeleteKey(HKEY_CLASSES_ROOT, "CLSID\\" CLSID_ASAPColumnHandler_str "\\InProcServer32");
	RegDeleteKey(HKEY_CLASSES_ROOT, "CLSID\\" CLSID_ASAPColumnHandler_str);
	return S_OK;
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
	if (ppv == NULL)
		return E_INVALIDARG;
	if (rclsid == CLSID_ASAPColumnHandler && riid == IID_IClassFactory) {
		static CASAPColumnHandlerFactory g_ClassFactory;
		*ppv = &g_ClassFactory;
		return S_OK;
	}
	*ppv = NULL;
	return CLASS_E_CLASSNOTAVAILABLE;
}
