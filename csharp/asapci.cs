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
				reg = (reg >> 5 ^ (reg & 255) << 9) + (reg >> 8);
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
	}
}
