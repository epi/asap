CSC = csc -nologo -o+ -out:$@
PP = cpp -C -P -I..
NDOC = "C:/Program Files/NDoc3/bin/NDoc3Console.exe"

all: asap2wav.exe

include ../players/Makefile

asap2wav.exe: asap2wav.cs ASAP.cs
	$(CSC) asap2wav.cs ASAP.cs

ASAP.cs: ASAP.ppcs ../asap.c ../acpu.c ../apokeysnd.c ../asap.h ../asap_internal.h ../anylang.h ../players.h
	$(PP) ASAP.ppcs > $@

doc: doc/ASAP.chm

doc/ASAP.chm: doc/ASAP.dll
	$(NDOC) -documenter:MSDN -CleanIntermediates=true -DocumentInheritedFrameworkMembers=false \
		-OutputTarget=HtmlHelp -HtmlHelpName=ASAP -Title="ASAP .NET API" doc/ASAP.dll

doc/ASAP.dll: ASAP.cs
	-mkdir doc
	$(CSC) -t:library -doc:doc/ASAP.xml ASAP.cs

clean:
	rm -rf asap2wav.exe ASAP.cs doc

.DELETE_ON_ERROR:
