/*
 * asap2wav.js - converter of ASAP-supported formats to WAV files
 *
 * Copyright (C) 2009-2023 Piotr Fusik
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

import fs from "fs";
import { ASAP, ASAPInfo, ASAPSampleFormat } from "./asap.mjs";

function printHelp()
{
	console.log(
		"Usage: node asap2wav.mjs [OPTIONS] INPUTFILE...\n" +
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

let outputFilename = null;
let outputHeader = true;
let song = -1;
let format = ASAPSampleFormat.S16_L_E;
let duration = -1;
let muteMask = 0;

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
	for (let i = 0; i < s.length; i++) {
		const ch = s.charCodeAt(i) - 49;
		if (ch >= 0 && ch < 8)
			muteMask |= 1 << ch;
	}
}

function processFile(inputFilename)
{
	const module = fs.readFileSync(inputFilename);
	const asap = new ASAP();
	asap.load(inputFilename, module, module.length);
	const info = asap.getInfo();
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
		const i = inputFilename.lastIndexOf(".");
		outputFilename = inputFilename.substring(0, i + 1) + (outputHeader ? "wav" : "raw");
	}
	const of = fs.openSync(outputFilename, "w");
	const buffer = new Uint8Array(8192);
	let nBytes;
	if (outputHeader) {
		nBytes = asap.getWavHeader(buffer, format, false);
		fs.writeSync(of, buffer, 0, nBytes);
	}
	do {
		nBytes = asap.generate(buffer, 8192, format);
		fs.writeSync(of, buffer, 0, nBytes);
	} while (nBytes == 8192);
	fs.closeSync(of);
	outputFilename = null;
	song = -1;
	duration = -1;
}

const args = process.argv.slice(2);
let noInputFiles = true;
for (let i = 0; i < args.length; i++) {
	const arg = args[i];
	if (arg.charAt(0) != "-") {
		processFile(arg);
		noInputFiles = false;
	}
	else if (arg == "-o")
		outputFilename = args[++i];
	else if (arg.substring(0, 9) == "--output=")
		outputFilename = arg.substring(9, arg.length);
	else if (arg == "-s")
		setSong(args[++i]);
	else if (arg.substring(0, 7) == "--song=")
		setSong(arg.substring(7, arg.length));
	else if (arg == "-t")
		setTime(args[++i]);
	else if (arg.substring(0, 7) ==  "--time=")
		setTime(arg.substring(7, arg.length));
	else if (arg == "-b" || arg == "--byte-samples")
		format = ASAPSampleFormat.U8;
	else if (arg == "-w" || arg == "--word-samples")
		format = ASAPSampleFormat.S16_L_E;
	else if (arg == "--raw")
		outputHeader = false;
	else if (arg == "-m")
		setMuteMask(args[++i]);
	else if (arg.substring(0, 7) == "--mute=")
		setMuteMask(arg.substring(7, arg.length));
	else if (arg == "-h" || arg == "--help") {
		printHelp();
		noInputFiles = false;
	}
	else if (arg == "-v" || arg == "--version") {
		console.log("ASAP2WAV (JavaScript) " + ASAPInfo.VERSION);
		noInputFiles = false;
	}
	else
		throw "unknown option: " + arg;
}
if (noInputFiles)
	printHelp();
