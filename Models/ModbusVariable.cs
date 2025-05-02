namespace DL6000WebConfig.Models
{
    public class ModbusVariable
    {
        private int _offset;
        public string DeviceName { get; set; } = "";
        public string Name { get; set; } = "";
        public int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                Address = $"40{(_offset + 1).ToString("D3")}";                
            }
        }
        public string Address { get; set; } = "";
        public ushort Value { get; set; }
    }
}
