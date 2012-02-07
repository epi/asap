/*
 * sap2txt.c - write plain text summary of a SAP file
 *
 * Copyright (C) 2012  Piotr Fusik
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

#include <stdio.h>
#ifdef __WIN32
#include <fcntl.h>
#endif
#include <zlib.h>

static int get_word(FILE *fp)
{
	int lo = getc(fp);
	int hi;
	if (lo == EOF)
		return -1;
	hi = getc(fp);
	if (hi == EOF)
		return -2;
	return lo | (hi << 8);
}

int main(int argc, char **argv)
{
	const char *input_file;
	FILE *fp;

	if (argc != 2) {
		fprintf(stderr, "Usage: sap2txt FILE.sap\n");
		return 1;
	}
	input_file = argv[1];
	fp = fopen(input_file, "rb");
	if (fp == NULL) {
		fprintf(stderr, "sap2txt: cannot open %s\n", input_file);
		return 1;
	}
#ifdef __WIN32
	_setmode(_fileno(stdout), _O_BINARY);
#endif

	for (;;) {
		int c = getc(fp);
		if (c == EOF)
			return 0;
		else if (c == 0xff)
			break;
		putchar(c);
	}
	if (getc(fp) != 0xff)
		return 0;

	for (;;) {
		int ffff = 0;
		int start_address = get_word(fp);
		int end_address;
		Bytef buffer[65536];
		int len;
		uLong crc;

		switch (start_address) {
		case -1:
			/* ok */
			return 0;
		case -2:
			printf("Unexpected end of file in a binary header\n");
			return 0;
		case 0xffff:
			ffff = 1;
			start_address = get_word(fp);
			break;
		default:
			break;
		}
		end_address = get_word(fp);
		if (end_address < 0) {
			printf("Unexpected end of file in a binary header\n");
			return 0;
		}
		if (end_address < start_address) {
			printf("Invalid binary header\n");
			return 0;
		}
		len = end_address - start_address + 1;
		if (fread(buffer, 1, len, fp) != len) {
			printf("Unexpected end of file in a binary block\n");
			return 0;
		}
		crc = crc32(0, Z_NULL, 0);
		crc = crc32(crc, buffer, len);
		if (ffff)
			printf("FFFF ");
		printf("LOAD %04X-%04X CRC32=%08lX\r\n", start_address, end_address, crc);
	}
}
