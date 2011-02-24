/*
 * SilverASAP.cs - Silverlight version of ASAP
 *
 * Copyright (C) 2010-2011  Piotr Fusik
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Media;

using Sf.Asap;

class ASAPMediaStreamSource : MediaStreamSource
{
	const int BitsPerSample = 16;
	static readonly Dictionary<MediaSampleAttributeKeys, string> SampleAttributes = new Dictionary<MediaSampleAttributeKeys, string>();

	readonly ASAP asap;
	readonly int duration;
	MediaStreamDescription mediaStreamDescription;

	public ASAPMediaStreamSource(ASAP asap, int duration)
	{
		this.asap = asap;
		this.duration = duration;
	}

	static string LittleEndianHex(int x)
	{
		return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", x & 0xff, (x >> 8) & 0xff, (x >> 16) & 0xff, (x >> 24) & 0xff);
	}

	protected override void OpenMediaAsync()
	{
		int channels = asap.ModuleInfo.Channels;
		int blockSize = channels * BitsPerSample >> 3;
		string waveFormatHex = string.Format("0100{0:X2}00{1}{2}{3:X2}00{4:X2}000000",
			channels, LittleEndianHex(ASAP.SampleRate), LittleEndianHex(ASAP.SampleRate * blockSize),
			blockSize, BitsPerSample);
		Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
		streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = waveFormatHex;
		this.mediaStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);

		Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
		sourceAttributes[MediaSourceAttributesKeys.CanSeek] = "True";
		sourceAttributes[MediaSourceAttributesKeys.Duration] = (duration < 0 ? 0 : duration * 10000).ToString();

		ReportOpenMediaCompleted(sourceAttributes, new MediaStreamDescription[1] { this.mediaStreamDescription });
	}

	protected override void GetSampleAsync(MediaStreamType mediaStreamType)
	{
		byte[] buffer = new byte[8192];
		int blocksPlayed;
		int bufferLen;
		lock (asap) {
			blocksPlayed = asap.BlocksPlayed;
			bufferLen = asap.Generate(buffer, buffer.Length, BitsPerSample == 8 ? ASAPSampleFormat.U8 : ASAPSampleFormat.S16LE);
		}
		Stream s = new MemoryStream(buffer);
		MediaStreamSample mss = new MediaStreamSample(this.mediaStreamDescription, s, 0, bufferLen,
			blocksPlayed * 10000000 / ASAP.SampleRate, SampleAttributes);
		ReportGetSampleCompleted(mss);
	}

	protected override void SeekAsync(long seekToTime)
	{
		lock (asap)
			asap.Seek((int) (seekToTime / 10000));
		ReportSeekCompleted(seekToTime);
	}

	protected override void CloseMedia()
	{
	}

	protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
	{
		throw new NotImplementedException();
	}

	protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
	{
		throw new NotImplementedException();
	}

}

public class SilverASAP : Application
{
	int defaultPlaybackTime = -1;
	int loopPlaybackTime = -1;
	const int Once = -2; // for loopPlaybackTime
	string filename;
	int song = -1;
	WebClient webClient = null;
	MediaElement mediaElement = null;

	public SilverASAP()
	{
		this.Startup += this.Application_Startup;
	}

	[ScriptableMember]
	public string DefaultPlaybackTime
	{
		set
		{
			this.defaultPlaybackTime = string.IsNullOrEmpty(value) ? -1 : ASAPInfo.ParseDuration(value);
		}
	}

	[ScriptableMember]
	public string LoopPlaybackTime
	{
		set
		{
			this.loopPlaybackTime =
				string.IsNullOrEmpty(value) ? -1 :
				value == "ONCE" ? Once :
				ASAPInfo.ParseDuration(value);
		}
	}

	void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
	{
		this.webClient = null;
		if (e.Cancelled || e.Error != null)
			return;
		byte[] module = new byte[e.Result.Length];
		int moduleLen = e.Result.Read(module, 0, module.Length);

		ASAP asap = new ASAP();
		asap.Load(this.filename, module, moduleLen);
		ASAPInfo moduleInfo = asap.ModuleInfo;
		if (this.song < 0)
			this.song = moduleInfo.DefaultSong;
		int duration = moduleInfo.Durations[this.song];
		if (duration < 0)
			duration = this.defaultPlaybackTime;
		else if (moduleInfo.Loops[this.song] && this.loopPlaybackTime != Once)
			duration = this.loopPlaybackTime;
		asap.PlaySong(this.song, duration);

		Stop();
		this.mediaElement = new MediaElement();
		this.mediaElement.Volume = 1;
		this.mediaElement.AutoPlay = true;
		this.mediaElement.SetSource(new ASAPMediaStreamSource(asap, duration));
	}

	[ScriptableMember]
	public void Play(string filename, int song)
	{
		this.filename = filename;
		this.song = song;
		this.webClient = new WebClient();
		this.webClient.OpenReadCompleted += WebClient_OpenReadCompleted;
		this.webClient.OpenReadAsync(new Uri(filename, UriKind.Relative));
	}

	[ScriptableMember]
	public void Play(string filename)
	{
		Play(filename, -1);
	}

	[ScriptableMember]
	public void Pause()
	{
		if (this.mediaElement != null) {
			if (this.mediaElement.CurrentState == MediaElementState.Playing)
				this.mediaElement.Pause();
			else
				this.mediaElement.Play();
		}
	}

	[ScriptableMember]
	public void Stop()
	{
		if (this.webClient != null)
			this.webClient.CancelAsync();
		if (this.mediaElement != null) {
			this.mediaElement.Stop();
			// in Opera Stop() doesn't work when mediaElement is in the Opening state:
			this.mediaElement.Source = null;
			this.mediaElement = null;
		}
	}

	void Application_Startup(object sender, StartupEventArgs e)
	{
		HtmlPage.RegisterScriptableObject("ASAP", this);
		string s;
		if (e.InitParams.TryGetValue("defaultPlaybackTime", out s))
			this.DefaultPlaybackTime = s;
		if (e.InitParams.TryGetValue("loopPlaybackTime", out s))
			this.LoopPlaybackTime = s;
		string filename;
		if (e.InitParams.TryGetValue("file", out filename)) {
			int song = -1;
			if (e.InitParams.TryGetValue("song", out s))
				song = int.Parse(s, CultureInfo.InvariantCulture);
			Play(filename, song);
		}
	}
}
