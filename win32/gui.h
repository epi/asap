#define BITS_PER_SAMPLE          16
#define IDC_STATIC     -1
#define IDD_INFO       300
#define IDC_AUTHOR     301
#define IDC_NAME       302
#define IDC_DATE       303

void showInfoDialog(HINSTANCE hInstance, HWND hwndParent, const ASAP_ModuleInfo *module_info);
void updateInfoDialog(const ASAP_ModuleInfo *module_info);

#ifdef WASAP

#define IDI_APP        101
#define IDI_STOP       102
#define IDI_PLAY       103
#define IDR_TRAYMENU   200
#define IDM_OPEN       201
#define IDM_STOP       202
#define IDM_FILE_INFO  203
#define IDM_ABOUT      204
#define IDM_EXIT       205
#define IDM_SONG1      211

#else /* WASAP */

/* config items */
#define DEFAULT_SONG_LENGTH      180
#define DEFAULT_SILENCE_SECONDS  2
/* 576 is a magic number for Winamp, better do not modify it */
#define BUFFERED_BLOCKS          576

#ifndef FOOBAR2000
extern ASAP_State asap;
extern int song_length;
extern int silence_seconds;
extern BOOL play_loops;
extern int mute_mask;
#endif

/* resource identifiers */
#define IDD_SETTINGS   400
#define IDC_UNLIMITED  401
#define IDC_LIMITED    402
#define IDC_MINUTES    403
#define IDC_SECONDS    404
#define IDC_SILENCE    405
#define IDC_SILSECONDS 406
#define IDC_LOOPS      407
#define IDC_NOLOOPS    408
#define IDC_MUTE1      411
#define IDD_PROGRESS   500
#define IDC_PROGRESS   501

/* functions */
BOOL settingsDialog(HINSTANCE hInstance, HWND hwndParent);
int getSongDuration(const ASAP_ModuleInfo *module_info, int song);
int playSong(int song);

#endif /* WASAP */