#!/usr/bin/perl -w
# Turns binary files into C source code (static const unsigned char arrays).
use strict;
for (@ARGV) {
	open INPUT, $_ and binmode INPUT or die "$_: $!\n";
	s!.*[/\\]!!;
	y/0-9A-Za-z/_/c;
	print "static const unsigned char ${_}[] = {\n\t";
	my $buf;
	print join ', ', map sprintf('0x%02X', $_), unpack 'C*', $buf
		if read INPUT, $buf, 16;
	print ",\n\t", join ', ', map sprintf('0x%02X', $_), unpack 'C*', $buf
		while read INPUT, $buf, 16;
	close INPUT;
	print "\n};\n"
}
