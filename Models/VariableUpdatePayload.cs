namespace DL6000WebConfig.Models
{
    public class VariableUpdatePayload
    {
        public ModbusVariable Original { get; set; } = new ModbusVariable();
        public ModbusVariable Updated { get; set; } = new ModbusVariable();
    }
}