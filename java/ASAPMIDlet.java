/*
 * ASAPMIDlet.java - ASAP midlet
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

import java.io.InputStream;
import java.io.IOException;
import java.util.Enumeration;
import java.util.Vector;
import javax.microedition.io.Connector;
import javax.microedition.io.file.FileConnection;
import javax.microedition.io.file.FileSystemRegistry;
import javax.microedition.lcdui.Alert;
import javax.microedition.lcdui.AlertType;
import javax.microedition.lcdui.Command;
import javax.microedition.lcdui.CommandListener;
import javax.microedition.lcdui.Display;
import javax.microedition.lcdui.Displayable;
import javax.microedition.lcdui.Form;
import javax.microedition.lcdui.Gauge;
import javax.microedition.lcdui.List;
import javax.microedition.media.Manager;
import javax.microedition.media.MediaException;
import javax.microedition.media.Player;
import javax.microedition.media.PlayerListener;
import javax.microedition.midlet.MIDlet;

import net.sf.asap.ASAP;
import net.sf.asap.ASAP_ModuleInfo;

class FileList
{
	String caption;
	String path;
	String[] names;
	FileList parent;
	FileList[] sublists;

	private static boolean lessOrEqual(String name1, String name2)
	{
		boolean dir1 = name1.endsWith("/");
		boolean dir2 = name2.endsWith("/");
		if (dir1 != dir2)
			return dir1;
		return (name1.compareTo(name2) <= 0);
	}

	private void sort(int start, int end)
	{
		while (start + 1 < end) {
			int left = start + 1;
			int right = end;
			String pivot = names[start];
			String tmp;
			while (left < right) {
				if (lessOrEqual(names[left], pivot))
					left++;
				else {
					right--;
					tmp = names[left];
					names[left] = names[right];
					names[right] = tmp;
				}
			}
			left--;
			tmp = names[left];
			names[left] = names[start];
			names[start] = tmp;
			sort(start, left);
			start = right;
		}
	}

	FileList(String caption, String path, FileList parent, Enumeration contents)
	{
		this.caption = caption;
		this.path = path;
		this.parent = parent;
		Vector v = new Vector();
		int start = 0;
		if (parent != null) {
			v.addElement("..");
			start = 1;
		}
		while (contents.hasMoreElements()) {
			String name = (String) contents.nextElement();
			if (name.endsWith("/") || ASAP.isOurFile(name))
				v.addElement(name);
		}
		int n = v.size();
		names = new String[n];
		sublists = new FileList[n];
		for (int i = 0; i < n; i++)
			names[i] = (String) v.elementAt(i);
		sort(start, n);
	}
}

class ASAPInputStream extends InputStream
{
	private ASAP asap;
	private Gauge gauge;
	private int[] header = new int[11];
	private byte[] buffer = new byte[4096];
	private int buffer_len;
	private int offset = 0;
	private int position = 0;

	private static final int BITS_PER_SAMPLE = 8;

	ASAPInputStream(ASAP asap, Gauge gauge)
	{
		this.asap = asap;
		this.gauge = gauge;
		ASAP_ModuleInfo module_info = asap.getModuleInfo();
		int song = module_info.default_song;
		int duration = module_info.durations[song];
		if (duration < 0)
			duration = 180000;
		asap.playSong(song, duration);
		int channels = module_info.channels;
		int block_size = channels * (BITS_PER_SAMPLE / 8);
		int bytes_per_second = ASAP.SAMPLE_RATE * block_size;
		int n_bytes = duration * (ASAP.SAMPLE_RATE / 100) / 10 * block_size;
		gauge.setMaxValue(n_bytes / buffer.length);
		header[0] = 'R' + ('I' << 8) + ('F' << 16) + ('F' << 24);
		header[1] = n_bytes + 36;
		header[2] = 'W' + ('A' << 8) + ('V' << 16) + ('E' << 24);
		header[3] = 'f' + ('m' << 8) + ('t' << 16) + (' ' << 24);
		header[4] = 16;
		header[5] = 1 + (channels << 16);
		header[6] = ASAP.SAMPLE_RATE;
		header[7] = bytes_per_second;
		header[8] = block_size + (BITS_PER_SAMPLE << 16);
		header[9] = 'd' + ('a' << 8) + ('t' << 16) + ('a' << 24);
		header[10] = n_bytes;
		buffer_len = asap.generate(buffer, BITS_PER_SAMPLE);
	}

	public int read()
	{
		int i = offset;
		if (i < 44)
			i = header[i >> 2] >> ((i & 3) << 3);
		else {
			i -= 44;
			if (i >= buffer_len) {
				if (buffer_len < buffer.length)
					return -1;
				buffer_len = asap.generate(buffer, BITS_PER_SAMPLE);
				gauge.setValue(++position);
				i = 0;
				offset = 44;
			}
			i = buffer[i];
		}
		offset++;
		return i & 0xff;
	}
}

public class ASAPMIDlet extends MIDlet implements CommandListener, PlayerListener
{
	private Command selectCommand;
	private Command stopCommand;
	private Command exitCommand;
	private FileList fileList;
	private List listControl;
	private byte[] module;
	private ASAP asap;
	private Player player;

	public ASAPMIDlet()
	{
	}

	private void displayFileList(FileList fileList)
	{
		this.fileList = fileList;
		this.listControl = new List(fileList.caption, List.IMPLICIT, fileList.names, null);
		listControl.addCommand(selectCommand);
		listControl.addCommand(exitCommand);
		listControl.setCommandListener(this);
		Display.getDisplay(this).setCurrent(listControl);
	}

	private void displayError(String message)
	{
		Alert alert = new Alert("Error", message, null, AlertType.ERROR);
		Display.getDisplay(this).setCurrent(alert);
	}

	public void startApp()
	{
		selectCommand = new Command("Select", Command.ITEM, 1);
		stopCommand = new Command("Stop", Command.STOP, 1);
		exitCommand = new Command("Exit", Command.EXIT, 2);
		module = new byte[ASAP.MODULE_MAX];
		asap = new ASAP();
		displayFileList(new FileList("Select File", "", null, FileSystemRegistry.listRoots()));
	}

	public void commandAction(Command c, Displayable s)
	{
		if (c == selectCommand || c == List.SELECT_COMMAND) {
			int index = listControl.getSelectedIndex();
			String filename = fileList.names[index];
			if (filename.equals("..")) {
				displayFileList(fileList.parent);
				return;
			}
			String path = fileList.path + filename;
			try {
				FileList newList = fileList.sublists[index];
				if (newList != null) {
					displayFileList(newList);
					return;
				}
				FileConnection fc = (FileConnection) Connector.open("file:///" + path, Connector.READ);
				if (fc.isDirectory()) {
					newList = new FileList(filename, path, fileList, fc.list());
					fileList.sublists[index] = newList;
					displayFileList(newList);
					return;
				}
				InputStream is = fc.openInputStream();
				int module_len = 0;
				for (;;) {
					int i = is.read(module, module_len, ASAP.MODULE_MAX - module_len);
					if (i <= 0)
						break;
					module_len += i;
				}
				is.close();
				asap.load(filename, module, module_len);
				Gauge gauge = new Gauge(filename, false, 1, 0);
				is = new ASAPInputStream(asap, gauge);
				player = Manager.createPlayer(is, "audio/x-wav");
				player.addPlayerListener(this);
				player.start();
				Form playForm = new Form("ASAP");
				playForm.append(gauge);
				playForm.addCommand(stopCommand);
				playForm.addCommand(exitCommand);
				playForm.setCommandListener(this);
				Display.getDisplay(this).setCurrent(playForm);
			} catch (Exception ex) {
				displayError(ex.toString());
			}
		}
		else if (c == stopCommand) {
			try {
				player.stop();
			} catch (MediaException ex) {
			}
			Display.getDisplay(this).setCurrent(listControl);
		}
		else if (c == exitCommand)
			notifyDestroyed();
	}

	public void playerUpdate(Player player, String event, Object eventData)
	{
		if (event == PlayerListener.END_OF_MEDIA)
			Display.getDisplay(this).setCurrent(listControl);
		else if (event == PlayerListener.ERROR)
			displayError((String) eventData);
	}

	public void pauseApp()
	{
	}

	public void destroyApp(boolean unconditional)
	{
	}
}
