namespace DL6000WebConfig.Models
{
    public class ModbusVariable
    {
        public string DeviceName { get; set; } = "";
        public string Name { get; set; } = "";
        public string FunctionCode { get; set; } = "";
        public int Offset { get; set; } // posição na memória
        public string Address { get; set; } = ""; // exibição
        public ushort Value { get; set; } // valor atual
    }
}
