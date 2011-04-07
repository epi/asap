all: asap.s asap64.s asap.html asap.jasm asap.abc

asap.s: ../../asapci.c ../../asapci.h
	gcc -S -O2 -Wall -o $@ $<

asap64.s: ../../asapci.c ../../asapci.h
	x86_64-w64-mingw32-gcc -S -O2 -Wall -o $@ $<

asap.html: ../../csharp/asap2wav.exe
	-"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.0A/Bin/ildasm.exe" /output=$@ /html /nobar /source $<

asap.jasm: ../../java/net/sf/asap/ASAP.class
	javap -c -private -classpath ../../java net.sf.asap.ASAP net.sf.asap.ASAPInfo net.sf.asap.Cpu6502 net.sf.asap.Pokey net.sf.asap.PokeyPair > $@

asap.abc: ../../flash/asap.swf
	swfdump -abc $< > $@

clean:
	rm -f asap.s asap64.s asap.html asap.jasm asap.abc

.DELETE_ON_ERROR:
