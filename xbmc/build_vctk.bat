set Path=C:\Program Files\Microsoft Visual C++ Toolkit 2003\bin;C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322
set INCLUDE=C:\Program Files\Microsoft Visual C++ Toolkit 2003\include
set LIB=C:\Program Files\Microsoft Visual C++ Toolkit 2003\lib;C:\Program Files\Microsoft Visual Studio .NET 2003\Vc7\lib
cl -GR- -DNDEBUG -nologo -O2 -GL -W3 -Fe../win32/xbmc_asap.dll -LD -I.. xbmc_asap.c ../asap.c ../win32/xbmc/xbmc_asap.res -Fo../win32/xbmc/ -MD -link -release
