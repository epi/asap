VERSION = 1.1.1
ASAP_DIR = ..\..\asap
FOOBAR2000_SDK_DIR = ..\..\foobar2000_SDK
DSHOW_BASECLASSES_DIR = "C:\Program Files\Microsoft SDKs\Windows\v6.0\Samples\Multimedia\DirectShow\BaseClasses"

CC = cl /nologo /O2 /GL /GR- /GS- /fp:fast /W3 /wd4996 /DNDEBUG
LINKOPT = /link /release /opt:nowin98
MKLIB = lib /nologo /ltcg /out:$@
ASAP_CC = $(CC) /Fe$@ /I . /I ..
PERL = perl
XASM = xasm /q
RC = rc /fo $@
CLARM = clarm /nologo /O2 /W3 /DNDEBUG /DARM /D_WIN32_WCE=300 /DWIN32 /DUNDER_CE=300 /DUNICODE /D_UNICODE
GMAKE = make

COMMON_C = ..\asap.c ..\acpu.c ..\apokeysnd.c
COMMON_H = ..\asap.h ..\asap_internal.h ..\players.h

all: asap2wav.exe wasap.exe in_asap.dll gspasap.dll asap_dsf.dll ASAP_Apollo.dll

..\players.h: ..\raw2c.pl ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx
	$(PERL) $** >$@

..\players\cmc.obx: ..\players\cmc.asx
	$(XASM) $** /o:$@

..\players\mpt.obx: ..\players\mpt.asx
	$(XASM) $** /o:$@

..\players\rmt4.obx: ..\players\rmt.asx
	$(XASM) $** /d:STEREOMODE=0 /o:$@

..\players\rmt8.obx: ..\players\rmt.asx
	$(XASM) $** /d:STEREOMODE=1 /o:$@

..\players\tmc.obx: ..\players\tmc.asx
	$(XASM) $** /o:$@

..\players\tm2.obx: ..\players\tm2.asx
	$(XASM) $** /o:$@

# ASAP2WAV

asap2wav.exe: ..\asap2wav.c $(COMMON_C) $(COMMON_H)
	$(ASAP_CC) ..\asap2wav.c $(COMMON_C) $(LINKOPT)

# WASAP

wasap.exe: wasap\wasap.c $(COMMON_C) gui.c wasap\wasap.res $(COMMON_H) gui.h
	$(ASAP_CC) /DWASAP wasap\wasap.c $(COMMON_C) gui.c wasap\wasap.res comdlg32.lib shell32.lib user32.lib winmm.lib $(LINKOPT)

wasap\wasap.res: gui.rc ..\asap.h gui.h wasap\wasap.ico wasap\play.ico wasap\stop.ico
	$(RC) /D WASAP gui.rc

# Winamp

in_asap.dll: winamp\in_asap.c $(COMMON_C) gui.c winamp\in_asap.res $(COMMON_H) gui.h
	$(ASAP_CC) /DWINAMP /LD winamp\in_asap.c $(COMMON_C) gui.c winamp\in_asap.res comdlg32.lib user32.lib $(LINKOPT)

winamp\in_asap.res: gui.rc ..\asap.h gui.h
	$(RC) /D WINAMP gui.rc

# foobar2000

foo: foo_asap.dll

FOOBAR2000_RUNTIME = $(FOOBAR2000_SDK_DIR)\foobar2000\foobar2000_component_client\component_client.cpp foobar2000\foobar2000_SDK.lib foobar2000\pfc.lib $(FOOBAR2000_SDK_DIR)\foobar2000\shared\shared.lib

foo_asap.dll: foobar2000\foo_asap.cpp $(COMMON_C) foobar2000\foo_asap.res $(COMMON_H) gui.h $(FOOBAR2000_RUNTIME)
	$(ASAP_CC) /DFOOBAR2000 /LD /DWIN32 /DUNICODE /EHsc /I $(FOOBAR2000_SDK_DIR) foobar2000\foo_asap.cpp $(COMMON_C) foobar2000\foo_asap.res $(FOOBAR2000_RUNTIME) user32.lib $(LINKOPT)

foobar2000\foobar2000_SDK.lib: foobar2000\abort_callback.obj foobar2000\audio_chunk.obj foobar2000\audio_chunk_channel_config.obj foobar2000\file_info.obj foobar2000\filesystem.obj foobar2000\guids.obj foobar2000\preferences_page.obj foobar2000\replaygain_info.obj foobar2000\service.obj
	$(MKLIB) $**

{$(FOOBAR2000_SDK_DIR)\foobar2000\SDK\}.cpp{foobar2000\}.obj:
	$(CC) /c /Fo$@ /DWIN32 /DUNICODE /EHsc /I $(FOOBAR2000_SDK_DIR) $**

