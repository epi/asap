VERSION = 0.2.1
ATARI800_SRC_DIR = ..\..\atari800\src
ASAP_DIR = ..\..\asap
FOOBAR2000_SDK_DIR = ..\..\foobar2000_SDK

CC = cl /nologo /O2 /GL /GR- /GS- /fp:fast /W3 /wd4996 /DNDEBUG
LINKOPT = /link /release /opt:nowin98
MKLIB = lib /nologo /ltcg /out:$@
ASAP_CC = $(CC) /Fe$@ /DHAVE_CONFIG_H /DASAP /DSOUND /I . /I .. /I $(ATARI800_SRC_DIR)
XASM = xasm /q

COMMON_C = ..\asap.c $(ATARI800_SRC_DIR)\cpu.c $(ATARI800_SRC_DIR)\mzpokeysnd.c $(ATARI800_SRC_DIR)\pokeysnd.c $(ATARI800_SRC_DIR)\remez.c
COMMON_H = config.h ..\asap.h ..\asap_internal.h ..\players.h $(ATARI800_SRC_DIR)\cpu.h $(ATARI800_SRC_DIR)\mzpokeysnd.h $(ATARI800_SRC_DIR)\pokey.h $(ATARI800_SRC_DIR)\pokeysnd.h $(ATARI800_SRC_DIR)\remez.h

all: asap2wav.exe wasap.exe winamp\in_asap.dll

asap2wav.exe: ..\asap2wav.c $(COMMON_C) $(COMMON_H)
	$(ASAP_CC) ..\asap2wav.c $(COMMON_C) $(LINKOPT)

wasap.exe: wasap.c $(COMMON_C) wasap.res $(COMMON_H)
	$(ASAP_CC) wasap.c $(COMMON_C) wasap.res comdlg32.lib shell32.lib user32.lib winmm.lib $(LINKOPT)

wasap.res: wasap.rc wasap.ico play.ico stop.ico
	rc wasap.rc

winamp\in_asap.dll: winamp\in_asap.c $(COMMON_C) $(COMMON_H)
	$(ASAP_CC) /LD winamp\in_asap.c $(COMMON_C) user32.lib $(LINKOPT)

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

# foobar2000

foo: foobar2000\foo_asap.dll

FOOBAR2000_RUNTIME = $(FOOBAR2000_SDK_DIR)\foobar2000\foobar2000_component_client\component_client.cpp foobar2000\pfc.lib foobar2000\utf8api.lib foobar2000\foobar2000_SDK.lib

foobar2000\foo_asap.dll: foobar2000\foo_asap.cpp $(COMMON_C) $(COMMON_H) $(FOOBAR2000_RUNTIME)
	$(ASAP_CC) /LD /DWIN32 /I $(FOOBAR2000_SDK_DIR) foobar2000\foo_asap.cpp $(COMMON_C) $(FOOBAR2000_RUNTIME) $(LINKOPT)

foobar2000\foobar2000_SDK.lib: foobar2000\audio_chunk.obj foobar2000\console.obj foobar2000\dsp.obj foobar2000\guids.obj foobar2000\reader.obj foobar2000\service.obj
	$(MKLIB) $**

{$(FOOBAR2000_SDK_DIR)\foobar2000\SDK\}.cpp{foobar2000\}.obj:
	$(CC) /c /Fo$@ /DWIN32 /I $(FOOBAR2000_SDK_DIR) $**

foobar2000\utf8api.lib: $(FOOBAR2000_SDK_DIR)\foobar2000\utf8api\text_drawing.cpp $(FOOBAR2000_SDK_DIR)\foobar2000\utf8api\utf8.cpp $(FOOBAR2000_SDK_DIR)\foobar2000\utf8api\utf8api.cpp foobar2000\pfc.lib
	$(CC) /LD /Fefoobar2000\utf8api.dll /DUTF8API_EXPORTS /DWIN32 $** advapi32.lib comctl32.lib comdlg32.lib gdi32.lib shell32.lib user32.lib

foobar2000\pfc.lib: foobar2000\byte_order_helper.obj foobar2000\cfg_var.obj foobar2000\mem_block.obj foobar2000\string.obj foobar2000\utf8.obj
	$(MKLIB) $**

{$(FOOBAR2000_SDK_DIR)\pfc\}.cpp{foobar2000\}.obj:
	$(CC) /c /Fo$@ /DWIN32 $**

# delete generated files

clean:
	del ..\players.h;wasap.res;*.obj;asap2wav.exe;wasap.exe;asap2wav.txt
	del foobar2000\*.obj;foobar2000\*.lib;foobar2000\*.exp;foobar2000\*.dll
	del winamp\in_asap.exp;winamp\in_asap.lib;winamp\in_asap.dll

# prepare files for release

asap2wav.txt: ..\asap2wav.1
	groff -m man -r LL=78n -r IN=5n -T ascii -P -cbu ..\asap2wav.1 >$@

dist: all foo ..\configure ..\config.h.in asap2wav.txt ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx
	del ..\..\asap-$(VERSION).tar.gz;..\..\asap2wav-$(VERSION)-win32.zip
	del ..\..\asap-$(VERSION)-win32.zip;..\..\asap-$(VERSION)-winamp.zip
	del ..\..\asap-$(VERSION)-foobar2000.zip
	perl ..\maketar.pl -d asap-$(VERSION) \
		-t $(ASAP_DIR)\asap.c $(ASAP_DIR)\asap.h $(ASAP_DIR)\asap_internal.h \
		$(ASAP_DIR)\asap2wav.c $(ASAP_DIR)\asap2wav.1 \
		$(ASAP_DIR)\players\cmc.asx $(ASAP_DIR)\players\mpt.asx \
		$(ASAP_DIR)\players\rmt.asx $(ASAP_DIR)\players\tmc.asx $(ASAP_DIR)\players\tm2.asx \
		$(ASAP_DIR)\configure.ac $(ASAP_DIR)\Makefile.in \
		$(ASAP_DIR)\config.h.in $(ASAP_DIR)\ChangeLog \
		$(ASAP_DIR)\COPYING $(ASAP_DIR)\CREDITS $(ASAP_DIR)\INSTALL \
		$(ASAP_DIR)\NEWS $(ASAP_DIR)\README $(ASAP_DIR)\TODO \
		$(ASAP_DIR)\win32\config.h $(ASAP_DIR)\win32\Makefile \
		$(ASAP_DIR)\win32\wasap.c $(ASAP_DIR)\win32\README \
		$(ASAP_DIR)\win32\resource.h $(ASAP_DIR)\win32\wasap.rc \
		$(ASAP_DIR)\win32\foobar2000\foo_asap.cpp $(ASAP_DIR)\win32\foobar2000\README \
		$(ASAP_DIR)\win32\winamp\in_asap.c $(ASAP_DIR)\win32\winamp\README \
		$(ASAP_DIR)\win32\winamp\in2.h $(ASAP_DIR)\win32\winamp\out.h \
		$(ASAP_DIR)\xmms\libasap.c \
		$(ATARI800_SRC_DIR)\cpu.c $(ATARI800_SRC_DIR)\mzpokeysnd.c \
		$(ATARI800_SRC_DIR)\cpu.h $(ATARI800_SRC_DIR)\mzpokeysnd.h \
		$(ATARI800_SRC_DIR)\pokey.h \
		$(ATARI800_SRC_DIR)\pokeysnd.c $(ATARI800_SRC_DIR)\remez.c \
		$(ATARI800_SRC_DIR)\pokeysnd.h $(ATARI800_SRC_DIR)\remez.h \
		-b $(ASAP_DIR)\players\cmc.obx $(ASAP_DIR)\players\mpt.obx \
		$(ASAP_DIR)\players\rmt4.obx $(ASAP_DIR)\players\rmt8.obx \
		$(ASAP_DIR)\players\tmc.obx $(ASAP_DIR)\players\tm2.obx \
		$(ASAP_DIR)\win32\play.ico $(ASAP_DIR)\win32\stop.ico \
		$(ASAP_DIR)\win32\wasap.ico \
		-s $(ASAP_DIR)\config.guess $(ASAP_DIR)\config.sub \
		$(ASAP_DIR)\install-sh $(ASAP_DIR)\configure \
		$(ASAP_DIR)\maketar.pl $(ASAP_DIR)\raw2c.pl \
		| 7z a -tgzip -si -mx=9 ..\..\asap-$(VERSION).tar.gz
	copy ..\COPYING copying.txt
	7z a -tzip -mx=9 ..\..\asap2wav-$(VERSION)-win32.zip asap2wav.exe asap2wav.txt copying.txt
	copy /b README + ..\README + ..\CREDITS + ..\NEWS readme.txt
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-win32.zip wasap.exe readme.txt copying.txt
	copy ..\COPYING winamp\copying.txt
	copy /b winamp\README + ..\README + ..\CREDITS + ..\NEWS winamp\in_asap.txt
	cd winamp
	7z a -tzip -mx=9 ..\..\..\asap-$(VERSION)-winamp.zip in_asap.dll in_asap.txt copying.txt
	cd ..
	copy ..\COPYING foobar2000\copying.txt
	copy /b foobar2000\README + ..\README + ..\CREDITS + ..\NEWS foobar2000\foo_asap.txt
	cd foobar2000
	7z a -tzip -mx=9 ..\..\..\asap-$(VERSION)-foobar2000.zip foo_asap.dll foo_asap.txt copying.txt
	cd ..
	del copying.txt;readme.txt
	del winamp\copying.txt;winamp\in_asap.txt
	del foobar2000\copying.txt;foobar2000\foo_asap.txt
