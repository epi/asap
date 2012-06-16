/*
 * asapweb.js - pure JavaScript ASAP for web browsers
 *
 * Copyright (C) 2009-2012  Piotr Fusik
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

function asapPlay(url, song)
{
	var module = downloadBinaryFile(url);
	var asap = new ASAP();
	asap.load(url, module, module.length);
	var info = asap.getInfo();
	if (song < 0)
		song = info.getDefaultSong();
	asap.playSong(song, info.getDuration(song));

	function audioCallback(samplesRequested)
	{
		var buffer = new Array(samplesRequested);
		buffer.length = asap.generate(buffer, samplesRequested, ASAPSampleFormat.U8);
		for (var i = 0; i < buffer.length; i++)
			buffer[i] = (buffer[i] - 128) / 128;
		return buffer;
	}
	function failureCallback()
	{
		alert("JavaScript sound not supported by your browser");
	}
	var audio = new XAudioServer(info.getChannels(), ASAP.SAMPLE_RATE, 4096, 8192, audioCallback, 1, failureCallback);
	function heartbeat()
	{
		audio.executeCallback();
	}
	setInterval(heartbeat, 50);
}
