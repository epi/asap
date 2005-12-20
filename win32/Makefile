ATARI800_SRC_DIR = ..\..\atari800\src
SRC = ..\asap.c $(ATARI800_SRC_DIR)\cpu.c $(ATARI800_SRC_DIR)\mzpokeysnd.c $(ATARI800_SRC_DIR)\pokeysnd.c $(ATARI800_SRC_DIR)\remez.c
CC = cl /nologo /Fe$@ /O2 /GL /fp:fast /W3 /wd4996 /DHAVE_CONFIG_H /DASAP /DSOUND /I . /I .. /I $(ATARI800_SRC_DIR)

all: asap2wav.exe wasap.exe winamp\in_asap.dll

asap2wav.exe: ..\asap2wav.c $(SRC) ..\players.h
	$(CC) ..\asap2wav.c $(SRC) /link /release

wasap.exe: wasap.c $(SRC) wasap.res ..\players.h
	$(CC) wasap.c $(SRC) wasap.res comdlg32.lib shell32.lib user32.lib winmm.lib /link /release

wasap.res: wasap.rc
	rc wasap.rc

winamp\in_asap.dll: winamp\in_asap.c $(SRC) ..\players.h
	$(CC) /LD winamp\in_asap.c $(SRC) user32.lib /link /release

..\players.h: ..\raw2c.pl ..\cmc_0500.raw ..\mpt_0500.raw ..\tmc_0500.raw
	perl $** >$@

clean:
	del ..\players.h;wasap.res;*.obj;winamp\in_asap.exp;winamp\in_asap.lib;asap2wav.exe;wasap.exe;winamp\in_asap.dll
