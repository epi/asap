/*
 * MainMenu.java - ASAP for Android
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

import android.app.Activity;
import android.app.AlertDialog;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;

class MainMenu
{
	static boolean onCreateOptionsMenu(Activity activity, Menu menu)
	{
		MenuInflater inflater = activity.getMenuInflater();
		inflater.inflate(R.menu.main, menu);
		return true;
	}

	static void showAbout(Activity activity)
	{
		new AlertDialog.Builder(activity).setTitle(R.string.about_title).setIcon(R.drawable.icon).setMessage(R.string.about_message).show();
	}

	static boolean onOptionsItemSelected(Activity activity, MenuItem item)
	{
		switch (item.getItemId()) {
		case R.id.menu_about:
			showAbout(activity);
			return true;
		default:
			return false;
		}
	}
}
