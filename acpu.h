#define N_FLAG  0x80
#define V_FLAG  0x40
#define D_FLAG  0x08
#define I_FLAG  0x04
#define Z_FLAG  0x02
#define C_FLAG  0x01

typedef struct {
	int cycle;
	int pc;
	int a;
	int x;
	int y;
	int s;
	int nz;
	int c;
	int vdi;
} CpuState;

void Cpu_Run(CpuState *cs, int cycle_limit);
