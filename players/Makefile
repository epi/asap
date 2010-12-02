PERL ?= perl
XASM ?= xasm -q -o $@
MADS ?= mads -s -o:$@
MKDIRS ?= mkdir -p

TOP_DIR ?= ..
PLAYERS_OBX_DIR ?= $(TOP_DIR)/players
PLAYERS = cmc cm3 cms dlt mpt rmt4 rmt8 tmc tm2
PLAYERS_OBX = $(PLAYERS:%=$(PLAYERS_OBX_DIR)/%.obx)

$(TOP_DIR)/players.h: $(TOP_DIR)/files2anylang.pl $(PLAYERS_OBX)
	$(PERL) $^ >$@

$(PLAYERS_OBX_DIR):
	$(MKDIRS) $@

$(PLAYERS_OBX_DIR)/cmc.obx: $(TOP_DIR)/players/cmc.asx $(PLAYERS_OBX_DIR)
	$(XASM) -d CM3=0 $<

$(PLAYERS_OBX_DIR)/cm3.obx: $(TOP_DIR)/players/cmc.asx $(PLAYERS_OBX_DIR)
	$(XASM) -d CM3=1 $<

$(PLAYERS_OBX_DIR)/dlt.obx: $(TOP_DIR)/players/dlt.as8 $(PLAYERS_OBX_DIR)
	$(MADS) -c $<

$(PLAYERS_OBX_DIR)/rmt4.obx: $(TOP_DIR)/players/rmt.asx $(PLAYERS_OBX_DIR)
	$(XASM) -d STEREOMODE=0 $<

$(PLAYERS_OBX_DIR)/rmt8.obx: $(TOP_DIR)/players/rmt.asx $(PLAYERS_OBX_DIR)
	$(XASM) -d STEREOMODE=1 $<

$(PLAYERS_OBX_DIR)/%.obx: $(TOP_DIR)/players/%.asx $(PLAYERS_OBX_DIR)
	$(XASM) $<
