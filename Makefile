prefix := /usr/local
srcdir := $(dir $(lastword $(MAKEFILE_LIST)))
CC = $(DO)gcc -s -O2 -Wall -o $@ $(if $(filter %.so,$@),-shared -fPIC) $(INCLUDEOPTS) $(filter %.c,$^)
AR = $(DO)ar rc $@ $^
CITO = $(DO)cito -o $@ $(patsubst %,-I %,$(sort $(dir $(filter-out %.ci,$^)))) $(filter %.ci,$^)
INSTALL = install
INSTALL_PROGRAM = mkdir -p $(DESTDIR)$(2) && $(INSTALL) $(1) $(DESTDIR)$(2)/$(1)
INSTALL_DATA = mkdir -p $(DESTDIR)$(2) && $(INSTALL) -m 644 $(1) $(DESTDIR)$(2)/$(1)
ASCIIDOC = $(DO)asciidoc -o - $(1) $< | sed -e "s/527bbd;/c02020;/" | xmllint --valid --nonet -o $@ -
SDL_CFLAGS = `sdl-config --cflags`
SDL_LIBS = `sdl-config --libs`
SEVENZIP = 7z a -mx=9
COPY = $(DO)cp $< $@
ACIDSAP = ../Acid800/out/Release/AcidSAP/standalone

# no user-configurable paths below this line

MAKEFLAGS = -r
ifdef V
DO = mkdir -p $(dir $@) &&
else
DO = @echo $@ && mkdir -p $(dir $@) &&
endif
src = $(addprefix $(srcdir),$(1:.[ch]=.c) $(patsubst %.[ch],%.h,$(filter %.[ch],$(1))))
INCLUDEOPTS = $(patsubst %/,-I%,$(sort $(dir $(filter %.h,$^))))
CLEAN :=
CLEANDIR :=

all: asapconv libasap.a
.PHONY: all

install: install-asapconv install-lib
.PHONY: install

include $(srcdir)6502/6502.mk
include $(srcdir)www/www.mk
include $(srcdir)release/release.mk

# asapconv

asapconv: $(call src,asapconv.c asap.[ch])
	$(CC)
CLEAN += asapconv

install-asapconv: asapconv
	$(call INSTALL_PROGRAM,asapconv,$(prefix)/bin)
.PHONY: install-asapconv

uninstall-asapconv:
	$(RM) $(DESTDIR)$(prefix)/bin/asapconv
.PHONY: uninstall-asapconv

# lib

lib: libasap.a
.PHONY: lib

libasap.a: asap.o
	$(AR)
CLEAN += libasap.a

asap.o: $(call src,asap.[ch])
	$(CC) -c
CLEAN += asap.o

install-lib: libasap.a $(srcdir)asap.h
	$(call INSTALL_DATA,$(srcdir)asap.h,$(prefix)/include)
	$(call INSTALL_DATA,libasap.a,$(prefix)/lib)
.PHONY: install-lib

uninstall-lib:
	$(RM) $(DESTDIR)$(prefix)/include/asap.h $(DESTDIR)$(prefix)/lib/libasap.a
.PHONY: uninstall-lib

# SDL

asap-sdl: $(call src,asap-sdl.c asap.[ch])
	$(CC) $(SDL_CFLAGS) $(SDL_LIBS)
CLEAN += asap-sdl

install-sdl: asap-sdl
	$(call INSTALL_PROGRAM,asap-sdl,$(prefix)/bin)
.PHONY: install-sdl

uninstall-sdl:
	$(RM) $(DESTDIR)$(prefix)/bin/asap-sdl
.PHONY: uninstall-sdl

# asapscan

asapscan: $(srcdir)asapscan.c asap-asapscan.h
	$(CC)
CLEAN += win32/asapscan.exe

asap-asapscan.h: $(call src,asap.ci asap6502.ci asapinfo.ci cpu6502.ci pokey.ci) $(NATIVE_ROUTINES_OBX) | asap-asapscan.c

asap-asapscan.c: $(call src,asap.ci asap6502.ci asapinfo.ci cpu6502.ci pokey.ci) $(NATIVE_ROUTINES_OBX)
	$(CITO) -D ASAPSCAN
CLEAN += asap-asapscan.c asap-asapscan.h

# asap.[ch]

$(srcdir)asap.h: $(call src,asap.ci asap6502.ci asapinfo.ci asapwriter.ci cpu6502.ci flashpack.ci pokey.ci) $(NATIVE_ROUTINES_OBX) 6502/xexb.obx 6502/xexd.obx | $(srcdir)asap.c

$(srcdir)asap.c: $(call src,asap.ci asap6502.ci asapinfo.ci asapwriter.ci cpu6502.ci flashpack.ci pokey.ci) $(NATIVE_ROUTINES_OBX) 6502/xexb.obx 6502/xexd.obx
	$(CITO) -D FLASHPACK

# Acid800

check: asapscan $(ACIDSAP)
	@export passed=0 total=0; \
		for name in $(ACIDSAP)/*.sap; do \
			echo -n \*\ ; ./asapscan -a "$$name"; \
			passed=$$(($$passed+!$$?)); total=$$(($$total+1)); \
		done; \
		echo PASSED $$passed of $$total tests
	@$(MAKE) -C test
	@export passed=0 total=0; \
		for name in test/*.sap; do \
			echo -n \*\ ; ./asapscan -a "$$name"; \
			passed=$$(($$passed+!$$?)); total=$$(($$total+1)); \
		done; \
		echo PASSED $$passed of $$total tests
.PHONY: check

# other

$(srcdir)README.html: $(call src,README INSTALL NEWS CREDITS)
	$(call ASCIIDOC,-a asapsrc -a asapports)

clean:
	$(RM) $(CLEAN)
	$(RM) -r $(CLEANDIR)
.PHONY: clean

.DELETE_ON_ERROR:

include $(srcdir)audacious/audacious.mk
include $(srcdir)moc/moc.mk
include $(srcdir)xbmc/xbmc.mk
include $(srcdir)xmms/xmms.mk

include $(srcdir)csharp/csharp.mk
include $(srcdir)d/d.mk
include $(srcdir)flash/flash.mk
include $(srcdir)java/java.mk
include $(srcdir)javascript/javascript.mk
include $(srcdir)win32/win32.mk
