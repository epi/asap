# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "javascript.mk")
endif

javascript: javascript/asap.mjs
.PHONY: javascript

javascript/asap.js javascript/asap.mjs: $(call src,asap.ci asap6502.ci asapinfo.ci cpu6502.ci pokey.ci) $(ASM6502_PLAYERS_OBX)
	$(CITO)
CLEAN += javascript/asap.js javascript/asap.mjs
