# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "python.mk")
endif

python: python/asap.py
.PHONY: python

python/asap.py: $(call src,asap.fu asap6502.fu asapinfo.fu cpu6502.fu pokey.fu) $(ASM6502_PLAYERS_OBX)
	$(FUT)
CLEAN += python/asap.py
