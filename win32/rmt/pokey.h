// Generated automatically with "cito". Do not edit.
#include <stdbool.h>

typedef enum {
	ASAPSampleFormat_U8,
	ASAPSampleFormat_S16_L_E,
	ASAPSampleFormat_S16_B_E
}
ASAPSampleFormat;
typedef struct PokeyPair PokeyPair;
PokeyPair *PokeyPair_New(void);
void PokeyPair_Delete(PokeyPair *self);
int PokeyPair_EndFrame(PokeyPair *self, int cycle);
int PokeyPair_Generate(PokeyPair *self, unsigned char *buffer, int bufferOffset, int blocks, ASAPSampleFormat format);
int PokeyPair_GetRandom(PokeyPair const *self, int addr, int cycle);
void PokeyPair_Initialize(PokeyPair *self, int mainClock, bool stereo);
void PokeyPair_Poke(PokeyPair *self, int addr, int data, int cycle);
void PokeyPair_StartFrame(PokeyPair *self);
