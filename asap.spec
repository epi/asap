Name: asap
Version: 3.0.0
Release: 1
Summary: Player of 8-bit Atari music
License: GPLv2+
Group: Applications/Multimedia
Source: http://prdownloads.sourceforge.net/asap/asap-%{version}.tar.gz
URL: http://asap.sourceforge.net/
BuildRoot: %{_tmppath}/asap-root

%description
ASAP is a player of 8-bit Atari music for modern computers.
It emulates the POKEY sound chip and the 6502 processor.
ASAP supports the following file formats:
SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2.

%package devel
Summary: Development library with 8-bit Atari music emulation
Group: Development/Libraries

%description devel
These are the files needed for compiling programs that use libasap.

%package audacious
Summary: ASAP plugin for Audacious
Group: Applications/Multimedia
Requires: audacious
BuildRequires: audacious-devel

%description audacious
Provides playback of 8-bit Atari music in Audacious.
Supports the following file formats:
SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2.

%package xmms
Summary: ASAP plugin for XMMS
Group: Applications/Multimedia
Requires: xmms
BuildRequires: xmms-devel

%description xmms
Provides playback of 8-bit Atari music in XMMS.
Supports the following file formats:
SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2.

%prep
%setup -q

%build
make asapconv libasap.a asap-audacious asap-xmms

%install
rm -rf $RPM_BUILD_ROOT
make DESTDIR=$RPM_BUILD_ROOT prefix=%{_prefix} install install-audacious install-xmms

%clean
rm -rf $RPM_BUILD_ROOT

%files
%defattr(-,root,root)
%doc README.html
%{_bindir}/asapconv

%files devel
%defattr(-,root,root)
%{_includedir}/asap.h
%{_libdir}/libasap.a

%files audacious
%defattr(-,root,root)
/usr/lib/audacious/Input/asapplug.so

%files xmms
%defattr(-,root,root)
/usr/lib/xmms/Input/libasap-xmms.so

%changelog
* Thu May 19 2011 Piotr Fusik <fox@scene.pl>
- 3.0.0-1
- Added subpackages with Audacious and XMMS plugins

* Wed Nov 3 2010 Piotr Fusik <fox@scene.pl>
- 2.1.2-1
- Initial packaging
