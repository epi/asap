all: asap.swf

asap.swf: ASAPPlayer.as ASAP.as
	mxmlc -o $@ -compiler.strict=false -compiler.debug -compiler.warn-no-type-decl=false -target-player 10 ASAPPlayer.as
# -compiler.optimize

ASAP.as: ASAP.ppas ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h
	cpp -P -I.. -DASAP_ONLY_SAP $< > $@

clean:
	rm -f asap.swf ASAP.as

.DELETE_ON_ERROR:
