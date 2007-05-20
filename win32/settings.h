// config items
#define BITS_PER_SAMPLE          16
#define DEFAULT_SONG_LENGTH      180
#define DEFAULT_SILENCE_SECONDS  2
// 576 is a magic number for Winamp, better do not modify it
#define BUFFERED_BLOCKS          576
#ifndef FOOBAR2000
extern int song_length;
extern int silence_seconds;
extern BOOL play_loops;
#endif

// resource identifiers
#define IDD_SETTINGS   101
#define IDD_PROGRESS   102
#define IDC_STATIC     -1
#define IDC_UNLIMITED  111
#define IDC_LIMITED    112
#define IDC_MINUTES    113
#define IDC_SECONDS    114
#define IDC_SILENCE    115
#define IDC_SILSECONDS 116
#define IDC_LOOPS      117
#define IDC_NOLOOPS    118
#define IDC_PROGRESS   119

// functions
BOOL settingsDialog(HINSTANCE hInstance, HWND hwndParent);
int getSongDuration(const ASAP_ModuleInfo *module_info, int song);
int playSong(ASAP_State *asap, int song);
