COMMON_CI = ../asap.ci ../asap6502.ci ../asapinfo.ci ../cpu6502.ci ../pokey.ci

all: asap2wav.js

include ../players/Makefile

asap2wav.exe: asap2wav.js
	jsc -nologo -w:2 -out:$@ $<

asap2wav.js: asap.js asap2wav1.js
	cat asap.js asap2wav1.js > $@

asap.js: $(COMMON_CI) $(PLAYERS_OBX)
	cito -o $@ -I ../players -l js $(COMMON_CI)

clean:
	rm -f asap2wav.exe asap2wav.js
	$(MAKE) -C air clean

.DELETE_ON_ERROR:
