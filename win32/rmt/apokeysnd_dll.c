/*
 * apokeysnd_dll.c - POKEY sound emulator for Raster Music Tracker
 *
 * Copyright (C) 2008-2011  Piotr Fusik
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

#include "asap_internal.h"

static ASAP_State asap;

__declspec(dllexport) void APokeySound_Initialize(abool stereo)
{
	asap.extra_pokey_mask = stereo ? 0x10 : 0;
	PokeySound_Initialize(&asap);
	PokeySound_Mute(&asap, &asap.base_pokey, 0);
	PokeySound_Mute(&asap, &asap.extra_pokey, 0);
	PokeySound_StartFrame(&asap);
}

__declspec(dllexport) void APokeySound_PutByte(int addr, int data)
{
	PokeySound_PutByte(&asap, addr, data);
}

__declspec(dllexport) int APokeySound_GetRandom(int addr, int cycle)
{
	return PokeySound_GetRandom(&asap, addr, cycle);
}

__declspec(dllexport) int APokeySound_Generate(int cycles, byte buffer[], ASAP_SampleFormat format)
{
	int len;
	PokeySound_EndFrame(&asap, cycles);
	len = PokeySound_Generate(&asap, buffer, 0, asap.samples, format);
	PokeySound_StartFrame(&asap);
	return len;
}

__declspec(dllexport) void APokeySound_About(const char **name, const char **author, const char **description)
{
	*name = "Another POKEY Sound Emulator, v" ASAP_VERSION;
	*author = "Piotr Fusik, (C) " ASAP_YEARS;
	*description = "Part of ASAP, http://asap.sourceforge.net";
}
