/* Generated automatically with "cito". Do not edit. */
typedef int cibool;
#ifndef TRUE
#define TRUE 1
#endif
#ifndef FALSE
#define FALSE 0
#endif
typedef struct ASAP ASAP;
typedef struct ASAPInfo ASAPInfo;

typedef enum {
	ASAPSampleFormat_U8,
	ASAPSampleFormat_S16_L_E,
	ASAPSampleFormat_S16_B_E
}
ASAPSampleFormat;
ASAP *ASAP_New(void);
void ASAP_Delete(ASAP *self);
void ASAP_DetectSilence(ASAP *self, int seconds);
int ASAP_Generate(ASAP *self, unsigned char *buffer, int bufferLen, ASAPSampleFormat format);
int ASAP_GetBlocksPlayed(ASAP const *self);
ASAPInfo const *ASAP_GetInfo(ASAP const *self);
int ASAP_GetPokeyChannelVolume(ASAP const *self, int channel);
int ASAP_GetPosition(ASAP const *self);
void ASAP_GetWavHeader(ASAP const *self, unsigned char *buffer, ASAPSampleFormat format);
cibool ASAP_Load(ASAP *self, const char *filename, unsigned char const *module, int moduleLen);
void ASAP_MutePokeyChannels(ASAP *self, int mask);
cibool ASAP_PlaySong(ASAP *self, int song, int duration);
#define ASAP_SAMPLE_RATE  44100
cibool ASAP_Seek(ASAP *self, int position);
#define ASAP_WAV_HEADER_LENGTH  44
ASAPInfo *ASAPInfo_New(void);
void ASAPInfo_Delete(ASAPInfo *self);
const char *ASAPInfo_GetAuthor(ASAPInfo const *self);
int ASAPInfo_GetChannels(ASAPInfo const *self);
const char *ASAPInfo_GetDate(ASAPInfo const *self);
int ASAPInfo_GetDefaultSong(ASAPInfo const *self);
int ASAPInfo_GetDuration(ASAPInfo const *self, int song);
cibool ASAPInfo_GetLoop(ASAPInfo const *self, int song);
int ASAPInfo_GetSongs(ASAPInfo const *self);
const char *ASAPInfo_GetTitle(ASAPInfo const *self);
const char *ASAPInfo_GetTitleOrFilename(ASAPInfo const *self);
cibool ASAPInfo_IsOurExt(const char *ext);
cibool ASAPInfo_IsOurFile(const char *filename);
cibool ASAPInfo_Load(ASAPInfo *self, const char *filename, unsigned char const *module, int moduleLen);
#define ASAPInfo_MAX_MODULE_LENGTH  65000
#define ASAPInfo_MAX_SONGS  32
int ASAPInfo_ParseDuration(const char *s);
#define ASAPInfo_VERSION  "3.0.0"
#define ASAPInfo_VERSION_MAJOR  3
#define ASAPInfo_VERSION_MICRO  0
#define ASAPInfo_VERSION_MINOR  0
