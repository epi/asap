/*
 * ASAPShellEx.cpp - ASAP Column Handler and Property Handler shell extensions
 *
 * Copyright (C) 2010-2019  Piotr Fusik
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

/* There are two separate implementations for different Windows versions:
   Column Handler (IColumnProvider) works in Windows XP and 2003 (probably 2000 too and maybe 98).
   Property Handler (IInitializeWithStream+IPropertyStore+IPropertyStoreCapabilities) works in Windows Vista and 7. */

#include <stdio.h>
#define _WIN32_IE 0x500
#include <shlobj.h>
#include <shlwapi.h>

#include "asap-infowriter.hpp"

static const char extensions[][5] =
	{ ".sap", ".cmc", ".cm3", ".cmr", ".cms", ".dmc", ".dlt", ".mpt", ".mpd", ".rmt", ".tmc", ".tm8", ".tm2", ".fc" };

static HINSTANCE g_hDll;
static LONG g_cRef = 0;
static enum { WINDOWS_OLD, WINDOWS_XP, WINDOWS_VISTA } g_windowsVer;

static void DllAddRef(void)
{
	InterlockedIncrement(&g_cRef);
}

static void DllRelease(void)
{
	InterlockedDecrement(&g_cRef);
}

#define CLSID_ASAPMetadataHandler_str "{5AE26367-B5CF-444D-B163-2CBC99B41287}"
static const GUID CLSID_ASAPMetadataHandler =
	{ 0x5ae26367, 0xb5cf, 0x444d, { 0xb1, 0x63, 0x2c, 0xbc, 0x99, 0xb4, 0x12, 0x87 } };

struct CMyPropertyDef
{
	const SHCOLUMNID scid;
	const UINT cChars;
	const DWORD csFlags;
	const LPCWSTR wszTitle;

	void CopyTo(SHCOLUMNINFO *psci) const
	{
		psci->scid = this->scid;
		psci->vt = VT_LPWSTR;
		psci->fmt = LVCFMT_LEFT;
		psci->cChars = this->cChars;
		psci->csFlags = this->csFlags;
		lstrcpyW(psci->wszTitle, this->wszTitle);
		lstrcpyW(psci->wszDescription, this->wszTitle);
	}
};

static const CMyPropertyDef g_propertyDefs[] = {
	{ { FMTID_SummaryInformation, PIDSI_TITLE }, 25, SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW, L"Title" },
	{ { FMTID_SummaryInformation, PIDSI_AUTHOR }, 25, SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW, L"Author" },
	{ { FMTID_MUSIC, PIDSI_ARTIST }, 25, SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW, L"Artist" },
	{ { FMTID_MUSIC, PIDSI_YEAR }, 4, SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW, L"Year" },
	{ { FMTID_AudioSummaryInformation, PIDASI_TIMELENGTH }, 8, SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW, L"Duration" },
	{ { FMTID_AudioSummaryInformation, PIDASI_CHANNEL_COUNT }, 9, SHCOLSTATE_TYPE_INT | SHCOLSTATE_SLOW, L"Channels" },
	{ { CLSID_ASAPMetadataHandler, 1 }, 8, SHCOLSTATE_TYPE_INT | SHCOLSTATE_SLOW, L"Subsongs" },
	{ { CLSID_ASAPMetadataHandler, 2 }, 8, SHCOLSTATE_TYPE_STR | SHCOLSTATE_SLOW, L"PAL/NTSC" }
};
#define N_PROPERTYDEFS ARRAYSIZE(g_propertyDefs)

class CMyLock
{
	const PCRITICAL_SECTION m_pLock;

public:

	CMyLock(PCRITICAL_SECTION pLock) : m_pLock(pLock)
	{
		EnterCriticalSection(pLock);
	}

	~CMyLock()
	{
		LeaveCriticalSection(m_pLock);
	}
};

