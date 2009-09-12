PREFIX = /usr/local

CC = gcc -s -O2 -Wall
AR = ar rc
PERL = perl
XASM = xasm -q
RM = rm -f
ASCIIDOC = asciidoc -a doctime
ASCIIDOC_POSTPROCESS = ${PERL} -pi -e 's/527bbd;/c02020;/'

XMMS_CFLAGS = `xmms-config --cflags`
XMMS_LIBS = `xmms-config --libs`
XMMS_INPUT_PLUGIN_DIR = `xmms-config --input-plugin-dir`
XMMS_USER_PLUGIN_DIR = ${HOME}/.xmms/Plugins
MOC_INCLUDE = ../moc-2.4.4
MOC_PLUGIN_DIR = /usr/local/lib/moc/decoder_plugins
XBMC_DLL_LOADER_EXPORTS = ../XBMC/xbmc/cores/DllLoader/exports

COMMON_C = asap.c acpu.c apokeysnd.c
COMMON_H = asap.h asap_internal.h players.h
PLAYERS_OBX = players/cmc.obx players/cm3.obx players/cms.obx players/mpt.obx players/rmt4.obx players/rmt8.obx players/tmc.obx players/tm2.obx

all: asap2wav libasap.a

asap2wav: asap2wav.c ${COMMON_C} ${COMMON_H}
	${CC} -o $@ -I. asap2wav.c ${COMMON_C}

lib: libasap.a

libasap.a: ${COMMON_C} ${COMMON_H}
	${CC} -c -I. ${COMMON_C}
	${AR} $@ asap.o acpu.o apokeysnd.o

asap-xmms: libasap-xmms.so

libasap.xmms.so: xmms/libasap-xmms.c ${COMMON_C} ${COMMON_H}
	${CC} ${XMMS_CFLAGS} -shared -fPIC -Wl,--version-script=xmms/libasap-xmms.map -o $@ -I. xmms/libasap-xmms.c ${COMMON_C} ${XMMS_LIBS}

asap-moc: libasap_decoder.so

libasap_decoder.so: moc/libasap_decoder.c ${COMMON_C} ${COMMON_H}
	${CC} -shared -fPIC -o $@ -I. -I${MOC_INCLUDE} moc/libasap_decoder.c ${COMMON_C}

asap-xbmc: xbmc_asap-i486-linux.so

xbmc_asap-i486-linux.so: xbmc/xbmc_asap.c ${COMMON_C} ${COMMON_H}
	${CC} -shared -fPIC -o $@ -I. xbmc/xbmc_asap.c `cat ${XBMC_DLL_LOADER_EXPORTS}/wrapper.def` ${XBMC_DLL_LOADER_EXPORTS}/wrapper.o ${COMMON_C}

players.h: raw2c.pl ${PLAYERS_OBX}
	${PERL} raw2c.pl ${PLAYERS_OBX} >$@

players/cmc.obx: players/cmc.asx
	${XASM} -d CM3=0 -o $@ players/cmc.asx

players/cm3.obx: players/cmc.asx
	${XASM} -d CM3=1 -o $@ players/cmc.asx

players/cms.obx: players/cms.asx
	${XASM} -o $@ players/cms.asx

players/mpt.obx: players/mpt.asx
	${XASM} -o $@ players/mpt.asx

players/rmt4.obx: players/rmt.asx
	${XASM} -d STEREOMODE=0 -o $@ players/rmt.asx

players/rmt8.obx: players/rmt.asx
	${XASM} -d STEREOMODE=1 -o $@ players/rmt.asx

players/tmc.obx: players/tmc.asx
	${XASM} -o $@ players/tmc.asx

players/tm2.obx: players/tm2.asx
	${XASM} -o $@ players/tm2.asx

install: install-asap2wav install-lib

install-asap2wav: asap2wav
	${INSTALL_PROGRAM} asap2wav ${PREFIX}/bin/asap2wav

uninstall-asap2wav:
	${RM} ${PREFIX}/bin/asap2wav

install-lib: libasap.a
	${INSTALL_DATA} asap.h ${PREFIX}/include/asap.h
	${INSTALL_DATA} libasap.a ${PREFIX}/lib/libasap.a

uninstall-lib:
	${RM} ${PREFIX}/include/asap.h ${PREFIX}/lib/libasap.a

install-xmms: libasap-xmms.so
	${INSTALL_PROGRAM} libasap-xmms.so ${XMMS_INPUT_PLUGIN_DIR}/libasap-xmms.so

uninstall-xmms:
	${RM} ${XMMS_INPUT_PLUGIN_DIR}/libasap-xmms.so

install-xmms-user: libasap-xmms.so
	${INSTALL_PROGRAM} libasap-xmms.so ${XMMS_USER_PLUGIN_DIR}/libasap-xmms.so

uninstall-xmms-user:
	${RM} ${XMMS_USER_PLUGIN_DIR}/libasap-xmms.so

install-moc: libasap_decoder.so
	${INSTALL_PROGRAM} libasap_decoder.so ${MOC_PLUGIN_DIR}/libasap_decoder.so

uninstall-moc:
	${RM} ${MOC_PLUGIN_DIR}/libasap_decoder.so

clean:
	${RM} players.h asap2wav libasap.a libasap-xmms.so libasap_decoder.so xbmc_asap-i486-linux.so

README.html: README INSTALL NEWS CREDITS
	${ASCIIDOC} -o $@ -a asapsrc README
	${ASCIIDOC_POSTPROCESS} $@

.DELETE_ON_ERROR:
