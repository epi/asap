VERSION = 1.2.2
ASAP_DIR = ..\..\asap
FOOBAR2000_SDK_DIR = ..\..\foobar2000_SDK
DSHOW_BASECLASSES_DIR = "C:\Program Files\Microsoft SDKs\Windows\v6.1\Samples\Multimedia\DirectShow\BaseClasses"
WINCE_PATH = C:\Program Files\Microsoft Visual Studio 9.0\VC\ce\bin\x86_arm;C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE;C:\Program Files\Microsoft Visual Studio 9.0\VC\bin
WINCE_SDK_DIR = C:\Program Files\Microsoft Visual Studio 9.0\SmartDevices\SDK\PocketPC2003
WINCE_INCLUDE = $(WINCE_SDK_DIR)\Include
WINCE_LIB=$(WINCE_SDK_DIR)\Lib\armv4;C:\Program Files\Microsoft Visual Studio 9.0\VC\ce\lib\armv4;C:\Program Files\Microsoft Visual Studio 9.0\VC\lib

CC = cl /nologo /O2 /GL /GR- /GS- /W3 /wd4996 /DNDEBUG
LINKOPT = /link /release
MKLIB = lib /nologo /ltcg /out:$@
ASAP_CC = $(CC) /Fe$@ /I . /I ..
ASAP_CC71 = cl /nologo /O2 /GL /GR- /W3 /DNDEBUG /Fe$@ /I . /I ..
ASAP_GCC = mingw32-gcc -s -O2 -Wall -o $@ -I. -I..
ASAP_GPP = mingw32-g++ -s -O2 -Wall -o $@ -I. -I..
WINDRES = windres -o $@
PERL = perl
XASM = xasm /q /o:$@
RC = rc /fo $@
WINCE_CL = cl /nologo /O2 /GS- /W3 /wd4996 /DNDEBUG /DARM /D_WIN32_WCE=300 /DWIN32 /DUNDER_CE=300 /DUNICODE /D_UNICODE
GMAKE = mingw32-make
CRLF = $(PERL) -pe 1
ASCIIDOC = asciidoc -o $@ -a doctime
ASCIIDOC_POSTPROCESS = $(PERL) -pi.bak -e "s/527bbd;/c02020;/;END{unlink '$@.bak'}" $@

COMMON_C = ..\asap.c ..\acpu.c ..\apokeysnd.c
COMMON_H = ..\asap.h ..\asap_internal.h ..\players.h

all: asap2wav.exe wasap.exe in_asap.dll gspasap.dll asap_dsf.dll ASAP_Apollo.dll apokeysnd.dll

..\players.h: ..\raw2c.pl ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx
	$(PERL) $** >$@

..\players\cmc.obx: ..\players\cmc.asx
	$(XASM) $**

..\players\mpt.obx: ..\players\mpt.asx
	$(XASM) $**

..\players\rmt4.obx: ..\players\rmt.asx
	$(XASM) /d:STEREOMODE=0 $**

..\players\rmt8.obx: ..\players\rmt.asx
	$(XASM) /d:STEREOMODE=1 $**

..\players\tmc.obx: ..\players\tmc.asx
	$(XASM) $**

..\players\tm2.obx: ..\players\tm2.asx
	$(XASM) $**

# ASAP2WAV

asap2wav.exe: ..\asap2wav.c $(COMMON_C) $(COMMON_H)
	$(ASAP_GCC) ..\asap2wav.c $(COMMON_C)

# WASAP

wasap.exe: wasap\wasap.c $(COMMON_C) gui.c wasap\wasap-res.o $(COMMON_H) gui.h
	$(ASAP_GCC) -DWASAP wasap\wasap.c $(COMMON_C) gui.c wasap\wasap-res.o -lcomdlg32 -lwinmm

wasap\wasap-res.o: gui.rc ..\asap.h gui.h wasap\wasap.ico wasap\play.ico wasap\stop.ico
	$(WINDRES) -DWASAP gui.rc

# Winamp

in_asap.dll: winamp\in_asap.c $(COMMON_C) gui.c winamp\in_asap-res.o $(COMMON_H) gui.h winamp\in2.h winamp\ipc_pe.h winamp\wa_ipc.h
	$(ASAP_GCC) -shared -DWINAMP winamp\in_asap.c $(COMMON_C) gui.c winamp\in_asap-res.o -lcomdlg32

winamp\in_asap-res.o: gui.rc ..\asap.h gui.h
	$(WINDRES) -DWINAMP gui.rc

# foobar2000

foo: foo_asap.dll

FOOBAR2000_RUNTIME = $(FOOBAR2000_SDK_DIR)\foobar2000\foobar2000_component_client\component_client.cpp foobar2000\foobar2000_SDK.lib foobar2000\pfc.lib $(FOOBAR2000_SDK_DIR)\foobar2000\shared\shared.lib

foo_asap.dll: foobar2000\foo_asap.cpp $(COMMON_C) foobar2000\foo_asap.res $(COMMON_H) gui.h $(FOOBAR2000_RUNTIME)
	$(ASAP_CC) /DFOOBAR2000 /LD /DWIN32 /DUNICODE /EHsc /I $(FOOBAR2000_SDK_DIR) foobar2000\foo_asap.cpp $(COMMON_C) foobar2000\foo_asap.res $(FOOBAR2000_RUNTIME) user32.lib $(LINKOPT)

foobar2000\foobar2000_SDK.lib: foobar2000\abort_callback.obj foobar2000\audio_chunk.obj foobar2000\audio_chunk_channel_config.obj foobar2000\file_info.obj foobar2000\filesystem.obj foobar2000\guids.obj foobar2000\mem_block_container.obj foobar2000\preferences_page.obj foobar2000\replaygain_info.obj foobar2000\service.obj
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
	set Path=$(WINCE_PATH)
	set INCLUDE=$(WINCE_INCLUDE)
	set LIB=$(WINCE_LIB)
	$(WINCE_CL) /Fe$@ /DGSPLAYER /I . /I .. /LD ..\gsplayer\gspasap.c $(COMMON_C) gui.c ..\gsplayer\gspasap.res ..\gsplayer\gspasap.def coredll.lib corelibc.lib /link /release

..\gsplayer\gspasap.res: gui.rc ..\asap.h gui.h
	$(RC) /D _WIN32_WCE /D GSPLAYER gui.rc

# DirectShow

DSHOW_BASECLASSES = $(DSHOW_BASECLASSES_DIR)\amfilter.cpp $(DSHOW_BASECLASSES_DIR)\combase.cpp $(DSHOW_BASECLASSES_DIR)\dllentry.cpp $(DSHOW_BASECLASSES_DIR)\dllsetup.cpp $(DSHOW_BASECLASSES_DIR)\mtype.cpp $(DSHOW_BASECLASSES_DIR)\source.cpp $(DSHOW_BASECLASSES_DIR)\wxlist.cpp $(DSHOW_BASECLASSES_DIR)\wxutil.cpp

asap_dsf.dll: dshow\asap_dsf.cpp $(COMMON_C) asap_dsf.res dshow\asap_dsf.def $(COMMON_H)
	$(ASAP_CC) /DDSHOW /LD /I$(DSHOW_BASECLASSES_DIR) dshow\asap_dsf.cpp $(COMMON_C) $(DSHOW_BASECLASSES) asap_dsf.res dshow\asap_dsf.def advapi32.lib ole32.lib oleaut32.lib strmiids.lib user32.lib winmm.lib $(LINKOPT)

asap_dsf.res: gui.rc ..\asap.h
	$(RC) /D DSHOW gui.rc

# Apollo

ASAP_Apollo.dll: apollo\ASAP_Apollo.cpp $(COMMON_C) gui.c ASAP_Apollo-res.o $(COMMON_H) gui.h
	$(ASAP_GPP) -shared -DAPOLLO apollo\ASAP_Apollo.cpp $(COMMON_C) gui.c ASAP_Apollo-res.o -lcomdlg32

ASAP_Apollo-res.o: gui.rc ..\asap.h gui.h
	$(WINDRES) -DAPOLLO gui.rc

# XBMC

xbmc: xbmc_asap.dll

xbmc_asap.dll: ..\xbmc\xbmc_asap.c $(COMMON_C) xbmc_asap.res $(COMMON_H)
	$(ASAP_CC71) /LD /MD ..\xbmc\xbmc_asap.c $(COMMON_C) $(LINKOPT)

xbmc_asap.res: gui.rc ..\asap.h
	$(RC) /D XBMC gui.rc

# APokeySound

apokeysnd.dll: ..\apokeysnd.c apokeysnd-res.o ..\asap.h ..\asap_internal.h
	$(ASAP_GCC) -shared -DAPOKEYSND ..\apokeysnd.c apokeysnd-res.o

apokeysnd-res.o: gui.rc ..\asap.h
	$(WINDRES) -DAPOKEYSND gui.rc

# asapscan

asapscan.exe: ..\asapscan.c $(COMMON_C) $(COMMON_H)
	$(ASAP_GCC) -DASAPSCAN ..\asapscan.c $(COMMON_C)

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
	del *.txt;*.html;*.dll;*.exe;*.obj;*.exp;*.lib;*.res;*.o
	del wasap\wasap-res.o;wasap.exe
	del winamp\in_asap-res.o;in_asap.dll
	del foobar2000\*.obj;foobar2000\*.lib;foobar2000\foo_asap.res
	del install_dsf.bat;uninstall_dsf.bat
	del ..\csharp\asap2wav.exe;..\csharp\ASAP.cs
	del ..\players\*.obx;..\players.h;..\README.html
	$(GMAKE) -C ../java clean

# prepare files for release

COPYING.txt: ..\COPYING
	$(CRLF) $** >$@

..\README.html: ..\README ..\NEWS ..\CREDITS ..\INSTALL
	$(ASCIIDOC) -a asapsrc ..\README
	$(ASCIIDOC_POSTPROCESS)

README_Java.html: ..\README ..\NEWS ..\CREDITS ..\java\USAGE
	$(ASCIIDOC) -a asapjava="(included in this binary package)" ..\README
	$(ASCIIDOC_POSTPROCESS)

README_Windows.html: ..\README ..\NEWS ..\CREDITS USAGE
	$(ASCIIDOC) -a asapwin="(included in this binary package)" ..\README
	$(ASCIIDOC_POSTPROCESS)

README_WindowsCE.html: ..\README ..\NEWS ..\CREDITS
	$(ASCIIDOC) -a asapwince="(included in this binary package)" ..\README
	$(ASCIIDOC_POSTPROCESS)

install_dsf.bat:
	echo regsvr32 asap_dsf.dll>$@

uninstall_dsf.bat:
	echo regsvr32 /u asap_dsf.dll>$@

srcdist: ..\configure ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx ..\README.html
	cd ..
	del ..\asap-$(VERSION).tar.gz
	perl maketar.pl -d asap-$(VERSION) \
		-t asap.c asap.h asap_internal.h acpu.c apokeysnd.c \
		asap2wav.c asapscan.c \
		players\cmc.asx players\mpt.asx players\rmt.asx players\tmc.asx players\tm2.asx \
		configure.ac Makefile.in \
		ChangeLog COPYING CREDITS INSTALL NEWS README README.html \
		csharp\ASAP.ppcs csharp\asap2wav.cs csharp\Makefile \
		gsplayer\gspasap.c gsplayer\mapplugin.h gsplayer\gspasap.def \
		java\ASAP.ppjava java\ASAP_ModuleInfo.java \
		java\ASAP2WAV.java java\asap2wav.MF \
		java\ASAPApplet.java \
		java\ASAPMIDlet.java java\asap_midlet.MF \
		java\mf2jad.pl java\Makefile \
		java\USAGE \
		moc\libasap_decoder.c \
		win32\Makefile \
		win32\USAGE \
		win32\gui.c win32\gui.h win32\gui.rc \
		win32\apollo\ASAP_Apollo.cpp win32\apollo\InputPlugin.h \
		win32\dshow\asap_dsf.cpp win32\dshow\asap_dsf.def \
		win32\foobar2000\foo_asap.cpp \
		win32\wasap\wasap.c \
		win32\winamp\in_asap.c \
		win32\winamp\in2.h win32\winamp\out.h \
		win32\winamp\ipc_pe.h win32\winamp\wa_ipc.h \
		xbmc\xbmc_asap.c \
		xmms\libasap.c xmms\libasap.map \
		-b players\cmc.obx players\mpt.obx \
		players\rmt4.obx players\rmt8.obx \
		players\tmc.obx players\tm2.obx \
		win32\wasap\play.ico win32\wasap\stop.ico win32\wasap\wasap.ico \
		-s install-sh configure chksap.pl maketar.pl raw2c.pl \
		| 7z a -tgzip -si -mx=9 ..\asap-$(VERSION).tar.gz
	cd win32

dist: all foo wince java xbmc srcdist \
	COPYING.txt README_Java.html README_Windows.html README_WindowsCE.html \
	install_dsf.bat uninstall_dsf.bat
	del ..\..\asap-$(VERSION)-java.zip
	del ..\..\asap-$(VERSION)-win32.zip
	del ..\..\asap-$(VERSION)-wince-arm.zip
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-java.zip \
		COPYING.txt README_Java.html \
		..\java\asap2wav.jar ..\java\asap_applet.jar ..\java\asap_midlet.jad ..\java\asap_midlet.jar
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-win32.zip \
		COPYING.txt README_Windows.html \
		install_dsf.bat uninstall_dsf.bat \
		asap2wav.exe wasap.exe in_asap.dll foo_asap.dll gspasap.dll asap_dsf.dll ASAP_Apollo.dll apokeysnd.dll xbmc_asap.dll
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-wince-arm.zip \
		COPYING.txt README_WindowsCE.html ..\gsplayer\gspasap.dll