class CASAPMetadataHandler final : IColumnProvider, IInitializeWithStream, IPropertyStore, IPropertyStoreCapabilities
{
	LONG m_cRef = 1;
	CRITICAL_SECTION m_lock;
	WCHAR m_filename[MAX_PATH];
	bool m_hasInfo = false;
	IStream *m_pstream = nullptr;
	ASAPInfo m_info;

	~CASAPMetadataHandler()
	{
		if (m_pstream != nullptr)
			m_pstream->Release();
		DeleteCriticalSection(&m_lock);
		DllRelease();
	}

	HRESULT LoadFile(LPCWSTR wszFile, IStream *pstream, DWORD grfMode)
	{
		m_hasInfo = false;
		if (m_pstream != nullptr) {
			m_pstream->Release();
			m_pstream = nullptr;
		}

		int cbFilename = WideCharToMultiByte(CP_ACP, 0, wszFile, -1, nullptr, 0, nullptr, nullptr);
		char filename[cbFilename];
		if (WideCharToMultiByte(CP_ACP, 0, wszFile, -1, filename, cbFilename, nullptr, nullptr) <= 0)
			return HRESULT_FROM_WIN32(GetLastError());

		byte module[ASAPInfo::maxModuleLength];
		int module_len;
		if (pstream != nullptr) {
			HRESULT hr = pstream->Read(module, ASAPInfo::maxModuleLength, reinterpret_cast<ULONG *>(&module_len));
			if (FAILED(hr))
				return hr;
		}
		else {
			HANDLE fh = CreateFile(filename, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_SEQUENTIAL_SCAN, nullptr);
			if (fh == INVALID_HANDLE_VALUE)
				return HRESULT_FROM_WIN32(GetLastError());
			if (!ReadFile(fh, module, ASAPInfo::maxModuleLength, reinterpret_cast<LPDWORD>(&module_len), nullptr)) {
				HRESULT hr = HRESULT_FROM_WIN32(GetLastError());
				CloseHandle(fh);
				return hr;
			}
			CloseHandle(fh);
		}

		try {
			m_info.load(filename, module, module_len);
		}
		catch (...) {
			return S_OK;
		}

		if ((grfMode & STGM_READWRITE) != 0) {
			const char *ext = strrchr(filename, '.');
			if (ext != nullptr && _stricmp(ext, ".sap") == 0) {
				m_pstream = pstream;
				pstream->AddRef();
			}
		}
		m_hasInfo = true;
		return S_OK;
	}

	static HRESULT GetInt(PROPVARIANT *pvarData, int i)
	{
		pvarData->vt = VT_UI4;
		pvarData->ulVal = static_cast<ULONG>(i);
		return S_OK;
	}

	static HRESULT GetString(PROPVARIANT *pvarData, const char *s)
	{
		pvarData->vt = VT_BSTR;
		// pvarData->bstrVal = A2BSTR(s); - just don't want dependency on ATL
		int cch = MultiByteToWideChar(CP_ACP, 0, s, -1, nullptr, 0);
		pvarData->bstrVal = SysAllocStringLen(nullptr, cch - 1);
		if (pvarData->bstrVal == nullptr)
			return E_OUTOFMEMORY;
		if (MultiByteToWideChar(CP_ACP, 0, s, -1, pvarData->bstrVal, cch) <= 0)
			return HRESULT_FROM_WIN32(GetLastError());
		return S_OK;
	}

