using Modbus.Data;

namespace DL6000WebConfig
{
    public static class MosbusSlaveTcpWrapper
    {
        public static DataStore? DataStore { get; set; }

        public static ushort GetValue(int offset)
        {
            if (DataStore?.HoldingRegisters == null || offset < 0 || offset >= DataStore.HoldingRegisters.Count)
                return 0;

            return DataStore.HoldingRegisters[offset];
        }
    }
}
