/*
 * asap2wav1.js - converter of ASAP-supported formats to WAV files
 *
 * Copyright (C) 2009-2020 Piotr Fusik
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

var usage;

if (typeof(java) == "object") {
	// Rhino

	usage = "java -jar js.jar -opt -1 asap2wav.js [OPTIONS] INPUTFILE...";

	var readBinaryFile = function(filename)
	{
		var stream = new java.io.FileInputStream(filename);
		var bytes = new Array();
		for (;;) {
			var c = stream.read();
			if (c < 0)
				break;
			bytes.push(c);
		}
		return bytes;
	}

	var BinaryFileOutput = function(filename)
	{
		this.stream = new java.io.BufferedOutputStream(new java.io.FileOutputStream(filename));

		this.write = function(bytes, len)
		{
			for (var i = 0; i < len; i++)
				this.stream.write(bytes[i]);
		}

		this.close = function()
		{
			this.stream.close();
		}
	}
}
else {
	// DMDScript, JaegerMonkey or d8

	if (typeof(readline) == "undefined") {
		var readline = readln;
		var write = print;
		var args = getenv("ARGS");
		arguments = args == null ? [] : args.split(" ");
		usage = "\nset ARGS=[OPTIONS] INPUTFILE\nbase64 INPUTFILE | ds asap2wav.js | base64 -d > OUTFILE";
	}
	else if (typeof(write) == "undefined") {
		var write = putstr;
		usage = "base64 INPUTFILE | js asap2wav.js [OPTIONS] INPUTFILE | base64 -d > OUTFILE";
	}
	else
		usage = "base64 INPUTFILE | d8 asap2wav.js -- [OPTIONS] INPUTFILE | base64 -d > OUTFILE";

	var base64chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

	var readBinaryFile = function(filename)
	{
		var bytes = new Array();
		var prev;
		var gotBits = 0;
		for (;;) {
			var line = readline();
			if (line == null || line.length == 0)
				break;
			for (var i = 0; i < line.length; i++) {
				var v = base64chars.indexOf(line.charAt(i));
				if (v >= 0) {
					gotBits = (gotBits + 6) & 6;
					if (gotBits != 6)
						bytes.push((prev << 6 | v) >> gotBits & 0xff);
					prev = v;
				}
			}
		}
		return bytes;
	}

	var BinaryFileOutput = function(filename)
	{
		this.prev = 0;
		this.gotBits = 0;

		this.write = function(bytes, len)
		{
			for (var i = 0; i < len; i++) {
				this.gotBits += 2;
				var v = this.prev << 8 | bytes[i];
				write(base64chars.charAt(v >> this.gotBits & 63));
				if (this.gotBits == 6) {
					write(base64chars.charAt(v & 63));
					this.gotBits = 0;
				}
				this.prev = bytes[i];
			}
		}

		this.close = function()
		{
			if (this.gotBits == 2) {
				write(base64chars.charAt((this.prev & 3) << 4));
				write("==");
			}
			else if (this.gotBits == 4) {
				write(base64chars.charAt((this.prev & 15) << 2));
				write("=");
			}
		}
	}
}

var outputFilename = null;
var outputHeader = true;
var song = -1;
var format = ASAPSampleFormat.S16_L_E;
var duration = -1;
var muteMask = 0;

function printHelp()
{
	print(
		"Usage: " + usage + "\n" +
		"Each INPUTFILE must be in a supported format:\n" +
		"SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2 or FC.\n" +
		"Options:\n" +
		"-o FILE     --output=FILE      Set output file name\n" +
		"-s SONG     --song=SONG        Select subsong number (zero-based)\n" +
		"-t TIME     --time=TIME        Set output length (MM:SS format)\n" +
		"-b          --byte-samples     Output 8-bit samples\n" +
		"-w          --word-samples     Output 16-bit samples (default)\n" +
		"            --raw              Output raw audio (no WAV header)\n" +
		"-m CHANNELS --mute=CHANNELS    Mute POKEY channels (1-8)\n" +
		"-h          --help             Display this information\n" +
		"-v          --version          Display version information"
	);
}

function setSong(s)
{
	song = parseInt(s, 10);
}

function setTime(s)
{
	duration = ASAPInfo.parseDuration(s);
}

function setMuteMask(s)
{
	muteMask = 0;
	for (var i = 0; i < s.length; i++) {
		var ch = s.charCodeAt(i) - 49;
		if (ch >= 0 && ch < 8)
			muteMask |= 1 << ch;
	}
}

function processFile(inputFilename)
{
	var module = readBinaryFile(inputFilename);
	var asap = new ASAP();
	asap.load(inputFilename, module, module.length);
	var info = asap.getInfo();
	if (song < 0)
		song = info.getDefaultSong();
	if (duration < 0) {
		duration = info.getDuration(song);
		if (duration < 0)
			duration = 180 * 1000;
	}
	asap.playSong(song, duration);
	asap.mutePokeyChannels(muteMask);
	if (outputFilename == null) {
		var i = inputFilename.lastIndexOf(".");
		outputFilename = inputFilename.substring(0, i + 1) + (outputHeader ? "wav" : "raw");
	}
	var of = new BinaryFileOutput(outputFilename);
	var buffer = new Array(8192);
	var nBytes;
	if (outputHeader) {
		nBytes = asap.getWavHeader(buffer, format, false);
		of.write(buffer, nBytes);
	}
	do {
		nBytes = asap.generate(buffer, 8192, format);
		of.write(buffer, nBytes);
	} while (nBytes == 8192);
	of.close();
	outputFilename = null;
	song = -1;
	duration = -1;
}

var noInputFiles = true;
for (var i = 0; i < arguments.length; i++) {
	var arg = arguments[i];
	if (arg.charAt(0) != "-") {
		processFile(arg);
		noInputFiles = false;
	}
	else if (arg == "-o")
		outputFilename = arguments[++i];
	else if (arg.substring(0, 9) == "--output=")
		outputFilename = arg.substring(9, arg.length);
	else if (arg == "-s")
		setSong(arguments[++i]);
	else if (arg.substring(0, 7) == "--song=")
		setSong(arg.substring(7, arg.length));
	else if (arg == "-t")
		setTime(arguments[++i]);
	else if (arg.substring(0, 7) ==  "--time=")
		setTime(arg.substring(7, arg.length));
	else if (arg == "-b" || arg == "--byte-samples")
		format = ASAPSampleFormat.U8;
	else if (arg == "-w" || arg == "--word-samples")
		format = ASAPSampleFormat.S16_L_E;
	else if (arg == "--raw")
		outputHeader = false;
	else if (arg == "-m")
		setMuteMask(arguments[++i]);
	else if (arg.substring(0, 7) == "--mute=")
		setMuteMask(arg.substring(7, arg.length));
	else if (arg == "-h" || arg == "--help") {
		printHelp();
		noInputFiles = false;
	}
	else if (arg == "-v" || arg == "--version") {
		print("ASAP2WAV (JavaScript) " + ASAPInfo.VERSION);
		noInputFiles = false;
	}
	else
		throw "unknown option: " + arg;
}
if (noInputFiles) {
	printHelp();
	quit(1);
}
