# Run "autoconf" to generate "configure" from this file.

AC_PREREQ(2.56)
AC_INIT(asap, 1.2.0, fox@scene.pl)
AC_CONFIG_SRCDIR(asap.c)

if [[ "X_$CFLAGS" = "X_" ]]; then
    CFLAGS="-O2 -Wall"
fi
if [[ "X_$LDFLAGS" = "X_" ]]; then
    LDFLAGS="-s"
fi
AC_PROG_CC

AC_CHECK_PROG(HAVE_XMMS_CONFIG, xmms-config, yes, no)
if [[ "$HAVE_XMMS_CONFIG" = "yes" ]]; then
    XMMS_CFLAGS=`xmms-config --cflags`
    XMMS_LIBS=`xmms-config --libs`
    XMMS_INPUT_PLUGIN_DIR=`xmms-config --input-plugin-dir`
else
    AC_MSG_WARN([xmms-config program not found!])
    AC_MSG_WARN([If you want to compile the XMMS plugin,])
    AC_MSG_WARN([install xmms-devel package,])
    AC_MSG_WARN([then re-run this configure script.])
    XMMS_CFLAGS="NOT_CONFIGURED"
    XMMS_LIBS="NOT_CONFIGURED"
    XMMS_INPUT_PLUGIN_DIR="NOT_CONFIGURED"
fi
AC_SUBST(XMMS_CFLAGS)
AC_SUBST(XMMS_LIBS)
AC_SUBST(XMMS_INPUT_PLUGIN_DIR)

if [[ "X_$MOC_INCLUDE" = "X_" ]]; then
    MOC_INCLUDE="../moc-2.4.3"
fi
if [[ "X_$MOC_PLUGIN_DIR" = "X_" ]]; then
    MOC_PLUGIN_DIR="/usr/local/lib/moc/decoder_plugins"
fi
AC_SUBST(MOC_INCLUDE)
AC_SUBST(MOC_PLUGIN_DIR)

AC_CONFIG_FILES(Makefile)
AC_OUTPUT
