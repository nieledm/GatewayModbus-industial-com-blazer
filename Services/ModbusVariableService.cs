using System.Text.Json;
using DL6000WebConfig.Models;
using System.Xml.Linq;

namespace DL6000WebConfig.Services
{
    public class ModbusVariableService
    {
        private readonly string _jsonPath;
        private readonly ConfigService _configService;

        public ModbusVariableService(string jsonPath, ConfigService configService)
        {
            _jsonPath = Path.GetFullPath(jsonPath);
            _configService = configService;
            if (!File.Exists(_jsonPath))
                File.WriteAllText(_jsonPath, "[]");
        }

        public List<ModbusVariable> GetAll()
        {
            var json = File.ReadAllText(_jsonPath);
            return JsonSerializer.Deserialize<List<ModbusVariable>>(json) ?? new List<ModbusVariable>();
        }

        public void Save(List<ModbusVariable> variables)
        {
            var json = JsonSerializer.Serialize(variables, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_jsonPath, json);
        }

        public void Add(ModbusVariable variable)
        {
            var list = GetAll();
            list.Add(variable);
            Save(list);
            _configService.AddStartIndexEntry(variable);
        }

        public void Update(ModbusVariable updated, ModbusVariable original)
        {
            Delete(original.DeviceName, original.Offset);
            Add(updated);
        }

        public void Delete(string deviceName, int offset)
        {
            var list = GetAll();
            var toRemove = list.FirstOrDefault(v => v.DeviceName == deviceName && v.Offset == offset);
            if (toRemove != null)
            {
                list.Remove(toRemove);
                Save(list);
                _configService.RemoveStartIndexEntry(toRemove);
            }
        }
        public void DeleteAllVariablesForDevice(string deviceName)
        {
            var list = GetAll();
            list.RemoveAll(v => v.DeviceName == deviceName);
            Save(list);
        }
    }
}