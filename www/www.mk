ifndef DO
$(error Use "Makefile" instead of "www.mk")
endif

www: www/index.html www/applet.html www/flash.html www/silverlight.html www/apokeysnd.dll www/asap.swf www/asap_applet.jar www/SilverASAP.xap www/favicon.ico www/PORTS.xml www/PORTS.xsl
.PHONY: www

www/index.html: $(call src,README NEWS CREDITS)
	$(call ASCIIDOC,-a asapwww -a asapports)

www/applet.html: $(srcdir)www/applet.txt
	$(call ASCIIDOC,)

www/flash.html: $(srcdir)www/flash.txt
	$(call ASCIIDOC,)

www/silverlight.html: $(srcdir)www/silverlight.txt
	$(call ASCIIDOC,)

www/apokeysnd.dll: win32/apokeysnd.dll
	$(COPY)

www/asap.swf: flash/asap.swf
	$(COPY)

www/asap_applet.jar: java/asap_applet.jar
	$(COPY)

www/SilverASAP.xap: csharp/SilverASAP.xap
	$(COPY)

www/favicon.ico: $(srcdir)win32/wasap/wasap.ico
	$(COPY)

www/PORTS.xml: $(srcdir)PORTS.xml
	$(COPY)

www/PORTS.xsl: $(srcdir)PORTS.xsl
	$(COPY)
