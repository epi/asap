# Run "autoconf" to generate "configure" from this file.
# Run "autoheader" to generate "config.h.in" from this file.

AC_PREREQ(2.56)
AC_INIT(asap, 0.2.1, fox@scene.pl)
AC_CONFIG_SRCDIR(asap.c)
AC_CONFIG_HEADER(config.h)

AC_CANONICAL_HOST
if [[ "X_$CFLAGS" = "X_" ]]; then
    CFLAGS="-O2 -Wall"
fi
if [[ "X_$LDFLAGS" = "X_" ]]; then
    LDFLAGS="-s"
fi
AC_PROG_CC
AC_C_BIGENDIAN
AC_C_CONST
AC_CHECK_LIB(m,cos,[LIBS="-lm $LIBS"])

AC_ARG_ENABLE(unalignedwords,AC_HELP_STRING(--enable-unalignedwords,[Override usage of unaligned words]))
if [[ "$enable_unalignedwords" != "yes" -a "$enable_unalignedwords" != "no" ]]; then
    case $host_cpu in
        alpha* | arm* | hppa* | ia64 | mips* | sparc*)
            enable_unalignedwords=no
            ;;
        i*86 | m68* | powerpc* | x86_64)
            enable_unalignedwords=yes
            ;;
        *)
            AC_MSG_WARN([$host_cpu architecture is unknown to this script.])
            AC_MSG_WARN([Performance may be sub-optimal.])
            AC_MSG_WARN([Please contact ASAP developers.])
            enable_unalignedwords=no
            ;;
    esac
fi
if [[ "$enable_unalignedwords" = "yes" ]]; then
    dnl Make sure it is allowed
    AC_MSG_CHECKING([for unaligned word access validity])
    AC_RUN_IFELSE(
        [AC_LANG_PROGRAM([[#include <stdio.h>]],[[
            unsigned char test_data[] = "Hello, I test unaligned word access validity.";
            unsigned char *p;
            /* step through test_data as far as 4 bytes are available via p */
            for (p = test_data; p[2] != '\0'; p++) {
                unsigned int word_read = *(unsigned short *) p;
                if (word_read != (p[0] + (p[1] << 8))
                 && word_read != (p[1] + (p[0] << 8))) {
                    printf("16-bit access at address %p yields bad data!\n"
                           "Bytes: %02X %02X; Value read: %04X\n",
                           p, p[0], p[1], word_read);
                    return 1;
                }
                word_read = *(unsigned int *) p;
                if (word_read != (p[0] + (p[1] << 8) + (p[2] << 16) + (p[3] << 24))
                 && word_read != (p[3] + (p[2] << 8) + (p[1] << 16) + (p[0] << 24))) {
                    printf("32-bit access at address %p yields bad data!\n"
                           "Bytes: %02X %02X %02X %02X; Value read: %08X\n",
                           p, p[0], p[1], p[2], p[3], word_read);
                    return 1;
                }
            }
        ]])],
        AC_MSG_RESULT([yes]),
        [enable_unalignedwords=no; AC_MSG_RESULT([failed! disabling unaligned word access])],
        AC_MSG_RESULT([skipped because cross-compiling])
    )
    if [[ "$enable_unalignedwords" = "yes" ]]; then
        AC_DEFINE(WORDS_UNALIGNED_OK,1,[Define if unaligned word access is ok.])
    fi
fi

dnl The A8_OPTION macro automates the calling of AC_ARG_ENABLE, AC_HELP_STRING and AC_DEFINE.
dnl         $1    = the name of the feature, what follows "--enable-".
dnl         $2    = "yes" or "no", determines if feature defaults to being active or not.
dnl         $3    = The right side of the help line.
dnl                 The left side is always "--enable-$1".
dnl         $4    = The symbol name which goes to config.h, the C #define symbol.
dnl                 A symbol called "WANT_$4" is defined for use in this configure.ac.
dnl         $5    = The help string which shows up in config.h for the $4 symbol.
AC_DEFUN([A8_OPTION],[
    AC_ARG_ENABLE($1,AC_HELP_STRING(--enable-$1,$3),WANT_$4=$enableval,WANT_$4=$2)
    if [[ "$WANT_$4" = "yes" ]]; then
        AC_DEFINE($4,1,$5)
    fi
])

A8_OPTION(stereo,yes,[Support stereo tunes (default=ON)],STEREO_SOUND,[Define to support stereo tunes.])

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

AC_CONFIG_FILES(Makefile)
AC_OUTPUT
