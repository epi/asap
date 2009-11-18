#!perl -l
# Generate mutual exclusion conditions
@a = qw($wasap.ext=3 $asap_dsf.ext=3 $in_asap.ext=3 $foo_asap.ext=3 $xbmc_asap.ext=3 $ASAP_Apollo.ext=3);
$" = " OR ";
for $a (@a[0 .. $#a - 1]) {
	$_ .= $" if $_;
	$_ .= "($a AND (@a[++$i .. $#a]))";
	print, $_ = '' if length > 200;
}
print;
