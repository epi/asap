// Generated automatically with "cito". Do not edit.
package net.sf.asap;

final class ASAP6502
{

	static byte[] getPlayerRoutine(ASAPInfo info)
	{
		switch (info.type) {
			case ASAPModuleType.CMC:
				return getBinaryResource("cmc.obx", 2019);
			case ASAPModuleType.CM3:
				return getBinaryResource("cm3.obx", 2022);
			case ASAPModuleType.CMR:
				return getBinaryResource("cmr.obx", 2019);
			case ASAPModuleType.CMS:
				return getBinaryResource("cms.obx", 2753);
			case ASAPModuleType.DLT:
				return getBinaryResource("dlt.obx", 2125);
			case ASAPModuleType.MPT:
				return getBinaryResource("mpt.obx", 2233);
			case ASAPModuleType.RMT:
				return info.channels == 1 ? getBinaryResource("rmt4.obx", 2007) : getBinaryResource("rmt8.obx", 2275);
			case ASAPModuleType.TMC:
				return getBinaryResource("tmc.obx", 2671);
			case ASAPModuleType.TM2:
				return getBinaryResource("tm2.obx", 3698);
			default:
				return null;
		}
	}

	private static byte[] getBinaryResource(String name, int length)
	{
		java.io.DataInputStream dis = new java.io.DataInputStream(ASAP6502.class.getResourceAsStream(name));
		byte[] result = new byte[length];
		try {
			try {
				dis.readFully(result);
			}
			finally {
				dis.close();
			}
		}
		catch (java.io.IOException e) {
			throw new RuntimeException();
		}
		return result;
	}
}
