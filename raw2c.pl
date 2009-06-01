#!/usr/bin/perl -w
# Turns binary files into C source code (static const unsigned char arrays).
use strict;
for (@ARGV) {
	open INPUT, $_ and binmode INPUT or die "$_: $!\n";
	s!.*[/\\]!!;
	y/0-9A-Za-z/_/c;
	print "static const unsigned char ${_}[] = {\n\t";
	$/ = \1;
	$. = 0;
	printf '%s0x%02X', $. % 16 != 1 ? ', ' : $. > 1 ? ",\n\t" : '', ord
		while <INPUT>;
	print "\n};\n"
}
