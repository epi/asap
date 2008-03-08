CSC = csc /nologo /o+
PP = cpp -P -I..
XASM = xasm /q

COMMON_C = ..\asap.c ..\acpu.c ..\apokeysnd.c
COMMON_H = ..\asap.h ..\asap_internal.h
PLAYERS_OBX = ..\players\cmc.obx ..\players\mpt.obx ..\players\rmt4.obx ..\players\rmt8.obx ..\players\tmc.obx ..\players\tm2.obx 

all: asap2wav.exe

asap2wav.exe: asap2wav.cs ASAP.cs $(PLAYERS_OBX)
	$(CSC) /out:asap2wav.exe \
	/res:..\players\cmc.obx,cmc /res:..\players\mpt.obx,mpt \
	/res:..\players\rmt4.obx,rmt4 /res:..\players\rmt8.obx,rmt8 \
	/res:..\players\tmc.obx,tmc /res:..\players\tm2.obx,tm2 \
	asap2wav.cs ASAP.cs

ASAP.cs: ASAP.ppcs $(COMMON_C) $(COMMON_H)
	$(PP) ASAP.ppcs > ASAP.cs

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

clean:
	del asap2wav.exe
	del ASAP.cs
	del ..\players\*.obx
