// using DL6000WebConfig.Models;
// using DL6000WebConfig.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace DL6000WebConfig.Controllers
// {
//     [Route("api/modbus")]
//     [ApiController]
//     public class ModbusController : ControllerBase
//     {
//         private readonly ModbusVariableService _variableService;

//         public ModbusController(ModbusVariableService variableService)
//         {
//             _variableService = variableService;
//         }

//         [HttpGet("variables")]
//         public ActionResult<List<ModbusVariable>> GetVariables()
//         {
//             var list = _variableService.GetAll();
//             foreach (var v in list)
//             {
//                 v.Address = $"40{(v.Offset + 1).ToString("D3")}";
//             }
//             return list;
//         }

//         [HttpPost("variables")]
//         public IActionResult AddVariable([FromBody] ModbusVariable variable)
//         {
//             _variableService.Add(variable);
//             return Ok();
//         }

//         [HttpPut("variables")]
//         public IActionResult UpdateVariable([FromBody] ModbusVariable variable)
//         {
//             _variableService.Update(variable);
//             return Ok();
//         }

//         [HttpDelete("variables/{deviceName}/{offset}")]
//         public IActionResult Delete(string deviceName, int offset)
//         {
//             _variableService.Delete(deviceName, offset);
//             return Ok();
//         }
//     }

// }
