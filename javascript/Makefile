PP = cpp -P -I..

all: asap2wav.js

asap2wav.js: asap2wav.ppjs asap.ppjs ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h
	$(PP) -DASAP_ONLY_SAP asap2wav.ppjs > $@

clean:
	rm -f asap2wav.js

.DELETE_ON_ERROR:
