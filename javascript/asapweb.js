/*
 * asapweb.js - pure JavaScript ASAP for web browsers
 *
 * Copyright (C) 2009-2011  Piotr Fusik
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

function downloadBinaryFile(url)
{
	try {
		var req = new XMLHttpRequest();
		req.open("GET", url, false);
		req.overrideMimeType("text/plain; charset=x-user-defined");
		req.send(null);
		if (req.status != 200 && req.status != 0)
			throw "Status: " + req.status;
		var response = req.responseText;
		var result = new Array(response.length);
		for (var i = 0; i < response.length; i++)
			result[i] = response.charCodeAt(i) & 0xff;
		return result;
	}
	catch (e) {
		throw "Error: Failed to load " + url;
	}
}

function ASAP2WAVURL(url, song, duration, format)
{
	var module = downloadBinaryFile(url);
	var asap = new ASAP();
	asap.load(url, module, module.length);
	var info = asap.getInfo();
	if (song < 0)
		song = info.getDefaultSong();
	if (duration != null)
		duration = ASAPInfo.parseDuration(duration);
	else {
		duration = info.getDuration(song);
		if (duration < 0)
			duration = 180 * 1000;
	}
	asap.playSong(song, duration);

	var result = "";
	var base64chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
	function appendBase64(data, len)
	{
		for (var i = 0; i < len; ) {
			var b1 = data[i++] & 0xff;
			var r = base64chars.charAt(b1 >> 2);
			if (i < len) {
				var b2 = data[i++] & 0xff;
				r += base64chars.charAt(((b1 & 3) << 4) + (b2 >> 4));
				if (i < len) {
					var b3 = data[i++] & 0xff;
					r += base64chars.charAt(((b2 & 15) << 2) + (b3 >> 6)) + base64chars.charAt(b3 & 63);
				}
				else
					r += base64chars.charAt((b2 & 15) << 2) + "=";
			}
			else
				r += base64chars.charAt(b1 << 4) + "==";
			result += r;
		}
	}

	var buffer = new Array(8193); // must be multiple of 3
	var got = asap.getWavHeader(buffer, format, false);
	for (;;) {
		got += asap.generateAt(buffer, got, buffer.length - got, format);
		appendBase64(buffer, got);
		if (got < buffer.length)
			break;
		got = 0;
	}
	return "data:audio/x-wav;base64," + result;
}
