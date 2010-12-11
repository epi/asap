XASM = xasm -q -o $@
RM = rm -f

TESTS = cpu_ane cpu_anx cpu_decimal cpu_las cpu_shx

all: antic_vcount_ntsc.sap antic_vcount_pal.sap $(TESTS:%=%.sap) antic_vcount.xex $(TESTS:%=%.xex)

%.sap: %.asx
	$(XASM) -d SAP=1 $<

%_ntsc.sap: %.asx
	$(XASM) -d SAP=1 -d NTSC=1 $<

%_pal.sap: %.asx
	$(XASM) -d SAP=1 -d NTSC=0 $<

%.xex: %.asx
	$(XASM) -d SAP=0 $<

clean:
	$(RM) *.sap *.xex

.PHONY: all clean

.DELETE_ON_ERROR:
