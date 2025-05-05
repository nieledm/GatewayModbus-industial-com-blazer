// namespace DL6000WebConfig.Models
// {
//     public class ModbusVariable
//     {
//         private int _offset;
//         public string DeviceName { get; set; } = "";
//         public string Name { get; set; } = "";
//         public int Offset
//         {
//             get => _offset;
//             set
//             {
//                 _offset = value;
//                 Address = $"40{(_offset + 1).ToString("D3")}";                
//             }
//         }
//         public string Address { get; set; } = "";
//         public ushort Value { get; set; }
        
//         #region Memoria de exposição modbus
//         //Calcular o endereço de esposição
//         public int GetRealAddress(DeviceConfigModel config)
//         {
//             int.TryParse(config.StartIndexDL1, out int startDL1);
//             return startDL1 + Offset;
//         }

//         public void UpdateRealAddress(DeviceConfigModel config)
//         {
//             RealAddress = GetRealAddress(config);
//         }

//         public int RealAddress { get; set; }

//     }
//     #endregion
// }
