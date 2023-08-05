DC = $(DO)dmd -release -inline -O $(if $(WINDIR),-of$(subst /,\\,$@) $(subst /,\\,$^),-of$@ $^)

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "d.mk")
endif

d: d/asap2wav.exe # d/asapplay.exe
.PHONY: d

d/asap2wav.exe: $(srcdir)d/asap2wav.d d/asap.d
	$(DC)
CLEAN += d/asap2wav.exe d/asap2wav.obj

# d/asapplay.exe: $(call src,d/asapplay.d d/waveout.d d/alsa/pcm.d) d/asap.d
# 	$(DC)
# CLEAN += d/asapplay.exe d/asapplay.obj

d/asap.d: $(call src,asap.fu asap6502.fu asapinfo.fu cpu6502.fu pokey.fu) $(ASM6502_PLAYERS_OBX)
	$(FUT)
CLEAN += d/asap.d
