JRE ?= C:/Program Files/Java/jre6
WTK = C:/bin/Java_ME_platform_SDK_3.0

JAVAC ?= javac -source 1.2 -d .
JAR ?= jar
JAR_OPTIMIZE ?= 7z a -mx=9 -tzip
PERL = perl
ME_CLASSES = $(WTK)/lib/cldc_1.1.jar;$(WTK)/lib/midp_2.0.jar;$(WTK)/lib/jsr75_1.0.jar
PREVERIFY = "$(WTK)/bin/preverify"

TOP_DIR = ../..
PLAYERS_OBX_DIR = ../net/sf/asap

all: asap_midlet.jad

include ../../players/Makefile

asap_midlet.jad: asap_midlet.jar MANIFEST.MF
	$(PERL) mf2jad.pl asap_midlet

asap_midlet.jar: MANIFEST.MF preverified/ASAPMIDlet.class $(PLAYERS_OBX)
	$(JAR) cfm $@ MANIFEST.MF -C preverified .
	# the following is needed and is not just an optimization
	cd .. && $(JAR_OPTIMIZE) j2me/$@ $(PLAYERS:%=net/sf/asap/%.obx)
	cd preverified && $(JAR_OPTIMIZE) ../$@ .

preverified/ASAPMIDlet.class: ASAPMIDlet.class ../net/sf/asap/ASAP.class ../net/sf/asap/ASAP_ModuleInfo.class
	$(PREVERIFY) -classpath "$(ME_CLASSES);.;.." -d preverified ASAPMIDlet FileList ASAPInputStream net.sf.asap.ASAP net.sf.asap.ASAP_ModuleInfo net.sf.asap.ASAP_State net.sf.asap.PokeyState

ASAPMIDlet.class: ASAPMIDlet.java ../net/sf/asap/ASAP.class
	$(JAVAC) -classpath ".;.." -bootclasspath "$(ME_CLASSES)" ASAPMIDlet.java

../net/sf/asap/ASAP.class:
	$(MAKE) -C .. net/sf/asap/ASAP.class

../net/sf/asap/ASAP_ModuleInfo.class:
	$(MAKE) -C .. net/sf/asap/ASAP_ModuleInfo.class

clean:
	rm -f asap_midlet.jar asap_midlet.jad *.class
	rm -rf preverified

.DELETE_ON_ERROR:
