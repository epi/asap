CC = arm-wince-mingw32ce-gcc -s -O2 -Wall -o $@ -I.. -I../win32
WINDRES = arm-wince-mingw32ce-windres -o $@
PERL = perl
XASM = xasm -q -o $@

COMMON_C = ../asap.c ../acpu.c ../apokeysnd.c
COMMON_H = ../asap.h ../asap_internal.h ../players.h

gspasap.dll: gspasap.c $(COMMON_C) ../win32/gui.c gspasap-res.o $(COMMON_H) ../win32/gui.h
	$(CC) -shared -DGSPLAYER gspasap.c $(COMMON_C) ../win32/gui.c gspasap-res.o

gspasap-res.o: ../win32/gui.rc ../asap.h ../win32/gui.h
	$(WINDRES) -D_WIN32_WCE -DGSPLAYER ../win32/gui.rc

../players.h: ../raw2c.pl ../players/cmc.obx ../players/mpt.obx ../players/rmt4.obx ../players/rmt8.obx ../players/tmc.obx ../players/tm2.obx
	$(PERL) $^ >$@

../players/%.obx: ../players/%.asx
	$(XASM) $<

../players/rmt4.obx: ../players/rmt.asx
	$(XASM) -d STEREOMODE=0 $<

../players/rmt8.obx: ../players/rmt.asx
	$(XASM) -d STEREOMODE=1 $<

clean:
	rm -f gspasap.dll gspasap-res.o ../players.h ../players/*.obx

.DELETE_ON_ERROR:
