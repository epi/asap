package
{
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.events.SampleDataEvent;
	import flash.external.ExternalInterface;
	import flash.media.Sound;
	import flash.media.SoundChannel;
	import flash.net.URLLoader;
	import flash.net.URLLoaderDataFormat;
	import flash.net.URLRequest;
	import flash.utils.ByteArray;
	import mx.core.Application;

	public class ASAPPlayer extends Sprite
	{
		private static const ONCE : int = -2;
		private var defaultPlaybackTime = -1;
		private var loopPlaybackTime = -1;

		private var filename : String;
		private var song : int;

		private var soundChannel : SoundChannel = null;

		public function setPlaybackTime(defaultPlaybackTime : String, loopPlaybackTime : String)
		{
			this.defaultPlaybackTime = ASAP.parseDuration(defaultPlaybackTime);
			if (loopPlaybackTime == "ONCE")
				this.loopPlaybackTime = ONCE;
			else
				this.loopPlaybackTime = ASAP.parseDuration(loopPlaybackTime);
		}

		private function completeHandler(event : Event) : void
		{
			var module : ByteArray = URLLoader(event.target).data;

			var asap : ASAP = new ASAP();
			asap.load(filename, module);
			var song = this.song;
			if (song < 0)
				song = asap.moduleInfo.default_song;
			var duration : int = asap.moduleInfo.durations[song];
			if (duration < 0)
				duration = this.defaultPlaybackTime;
			else if (asap.moduleInfo.loops[song] && this.loopPlaybackTime != ONCE)
				duration = this.loopPlaybackTime;
			asap.playSong(song, duration);

			var sound : Sound = new Sound();
			function generator(event : SampleDataEvent) : void
			{
				asap.generate(event.data, 8192);
			}
			sound.addEventListener(SampleDataEvent.SAMPLE_DATA, generator);
			this.soundChannel = sound.play();
		}

		public function play(filename : String, song : int = -1) : void
		{
			this.filename = filename;
			this.song = song;
			var loader : URLLoader = new URLLoader();
			loader.dataFormat = URLLoaderDataFormat.BINARY;
			loader.addEventListener(Event.COMPLETE, completeHandler);
			loader.load(new URLRequest(filename));
		}

		public function stop() : void
		{
			if (this.soundChannel != null)
			{
				this.soundChannel.stop();
				this.soundChannel = null;
			}
		}

		public function ASAPPlayer()
		{
			ExternalInterface.addCallback("setPlaybackTime", setPlaybackTime);
			ExternalInterface.addCallback("asapPlay", play);
			ExternalInterface.addCallback("asapStop", stop);
			//Application.application.parameters.file;
		}
	}
}
