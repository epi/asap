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
	}
}
