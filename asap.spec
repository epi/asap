Name: asap
Version: 4.0.0
Release: 1
Summary: Player of Atari 8-bit music
License: GPLv2+
Group: Applications/Multimedia
Source: http://prdownloads.sourceforge.net/asap/asap-%{version}.tar.gz
URL: http://asap.sourceforge.net/
BuildRequires: gcc
BuildRoot: %{_tmppath}/asap-root

%description
ASAP is a player of Atari 8-bit music for modern computers.
It emulates the POKEY sound chip and the 6502 processor.
ASAP supports the following file formats:
SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2, FC.

%package devel
Summary: Development library providing Atari 8-bit music emulation
Group: Development/Libraries

%description devel
These are the files needed for compiling programs that use libasap.

%package xmms
Summary: ASAP plugin for XMMS
Group: Applications/Multimedia
Requires: xmms
BuildRequires: xmms-devel

%description xmms
Provides playback of Atari 8-bit music in XMMS.
Supports the following file formats:
SAP, CMC, CM3, CMR, CMS, DMC, DLT, MPT, MPD, RMT, TMC, TM8, TM2, FC.

%package vlc
Summary: ASAP plugin for VLC
Group: Applications/Multimedia
Requires: vlc
BuildRequires: vlc-devel

%description vlc
Provides playback of Atari 8-bit music in VLC.
Supports the following file formats: SAP, RMT, FC.

%prep
%setup -q

%build
make asapconv libasap.a asap-xmms asap-vlc

%install
rm -rf $RPM_BUILD_ROOT
make DESTDIR=$RPM_BUILD_ROOT prefix=%{_prefix} install install-xmms install-vlc

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

%files xmms
%defattr(-,root,root)
%{_libdir}/xmms/Input/libasap-xmms.so

%files vlc
%defattr(-,root,root)
%{_libdir}/vlc/plugins/demux/libasap_plugin.so

%changelog
* Thu Jan 10 2019 Piotr Fusik <fox@scene.pl>
- 4.0.0-1

* Sat Aug 12 2017 Piotr Fusik <fox@scene.pl>
- Discontinued GStreamer

* Mon Jun 23 2014 Piotr Fusik <fox@scene.pl>
- 3.2.0-1

* Wed Jan 15 2014 Piotr Fusik <fox@scene.pl>
- 3.1.6-1

* Fri Aug 16 2013 Piotr Fusik <fox@scene.pl>
- 3.1.5-1
- Corrected descriptions of GStreamer and VLC plugins - they don't support all the formats

* Mon Apr 29 2013 Piotr Fusik <fox@scene.pl>
- 3.1.4-1
- lib64 compatibility
- Removed the Audacious subpackage

* Tue Dec 4 2012 Piotr Fusik <fox@scene.pl>
- 3.1.3-1
- Added subpackages with GStreamer and VLC plugins

* Mon Jun 25 2012 Piotr Fusik <fox@scene.pl>
- 3.1.2-1

* Wed Oct 26 2011 Piotr Fusik <fox@scene.pl>
- 3.1.1-1

* Sat Sep 24 2011 Piotr Fusik <fox@scene.pl>
- 3.1.0-1

* Fri Jul 15 2011 Piotr Fusik <fox@scene.pl>
- 3.0.1-1

* Thu May 19 2011 Piotr Fusik <fox@scene.pl>
- 3.0.0-1
- Added subpackages with Audacious and XMMS plugins

* Wed Nov 3 2010 Piotr Fusik <fox@scene.pl>
- 2.1.2-1
- Initial packaging
