VERSION = 0.1.0
ATARI800_SRC_DIR = ..\..\atari800\src
ASAP_DIR = ..\..\asap
SRC = ..\asap.c $(ATARI800_SRC_DIR)\cpu.c $(ATARI800_SRC_DIR)\mzpokeysnd.c $(ATARI800_SRC_DIR)\pokeysnd.c $(ATARI800_SRC_DIR)\remez.c
CC = cl /nologo /Fe$@ /O2 /GL /fp:fast /W3 /wd4996 /DHAVE_CONFIG_H /DASAP /DSOUND /I . /I .. /I $(ATARI800_SRC_DIR)

all: asap2wav.exe wasap.exe winamp\in_asap.dll

asap2wav.exe: ..\asap2wav.c $(SRC) ..\players.h
	$(CC) ..\asap2wav.c $(SRC) /link /release

wasap.exe: wasap.c $(SRC) wasap.res ..\players.h
	$(CC) wasap.c $(SRC) wasap.res comdlg32.lib shell32.lib user32.lib winmm.lib /link /release

wasap.res: wasap.rc wasap.ico play.ico stop.ico
	rc wasap.rc

winamp\in_asap.dll: winamp\in_asap.c $(SRC) ..\players.h
	$(CC) /LD winamp\in_asap.c $(SRC) user32.lib /link /release

..\players.h: ..\raw2c.pl ..\cmc_0500.raw ..\mpt_0500.raw ..\tmc_0500.raw
	perl $** >$@

clean:
	del ..\players.h;wasap.res;*.obj;asap2wav.exe;wasap.exe
	del winamp\in_asap.exp;winamp\in_asap.lib;winamp\in_asap.dll

dist: all ..\configure ..\config.h.in
	del ..\..\asap-$(VERSION)-src.tar.gz;..\..\asap2wav-$(VERSION)-win32.zip
	del ..\..\asap-$(VERSION)-win32.zip;..\..\asap-$(VERSION)-winamp.zip
	perl ..\maketar.pl -d asap-$(VERSION) \
		-t $(ASAP_DIR)\asap.c $(ASAP_DIR)\asap.h \
		$(ASAP_DIR)\asap_internal.h $(ASAP_DIR)\asap2wav.c \
		$(ASAP_DIR)\configure.ac $(ASAP_DIR)\Makefile.in \
		$(ASAP_DIR)\config.h.in \
		$(ASAP_DIR)\ChangeLog $(ASAP_DIR)\COPYING $(ASAP_DIR)\TODO \
		$(ASAP_DIR)\win32\config.h $(ASAP_DIR)\win32\Makefile \
		$(ASAP_DIR)\win32\wasap.c \
		$(ASAP_DIR)\win32\resource.h $(ASAP_DIR)\win32\wasap.rc \
		$(ASAP_DIR)\win32\winamp\in_asap.c \
		$(ASAP_DIR)\win32\winamp\in2.h $(ASAP_DIR)\win32\winamp\out.h \
		$(ATARI800_SRC_DIR)\cpu.c $(ATARI800_SRC_DIR)\mzpokeysnd.c \
		$(ATARI800_SRC_DIR)\cpu.h $(ATARI800_SRC_DIR)\mzpokeysnd.h \
		$(ATARI800_SRC_DIR)\pokey.h \
		$(ATARI800_SRC_DIR)\pokeysnd.c $(ATARI800_SRC_DIR)\remez.c \
		$(ATARI800_SRC_DIR)\pokeysnd.h $(ATARI800_SRC_DIR)\remez.h \
		-b $(ASAP_DIR)\cmc_0500.raw $(ASAP_DIR)\mpt_0500.raw \
		$(ASAP_DIR)\tmc_0500.raw \
		$(ASAP_DIR)\win32\play.ico $(ASAP_DIR)\win32\stop.ico \
		$(ASAP_DIR)\win32\wasap.ico \
		-s $(ASAP_DIR)\config.guess $(ASAP_DIR)\config.sub \
		$(ASAP_DIR)\install-sh $(ASAP_DIR)\configure \
		$(ASAP_DIR)\maketar.pl $(ASAP_DIR)\raw2c.pl \
		| 7z a -tgzip -si -mx=9 ..\..\asap-$(VERSION)-src.tar.gz
	7z a -tzip -mx=9 ..\..\asap2wav-$(VERSION)-win32.zip asap2wav.exe
	7z a -tzip -mx=9 ..\..\asap-$(VERSION)-win32.zip wasap.exe
	cd winamp
	7z a -tzip -mx=9 ..\..\..\asap-$(VERSION)-winamp.zip in_asap.dll