	HRESULT GetAuthors(PROPVARIANT *pvarData, bool vista)
	{
		const char *author = m_info.getAuthor().data();
		if (!vista)
			return GetString(pvarData, author);
		if (author[0] == '\0') {
			pvarData->vt = VT_EMPTY;
			return S_OK;
		}
		// split on " & "
		int i;
		const char *s = author;
		for (i = 1; ; i++) {
			s = strstr(s, " & ");
			if (s == nullptr)
				break;
			s += 3;
		}
		pvarData->vt = VT_VECTOR | VT_LPSTR;
		pvarData->calpstr.cElems = i;
		LPSTR *pElems = static_cast<LPSTR *>(CoTaskMemAlloc(i * sizeof(LPSTR)));
		pvarData->calpstr.pElems = pElems;
		if (pElems == nullptr)
			return E_OUTOFMEMORY;
		s = author;
		for (i = 0; ; i++) {
			const char *e = strstr(s, " & ");
			size_t len = e != nullptr ? e - s : strlen(s);
			pElems[i] = static_cast<LPSTR>(CoTaskMemAlloc(len + 1));
			if (pElems[i] == nullptr)
				return E_OUTOFMEMORY;
			memcpy(pElems[i], s, len);
			pElems[i][len] = '\0';
			if (e == nullptr)
				break;
			s = e + 3;
		}
		return S_OK;
	}

	HRESULT GetData(LPCSHCOLUMNID pscid, PROPVARIANT *pvarData, bool vista)
	{
		if (!m_hasInfo)
			return S_FALSE;

		if (pscid->fmtid == FMTID_SummaryInformation) {
			if (pscid->pid == PIDSI_TITLE)
				return GetString(pvarData, m_info.getTitle().data());
			if (pscid->pid == PIDSI_AUTHOR)
				return GetAuthors(pvarData, vista);
		}
		else if (pscid->fmtid == FMTID_MUSIC) {
			if (pscid->pid == PIDSI_ARTIST)
				return GetAuthors(pvarData, vista);
			if (pscid->pid == PIDSI_YEAR) {
				int year = m_info.getYear();
				if (year < 0) {
					pvarData->vt = VT_EMPTY;
					return S_OK;
				}
				return GetInt(pvarData, year);
			}
		}
		else if (pscid->fmtid == FMTID_AudioSummaryInformation) {
			if (pscid->pid == PIDASI_TIMELENGTH) {
				int duration = m_info.getDuration(m_info.getDefaultSong());
				if (duration < 0) {
					pvarData->vt = VT_EMPTY;
					return S_OK;
				}
				if (g_windowsVer == WINDOWS_OLD) {
					duration /= 1000;
					char timeStr[16];
					sprintf(timeStr, "%02d:%02d:%02d", duration / 3600, duration / 60 % 60, duration % 60);
					return GetString(pvarData, timeStr);
				}
				else {
					pvarData->vt = VT_UI8;
					pvarData->uhVal.QuadPart = 10000ULL * duration;
					return S_OK;
				}
			}
			if (pscid->pid == PIDASI_CHANNEL_COUNT)
				return GetInt(pvarData, m_info.getChannels());
		}
		else if (pscid->fmtid == CLSID_ASAPMetadataHandler) {
			if (pscid->pid == 1)
				return GetInt(pvarData, m_info.getSongs());
			if (pscid->pid == 2)
				return GetString(pvarData, m_info.isNtsc() ? "NTSC" : "PAL");
		}
		return S_FALSE;
	}

	static HRESULT AppendString(char *dest, int *offset, LPCWSTR wszVal)
	{
		int i = *offset;
		while (*wszVal != 0) {
			if (i >= ASAPInfo::maxTextLength) {
				dest[i] = '\0';
				return INPLACE_S_TRUNCATED;
			}
			WCHAR c = *wszVal++;
			if (c < ' ' || c > 'z')
				return E_FAIL;
			dest[i++] = static_cast<char>(c);
		}
		dest[i] = '\0';
		*offset = i;
		return S_OK;
	}

	HRESULT SetString(void (ASAPInfo::*psetfunc)(std::string_view), REFPROPVARIANT propvar)
	{
		char s[ASAPInfo::maxTextLength + 1];
		int offset = 0;
		HRESULT hr = S_OK;
		switch (propvar.vt) {
		case VT_EMPTY:
			s[0] = '\0';
			break;
		case VT_UI4:
			if (sprintf(s, "%lu", propvar.ulVal) != 4)
				return E_FAIL;
			break;
		case VT_LPWSTR:
			hr = AppendString(s, &offset, propvar.pwszVal);
			if (FAILED(hr))
				return hr;
			break;
		case VT_VECTOR | VT_LPWSTR:
			s[0] = '\0';
			for (ULONG i = 0; i < propvar.calpwstr.cElems; i++) {
				if (i > 0) {
					hr = AppendString(s, &offset, L" & ");
					if (FAILED(hr))
						return hr;
				}
				hr = AppendString(s, &offset, propvar.calpwstr.pElems[i]);
				if (FAILED(hr))
					return hr;
			}
			break;
		default:
			return E_NOTIMPL;
		}
		try {
			(m_info.*psetfunc)(s);
			return hr;
		}
		catch (...) {
			return E_FAIL;
		}
	}

public:

	CASAPMetadataHandler()
	{
		DllAddRef();
		InitializeCriticalSection(&m_lock);
		m_filename[0] = '\0';
	}

	STDMETHODIMP QueryInterface(REFIID riid, void **ppv)
	{
		if (riid == IID_IUnknown || riid == IID_IColumnProvider) {
			*ppv = static_cast<IColumnProvider *>(this);
			AddRef();
			return S_OK;
		}
		if (riid == IID_IInitializeWithStream) {
			*ppv = static_cast<IInitializeWithStream *>(this);
			AddRef();
			return S_OK;
		}
		if (riid == IID_IPropertyStore) {
			*ppv = static_cast<IPropertyStore *>(this);
			AddRef();
			return S_OK;
		}
		if (riid == IID_IPropertyStoreCapabilities) {
			*ppv = static_cast<IPropertyStoreCapabilities *>(this);
			AddRef();
			return S_OK;
		}
		*ppv = nullptr;
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

	// IColumnProvider

	STDMETHODIMP Initialize(LPCSHCOLUMNINIT psci)
	{
		return S_OK;
	}

	STDMETHODIMP GetColumnInfo(DWORD dwIndex, SHCOLUMNINFO *psci)
	{
		if (dwIndex >= 0 && dwIndex < N_PROPERTYDEFS) {
			g_propertyDefs[dwIndex].CopyTo(psci);
			return S_OK;
		}
		return S_FALSE;
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
			ext[i] = static_cast<char>(c);
		}
		if (pscd->pwszExt[5] != '\0')
			return S_FALSE;
		ext[3] = '\0';
		if (!ASAPInfo::isOurExt(ext))
			return S_FALSE;

		CMyLock lck(&m_lock);
		if ((pscd->dwFlags & SHCDF_UPDATEITEM) != 0 || lstrcmpW(m_filename, pscd->wszFile) != 0) {
			lstrcpyW(m_filename, pscd->wszFile);
			HRESULT hr = LoadFile(pscd->wszFile, nullptr, STGM_READ);
			if (FAILED(hr))
				return hr;
		}
		return GetData(pscid, reinterpret_cast<PROPVARIANT *>(pvarData), false);
	}

	// IInitializeWithStream

	STDMETHODIMP Initialize(IStream *pstream, DWORD grfMode)
	{
		STATSTG statstg;
		HRESULT hr = pstream->Stat(&statstg, STATFLAG_DEFAULT);
		if (FAILED(hr))
			return hr;
		CMyLock lck(&m_lock);
		hr = LoadFile(statstg.pwcsName, pstream, grfMode);
		CoTaskMemFree(statstg.pwcsName);
		return hr;
	}

	// IPropertyStore

	STDMETHODIMP GetCount(DWORD *cProps)
	{
		CMyLock lck(&m_lock);
		return m_hasInfo ? N_PROPERTYDEFS : 0;
	}

	STDMETHODIMP GetAt(DWORD iProp, PROPERTYKEY *pkey)
	{
		CMyLock lck(&m_lock);
		if (m_hasInfo && iProp >= 0 && iProp < N_PROPERTYDEFS) {
			*pkey = g_propertyDefs[iProp].scid;
			return S_OK;
		}
		return E_INVALIDARG;
	}

	STDMETHODIMP GetValue(REFPROPERTYKEY key, PROPVARIANT *pv)
	{
		CMyLock lck(&m_lock);
		HRESULT hr = GetData(&key, pv, true);
		if (hr == S_FALSE) {
			pv->vt = VT_EMPTY;
			return S_OK;
		}
		return hr;
	}

	STDMETHODIMP SetValue(REFPROPERTYKEY key, REFPROPVARIANT propvar)
	{
		CMyLock lck(&m_lock);
		if (m_pstream == nullptr)
			return STG_E_ACCESSDENIED;
		if (key.fmtid == FMTID_SummaryInformation) {
			if (key.pid == PIDSI_TITLE)
				return SetString(ASAPInfo::setTitle, propvar);
			if (key.pid == PIDSI_AUTHOR)
				return SetString(ASAPInfo::setAuthor, propvar);
		}
		else if (key.fmtid == FMTID_MUSIC) {
			if (key.pid == PIDSI_ARTIST)
				return SetString(ASAPInfo::setAuthor, propvar);
			if (key.pid == PIDSI_YEAR)
				return SetString(ASAPInfo::setDate, propvar);
		}
		return E_FAIL;
	}

	STDMETHODIMP Commit(void)
	{
		CMyLock lck(&m_lock);
		if (m_pstream == nullptr)
			return STG_E_ACCESSDENIED;
		LARGE_INTEGER liZero;
		liZero.LowPart = 0;
		liZero.HighPart = 0;
		HRESULT hr = m_pstream->Seek(liZero, STREAM_SEEK_SET, nullptr);
		if (SUCCEEDED(hr)) {
			byte module[ASAPInfo::maxModuleLength];
			int module_len;
			hr = m_pstream->Read(module, ASAPInfo::maxModuleLength, reinterpret_cast<ULONG *>(&module_len));
			if (SUCCEEDED(hr)) {
				byte output[ASAPInfo::maxModuleLength];
				{
					ASAPWriter writer;
					writer.setOutput(output, 0, sizeof(output));
					module_len = writer.write("dummy.sap", &m_info, module, module_len, false);
				}
				if (module_len < 0)
					hr = E_FAIL;
				else {
					hr = m_pstream->Seek(liZero, STREAM_SEEK_SET, nullptr);
					if (SUCCEEDED(hr)) {
						ULARGE_INTEGER liSize;
						liSize.LowPart = module_len;
						liSize.HighPart = 0;
						hr = m_pstream->SetSize(liSize);
						if (SUCCEEDED(hr)) {
							hr = m_pstream->Write(output, module_len, nullptr);
							if (SUCCEEDED(hr))
								hr = m_pstream->Commit(STGC_DEFAULT);
						}
					}
				}
			}
		}
		m_pstream->Release();
		m_pstream = nullptr;
		return hr;
	}

	// IPropertyStoreCapabilities

	STDMETHODIMP IsPropertyWritable(REFPROPERTYKEY key)
	{
		if (key.fmtid == FMTID_SummaryInformation) {
			if (key.pid == PIDSI_TITLE || key.pid == PIDSI_AUTHOR)
				return S_OK;
		}
		else if (key.fmtid == FMTID_MUSIC) {
			if (key.pid == PIDSI_ARTIST || key.pid == PIDSI_YEAR)
				return S_OK;
		}
		return S_FALSE;
	}
};

class CASAPMetadataHandlerFactory : public IClassFactory
{
public:

	STDMETHODIMP QueryInterface(REFIID riid, void **ppv)
	{
		if (riid == IID_IUnknown || riid == IID_IClassFactory) {
			*ppv = static_cast<IClassFactory *>(this);
			DllAddRef();
			return S_OK;
		}
		*ppv = nullptr;
		return E_NOINTERFACE;
	}

	STDMETHODIMP_(ULONG) AddRef()
	{
		DllAddRef();
		return 2;
	}

	STDMETHODIMP_(ULONG) Release()
	{
		DllRelease();
		return 1;
	}

	STDMETHODIMP CreateInstance(LPUNKNOWN punkOuter, REFIID riid, void **ppv)
	{
		*ppv = nullptr;
		if (punkOuter != nullptr)
			return CLASS_E_NOAGGREGATION;
		CASAPMetadataHandler *punk = new CASAPMetadataHandler;
		if (punk == nullptr)
			return E_OUTOFMEMORY;
		HRESULT hr = punk->QueryInterface(riid, ppv);
		punk->Release();
		return hr;
	}

	STDMETHODIMP LockServer(BOOL fLock)
	{
		if (fLock)
			DllAddRef();
		else
			DllRelease();
		return S_OK;
	};
};

STDAPI_(BOOL) __declspec(dllexport) DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	if (dwReason == DLL_PROCESS_ATTACH) {
		g_hDll = hInstance;
		OSVERSIONINFO osvi;
		osvi.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
		GetVersionEx(&osvi);
		g_windowsVer = WINDOWS_OLD;
		if (osvi.dwPlatformId == VER_PLATFORM_WIN32_NT) {
			if (osvi.dwMajorVersion == 5 && osvi.dwMinorVersion >= 1)
				g_windowsVer = WINDOWS_XP;
			else if (osvi.dwMajorVersion >= 6)
				g_windowsVer = WINDOWS_VISTA;
		}
	}
	return TRUE;
}

static LSTATUS RegSetString(HKEY hKey, LPCSTR lpValueName, LPCSTR lpStr)
{
	return RegSetValueEx(hKey, lpValueName, 0, REG_SZ, reinterpret_cast<const BYTE *>(lpStr), strlen(lpStr) + 1);
}

STDAPI __declspec(dllexport) DllRegisterServer(void)
{
	HKEY hk1;
	if (RegCreateKeyEx(HKEY_CLASSES_ROOT, "CLSID\\" CLSID_ASAPMetadataHandler_str, 0, nullptr, 0, KEY_WRITE, nullptr, &hk1, nullptr) != ERROR_SUCCESS)
		return E_FAIL;
	HKEY hk2;
	if (RegCreateKeyEx(hk1, "InProcServer32", 0, nullptr, 0, KEY_WRITE, nullptr, &hk2, nullptr) != ERROR_SUCCESS) {
		RegCloseKey(hk1);
		return E_FAIL;
	}
	char szModulePath[MAX_PATH];
	if (GetModuleFileName(g_hDll, szModulePath, MAX_PATH) == 0
	 || RegSetString(hk2, nullptr, szModulePath) != ERROR_SUCCESS
	 || RegSetString(hk2, "ThreadingModel", "Both") != ERROR_SUCCESS) {
		RegCloseKey(hk2);
		RegCloseKey(hk1);
		return E_FAIL;
	}
	RegCloseKey(hk2);
	RegCloseKey(hk1);

	if (g_windowsVer == WINDOWS_VISTA) {
		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PropertySystem\\PropertyHandlers", 0, KEY_WRITE, &hk1) != ERROR_SUCCESS)
			return E_FAIL;
		for (LPCSTR ext : extensions) {
			if (RegCreateKeyEx(hk1, ext, 0, nullptr, 0, KEY_WRITE, nullptr, &hk2, nullptr) != ERROR_SUCCESS) {
				RegCloseKey(hk1);
				return E_FAIL;
			}
			if (RegSetString(hk2, nullptr, CLSID_ASAPMetadataHandler_str) != ERROR_SUCCESS) {
				RegCloseKey(hk2);
				RegCloseKey(hk1);
				return E_FAIL;
			}
			RegCloseKey(hk2);
		}
		RegCloseKey(hk1);
	}
	else {
		if (RegCreateKeyEx(HKEY_CLASSES_ROOT, "Folder\\shellex\\ColumnHandlers\\" CLSID_ASAPMetadataHandler_str, 0, nullptr, 0, KEY_WRITE, nullptr, &hk1, nullptr) != ERROR_SUCCESS)
			return E_FAIL;
		RegCloseKey(hk1);
	}

	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", 0, KEY_SET_VALUE, &hk1) != ERROR_SUCCESS)
		return E_FAIL;
	if (RegSetString(hk1, CLSID_ASAPMetadataHandler_str, "ASAP Metadata Handler") != ERROR_SUCCESS) {
		RegCloseKey(hk1);
		return E_FAIL;
	}
	RegCloseKey(hk1);
	return S_OK;
}

STDAPI __declspec(dllexport) DllUnregisterServer(void)
{
	HKEY hk1;
	if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved", 0, KEY_SET_VALUE, &hk1) == ERROR_SUCCESS) {
		RegDeleteValue(hk1, CLSID_ASAPMetadataHandler_str);
		RegCloseKey(hk1);
	}
	if (g_windowsVer == WINDOWS_VISTA) {
		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PropertySystem\\PropertyHandlers", 0, DELETE, &hk1) == ERROR_SUCCESS) {
			for (LPCSTR ext : extensions)
				RegDeleteKey(hk1, ext);
			RegCloseKey(hk1);
		}
	}
	else
		RegDeleteKey(HKEY_CLASSES_ROOT, "Folder\\shellex\\ColumnHandlers\\" CLSID_ASAPMetadataHandler_str);
	RegDeleteKey(HKEY_CLASSES_ROOT, "CLSID\\" CLSID_ASAPMetadataHandler_str "\\InProcServer32");
	RegDeleteKey(HKEY_CLASSES_ROOT, "CLSID\\" CLSID_ASAPMetadataHandler_str);
	return S_OK;
}

STDAPI __declspec(dllexport) DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
	if (ppv == nullptr)
		return E_INVALIDARG;
	if (rclsid == CLSID_ASAPMetadataHandler) {
		static CASAPMetadataHandlerFactory g_ClassFactory;
		return g_ClassFactory.QueryInterface(riid, ppv);
	}
	*ppv = nullptr;
	return CLASS_E_CLASSNOTAVAILABLE;
}

STDAPI __declspec(dllexport) DllCanUnloadNow(void)
{
	return g_cRef == 0 ? S_OK : S_FALSE;
}

static HRESULT DoPropertySchema(LPCSTR funcName)
{
	HRESULT hr = CoInitialize(nullptr);
	if (SUCCEEDED(hr)) {
		WCHAR szSchemaPath[MAX_PATH];
		hr = E_FAIL;
		if (GetModuleFileNameW(g_hDll, szSchemaPath, MAX_PATH)
		 && PathRemoveFileSpecW(szSchemaPath)
		 && PathAppendW(szSchemaPath, L"ASAPShellEx.propdesc")) {
			HMODULE propsysDll = LoadLibrary("propsys.dll");
			if (propsysDll != nullptr) {
				typedef HRESULT (__stdcall *FuncType)(PCWSTR);
				FuncType func = reinterpret_cast<FuncType>(GetProcAddress(propsysDll, funcName));
				if (func != nullptr) {
					hr = func(szSchemaPath);
					if (hr == INPLACE_S_TRUNCATED) // returned on Windows 10, no idea why
						hr = S_OK;
				}
				FreeLibrary(propsysDll);
			}
		}
		CoUninitialize();
	}
	return hr;
}

STDAPI __declspec(dllexport) InstallPropertySchema(void)
{
	HRESULT hr = DoPropertySchema("PSRegisterPropertySchema");
	if (SUCCEEDED(hr))
		SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, nullptr, nullptr);
	return hr;
}

STDAPI __declspec(dllexport) UninstallPropertySchema(void)
{
	return DoPropertySchema("PSUnregisterPropertySchema");
}
