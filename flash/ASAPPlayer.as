package
{
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.events.SampleDataEvent;
	import flash.media.Sound;
	import flash.net.URLLoader;
	import flash.net.URLLoaderDataFormat;
	import flash.net.URLRequest;
	import flash.utils.ByteArray;

	public class ASAPPlayer extends Sprite
	{
		private var filename : String = "Killa_Cycle.sap";

		private function completeHandler(event : Event) : void
		{
			var module : ByteArray = URLLoader(event.target).data;
			var asap : ASAP = new ASAP();

			asap.load(filename, module);
			var song : int = asap.moduleInfo.default_song;
			var duration : int = asap.moduleInfo.durations[song];
			asap.playSong(song, duration);

			var buffer : ByteArray = new ByteArray();
			var sound : Sound = new Sound();
			function generator(event : SampleDataEvent) : void
			{
				var len : int = asap.generate(buffer, 8192, ASAP.ASAP_SampleFormat.S16LE);
				for (var i : int = 0; i < len; i += 2) {
					var sample : Number = (buffer[i] + (buffer[i + 1] << 8)) / 32768;
					event.data.writeFloat(sample);
					event.data.writeFloat(sample);
				}
			}
			sound.addEventListener(SampleDataEvent.SAMPLE_DATA, generator);
			sound.play();
		}

		public function ASAPPlayer()
		{
			var loader : URLLoader = new URLLoader();
			loader.dataFormat = URLLoaderDataFormat.BINARY;
			loader.addEventListener(Event.COMPLETE, completeHandler);
			loader.load(new URLRequest(filename));
		}
	}
}
