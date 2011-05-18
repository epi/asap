XBMC_DLL_LOADER_EXPORTS = ../XBMC/xbmc/cores/DllLoader/exports

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "xbmc.mk")
endif

asap-xbmc: xbmc_asap-i486-linux.so
.PHONY: asap-xbmc

xbmc_asap-i486-linux.so: $(call src,xbmc/xbmc_asap.c asap.[ch])
	$(CC) `cat $(XBMC_DLL_LOADER_EXPORTS)/wrapper.def` $(XBMC_DLL_LOADER_EXPORTS)/wrapper.o
CLEAN += xbmc_asap-i486-linux.so
