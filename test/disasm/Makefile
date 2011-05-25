all: asap.s asap64.s asap.html asap.jasm asap.abc

asap.s: ../../asap.c ../../asap.h
	gcc -S -O2 -Wall -o $@ $<

asap64.s: ../../asap.c ../../asap.h
	x86_64-w64-mingw32-gcc -S -O2 -Wall -o $@ $<

asap.html: ../../csharp/asap2wav.exe
	-"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.0A/Bin/ildasm.exe" /output=$@ /html /nobar /source $<

asap.jasm: ../../java/net/sf/asap/ASAP.class
	javap -c -private -classpath ../../java net.sf.asap.ASAP net.sf.asap.ASAPInfo net.sf.asap.Cpu6502 net.sf.asap.Pokey net.sf.asap.PokeyPair > $@

asap.abc: ../../flash/asap.swf
	swfdump -abc $< > $@

asap.dexdump:
	java -jar "c:/bin/android-sdk-windows/platforms/android-8//tools/lib/dx.jar" --dex --dump-to=$@ ../../java/android/classes/

clean:
	$(RM) asap.s asap64.s asap.html asap.jasm asap.abc asap.dexdump

.PHONY: all clean

.DELETE_ON_ERROR:
