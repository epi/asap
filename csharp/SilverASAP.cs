using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using ASAP;

class ASAPMediaStreamSource : MediaStreamSource
{
	const int BitsPerSample = 16;
	static readonly Dictionary<MediaSampleAttributeKeys, string> SampleAttributes = new Dictionary<MediaSampleAttributeKeys, string>();

	readonly ASAP_Player asap;
	MediaStreamDescription mediaStreamDescription;

	public ASAPMediaStreamSource(ASAP_Player asap)
	{
		this.asap = asap;
	}

	static string LittleEndianHex(int x)
	{
		return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", x & 0xff, (x >> 8) & 0xff, (x >> 16) & 0xff, (x >> 24) & 0xff);
	}

	protected override void OpenMediaAsync()
	{
		int channels = asap.GetModuleInfo().channels;
		int blockSize = channels * BitsPerSample >> 3;
		string waveFormatHex = string.Format("0100{0:X2}00{1}{2}{3:X2}00{4:X2}000000",
			channels,
			LittleEndianHex(ASAP_Player.SampleRate),
			LittleEndianHex(ASAP_Player.SampleRate * blockSize),
			blockSize,
			BitsPerSample
		);
		Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
		streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = waveFormatHex;
		this.mediaStreamDescription = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);

		Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
		sourceAttributes[MediaSourceAttributesKeys.CanSeek] = "False";
		sourceAttributes[MediaSourceAttributesKeys.Duration] = "0";

		ReportOpenMediaCompleted(sourceAttributes, new MediaStreamDescription[1] { this.mediaStreamDescription });
	}

	protected override void GetSampleAsync(MediaStreamType mediaStreamType)
	{
		int blocksPlayed = asap.GetBlocksPlayed();
		byte[] buffer = new byte[8192];
		int buffer_len = asap.Generate(buffer, BitsPerSample == 8 ? ASAP_SampleFormat.U8 : ASAP_SampleFormat.S16LE);
		Stream s = new MemoryStream(buffer);
		MediaStreamSample mss = new MediaStreamSample(
			this.mediaStreamDescription,
			s,
			0,
			buffer_len,
			blocksPlayed * 10000000 / ASAP_Player.SampleRate,
			SampleAttributes);
		ReportGetSampleCompleted(mss);
	}

	protected override void SeekAsync(long seekToTime)
	{
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
	string filename;

	public SilverASAP()
	{
		this.Startup += this.Application_Startup;
	}

	void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
	{
		byte[] module = new byte[e.Result.Length];
		int module_len = e.Result.Read(module, 0, module.Length);

		ASAP_Player asap = new ASAP_Player();
		asap.Load(this.filename, module, module_len);
		asap.PlaySong(asap.GetModuleInfo().default_song, -1);

		MediaElement me = new MediaElement();
		me.Volume = 1;
		me.AutoPlay = true;
		me.SetSource(new ASAPMediaStreamSource(asap));
	}

	void Application_Startup(object sender, StartupEventArgs e)
	{
		this.filename = "X_Ray_2.sap";
		WebClient wc = new WebClient();
		wc.OpenReadCompleted += WebClient_OpenReadCompleted;
		wc.OpenReadAsync(new Uri(this.filename, UriKind.Relative));
	}
}
