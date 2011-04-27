CITO = cito -o $@
ADL = adl.exe
ADT = adt.bat

COMMON_CI = ../../asap.ci ../../asap6502.ci ../../asapinfo.ci ../../cpu6502.ci ../../pokey.ci

run: asap.js
	$(ADL) AIRASAP-app.xml

TOP_DIR = ../..
include ../../players/Makefile

AIRASAP.air: AIRASAP-app.xml airasap.html asap.js airasap.js asap16x16.png asap32x32.png
	$(ADT) -package -storetype pkcs12 -keystore pfusik-air.pfx -storepass walsie -tsa none $@ $^

asap.js: $(COMMON_CI) $(PLAYERS_OBX)
	$(CITO) -I ../../players -l js -D FLASH $(COMMON_CI)

clean:
	rm -f AIRASAP.air

.DELETE_ON_ERROR:
