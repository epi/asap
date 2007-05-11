/*
 * ASAP_ModuleInfo.java - file information
 *
 * Copyright (C) 2007  Piotr Fusik
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

package net.sf.asap;

public class ASAP_ModuleInfo {
	/** author's name */
	public String author;
	/** title */
	public String name;
	/** creation date */
	public String date;
	/** author, name and date formatted in multiple lines */
	public String all_info;
	/* 1 for mono or 2 for stereo */
	public int channels;
	/* number of subsongs */
	public int songs;
	/* 0-based index of the "main" subsong */
	public int default_song;
	/* lengths of songs, in milliseconds, -1 = unspecified */
	public int[] durations = new int[32];
	/* whether songs repeat or not */
	public boolean[] loops = new boolean[32];
}
