Name: asap
Version: 2.1.1
Release: 1
Summary: Player of 8-bit Atari music
License: GPL
Group: Applications/Multimedia
Packager: Piotr Fusik <fox@scene.pl>
Source: http://prdownloads.sourceforge.net/asap/asap-%{version}.tar.gz
URL: http://asap.sourceforge.net/
BuildRoot: %{_tmppath}/asap-root

%description
ASAP is a player of 8-bit Atari music for modern computers.
It emulates the POKEY sound chip and the 6502 processor.
ASAP supports the following file formats:
SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2.

%prep
%setup -q

%build
make

%install
rm -rf $RPM_BUILD_ROOT
make DESTDIR=$RPM_BUILD_ROOT PREFIX=%{_prefix} install

%clean
rm -rf $RPM_BUILD_ROOT

%files
%defattr(-,root,root)
%doc README.html
%{_bindir}/asapconv
%{_includedir}/asap.h
%{_libdir}/libasap.a
