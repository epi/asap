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

import java.io.IOException;
import java.util.Enumeration;
import javax.microedition.io.Connector;
import javax.microedition.io.file.FileConnection;
import javax.microedition.io.file.FileSystemRegistry;
import javax.microedition.lcdui.Command;
import javax.microedition.lcdui.CommandListener;
import javax.microedition.lcdui.Display;
import javax.microedition.lcdui.Displayable;
import javax.microedition.lcdui.Form;
import javax.microedition.lcdui.List;
import javax.microedition.midlet.MIDlet;

public class ASAPMIDlet extends MIDlet implements CommandListener
{
	private List fileList;
	private Command selectCommand;
	private Command exitCommand;

	private String currentPath = "";

	private void reloadFileList()
	{
		for (int i = fileList.size(); --i >= 0; )
			fileList.delete(i);
		Enumeration e;
		if (currentPath.length() == 0) {
			fileList.setTitle("Select File");
			e = FileSystemRegistry.listRoots();
		}
		else {
			fileList.setTitle(currentPath);
			fileList.append("..", null);
			try {
				FileConnection fc = (FileConnection) Connector.open("file:///" + currentPath, Connector.READ);
				e = fc.list();
			} catch (IOException ex) {
				// TODO
				return;
			}
		}
		while (e.hasMoreElements())
			fileList.append((String) e.nextElement(), null);
	}

	public ASAPMIDlet()
	{
	}

	public void startApp()
	{
		fileList = new List(null, List.IMPLICIT);
		selectCommand = new Command("Select", Command.ITEM, 1);
		fileList.addCommand(selectCommand);
		exitCommand = new Command("Exit", Command.EXIT, 1);
		fileList.addCommand(exitCommand);
		fileList.setCommandListener(this);
		reloadFileList();
		Display.getDisplay(this).setCurrent(fileList);
	}

	public void commandAction(Command c, Displayable s)
	{
		if (c == selectCommand || c == List.SELECT_COMMAND) {
			String filename = fileList.getString(fileList.getSelectedIndex());
			if (filename.equals("..")) {
				int i = currentPath.lastIndexOf('/', currentPath.length() - 2);
				currentPath = currentPath.substring(0, i + 1);
			}
			else {
				currentPath += filename;
			}
			reloadFileList();
		}
		else if (c == exitCommand)
			notifyDestroyed();
	}

	public void pauseApp()
	{
	}

	public void destroyApp(boolean unconditional)
	{
	}
}
