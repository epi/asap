/*
 * asapweb.js - pure JavaScript ASAP for web browsers
 *
 * Copyright (C) 2009-2019  Piotr Fusik
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

var asap = {
	stop : function() {
		var processor = window.asap.processor;
		if (processor) {
			processor.disconnect();
			delete window.asap.processor;
		}
	},

	playContent : function(filename, content, song)
	{
		var asap = new ASAP();
		asap.load(filename, content, content.length);
		var info = asap.getInfo();
		window.asap.author = info.getAuthor();
		window.asap.title = info.getTitle();
		window.asap.date = info.getDate();
		if (song === undefined)
			song = info.getDefaultSong();
		asap.playSong(song, -1);

		window.asap.stop();
		var length = 4096;
		var channels = asap.getInfo().getChannels();
		var buffer = new Uint8Array(new ArrayBuffer(length * channels));

		var AudioContext = window.AudioContext || window.webkitAudioContext;
		var context = new AudioContext({ sampleRate : ASAP.SAMPLE_RATE });
		var processor = context.createScriptProcessor(length, 0, channels);
		processor.onaudioprocess = function (e) {
			asap.generate(buffer, length * channels, ASAPSampleFormat.U8);
			for (var c = 0; c < channels; c++) {
				var output = e.outputBuffer.getChannelData(c);
				for (var i = 0; i < length; i++)
					output[i] = (buffer[i * channels + c] - 128) / 128;
			}
		};
		processor.connect(context.destination);
		window.asap.processor = processor;
	},

	playUrl : function(url)
	{
		var request = new XMLHttpRequest();
		request.open("GET", url, true);
		request.responseType = "arraybuffer";
		request.onload = function (e) {
			if (this.status == 200 || this.status == 0)
				asap.playContent(url, new Uint8Array(this.response));
		};
		request.send();
	},

	playFile : function(file)
	{
		var reader = new FileReader();
		reader.onload = function (e) {
			asap.playContent(file.name, new Uint8Array(e.target.result));
		};
		reader.readAsArrayBuffer(file);
	}
};
