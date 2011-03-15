all: asap2wav.js

asap2wav.exe: asap2wav.js
	jsc -nologo -w:2 -out:$@ $<

asap2wav.js: asap.js asap2wav1.js
	cat asap.js asap2wav1.js > $@

clean:
	rm -f asap2wav.exe asap2wav.js
	$(MAKE) -C air clean

.DELETE_ON_ERROR:
