# MinGW for most ports
WIN32_CC = $(DO)mingw32-gcc $(WIN32_CARGS)
WIN32_CXX = $(DO)mingw32-g++ -static $(WIN32_CARGS)
WIN32_WINDRES = $(DO)windres -o $@ $<

# Microsoft compiler for Windows Media Player and foobar2000
DSHOW_BASECLASSES_DIR = "C:/Program Files/Microsoft SDKs/Windows/v7.1/Samples/multimedia/directshow/baseclasses"
FOOBAR2000_SDK_DIR = ../foobar2000_SDK
WIN32_CL = $(DO)cl -GR- -GS- -wd4996 -DNDEBUG $(WIN32_CLARGS)
LINKOPT = -link -release
WIN32_MKLIB = $(DO)lib -nologo -ltcg -out:$@ $^

# MinGW x64
WIN64_CC = $(DO)x86_64-w64-mingw32-gcc $(WIN32_CARGS)
WIN64_CXX = $(DO)x86_64-w64-mingw32-g++ -static $(WIN32_CARGS)
WIN64_WINDRES = $(DO)x86_64-w64-mingw32-windres -o $@ $<

# old Microsoft compiler for XBMC plugin
WIN32_CL71 = $(DO)cl -GR- -DNDEBUG $(WIN32_CLARGS)
 
# gcc for Windows Mobile
WINCE_CC = $(DO)arm-mingw32ce-gcc -s -O2 -Wall -o $@ $(INCLUDEOPTS) $(filter-out %.h,$^)
WINCE_WINDRES = $(DO)arm-mingw32ce-windres -o $@ -D_WIN32_WCE $<
 
# Microsoft compiler for Windows Mobile
WINCE_VC = "C:/Program Files (x86)/Microsoft Visual Studio 9.0/VC/ce"
WINCE_CL = $(DO)$(WINCE_VC)/bin/x86_arm/cl -DUNICODE -DUNDER_CE -D_ARM_ $(WIN32_CLARGS)
WINCE_SDK = "C:/Program Files (x86)/Windows Mobile 5.0 SDK R2/PocketPC"
WINCE_CABWIZ = "C:/Program Files (x86)/Windows Mobile 6 SDK/Tools/CabWiz/cabwiz.exe"

# Windows Installer XML
CANDLE = $(DO)candle -nologo -o $@
LIGHT = $(DO)light -nologo -o $@

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "win32.mk")
endif

comma = ,
WIN32_CARGS = -s -O2 -Wall -Wl,--nxcompat -o $@ $(if $(filter %.dll,$@),-shared -Wl$(comma)-subsystem$(comma)windows) $(INCLUDEOPTS) $(filter-out %.h,$^)
WIN32_CLARGS = -nologo -O2 -GL -W3 $(if $(filter %.obj,$@),-c -Fo$@,-Fe$@) $(if $(filter %.dll,$@),-LD) $(INCLUDEOPTS) $(filter-out %.h,$^)

mingw: $(addprefix win32/,asapconv.exe libasap.a asapscan.exe wasap.exe ASAP_Apollo.dll bass_asap.dll gspasap.dll in_asap.dll xmp-asap.dll apokeysnd.dll ASAPShellEx.dll)
.PHONY: mingw

wince: $(addprefix win32/wince/,wasap.exe gspasap.dll asap_dsf.dll)
.PHONY: wince

# asapconv

win32/asapconv.exe: $(call src,asapconv.c asap.[ch])
	$(WIN32_CC) -DHAVE_LIBMP3LAME -DHAVE_LIBMP3LAME_DLL
CLEAN += win32/asapconv.exe

win32/asapconv-static-lame.exe: $(call src,asapconv.c asap.[ch])
	$(WIN32_CC) -DHAVE_LIBMP3LAME -lmp3lame
CLEAN += win32/asapconv-static-lame.exe

win32/asapconv-no-lame.exe: $(call src,asapconv.c asap.[ch])
	$(WIN32_CC)
CLEAN += win32/asapconv-no-lame.exe

win32/x64/asapconv.exe: $(call src,asapconv.c asap.[ch])
	$(WIN64_CC)
CLEAN += win32/x64/asapconv.exe

# lib

win32/libasap.a: win32/asap.o
	$(AR)
CLEAN += win32/libasap.a

win32/asap.o: $(call src,asap.[ch])
	$(WIN32_CC) -c
CLEAN += win32/asap.o

win32/asap.lib: win32/asap.obj
	$(WIN32_MKLIB)
CLEAN += win32/asap.lib

win32/asap.obj: $(call src,asap.[ch])
	$(WIN32_CL)
CLEAN += win32/asap.obj

# SDL

win32/asap-sdl.exe: $(call src,asap-sdl.c asap.[ch])
	$(WIN32_CC) -lmingw32 -lSDLmain -lSDL
CLEAN += win32/asap-sdl.exe

# asapscan

win32/asapscan.exe: $(srcdir)asapscan.c asap-asapscan.h
	$(WIN32_CC)
CLEAN += win32/asapscan.exe

# WASAP

win32/wasap.exe: $(call src,win32/wasap/wasap.[ch] asap.[ch] win32/info_dlg.[ch]) win32/wasap/wasap-res.o
	$(WIN32_CC) -Wl,-subsystem,windows -DWASAP -lcomctl32 -lcomdlg32 -lwinmm
CLEAN += win32/wasap.exe

win32/wasap/wasap-res.o: $(call src,win32/gui.rc asap.h win32/info_dlg.h win32/wasap/wasap.h win32/wasap/wasap.ico win32/wasap/play.ico win32/wasap/stop.ico)
	$(WIN32_WINDRES) -DWASAP
CLEAN += win32/wasap/wasap-res.o

win32/wince/wasap.exe: $(call src,win32/wasap/wasap.[ch] asap.[ch] win32/info_dlg.[ch]) win32/wince/wasap-res.o 
	$(WINCE_CC) -DWASAP
CLEAN += win32/wince/wasap.exe

win32/wince/wasap-res.o: $(call src,win32/gui.rc asap.h win32/info_dlg.h win32/wasap/wasap.h win32/wasap/wasap.ico win32/wasap/play.ico win32/wasap/stop.ico)
	$(WINCE_WINDRES) -DWASAP
CLEAN += win32/wince/wasap-res.o

# Apollo

win32/ASAP_Apollo.dll: $(call src,win32/apollo/ASAP_Apollo.cpp asap.[ch] win32/info_dlg.[ch] win32/settings_dlg.[ch] win32/apollo/InputPlugin.h) win32/apollo/ASAP_Apollo-res.o
	$(WIN32_CXX) -DAPOLLO -lcomctl32 -lcomdlg32
CLEAN += win32/ASAP_Apollo.dll

win32/apollo/ASAP_Apollo-res.o: $(call src,win32/gui.rc asap.h win32/info_dlg.h win32/settings_dlg.h)
	$(WIN32_WINDRES) -DAPOLLO
CLEAN += win32/apollo/ASAP_Apollo-res.o

# BASS

win32/bass_asap.dll: $(call src,win32/bass/bass_asap.c asap.[ch] win32/bass/bass-addon.h win32/bass/bass.h win32/bass/bass.lib) win32/bass/bass_asap-res.o
	$(WIN32_CC) -Wl,--kill-at -DBASS
CLEAN += win32/bass_asap.dll

win32/bass/bass_asap-res.o: $(call src,win32/gui.rc asap.h)
	$(WIN32_WINDRES) -DBASS
CLEAN += win32/bass/bass_asap-res.o

# DirectShow

DSHOW_BASECLASSES = $(patsubst %,$(DSHOW_BASECLASSES_DIR)/%.cpp,amfilter combase dllentry dllsetup mtype source wxlist wxutil)

win32/asap_dsf.dll: $(call src,win32/dshow/asap_dsf.cpp asap.[ch] win32/dshow/asap_dsf.def) win32/dshow/asap_dsf.res
	$(WIN32_CL) $(DSHOW_BASECLASSES) -Fowin32/dshow/ -DDSHOW -I$(DSHOW_BASECLASSES_DIR) advapi32.lib ole32.lib oleaut32.lib strmiids.lib user32.lib winmm.lib $(LINKOPT)
CLEAN += win32/asap_dsf.dll win32/asap_dsf.exp win32/asap_dsf.lib win32/dshow/*.obj

win32/dshow/asap_dsf.res: $(call src,win32/gui.rc asap.h)
	$(WIN32_WINDRES) -DDSHOW
CLEAN += win32/dshow/asap_dsf.res

win32/wince/asap_dsf.dll: $(call src,win32/dshow/asap_dsf.cpp asap.[ch] win32/dshow/asap_dsf.def) win32/wince/asap_dsf.res
	$(WINCE_CL) -Fowin32/wince/ -Zc:wchar_t- -I$(WINCE_SDK)/Include/Armv4i ole32.lib strmbase.lib strmiids.lib uuid.lib -link -subsystem:windowsce,4.02 -release -libpath:$(WINCE_VC)/lib/armv4i -libpath:$(WINCE_SDK)/Lib/ARMV4I
CLEAN += win32/wince/asap_dsf.dll win32/wince/asap_dsf.exp win32/wince/asap_dsf.lib win32/wince/asap_dsf.obj win32/wince/asap.obj

win32/wince/asap_dsf.res: $(call src,win32/gui.rc asap.h)
	$(WINCE_WINDRES) -DDSHOW
CLEAN += win32/wince/asap_dsf.res

win32/install_dsf.bat:
	$(DO)echo regsvr32 asap_dsf.dll >$@
CLEAN += win32/install_dsf.bat

win32/uninstall_dsf.bat:
	$(DO)echo regsvr32 /u asap_dsf.dll >$@
CLEAN += win32/uninstall_dsf.bat

# foobar2000

FOOBAR2000_RUNTIME = $(FOOBAR2000_SDK_DIR)/foobar2000/foobar2000_component_client/component_client.cpp win32/foobar2000/foobar2000_SDK.lib win32/foobar2000/pfc.lib $(FOOBAR2000_SDK_DIR)/foobar2000/shared/shared.lib

win32/foo_asap.dll: $(call src,win32/foobar2000/foo_asap.cpp asap.[ch] win32/settings_dlg.[ch]) win32/foobar2000/foo_asap.res $(FOOBAR2000_RUNTIME)
	$(WIN32_CL) -Fowin32/foobar2000/ -DFOOBAR2000 -DWIN32 -DUNICODE -EHsc -I$(FOOBAR2000_SDK_DIR) user32.lib $(LINKOPT)
CLEAN += win32/foo_asap.dll win32/foo_asap.exp win32/foo_asap.lib

win32/foobar2000/foobar2000_SDK.lib: $(patsubst %,win32/foobar2000/%.obj,abort_callback audio_chunk audio_chunk_channel_config console file_info filesystem guids mem_block_container playable_location preferences_page replaygain_info service)
	$(WIN32_MKLIB)
CLEAN += win32/foobar2000/foobar2000_SDK.lib

win32/foobar2000/%.obj: $(FOOBAR2000_SDK_DIR)/foobar2000/SDK/%.cpp
	$(WIN32_CL) -DWIN32 -DUNICODE -EHsc -I$(FOOBAR2000_SDK_DIR)
CLEAN += win32/foobar2000/*.obj

win32/foobar2000/pfc.lib: $(patsubst %,win32/foobar2000/%.obj,cfg_var guid other string string_conv utf8)
	$(WIN32_MKLIB)
CLEAN += win32/foobar2000/pfc.lib

win32/foobar2000/%.obj: $(FOOBAR2000_SDK_DIR)/pfc/%.cpp
	$(WIN32_CL) -DWIN32 -DUNICODE -EHsc

win32/foobar2000/foo_asap.res: $(call src,win32/gui.rc asap.h win32/settings_dlg.h)
	$(WIN32_WINDRES) -DFOOBAR2000
CLEAN += win32/foobar2000/foo_asap.res

# GSPlayer

win32/gspasap.dll: $(call src,win32/gsplayer/gspasap.c asap.[ch] win32/settings_dlg.[ch]) win32/gsplayer/gspasap-res.o
	$(WIN32_CC) -Wl,--kill-at -DGSPLAYER
CLEAN += win32/gspasap.dll

win32/gsplayer/gspasap-res.o: $(call src,win32/gui.rc asap.h win32/settings_dlg.h)
	$(WIN32_WINDRES) -DGSPLAYER
CLEAN += win32/gsplayer/gspasap-res.o

win32/wince/gspasap.dll: $(call src,win32/gsplayer/gspasap.c asap.[ch] win32/settings_dlg.[ch]) win32/wince/gspasap-res.o
	$(WINCE_CC) -shared -DGSPLAYER
CLEAN += win32/wince/gspasap.dll

win32/wince/gspasap-res.o: $(call src,win32/gui.rc asap.h win32/settings_dlg.h)
	$(WINCE_WINDRES) -DGSPLAYER
CLEAN += win32/wince/gspasap-res.o

# Winamp

win32/in_asap.dll: $(call src,win32/winamp/in_asap.c asap.[ch] win32/info_dlg.[ch] win32/settings_dlg.[ch] win32/winamp/in2.h win32/winamp/out.h win32/winamp/ipc_pe.h win32/winamp/wa_ipc.h) win32/winamp/in_asap-res.o
	$(WIN32_CC) -DWINAMP -lcomctl32 -lcomdlg32
CLEAN += win32/in_asap.dll

win32/winamp/in_asap-res.o: $(call src,win32/gui.rc asap.h win32/info_dlg.h win32/settings_dlg.h)
	$(WIN32_WINDRES) -DWINAMP
CLEAN += win32/winamp/in_asap-res.o

# XBMC

win32/xbmc_asap.dll: $(call src,xbmc/xbmc_asap.c asap.[ch]) win32/xbmc/xbmc_asap.res
	$(WIN32_CL71) -Fowin32/xbmc/ -MD $(LINKOPT)
CLEAN += win32/xbmc_asap.dll win32/xbmc_asap.exp win32/xbmc_asap.lib win32/xbmc/xbmc_asap.obj win32/xbmc/asap.obj

win32/xbmc/xbmc_asap.res: $(call src,win32/gui.rc asap.h)
	$(WIN32_WINDRES) -DXBMC
CLEAN += win32/xbmc/xbmc_asap.res

# XMPlay

win32/xmp-asap.dll: $(call src,win32/xmplay/xmp-asap.c asap.[ch] win32/info_dlg.[ch] win32/settings_dlg.[ch] win32/xmplay/xmpin.h win32/xmplay/xmpfunc.h) win32/xmplay/xmp-asap-res.o
	$(WIN32_CC) -Wl,--kill-at -DXMPLAY -lcomctl32 -lcomdlg32
CLEAN += win32/xmp-asap.dll

win32/xmplay/xmp-asap-res.o: $(call src,win32/gui.rc asap.h win32/info_dlg.h win32/settings_dlg.h)
	$(WIN32_WINDRES) -DXMPLAY
CLEAN += win32/xmplay/xmp-asap-res.o

# Raster Music Tracker

win32/apokeysnd.dll: $(call src,win32/rmt/apokeysnd_dll.c) win32/rmt/pokey.c win32/rmt/pokey.h win32/rmt/apokeysnd-res.o
	$(WIN32_CC) --std=c99 -DAPOKEYSND
CLEAN += win32/apokeysnd.dll

win32/rmt/pokey.h: $(srcdir)pokey.ci | win32/rmt/pokey.c

win32/rmt/pokey.c: $(srcdir)pokey.ci
	$(CITO) -l c99 -D APOKEYSND
CLEAN += win32/rmt/pokey.c win32/rmt/pokey.h

win32/rmt/apokeysnd-res.o: $(call src,win32/gui.rc asap.h)
	$(WIN32_WINDRES) -DAPOKEYSND
CLEAN += win32/rmt/apokeysnd-res.o

# ASAPShellEx

win32/ASAPShellEx.dll: $(srcdir)win32/shellex/ASAPShellEx.cpp win32/shellex/asap-infowriter.c win32/shellex/asap-infowriter.h win32/shellex/ASAPShellEx-res.o
	$(WIN32_CXX) -Wl,--kill-at -lole32 -loleaut32 -lshlwapi -luuid
CLEAN += win32/ASAPShellEx.dll

win32/shellex/ASAPShellEx-res.o: $(call src,win32/gui.rc asap.h)
	$(WIN32_WINDRES) -DSHELLEX
CLEAN += win32/shellex/ASAPShellEx-res.o

win32/x64/ASAPShellEx.dll: $(srcdir)win32/shellex/ASAPShellEx.cpp win32/shellex/asap-infowriter.c win32/shellex/asap-infowriter.h win32/x64/ASAPShellEx-res.o
	$(WIN64_CXX) -Wl,--kill-at -lole32 -loleaut32 -lshlwapi -luuid
CLEAN += win32/x64/ASAPShellEx.dll

win32/x64/ASAPShellEx-res.o: $(call src,win32/gui.rc asap.h)
	$(WIN64_WINDRES) -DSHELLEX
CLEAN += win32/x64/ASAPShellEx-res.o

win32/shellex/asap-infowriter.h: $(call src,asapinfo.ci asap6502.ci asapwriter.ci) $(NATIVE_ROUTINES_OBX) | win32/shellex/asap-infowriter.c

win32/shellex/asap-infowriter.c: $(call src,asapinfo.ci asap6502.ci asapwriter.ci) $(NATIVE_ROUTINES_OBX)
	$(CITO)
CLEAN += win32/shellex/asap-infowriter.c win32/shellex/asap-infowriter.h

# setups

win32/setup: release/asap-$(VERSION)-win32.msi
.PHONY: win32/setup

release/asap-$(VERSION)-win32.msi: win32/setup/asap.wixobj release/README_WindowsSetup.html \
	$(call src,win32/wasap/wasap.ico win32/setup/license.rtf win32/setup/asap-banner.jpg win32/setup/asap-dialog.jpg win32/setup/Website.url win32/shellex/ASAPShellEx.propdesc) \
	$(addprefix win32/,asapconv.exe wasap.exe in_asap.dll gspasap.dll ASAP_Apollo.dll xmp-asap.dll bass_asap.dll apokeysnd.dll ASAPShellEx.dll asap_dsf.dll foo_asap.dll xbmc_asap.dll)
	$(LIGHT) -ext WixUIExtension -sice:ICE69 -b win32 -b release -b $(srcdir)win32/setup -b $(srcdir)win32 $<

win32/setup/asap.wixobj: $(srcdir)win32/setup/asap.wxs
	$(CANDLE) -dVERSION=$(VERSION) $<
CLEAN += win32/setup/asap.wixobj

release/README_WindowsSetup.html: $(call src,README win32/USAGE NEWS CREDITS)
	$(call ASCIIDOC,-a asapwin="(included in this binary package)" -a asapsetup)
CLEAN += release/README_WindowsSetup.html

release/asap-shellex-$(VERSION)-win64.msi: win32/x64/asap.wixobj \
	$(call src,win32/wasap/wasap.ico win32/setup/license.rtf win32/setup/asap-banner.jpg win32/setup/asap-dialog.jpg win32/shellex/ASAPShellEx.propdesc) \
	win32/x64/ASAPShellEx.dll
	$(LIGHT) -ext WixUIExtension -sice:ICE69 -b win32 -b $(srcdir)/win32/setup -b $(srcdir)win32 $<

win32/x64/asap.wixobj: $(srcdir)win32/setup/asap.wxs
	$(CANDLE) -arch x64 -dVERSION=$(VERSION) $<
CLEAN += win32/x64/asap.wixobj

release/asap-$(VERSION)-wince-arm.cab: $(srcdir)win32/wince/asap.inf \
	$(addprefix win32/wince/,wasap.exe gspasap.dll asap_dsf.dll)
	$(DO)cd win32/wince && $(WINCE_CABWIZ) '$(shell cygpath -wa $<)' /cpu ARM
	@mv win32/wince/asap.ARM.CAB $@
