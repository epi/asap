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

#if defined(JAVA) || defined(CSHARP) || defined(JAVASCRIPT) || defined(ACTIONSCRIPT)

#define FALSE                   false
#define TRUE                    true
#define NULL                    null
#define _                       .

#else

#define C
#include <string.h>

#define PRIVATE_FUNC(type)      static type
#define PUBLIC_FUNC(type)       type
#define P(type)                 type
#define VAR(type)               type
#define _                       ->
#define PTR                     *
#define ADDRESSOF               &
#define CAST(type)              (type)
#define TO_INT(x)               (int) (x)
#define TO_BYTE(x)              (byte) (x)
#define ARRAY                   *
#define VOIDPTR                 void *
#define UBYTE(data)             (data)
#define SBYTE(data)             (signed char) (data)
#define CONST_ARRAY(type, name) static const type name[] = {
#define END_CONST_ARRAY         }
#define ZERO_ARRAY(array)       memset(array, 0, sizeof(array))
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                memcpy(dest + dest_offset, src + src_offset, len)
#define NEW_ARRAY(type, name, size) \
                                type name[size]
#define INIT_ARRAY(array)       memset(array, 0, sizeof(array))
#define STRING                  const char *
#define CHARAT(s, i)            (s)[i]
#define CHARCODE(c)             (c)
#define EQUAL_STRINGS(s1, s2)   (strcmp(s1, s2) == 0)
#define CONTAINS_STRING(s1, s2) (strstr(s1, s2) != NULL)
#define EMPTY_STRING(s)         (s)[0] = '\0'
#define SUBSTRING(dest, src, src_offset, len) \
                                do { memcpy(dest, src + src_offset, len); (dest)[len] = '\0'; } while (FALSE)

#define ASAP_OBX                const byte *
#define GET_OBX(name)           name##_obx

#endif /* defined(JAVA) || defined(CSHARP) || defined(JAVASCRIPT) || defined(ACTIONSCRIPT) */

#ifdef JAVA

#define abool                   boolean
#define const
#define PRIVATE_FUNC(type)      private static type
#define PUBLIC_FUNC(type)       private static type
#define P(type)                 type
#define VAR(type)               type
#define PTR
#define ADDRESSOF
#define CAST(type)              (type)
#define TO_INT(x)               (int) (x)
#define TO_BYTE(x)              (byte) (x)
#define ARRAY                   []
#define VOIDPTR                 byte[]
#define UBYTE(data)             ((data) & 0xff)
#define SBYTE(data)             (byte) (data)
#define CONST_ARRAY(type, name) private static final type[] name = {
#define END_CONST_ARRAY         }
#define sizeof(array)           array.length
#define ZERO_ARRAY(array)       for (int ii = 0; ii < array.length; ii++) array[ii] = 0
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                System.arraycopy(src, src_offset, dest, dest_offset, len)
#define NEW_ARRAY(type, name, size) \
                                type[] name = new type[size]
#define INIT_ARRAY(array)
#define STRING                  String
#define CHARAT(s, i)            (s).charAt(i)
#define CHARCODE(c)             (c)
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
#define const
#define PRIVATE_FUNC(type)      private static type
#define PUBLIC_FUNC(type)       private static type
#define P(type)                 type
#define VAR(type)               type
#define PTR
#define ADDRESSOF
#define CAST(type)              (type)
#define TO_INT(x)               (int) (x)
#define TO_BYTE(x)              (byte) (x)
#define ARRAY                   []
#define VOIDPTR                 byte[]
#define UBYTE(data)             (data)
#define SBYTE(data)             (sbyte) (data)
#define CONST_ARRAY(type, name) private static readonly type[] name = {
#define END_CONST_ARRAY         }
#define sizeof(array)           array.Length
#define ZERO_ARRAY(array)       Array.Clear(array, 0, array.Length)
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                Array.Copy(src, src_offset, dest, dest_offset, len)
#define NEW_ARRAY(type, name, size) \
                                type[] name = new type[size]
#define INIT_ARRAY(array)
#define STRING                  string
#define CHARAT(s, i)            (s)[i]
#define CHARCODE(c)             (c)
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

#elif defined(JAVASCRIPT) || defined(ACTIONSCRIPT)

#define abool                   var
#ifdef ACTIONSCRIPT
#define PRIVATE_FUNC(type)      private static function
#define PUBLIC_FUNC(type)       private static function
#else
#define PRIVATE_FUNC(type)      function
#define PUBLIC_FUNC(type)       function
#endif
#define P(type)
#define VAR(type)               var
#define int                     var
#define char                    var
#define PTR
#define ADDRESSOF
#define CAST(type)
#define TO_INT(x)               Math.floor(x)
#define TO_BYTE(x)              ((x) & 0xff)
#define UBYTE(data)             (data)
#define SBYTE(data)             ((data) < 0x80 ? (data) : (data) - 256)
#ifdef ACTIONSCRIPT
#define CONST_ARRAY(type, name) private static const name = [
#else
#define CONST_ARRAY(type, name) var name = [
#endif
#define END_CONST_ARRAY         ]
#define sizeof(array)           array.length
#define ZERO_ARRAY(array)       for (var ii = 0; ii < array.length; ii++) array[ii] = 0
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                for (var ii = 0; ii < len; ii++) dest[dest_offset + ii] = src[src_offset + ii]
#define NEW_ARRAY(type, name, size) \
                                var name = new Array(size)
#define INIT_ARRAY(array)       for (var ii = 0; ii < array.length; ii++) array[ii] = 0
#define CHARAT(s, i)            (s).charAt(i)
#define CHARCODE(c)             (c).charCodeAt(0)
#define strlen(s)               (s).length
#define EQUAL_STRINGS(s1, s2)   ((s1) == (s2))
#define CONTAINS_STRING(s1, s2) ((s1).indexOf(s2) >= 0)
#define EMPTY_STRING(s)         s = ""
#define SUBSTRING(dest, src, src_offset, len) \
                                dest = (src).substring(src_offset, src_offset + len)

#define GET_OBX(name)           name##_obx

#endif

#endif /* _ANYLANG_H_ */
