// using System.Text.Json;
// using DL6000WebConfig.Models;

// namespace DL6000WebConfig.Services
// {
//     public class ModbusVariableService
//     {
//         private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "modbus-variables.json");
//         private List<ModbusVariable> _variables = new();

//         public ModbusVariableService()
//         {
//             LoadVariables();
//         }

//         private void LoadVariables()
//         {
//             if (File.Exists(_filePath))
//             {
//                 var json = File.ReadAllText(_filePath);
//                 var list = JsonSerializer.Deserialize<List<ModbusVariable>>(json);
//                 if (list != null)
//                     _variables = list;
//             }
//         }

//         private void SaveVariables()
//         {
//             var json = JsonSerializer.Serialize(_variables, new JsonSerializerOptions { WriteIndented = true });
//             File.WriteAllText(_filePath, json);
//         }

//         public List<ModbusVariable> GetAll()
//         {
//             return _variables.ToList(); // retorna cópia
//         }

//         public void Add(ModbusVariable variable)
//         {
//             _variables.Add(variable);
//             SaveVariables();
//         }

//         public void Update(ModbusVariable variable)
//         {
//             var existing = _variables.FirstOrDefault(v =>
//                 v.DeviceName == variable.DeviceName && v.Offset == variable.Offset);

//             if (existing != null)
//             {
//                 existing.Name = variable.Name;
//                 existing.FunctionCode = variable.FunctionCode;
//                 existing.Value = variable.Value;
//                 SaveVariables();
//             }
//         }

//         public void Delete(string deviceName, int offset)
//         {
//             var existing = _variables.FirstOrDefault(v =>
//                 v.DeviceName == deviceName && v.Offset == offset);

//             if (existing != null)
//             {
//                 _variables.Remove(existing);
//                 SaveVariables();
//             }
//         }

//         public void ReplaceAll(List<ModbusVariable> newList)
//         {
//             _variables = newList;
//             SaveVariables();
//         }
//     }
// }
