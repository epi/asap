CSC = csc -nologo -o+ -out:$@
PP = cpp -P -I..
XASM = xasm -q -o $@

all: asap2wav.exe

asap2wav.exe: asap2wav.cs ASAP.cs ../players/cmc.obx ../players/cm3.obx ../players/cms.obx ../players/mpt.obx ../players/rmt4.obx ../players/rmt8.obx ../players/tmc.obx ../players/tm2.obx
	$(CSC) \
	-res:../players/cmc.obx,cmc -res:../players/cm3.obx,cm3 -res:../players/cms.obx,cms \
	-res:../players/mpt.obx,mpt \
	-res:../players/rmt4.obx,rmt4 -res:../players/rmt8.obx,rmt8 \
	-res:../players/tmc.obx,tmc -res:../players/tm2.obx,tm2 \
	asap2wav.cs ASAP.cs

ASAP.cs: ASAP.ppcs ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h
	$(PP) ASAP.ppcs > $@

../players/%.obx: ../players/%.asx
	$(XASM) $<

../players/cmc.obx: ../players/cmc.asx
	$(XASM) -d CM3=0 $<

../players/cm3.obx: ../players/cmc.asx
	$(XASM) -d CM3=1 $<

../players/rmt4.obx: ../players/rmt.asx
	$(XASM) -d STEREOMODE=0 $<

../players/rmt8.obx: ../players/rmt.asx
	$(XASM) -d STEREOMODE=1 $<

clean:
	rm -f asap2wav.exe ASAP.cs ../players/*.obx

.DELETE_ON_ERROR:
