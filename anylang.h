/*
 * anylang.h - C/Java/C# abstraction layer
 *
 * Copyright (C) 2007-2009  Piotr Fusik
 *
 * This file is part of ASAP (Another Slight Atari Player),
 * see http://asap.sourceforge.net
 *
 * ASAP is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License as published
 * by the Free Software Foundation; either version 2 of the License,
 * or (at your option) any later version.
 *
 * ASAP is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ASAP; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

#ifndef _ANYLANG_H_
#define _ANYLANG_H_

#ifdef JAVA

#define abool                   boolean
#define FALSE                   false
#define TRUE                    true
#define NULL                    null
#define const
#define PRIVATE_FUNC            private static
#define PUBLIC_FUNC             private static
#define _                       .
#define PTR
#define ADDRESSOF
#define ARRAY                   []
#define VOIDPTR                 byte[]
#define UBYTE(data)             ((data) & 0xff)
#define SBYTE(data)             (byte) (data)
#define CONST_ARRAY(type, name) private static final type[] name
#define sizeof(array)           array.length
#define ZERO_ARRAY(array)       for (int ii = 0; ii < array.length; ii++) array[ii] = 0
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                System.arraycopy(src, src_offset, dest, dest_offset, len)
#define NEW_ARRAY(type, name, size) \
                                type[] name = new type[size]
#define INIT_ARRAY(array)
#define STRING                  String
#define CHARAT(s, i)            (s).charAt(i)
#define strlen(s)               (s).length()
#define EQUAL_STRINGS(s1, s2)   (s1).equals(s2)
#define CONTAINS_STRING(s1, s2) ((s1).indexOf(s2) >= 0)
#define EMPTY_STRING(s)         (s) = ""
#define SUBSTRING(dest, src, src_offset, len) \
                                (dest) = (src).substring(src_offset, src_offset + len)
#define READ_BYTE               read
#define READ_ARRAY              read
#define RUNTIME_EXCEPTION       RuntimeException

#define ASAP_OBX                InputStream
#define GET_OBX(name)           ASAP.class.getResourceAsStream(#name + ".obx")

#elif defined(CSHARP)

#define abool                   bool
#define FALSE                   false
#define TRUE                    true
#define NULL                    null
#define const
#define PRIVATE_FUNC            private static
#define PUBLIC_FUNC             private static
#define _                       .
#define PTR
#define ADDRESSOF
#define ARRAY                   []
#define VOIDPTR                 byte[]
#define UBYTE(data)             (data)
#define SBYTE(data)             (sbyte) (data)
#define CONST_ARRAY(type, name) private static readonly type[] name
#define sizeof(array)           array.Length
#define ZERO_ARRAY(array)       Array.Clear(array, 0, array.Length)
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                Array.Copy(src, src_offset, dest, dest_offset, len)
#define NEW_ARRAY(type, name, size) \
                                type[] name = new type[size]
#define INIT_ARRAY(array)
#define STRING                  string
#define CHARAT(s, i)            (s)[i]
#define strlen(s)               (s).Length
#define EQUAL_STRINGS(s1, s2)   ((s1) == (s2))
#define CONTAINS_STRING(s1, s2) ((s1).IndexOf(s2) >= 0)
#define EMPTY_STRING(s)         (s) = string.Empty
#define SUBSTRING(dest, src, src_offset, len) \
                                (dest) = (src).Substring(src_offset, len)
#define READ_BYTE               ReadByte
#define READ_ARRAY              Read
#define RUNTIME_EXCEPTION       Exception

#define ASAP_OBX                Stream
#define GET_OBX(name)           System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(#name)

#else /* C */

#include <string.h>

#define PRIVATE_FUNC            static
#define PUBLIC_FUNC
#define _                       ->
#define PTR                     *
#define ADDRESSOF               &
#define ARRAY                   *
#define VOIDPTR                 void *
#define UBYTE(data)             (data)
#define SBYTE(data)             (signed char) (data)
#define CONST_ARRAY(type, name) static const type name[]
#define ZERO_ARRAY(array)       memset(array, 0, sizeof(array))
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                memcpy(dest + dest_offset, src + src_offset, len)
#define NEW_ARRAY(type, name, size) \
                                type name[size]
#define INIT_ARRAY(array)       memset(array, 0, sizeof(array))
#define STRING                  const char *
#define CHARAT(s, i)            (s)[i]
#define EQUAL_STRINGS(s1, s2)   (strcmp(s1, s2) == 0)
#define CONTAINS_STRING(s1, s2) (strstr(s1, s2) != NULL)
#define EMPTY_STRING(s)         (s)[0] = '\0'
#define SUBSTRING(dest, src, src_offset, len) \
                                do { memcpy(dest, src + src_offset, len); (dest)[len] = '\0'; } while (FALSE)

#define ASAP_OBX                const byte *
#define GET_OBX(name)           name##_obx

#endif

#endif /* _ANYLANG_H_ */
