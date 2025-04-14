using System.Xml.Linq;
using DL6000WebConfig.Models;

namespace DL6000WebConfig.Services
{
    public class ConfigService
    {
        private readonly string _path;
        private readonly XDocument _xml;

        public ConfigService(string path)
        {
            _path = Path.GetFullPath(path);
            _xml = XDocument.Load(_path);
        }

        public List<DeviceConfigModel> GetDevices()
        {
            var appSettings = _xml.Root?
                .Element("appSettings")?
                .Elements("add")
                .ToList();

            var devices = new List<DeviceConfigModel>();

            if (appSettings == null) return devices;

            // identificar os equipamentos pelo sufixo: _DL_6000_1, _DL_6000_2...
            var groups = appSettings
                .Select(e => e.Attribute("key")?.Value)
                .Where(k => k != null && k.Contains("_DL_6000_"))
                .Select(k => k!.Split("_DL_6000_").Last())
                .Distinct();

            foreach (var suffix in groups)
            {
                var get = (string key) =>
                    appSettings.FirstOrDefault(e => e.Attribute("key")?.Value == key + "_DL_6000_" + suffix)?
                    .Attribute("value")?.Value ?? "";

                devices.Add(new DeviceConfigModel
                {
                    Name = "DL6000_" + suffix,
                    Ip = get("ip"),
                    Port = get("port"),
                    UnitId1 = get("unitId1"),
                    UnitId2 = get("unitId2"),
                    StartIndexDL1 = get("StartIndexDL1"),
                    StartIndexDL2 = get("StartIndexDL2"),
                    Cycle = get("cycle"),
                    TimeoutSend = get("timeOutSend"),
                    TimeoutReceive = get("timeOutReceive")
                });
            }

            return devices;
        }
        public List<ModbusVariable> GetDefaultVariables()
        {
            var list = new List<ModbusVariable>();

            // Exemplo estático só com DL6000_1 por enquanto:
            list.Add(new ModbusVariable
            {
                DeviceName = "DL6000_1",
                Name = "Vazão",
                FunctionCode = "19",
                Offset = 4,
                Address = $"4000{4 + 1}",
                Value = MosbusSlaveTcpWrapper.GetValue(4)
            });

            list.Add(new ModbusVariable
            {
                DeviceName = "DL6000_1",
                Name = "Temperatura",
                FunctionCode = "1A",
                Offset = 5,
                Address = $"4000{5 + 1}",
                Value = MosbusSlaveTcpWrapper.GetValue(5)
            });

            return list;
        }

        //retorna uma lista simples
        public List<ModbusVariable> GetConfiguredVariables()
        {
            var devices = GetDevices(); // já existe
            var result = new List<ModbusVariable>();

            foreach (var d in devices)
            {
                if (int.TryParse(d.StartIndexDL1, out int dl1) && dl1 > 0)
                {
                    result.Add(new ModbusVariable
                    {
                        DeviceName = d.Name,
                        Name = "Função 19 (DL1)",
                        FunctionCode = "19",
                        Offset = dl1
                    });
                }

                if (int.TryParse(d.StartIndexDL2, out int dl2) && dl2 > 0)
                {
                    result.Add(new ModbusVariable
                    {
                        DeviceName = d.Name,
                        Name = "Função 19 (DL2)",
                        FunctionCode = "19",
                        Offset = dl2
                    });
                }
            }

            return result;
}


    }

    // public class DeviceConfigModel
    // {
    //     public string Name { get; set; } = "";
    //     public string Ip { get; set; } = "";
    //     public string Port { get; set; } = "502";
    //     public string UnitId1 { get; set; } = "1";
    //     public string UnitId2 { get; set; } = "2";
    //     public string StartIndexDL1 { get; set; } = "2";
    //     public string StartIndexDL2 { get; set; } = "32";
    //     public string Cycle { get; set; } = "1000";
    //     public string TimeoutSend { get; set; } = "4000";
    //     public string TimeoutReceive { get; set; } = "4000";
    // }
}
