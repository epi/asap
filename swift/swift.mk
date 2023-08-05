SWIFTC = $(DO)swiftc -no-color-diagnostics

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "swift.mk")
endif

swift: swift/asap2wav.exe
.PHONY: swift

swift/asap2wav.exe: swift/main.swift swift/asap.swift
	$(SWIFTC) -O -o $@ $^
CLEAN += swift/asap2wav.exe swift/asap2wav.exp swift/asap2wav.lib

swift/asap.swift: $(call src,asap.fu asap6502.fu asapinfo.fu cpu6502.fu pokey.fu) $(ASM6502_PLAYERS_OBX)
	$(FUT)
CLEAN += swift/asap.swift
