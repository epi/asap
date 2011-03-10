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

	readonly ASAP Asap;
	readonly int Duration;
	MediaStreamDescription MediaStreamDescription;

	public ASAPMediaStreamSource(ASAP asap, int duration)
	{
		Asap = asap;
		Duration = duration;
	}

	static string LittleEndianHex(int x)
	{
		return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", x & 0xff, (x >> 8) & 0xff, (x >> 16) & 0xff, (x >> 24) & 0xff);
	}

	protected override void OpenMediaAsync()
	{
		int channels = Asap.GetInfo().GetChannels();
		int blockSize = channels * BitsPerSample >> 3;
		string waveFormatHex = string.Format("0100{0:X2}00{1}{2}{3:X2}00{4:X2}000000",
			channels, LittleEndianHex(ASAP.SampleRate), LittleEndianHex(ASAP.SampleRate * blockSize),
			blockSize, BitsPerSample);
		Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
		streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = waveFormatHex;
		MediaStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);

		Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
		sourceAttributes[MediaSourceAttributesKeys.CanSeek] = "True";
		sourceAttributes[MediaSourceAttributesKeys.Duration] = (Duration < 0 ? 0 : Duration * 10000).ToString();

		ReportOpenMediaCompleted(sourceAttributes, new MediaStreamDescription[1] { MediaStreamDescription });
	}

	protected override void GetSampleAsync(MediaStreamType mediaStreamType)
	{
		byte[] buffer = new byte[8192];
		int blocksPlayed;
		int bufferLen;
		lock (Asap) {
			blocksPlayed = Asap.GetBlocksPlayed();
			bufferLen = Asap.Generate(buffer, buffer.Length, BitsPerSample == 8 ? ASAPSampleFormat.U8 : ASAPSampleFormat.S16LE);
		}
		Stream s = new MemoryStream(buffer);
		MediaStreamSample mss = new MediaStreamSample(MediaStreamDescription, s, 0, bufferLen,
			blocksPlayed * 10000000 / ASAP.SampleRate, SampleAttributes);
		ReportGetSampleCompleted(mss);
	}

	protected override void SeekAsync(long seekToTime)
	{
		lock (Asap)
			Asap.Seek((int) (seekToTime / 10000));
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
	string Filename;
	int Song = -1;
	WebClient WebClient = null;
	MediaElement MediaElement = null;

	public SilverASAP()
	{
		this.Startup += this.Application_Startup;
	}

	void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
	{
		WebClient = null;
		if (e.Cancelled || e.Error != null)
			return;
		byte[] module = new byte[e.Result.Length];
		int moduleLen = e.Result.Read(module, 0, module.Length);

		ASAP asap = new ASAP();
		asap.Load(Filename, module, moduleLen);
		ASAPInfo info = asap.GetInfo();
		if (Song < 0)
			Song = info.GetDefaultSong();
		int duration = info.GetLoop(Song) ? -1 : info.GetDuration(Song);
		asap.PlaySong(Song, duration);

		Stop();
		MediaElement = new MediaElement();
		MediaElement.Volume = 1;
		MediaElement.AutoPlay = true;
		MediaElement.SetSource(new ASAPMediaStreamSource(asap, duration));
	}

	[ScriptableMember]
	public void Play(string filename, int song)
	{
		Filename = filename;
		Song = song;
		WebClient = new WebClient();
		WebClient.OpenReadCompleted += WebClient_OpenReadCompleted;
		WebClient.OpenReadAsync(new Uri(filename, UriKind.Relative));
	}

	[ScriptableMember]
	public void Play(string filename)
	{
		Play(filename, -1);
	}

	[ScriptableMember]
	public void Pause()
	{
		if (MediaElement != null) {
			if (MediaElement.CurrentState == MediaElementState.Playing)
				MediaElement.Pause();
			else
				MediaElement.Play();
		}
	}

	[ScriptableMember]
	public void Stop()
	{
		if (WebClient != null)
			WebClient.CancelAsync();
		if (MediaElement != null) {
			MediaElement.Stop();
			// in Opera Stop() doesn't work when mediaElement is in the Opening state:
			MediaElement.Source = null;
			MediaElement = null;
		}
	}

	void Application_Startup(object sender, StartupEventArgs e)
	{
		HtmlPage.RegisterScriptableObject("ASAP", this);
		string filename;
		if (e.InitParams.TryGetValue("file", out filename)) {
			string s;
			int song = -1;
			if (e.InitParams.TryGetValue("song", out s))
				song = int.Parse(s, CultureInfo.InvariantCulture);
			Play(filename, song);
		}
	}
}