foobar2000\pfc.lib: foobar2000\cfg_var.obj foobar2000\guid.obj foobar2000\other.obj foobar2000\string.obj foobar2000\string_conv.obj foobar2000\utf8.obj
	$(MKLIB) $**

{$(FOOBAR2000_SDK_DIR)\pfc\}.cpp{foobar2000\}.obj:
	$(CC) /c /Fo$@ /DWIN32 /DUNICODE /EHsc $**

foobar2000\foo_asap.res: gui.rc ..\asap.h gui.h
	$(RC) /D FOOBAR2000 gui.rc

# GSPlayer

gspasap.dll: ..\gsplayer\gspasap.c $(COMMON_C) gui.c gspasap.res ..\gsplayer\gspasap.def $(COMMON_H) gui.h
	$(ASAP_CC) /DGSPLAYER /LD ..\gsplayer\gspasap.c $(COMMON_C) gui.c gspasap.res ..\gsplayer\gspasap.def advapi32.lib user32.lib $(LINKOPT)

gspasap.res: gui.rc ..\asap.h gui.h
	$(RC) /D GSPLAYER gui.rc

# GSPlayer - Windows CE

wince: ..\gsplayer\gspasap.dll

..\gsplayer\gspasap.dll: ..\gsplayer\gspasap.c $(COMMON_C) gui.c ..\gsplayer\gspasap.res ..\gsplayer\gspasap.def $(COMMON_H) gui.h
	$(CLARM) /Fe$@ /DGSPLAYER /DHAVE_CONFIG_H /DASAP /DSOUND /I . /I .. \
		/LD ..\gsplayer\gspasap.c $(COMMON_C) gui.c ..\gsplayer\gspasap.res ..\gsplayer\gspasap.def /link /release /SUBSYSTEM:WINDOWSCE,3.00

..\gsplayer\gspasap.res: gui.rc ..\asap.h gui.h
	$(RC) /D _WIN32_WCE /D GSPLAYER gui.rc

# DirectShow

DSHOW_BASECLASSES = $(DSHOW_BASECLASSES_DIR)\amfilter.cpp $(DSHOW_BASECLASSES_DIR)\combase.cpp $(DSHOW_BASECLASSES_DIR)\dllentry.cpp $(DSHOW_BASECLASSES_DIR)\dllsetup.cpp $(DSHOW_BASECLASSES_DIR)\mtype.cpp $(DSHOW_BASECLASSES_DIR)\source.cpp $(DSHOW_BASECLASSES_DIR)\wxlist.cpp $(DSHOW_BASECLASSES_DIR)\wxutil.cpp

asap_dsf.dll: dshow\asap_dsf.cpp $(COMMON_C) asap_dsf.res dshow\asap_dsf.def $(COMMON_H)
	$(ASAP_CC) /DDSHOW /LD /I$(DSHOW_BASECLASSES_DIR) dshow\asap_dsf.cpp $(COMMON_C) $(DSHOW_BASECLASSES) asap_dsf.res dshow\asap_dsf.def advapi32.lib ole32.lib oleaut32.lib strmiids.lib user32.lib winmm.lib $(LINKOPT)

asap_dsf.res: gui.rc ..\asap.h gui.h
	$(RC) /D DSHOW gui.rc

# Apollo

ASAP_Apollo.dll: apollo\ASAP_Apollo.cpp $(COMMON_C) gui.c ASAP_Apollo.res $(COMMON_H) gui.h
	$(ASAP_CC) /DAPOLLO /LD apollo\ASAP_Apollo.cpp $(COMMON_C) gui.c ASAP_Apollo.res comdlg32.lib user32.lib $(LINKOPT)

ASAP_Apollo.res: gui.rc ..\asap.h gui.h
	$(RC) /D APOLLO gui.rc

# asapscan

asapscan.exe: ..\asapscan.c $(COMMON_C) $(COMMON_H)
	$(ASAP_CC) /DASAPSCAN ..\asapscan.c $(COMMON_C) $(LINKOPT)

# library

lib: asap.lib

asap.lib: $(COMMON_C) $(COMMON_H)
	$(CC) /c $(COMMON_C)
	$(MKLIB) asap.obj acpu.obj apokeysnd.obj

# Java

java:
	$(GMAKE) -C ../java

# delete generated files

clean:
	del ..\players\*.obx;..\players.h
	del *.txt;*.obj;*.exp;*.lib
	del asap2wav.exe
	del wasap\wasap.res;wasap.exe
	del winamp\in_asap.res;in_asap.dll
	del foobar2000\*.obj;foobar2000\*.lib;foobar2000\foo_asap.res;foo_asap.dll
	del gspasap.dll;gspasap.res
	del asap_dsf.dll;asap_dsf.res;install_dsf.bat;uninstall_dsf.bat
	del ..\gsplayer\gspasap.dll;..\gsplayer\gspasap.exp;..\gsplayer\gspasap.lib;..\gsplayer\gspasap.res
	del asapscan.exe
	del csharp\asap2wav.exe;csharp\ASAP.cs
	$(GMAKE) -C ../java clean

