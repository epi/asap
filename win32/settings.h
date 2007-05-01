#ifndef FREQUENCY
#define FREQUENCY            44100
#endif
#ifndef BITS_PER_SAMPLE
#define BITS_PER_SAMPLE      16
#endif
#ifndef QUALITY
#define QUALITY              1
#endif
#define DEFAULT_SONG_LENGTH  180
// 576 is a magic number for Winamp, better do not modify it
#define BUFFERED_BLOCKS      576

// resource identifiers
#define IDD_SETTINGS   101
#define IDC_STATIC     -1
#define IDC_UNLIMITED  102
#define IDC_LIMITED    103
#define IDC_MINUTES    104
#define IDC_SECONDS    105
#define IDC_LOOPS      106
#define IDC_NOLOOPS    107

// functions
void settingsDialog(HINSTANCE hInstance, HWND hwndParent);
int getSubsongSeconds(const ASAP_ModuleInfo *module_info, int song);
