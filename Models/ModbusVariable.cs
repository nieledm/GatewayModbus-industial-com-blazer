namespace DL6000WebConfig.Models
{
    public class ModbusVariable
    {
        public string Name { get; set; } = "";
        public string FunctionCode { get; set; } = "";
        public int Offset { get; set; } // posição na memória
        public string Address => (40001 + Offset).ToString(); // exibição
        public ushort Value { get; set; } // valor atual
    }
}