# prepare files for release

copying.txt: ..\COPYING
	copy $** $@

credits.txt: ..\CREDITS
	copy $** $@

news.txt: ..\NEWS
	copy $** $@

readme.txt: ..\README
	copy $** $@

asap2wav.txt: ..\asap2wav.1
	groff -m man -r LL=78n -r IN=5n -T ascii -P -cbu ..\asap2wav.1 >$@

wasap.txt: wasap\README
	copy $** $@

in_asap.txt: winamp\README
	copy $** $@

foo_asap.txt: foobar2000\README
	copy $** $@

gspasap.txt: ..\gsplayer\gspasap.txt
	copy $** $@

asap_dsf.txt: dshow\README
	copy $** $@

install_dsf.bat:
	echo regsvr32 asap_dsf.dll >$@

uninstall_dsf.bat:
	echo regsvr32 /u asap_dsf.dll >$@

ASAP_Apollo.txt: apollo\README
	copy $** $@

srcdist: ..\configure ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx
	cd ..
	del ..\asap-$(VERSION).tar.gz
	perl maketar.pl -d asap-$(VERSION) \
		-t asap.c asap.h asap_internal.h acpu.c apokeysnd.c \
		asap2wav.c asap2wav.1 asapscan.c \
		players\cmc.asx players\mpt.asx players\rmt.asx players\tmc.asx players\tm2.asx \
		configure.ac Makefile.in \
		ChangeLog COPYING CREDITS INSTALL NEWS README \
		csharp\ASAP.ppcs csharp\asap2wav.cs csharp\Makefile \
		gsplayer\gspasap.c gsplayer\mapplugin.h \
		gsplayer\gspasap.def gsplayer\gspasap.txt \
		java\ASAP.ppjava java\ASAP_ModuleInfo.java \
		java\ASAP2WAV.java java\asap2wav.MF \
		java\ASAPApplet.java java\asap_applet.txt \
		java\ASAPMIDlet.java java\asap_midlet.MF java\asap_midlet.txt \
		java\mf2jad.pl java\Makefile \
		moc\libasap_decoder.c \
		win32\Makefile \
		win32\gui.c win32\gui.h win32\gui.rc \
		win32\apollo\ASAP_Apollo.cpp win32\apollo\InputPlugin.h win32\apollo\README \
		win32\dshow\asap_dsf.cpp win32\dshow\asap_dsf.def win32\dshow\README \
		win32\foobar2000\foo_asap.cpp win32\foobar2000\README \
		win32\wasap\wasap.c win32\wasap\README \
		win32\winamp\in_asap.c win32\winamp\README \
		win32\winamp\in2.h win32\winamp\out.h \
		win32\winamp\ipc_pe.h win32\winamp\wa_ipc.h \
		xmms\libasap.c xmms\libasap.map \
		-b players\cmc.obx players\mpt.obx \
		players\rmt4.obx players\rmt8.obx \
		players\tmc.obx players\tm2.obx \
		win32\wasap\play.ico win32\wasap\stop.ico win32\wasap\wasap.ico \
		-s install-sh configure chksap.pl maketar.pl raw2c.pl \
		| 7z a -tgzip -si -mx=9 ..\asap-$(VERSION).tar.gz
	cd win32

dist: all foo wince java srcdist copying.txt credits.txt news.txt readme.txt \
	asap2wav.txt wasap.txt in_asap.txt foo_asap.txt gspasap.txt asap_dsf.txt ASAP_Apollo.txt \
	install_dsf.bat uninstall_dsf.bat
	del ..\..\asap-$(VERSION)-java.zip
	del ..\..\asap-$(VERSION)-win32.zip
	del ..\..\asap-$(VERSION)-wince-arm.zip
	cd ..\java
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-java.zip \
		asap2wav.jar asap_applet.jar asap_midlet.jad asap_midlet.jar \
		asap_applet.txt asap_midlet.txt
	cd ..\win32
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-java.zip \
		copying.txt credits.txt news.txt readme.txt asap2wav.txt
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-win32.zip \
		copying.txt credits.txt news.txt readme.txt \
		asap2wav.txt wasap.txt in_asap.txt foo_asap.txt gspasap.txt asap_dsf.txt ASAP_Apollo.txt \
		install_dsf.bat uninstall_dsf.bat \
		asap2wav.exe wasap.exe in_asap.dll foo_asap.dll gspasap.dll asap_dsf.dll ASAP_Apollo.dll
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-wince-arm.zip \
		copying.txt credits.txt news.txt readme.txt \
		..\gsplayer\gspasap.dll ..\gsplayer\gspasap.txt
