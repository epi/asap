package
{
	public class ASAP_ModuleInfo
	{
		public var author : String;
		public var name : String;
		public var date : String;
		public var channels : int;
		public var songs : int;
		public var default_song : int;
		public const durations = new Array(32);
		public const loops = new Array(32);
		internal var type : int;
		internal var fastplay : int;
		internal var music : int;
		internal var init : int;
		internal var player : int;
		internal var covox_addr : int;
		internal var header_len : int;
		internal const song_pos = new Array(128);
	}
}
