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

        #region CRUD DE DISPOSITIVOS
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

        public void UpdateDevice(DeviceConfigModel updated)
        {
            var suffix = updated.Name.Replace("DL6000_", "");
            var keys = new Dictionary<string, string>
            {
                [$"ip_DL_6000_{suffix}"] = updated.Ip,
                [$"port_DL_6000_{suffix}"] = updated.Port,
                [$"unitId1_DL_6000_{suffix}"] = updated.UnitId1,
                [$"unitId2_DL_6000_{suffix}"] = updated.UnitId2,
                [$"StartIndexDL1_DL_6000_{suffix}"] = updated.StartIndexDL1,
                [$"StartIndexDL2_DL_6000_{suffix}"] = updated.StartIndexDL2,
                [$"cycle_DL_6000_{suffix}"] = updated.Cycle,
                [$"timeOutSend_DL_6000_{suffix}"] = updated.TimeoutSend,
                [$"timeOutReceive_DL_6000_{suffix}"] = updated.TimeoutReceive
            };

            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            foreach (var key in keys.Keys)
            {
                var element = appSettings.Elements("add")
                    .FirstOrDefault(e => e.Attribute("key")?.Value == key);

                if (element != null)
                {
                    element.SetAttributeValue("value", keys[key]);
                }
                else
                {
                    appSettings.Add(new XElement("add",
                        new XAttribute("key", key),
                        new XAttribute("value", keys[key])));
                }
            }

            _xml.Save(_path);
        }

        public void DeleteDevice(string deviceName)
        {
            string suffix = deviceName.Replace("DL6000_", "");

            var keysToRemove = _xml.Root?
                .Element("appSettings")?
                .Elements("add")
                .Where(e =>
                {
                    var key = e.Attribute("key")?.Value ?? "";
                    return key.EndsWith($"_DL_6000_{suffix}") || key.Contains($"VAR_{suffix}_");
                })
                .ToList();

            if (keysToRemove != null)
            {
                foreach (var el in keysToRemove)
                    el.Remove();
                _xml.Save(_path);
            }
        }
        #endregion
        
        #region CRUD DAS MEMÓRIAS
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
        #endregion        

        private string? DetermineDLSection(int offset)
        {
            if (offset >= 0 && offset < 32) return "DL1";
            if (offset >= 32) return "DL2";
            return null;
        }

        #region CONFIGURAÇÃO GERAL
        public GeneralSettingsModel GetGeneralSettings()
        {
            var get = (string key) =>
                _xml.Root?.Element("appSettings")?.Elements("add")
                    .FirstOrDefault(e => e.Attribute("key")?.Value == key)?
                    .Attribute("value")?.Value ?? "";

            return new GeneralSettingsModel
            {
                SlaveID = get("slave_ID"),
                SlaveIp1 = get("slave_Ip_byte_1"),
                SlaveIp2 = get("slave_Ip_byte_2"),
                SlaveIp3 = get("slave_Ip_byte_3"),
                SlaveIp4 = get("slave_Ip_byte_4"),
                SlavePort = get("slave_port"),
                DeviceCount = get("deviceCount")
            };
        }

        public void SaveGeneralSettings(GeneralSettingsModel settings)
        {
            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            void Set(string key, string value)
            {
                var el = appSettings.Elements("add")
                    .FirstOrDefault(e => e.Attribute("key")?.Value == key);

                if (el != null)
                    el.SetAttributeValue("value", value);
                else
                    appSettings.Add(new XElement("add", new XAttribute("key", key), new XAttribute("value", value)));
            }

            Set("slave_ID", settings.SlaveID);
            Set("slave_Ip_byte_1", settings.SlaveIp1);
            Set("slave_Ip_byte_2", settings.SlaveIp2);
            Set("slave_Ip_byte_3", settings.SlaveIp3);
            Set("slave_Ip_byte_4", settings.SlaveIp4);
            Set("slave_port", settings.SlavePort);
            Set("deviceCount", settings.DeviceCount);

            _xml.Save(_path);
        }
        #endregion

    }
}