DC = dmd
DFLAGS = -release -inline -O
CITO = cito
RM = rm -f

ASAPCI_SRC = ../cpu6502.ci ../asap.ci ../asapinfo.ci ../pokey.ci ../asap6502.ci
PLAYERS_OBX = players/cmc.obx players/cm3.obx players/cmr.obx players/cms.obx players/dlt.obx players/mpt.obx players/rmt4.obx players/rmt8.obx players/tmc.obx players/tm2.obx

all: asapplay.exe asap2wav.exe

asap2wav.exe: asap2wav.d asapci.d
	$(DC) $(DFLAGS) $^ -of$@

asapplay.exe: asapplay.d waveout.d asapci.d
	$(DC) $(DFLAGS) $^ -of$@

asapci.d: $(ASAPCI_SRC) $(foreach pl,$(PLAYERS_OBX),../$(pl))
	$(CITO) -l d -o $@ $(ASAPCI_SRC) -I ../players

$(foreach pl,$(PLAYERS_OBX),../$(pl)):
	$(MAKE) -C .. $(PLAYERS_OBX)

clean:
	$(RM) asapci.d asapplay.exe asapplay.map asapplay.obj asap2wav.exe asap2wav.map asap2wav.obj

.PHONY: all clean

.DELETE_ON_ERROR:
