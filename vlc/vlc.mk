VLC_CFLAGS = -std=gnu99 -I/usr/include/vlc/plugins
VLC_DEMUX_PLUGIN_DIR = /usr/lib/vlc/plugins/demux

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "vlc.mk")
endif

asap-vlc: libasap_plugin.so
.PHONY: asap-vlc

libasap_plugin.so: $(call src,vlc/libasap_plugin.c asap.[ch])
	$(CC) $(VLC_CFLAGS)
CLEAN += libasap_plugin.so

install-vlc: libasap_plugin.so
	$(call INSTALL_PROGRAM,libasap_plugin.so,$(VLC_DEMUX_PLUGIN_DIR))
.PHONY: install-vlc

uninstall-vlc:
	$(RM) $(DESTDIR)$(VLC_DEMUX_PLUGIN_DIR)/libasap_plugin.so
.PHONY: uninstall-vlc
