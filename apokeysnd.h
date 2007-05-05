void PokeySound_Initialize(int stereo);
void PokeySound_PutByte(int addr, int data, int current_cycle);
int PokeySound_GetRandom(int addr, int current_cycle);
void PokeySound_Flush(int current_cycle);

extern unsigned char atari_sound[2048];
extern int atari_sound_len;
