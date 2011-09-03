MADS = $(DO)mads -s -o:$@ $<

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "6502.mk")
endif

NATIVE_ROUTINES = cmc cm3 cmr cms dlt mpt rmt4 rmt8 tmc tm2 fc
NATIVE_ROUTINES_OBX = $(NATIVE_ROUTINES:%=6502/%.obx)

6502/cmc.obx: $(srcdir)6502/cmc.asx
	$(XASM) -d CM3=0 -d CMR=0

6502/cm3.obx: $(srcdir)6502/cmc.asx
	$(XASM) -d CM3=1 -d CMR=0

6502/cmr.obx: $(srcdir)6502/cmc.asx
	$(XASM) -d CM3=0 -d CMR=1

6502/dlt.obx: $(srcdir)6502/dlt.as8
	$(MADS) -c

6502/rmt4.obx: $(srcdir)6502/rmt.asx
	$(XASM) -d STEREOMODE=0

6502/rmt8.obx: $(srcdir)6502/rmt.asx
	$(XASM) -d STEREOMODE=1

6502/fc.obx: $(srcdir)6502/fc.as8
	$(MADS)

6502/fp3depk.obx: $(srcdir)6502/fp3depk.asx
	$(XASM) -l 6502/fp3depk.lst

6502/%.obx: $(srcdir)6502/%.asx
	$(XASM)

CLEAN += $(NATIVE_ROUTINES_OBX) 6502/xexb.obx 6502/xexd.obx 6502/fp3depk.obx 6502/fp3depk.lst
