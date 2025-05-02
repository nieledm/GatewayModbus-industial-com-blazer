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
        
        private void Save()
        {
            _xml.Save(_path);
        }
   
        public void AddDevice(DeviceConfigModel device)
        {
            string suffix = device.Name.Replace("DL6000_", "");

            var settings = new Dictionary<string, string>
            {
                { $"ip_DL_6000_{suffix}", device.Ip },
                { $"port_DL_6000_{suffix}", device.Port },
                { $"unitId1_DL_6000_{suffix}", device.UnitId1 },
                { $"unitId2_DL_6000_{suffix}", device.UnitId2 },
                { $"StartIndexDL1_DL_6000_{suffix}", device.StartIndexDL1 },
                { $"StartIndexDL2_DL_6000_{suffix}", device.StartIndexDL2 },
                { $"cycle_DL_6000_{suffix}", device.Cycle },
                { $"timeOutSend_DL_6000_{suffix}", device.TimeoutSend },
                { $"timeOutReceive_DL_6000_{suffix}", device.TimeoutReceive }
            };

            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            foreach (var kvp in settings)
            {
                // Remove se já existir
                var existing = appSettings.Elements("add")
                    .FirstOrDefault(e => e.Attribute("key")?.Value == kvp.Key);
                if (existing != null)
                    existing.Remove();

                appSettings.Add(new XElement("add",
                    new XAttribute("key", kvp.Key),
                    new XAttribute("value", kvp.Value)));
            }

            _xml.Save(_path);
        }
        
         public void AddStartIndexEntry(ModbusVariable variable)
        {
            var suffix = variable.DeviceName.Replace("DL6000_", "");
            var tipo = DetermineDLSection(variable.Offset);

            if (tipo == null) return;

            string key = $"StartIndex{tipo}_DL_6000_{suffix}";
            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            var existing = appSettings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == key);

            if (existing != null)
                existing.SetAttributeValue("value", variable.Offset.ToString());
            else
                appSettings.Add(new XElement("add",
                    new XAttribute("key", key),
                    new XAttribute("value", variable.Offset.ToString())));

            _xml.Save(_path);
        }

        public void RemoveStartIndexEntry(ModbusVariable variable)
        {
            var suffix = variable.DeviceName.Replace("DL6000_", "");
            var tipo = DetermineDLSection(variable.Offset);

            if (tipo == null) return;

            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            // Remove StartIndex
            string startIndexKey = $"StartIndex{tipo}_DL_6000_{suffix}";
            var startIndex = appSettings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == startIndexKey);
            startIndex?.Remove();
            

            // Remove VAR entry
            string varKey = $"VAR_{suffix}_{variable.Offset}";
            var varEntry = appSettings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == varKey);
            varEntry?.Remove();            
            
            _xml.Save(_path);
            
        }

        private string? DetermineDLSection(int offset)
        {
            if (offset >= 0 && offset < 32) return "DL1";
            if (offset >= 32) return "DL2";
            return null;
        }
    }
}