/*
 * aatr.c - another ATR file extractor
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
#include <stdlib.h>
#include <string.h>
#include "aatr.h"

struct AATR
{
	FILE *fp;
	int bytes_per_sector;
	int sector4_offset;
	int file_no;
	unsigned char dir_sector[128];
	char filename[8 + 1 + 3 + 1];
};

AATR *AATR_New(void)
{
	AATR *self = (AATR *) malloc(sizeof(AATR));
	self->fp = NULL;
	return self;
}

void AATR_Delete(AATR *self)
{
	if (self->fp != NULL)
		fclose(self->fp);
	free(self);
}

static int fgetw(FILE *fp)
{
	int lo = fgetc(fp);
	int hi;
	if (lo < 0)
		return -1;
	hi = fgetc(fp);
	if (hi < 0)
		return -1;
	return lo + (hi << 8);
}

cibool AATR_Open(AATR *self, const char *filename)
{
	FILE *fp;
	int paragraphs;
	fp = fopen(filename, "rb");
	if (fp == NULL)
		return FALSE;
	if (fgetw(fp) != 0x296) {
		fclose(fp);
		return FALSE;
	}
	paragraphs = fgetw(fp);
	if ((paragraphs & 7) != 0) {
		fclose(fp);
		return FALSE;
	}
	self->bytes_per_sector = fgetw(fp);
	self->sector4_offset = 0x190;
	switch (self->bytes_per_sector) {
	case 0x80:
		break;
	case 0x100:
		if ((paragraphs & 8) == 0)
			self->sector4_offset = 0x310;
		break;
	default:
		fclose(fp);
		return FALSE;
	}
	self->fp = fp;
	self->file_no = -1;
	return TRUE;
}

static cibool AATR_ReadSector(AATR *self, int sector, unsigned char buffer[], int length)
{
	if (sector < 4)
		return FALSE;
	if (fseek(self->fp, self->sector4_offset + (sector - 4) * self->bytes_per_sector, SEEK_SET) != 0)
		return FALSE;
	return fread(buffer, length, 1, self->fp) == 1;
}

static int filename_part_chars(const char *p, int max)
{
	int i;
	for (i = 0; i < max; i++) {
		char c = p[i];
		if (c == ' ') {
			int result = i;
			/* make sure no spaces in filename */
			while (++i < max) {
				if (p[i] != ' ')
					return -1;
			}
			return result;
		}
		if ((c >= '0' && c <= '9')
		 || (c >= 'A' && c <= 'Z')
		 || c == '_')
			continue;
		return -1;
	}
	return max;
}

const char *AATR_NextFile(AATR *self)
{
	int i;
	int filename_chars;
	int ext_chars;
	for (;;) {
		const char *filename;
		if (self->file_no >= 63)
			break;
		self->file_no++;
		i = (self->file_no & 7) << 4;
		if (i == 0) {
			if (!AATR_ReadSector(self, 361 + (self->file_no >> 3), self->dir_sector, 128))
				break;
		}
		switch (self->dir_sector[i] & 0xd7) { /* mask out readonly and unused bits */
		case 0x00: /* end of directory */
			break;
		case 0x42: /* DOS 2 file */
		case 0x46: /* MyDOS file */
		case 0x03: /* DOS 2.5 file */
			break;
		default:
			continue;
		}
		filename = (const char *) self->dir_sector + i + 5;
		filename_chars = filename_part_chars(filename, 8);
		ext_chars = filename_part_chars(filename + 8, 3);
		if (filename_chars < 0 || ext_chars < 0 || (filename_chars == 0 && ext_chars == 0))
			continue;
		sprintf(self->filename, "%.*s.%.*s", filename_chars, filename, ext_chars, filename + 8);
		return self->filename;
	}
	fclose(self->fp);
	self->fp = NULL;
	return NULL;
}

int AATR_ReadCurrentFile(AATR *self, unsigned char output[], int length)
{
	int result = 0;
	int i = (self->file_no & 7) << 4;
	int sector = self->dir_sector[i + 3] + (self->dir_sector[i + 4] << 8);
	while (sector != 0 && length > 0) {
		unsigned char sector_data[256];
		int sector_used;
		if (!AATR_ReadSector(self, sector, sector_data, self->bytes_per_sector))
			return -1;
		sector_used = sector_data[self->bytes_per_sector - 1];
		if (sector_used > length)
			sector_used = length;
		memcpy(output + result, sector_data, sector_used);
		result += sector_used;
		length -= sector_used;
		sector = (sector_data[self->bytes_per_sector - 2] + (sector_data[self->bytes_per_sector - 3] << 8)) & 0x3ff;
	}
	return result;
}

int AATR_ReadFile(const char *atr_filename, const char *inside_filename, unsigned char output[], int length)
{
	AATR aatr;
	if (!AATR_Open(&aatr, atr_filename))
		return -1;
	for (;;) {
		const char *current_filename = AATR_NextFile(&aatr);
		if (current_filename == NULL)
			return -1;
		if (strcmp(current_filename, inside_filename) == 0) {
			length = AATR_ReadCurrentFile(&aatr, output, length);
			fclose(aatr.fp);
			return length;
		}
	}
}

#if 0
int main(int argc, char **argv)
{
	AATR aatr;
	if (!AATR_Open(&aatr, "C:\\0\\a8\\asap\\TMC2.atr"))
		return 1;
	for (;;) {
		const char *current_filename = AATR_NextFile(&aatr);
		unsigned char buffer[30000];
		int length;
		if (current_filename == NULL)
			break;
		length = AATR_ReadCurrentFile(&aatr, buffer, sizeof(buffer));
		printf("%s (%d)\n", current_filename, length);
	}
	return 0;
}
#endif
