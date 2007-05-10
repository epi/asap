#ifndef _ASAP_INTERNAL_H_
#define _ASAP_INTERNAL_H_

#ifndef JAVA

#include "asap.h"

#define CONST_LOOKUP         static const
#define AS                   as->

#define dGetByte(x)          (as->memory[x])
#define dPutByte(x, y)       as->memory[x] = y
#define GetByte(addr)        (((addr) & 0xf900) == 0xd000 ? ASAP_GetByte(as, addr) : dGetByte(addr))
#define PutByte(addr, data)  do { if (((addr) & 0xf900) == 0xd000) ASAP_PutByte(as, addr, data); else dPutByte(addr, data); } while (FALSE)

int ASAP_GetByte(ASAP_State *as, int addr);
void ASAP_PutByte(ASAP_State *as, int addr, int data);

void Cpu_Run(ASAP_State *as, int cycle_limit);

void PokeySound_Initialize(ASAP_State *as);
void PokeySound_StartFrame(ASAP_State *as);
void PokeySound_PutByte(ASAP_State *as, int addr, int data, int current_cycle);
int PokeySound_GetRandom(ASAP_State *as, int addr, int current_cycle);
void PokeySound_EndFrame(ASAP_State *as, int current_cycle);
int PokeySound_Generate(ASAP_State *as, void *buffer, int blocks, ASAP_SampleFormat format);

#endif /* JAVA */

#define ASAP_MAIN_CLOCK      1773447

#define V_FLAG               0x40
#define D_FLAG               0x08
#define I_FLAG               0x04
#define Z_FLAG               0x02

#define NEVER                0x800000

#define dGetWord(addr)       (dGetByte(addr) + (dGetByte((addr) + 1) << 8))

#endif /* _ASAP_INTERNAL_H_ */
