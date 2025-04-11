namespace DL6000WebConfig.Models
{
    public class DeviceConfigModel
    {
        public string Name { get; set; } = "";
        public string Ip { get; set; } = "";
        public string Port { get; set; } = "502";
        public string UnitId1 { get; set; } = "1";
        public string UnitId2 { get; set; } = "2";
        public string StartIndexDL1 { get; set; } = "2";
        public string StartIndexDL2 { get; set; } = "32";
        public string Cycle { get; set; } = "1000";
        public string TimeoutSend { get; set; } = "4000";
        public string TimeoutReceive { get; set; } = "4000";
    }
}
