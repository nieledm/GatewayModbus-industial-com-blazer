using System.Xml.Linq;
using DL6000WebConfig.Models;

namespace DL6000WebConfig.Services
{
    public class ConfigService
    {
        private readonly string _path;
        private readonly XDocument _xml;

        private ModbusVariableService? _variableService;

        public ConfigService(string path)
        {
            _path = Path.GetFullPath(path);
            _xml = XDocument.Load(_path);
        }

        public void SetVariableService(ModbusVariableService variableService)
        {
            _variableService = variableService;
        }
        #region Método para buscar device por nome

        public DeviceConfigModel? GetDeviceByName(string deviceName)
        {
            var devices = GetDevices();
            return devices?.FirstOrDefault(d => d.Name == deviceName);
        }
        #endregion


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

        public void UpdateDevice(DeviceConfigModel updated, string oldName)
        {
            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            var newSuffix = updated.Name.Replace("DL6000_", "");
            var oldSuffix = oldName.Replace("DL6000_", "");

            // Se o nome mudou, remove entradas antigas
            if (oldSuffix != newSuffix)
            {
                // no json
                if (_variableService is not null)
                {
                    var variables = _variableService.GetAll();
                    foreach (var v in variables.Where(v => v.DeviceName == oldName))
                    {
                        v.DeviceName = updated.Name;
                    }
                    _variableService.Save(variables);

                    var oldKeys = appSettings.Elements("add")
                        .Where(e => e.Attribute("key")?.Value?.EndsWith("_DL_6000_" + oldSuffix) == true)
                        .ToList();

                    foreach (var el in oldKeys)
                    {
                        el.Remove();
                    }
                }
                else
                {
                    // log ou exception
                    throw new InvalidOperationException("ModbusVariableService não foi injetado.");
                }
            }

            var keys = new Dictionary<string, string>
            {
                [$"ip_DL_6000_{newSuffix}"] = updated.Ip,
                [$"port_DL_6000_{newSuffix}"] = updated.Port,
                [$"unitId1_DL_6000_{newSuffix}"] = updated.UnitId1,
                [$"unitId2_DL_6000_{newSuffix}"] = updated.UnitId2,
                [$"StartIndexDL1_DL_6000_{newSuffix}"] = updated.StartIndexDL1,
                [$"StartIndexDL2_DL_6000_{newSuffix}"] = updated.StartIndexDL2,
                [$"cycle_DL_6000_{newSuffix}"] = updated.Cycle,
                [$"timeOutSend_DL_6000_{newSuffix}"] = updated.TimeoutSend,
                [$"timeOutReceive_DL_6000_{newSuffix}"] = updated.TimeoutReceive
            };

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
        private string? DetermineDLSection(ModbusVariable variable)
        {
            var suffix = variable.DeviceName.Replace("DL6000_", "");

            var startIndices = GetAllStartIndices();

            var equipment = startIndices.FirstOrDefault(e => e.Suffix == suffix);
            if (equipment == null)
                return null;

            int offset = variable.Offset;

            if (offset >= equipment.StartIndexDL1 && offset < equipment.StartIndexDL2)
                return "DL1";

            if (offset >= equipment.StartIndexDL2)
                return "DL2";

            return null;
        }

        public List<EquipmentStartIndices> GetAllStartIndices()
        {
            var appSettings = _xml.Root?.Element("appSettings")?.Elements("add").ToList();
            var list = new List<EquipmentStartIndices>();

            if (appSettings == null) return list;

            var startIndexKeys = appSettings
                .Where(e => e.Attribute("key")?.Value.StartsWith("StartIndex") == true)
                .Select(e => e.Attribute("key")?.Value)
                .Where(k => k != null && k.Contains("_DL_6000_"))
                .Distinct();

            var suffixes = startIndexKeys
                .Select(k => k!.Split("_DL_6000_").Last())
                .Distinct();

            foreach (var suffix in suffixes)
            {
                string Get(string tipo) =>
                    appSettings.FirstOrDefault(e => e.Attribute("key")?.Value == $"StartIndex{tipo}_DL_6000_{suffix}")
                    ?.Attribute("value")?.Value ?? "0";

                list.Add(new EquipmentStartIndices
                {
                    Suffix = suffix,
                    StartIndexDL1 = int.TryParse(Get("DL1"), out var dl1) ? dl1 : 0,
                    StartIndexDL2 = int.TryParse(Get("DL2"), out var dl2) ? dl2 : 0
                });
            }

            return list;
        }

        
        public void AddStartIndexEntry(ModbusVariable variable)
        {
            var suffix = variable.DeviceName.Replace("DL6000_", "");
            var tipo = DetermineDLSection(variable); // usa offset + StartIndex carregado do .config

            if (tipo == null) return;

            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            // Apenas cria ou atualiza uma entrada de variável, se você quiser armazenar assim
            string varKey = $"VAR_{suffix}_{variable.Offset}";
            var existingVar = appSettings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == varKey);

            if (existingVar != null)
            {
                existingVar.SetAttributeValue("value", variable.Name); // ou qualquer info útil
            }
            else
            {
                appSettings.Add(new XElement("add",
                    new XAttribute("key", varKey),
                    new XAttribute("value", variable.Name)));
            }

            _xml.Save(_path);
        }


        public void RemoveStartIndexEntry(ModbusVariable variable)
        {
            var suffix = variable.DeviceName.Replace("DL6000_", "");
            var tipo = DetermineDLSection(variable);

            if (tipo == null) return;

            var appSettings = _xml.Root?.Element("appSettings");
            if (appSettings == null) return;

            // NÃO remover StartIndex, apenas a variável (se aplicável)
            string varKey = $"VAR_{suffix}_{variable.Offset}";
            var varEntry = appSettings.Elements("add")
                .FirstOrDefault(e => e.Attribute("key")?.Value == varKey);
            varEntry?.Remove();

            _xml.Save(_path);
        }

        #endregion        


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