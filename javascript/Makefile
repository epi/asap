PP = cpp -P -I.. -DASAP_ONLY_SAP

all: asap2wav.js asapweb.js

asap2wav.js: asap2wav.ppjs asap.ppjs ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h
	$(PP) $< > $@

asapweb.js: asapweb.ppjs asap.ppjs ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h
	$(PP) $< > $@

clean:
	rm -f asap2wav.js asapweb.js

.DELETE_ON_ERROR:
