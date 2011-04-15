SOURCES = ../asap.ci ../asap6502.ci ../asapinfo.ci ../cpu6502.ci ../pokey.ci

all: asap.swf

include ../players/Makefile

asap.swf: ASAPPlayer.as net/sf/asap/ASAP.as $(PLAYERS_OBX)
	mxmlc -o $@ -compiler.optimize -compiler.warn-duplicate-variable-def=false -static-link-runtime-shared-libraries -target-player 10 -source-path ../players -- ASAPPlayer.as
# -compiler.debug -compiler.warn-no-type-decl=false

net/sf/asap/ASAP.as: $(SOURCES) net/sf/asap
	cito -I ../players -l as -n net.sf.asap -D FLASH -o net/sf/asap $(SOURCES)

net/sf/asap:
	mkdir -p net/sf/asap

debug:
	tail -f "${APPDATA}\Macromedia\Flash Player\Logs\flashlog.txt"

doc: net/sf/asap/ASAP.as
	asdoc -compiler.warn-duplicate-variable-def=false -compiler.warn-const-not-initialized=false -output=doc -window-title="ASAP ActionScript API" -main-title="ASAP (Another Slight Atari Player) ActionScript API" -doc-sources=net/sf/asap

clean:
	rm -f asap.swf
	rm -rf net doc

.DELETE_ON_ERROR:
