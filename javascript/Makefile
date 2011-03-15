PP = cpp -C -P -I..
JSC = jsc -nologo -w:2 -out:$@

COMMON_C = ../asap.c ../acpu.c ../apokeysnd.c
COMMON_H = ../asap.h ../asap_internal.h ../anylang.h ../players.h

all: asap2wav.js

include ../players/Makefile

asap2wav.exe: asap2wav.js
	$(JSC) $<

asap2wav.js: asap2wav.ppjs asap.ppjs $(COMMON_C) $(COMMON_H)
	$(PP) $< > $@

clean:
	rm -f asap2wav.exe asap2wav.js
	$(MAKE) -C air clean

.DELETE_ON_ERROR:
