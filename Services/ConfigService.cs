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
        

        //Mapeando os endereços do arquivo DL6000_TO_MODBUS_SLAVE.exe.config
        public List<ModbusVariable> GetConfiguredVariables()
        {
            var appSettings = _xml.Root?
                .Element("appSettings")?
                .Elements("add")
                .ToList();

            var result = new List<ModbusVariable>();
            if (appSettings == null) return result;

            // Dicionário de aliases (pode ser expandido)
            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "DL1", "Vazão" },
                { "DL2", "Temperatura" },
                { "Pressao", "Pressão" },
                { "Nivel", "Nível" },
                { "TempAmb", "Temperatura Ambiente" },
            };

            // Descobre os sufixos únicos (ex: 1, 2, 3)
            var suffixes = appSettings
                .Select(x => x.Attribute("key")?.Value)
                .Where(k => k != null && k.Contains("_DL_6000_"))
                .Select(k => k!.Split("_DL_6000_").Last())
                .Distinct();

            foreach (var suffix in suffixes)
            {
                string deviceName = "DL6000_" + suffix;

                // Pega todas as chaves desse equipamento
                var keysForDevice = appSettings
                    .Where(e => e.Attribute("key")?.Value?.EndsWith($"_DL_6000_{suffix}") == true)
                    .ToList();

                foreach (var entry in keysForDevice)
                {
                    var fullKey = entry.Attribute("key")?.Value ?? "";
                    var value = entry.Attribute("value")?.Value ?? "";

                    if (string.IsNullOrEmpty(fullKey) || !fullKey.StartsWith("StartIndex")) continue;

                    // Pega o tipo da variável, ex: DL1, DL2, Pressao...
                    string rawType = fullKey.Replace("StartIndex", "").Split("_DL_6000_")[0];
                    string name = aliases.ContainsKey(rawType) ? aliases[rawType] : rawType;

                    if (int.TryParse(value, out int offset))
                    {
                        result.Add(new ModbusVariable
                        {
                            DeviceName = deviceName,
                            Name = name,
                            FunctionCode = "19", // fixo por enquanto
                            Offset = offset,
                            Address = $"4{(offset + 1).ToString("D4")}",
                            Value = MosbusSlaveTcpWrapper.GetValue(offset)
                        });
                    }
                }
            }

            return result;
        }     

    }


}
