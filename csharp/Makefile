CSC = "C:/Windows/Microsoft.NET/Framework/v2.0.50727/csc.exe" -nologo -o+ -out:$@
CITO = cito -o $@
NDOC = "C:/Program Files (x86)/NDoc3/bin/NDoc3Console.exe"
SEVENZIP = 7z a -mx=9
SL = "C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/Silverlight/v3.0"

COMMON_CI =  ../asap.ci ../asap6502.ci ../asapinfo.ci ../cpu6502.ci ../pokey.ci

all: asap2wav.exe asapplay.exe SilverASAP.xap

include ../players/Makefile

asap2wav.exe: asap2wav.cs asapci.cs
	$(CSC) asap2wav.cs asapci.cs

asapplay.exe: asapplay.cs asapci.cs
	$(CSC) asapplay.cs asapci.cs

SilverASAP.xap: SilverASAP.dll AppManifest.xaml
	$(SEVENZIP) -tzip $@ SilverASAP.dll AppManifest.xaml

SilverASAP.dll: SilverASAP.cs asapci.cs
	$(CSC) -t:library SilverASAP.cs asapci.cs -nostdlib -noconfig -r:$(SL)/mscorlib.dll -r:$(SL)/system.dll -r:$(SL)/System.Net.dll -r:$(SL)/System.Windows.dll -r:$(SL)/System.Windows.Browser.dll

chksap.exe: chksap.cs asapci.cs
	$(CSC) chksap.cs asapci.cs

asapci.cs: $(COMMON_CI) $(PLAYERS_OBX)
	$(CITO) -I ../players -l cs -n Sf.Asap $(COMMON_CI)

doc: doc/ASAP.chm

doc/ASAP.chm: doc/ASAP.dll
	$(NDOC) -documenter:MSDN -CleanIntermediates=true -DocumentInheritedFrameworkMembers=false \
		-OutputTarget=HtmlHelp -HtmlHelpName=ASAP -Title="ASAP .NET API" doc/ASAP.dll

doc/ASAP.dll: asapci.cs
	-mkdir doc
	$(CSC) -t:library -doc:doc/ASAP.xml asapci.cs

clean:
	rm -rf asap2wav.exe asapplay.exe SilverASAP.xap SilverASAP.dll doc

.DELETE_ON_ERROR:
