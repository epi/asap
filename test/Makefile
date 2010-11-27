XASM = xasm -q -o $@
RM = rm -f

TESTS = cpu_decimal cpu_shx

all: $(TESTS:%=%.sap) $(TESTS:%=%.xex)

%.sap: %.asx
	$(XASM) -d SAP=1 $<

%.xex: %.asx
	$(XASM) -d SAP=0 $<

clean:
	$(RM) *.sap *.xex

.PHONY: all clean

.DELETE_ON_ERROR:
