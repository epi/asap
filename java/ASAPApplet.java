/*
 * ASAPApplet.java - ASAP applet
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

import java.applet.Applet;
import java.awt.Graphics;
import java.io.InputStream;
import java.io.IOException;
import java.net.URL;
import javax.sound.sampled.AudioFormat;
import javax.sound.sampled.AudioSystem;
import javax.sound.sampled.DataLine;
import javax.sound.sampled.LineUnavailableException;
import javax.sound.sampled.SourceDataLine;

import net.sf.asap.ASAP;
import net.sf.asap.ASAP_ModuleInfo;

public class ASAPApplet extends Applet implements Runnable
{
	private ASAP asap;
	private int song;
	private SourceDataLine line;
	private boolean stopIt;

	private static final int BITS_PER_SAMPLE = 16;

	public void paint(Graphics g)
	{
		ASAP_ModuleInfo module_info = asap.getModuleInfo();
		g.drawString("Author: " + module_info.author, 10, 20);
		g.drawString("Name: " + module_info.name, 10, 40);
		g.drawString("Date: " + module_info.date, 10, 60);
		if (module_info.songs > 1)
			g.drawString("Song " + song + " of " + module_info.songs, 10, 80);
	}

	public void run()
	{
		byte[] buffer = new byte[8192];
		int len;
		do {
			len = asap.generate(buffer, BITS_PER_SAMPLE);
			line.write(buffer, 0, len);
		} while (len == buffer.length && !stopIt);
	}

	public void start()
	{
		String filename = getParameter("file");
		byte[] module;
		int module_len = 0;
		try {
			InputStream is = new URL(getDocumentBase(), filename).openStream();
			module = new byte[ASAP.MODULE_MAX];
			for (;;) {
				int i = is.read(module, module_len, ASAP.MODULE_MAX - module_len);
				if (i <= 0)
					break;
				module_len += i;
			}
			is.close();
		} catch (IOException e) {
			showStatus("ERROR LOADING " + filename);
			return;
		}
		asap = new ASAP();
		asap.load(filename, module, module_len);
		ASAP_ModuleInfo module_info = asap.getModuleInfo();
		song = module_info.default_song;
		try {
			song = Integer.parseInt(getParameter("song"));
		} catch (Exception e) {
		}
		int duration = module_info.durations[song];
		try {
			if (duration < 0)
				duration = ASAP.parseDuration(getParameter("defaultPlaybackTime"));
			else if (module_info.loops[song])
				duration = ASAP.parseDuration(getParameter("loopPlaybackTime"));
		} catch (Exception e) {
		}
		asap.playSong(song, duration);
		AudioFormat format = new AudioFormat(ASAP.SAMPLE_RATE, BITS_PER_SAMPLE, module_info.channels, BITS_PER_SAMPLE != 8, false);
		try {
			line = (SourceDataLine) AudioSystem.getLine(new DataLine.Info(SourceDataLine.class, format));
			line.open(format);
		} catch (LineUnavailableException e) {
			showStatus("ERROR OPENING AUDIO");
			return;
		}
		line.start();
		stopIt = false;
		new Thread(this).start();
	}

	public void stop()
	{
		stopIt = true;
	}
}
