JRE = C:/Program Files/Java/jre6

JAVAC = javac -source 1.2 -d .
#JAVAC = gcj -C -d .
JAR = jar
JAR_OPTIMIZE = 7z a -mx=9 -tzip
JAVADOC = javadoc

COMMON_JAVA = ASAP.java ASAP6502.java ASAPInfo.java ASAPModuleType.java ASAPSampleFormat.java ASAPWriter.java ByteWriter.java Cpu6502.java NmiStatus.java Pokey.java PokeyPair.java StringConsumer.java
PLAYERS_OBX_DIR = net/sf/asap

all: asap2wav.jar asap_applet.jar asap.jar j2me

include ../players/Makefile

asap2wav.jar: asap2wav.MF ASAP2WAV.class net/sf/asap/ASAP.class $(PLAYERS_OBX)
	$(JAR) cfm $@ asap2wav.MF ASAP2WAV.class net/sf/asap
	$(JAR_OPTIMIZE) $@ ASAP2WAV.class net/sf/asap

ASAP2WAV.class: ASAP2WAV.java net/sf/asap/ASAP.class $(PLAYERS_OBX)
	$(JAVAC) -classpath . ASAP2WAV.java

asap_applet.jar: ASAPApplet.class net/sf/asap/ASAP.class $(PLAYERS_OBX)
	$(JAR) cf $@ ASAPApplet.class net/sf/asap
	$(JAR_OPTIMIZE) $@ ASAPApplet.class net/sf/asap

ASAPApplet.class: ASAPApplet.java net/sf/asap/ASAP.class
	$(JAVAC) -classpath "$(JRE)/lib/plugin.jar;." ASAPApplet.java

asap.jar: net/sf/asap/ASAPMusicRoutine.class $(PLAYERS_OBX)
	$(JAR) cf $@ net/sf/asap
	$(JAR_OPTIMIZE) $@ ASAPApplet.class net/sf/asap

net/sf/asap/ASAPMusicRoutine.class: ASAPMusicRoutine.java
	$(JAVAC) -classpath . ASAPMusicRoutine.java

net/sf/asap/ASAP.class: $(COMMON_JAVA)
	$(JAVAC) $(COMMON_JAVA)

j2me:
	$(MAKE) -C j2me asap_midlet.jad

ASAP.java: ../asap.ci ../asap6502.ci ../asapinfo.ci ../asapwriter.ci ../cpu6502.ci ../pokey.ci
	cito -I ../players -l java -n net.sf.asap -o . $^

doc: $(COMMON_JAVA) ASAPMusicRoutine.java
	$(JAVADOC) -d doc $(COMMON_JAVA) ASAPMusicRoutine.java

clean:
	rm -f asap2wav.jar asap_applet.jar asap.jar *.class
	rm -rf net doc
	$(MAKE) -C j2me clean
	$(MAKE) -C android clean

.PHONY: all j2me clean

.DELETE_ON_ERROR:
