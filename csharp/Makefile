CSC = csc -nologo -o+ -out:$@
PP = cpp -P -I..

all: asap2wav.exe

include ../players/Makefile

asap2wav.exe: asap2wav.cs ASAP.cs
	$(CSC) asap2wav.cs ASAP.cs

ASAP.cs: ASAP.ppcs ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h ../players.h
	$(PP) ASAP.ppcs > $@

clean:
	rm -f asap2wav.exe ASAP.cs

.DELETE_ON_ERROR:
