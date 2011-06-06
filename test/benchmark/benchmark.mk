benchmark: test/benchmark/BENCHMARK.html
.PHONY: benchmark

test/benchmark/BENCHMARK.html: test/benchmark/BENCHMARK.txt
	$(call ASCIIDOC,)

test/benchmark/BENCHMARK.txt: $(srcdir)test/benchmark/benchmark.pl win32/msvc/asapconv.exe win32/asapconv.exe win32/x64/asapconv.exe java/asap2wav.jar csharp/asap2wav.exe
	perl $< > $@
