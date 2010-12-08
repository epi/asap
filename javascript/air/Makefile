PP = cpp -C -P -I.. -I../..
ADL = adl.exe
ADT = adt.bat

COMMON_C = ../../asap.c ../../acpu.c ../../apokeysnd.c
COMMON_H = ../../asap.h ../../asap_internal.h ../../anylang.h ../../players.h

run: airasap.js
	$(ADL) AIRASAP-app.xml

TOP_DIR = ../..
include ../../players/Makefile

AIRASAP.air: AIRASAP-app.xml airasap.html airasap.js asap16x16.png asap32x32.png
	$(ADT) -package -storetype pkcs12 -keystore pfusik-air.pfx -storepass walsie -tsa none $@ $^

airasap.js: airasap.ppjs ../asap.ppjs $(COMMON_C) $(COMMON_H)
	$(PP) $< > $@

clean:
	rm -f AIRASAP.air airasap.js

.DELETE_ON_ERROR:
