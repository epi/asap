VERSION = 0.4.0
ASAP_DIR = ..\..\asap
FOOBAR2000_SDK_DIR = ..\..\foobar2000_SDK

CC = cl /nologo /O2 /GL /GR- /GS- /fp:fast /W3 /wd4996 /DNDEBUG
LINKOPT = /link /release /opt:nowin98
MKLIB = lib /nologo /ltcg /out:$@
ASAP_CC = $(CC) /Fe$@ /I . /I ..
XASM = xasm /q
RC = rc /fo $@
CLARM = clarm /nologo /O2 /W3 /DNDEBUG /DARM /D_ARM /DARM_ /DARMV4 /D_WIN32_WCE=300 /DWIN32 /DUNDER_CE=300 /DUNICODE /D_UNICODE
PP = cl /nologo /EP
JAVAC = javac -classpath ..\java -d ..\java
JAR = jar

COMMON_C = ..\asap.c ..\acpu.c ..\apokeysnd.c
COMMON_H = ..\asap.h ..\asap_internal.h ..\players.h

all: asap2wav.exe wasap.exe in_asap.dll gspasap.dll

..\players.h: ..\raw2c.pl ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx
	perl $** >$@

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

wasap.exe: wasap\wasap.c $(COMMON_C) wasap\wasap.res $(COMMON_H) wasap\resource.h
	$(ASAP_CC) wasap\wasap.c $(COMMON_C) wasap\wasap.res comdlg32.lib shell32.lib user32.lib winmm.lib $(LINKOPT)

wasap\wasap.res: wasap\wasap.rc ..\asap.h wasap\resource.h wasap\wasap.ico wasap\play.ico wasap\stop.ico
	$(RC) wasap\wasap.rc

# Winamp

in_asap.dll: winamp\in_asap.c $(COMMON_C) settings.c winamp\in_asap.res $(COMMON_H) settings.h
	$(ASAP_CC) /LD winamp\in_asap.c $(COMMON_C) settings.c winamp\in_asap.res user32.lib $(LINKOPT)

winamp\in_asap.res: settings.rc ..\asap.h settings.h
	$(RC) /D WINAMP settings.rc

# foobar2000

foo: foo_asap.dll

FOOBAR2000_RUNTIME = $(FOOBAR2000_SDK_DIR)\foobar2000\foobar2000_component_client\component_client.cpp foobar2000\foobar2000_SDK.lib foobar2000\pfc.lib $(FOOBAR2000_SDK_DIR)\foobar2000\shared\shared.lib

foo_asap.dll: foobar2000\foo_asap.cpp $(COMMON_C) foobar2000\foo_asap.res $(COMMON_H) settings.h $(FOOBAR2000_RUNTIME)
	$(ASAP_CC) /LD /DWIN32 /DUNICODE /EHsc /I $(FOOBAR2000_SDK_DIR) foobar2000\foo_asap.cpp $(COMMON_C) foobar2000\foo_asap.res $(FOOBAR2000_RUNTIME) user32.lib $(LINKOPT)

foobar2000\foobar2000_SDK.lib: foobar2000\abort_callback.obj foobar2000\audio_chunk.obj foobar2000\audio_chunk_channel_config.obj foobar2000\file_info.obj foobar2000\filesystem.obj foobar2000\guids.obj foobar2000\preferences_page.obj foobar2000\replaygain_info.obj foobar2000\service.obj
	$(MKLIB) $**

{$(FOOBAR2000_SDK_DIR)\foobar2000\SDK\}.cpp{foobar2000\}.obj:
	$(CC) /c /Fo$@ /DWIN32 /DUNICODE /EHsc /I $(FOOBAR2000_SDK_DIR) $**

foobar2000\pfc.lib: foobar2000\cfg_var.obj foobar2000\guid.obj foobar2000\other.obj foobar2000\string.obj foobar2000\string_conv.obj foobar2000\utf8.obj
	$(MKLIB) $**

{$(FOOBAR2000_SDK_DIR)\pfc\}.cpp{foobar2000\}.obj:
	$(CC) /c /Fo$@ /DWIN32 /DUNICODE /EHsc $**

foobar2000\foo_asap.res: settings.rc ..\asap.h settings.h
	$(RC) /D FOOBAR2000 settings.rc

# GSPlayer

gspasap.dll: ..\gsplayer\gspasap.c $(COMMON_C) settings.c gspasap.res ..\gsplayer\gspasap.def $(COMMON_H) settings.h
	$(ASAP_CC) /LD ..\gsplayer\gspasap.c $(COMMON_C) settings.c gspasap.res ..\gsplayer\gspasap.def user32.lib $(LINKOPT)

gspasap.res: settings.rc ..\asap.h settings.h
	$(RC) /D GSPLAYER settings.rc

# GSPlayer - Windows CE

wince: ..\gsplayer\gspasap.dll

..\gsplayer\gspasap.dll: ..\gsplayer\gspasap.c $(COMMON_C) settings.c ..\gsplayer\gspasap.res ..\gsplayer\gspasap.def $(COMMON_H) settings.h
	$(CLARM) /Fe$@ /DHAVE_CONFIG_H /DASAP /DSOUND /I . /I .. \
		/LD ..\gsplayer\gspasap.c $(COMMON_C) settings.c ..\gsplayer\gspasap.res ..\gsplayer\gspasap.def /link /release /SUBSYSTEM:WINDOWSCE,3.00

..\gsplayer\gspasap.res: settings.rc ..\asap.h settings.h
	$(RC) /D _WIN32_WCE /D GSPLAYER settings.rc

# asapscan

asapscan.exe: ..\asapscan.c $(COMMON_C) $(COMMON_H)
	$(ASAP_CC) ..\asapscan.c $(COMMON_C) $(LINKOPT)

# Java

java: ..\java\ASAP2WAV.class ..\java\asap_applet.jar

..\java\ASAP2WAV.class: ..\java\ASAP2WAV.java ..\java\net\sf\asap\ASAP.class
	$(JAVAC) ..\java\ASAP2WAV.java

..\java\asap_applet.jar: ..\java\ASAPApplet.class ..\java\net\sf\asap\ASAP.class \
	..\java\net\sf\asap\ASAP_ModuleInfo.class ..\java\net\sf\asap\ASAP_State.class ..\java\net\sf\asap\PokeyState.class
	$(JAR) cf $@ -C ..\java ASAPApplet.class -C ..\java net\sf\asap

..\java\ASAPApplet.class: ..\java\ASAPApplet.java ..\java\net\sf\asap\ASAP.class
	$(JAVAC) ..\java\ASAPApplet.java

..\java\net\sf\asap\ASAP.class: ..\java\ASAP.java ..\java\net\sf\asap\ASAP_ModuleInfo.class
	$(JAVAC) ..\java\ASAP.java

..\java\ASAP.java: ..\java\ASAP.ppjava $(COMMON_C) $(COMMON_H)
	$(PP) /I .. ..\java\ASAP.ppjava > ..\java\ASAP.java

..\java\net\sf\asap\ASAP_ModuleInfo.class: ..\java\ASAP_ModuleInfo.java
	$(JAVAC) ..\java\ASAP_ModuleInfo.java

# delete generated files

clean:
	del ..\players\*.obx;..\players.h
	del *.txt;*.obj;*.exp;*.lib
	del asap2wav.exe
	del wasap\wasap.res;wasap.exe
	del winamp\in_asap.res;in_asap.dll
	del foobar2000\*.obj;foobar2000\*.lib;foobar2000\foo_asap.res;foo_asap.dll
	del gspasap.dll;gspasap.res
	del ..\gsplayer\gspasap.dll;..\gsplayer\gspasap.res
	del asapscan.exe
	del ..\java\*.class;..\java\ASAP.java

# prepare files for release

copying.txt: ..\COPYING
	copy $** $@

credits.txt: ..\CREDITS
	copy $** $@

news.txt: ..\NEWS
	copy $** $@

readme.txt: ..\README
	copy $** $@

todo.txt: ..\TODO
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

dist: all foo wince ..\configure copying.txt credits.txt news.txt readme.txt todo.txt \
	asap2wav.txt wasap.txt in_asap.txt foo_asap.txt gspasap.txt \
	..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx
	cd ..
	del ..\asap-$(VERSION).tar.gz
	del ..\asap-$(VERSION)-win32.zip
	del ..\asap-$(VERSION)-wince-arm.zip
	perl maketar.pl -d asap-$(VERSION) \
		-t asap.c asap.h asap_internal.h acpu.c apokeysnd.c \
		asap2wav.c asap2wav.1 asapscan.c \
		players\cmc.asx players\mpt.asx players\rmt.asx players\tmc.asx players\tm2.asx \
		configure.ac Makefile.in \
		ChangeLog COPYING CREDITS INSTALL NEWS README TODO \
		gsplayer\gspasap.c gsplayer\mapplugin.h \
		gsplayer\gspasap.def gsplayer\gspasap.txt \
		java\ASAP.ppjava java\ASAP_ModuleInfo.java java\ASAP2WAV.java \
		win32\Makefile \
		win32\settings.c win32\settings.h win32\settings.rc \
		win32\foobar2000\foo_asap.cpp win32\foobar2000\README \
		win32\wasap\wasap.c win32\wasap\README \
		win32\wasap\resource.h win32\wasap\wasap.rc \
		win32\winamp\in_asap.c win32\winamp\README \
		win32\winamp\in2.h win32\winamp\out.h \
		xmms\libasap.c xmms\libasap.map \
		-b players\cmc.obx players\mpt.obx \
		players\rmt4.obx players\rmt8.obx \
		players\tmc.obx players\tm2.obx \
		win32\wasap\play.ico win32\wasap\stop.ico win32\wasap\wasap.ico \
		-s install-sh configure chksap.pl maketar.pl raw2c.pl \
		| 7z a -tgzip -si -mx=9 ..\asap-$(VERSION).tar.gz
	cd win32
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-win32.zip \
		copying.txt credits.txt news.txt readme.txt todo.txt \
		asap2wav.txt wasap.txt in_asap.txt foo_asap.txt gspasap.txt \
		asap2wav.exe wasap.exe in_asap.dll foo_asap.dll gspasap.dll
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-wince-arm.zip \
		copying.txt credits.txt news.txt readme.txt todo.txt \
		..\gsplayer\gspasap.dll ..\gsplayer\gspasap.txt
