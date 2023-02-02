#!/usr/bin/env -S dmd -g -O -run
import std.stdio;
import std.parallelism;
import std.file;
import std.path;
import std.uni;
import std.process;
import std.string;
import std.array;
import std.algorithm;
import std.range;
import std.conv : to;
import std.exception;
import std.getopt;
import core.atomic;

T parseInt(T)(in char[] t) {
	if (t.startsWith("0x") || t.startsWith("0X"))
		return t[2 .. $].to!T(16);
	if (t.startsWith("0b") || t.startsWith("0B"))
		return t[2 .. $].to!T(2);
	return t.to!T(10);
}

enum MemTrace : ubyte {
	file = 1,
	store = 2,
	load = 4,
	loadUninit = 8,
	push = 16,
	pull = 32,
	all = 63,
	hw = 128,
}

struct Region {
	ushort from;
	ushort to;
	ushort size() const => cast(ushort) (to - from + 1);

	this(in char[] s, uint radix = 0) {
		auto ll = s
			.split("-")
			.map!(a => radix > 0 ? a.to!ushort(radix) : a.parseInt!ushort);
		auto len = ll.length == 1 ? 1 : ll[1] - ll[0] + 1;
		from = ll[0];
		to = cast(ushort)(from + len - 1);
	}
}

struct SapInfo {
	this(string filename) {
		this.filename = filename;
		this.regions.length = 64;
		this.memory.length = 65536;
	}

	string filename;
	string[] tags;
	Region[][] regions;
	ubyte[] memory;

	ubyte stackUsage() const {
		return cast(ubyte) regions[32 .. $].join.map!(r => r.size).sum;
	}

	void scan() {
		auto p = pipeProcess(["asapscan", "-m", filename]);
		scope(exit) wait(p.pid);

		int i = -1;
		int mask = 0;
		auto labels = [
			"file data", "store", "load", "load uninitialized", "push", "pull"
		];
		foreach (l; p.stdout.byLine) {
			if (l.startsWith("Unused")) {
				i = 0;
			} else if (l.startsWith("Memory regions")) {
				auto ll = l.split(": ")[1].split(", ");
				i = 0;
				foreach (lll; ll) {
					foreach (j, k; labels) {
						if (lll == k)
							i |= 1 << j;
					}
				}
				mask |= i;
			} else if (i >= 0) {
				auto r = Region(l, 16);
				regions[i] ~= r;
				memory[r.from .. r.to + 1] = cast(ubyte) i;
			}
		}
	}

	bool loadFromCache(string cacheFile) {
		if (!cacheFile.exists)
			return false;
		if (!cacheFile.isFile)
			return false;
		if (cacheFile.timeLastModified < filename.timeLastModified)
			return false;
		File f = File(cacheFile);
		if (f.rawRead(memory[]).length != memory.length)
			return false;
		ushort[64] lengths;
		if (f.rawRead(lengths[]).length != lengths.length)
			return false;
		foreach (i, ref reg; regions) {
			reg.length = lengths[i];
			if (f.rawRead(reg[]).length != reg.length)
				return false;
		}
		return true;
	}

	void saveCacheFile(string cacheFile) {
		mkdirRecurse(cacheFile.dirName);
		auto f = File(cacheFile, "wb");
		f.rawWrite(memory[]);
		auto lengths = regions[].map!(a => cast(ushort) a.length).array;
		f.rawWrite(lengths);
		foreach (j, reg; regions[])
			f.rawWrite(reg);
	}
}

SapInfo[] loadAll(string dir, bool verbose)
{
	string cacheDir = buildPath(dir, ".asap-cache");

	SapInfo[] saps;
	saps = dirEntries(dir, SpanMode.breadth)
		.map!(e => e.name)
		.filter!(e => !e.startsWith(cacheDir))
		.filter!(e => sicmp(e.extension, ".sap") == 0)
		.map!(e => SapInfo(e.idup))
		.array
		.sort!((a, b) => a.filename < b.filename)
		.release;

	shared int n;
	foreach (ref sap; parallel(saps, 5)) {
		string cacheFile = buildNormalizedPath(cacheDir, "./" ~ sap.filename.chompPrefix(dir)).setExtension(".asapscan");
		if (!sap.loadFromCache(cacheFile)) {
			sap.scan();
			sap.saveCacheFile(cacheFile);
		}

		const w = n.atomicFetchAdd(1);
		if (verbose) {
			writefln("%5d. %-80s |%s| %3d", w, sap.filename,
				sap.regions[].map!(a => a.length > 0 ? 'x' : ' '),
				sap.stackUsage);
		} else {
			auto proc = ((w + 1) * 100) / saps.length;
			stderr.writef("%3d%%\r", proc);
		}
	}
	return saps;
}

void main(string[] args)
{
	bool printStats;
	bool printHistogram;
	uint binWidth = 256;
	bool verbose;
	ubyte accessMask = MemTrace.all;
	bool[] include;
	bool printHelp;
	bool list;

	void includeExclude(string opt, string val) {
		auto r = Region(val);
		if (opt == "i|include") {
			if (include.empty)
				include = false.repeat(65536).array;
			include[r.from .. r.to + 1] = true;
		} else if (opt == "e|exclude") {
			if (include.empty)
				include = true.repeat(65536).array;
			include[r.from .. r.to + 1] = false;
		} else {
			assert(0);
		}
	}

	auto help = getopt(args,
		config.caseSensitive,
		"s|stats", "Print basic cumulative statistics", &printStats,
		"H|histogram", &printHistogram,
		"w|bin-width", (string opt, string val) {
			binWidth = val.parseInt!uint;
		},
		"i|include", "Include specified address ranges", &includeExclude,
		"e|exclude", "Exclude specified address ranges", &includeExclude,
		"m|access-mask", "Include only specified access types in the histogram", (string opt, string val) {
			accessMask = val.parseInt!ubyte;
		},
		"l|list", "List matching modules", &list,
		"v|verbose", &verbose);

	if (include.length == 0)
		include = true.repeat(65536).array;

	if (help.helpWanted) {
		defaultGetoptPrinter("asmascan", help.options);
		return;
	}

	auto saps = loadAll(args[1], verbose);

	if (printStats) {
		auto rdu = saps.filter!(sap => sap.regions[8].length > 0).count;
		auto rduw = saps.filter!(sap => sap.regions[2 | 8].length > 0).count;
		auto rdurduw = saps.filter!(sap => sap.regions[2 | 8].length > 0 && sap.regions[8].length > 0).count;
		auto wro = saps.filter!(sap => sap.regions[2].length + sap.regions[6].length + sap.regions[10].length + sap.regions[14].length).count;
		writeln(saps.length, " modules");
		writeln(rdu, " modules read uninitialized data");
		writeln(rduw, " modules read uninitialized data and overwrite all of it");
		writeln(rdurduw, " modules read uninitialized data and sometimes overwrite it");
		writeln(wro, " write outside blocks read from file");
	}

	if (printHistogram) {
		size_t nbins = (65535 + binWidth) / binWidth;
		auto histogram = new uint[nbins];
		foreach (sap; saps) {
			foreach (bin; 0 .. nbins) {
				foreach (j; 0 .. binWidth) {
					const addr = bin * binWidth + j;
					if (addr >= 65536)
						break;
					if ((sap.memory[addr] & accessMask) && include[addr]) {
						histogram[bin]++;
						break;
					}
				}
			}
		}
		foreach (bin; 0 .. nbins)
			writefln("%4X %5d", bin, histogram[bin]);
	}

	if (list) {
		foreach (sap; saps) {
			foreach (addr, incl; include[]) {
				if (incl && (sap.memory[addr] & accessMask)) {
					writeln(sap.filename);
					break;
				}
			}
		}
	}
}
