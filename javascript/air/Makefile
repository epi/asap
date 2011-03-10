ADL = adl.exe
ADT = adt.bat

run: airasap.js
	$(ADL) AIRASAP-app.xml

TOP_DIR = ../..
include ../../players/Makefile

AIRASAP.air: AIRASAP-app.xml airasap.html asap.js airasap.js asap16x16.png asap32x32.png
	$(ADT) -package -storetype pkcs12 -keystore pfusik-air.pfx -storepass walsie -tsa none $@ $^

clean:
	rm -f AIRASAP.air

.DELETE_ON_ERROR:
