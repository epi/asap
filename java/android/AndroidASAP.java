/*
 * AndroidASAP.java - ASAP for Android
 *
 * Copyright (C) 2010  Piotr Fusik
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

import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;

import android.app.ListActivity;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;

public class AndroidASAP extends ListActivity
{
	private ArrayAdapter<String> listAdapter;
	private File currentDir;
	private final byte[] module = new byte[ASAP.MODULE_MAX];
	private final ASAP asap = new ASAP();
	private final byte[] buffer = new byte[8192];

	private void enterDirectory(File newDir)
	{
		currentDir = newDir;
		listAdapter.clear();
		if (newDir.getParentFile() != null)
			listAdapter.add("..");
		for (File file : newDir.listFiles())
		{
			String name = file.getName();
			if (file.isDirectory())
				listAdapter.add(name + '/');
			else if (ASAP.isOurFile(name))
				listAdapter.add(name);
		}
	}

	@Override
	public void onCreate(Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);
		listAdapter = new ArrayAdapter<String>(this, R.layout.listitem);
		setListAdapter(listAdapter);
		enterDirectory(new File("/"));
		setContentView(R.layout.filelist);
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id)
	{
		String name = listAdapter.getItem(position);
		if (name.equals(".."))
			enterDirectory(currentDir.getParentFile());
		else if (name.endsWith("/"))
			enterDirectory(new File(currentDir, name));
		else {
			try {
				FileInputStream fis = new FileInputStream(new File(currentDir, name));
				int module_len = ASAP.readAndClose(fis, module);
				asap.load(name, module, module_len);
				ASAP_ModuleInfo module_info = asap.getModuleInfo();
				int song = module_info.default_song;
				int duration = module_info.loops[song] ? -1 : module_info.durations[song];
				asap.playSong(song, duration);
				AudioTrack audioTrack = new AudioTrack(AudioManager.STREAM_MUSIC, ASAP.SAMPLE_RATE,
					module_info.channels == 1 ? AudioFormat.CHANNEL_CONFIGURATION_MONO : AudioFormat.CHANNEL_CONFIGURATION_STEREO,
					AudioFormat.ENCODING_PCM_8BIT, buffer.length, AudioTrack.MODE_STREAM);
				audioTrack.play();
				int len;
				do {
					len = asap.generate(buffer, ASAP.FORMAT_U8);
					audioTrack.write(buffer, 0, len);
				} while (len == buffer.length);
				audioTrack.flush();
				audioTrack.stop();
			}
			catch (IOException ex) {
				// TODO
			}
		}
	}
}
