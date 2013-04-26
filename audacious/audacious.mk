AUDACIOUS_CFLAGS = `pkg-config --cflags gtk+-3.0`
AUDACIOUS_INPUT_PLUGIN_DIR = `pkg-config --variable=input_plugin_dir audacious`

# no user-configurable paths below this line

ifndef DO
$(error Use "Makefile" instead of "audacious.mk")
endif

asap-audacious: asapplug.so
.PHONY: asap-audacious

asapplug.so: $(call src,audacious/asapplug.c asap.[ch])
	$(CC) $(AUDACIOUS_CFLAGS)
CLEAN += asapplug.so

install-audacious: asapplug.so
	$(call INSTALL_PROGRAM,asapplug.so,$(AUDACIOUS_INPUT_PLUGIN_DIR))
.PHONY: install-audacious

uninstall-audacious:
	$(RM) $(DESTDIR)$(AUDACIOUS_INPUT_PLUGIN_DIR)/asapplug.so
.PHONY: uninstall-audacious
