JRE = C:/java/jdk/jre
WTK = C:/java/sesdk/PC_Emulation/WTK2

JAVAC = javac -classpath ".;${JRE}/lib/plugin.jar" -d .
JAR = jar
JAVADOC = javadoc
PP = cpp -C -P -I..
PERL = perl
XASM = xasm -q
RM = rm -f
RMR = rm -rf
ME_CLASSES = ${WTK}/lib/cldcapi10.jar;${WTK}/lib/midpapi10.jar;${WTK}/lib/jsr75.jar
PREVERIFY = ${WTK}/bin/preverify1.0
ME_EMU = ${WTK}/bin/emulatorw -gui

COMMON_C = ../asap.c ../acpu.c ../apokeysnd.c
COMMON_H = ../asap.h ../asap_internal.h ../players.h
PLAYERS_OBX = ../players/cmc.obx ../players/mpt.obx ../players/rmt4.obx ../players/rmt8.obx ../players/tmc.obx ../players/tm2.obx

all: ASAP2WAV.class asap_applet.jar asap_midlet.jar

ASAP2WAV.class: ASAP2WAV.java net/sf/asap/ASAP.class
	${JAVAC} ASAP2WAV.java

asap_applet.jar: ASAPApplet.class net/sf/asap/ASAP.class \
	net/sf/asap/ASAP_ModuleInfo.class net/sf/asap/ASAP_State.class net/sf/asap/PokeyState.class
	${JAR} cf $@ ASAPApplet.class net/sf/asap

ASAPApplet.class: ASAPApplet.java net/sf/asap/ASAP.class
	${JAVAC} ASAPApplet.java

emu: asap_midlet.jad
	${ME_EMU} -Xdescriptor:asap_midlet.jad

asap_midlet.jad: asap_midlet.jar MIDLET_MANIFEST.MF
	${PERL} mf2jad.pl asap_midlet

asap_midlet.jar: MIDLET_MANIFEST.MF preverified/ASAPMIDlet.class
	${JAR} cfm $@ MIDLET_MANIFEST.MF -C preverified .

preverified/ASAPMIDlet.class: ASAPMIDlet.class net/sf/asap/ASAP.class net/sf/asap/ASAP_ModuleInfo.class
	${PREVERIFY} -classpath "${ME_CLASSES};." -d preverified ASAPMIDlet net.sf.asap.ASAP net.sf.asap.ASAP_ModuleInfo net.sf.asap.ASAP_State net.sf.asap.PokeyState

ASAPMIDlet.class: ASAPMIDlet.java net/sf/asap/ASAP.class
	${JAVAC} -bootclasspath "${ME_CLASSES}" ASAPMIDlet.java

net/sf/asap/ASAP.class: ASAP.java net/sf/asap/ASAP_ModuleInfo.class
	${JAVAC} ASAP.java

ASAP.java: ASAP.ppjava ${COMMON_C} ${COMMON_H}
	${PP} ASAP.ppjava > ASAP.java

net/sf/asap/ASAP_ModuleInfo.class: ASAP_ModuleInfo.java
	${JAVAC} ASAP_ModuleInfo.java

../players.h: ../raw2c.pl ${PLAYERS_OBX}
	${PERL} ../raw2c.pl ${PLAYERS_OBX} >$@

../players/cmc.obx: ../players/cmc.asx
	${XASM} -o $@ ../players/cmc.asx

../players/mpt.obx: ../players/mpt.asx
	${XASM} -o $@ ../players/mpt.asx

../players/rmt4.obx: ../players/rmt.asx
	${XASM} -d STEREOMODE=0 -o $@ ../players/rmt.asx

../players/rmt8.obx: ../players/rmt.asx
	${XASM} -d STEREOMODE=1 -o $@ ../players/rmt.asx

../players/tmc.obx: ../players/tmc.asx
	${XASM} -o $@ ../players/tmc.asx

../players/tm2.obx: ../players/tm2.asx
	${XASM} -o $@ ../players/tm2.asx

doc: ASAP.java ASAP_ModuleInfo.java
	$(JAVADOC) -d doc ASAP.java ASAP_ModuleInfo.java

clean:
	$(RM) *.class ASAP.java
	$(RMR) net doc
