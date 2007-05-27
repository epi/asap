#ifndef _ASAP_INTERNAL_H_
#define _ASAP_INTERNAL_H_

#ifndef JAVA

#include "asap.h"

#define CONST_LOOKUP            static const
#define FILE_FUNC               static
#define ASAP_FUNC
#define PTR                     *
#define ADDRESSOF               &
#define VOIDPTR                 void *
#define UBYTE(data)             (data)
#define SBYTE                   signed char
#define STRING                  const char *
#define ZERO_ARRAY(array)       memset(array, 0, sizeof(array))
#define COPY_ARRAY(dest, dest_offset, src, src_offset, len) \
                                memcpy(dest + dest_offset, src + src_offset, len)
#define NEW_ARRAY(type, size)   [size]
#define INIT_BOOL_ARRAY(array)  memset(array, FALSE, sizeof(array))

#define AS                      as->
#define PS                      ps->
#define MODULE_INFO             module_info->
#define ASAP_Player             const byte *
#define PLAYER_OBX(name)        name##_obx

int ASAP_GetByte(ASAP_State *as, int addr);
void ASAP_PutByte(ASAP_State *as, int addr, int data);

void Cpu_RunScanlines(ASAP_State *as, int scanlines);

void PokeySound_Initialize(ASAP_State *as);
void PokeySound_StartFrame(ASAP_State *as);
void PokeySound_PutByte(ASAP_State *as, int addr, int data);
int PokeySound_GetRandom(ASAP_State *as, int addr);
void PokeySound_EndFrame(ASAP_State *as, int cycle_limit);
int PokeySound_Generate(ASAP_State *as, byte buffer[], int buffer_offset, int blocks, ASAP_SampleFormat format);
abool PokeySound_IsSilent(const PokeyState *ps);
void PokeySound_Mute(const ASAP_State *as, PokeyState *ps, int mask);

#endif /* JAVA */

#define ASAP_MAIN_CLOCK         1773447

#define V_FLAG                  0x40
#define D_FLAG                  0x08
#define I_FLAG                  0x04
#define Z_FLAG                  0x02

#define NEVER                   0x800000

#define dGetByte(addr)          UBYTE(AS memory[addr])
#define dPutByte(addr, data)    AS memory[addr] = (byte) (data)
#define dGetWord(addr)          (dGetByte(addr) + (dGetByte((addr) + 1) << 8))
#define GetByte(addr)           (((addr) & 0xf900) == 0xd000 ? ASAP_GetByte(as, addr) : dGetByte(addr))
#define PutByte(addr, data)     do { if (((addr) & 0xf900) == 0xd000) ASAP_PutByte(as, addr, data); else dPutByte(addr, data); } while (FALSE)
#define RMW_GetByte(dest, addr) do { if (((addr) & 0xf900) == 0xd000) { dest = ASAP_GetByte(as, addr); AS cycle--; ASAP_PutByte(as, addr, dest); AS cycle++; } else dest = dGetByte(addr); } while (FALSE)

#endif /* _ASAP_INTERNAL_H_ */
