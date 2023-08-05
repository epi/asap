# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "javascript.mk")
endif

javascript: javascript/asap.mjs
.PHONY: javascript

javascript/asap.js javascript/asap.mjs: $(call src,asap.fu asap6502.fu asapinfo.fu cpu6502.fu pokey.fu) $(ASM6502_PLAYERS_OBX)
	$(FUT)
CLEAN += javascript/asap.js javascript/asap.mjs
