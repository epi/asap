// Generated automatically with "cito". Do not edit.
namespace Sf.Asap {

	/// <summary>Format of output samples.</summary>
	public enum ASAP_SampleFormat {
		/// <summary>Unsigned 8-bit.</summary>
		U8,
		/// <summary>Signed 16-bit little-endian.</summary>
		S16LE,
		/// <summary>Signed 16-bit big-endian.</summary>
		S16BE
	}

	/// <summary>Information about a music file.</summary>
	public class ASAP_ModuleInfo {
		/// <summary>Author's name.</summary>
		/// <remarks>A nickname may be included in parentheses after the real name.
		/// Multiple authors are separated with <c>" &amp; "</c>.
		/// Empty string means the author is unknown.</remarks>
		public string author;
		/// <summary>Music title.</summary>
		/// <remarks>Empty string means the title is unknown.</remarks>
		public string name;
		/// <summary>Music creation date.</summary>
		/// <remarks>Some of the possible formats are:
		/// <list type="bullet">
		/// <item>YYYY</item>
		/// <item>MM/YYYY</item>
		/// <item>DD/MM/YYYY</item>
		/// <item>YYYY-YYYY</item>
		/// </list>
		/// Empty string means the date is unknown.</remarks>
		public string date;
		/// <summary>1 for mono or 2 for stereo.</summary>
		public int channels;
		/// <summary>Number of songs in the file.</summary>
		public int songs;
		/// <summary>0-based index of the "main" song.</summary>
		/// <remarks>The specified song should be played by default.</remarks>
		public int default_song;
		/// <summary>Lengths of songs.</summary>
		/// <remarks>Each element of the array represents length of one song,
		/// in milliseconds. -1 means the length is indeterminate.</remarks>
		public readonly int[] durations = new int[32];
		/// <summary>Information about finite vs infinite songs.</summary>
		/// <remarks>Each element of the array represents one song, and is:
		/// <list type="bullet">
		/// <item><see langword="true" /> if the song loops</item>
		/// <item><see langword="false" /> if the song stops</item>
		/// </list>
		/// </remarks>
		public readonly bool[] loops = new bool[32];
		internal bool ntsc;
		internal int type;
		internal int fastplay;
		internal int music;
		internal int init;
		internal int player;
		internal int covox_addr;
		internal int header_len;
		internal readonly byte[] song_pos = new byte[32];
	}

	internal class PokeyState {
		internal int audctl;
		internal bool init;
		internal int poly_index;
		internal int div_cycles;
		internal int mute1;
		internal int mute2;
		internal int mute3;
		internal int mute4;
		internal int audf1;
		internal int audf2;
		internal int audf3;
		internal int audf4;
		internal int audc1;
		internal int audc2;
		internal int audc3;
		internal int audc4;
		internal int tick_cycle1;
		internal int tick_cycle2;
		internal int tick_cycle3;
		internal int tick_cycle4;
		internal int period_cycles1;
		internal int period_cycles2;
		internal int period_cycles3;
		internal int period_cycles4;
		internal int reload_cycles1;
		internal int reload_cycles3;
		internal int out1;
		internal int out2;
		internal int out3;
		internal int out4;
		internal int delta1;
		internal int delta2;
		internal int delta3;
		internal int delta4;
		internal int skctl;
		internal readonly int[] delta_buffer = new int[888];
	}

	internal class ASAP_State {
		internal int cycle;
		internal int cpu_pc;
		internal int cpu_a;
		internal int cpu_x;
		internal int cpu_y;
		internal int cpu_s;
		internal int cpu_nz;
		internal int cpu_c;
		internal int cpu_vdi;
		internal int scanline_number;
		internal int nearest_event_cycle;
		internal int next_scanline_cycle;
		internal int timer1_cycle;
		internal int timer2_cycle;
		internal int timer4_cycle;
		internal int irqst;
		internal int extra_pokey_mask;
		internal int consol;
		internal int nmist;
		internal readonly byte[] covox = new byte[4];
		internal readonly PokeyState base_pokey = new PokeyState();
		internal readonly PokeyState extra_pokey = new PokeyState();
		internal int sample_offset;
		internal int sample_index;
		internal int samples;
		internal int iir_acc_left;
		internal int iir_acc_right;
		public readonly ASAP_ModuleInfo module_info = new ASAP_ModuleInfo();
		internal int tmc_per_frame;
		internal int tmc_per_frame_counter;
		internal int current_song;
		internal int current_duration;
		internal int blocks_played;
		internal int silence_cycles;
		internal int silence_cycles_counter;
		internal readonly byte[] poly9_lookup = new byte[511];
		internal readonly byte[] poly17_lookup = new byte[16385];
		internal readonly byte[] memory = new byte[65536];
	}
	public partial class ASAP {
		static readonly byte[] CiConstArray_1 = { 0, 0, 0, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1 };
		static readonly byte[] CiConstArray_2 = { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1 };
		static readonly int[] CiConstArray_3 = { 7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6, 2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, 6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6, 2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, 6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6, 2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, 6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6, 2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, 2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4, 2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5, 2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4, 2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4, 2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6, 2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7, 2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6, 2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7 };
		static readonly byte[] CiConstArray_4 = { 92, 86, 80, 77, 71, 68, 65, 62, 56, 53, 136, 127, 121, 115, 108, 103, 96, 90, 85, 81, 76, 72, 67, 63, 61, 57, 52, 51, 48, 45, 42, 40, 37, 36, 33, 31, 30 };
		static readonly byte[] CiConstArray_5 = { 16, 8, 4, 3, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 };
		public const int VersionMajor = 2;
		public const int VersionMinor = 1;
		public const int VersionMicro = 3;
		public const string Version = "2.1.3";
		/// <summary>Maximum length of a supported input file.</summary>
		/// <remarks>You may assume that files longer than this are not supported by ASAP.</remarks>
		public const int ModuleMax = 65000;
		/// <summary>Maximum number of songs in a file.</summary>
		public const int SongsMax = 32;
		/// <summary>Output sample rate.</summary>
		public const int SampleRate = 44100;
		/// <summary>WAV file header length.</summary>
		public const int WavHeaderBytes = 44;

		static void PokeySound_InitializeChip(PokeyState pst) {
			pst.audctl = 0;
			pst.init = false;
			pst.poly_index = 60948015;
			pst.div_cycles = 28;
			pst.mute1 = 5;
			pst.mute2 = 5;
			pst.mute3 = 5;
			pst.mute4 = 5;
			pst.audf1 = 0;
			pst.audf2 = 0;
			pst.audf3 = 0;
			pst.audf4 = 0;
			pst.audc1 = 0;
			pst.audc2 = 0;
			pst.audc3 = 0;
			pst.audc4 = 0;
			pst.tick_cycle1 = 8388608;
			pst.tick_cycle2 = 8388608;
			pst.tick_cycle3 = 8388608;
			pst.tick_cycle4 = 8388608;
			pst.period_cycles1 = 28;
			pst.period_cycles2 = 28;
			pst.period_cycles3 = 28;
			pst.period_cycles4 = 28;
			pst.reload_cycles1 = 28;
			pst.reload_cycles3 = 28;
			pst.out1 = 0;
			pst.out2 = 0;
			pst.out3 = 0;
			pst.out4 = 0;
			pst.delta1 = 0;
			pst.delta2 = 0;
			pst.delta3 = 0;
			pst.delta4 = 0;
			pst.skctl = 3;
			Array_Clear(pst.delta_buffer);
		}

		static void PokeySound_Initialize(ASAP_State ast) {
			int reg = 511;
			for (int i = 0; i < 511; i++) {
				reg = (((reg >> 5 ^ reg) & 1) << 8) + (reg >> 1);
				ast.poly9_lookup[i] = (byte) reg;
			}
			reg = 131071;
			for (int i = 0; i < 16385; i++) {
				reg = (((reg >> 5 ^ reg) & 255) << 9) + (reg >> 8);
				ast.poly17_lookup[i] = (byte) (reg >> 1);
			}
			ast.sample_offset = 0;
			ast.sample_index = 0;
			ast.samples = 0;
			ast.iir_acc_left = 0;
			ast.iir_acc_right = 0;
			PokeySound_InitializeChip(ast.base_pokey);
			PokeySound_InitializeChip(ast.extra_pokey);
		}

		/// <summary>Fills <c>delta_buffer</c> up to <c>current_cycle</c> basing on current AUDF/AUDC/AUDCTL values.</summary>
		static void PokeySound_GenerateUntilCycle(ASAP_State ast, PokeyState pst, int current_cycle) {
			for (;;) {
				int cycle = current_cycle;
				if (cycle > pst.tick_cycle1)
					cycle = pst.tick_cycle1;
				if (cycle > pst.tick_cycle2)
					cycle = pst.tick_cycle2;
				if (cycle > pst.tick_cycle3)
					cycle = pst.tick_cycle3;
				if (cycle > pst.tick_cycle4)
					cycle = pst.tick_cycle4;
				if (cycle == current_cycle)
					break;
				if (cycle == pst.tick_cycle3) {
					pst.tick_cycle3 += pst.period_cycles3;
					if ((pst.audctl & 4) != 0 && pst.delta1 > 0 && pst.mute1 == 0)
						pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta1 = -pst.delta1;
					if (pst.init) {
						switch (pst.audc3 >> 4) {
							case 10:
							case 14:
								pst.out3 ^= 1;
								pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta3 = -pst.delta3;
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + pst.poly_index - 2;
						int newout = pst.out3;
						switch (pst.audc3 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((pst.audctl & 128) != 0)
										newout = ast.poly9_lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newout ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newout = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((pst.audctl & 128) != 0)
									newout = ast.poly9_lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newout ^= 1;
								break;
							case 12:
								newout = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newout != pst.out3) {
							pst.out3 = newout;
							pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta3 = -pst.delta3;
						}
					}
				}
				if (cycle == pst.tick_cycle4) {
					pst.tick_cycle4 += pst.period_cycles4;
					if ((pst.audctl & 8) != 0)
						pst.tick_cycle3 = cycle + pst.reload_cycles3;
					if ((pst.audctl & 2) != 0 && pst.delta2 > 0 && pst.mute2 == 0)
						pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta2 = -pst.delta2;
					if (pst.init) {
						switch (pst.audc4 >> 4) {
							case 10:
							case 14:
								pst.out4 ^= 1;
								pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta4 = -pst.delta4;
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + pst.poly_index - 3;
						int newout = pst.out4;
						switch (pst.audc4 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((pst.audctl & 128) != 0)
										newout = ast.poly9_lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newout ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newout = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((pst.audctl & 128) != 0)
									newout = ast.poly9_lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newout ^= 1;
								break;
							case 12:
								newout = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newout != pst.out4) {
							pst.out4 = newout;
							pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta4 = -pst.delta4;
						}
					}
				}
				if (cycle == pst.tick_cycle1) {
					pst.tick_cycle1 += pst.period_cycles1;
					if ((pst.skctl & 136) == 8)
						pst.tick_cycle2 = cycle + pst.period_cycles2;
					if (pst.init) {
						switch (pst.audc1 >> 4) {
							case 10:
							case 14:
								pst.out1 ^= 1;
								pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta1 = -pst.delta1;
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + pst.poly_index - 0;
						int newout = pst.out1;
						switch (pst.audc1 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((pst.audctl & 128) != 0)
										newout = ast.poly9_lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newout ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newout = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((pst.audctl & 128) != 0)
									newout = ast.poly9_lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newout ^= 1;
								break;
							case 12:
								newout = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newout != pst.out1) {
							pst.out1 = newout;
							pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta1 = -pst.delta1;
						}
					}
				}
				if (cycle == pst.tick_cycle2) {
					pst.tick_cycle2 += pst.period_cycles2;
					if ((pst.audctl & 16) != 0)
						pst.tick_cycle1 = cycle + pst.reload_cycles1;
					else
						if ((pst.skctl & 8) != 0)
							pst.tick_cycle1 = cycle + pst.period_cycles1;
					if (pst.init) {
						switch (pst.audc2 >> 4) {
							case 10:
							case 14:
								pst.out2 ^= 1;
								pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta2 = -pst.delta2;
								break;
							default:
								break;
						}
					}
					else {
						int poly = cycle + pst.poly_index - 1;
						int newout = pst.out2;
						switch (pst.audc2 >> 4) {
							case 0:
								if (CiConstArray_2[poly % 31] != 0) {
									if ((pst.audctl & 128) != 0)
										newout = ast.poly9_lookup[poly % 511] & 1;
									else {
										poly %= 131071;
										newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
									}
								}
								break;
							case 2:
							case 6:
								newout ^= CiConstArray_2[poly % 31];
								break;
							case 4:
								if (CiConstArray_2[poly % 31] != 0)
									newout = CiConstArray_1[poly % 15];
								break;
							case 8:
								if ((pst.audctl & 128) != 0)
									newout = ast.poly9_lookup[poly % 511] & 1;
								else {
									poly %= 131071;
									newout = ast.poly17_lookup[poly >> 3] >> (poly & 7) & 1;
								}
								break;
							case 10:
							case 14:
								newout ^= 1;
								break;
							case 12:
								newout = CiConstArray_1[poly % 15];
								break;
							default:
								break;
						}
						if (newout != pst.out2) {
							pst.out2 = newout;
							pst.delta_buffer[(cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta2 = -pst.delta2;
						}
					}
				}
			}
		}

		static void PokeySound_PutByte(ASAP_State ast, int addr, int data) {
			PokeyState pst = (addr & ast.extra_pokey_mask) != 0 ? ast.extra_pokey : ast.base_pokey;
			switch (addr & 15) {
				case 0:
					if (data == pst.audf1)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audf1 = data;
					switch (pst.audctl & 80) {
						case 0:
							pst.period_cycles1 = pst.div_cycles * (data + 1);
							break;
						case 16:
							pst.period_cycles2 = pst.div_cycles * (data + 256 * pst.audf2 + 1);
							pst.reload_cycles1 = pst.div_cycles * (data + 1);
							if (pst.period_cycles2 <= 112 && (pst.audc2 >> 4 == 10 || pst.audc2 >> 4 == 14)) {
								pst.mute2 |= 1;
								pst.tick_cycle2 = 8388608;
							}
							else {
								pst.mute2 &= ~1;
								if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
									pst.tick_cycle2 = ast.cycle;
							}
							break;
						case 64:
							pst.period_cycles1 = data + 4;
							break;
						case 80:
							pst.period_cycles2 = data + 256 * pst.audf2 + 7;
							pst.reload_cycles1 = data + 4;
							if (pst.period_cycles2 <= 112 && (pst.audc2 >> 4 == 10 || pst.audc2 >> 4 == 14)) {
								pst.mute2 |= 1;
								pst.tick_cycle2 = 8388608;
							}
							else {
								pst.mute2 &= ~1;
								if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
									pst.tick_cycle2 = ast.cycle;
							}
							break;
					}
					if (pst.period_cycles1 <= 112 && (pst.audc1 >> 4 == 10 || pst.audc1 >> 4 == 14)) {
						pst.mute1 |= 1;
						pst.tick_cycle1 = 8388608;
					}
					else {
						pst.mute1 &= ~1;
						if (pst.tick_cycle1 == 8388608 && pst.mute1 == 0)
							pst.tick_cycle1 = ast.cycle;
					}
					break;
				case 1:
					if (data == pst.audc1)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audc1 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pst.mute1 & 4) == 0)
							pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta1 > 0 ? data - pst.delta1 : data;
						pst.delta1 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pst.period_cycles1 <= 112 && (pst.audc1 >> 4 == 10 || pst.audc1 >> 4 == 14)) {
							pst.mute1 |= 1;
							pst.tick_cycle1 = 8388608;
						}
						else {
							pst.mute1 &= ~1;
							if (pst.tick_cycle1 == 8388608 && pst.mute1 == 0)
								pst.tick_cycle1 = ast.cycle;
						}
						if (pst.delta1 > 0) {
							if ((pst.mute1 & 4) == 0)
								pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += data - pst.delta1;
							pst.delta1 = data;
						}
						else
							pst.delta1 = -data;
					}
					break;
				case 2:
					if (data == pst.audf2)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audf2 = data;
					switch (pst.audctl & 80) {
						case 0:
						case 64:
							pst.period_cycles2 = pst.div_cycles * (data + 1);
							break;
						case 16:
							pst.period_cycles2 = pst.div_cycles * (pst.audf1 + 256 * data + 1);
							break;
						case 80:
							pst.period_cycles2 = pst.audf1 + 256 * data + 7;
							break;
					}
					if (pst.period_cycles2 <= 112 && (pst.audc2 >> 4 == 10 || pst.audc2 >> 4 == 14)) {
						pst.mute2 |= 1;
						pst.tick_cycle2 = 8388608;
					}
					else {
						pst.mute2 &= ~1;
						if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
							pst.tick_cycle2 = ast.cycle;
					}
					break;
				case 3:
					if (data == pst.audc2)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audc2 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pst.mute2 & 4) == 0)
							pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta2 > 0 ? data - pst.delta2 : data;
						pst.delta2 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pst.period_cycles2 <= 112 && (pst.audc2 >> 4 == 10 || pst.audc2 >> 4 == 14)) {
							pst.mute2 |= 1;
							pst.tick_cycle2 = 8388608;
						}
						else {
							pst.mute2 &= ~1;
							if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
								pst.tick_cycle2 = ast.cycle;
						}
						if (pst.delta2 > 0) {
							if ((pst.mute2 & 4) == 0)
								pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += data - pst.delta2;
							pst.delta2 = data;
						}
						else
							pst.delta2 = -data;
					}
					break;
				case 4:
					if (data == pst.audf3)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audf3 = data;
					switch (pst.audctl & 40) {
						case 0:
							pst.period_cycles3 = pst.div_cycles * (data + 1);
							break;
						case 8:
							pst.period_cycles4 = pst.div_cycles * (data + 256 * pst.audf4 + 1);
							pst.reload_cycles3 = pst.div_cycles * (data + 1);
							if (pst.period_cycles4 <= 112 && (pst.audc4 >> 4 == 10 || pst.audc4 >> 4 == 14)) {
								pst.mute4 |= 1;
								pst.tick_cycle4 = 8388608;
							}
							else {
								pst.mute4 &= ~1;
								if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
									pst.tick_cycle4 = ast.cycle;
							}
							break;
						case 32:
							pst.period_cycles3 = data + 4;
							break;
						case 40:
							pst.period_cycles4 = data + 256 * pst.audf4 + 7;
							pst.reload_cycles3 = data + 4;
							if (pst.period_cycles4 <= 112 && (pst.audc4 >> 4 == 10 || pst.audc4 >> 4 == 14)) {
								pst.mute4 |= 1;
								pst.tick_cycle4 = 8388608;
							}
							else {
								pst.mute4 &= ~1;
								if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
									pst.tick_cycle4 = ast.cycle;
							}
							break;
					}
					if (pst.period_cycles3 <= 112 && (pst.audc3 >> 4 == 10 || pst.audc3 >> 4 == 14)) {
						pst.mute3 |= 1;
						pst.tick_cycle3 = 8388608;
					}
					else {
						pst.mute3 &= ~1;
						if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
							pst.tick_cycle3 = ast.cycle;
					}
					break;
				case 5:
					if (data == pst.audc3)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audc3 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pst.mute3 & 4) == 0)
							pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta3 > 0 ? data - pst.delta3 : data;
						pst.delta3 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pst.period_cycles3 <= 112 && (pst.audc3 >> 4 == 10 || pst.audc3 >> 4 == 14)) {
							pst.mute3 |= 1;
							pst.tick_cycle3 = 8388608;
						}
						else {
							pst.mute3 &= ~1;
							if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
								pst.tick_cycle3 = ast.cycle;
						}
						if (pst.delta3 > 0) {
							if ((pst.mute3 & 4) == 0)
								pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += data - pst.delta3;
							pst.delta3 = data;
						}
						else
							pst.delta3 = -data;
					}
					break;
				case 6:
					if (data == pst.audf4)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audf4 = data;
					switch (pst.audctl & 40) {
						case 0:
						case 32:
							pst.period_cycles4 = pst.div_cycles * (data + 1);
							break;
						case 8:
							pst.period_cycles4 = pst.div_cycles * (pst.audf3 + 256 * data + 1);
							break;
						case 40:
							pst.period_cycles4 = pst.audf3 + 256 * data + 7;
							break;
					}
					if (pst.period_cycles4 <= 112 && (pst.audc4 >> 4 == 10 || pst.audc4 >> 4 == 14)) {
						pst.mute4 |= 1;
						pst.tick_cycle4 = 8388608;
					}
					else {
						pst.mute4 &= ~1;
						if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
							pst.tick_cycle4 = ast.cycle;
					}
					break;
				case 7:
					if (data == pst.audc4)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audc4 = data;
					if ((data & 16) != 0) {
						data = (data & 15) << 20;
						if ((pst.mute4 & 4) == 0)
							pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += pst.delta4 > 0 ? data - pst.delta4 : data;
						pst.delta4 = data;
					}
					else {
						data = (data & 15) << 20;
						if (pst.period_cycles4 <= 112 && (pst.audc4 >> 4 == 10 || pst.audc4 >> 4 == 14)) {
							pst.mute4 |= 1;
							pst.tick_cycle4 = 8388608;
						}
						else {
							pst.mute4 &= ~1;
							if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
								pst.tick_cycle4 = ast.cycle;
						}
						if (pst.delta4 > 0) {
							if ((pst.mute4 & 4) == 0)
								pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += data - pst.delta4;
							pst.delta4 = data;
						}
						else
							pst.delta4 = -data;
					}
					break;
				case 8:
					if (data == pst.audctl)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.audctl = data;
					pst.div_cycles = (data & 1) != 0 ? 114 : 28;
					switch (data & 80) {
						case 0:
							pst.period_cycles1 = pst.div_cycles * (pst.audf1 + 1);
							pst.period_cycles2 = pst.div_cycles * (pst.audf2 + 1);
							break;
						case 16:
							pst.period_cycles1 = pst.div_cycles * 256;
							pst.period_cycles2 = pst.div_cycles * (pst.audf1 + 256 * pst.audf2 + 1);
							pst.reload_cycles1 = pst.div_cycles * (pst.audf1 + 1);
							break;
						case 64:
							pst.period_cycles1 = pst.audf1 + 4;
							pst.period_cycles2 = pst.div_cycles * (pst.audf2 + 1);
							break;
						case 80:
							pst.period_cycles1 = 256;
							pst.period_cycles2 = pst.audf1 + 256 * pst.audf2 + 7;
							pst.reload_cycles1 = pst.audf1 + 4;
							break;
					}
					if (pst.period_cycles1 <= 112 && (pst.audc1 >> 4 == 10 || pst.audc1 >> 4 == 14)) {
						pst.mute1 |= 1;
						pst.tick_cycle1 = 8388608;
					}
					else {
						pst.mute1 &= ~1;
						if (pst.tick_cycle1 == 8388608 && pst.mute1 == 0)
							pst.tick_cycle1 = ast.cycle;
					}
					if (pst.period_cycles2 <= 112 && (pst.audc2 >> 4 == 10 || pst.audc2 >> 4 == 14)) {
						pst.mute2 |= 1;
						pst.tick_cycle2 = 8388608;
					}
					else {
						pst.mute2 &= ~1;
						if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
							pst.tick_cycle2 = ast.cycle;
					}
					switch (data & 40) {
						case 0:
							pst.period_cycles3 = pst.div_cycles * (pst.audf3 + 1);
							pst.period_cycles4 = pst.div_cycles * (pst.audf4 + 1);
							break;
						case 8:
							pst.period_cycles3 = pst.div_cycles * 256;
							pst.period_cycles4 = pst.div_cycles * (pst.audf3 + 256 * pst.audf4 + 1);
							pst.reload_cycles3 = pst.div_cycles * (pst.audf3 + 1);
							break;
						case 32:
							pst.period_cycles3 = pst.audf3 + 4;
							pst.period_cycles4 = pst.div_cycles * (pst.audf4 + 1);
							break;
						case 40:
							pst.period_cycles3 = 256;
							pst.period_cycles4 = pst.audf3 + 256 * pst.audf4 + 7;
							pst.reload_cycles3 = pst.audf3 + 4;
							break;
					}
					if (pst.period_cycles3 <= 112 && (pst.audc3 >> 4 == 10 || pst.audc3 >> 4 == 14)) {
						pst.mute3 |= 1;
						pst.tick_cycle3 = 8388608;
					}
					else {
						pst.mute3 &= ~1;
						if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
							pst.tick_cycle3 = ast.cycle;
					}
					if (pst.period_cycles4 <= 112 && (pst.audc4 >> 4 == 10 || pst.audc4 >> 4 == 14)) {
						pst.mute4 |= 1;
						pst.tick_cycle4 = 8388608;
					}
					else {
						pst.mute4 &= ~1;
						if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
							pst.tick_cycle4 = ast.cycle;
					}
					if (pst.init && (data & 64) == 0) {
						pst.mute1 |= 2;
						pst.tick_cycle1 = 8388608;
					}
					else {
						pst.mute1 &= ~2;
						if (pst.tick_cycle1 == 8388608 && pst.mute1 == 0)
							pst.tick_cycle1 = ast.cycle;
					}
					if (pst.init && (data & 80) != 80) {
						pst.mute2 |= 2;
						pst.tick_cycle2 = 8388608;
					}
					else {
						pst.mute2 &= ~2;
						if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
							pst.tick_cycle2 = ast.cycle;
					}
					if (pst.init && (data & 32) == 0) {
						pst.mute3 |= 2;
						pst.tick_cycle3 = 8388608;
					}
					else {
						pst.mute3 &= ~2;
						if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
							pst.tick_cycle3 = ast.cycle;
					}
					if (pst.init && (data & 40) != 40) {
						pst.mute4 |= 2;
						pst.tick_cycle4 = 8388608;
					}
					else {
						pst.mute4 &= ~2;
						if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
							pst.tick_cycle4 = ast.cycle;
					}
					break;
				case 9:
					if (pst.tick_cycle1 != 8388608)
						pst.tick_cycle1 = ast.cycle + pst.period_cycles1;
					if (pst.tick_cycle2 != 8388608)
						pst.tick_cycle2 = ast.cycle + pst.period_cycles2;
					if (pst.tick_cycle3 != 8388608)
						pst.tick_cycle3 = ast.cycle + pst.period_cycles3;
					if (pst.tick_cycle4 != 8388608)
						pst.tick_cycle4 = ast.cycle + pst.period_cycles4;
					break;
				case 15:
					if (data == pst.skctl)
						break;
					PokeySound_GenerateUntilCycle(ast, pst, ast.cycle);
					pst.skctl = data;
					bool init = (data & 3) == 0;
					if (pst.init && !init)
						pst.poly_index = ((pst.audctl & 128) != 0 ? 237614 : 60948014) - ast.cycle;
					pst.init = init;
					if (pst.init && (pst.audctl & 64) == 0) {
						pst.mute1 |= 2;
						pst.tick_cycle1 = 8388608;
					}
					else {
						pst.mute1 &= ~2;
						if (pst.tick_cycle1 == 8388608 && pst.mute1 == 0)
							pst.tick_cycle1 = ast.cycle;
					}
					if (pst.init && (pst.audctl & 80) != 80) {
						pst.mute2 |= 2;
						pst.tick_cycle2 = 8388608;
					}
					else {
						pst.mute2 &= ~2;
						if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
							pst.tick_cycle2 = ast.cycle;
					}
					if (pst.init && (pst.audctl & 32) == 0) {
						pst.mute3 |= 2;
						pst.tick_cycle3 = 8388608;
					}
					else {
						pst.mute3 &= ~2;
						if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
							pst.tick_cycle3 = ast.cycle;
					}
					if (pst.init && (pst.audctl & 40) != 40) {
						pst.mute4 |= 2;
						pst.tick_cycle4 = 8388608;
					}
					else {
						pst.mute4 &= ~2;
						if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
							pst.tick_cycle4 = ast.cycle;
					}
					if ((data & 16) != 0) {
						pst.mute3 |= 8;
						pst.tick_cycle3 = 8388608;
					}
					else {
						pst.mute3 &= ~8;
						if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
							pst.tick_cycle3 = ast.cycle;
					}
					if ((data & 16) != 0) {
						pst.mute4 |= 8;
						pst.tick_cycle4 = 8388608;
					}
					else {
						pst.mute4 &= ~8;
						if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
							pst.tick_cycle4 = ast.cycle;
					}
					break;
				default:
					break;
			}
		}

		static int PokeySound_GetRandom(ASAP_State ast, int addr, int cycle) {
			PokeyState pst = (addr & ast.extra_pokey_mask) != 0 ? ast.extra_pokey : ast.base_pokey;
			if (pst.init)
				return 255;
			int i = cycle + pst.poly_index;
			if ((pst.audctl & 128) != 0)
				return ast.poly9_lookup[i % 511];
			else {
				i %= 131071;
				int j = i >> 3;
				i &= 7;
				return (ast.poly17_lookup[j] >> i) + (ast.poly17_lookup[j + 1] << 8 - i) & 255;
			}
		}

		static void end_frame(ASAP_State ast, PokeyState pst, int cycle_limit) {
			PokeySound_GenerateUntilCycle(ast, pst, cycle_limit);
			pst.poly_index += cycle_limit;
			int m = (pst.audctl & 128) != 0 ? 237615 : 60948015;
			if (pst.poly_index >= 2 * m)
				pst.poly_index -= m;
			if (pst.tick_cycle1 != 8388608)
				pst.tick_cycle1 -= cycle_limit;
			if (pst.tick_cycle2 != 8388608)
				pst.tick_cycle2 -= cycle_limit;
			if (pst.tick_cycle3 != 8388608)
				pst.tick_cycle3 -= cycle_limit;
			if (pst.tick_cycle4 != 8388608)
				pst.tick_cycle4 -= cycle_limit;
		}

		static void PokeySound_StartFrame(ASAP_State ast) {
			Array_Clear(ast.base_pokey.delta_buffer);
			if (ast.extra_pokey_mask != 0)
				Array_Clear(ast.extra_pokey.delta_buffer);
		}

		static void PokeySound_EndFrame(ASAP_State ast, int current_cycle) {
			end_frame(ast, ast.base_pokey, current_cycle);
			if (ast.extra_pokey_mask != 0)
				end_frame(ast, ast.extra_pokey, current_cycle);
			ast.sample_offset += current_cycle * 44100;
			ast.sample_index = 0;
			int clk = ast.module_info.ntsc ? 1789772 : 1773447;
			ast.samples = ast.sample_offset / clk;
			ast.sample_offset %= clk;
		}

		/// <summary>Fills buffer with samples from <c>delta_buffer</c>.</summary>
		static int PokeySound_Generate(ASAP_State ast, byte[] buffer, int buffer_offset, int blocks, ASAP_SampleFormat format) {
			int i = ast.sample_index;
			int samples = ast.samples;
			int acc_left = ast.iir_acc_left;
			int acc_right = ast.iir_acc_right;
			if (blocks < samples - i)
				samples = i + blocks;
			else
				blocks = samples - i;
			for (; i < samples; i++) {
				acc_left += ast.base_pokey.delta_buffer[i] - (acc_left * 3 >> 10);
				int sample = acc_left >> 10;
				if (sample < -32767)
					sample = -32767;
				else
					if (sample > 32767)
						sample = 32767;
				switch (format) {
					case ASAP_SampleFormat.U8:
						buffer[buffer_offset++] = (byte) ((sample >> 8) + 128);
						break;
					case ASAP_SampleFormat.S16LE:
						buffer[buffer_offset++] = (byte) sample;
						buffer[buffer_offset++] = (byte) (sample >> 8);
						break;
					case ASAP_SampleFormat.S16BE:
						buffer[buffer_offset++] = (byte) (sample >> 8);
						buffer[buffer_offset++] = (byte) sample;
						break;
				}
				if (ast.extra_pokey_mask != 0) {
					acc_right += ast.extra_pokey.delta_buffer[i] - (acc_right * 3 >> 10);
					sample = acc_right >> 10;
					if (sample < -32767)
						sample = -32767;
					else
						if (sample > 32767)
							sample = 32767;
					switch (format) {
						case ASAP_SampleFormat.U8:
							buffer[buffer_offset++] = (byte) ((sample >> 8) + 128);
							break;
						case ASAP_SampleFormat.S16LE:
							buffer[buffer_offset++] = (byte) sample;
							buffer[buffer_offset++] = (byte) (sample >> 8);
							break;
						case ASAP_SampleFormat.S16BE:
							buffer[buffer_offset++] = (byte) (sample >> 8);
							buffer[buffer_offset++] = (byte) sample;
							break;
					}
				}
			}
			if (i == ast.samples) {
				acc_left += ast.base_pokey.delta_buffer[i];
				acc_right += ast.extra_pokey.delta_buffer[i];
			}
			ast.sample_index = i;
			ast.iir_acc_left = acc_left;
			ast.iir_acc_right = acc_right;
			return blocks;
		}

		static bool PokeySound_IsSilent(PokeyState pst) {
			return ((pst.audc1 | pst.audc2 | pst.audc3 | pst.audc4) & 15) == 0;
		}

		static void PokeySound_Mute(ASAP_State ast, PokeyState pst, int mask) {
			if ((mask & 1) != 0) {
				pst.mute1 |= 4;
				pst.tick_cycle1 = 8388608;
			}
			else {
				pst.mute1 &= ~4;
				if (pst.tick_cycle1 == 8388608 && pst.mute1 == 0)
					pst.tick_cycle1 = ast.cycle;
			}
			if ((mask & 2) != 0) {
				pst.mute2 |= 4;
				pst.tick_cycle2 = 8388608;
			}
			else {
				pst.mute2 &= ~4;
				if (pst.tick_cycle2 == 8388608 && pst.mute2 == 0)
					pst.tick_cycle2 = ast.cycle;
			}
			if ((mask & 4) != 0) {
				pst.mute3 |= 4;
				pst.tick_cycle3 = 8388608;
			}
			else {
				pst.mute3 &= ~4;
				if (pst.tick_cycle3 == 8388608 && pst.mute3 == 0)
					pst.tick_cycle3 = ast.cycle;
			}
			if ((mask & 8) != 0) {
				pst.mute4 |= 4;
				pst.tick_cycle4 = 8388608;
			}
			else {
				pst.mute4 &= ~4;
				if (pst.tick_cycle4 == 8388608 && pst.mute4 == 0)
					pst.tick_cycle4 = ast.cycle;
			}
		}

		static int ASAP_GetByte(ASAP_State ast, int addr) {
			switch (addr & 65311) {
				case 53268:
					return ast.module_info.ntsc ? 15 : 1;
				case 53770:
				case 53786:
					return PokeySound_GetRandom(ast, addr, ast.cycle);
				case 53774:
					return ast.irqst;
				case 53790:
					if (ast.extra_pokey_mask != 0) {
						return 255;
					}
					return ast.irqst;
				case 53772:
				case 53788:
				case 53775:
				case 53791:
					return 255;
				case 54283:
				case 54299:
					if (ast.scanline_number == 0 && ast.cycle == 13)
						return ast.module_info.ntsc ? 131 : 156;
					return ast.scanline_number >> 1;
				case 54287:
					if (ast.nmist == 2)
						return 95;
					if (ast.nmist == 0)
						return 31;
					return ast.cycle < 28295 ? 31 : 95;
				default:
					return ast.memory[addr];
			}
		}

		static void ASAP_PutByte(ASAP_State ast, int addr, int data) {
			if (addr >> 8 == 210) {
				if ((addr & ast.extra_pokey_mask + 15) == 14) {
					ast.irqst |= data ^ 255;
					if ((data & ast.irqst & 1) != 0) {
						if (ast.timer1_cycle == 8388608) {
							int t = ast.base_pokey.tick_cycle1;
							while (t < ast.cycle)
								t += ast.base_pokey.period_cycles1;
							ast.timer1_cycle = t;
							if (ast.nearest_event_cycle > t)
								ast.nearest_event_cycle = t;
						}
					}
					else
						ast.timer1_cycle = 8388608;
					if ((data & ast.irqst & 2) != 0) {
						if (ast.timer2_cycle == 8388608) {
							int t = ast.base_pokey.tick_cycle2;
							while (t < ast.cycle)
								t += ast.base_pokey.period_cycles2;
							ast.timer2_cycle = t;
							if (ast.nearest_event_cycle > t)
								ast.nearest_event_cycle = t;
						}
					}
					else
						ast.timer2_cycle = 8388608;
					if ((data & ast.irqst & 4) != 0) {
						if (ast.timer4_cycle == 8388608) {
							int t = ast.base_pokey.tick_cycle4;
							while (t < ast.cycle)
								t += ast.base_pokey.period_cycles4;
							ast.timer4_cycle = t;
							if (ast.nearest_event_cycle > t)
								ast.nearest_event_cycle = t;
						}
					}
					else
						ast.timer4_cycle = 8388608;
				}
				else
					PokeySound_PutByte(ast, addr, data);
			}
			else
				if ((addr & 65295) == 54282) {
					if (ast.cycle <= ast.next_scanline_cycle - 4)
						ast.cycle = ast.next_scanline_cycle - 4;
					else
						ast.cycle = ast.next_scanline_cycle + 110;
				}
				else
					if ((addr & 65295) == 54287) {
						ast.nmist = ast.cycle < 28296 ? 1 : 0;
					}
					else
						if ((addr & 65280) == ast.module_info.covox_addr) {
							PokeyState pst;
							addr &= 3;
							if (addr == 0 || addr == 3)
								pst = ast.base_pokey;
							else
								pst = ast.extra_pokey;
							pst.delta_buffer[(ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447)] += data - ast.covox[addr] << 17;
							ast.covox[addr] = (byte) (data);
						}
						else
							if ((addr & 65311) == 53279) {
								data &= 8;
								int delta = ast.consol - data << 20;
								ast.consol = data;
								int sample = (ast.cycle * 44100 + ast.sample_offset) / (ast.module_info.ntsc ? 1789772 : 1773447);
								ast.base_pokey.delta_buffer[sample] += delta;
								ast.extra_pokey.delta_buffer[sample] += delta;
							}
							else
								ast.memory[addr] = (byte) (data);
		}

		/// <summary>Runs 6502 emulation for the specified number of Atari scanlines.</summary>
		/// <remarks>Each scanline is 114 cycles of which 9 is taken by ANTIC for memory refresh.</remarks>
		static void Cpu_RunScanlines(ASAP_State ast, int scanlines) {
			int pc = ast.cpu_pc;
			int nz = ast.cpu_nz;
			int a = ast.cpu_a;
			int x = ast.cpu_x;
			int y = ast.cpu_y;
			int c = ast.cpu_c;
			int s = ast.cpu_s;
			int vdi = ast.cpu_vdi;
			ast.next_scanline_cycle = 114;
			int next_event_cycle = 114;
			int cycle_limit = 114 * scanlines;
			if (next_event_cycle > ast.timer1_cycle)
				next_event_cycle = ast.timer1_cycle;
			if (next_event_cycle > ast.timer2_cycle)
				next_event_cycle = ast.timer2_cycle;
			if (next_event_cycle > ast.timer4_cycle)
				next_event_cycle = ast.timer4_cycle;
			ast.nearest_event_cycle = next_event_cycle;
			for (;;) {
				int cycle = ast.cycle;
				if (cycle >= ast.nearest_event_cycle) {
					if (cycle >= ast.next_scanline_cycle) {
						if (++ast.scanline_number == (ast.module_info.ntsc ? 262 : 312)) {
							ast.scanline_number = 0;
							ast.nmist = ast.nmist == 0 ? 1 : 2;
						}
						if (ast.cycle - ast.next_scanline_cycle < 50)
							ast.cycle = cycle += 9;
						ast.next_scanline_cycle += 114;
						if (--scanlines <= 0)
							break;
					}
					next_event_cycle = ast.next_scanline_cycle;
					if (cycle >= ast.timer1_cycle) {
						ast.irqst &= ~1;
						ast.timer1_cycle = 8388608;
					}
					else
						if (next_event_cycle > ast.timer1_cycle)
							next_event_cycle = ast.timer1_cycle;
					if (cycle >= ast.timer2_cycle) {
						ast.irqst &= ~2;
						ast.timer2_cycle = 8388608;
					}
					else
						if (next_event_cycle > ast.timer2_cycle)
							next_event_cycle = ast.timer2_cycle;
					if (cycle >= ast.timer4_cycle) {
						ast.irqst &= ~4;
						ast.timer4_cycle = 8388608;
					}
					else
						if (next_event_cycle > ast.timer4_cycle)
							next_event_cycle = ast.timer4_cycle;
					ast.nearest_event_cycle = next_event_cycle;
					if ((vdi & 4) == 0 && ast.irqst != 255) {
						ast.memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
						s = s - 1 & 255;
						vdi |= 4;
						pc = ast.memory[65534] + (ast.memory[65535] << 8);
						ast.cycle += 7;
					}
				}
				int data = ast.memory[pc++];
				ast.cycle += CiConstArray_3[data];
				int addr;
				switch (data) {
					case 0:
						pc++;
						ast.memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48);
						s = s - 1 & 255;
						vdi |= 4;
						pc = ast.memory[65534] + (ast.memory[65535] << 8);
						break;
					case 1:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						nz = a |= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 2:
					case 18:
					case 34:
					case 50:
					case 66:
					case 82:
					case 98:
					case 114:
					case 146:
					case 178:
					case 210:
					case 242:
						ast.scanline_number = (ast.scanline_number + scanlines - 1) % (ast.module_info.ntsc ? 262 : 312);
						scanlines = 1;
						ast.cycle = cycle_limit;
						break;
					case 5:
						addr = ast.memory[pc++];
						nz = a |= ast.memory[addr];
						break;
					case 6:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 8:
						ast.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48);
						s = s - 1 & 255;
						break;
					case 9:
						nz = a |= ast.memory[pc++];
						break;
					case 10:
						c = a >> 7;
						nz = a = a << 1 & 255;
						break;
					case 13:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = a |= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 14:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 16:
						if (nz < 128) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 17:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a |= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 21:
						addr = ast.memory[pc++] + x & 255;
						nz = a |= ast.memory[addr];
						break;
					case 22:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 24:
						c = 0;
						break;
					case 25:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a |= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 29:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						nz = a |= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 30:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 32:
						addr = ast.memory[pc++];
						ast.memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						pc = addr + (ast.memory[pc] << 8);
						break;
					case 33:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						nz = a &= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 36:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						vdi = (vdi & 12) + (nz & 64);
						nz = ((nz & 128) << 1) + (nz & a);
						break;
					case 37:
						addr = ast.memory[pc++];
						nz = a &= ast.memory[addr];
						break;
					case 38:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 40:
						s = s + 1 & 255;
						vdi = ast.memory[256 + s];
						nz = ((vdi & 128) << 1) + (~vdi & 2);
						c = vdi & 1;
						vdi &= 76;
						if ((vdi & 4) == 0 && ast.irqst != 255) {
							ast.memory[256 + s] = (byte) (pc >> 8);
							s = s - 1 & 255;
							ast.memory[256 + s] = (byte) pc;
							s = s - 1 & 255;
							ast.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
							s = s - 1 & 255;
							vdi |= 4;
							pc = ast.memory[65534] + (ast.memory[65535] << 8);
							ast.cycle += 7;
						}
						break;
					case 41:
						nz = a &= ast.memory[pc++];
						break;
					case 42:
						a = (a << 1) + c;
						c = a >> 8;
						nz = a &= 255;
						break;
					case 44:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						vdi = (vdi & 12) + (nz & 64);
						nz = ((nz & 128) << 1) + (nz & a);
						break;
					case 45:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = a &= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 46:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 48:
						if (nz >= 128) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 49:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a &= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 53:
						addr = ast.memory[pc++] + x & 255;
						nz = a &= ast.memory[addr];
						break;
					case 54:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 56:
						c = 1;
						break;
					case 57:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a &= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 61:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						nz = a &= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 62:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 64:
						s = s + 1 & 255;
						vdi = ast.memory[256 + s];
						nz = ((vdi & 128) << 1) + (~vdi & 2);
						c = vdi & 1;
						vdi &= 76;
						s = s + 1 & 255;
						pc = ast.memory[256 + s];
						s = s + 1 & 255;
						addr = ast.memory[256 + s];
						pc += addr << 8;
						if ((vdi & 4) == 0 && ast.irqst != 255) {
							ast.memory[256 + s] = (byte) (pc >> 8);
							s = s - 1 & 255;
							ast.memory[256 + s] = (byte) pc;
							s = s - 1 & 255;
							ast.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
							s = s - 1 & 255;
							vdi |= 4;
							pc = ast.memory[65534] + (ast.memory[65535] << 8);
							ast.cycle += 7;
						}
						break;
					case 65:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						nz = a ^= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 69:
						addr = ast.memory[pc++];
						nz = a ^= ast.memory[addr];
						break;
					case 70:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						break;
					case 72:
						ast.memory[256 + s] = (byte) (a);
						s = s - 1 & 255;
						break;
					case 73:
						nz = a ^= ast.memory[pc++];
						break;
					case 74:
						c = a & 1;
						nz = a >>= 1;
						break;
					case 76:
						addr = ast.memory[pc++];
						pc = addr + (ast.memory[pc] << 8);
						break;
					case 77:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = a ^= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 78:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 80:
						if ((vdi & 64) == 0) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 81:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a ^= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 85:
						addr = ast.memory[pc++] + x & 255;
						nz = a ^= ast.memory[addr];
						break;
					case 86:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						break;
					case 88:
						vdi &= 72;
						if ((vdi & 4) == 0 && ast.irqst != 255) {
							ast.memory[256 + s] = (byte) (pc >> 8);
							s = s - 1 & 255;
							ast.memory[256 + s] = (byte) pc;
							s = s - 1 & 255;
							ast.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
							s = s - 1 & 255;
							vdi |= 4;
							pc = ast.memory[65534] + (ast.memory[65535] << 8);
							ast.cycle += 7;
						}
						break;
					case 89:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a ^= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 93:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						nz = a ^= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 94:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 96:
						s = s + 1 & 255;
						pc = ast.memory[256 + s];
						s = s + 1 & 255;
						addr = ast.memory[256 + s];
						pc += (addr << 8) + 1;
						break;
					case 97:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 101:
						addr = ast.memory[pc++];
						data = ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 102:
						addr = ast.memory[pc++];
						nz = ast.memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						break;
					case 104:
						s = s + 1 & 255;
						a = ast.memory[256 + s];
						nz = a;
						break;
					case 105:
						data = ast.memory[pc++];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 106:
						nz = (c << 7) + (a >> 1);
						c = a & 1;
						a = nz;
						break;
					case 108:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if ((addr & 255) == 255)
							pc = (ast.memory[addr - 255] << 8) + ast.memory[addr];
						else
							pc = ast.memory[addr] + (ast.memory[addr + 1] << 8);
						break;
					case 109:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 110:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 112:
						if ((vdi & 64) != 0) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 113:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 117:
						addr = ast.memory[pc++] + x & 255;
						data = ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 118:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						break;
					case 120:
						vdi |= 4;
						break;
					case 121:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 125:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 126:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 129:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, a);
						else
							ast.memory[addr] = (byte) (a);
						break;
					case 132:
						addr = ast.memory[pc++];
						ast.memory[addr] = (byte) (y);
						break;
					case 133:
						addr = ast.memory[pc++];
						ast.memory[addr] = (byte) (a);
						break;
					case 134:
						addr = ast.memory[pc++];
						ast.memory[addr] = (byte) (x);
						break;
					case 136:
						nz = y = y - 1 & 255;
						break;
					case 138:
						nz = a = x;
						break;
					case 140:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, y);
						else
							ast.memory[addr] = (byte) (y);
						break;
					case 141:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, a);
						else
							ast.memory[addr] = (byte) (a);
						break;
					case 142:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, x);
						else
							ast.memory[addr] = (byte) (x);
						break;
					case 144:
						if (c == 0) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 145:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, a);
						else
							ast.memory[addr] = (byte) (a);
						break;
					case 148:
						addr = ast.memory[pc++] + x & 255;
						ast.memory[addr] = (byte) (y);
						break;
					case 149:
						addr = ast.memory[pc++] + x & 255;
						ast.memory[addr] = (byte) (a);
						break;
					case 150:
						addr = ast.memory[pc++] + y & 255;
						ast.memory[addr] = (byte) (x);
						break;
					case 152:
						nz = a = y;
						break;
					case 153:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, a);
						else
							ast.memory[addr] = (byte) (a);
						break;
					case 154:
						s = x;
						break;
					case 157:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, a);
						else
							ast.memory[addr] = (byte) (a);
						break;
					case 160:
						nz = y = ast.memory[pc++];
						break;
					case 161:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						nz = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 162:
						nz = x = ast.memory[pc++];
						break;
					case 164:
						addr = ast.memory[pc++];
						nz = y = ast.memory[addr];
						break;
					case 165:
						addr = ast.memory[pc++];
						nz = a = ast.memory[addr];
						break;
					case 166:
						addr = ast.memory[pc++];
						nz = x = ast.memory[addr];
						break;
					case 168:
						nz = y = a;
						break;
					case 169:
						nz = a = ast.memory[pc++];
						break;
					case 170:
						nz = x = a;
						break;
					case 172:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = y = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 173:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 174:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = x = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 176:
						if (c != 0) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 177:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 180:
						addr = ast.memory[pc++] + x & 255;
						nz = y = ast.memory[addr];
						break;
					case 181:
						addr = ast.memory[pc++] + x & 255;
						nz = a = ast.memory[addr];
						break;
					case 182:
						addr = ast.memory[pc++] + y & 255;
						nz = x = ast.memory[addr];
						break;
					case 184:
						vdi &= 12;
						break;
					case 185:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 186:
						nz = x = s;
						break;
					case 188:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						nz = y = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 189:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						nz = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 190:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = x = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 192:
						nz = ast.memory[pc++];
						c = y >= nz ? 1 : 0;
						nz = y - nz & 255;
						break;
					case 193:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 196:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = y >= nz ? 1 : 0;
						nz = y - nz & 255;
						break;
					case 197:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 198:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						nz = nz - 1 & 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 200:
						nz = y = y + 1 & 255;
						break;
					case 201:
						nz = ast.memory[pc++];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 202:
						nz = x = x - 1 & 255;
						break;
					case 204:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = y >= nz ? 1 : 0;
						nz = y - nz & 255;
						break;
					case 205:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 206:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 208:
						if ((nz & 255) != 0) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 209:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 213:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 214:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						nz = nz - 1 & 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 216:
						vdi &= 68;
						break;
					case 217:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 221:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 222:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 224:
						nz = ast.memory[pc++];
						c = x >= nz ? 1 : 0;
						nz = x - nz & 255;
						break;
					case 225:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 228:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = x >= nz ? 1 : 0;
						nz = x - nz & 255;
						break;
					case 229:
						addr = ast.memory[pc++];
						data = ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 230:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						nz = nz + 1 & 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 232:
						nz = x = x + 1 & 255;
						break;
					case 233:
					case 235:
						data = ast.memory[pc++];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 234:
					case 26:
					case 58:
					case 90:
					case 122:
					case 218:
					case 250:
						break;
					case 236:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						c = x >= nz ? 1 : 0;
						nz = x - nz & 255;
						break;
					case 237:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 238:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 240:
						if ((nz & 255) == 0) {
							addr = (sbyte) ast.memory[pc];
							pc++;
							addr += pc;
							if ((addr ^ pc) >> 8 != 0)
								ast.cycle++;
							ast.cycle++;
							pc = addr;
							break;
						}
						pc++;
						break;
					case 241:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 245:
						addr = ast.memory[pc++] + x & 255;
						data = ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 246:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						nz = nz + 1 & 255;
						ast.memory[addr] = (byte) (nz);
						break;
					case 248:
						vdi |= 8;
						break;
					case 249:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 253:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if ((addr & 255) < x)
							ast.cycle++;
						data = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 254:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						break;
					case 3:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 4:
					case 68:
					case 100:
					case 20:
					case 52:
					case 84:
					case 116:
					case 212:
					case 244:
					case 128:
					case 130:
					case 137:
					case 194:
					case 226:
						pc++;
						break;
					case 7:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 11:
					case 43:
						nz = a &= ast.memory[pc++];
						c = nz >> 7;
						break;
					case 12:
						pc += 2;
						break;
					case 15:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 19:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 23:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 27:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 28:
					case 60:
					case 92:
					case 124:
					case 220:
					case 252:
						if (ast.memory[pc++] + x >= 256)
							ast.cycle++;
						pc++;
						break;
					case 31:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz >> 7;
						nz = nz << 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a |= nz;
						break;
					case 35:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 39:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 47:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 51:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 55:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 59:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 63:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = (nz << 1) + c;
						c = nz >> 8;
						nz &= 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a &= nz;
						break;
					case 67:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 71:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 75:
						a &= ast.memory[pc++];
						c = a & 1;
						nz = a >>= 1;
						break;
					case 79:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 83:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 87:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 91:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 95:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						nz = a ^= nz;
						break;
					case 99:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 103:
						addr = ast.memory[pc++];
						nz = ast.memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 107:
						data = a & ast.memory[pc++];
						nz = a = (data >> 1) + (c << 7);
						vdi = (vdi & 12) + ((a ^ data) & 64);
						if ((vdi & 8) == 0)
							c = data >> 7;
						else {
							if ((data & 15) >= 5)
								a = (a & 240) + (a + 6 & 15);
							if (data >= 80) {
								a = a + 96 & 255;
								c = 1;
							}
							else
								c = 0;
						}
						break;
					case 111:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 115:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 119:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr] + (c << 8);
						c = nz & 1;
						nz >>= 1;
						ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 123:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 127:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz += c << 8;
						c = nz & 1;
						nz >>= 1;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a + data + c;
							nz = tmp & 255;
							if ((vdi & 8) == 0) {
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								c = tmp >> 8;
								a = nz;
							}
							else {
								int al = (a & 15) + (data & 15) + c;
								if (al >= 10)
									tmp += al < 26 ? 6 : -10;
								nz = ((tmp & 128) << 1) + (nz != 0 ? 1 : 0);
								vdi = (vdi & 12) + ((~(data ^ a) & (a ^ tmp)) >> 1 & 64);
								if (tmp >= 160) {
									c = 1;
									a = tmp + 96 & 255;
								}
								else {
									c = 0;
									a = tmp;
								}
							}
						}
						break;
					case 131:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						data = a & x;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, data);
						else
							ast.memory[addr] = (byte) (data);
						break;
					case 135:
						addr = ast.memory[pc++];
						data = a & x;
						ast.memory[addr] = (byte) (data);
						break;
					case 139:
						data = ast.memory[pc++];
						a &= (data | 239) & x;
						nz = a & data;
						break;
					case 143:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						data = a & x;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, data);
						else
							ast.memory[addr] = (byte) (data);
						break;
					case 147:
 {
							addr = ast.memory[pc++];
							int hi = ast.memory[addr + 1 & 255];
							addr = ast.memory[addr];
							data = hi + 1 & a & x;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								ASAP_PutByte(ast, addr, data);
							else
								ast.memory[addr] = (byte) (data);
						}
						break;
					case 151:
						addr = ast.memory[pc++] + y & 255;
						data = a & x;
						ast.memory[addr] = (byte) (data);
						break;
					case 155:
						s = a & x;
 {
							addr = ast.memory[pc++];
							int hi = ast.memory[pc++];
							data = hi + 1 & s;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								ASAP_PutByte(ast, addr, data);
							else
								ast.memory[addr] = (byte) (data);
						}
						break;
					case 156:
 {
							addr = ast.memory[pc++];
							int hi = ast.memory[pc++];
							data = hi + 1 & y;
							addr += x;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								ASAP_PutByte(ast, addr, data);
							else
								ast.memory[addr] = (byte) (data);
						}
						break;
					case 158:
 {
							addr = ast.memory[pc++];
							int hi = ast.memory[pc++];
							data = hi + 1 & x;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								ASAP_PutByte(ast, addr, data);
							else
								ast.memory[addr] = (byte) (data);
						}
						break;
					case 159:
 {
							addr = ast.memory[pc++];
							int hi = ast.memory[pc++];
							data = hi + 1 & a & x;
							addr += y;
							if (addr >= 256)
								hi = data - 1;
							addr += hi << 8;
							if ((addr & 63744) == 53248)
								ASAP_PutByte(ast, addr, data);
							else
								ast.memory[addr] = (byte) (data);
						}
						break;
					case 163:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						nz = x = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 167:
						addr = ast.memory[pc++];
						nz = x = a = ast.memory[addr];
						break;
					case 171:
						nz = x = a &= ast.memory[pc++];
						break;
					case 175:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						nz = x = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 179:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = x = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 183:
						addr = ast.memory[pc++] + y & 255;
						nz = x = a = ast.memory[addr];
						break;
					case 187:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = x = a = s &= (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 191:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if ((addr & 255) < y)
							ast.cycle++;
						nz = x = a = (addr & 63744) == 53248 ? ASAP_GetByte(ast, addr) : ast.memory[addr];
						break;
					case 195:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 199:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						nz = nz - 1 & 255;
						ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 203:
						nz = ast.memory[pc++];
						x &= a;
						c = x >= nz ? 1 : 0;
						nz = x = x - nz & 255;
						break;
					case 207:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 211:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 215:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						nz = nz - 1 & 255;
						ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 219:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 223:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz - 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						c = a >= nz ? 1 : 0;
						nz = a - nz & 255;
						break;
					case 227:
						addr = ast.memory[pc++] + x & 255;
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8);
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 231:
						addr = ast.memory[pc++];
						nz = ast.memory[addr];
						nz = nz + 1 & 255;
						ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 239:
						addr = ast.memory[pc++];
						addr += ast.memory[pc++] << 8;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 243:
						addr = ast.memory[pc++];
						addr = ast.memory[addr] + (ast.memory[addr + 1 & 255] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 247:
						addr = ast.memory[pc++] + x & 255;
						nz = ast.memory[addr];
						nz = nz + 1 & 255;
						ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 251:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + y & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
					case 255:
						addr = ast.memory[pc++];
						addr = addr + (ast.memory[pc++] << 8) + x & 65535;
						if (addr >> 8 == 210) {
							nz = ASAP_GetByte(ast, addr);
							ast.cycle--;
							ASAP_PutByte(ast, addr, nz);
							ast.cycle++;
						}
						else
							nz = ast.memory[addr];
						nz = nz + 1 & 255;
						if ((addr & 63744) == 53248)
							ASAP_PutByte(ast, addr, nz);
						else
							ast.memory[addr] = (byte) (nz);
						data = nz;
 {
							int tmp = a - data - 1 + c;
							int al = (a & 15) - (data & 15) - 1 + c;
							vdi = (vdi & 12) + (((data ^ a) & (a ^ tmp)) >> 1 & 64);
							c = tmp >= 0 ? 1 : 0;
							nz = a = tmp & 255;
							if ((vdi & 8) != 0) {
								if (al < 0)
									a += al < -10 ? 10 : -6;
								if (c == 0)
									a = a - 96 & 255;
							}
						}
						break;
				}
			}
			ast.cpu_pc = pc;
			ast.cpu_nz = nz;
			ast.cpu_a = a;
			ast.cpu_x = x;
			ast.cpu_y = y;
			ast.cpu_c = c;
			ast.cpu_s = s;
			ast.cpu_vdi = vdi;
			ast.cycle -= cycle_limit;
			if (ast.timer1_cycle != 8388608)
				ast.timer1_cycle -= cycle_limit;
			if (ast.timer2_cycle != 8388608)
				ast.timer2_cycle -= cycle_limit;
			if (ast.timer4_cycle != 8388608)
				ast.timer4_cycle -= cycle_limit;
		}

		static int uword(byte[] array, int i) {
			return array[i] + (array[i + 1] << 8);
		}

		/// <summary>Loads a native module (anything except SAP) and a 6502 player routine.</summary>
		static bool load_native(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len, byte[] player) {
			if ((module[0] != 255 || module[1] != 255) && (module[0] != 0 || module[1] != 0))
				return false;
			module_info.music = uword(module, 2);
			module_info.player = uword(player, 2);
			int player_last_byte = uword(player, 4);
			if (module_info.music <= player_last_byte)
				return false;
			int music_last_byte = uword(module, 4);
			if (module_info.music <= 55295 && music_last_byte >= 53248)
				return false;
			int block_len = music_last_byte + 1 - module_info.music;
			if (6 + block_len != module_len) {
				if (module_info.type != 11 || 11 + block_len > module_len)
					return false;
				int info_addr = uword(module, 6 + block_len);
				if (info_addr != module_info.music + block_len)
					return false;
				int info_len = uword(module, 8 + block_len) + 1 - info_addr;
				if (10 + block_len + info_len != module_len)
					return false;
			}
			if (ast != null) {
				System.Array.Copy(module, 6, ast.memory, module_info.music, block_len);
				System.Array.Copy(player, 6, ast.memory, module_info.player, player_last_byte + 1 - module_info.player);
			}
			return true;
		}

		static void set_song_duration(ASAP_ModuleInfo module_info, int player_calls) {
			module_info.durations[module_info.songs] = (int) ((long) (player_calls * module_info.fastplay) * (114000) / (1773447));
			module_info.songs++;
		}

		static void parse_cmc_song(ASAP_ModuleInfo module_info, byte[] module, int pos) {
			int tempo = module[25];
			int player_calls = 0;
			int rep_start_pos = 0;
			int rep_end_pos = 0;
			int rep_times = 0;
			byte[] seen = new byte[85];
			while (pos >= 0 && pos < 85) {
				if (pos == rep_end_pos && rep_times > 0) {
					for (int i = 0; i < 85; i++)
						if (seen[i] == 1 || seen[i] == 3)
							seen[i] = 0;
					rep_times--;
					pos = rep_start_pos;
				}
				if (seen[pos] != 0) {
					if (seen[pos] != 1)
						module_info.loops[module_info.songs] = true;
					break;
				}
				seen[pos] = (byte) (1);
				int p1 = module[518 + pos];
				int p2 = module[603 + pos];
				int p3 = module[688 + pos];
				if (p1 == 254 || p2 == 254 || p3 == 254) {
					pos++;
					continue;
				}
				p1 >>= 4;
				if (p1 == 8)
					break;
				if (p1 == 9) {
					pos = p2;
					continue;
				}
				if (p1 == 10) {
					pos -= p2;
					continue;
				}
				if (p1 == 11) {
					pos += p2;
					continue;
				}
				if (p1 == 12) {
					tempo = p2;
					pos++;
					continue;
				}
				if (p1 == 13) {
					pos++;
					rep_start_pos = pos;
					rep_end_pos = pos + p2;
					rep_times = p3 - 1;
					continue;
				}
				if (p1 == 14) {
					module_info.loops[module_info.songs] = true;
					break;
				}
				p2 = rep_times > 0 ? 3 : 2;
				for (p1 = 0; p1 < 85; p1++)
					if (seen[p1] == 1)
						seen[p1] = (byte) (p2);
				player_calls += tempo * (module_info.type == 6 ? 48 : 64);
				pos++;
			}
			set_song_duration(module_info, player_calls);
		}

		static bool parse_cmc(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len, int type, byte[] player) {
			if (module_len < 774)
				return false;
			module_info.type = type;
			if (!load_native(ast, module_info, module, module_len, player))
				return false;
			if (ast != null && type == 7) {
			}
			int last_pos = 84;
			while (--last_pos >= 0) {
				if (module[518 + last_pos] < 176 || module[603 + last_pos] < 64 || module[688 + last_pos] < 64)
					break;
				if (module_info.channels == 2) {
					if (module[774 + last_pos] < 176 || module[859 + last_pos] < 64 || module[944 + last_pos] < 64)
						break;
				}
			}
			module_info.songs = 0;
			parse_cmc_song(module_info, module, 0);
			for (int pos = 0; pos < last_pos && module_info.songs < 32; pos++)
				if (module[518 + pos] == 143 || module[518 + pos] == 239)
					parse_cmc_song(module_info, module, pos + 1);
			return true;
		}

		static bool is_dlt_track_empty(byte[] module, int pos) {
			return module[8198 + pos] >= 67 && module[8454 + pos] >= 64 && module[8710 + pos] >= 64 && module[8966 + pos] >= 64;
		}

		static bool is_dlt_pattern_end(byte[] module, int pos, int i) {
			for (int ch = 0; ch < 4; ch++) {
				int pattern = module[8198 + (ch << 8) + pos];
				if (pattern < 64) {
					int offset = 6 + (pattern << 7) + (i << 1);
					if ((module[offset] & 128) == 0 && (module[offset + 1] & 128) != 0)
						return true;
				}
			}
			return false;
		}

		static void parse_dlt_song(ASAP_ModuleInfo module_info, byte[] module, bool[] seen, int pos) {
			while (pos < 128 && !seen[pos] && is_dlt_track_empty(module, pos))
				seen[pos++] = true;
			module_info.song_pos[module_info.songs] = (byte) (pos);
			int player_calls = 0;
			bool loop = false;
			int tempo = 6;
			while (pos < 128) {
				if (seen[pos]) {
					loop = true;
					break;
				}
				seen[pos] = true;
				int p1 = module[8198 + pos];
				if (p1 == 64 || is_dlt_track_empty(module, pos))
					break;
				if (p1 == 65)
					pos = module[8326 + pos];
				else
					if (p1 == 66)
						tempo = module[8326 + pos++];
					else {
						for (int i = 0; i < 64 && !is_dlt_pattern_end(module, pos, i); i++)
							player_calls += tempo;
						pos++;
					}
			}
			if (player_calls > 0) {
				module_info.loops[module_info.songs] = loop;
				set_song_duration(module_info, player_calls);
			}
		}

		static bool parse_dlt(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (module_len == 11270) {
				if (ast != null)
					ast.memory[19456] = 0;
			}
			else
				if (module_len != 11271)
					return false;
			module_info.type = 9;
			if (!load_native(ast, module_info, module, module_len, null) || module_info.music != 8192)
				return false;
			bool[] seen = new bool[128];
			module_info.songs = 0;
			for (int pos = 0; pos < 128 && module_info.songs < 32; pos++) {
				if (!seen[pos])
					parse_dlt_song(module_info, module, seen, pos);
			}
			return module_info.songs > 0;
		}

		static void parse_mpt_song(ASAP_ModuleInfo module_info, byte[] module, bool[] global_seen, int song_len, int pos) {
			int addr_to_offset = uword(module, 2) - 6;
			int tempo = module[463];
			int player_calls = 0;
			byte[] seen = new byte[256];
			int[] pattern_offset = new int[4];
			int[] blank_rows = new int[4];
			int[] blank_rows_counter = new int[4];
			while (pos < song_len) {
				if (seen[pos] != 0) {
					if (seen[pos] != 1)
						module_info.loops[module_info.songs] = true;
					break;
				}
				seen[pos] = (byte) (1);
				global_seen[pos] = true;
				int i = module[464 + pos * 2];
				if (i == 255) {
					pos = module[465 + pos * 2];
					continue;
				}
				int ch;
				for (ch = 3; ch >= 0; ch--) {
					i = module[454 + ch] + (module[458 + ch] << 8) - addr_to_offset;
					i = module[i + pos * 2];
					if (i >= 64)
						break;
					i <<= 1;
					i = uword(module, 70 + i);
					pattern_offset[ch] = i == 0 ? 0 : i - addr_to_offset;
					blank_rows_counter[ch] = 0;
				}
				if (ch >= 0)
					break;
				for (i = 0; i < song_len; i++)
					if (seen[i] == 1)
						seen[i] = (byte) (2);
				for (int pattern_rows = module[462]; --pattern_rows >= 0;) {
					for (ch = 3; ch >= 0; ch--) {
						if (pattern_offset[ch] == 0 || --blank_rows_counter[ch] >= 0)
							continue;
						for (;;) {
							i = module[pattern_offset[ch]++];
							if (i < 64 || i == 254)
								break;
							if (i < 128)
								continue;
							if (i < 192) {
								blank_rows[ch] = i - 128;
								continue;
							}
							if (i < 208)
								continue;
							if (i < 224) {
								tempo = i - 207;
								continue;
							}
							pattern_rows = 0;
						}
						blank_rows_counter[ch] = blank_rows[ch];
					}
					player_calls += tempo;
				}
				pos++;
			}
			if (player_calls > 0)
				set_song_duration(module_info, player_calls);
		}

		static bool parse_mpt(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (module_len < 464)
				return false;
			module_info.type = 10;
			if (!load_native(ast, module_info, module, module_len, null))
				return false;
			int track0_addr = uword(module, 2) + 458;
			if (module[454] + (module[458] << 8) != track0_addr)
				return false;
			int song_len = module[455] + (module[459] << 8) - track0_addr >> 1;
			if (song_len > 254)
				return false;
			bool[] global_seen = new bool[256];
			module_info.songs = 0;
			for (int pos = 0; pos < song_len && module_info.songs < 32; pos++) {
				if (!global_seen[pos]) {
					module_info.song_pos[module_info.songs] = (byte) (pos);
					parse_mpt_song(module_info, module, global_seen, song_len, pos);
				}
			}
			return module_info.songs > 0;
		}

		static int rmt_instrument_frames(byte[] module, int instrument, int volume, int volume_frame, bool extra_pokey) {
			int addr_to_offset = uword(module, 2) - 6;
			instrument = uword(module, 14) - addr_to_offset + (instrument << 1);
			if (module[instrument + 1] == 0)
				return 0;
			instrument = uword(module, instrument) - addr_to_offset;
			int per_frame = module[12];
			int player_call = volume_frame * per_frame;
			int player_calls = player_call;
			int index = module[instrument] + 1 + player_call * 3;
			int index_end = module[instrument + 2] + 3;
			int index_loop = module[instrument + 3];
			if (index_loop >= index_end)
				return 0;
			int volume_slide_depth = module[instrument + 6];
			int volume_min = module[instrument + 7];
			if (index >= index_end)
				index = (index - index_end) % (index_end - index_loop) + index_loop;
			else {
				do {
					int vol = module[instrument + index];
					if (extra_pokey)
						vol >>= 4;
					if ((vol & 15) >= CiConstArray_5[volume])
						player_calls = player_call + 1;
					player_call++;
					index += 3;
				}
				while (index < index_end);
			}
			if (volume_slide_depth == 0)
				return player_calls / per_frame;
			int volume_slide = 128;
			bool silent_loop = false;
			for (;;) {
				if (index >= index_end) {
					if (silent_loop)
						break;
					silent_loop = true;
					index = index_loop;
				}
				int vol = module[instrument + index];
				if (extra_pokey)
					vol >>= 4;
				if ((vol & 15) >= CiConstArray_5[volume]) {
					player_calls = player_call + 1;
					silent_loop = false;
				}
				player_call++;
				index += 3;
				volume_slide -= volume_slide_depth;
				if (volume_slide < 0) {
					volume_slide += 256;
					if (--volume <= volume_min)
						break;
				}
			}
			return player_calls / per_frame;
		}

		static void parse_rmt_song(ASAP_ModuleInfo module_info, byte[] module, bool[] global_seen, int song_len, int pos_shift, int pos) {
			int addr_to_offset = uword(module, 2) - 6;
			int tempo = module[11];
			int frames = 0;
			int song_offset = uword(module, 20) - addr_to_offset;
			int pattern_lo_offset = uword(module, 16) - addr_to_offset;
			int pattern_hi_offset = uword(module, 18) - addr_to_offset;
			byte[] seen = new byte[256];
			int[] pattern_begin = new int[8];
			int[] pattern_offset = new int[8];
			int[] blank_rows = new int[8];
			int[] instrument_no = new int[8];
			int[] instrument_frame = new int[8];
			int[] volume_value = new int[8];
			int[] volume_frame = new int[8];
			while (pos < song_len) {
				if (seen[pos] != 0) {
					if (seen[pos] != 1)
						module_info.loops[module_info.songs] = true;
					break;
				}
				seen[pos] = (byte) (1);
				global_seen[pos] = true;
				if (module[song_offset + (pos << pos_shift)] == 254) {
					pos = module[song_offset + (pos << pos_shift) + 1];
					continue;
				}
				for (int ch = 0; ch < 1 << pos_shift; ch++) {
					int p = module[song_offset + (pos << pos_shift) + ch];
					if (p == 255)
						blank_rows[ch] = 256;
					else {
						pattern_offset[ch] = pattern_begin[ch] = module[pattern_lo_offset + p] + (module[pattern_hi_offset + p] << 8) - addr_to_offset;
						blank_rows[ch] = 0;
					}
				}
				for (int i = 0; i < song_len; i++)
					if (seen[i] == 1)
						seen[i] = (byte) (2);
				for (int pattern_rows = module[10]; --pattern_rows >= 0;) {
					for (int ch = 0; ch < 1 << pos_shift; ch++) {
						if (--blank_rows[ch] > 0)
							continue;
						for (;;) {
							int i = module[pattern_offset[ch]++];
							if ((i & 63) < 62) {
								i += module[pattern_offset[ch]++] << 8;
								if ((i & 63) != 61) {
									instrument_no[ch] = i >> 10;
									instrument_frame[ch] = frames;
								}
								volume_value[ch] = i >> 6 & 15;
								volume_frame[ch] = frames;
								break;
							}
							if (i == 62) {
								blank_rows[ch] = module[pattern_offset[ch]++];
								break;
							}
							if ((i & 63) == 62) {
								blank_rows[ch] = i >> 6;
								break;
							}
							if ((i & 191) == 63) {
								tempo = module[pattern_offset[ch]++];
								continue;
							}
							if (i == 191) {
								pattern_offset[ch] = pattern_begin[ch] + module[pattern_offset[ch]];
								continue;
							}
							pattern_rows = -1;
							break;
						}
						if (pattern_rows < 0)
							break;
					}
					if (pattern_rows >= 0)
						frames += tempo;
				}
				pos++;
			}
			int instrument_frames = 0;
			for (int ch = 0; ch < 1 << pos_shift; ch++) {
				int frame = instrument_frame[ch];
				frame += rmt_instrument_frames(module, instrument_no[ch], volume_value[ch], volume_frame[ch] - frame, ch >= 4);
				if (instrument_frames < frame)
					instrument_frames = frame;
			}
			if (frames > instrument_frames) {
				if (frames - instrument_frames > 100)
					module_info.loops[module_info.songs] = false;
				frames = instrument_frames;
			}
			if (frames > 0)
				set_song_duration(module_info, frames);
		}

		static bool parse_rmt(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (module_len < 48 || module[6] != 82 || module[7] != 77 || module[8] != 84 || module[13] != 1)
				return false;
			int pos_shift;
			switch (module[9]) {
				case 52:
					pos_shift = 2;
					break;
				case 56:
					module_info.channels = 2;
					pos_shift = 3;
					break;
				default:
					return false;
			}
			int per_frame = module[12];
			if (per_frame < 1 || per_frame > 4)
				return false;
			module_info.type = 11;
			if (!load_native(ast, module_info, module, module_len, module_info.channels == 2 ? null : null))
				return false;
			int song_len = uword(module, 4) + 1 - uword(module, 20);
			if (pos_shift == 3 && (song_len & 4) != 0 && module[6 + uword(module, 4) - uword(module, 2) - 3] == 254)
				song_len += 4;
			song_len >>= pos_shift;
			if (song_len >= 256)
				return false;
			bool[] global_seen = new bool[256];
			module_info.songs = 0;
			for (int pos = 0; pos < song_len && module_info.songs < 32; pos++) {
				if (!global_seen[pos]) {
					module_info.song_pos[module_info.songs] = (byte) (pos);
					parse_rmt_song(module_info, module, global_seen, song_len, pos_shift, pos);
				}
			}
			module_info.fastplay = 312 / per_frame;
			module_info.player = 1536;
			return module_info.songs > 0;
		}

		static void parse_tmc_song(ASAP_ModuleInfo module_info, byte[] module, int pos) {
			int addr_to_offset = uword(module, 2) - 6;
			int tempo = module[36] + 1;
			int frames = 0;
			int[] pattern_offset = new int[8];
			int[] blank_rows = new int[8];
			while (module[437 + pos] < 128) {
				for (int ch = 7; ch >= 0; ch--) {
					int pat = module[437 + pos - 2 * ch];
					pattern_offset[ch] = module[166 + pat] + (module[294 + pat] << 8) - addr_to_offset;
					blank_rows[ch] = 0;
				}
				for (int pattern_rows = 64; --pattern_rows >= 0;) {
					for (int ch = 7; ch >= 0; ch--) {
						if (--blank_rows[ch] >= 0)
							continue;
						for (;;) {
							int i = module[pattern_offset[ch]++];
							if (i < 64) {
								pattern_offset[ch]++;
								break;
							}
							if (i == 64) {
								i = module[pattern_offset[ch]++];
								if ((i & 127) == 0)
									pattern_rows = 0;
								else
									tempo = (i & 127) + 1;
								if (i >= 128)
									pattern_offset[ch]++;
								break;
							}
							if (i < 128) {
								i = module[pattern_offset[ch]++] & 127;
								if (i == 0)
									pattern_rows = 0;
								else
									tempo = i + 1;
								pattern_offset[ch]++;
								break;
							}
							if (i < 192)
								continue;
							blank_rows[ch] = i - 191;
							break;
						}
					}
					frames += tempo;
				}
				pos += 16;
			}
			if (module[436 + pos] < 128)
				module_info.loops[module_info.songs] = true;
			set_song_duration(module_info, frames);
		}

		static bool parse_tmc(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (module_len < 464)
				return false;
			module_info.type = 12;
			if (!load_native(ast, module_info, module, module_len, null))
				return false;
			module_info.channels = 2;
			int i = 0;
			while (module[102 + i] == 0) {
				if (++i >= 64)
					return false;
			}
			int last_pos = (module[102 + i] << 8) + module[38 + i] - uword(module, 2) - 432;
			if (437 + last_pos >= module_len)
				return false;
			do {
				if (last_pos <= 0)
					return false;
				last_pos -= 16;
			}
			while (module[437 + last_pos] >= 128);
			module_info.songs = 0;
			parse_tmc_song(module_info, module, 0);
			for (i = 0; i < last_pos && module_info.songs < 32; i += 16)
				if (module[437 + i] >= 128)
					parse_tmc_song(module_info, module, i + 16);
			i = module[37];
			if (i < 1 || i > 4)
				return false;
			if (ast != null)
				ast.tmc_per_frame = i;
			module_info.fastplay = 312 / i;
			return true;
		}

		static void parse_tm2_song(ASAP_ModuleInfo module_info, byte[] module, int pos) {
			int addr_to_offset = uword(module, 2) - 6;
			int tempo = module[36] + 1;
			int player_calls = 0;
			int[] pattern_offset = new int[8];
			int[] blank_rows = new int[8];
			for (;;) {
				int pattern_rows = module[918 + pos];
				if (pattern_rows == 0)
					break;
				if (pattern_rows >= 128) {
					module_info.loops[module_info.songs] = true;
					break;
				}
				for (int ch = 7; ch >= 0; ch--) {
					int pat = module[917 + pos - 2 * ch];
					pattern_offset[ch] = module[262 + pat] + (module[518 + pat] << 8) - addr_to_offset;
					blank_rows[ch] = 0;
				}
				while (--pattern_rows >= 0) {
					for (int ch = 7; ch >= 0; ch--) {
						if (--blank_rows[ch] >= 0)
							continue;
						for (;;) {
							int i = module[pattern_offset[ch]++];
							if (i == 0) {
								pattern_offset[ch]++;
								break;
							}
							if (i < 64) {
								if (module[pattern_offset[ch]++] >= 128)
									pattern_offset[ch]++;
								break;
							}
							if (i < 128) {
								pattern_offset[ch]++;
								break;
							}
							if (i == 128) {
								blank_rows[ch] = module[pattern_offset[ch]++];
								break;
							}
							if (i < 192)
								break;
							if (i < 208) {
								tempo = i - 191;
								continue;
							}
							if (i < 224) {
								pattern_offset[ch]++;
								break;
							}
							if (i < 240) {
								pattern_offset[ch] += 2;
								break;
							}
							if (i < 255) {
								blank_rows[ch] = i - 240;
								break;
							}
							blank_rows[ch] = 64;
							break;
						}
					}
					player_calls += tempo;
				}
				pos += 17;
			}
			set_song_duration(module_info, player_calls);
		}

		static bool parse_tm2(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (module_len < 932)
				return false;
			module_info.type = 13;
			if (!load_native(ast, module_info, module, module_len, null))
				return false;
			int i = module[37];
			if (i < 1 || i > 4)
				return false;
			module_info.fastplay = 312 / i;
			module_info.player = 1280;
			if (module[31] != 0)
				module_info.channels = 2;
			int last_pos = 65535;
			for (i = 0; i < 128; i++) {
				int instr_addr = module[134 + i] + (module[774 + i] << 8);
				if (instr_addr != 0 && instr_addr < last_pos)
					last_pos = instr_addr;
			}
			for (i = 0; i < 256; i++) {
				int pattern_addr = module[262 + i] + (module[518 + i] << 8);
				if (pattern_addr != 0 && pattern_addr < last_pos)
					last_pos = pattern_addr;
			}
			last_pos -= uword(module, 2) + 896;
			if (902 + last_pos >= module_len)
				return false;
			int c;
			do {
				if (last_pos <= 0)
					return false;
				last_pos -= 17;
				c = module[918 + last_pos];
			}
			while (c == 0 || c >= 128);
			module_info.songs = 0;
			parse_tm2_song(module_info, module, 0);
			for (i = 0; i < last_pos && module_info.songs < 32; i += 17) {
				c = module[918 + i];
				if (c == 0 || c >= 128)
					parse_tm2_song(module_info, module, i + 17);
			}
			return true;
		}

		static bool has_string_at(byte[] module, int module_index, string s) {
			int n = s.Length;
			for (int i = 0; i < n; i++)
				if (module[module_index + i] != s[i])
					return false;
			return true;
		}

		static string parse_text(byte[] module, int module_index) {
			if (module[module_index] != 34)
				return null;
			for (int i = 0;; i++) {
				int c = module[module_index + 1 + i];
				if (c == 34)
					break;
				if (c < 32 || c >= 127)
					return null;
			}
			return null;
		}

		static int parse_dec(byte[] module, int module_index, int maxval) {
			if (module[module_index] == 13)
				return -1;
			for (int r = 0;;) {
				int c = module[module_index++];
				if (c == 13)
					return r;
				if (c < 48 || c > 57)
					return -1;
				r = 10 * r + c - 48;
				if (r > maxval)
					return -1;
			}
		}

		static int parse_hex(byte[] module, int module_index) {
			if (module[module_index] == 13)
				return -1;
			for (int r = 0;;) {
				int c = module[module_index++];
				if (c == 13)
					return r;
				if (r > 4095)
					return -1;
				r <<= 4;
				if (c >= 48 && c <= 57)
					r += c - 48;
				else
					if (c >= 65 && c <= 70)
						r += c - 65 + 10;
					else
						if (c >= 97 && c <= 102)
							r += c - 97 + 10;
						else
							return -1;
			}
		}

		static int ASAP_ParseDuration(string s) {
			int i = 0;
			int n = s.Length;
			int d;
			if (i >= n)
				return -1;
			d = s[i] - 48;
			if (d < 0 || d > 9)
				return -1;
			i++;
			int r = d;
			if (i < n) {
				d = s[i] - 48;
				if (d >= 0 && d <= 9) {
					i++;
					r = 10 * r + d;
				}
				if (i < n && s[i] == 58) {
					i++;
					if (i >= n)
						return -1;
					d = s[i] - 48;
					if (d < 0 || d > 5)
						return -1;
					i++;
					r = (6 * r + d) * 10;
					if (i >= n)
						return -1;
					d = s[i] - 48;
					if (d < 0 || d > 9)
						return -1;
					i++;
					r += d;
				}
			}
			r *= 1000;
			if (i >= n)
				return r;
			if (s[i] != 46)
				return -1;
			i++;
			if (i >= n)
				return -1;
			d = s[i] - 48;
			if (d < 0 || d > 9)
				return -1;
			i++;
			r += 100 * d;
			if (i >= n)
				return r;
			d = s[i] - 48;
			if (d < 0 || d > 9)
				return -1;
			i++;
			r += 10 * d;
			if (i >= n)
				return r;
			d = s[i] - 48;
			if (d < 0 || d > 9)
				return -1;
			i++;
			r += d;
			return r;
		}

		static bool parse_sap_header(ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (!has_string_at(module, 0, "SAP\r\n"))
				return false;
			module_info.fastplay = -1;
			int type = 0;
			int module_index = 5;
			while (module[module_index] != 255) {
				if (module_index + 8 >= module_len)
					return false;
				if (has_string_at(module, module_index, "SONGS ")) {
					module_info.songs = parse_dec(module, module_index + 6, 32);
					if (module_info.songs < 1)
						return false;
				}
				else
					if (has_string_at(module, module_index, "DEFSONG ")) {
						module_info.default_song = parse_dec(module, module_index + 8, 31);
						if (module_info.default_song < 0)
							return false;
					}
					else
						if (has_string_at(module, module_index, "STEREO\r"))
							module_info.channels = 2;
						else
							if (has_string_at(module, module_index, "NTSC\r"))
								module_info.ntsc = true;
							else
								if (has_string_at(module, module_index, "TIME ")) {
								}
								else
									if (has_string_at(module, module_index, "TYPE "))
										type = module[module_index + 5];
									else
										if (has_string_at(module, module_index, "FASTPLAY ")) {
											module_info.fastplay = parse_dec(module, module_index + 9, 312);
											if (module_info.fastplay < 1)
												return false;
										}
										else
											if (has_string_at(module, module_index, "MUSIC ")) {
												module_info.music = parse_hex(module, module_index + 6);
											}
											else
												if (has_string_at(module, module_index, "INIT ")) {
													module_info.init = parse_hex(module, module_index + 5);
												}
												else
													if (has_string_at(module, module_index, "PLAYER ")) {
														module_info.player = parse_hex(module, module_index + 7);
													}
													else
														if (has_string_at(module, module_index, "COVOX ")) {
															module_info.covox_addr = parse_hex(module, module_index + 6);
															if (module_info.covox_addr != 54784)
																return false;
															module_info.channels = 2;
														}
				while (module[module_index++] != 13) {
					if (module_index >= module_len)
						return false;
				}
				if (module[module_index++] != 10)
					return false;
			}
			if (module_info.default_song >= module_info.songs)
				return false;
			switch (type) {
				case 66:
					if (module_info.player < 0 || module_info.init < 0)
						return false;
					module_info.type = 1;
					break;
				case 67:
					if (module_info.player < 0 || module_info.music < 0)
						return false;
					module_info.type = 2;
					break;
				case 68:
					if (module_info.init < 0)
						return false;
					module_info.type = 3;
					break;
				case 83:
					if (module_info.init < 0)
						return false;
					module_info.type = 4;
					module_info.fastplay = 78;
					break;
				default:
					return false;
			}
			if (module_info.fastplay < 0)
				module_info.fastplay = module_info.ntsc ? 262 : 312;
			else
				if (module_info.ntsc && module_info.fastplay > 262)
					return false;
			if (module[module_index + 1] != 255)
				return false;
			module_info.header_len = module_index;
			return true;
		}

		static bool parse_sap(ASAP_State ast, ASAP_ModuleInfo module_info, byte[] module, int module_len) {
			if (!parse_sap_header(module_info, module, module_len))
				return false;
			if (ast == null)
				return true;
			Array_Clear(ast.memory);
			int module_index = module_info.header_len + 2;
			while (module_index + 5 <= module_len) {
				int start_addr = uword(module, module_index);
				int block_len = uword(module, module_index + 2) + 1 - start_addr;
				if (block_len <= 0 || module_index + block_len > module_len)
					return false;
				module_index += 4;
				System.Array.Copy(module, module_index, ast.memory, start_addr, block_len);
				module_index += block_len;
				if (module_index == module_len)
					return true;
				if (module_index + 7 <= module_len && module[module_index] == 255 && module[module_index + 1] == 255)
					module_index += 2;
			}
			return false;
		}

		static int get_packed_ext(string filename) {
			int ext = 0;
			for (int i = filename.Length; --i > 0;) {
				int c = filename[i];
				if (c <= 32 || c > 122)
					return 0;
				if (c == 46)
					return ext | 2105376;
				ext = (ext << 8) + c;
			}
			return 0;
		}

		static bool is_our_ext(int ext) {
			switch (ext) {
				case 7364979:
				case 6516067:
				case 3370339:
				case 7499107:
				case 7564643:
				case 6516068:
				case 7629924:
				case 7630957:
				case 6582381:
				case 7630194:
				case 6516084:
				case 3698036:
				case 3304820:
					return true;
				default:
					return false;
			}
		}

		static bool ASAP_IsOurFile(string filename) {
			return is_our_ext(get_packed_ext(filename));
		}

		static bool ASAP_IsOurExt(string ext) {
			return ext.Length == 3 && is_our_ext(ext[0] + (ext[1] << 8) + (ext[2] << 16) | 2105376);
		}

		static bool parse_file(ASAP_State ast, ASAP_ModuleInfo module_info, string filename, byte[] module, int module_len) {
			int len = filename.Length;
			int basename = 0;
			int ext = -1;
			for (int i = 0; i < len; i++) {
				int c = filename[i];
				if (c == 47 || c == 92) {
					basename = i + 1;
					ext = -1;
				}
				else
					if (c == 46)
						ext = i;
			}
			if (ext < 0)
				return false;
			module_info.channels = 1;
			module_info.songs = 1;
			module_info.default_song = 0;
			for (int i = 0; i < 32; i++) {
				module_info.durations[i] = -1;
				module_info.loops[i] = false;
			}
			module_info.ntsc = false;
			module_info.fastplay = 312;
			module_info.music = -1;
			module_info.init = -1;
			module_info.player = -1;
			module_info.covox_addr = -1;
			switch (get_packed_ext(filename)) {
				case 7364979:
					return parse_sap(ast, module_info, module, module_len);
				case 6516067:
					return parse_cmc(ast, module_info, module, module_len, 5, null);
				case 3370339:
					return parse_cmc(ast, module_info, module, module_len, 6, null);
				case 7499107:
					return parse_cmc(ast, module_info, module, module_len, 7, null);
				case 7564643:
					module_info.channels = 2;
					return parse_cmc(ast, module_info, module, module_len, 8, null);
				case 6516068:
					module_info.fastplay = 156;
					return parse_cmc(ast, module_info, module, module_len, 5, null);
				case 7629924:
					return parse_dlt(ast, module_info, module, module_len);
				case 7630957:
					return parse_mpt(ast, module_info, module, module_len);
				case 6582381:
					module_info.fastplay = 156;
					return parse_mpt(ast, module_info, module, module_len);
				case 7630194:
					return parse_rmt(ast, module_info, module, module_len);
				case 6516084:
				case 3698036:
					return parse_tmc(ast, module_info, module, module_len);
				case 3304820:
					return parse_tm2(ast, module_info, module, module_len);
				default:
					return false;
			}
		}

		static bool ASAP_GetModuleInfo(ASAP_ModuleInfo module_info, string filename, byte[] module, int module_len) {
			return parse_file(null, module_info, filename, module, module_len);
		}

		static bool ASAP_Load(ASAP_State ast, string filename, byte[] module, int module_len) {
			ast.silence_cycles = 0;
			return parse_file(ast, ast.module_info, filename, module, module_len);
		}

		static void ASAP_DetectSilence(ASAP_State ast, int seconds) {
			ast.silence_cycles = seconds * (ast.module_info.ntsc ? 1789772 : 1773447);
		}

		static void call_6502(ASAP_State ast, int addr, int max_scanlines) {
			ast.cpu_pc = addr;
			ast.memory[53770] = 210;
			ast.memory[510] = 9;
			ast.memory[511] = 210;
			ast.cpu_s = 253;
			Cpu_RunScanlines(ast, max_scanlines);
		}

		static void call_6502_init(ASAP_State ast, int addr, int a, int x, int y) {
			ast.cpu_a = a & 255;
			ast.cpu_x = x & 255;
			ast.cpu_y = y & 255;
			call_6502(ast, addr, 15600);
		}

		static void ASAP_MutePokeyChannels(ASAP_State ast, int mask) {
			PokeySound_Mute(ast, ast.base_pokey, mask);
			PokeySound_Mute(ast, ast.extra_pokey, mask >> 4);
		}

		static void ASAP_PlaySong(ASAP_State ast, int song, int duration) {
			ast.current_song = song;
			ast.current_duration = duration;
			ast.blocks_played = 0;
			ast.silence_cycles_counter = ast.silence_cycles;
			ast.extra_pokey_mask = ast.module_info.channels > 1 ? 16 : 0;
			ast.consol = 8;
			ast.nmist = 1;
			ast.covox[0] = 128;
			ast.covox[1] = 128;
			ast.covox[2] = 128;
			ast.covox[3] = 128;
			PokeySound_Initialize(ast);
			ast.cycle = 0;
			ast.cpu_nz = 0;
			ast.cpu_c = 0;
			ast.cpu_vdi = 0;
			ast.scanline_number = 0;
			ast.next_scanline_cycle = 0;
			ast.timer1_cycle = 8388608;
			ast.timer2_cycle = 8388608;
			ast.timer4_cycle = 8388608;
			ast.irqst = 255;
			switch (ast.module_info.type) {
				case 1:
					call_6502_init(ast, ast.module_info.init, song, 0, 0);
					break;
				case 2:
				case 5:
				case 6:
				case 7:
				case 8:
					call_6502_init(ast, ast.module_info.player + 3, 112, ast.module_info.music, ast.module_info.music >> 8);
					call_6502_init(ast, ast.module_info.player + 3, 0, song, 0);
					break;
				case 3:
				case 4:
					ast.cpu_a = song;
					ast.cpu_x = 0;
					ast.cpu_y = 0;
					ast.cpu_s = 255;
					ast.cpu_pc = ast.module_info.init;
					break;
				case 9:
					call_6502_init(ast, ast.module_info.player + 256, 0, 0, ast.module_info.song_pos[song]);
					break;
				case 10:
					call_6502_init(ast, ast.module_info.player, 0, ast.module_info.music >> 8, ast.module_info.music);
					call_6502_init(ast, ast.module_info.player, 2, ast.module_info.song_pos[song], 0);
					break;
				case 11:
					call_6502_init(ast, ast.module_info.player, ast.module_info.song_pos[song], ast.module_info.music, ast.module_info.music >> 8);
					break;
				case 12:
				case 13:
					call_6502_init(ast, ast.module_info.player, 112, ast.module_info.music >> 8, ast.module_info.music);
					call_6502_init(ast, ast.module_info.player, 0, song, 0);
					ast.tmc_per_frame_counter = 1;
					break;
			}
			ASAP_MutePokeyChannels(ast, 0);
		}

		static bool call_6502_player(ASAP_State ast) {
			PokeySound_StartFrame(ast);
			int player = ast.module_info.player;
			switch (ast.module_info.type) {
				case 1:
					call_6502(ast, player, ast.module_info.fastplay);
					break;
				case 2:
				case 5:
				case 6:
				case 7:
				case 8:
					call_6502(ast, player + 6, ast.module_info.fastplay);
					break;
				case 3:
					if (player >= 0) {
						int s = ast.cpu_s;
						ast.memory[256 + s] = (byte) (ast.cpu_pc >> 8);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (ast.cpu_pc & 255);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (((ast.cpu_nz | ast.cpu_nz >> 1) & 128) + ast.cpu_vdi + ((ast.cpu_nz & 255) == 0 ? 2 : 0) + ast.cpu_c + 32);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (ast.cpu_a);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (ast.cpu_x);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (ast.cpu_y);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (209);
						s = s - 1 & 255;
						ast.memory[256 + s] = (byte) (255);
						s = s - 1 & 255;
						ast.cpu_s = s;
						ast.memory[53760] = 104;
						ast.memory[53761] = 168;
						ast.memory[53762] = 104;
						ast.memory[53763] = 170;
						ast.memory[53764] = 104;
						ast.memory[53765] = 64;
						ast.cpu_pc = player;
					}
					Cpu_RunScanlines(ast, ast.module_info.fastplay);
					break;
				case 4:
					Cpu_RunScanlines(ast, ast.module_info.fastplay);
 {
						int i = ast.memory[69] - 1;
						ast.memory[69] = (byte) (i);
						if (i == 0)
							ast.memory[45179] = (byte) (ast.memory[45179] + 1);
					}
					break;
				case 9:
					call_6502(ast, player + 259, ast.module_info.fastplay);
					break;
				case 10:
				case 11:
				case 13:
					call_6502(ast, player + 3, ast.module_info.fastplay);
					break;
				case 12:
					if (--ast.tmc_per_frame_counter <= 0) {
						ast.tmc_per_frame_counter = ast.tmc_per_frame;
						call_6502(ast, player + 3, ast.module_info.fastplay);
					}
					else
						call_6502(ast, player + 6, ast.module_info.fastplay);
					break;
			}
			PokeySound_EndFrame(ast, ast.module_info.fastplay * 114);
			if (ast.silence_cycles > 0) {
				if (PokeySound_IsSilent(ast.base_pokey) && PokeySound_IsSilent(ast.extra_pokey)) {
					ast.silence_cycles_counter -= ast.module_info.fastplay * 114;
					if (ast.silence_cycles_counter <= 0)
						return false;
				}
				else
					ast.silence_cycles_counter = ast.silence_cycles;
			}
			return true;
		}

		static int ASAP_GetPosition(ASAP_State ast) {
			return ast.blocks_played * 10 / 441;
		}

		static int milliseconds_to_blocks(int milliseconds) {
			return milliseconds * 441 / 10;
		}

		static void ASAP_Seek(ASAP_State ast, int position) {
			int block = milliseconds_to_blocks(position);
			if (block < ast.blocks_played)
				ASAP_PlaySong(ast, ast.current_song, ast.current_duration);
			while (ast.blocks_played + ast.samples - ast.sample_index < block) {
				ast.blocks_played += ast.samples - ast.sample_index;
				call_6502_player(ast);
			}
			ast.sample_index += block - ast.blocks_played;
			ast.blocks_played = block;
		}

		static void serialize_int(byte[] buffer, int offset, int value) {
			buffer[offset] = (byte) value;
			buffer[offset + 1] = (byte) (value >> 8);
			buffer[offset + 2] = (byte) (value >> 16);
			buffer[offset + 3] = (byte) (value >> 24);
		}

		static void ASAP_GetWavHeader(ASAP_State ast, byte[] buffer, ASAP_SampleFormat format) {
			int use_16bit = format != ASAP_SampleFormat.U8 ? 1 : 0;
			int block_size = ast.module_info.channels << use_16bit;
			int bytes_per_second = 44100 * block_size;
			int total_blocks = milliseconds_to_blocks(ast.current_duration);
			int n_bytes = (total_blocks - ast.blocks_played) * block_size;
			buffer[0] = 82;
			buffer[1] = 73;
			buffer[2] = 70;
			buffer[3] = 70;
			serialize_int(buffer, 4, n_bytes + 36);
			buffer[8] = 87;
			buffer[9] = 65;
			buffer[10] = 86;
			buffer[11] = 69;
			buffer[12] = 102;
			buffer[13] = 109;
			buffer[14] = 116;
			buffer[15] = 32;
			buffer[16] = 16;
			buffer[17] = 0;
			buffer[18] = 0;
			buffer[19] = 0;
			buffer[20] = 1;
			buffer[21] = 0;
			buffer[22] = (byte) (ast.module_info.channels);
			buffer[23] = 0;
			serialize_int(buffer, 24, 44100);
			serialize_int(buffer, 28, bytes_per_second);
			buffer[32] = (byte) (block_size);
			buffer[33] = 0;
			buffer[34] = (byte) (8 << use_16bit);
			buffer[35] = 0;
			buffer[36] = 100;
			buffer[37] = 97;
			buffer[38] = 116;
			buffer[39] = 97;
			serialize_int(buffer, 40, n_bytes);
		}

		static int ASAP_GenerateAt(ASAP_State ast, byte[] buffer, int buffer_offset, int buffer_len, ASAP_SampleFormat format) {
			if (ast.silence_cycles > 0 && ast.silence_cycles_counter <= 0)
				return 0;
			int block_shift = ast.module_info.channels - 1 + (format != ASAP_SampleFormat.U8 ? 1 : 0);
			int buffer_blocks = buffer_len >> block_shift;
			if (ast.current_duration > 0) {
				int total_blocks = milliseconds_to_blocks(ast.current_duration);
				if (buffer_blocks > total_blocks - ast.blocks_played)
					buffer_blocks = total_blocks - ast.blocks_played;
			}
			int block = 0;
			do {
				int blocks = PokeySound_Generate(ast, buffer, buffer_offset + (block << block_shift), buffer_blocks - block, format);
				ast.blocks_played += blocks;
				block += blocks;
			}
			while (block < buffer_blocks && call_6502_player(ast));
			return block << block_shift;
		}

		static int ASAP_Generate(ASAP_State ast, byte[] buffer, int buffer_len, ASAP_SampleFormat format) {
			return ASAP_GenerateAt(ast, buffer, 0, buffer_len, format);
		}
	}
}
