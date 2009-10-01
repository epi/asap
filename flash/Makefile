all: asap.swf

asap.swf: ASAPPlayer.as ASAP.as ASAP_ModuleInfo.as PokeyState.as
	mxmlc -o $@ -compiler.optimize -compiler.warn-duplicate-variable-def=false -compiler.warn-no-type-decl=false -target-player 10 ASAPPlayer.as
# -compiler.debug

ASAP.as: ASAP.ppas ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h
	cpp -P -I.. -DASAP_ONLY_SAP $< > $@

debug:
	tail -f "${APPDATA}\Macromedia\Flash Player\Logs\flashlog.txt"

clean:
	rm -f asap.swf ASAP.as

.DELETE_ON_ERROR:
