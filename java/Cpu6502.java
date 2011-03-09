// Generated automatically with "cito". Do not edit.
package net.sf.asap;

final class Cpu6502
{
	int a;
	int c;

	/**
	 * Runs 6502 emulation for the specified number of Atari scanlines.
	 * Each scanline is 114 cycles of which 9 is taken by ANTIC for memory refresh.
	 */
	void doFrame(ASAP asap, int cycleLimit)
	{
		int pc = this.pc;
		int nz = this.nz;
		int a = this.a;
		int x = this.x;
		int y = this.y;
		int c = this.c;
		int s = this.s;
		int vdi = this.vdi;
		while (asap.cycle < cycleLimit) {
			if (asap.cycle >= asap.nextEventCycle) {
				this.pc = pc;
				this.s = s;
				asap.handleEvent();
				pc = this.pc;
				s = this.s;
				if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
					asap.memory[256 + s] = (byte) (pc >> 8);
					s = s - 1 & 255;
					asap.memory[256 + s] = (byte) pc;
					s = s - 1 & 255;
					asap.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
					s = s - 1 & 255;
					vdi |= 4;
					pc = (asap.memory[65534] & 0xff) + ((asap.memory[65535] & 0xff) << 8);
					asap.cycle += 7;
				}
			}
			int data = asap.memory[pc++] & 0xff;
			asap.cycle += CI_CONST_ARRAY_1[data];
			int addr;
			switch (data) {
				case 0:
					pc++;
					asap.memory[256 + s] = (byte) (pc >> 8);
					s = s - 1 & 255;
					asap.memory[256 + s] = (byte) pc;
					s = s - 1 & 255;
					asap.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48);
					s = s - 1 & 255;
					vdi |= 4;
					pc = (asap.memory[65534] & 0xff) + ((asap.memory[65535] & 0xff) << 8);
					break;
				case 1:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
					pc--;
					asap.cycle = asap.nextEventCycle;
					break;
				case 5:
					addr = asap.memory[pc++] & 0xff;
					nz = a |= asap.memory[addr] & 0xff;
					break;
				case 6:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 8:
					asap.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 48);
					s = s - 1 & 255;
					break;
				case 9:
					nz = a |= asap.memory[pc++] & 0xff;
					break;
				case 10:
					c = a >> 7;
					nz = a = a << 1 & 255;
					break;
				case 13:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 14:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 16:
					if (nz < 128) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 17:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 21:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = a |= asap.memory[addr] & 0xff;
					break;
				case 22:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 24:
					c = 0;
					break;
				case 25:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 29:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					nz = a |= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 30:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 32:
					addr = asap.memory[pc++] & 0xff;
					asap.memory[256 + s] = (byte) (pc >> 8);
					s = s - 1 & 255;
					asap.memory[256 + s] = (byte) pc;
					s = s - 1 & 255;
					pc = addr + ((asap.memory[pc] & 0xff) << 8);
					break;
				case 33:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 36:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					vdi = (vdi & 12) + (nz & 64);
					nz = ((nz & 128) << 1) + (nz & a);
					break;
				case 37:
					addr = asap.memory[pc++] & 0xff;
					nz = a &= asap.memory[addr] & 0xff;
					break;
				case 38:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 40:
					s = s + 1 & 255;
					vdi = asap.memory[256 + s] & 0xff;
					nz = ((vdi & 128) << 1) + (~vdi & 2);
					c = vdi & 1;
					vdi &= 76;
					if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
						asap.memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						asap.memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						asap.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
						s = s - 1 & 255;
						vdi |= 4;
						pc = (asap.memory[65534] & 0xff) + ((asap.memory[65535] & 0xff) << 8);
						asap.cycle += 7;
					}
					break;
				case 41:
					nz = a &= asap.memory[pc++] & 0xff;
					break;
				case 42:
					a = (a << 1) + c;
					c = a >> 8;
					nz = a &= 255;
					break;
				case 44:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					vdi = (vdi & 12) + (nz & 64);
					nz = ((nz & 128) << 1) + (nz & a);
					break;
				case 45:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 46:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 48:
					if (nz >= 128) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 49:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 53:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = a &= asap.memory[addr] & 0xff;
					break;
				case 54:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 56:
					c = 1;
					break;
				case 57:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 61:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					nz = a &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 62:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 64:
					s = s + 1 & 255;
					vdi = asap.memory[256 + s] & 0xff;
					nz = ((vdi & 128) << 1) + (~vdi & 2);
					c = vdi & 1;
					vdi &= 76;
					s = s + 1 & 255;
					pc = asap.memory[256 + s] & 0xff;
					s = s + 1 & 255;
					addr = asap.memory[256 + s] & 0xff;
					pc += addr << 8;
					if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
						asap.memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						asap.memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						asap.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
						s = s - 1 & 255;
						vdi |= 4;
						pc = (asap.memory[65534] & 0xff) + ((asap.memory[65535] & 0xff) << 8);
						asap.cycle += 7;
					}
					break;
				case 65:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 69:
					addr = asap.memory[pc++] & 0xff;
					nz = a ^= asap.memory[addr] & 0xff;
					break;
				case 70:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
					break;
				case 72:
					asap.memory[256 + s] = (byte) a;
					s = s - 1 & 255;
					break;
				case 73:
					nz = a ^= asap.memory[pc++] & 0xff;
					break;
				case 74:
					c = a & 1;
					nz = a >>= 1;
					break;
				case 76:
					addr = asap.memory[pc++] & 0xff;
					pc = addr + ((asap.memory[pc] & 0xff) << 8);
					break;
				case 77:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 78:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 80:
					if ((vdi & 64) == 0) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 81:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 85:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = a ^= asap.memory[addr] & 0xff;
					break;
				case 86:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
					break;
				case 88:
					vdi &= 72;
					if ((vdi & 4) == 0 && asap.pokeys.irqst != 255) {
						asap.memory[256 + s] = (byte) (pc >> 8);
						s = s - 1 & 255;
						asap.memory[256 + s] = (byte) pc;
						s = s - 1 & 255;
						asap.memory[256 + s] = (byte) (((nz | nz >> 1) & 128) + vdi + ((nz & 255) == 0 ? 2 : 0) + c + 32);
						s = s - 1 & 255;
						vdi |= 4;
						pc = (asap.memory[65534] & 0xff) + ((asap.memory[65535] & 0xff) << 8);
						asap.cycle += 7;
					}
					break;
				case 89:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 93:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					nz = a ^= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 94:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 96:
					s = s + 1 & 255;
					pc = asap.memory[256 + s] & 0xff;
					s = s + 1 & 255;
					addr = asap.memory[256 + s] & 0xff;
					pc += (addr << 8) + 1;
					break;
				case 97:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					data = asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					nz = (asap.memory[addr] & 0xff) + (c << 8);
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
					break;
				case 104:
					s = s + 1 & 255;
					a = asap.memory[256 + s] & 0xff;
					nz = a;
					break;
				case 105:
					data = asap.memory[pc++] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if ((addr & 255) == 255)
						pc = (asap.memory[addr] & 0xff) + ((asap.memory[addr - 255] & 0xff) << 8);
					else
						pc = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1] & 0xff) << 8);
					break;
				case 109:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 112:
					if ((vdi & 64) != 0) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 113:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					data = asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = (asap.memory[addr] & 0xff) + (c << 8);
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
					break;
				case 120:
					vdi |= 4;
					break;
				case 121:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 129:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, a);
					else
						asap.memory[addr] = (byte) a;
					break;
				case 132:
					addr = asap.memory[pc++] & 0xff;
					asap.memory[addr] = (byte) y;
					break;
				case 133:
					addr = asap.memory[pc++] & 0xff;
					asap.memory[addr] = (byte) a;
					break;
				case 134:
					addr = asap.memory[pc++] & 0xff;
					asap.memory[addr] = (byte) x;
					break;
				case 136:
					nz = y = y - 1 & 255;
					break;
				case 138:
					nz = a = x;
					break;
				case 140:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, y);
					else
						asap.memory[addr] = (byte) y;
					break;
				case 141:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, a);
					else
						asap.memory[addr] = (byte) a;
					break;
				case 142:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, x);
					else
						asap.memory[addr] = (byte) x;
					break;
				case 144:
					if (c == 0) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 145:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, a);
					else
						asap.memory[addr] = (byte) a;
					break;
				case 148:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					asap.memory[addr] = (byte) y;
					break;
				case 149:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					asap.memory[addr] = (byte) a;
					break;
				case 150:
					addr = (asap.memory[pc++] & 0xff) + y & 255;
					asap.memory[addr] = (byte) x;
					break;
				case 152:
					nz = a = y;
					break;
				case 153:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, a);
					else
						asap.memory[addr] = (byte) a;
					break;
				case 154:
					s = x;
					break;
				case 157:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, a);
					else
						asap.memory[addr] = (byte) a;
					break;
				case 160:
					nz = y = asap.memory[pc++] & 0xff;
					break;
				case 161:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 162:
					nz = x = asap.memory[pc++] & 0xff;
					break;
				case 164:
					addr = asap.memory[pc++] & 0xff;
					nz = y = asap.memory[addr] & 0xff;
					break;
				case 165:
					addr = asap.memory[pc++] & 0xff;
					nz = a = asap.memory[addr] & 0xff;
					break;
				case 166:
					addr = asap.memory[pc++] & 0xff;
					nz = x = asap.memory[addr] & 0xff;
					break;
				case 168:
					nz = y = a;
					break;
				case 169:
					nz = a = asap.memory[pc++] & 0xff;
					break;
				case 170:
					nz = x = a;
					break;
				case 172:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = y = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 173:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 174:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = x = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 176:
					if (c != 0) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 177:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 180:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = y = asap.memory[addr] & 0xff;
					break;
				case 181:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = a = asap.memory[addr] & 0xff;
					break;
				case 182:
					addr = (asap.memory[pc++] & 0xff) + y & 255;
					nz = x = asap.memory[addr] & 0xff;
					break;
				case 184:
					vdi &= 12;
					break;
				case 185:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 186:
					nz = x = s;
					break;
				case 188:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					nz = y = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 189:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					nz = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 190:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = x = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 192:
					nz = asap.memory[pc++] & 0xff;
					c = y >= nz ? 1 : 0;
					nz = y - nz & 255;
					break;
				case 193:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 196:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = y >= nz ? 1 : 0;
					nz = y - nz & 255;
					break;
				case 197:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 198:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 200:
					nz = y = y + 1 & 255;
					break;
				case 201:
					nz = asap.memory[pc++] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 202:
					nz = x = x - 1 & 255;
					break;
				case 204:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = y >= nz ? 1 : 0;
					nz = y - nz & 255;
					break;
				case 205:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 206:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 208:
					if ((nz & 255) != 0) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 209:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 213:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 214:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 216:
					vdi &= 68;
					break;
				case 217:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 221:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 222:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 224:
					nz = asap.memory[pc++] & 0xff;
					c = x >= nz ? 1 : 0;
					nz = x - nz & 255;
					break;
				case 225:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = x >= nz ? 1 : 0;
					nz = x - nz & 255;
					break;
				case 229:
					addr = asap.memory[pc++] & 0xff;
					data = asap.memory[addr] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 232:
					nz = x = x + 1 & 255;
					break;
				case 233:
				case 235:
					data = asap.memory[pc++] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					c = x >= nz ? 1 : 0;
					nz = x - nz & 255;
					break;
				case 237:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 240:
					if ((nz & 255) == 0) {
						addr = (byte) asap.memory[pc];
						pc++;
						addr += pc;
						if ((addr ^ pc) >> 8 != 0)
							asap.cycle++;
						asap.cycle++;
						pc = addr;
						break;
					}
					pc++;
					break;
				case 241:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					data = asap.memory[addr] & 0xff;
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					asap.memory[addr] = (byte) nz;
					break;
				case 248:
					vdi |= 8;
					break;
				case 249:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if ((addr & 255) < x)
						asap.cycle++;
					data = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					break;
				case 3:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					asap.memory[addr] = (byte) nz;
					nz = a |= nz;
					break;
				case 11:
				case 43:
					nz = a &= asap.memory[pc++] & 0xff;
					c = nz >> 7;
					break;
				case 12:
					pc += 2;
					break;
				case 15:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a |= nz;
					break;
				case 19:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a |= nz;
					break;
				case 23:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					asap.memory[addr] = (byte) nz;
					nz = a |= nz;
					break;
				case 27:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a |= nz;
					break;
				case 28:
				case 60:
				case 92:
				case 124:
				case 220:
				case 252:
					if ((asap.memory[pc++] & 0xff) + x >= 256)
						asap.cycle++;
					pc++;
					break;
				case 31:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz >> 7;
					nz = nz << 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a |= nz;
					break;
				case 35:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 39:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 47:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 51:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 55:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 59:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 63:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = (nz << 1) + c;
					c = nz >> 8;
					nz &= 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a &= nz;
					break;
				case 67:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 71:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 75:
					a &= asap.memory[pc++] & 0xff;
					c = a & 1;
					nz = a >>= 1;
					break;
				case 79:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 83:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 87:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 91:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 95:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					nz = a ^= nz;
					break;
				case 99:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					nz = (asap.memory[addr] & 0xff) + (c << 8);
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					data = a & asap.memory[pc++] & 0xff;
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
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = (asap.memory[addr] & 0xff) + (c << 8);
					c = nz & 1;
					nz >>= 1;
					asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz += c << 8;
					c = nz & 1;
					nz >>= 1;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
							if (al >= 10) {
								tmp += al < 26 ? 6 : -10;
								if (nz != 0)
									nz = (tmp & 128) + 1;
							}
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					data = a & x;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = (byte) data;
					break;
				case 135:
					addr = asap.memory[pc++] & 0xff;
					data = a & x;
					asap.memory[addr] = (byte) data;
					break;
				case 139:
					data = asap.memory[pc++] & 0xff;
					a &= (data | 239) & x;
					nz = a & data;
					break;
				case 143:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					data = a & x;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, data);
					else
						asap.memory[addr] = (byte) data;
					break;
				case 147:
					{
						addr = asap.memory[pc++] & 0xff;
						int hi = asap.memory[addr + 1 & 255] & 0xff;
						addr = asap.memory[addr] & 0xff;
						data = hi + 1 & a & x;
						addr += y;
						if (addr >= 256)
							hi = data - 1;
						addr += hi << 8;
						if ((addr & 63744) == 53248)
							asap.pokeHardware(addr, data);
						else
							asap.memory[addr] = (byte) data;
					}
					break;
				case 151:
					addr = (asap.memory[pc++] & 0xff) + y & 255;
					data = a & x;
					asap.memory[addr] = (byte) data;
					break;
				case 155:
					s = a & x;
					{
						addr = asap.memory[pc++] & 0xff;
						int hi = asap.memory[pc++] & 0xff;
						data = hi + 1 & s;
						addr += y;
						if (addr >= 256)
							hi = data - 1;
						addr += hi << 8;
						if ((addr & 63744) == 53248)
							asap.pokeHardware(addr, data);
						else
							asap.memory[addr] = (byte) data;
					}
					break;
				case 156:
					{
						addr = asap.memory[pc++] & 0xff;
						int hi = asap.memory[pc++] & 0xff;
						data = hi + 1 & y;
						addr += x;
						if (addr >= 256)
							hi = data - 1;
						addr += hi << 8;
						if ((addr & 63744) == 53248)
							asap.pokeHardware(addr, data);
						else
							asap.memory[addr] = (byte) data;
					}
					break;
				case 158:
					{
						addr = asap.memory[pc++] & 0xff;
						int hi = asap.memory[pc++] & 0xff;
						data = hi + 1 & x;
						addr += y;
						if (addr >= 256)
							hi = data - 1;
						addr += hi << 8;
						if ((addr & 63744) == 53248)
							asap.pokeHardware(addr, data);
						else
							asap.memory[addr] = (byte) data;
					}
					break;
				case 159:
					{
						addr = asap.memory[pc++] & 0xff;
						int hi = asap.memory[pc++] & 0xff;
						data = hi + 1 & a & x;
						addr += y;
						if (addr >= 256)
							hi = data - 1;
						addr += hi << 8;
						if ((addr & 63744) == 53248)
							asap.pokeHardware(addr, data);
						else
							asap.memory[addr] = (byte) data;
					}
					break;
				case 163:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 167:
					addr = asap.memory[pc++] & 0xff;
					nz = x = a = asap.memory[addr] & 0xff;
					break;
				case 171:
					nz = x = a &= asap.memory[pc++] & 0xff;
					break;
				case 175:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 179:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 183:
					addr = (asap.memory[pc++] & 0xff) + y & 255;
					nz = x = a = asap.memory[addr] & 0xff;
					break;
				case 187:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = x = a = s &= (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 191:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if ((addr & 255) < y)
						asap.cycle++;
					nz = x = a = (addr & 63744) == 53248 ? asap.peekHardware(addr) : asap.memory[addr] & 0xff;
					break;
				case 195:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 199:
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 203:
					nz = asap.memory[pc++] & 0xff;
					x &= a;
					c = x >= nz ? 1 : 0;
					nz = x = x - nz & 255;
					break;
				case 207:
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 211:
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 215:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 219:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 223:
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz - 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
					c = a >= nz ? 1 : 0;
					nz = a - nz & 255;
					break;
				case 227:
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8);
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
					addr = asap.memory[pc++] & 0xff;
					nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					asap.memory[addr] = (byte) nz;
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
					addr = asap.memory[pc++] & 0xff;
					addr += (asap.memory[pc++] & 0xff) << 8;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
					addr = asap.memory[pc++] & 0xff;
					addr = (asap.memory[addr] & 0xff) + ((asap.memory[addr + 1 & 255] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
					addr = (asap.memory[pc++] & 0xff) + x & 255;
					nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					asap.memory[addr] = (byte) nz;
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + y & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
					addr = asap.memory[pc++] & 0xff;
					addr = addr + ((asap.memory[pc++] & 0xff) << 8) + x & 65535;
					if (addr >> 8 == 210) {
						asap.cycle--;
						nz = asap.peekHardware(addr);
						asap.pokeHardware(addr, nz);
						asap.cycle++;
					}
					else
						nz = asap.memory[addr] & 0xff;
					nz = nz + 1 & 255;
					if ((addr & 63744) == 53248)
						asap.pokeHardware(addr, nz);
					else
						asap.memory[addr] = (byte) nz;
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
		this.pc = pc;
		this.nz = nz;
		this.a = a;
		this.x = x;
		this.y = y;
		this.c = c;
		this.s = s;
		this.vdi = vdi;
	}
	int nz;
	int pc;
	int s;
	int vdi;
	int x;
	int y;
	private static final int[] CI_CONST_ARRAY_1 = { 7, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 4, 4, 6, 6,
		2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
		6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 4, 4, 6, 6,
		2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
		6, 6, 2, 8, 3, 3, 5, 5, 3, 2, 2, 2, 3, 4, 6, 6,
		2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
		6, 6, 2, 8, 3, 3, 5, 5, 4, 2, 2, 2, 5, 4, 6, 6,
		2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
		2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
		2, 6, 2, 6, 4, 4, 4, 4, 2, 5, 2, 5, 5, 5, 5, 5,
		2, 6, 2, 6, 3, 3, 3, 3, 2, 2, 2, 2, 4, 4, 4, 4,
		2, 5, 2, 5, 4, 4, 4, 4, 2, 4, 2, 4, 4, 4, 4, 4,
		2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
		2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7,
		2, 6, 2, 8, 3, 3, 5, 5, 2, 2, 2, 2, 4, 4, 6, 6,
		2, 5, 2, 8, 4, 4, 6, 6, 2, 4, 2, 7, 4, 4, 7, 7 };
}
