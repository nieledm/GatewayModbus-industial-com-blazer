// using DL6000WebConfig.Models;
// using DL6000WebConfig.Services;
// using Microsoft.AspNetCore.Mvc;

// namespace DL6000WebConfig.Controllers
// {
//     [Route("api/modbus")]
//     [ApiController]
//     public class ModbusController : ControllerBase
//     {
//         private readonly ConfigService _configService;
//         private readonly ModbusVariableService _variableService;

//         public ModbusController(ConfigService configService, ModbusVariableService variableService)
//         {
//             _configService = configService;
//             _variableService = variableService;
//         }

//         [HttpGet("variables")]
//         public ActionResult<List<ModbusVariable>> GetVariables()
//         {
            
//             // Recupera os dispositivos do .config
//             var configDevices = _configService.GetDevices();

//             // Carrega as variáveis do JSON
//             var variables = _variableService.GetAll();

//             foreach (var v in variables)
//             {
//                 // Atualiza o valor e o endereço com base no offset
//                 v.Value = MosbusSlaveTcpWrapper.GetValue(v.Offset);
//             }

//             return variables;
//         }

//         [HttpPost("variables")]
//         public IActionResult AddVariable([FromBody] ModbusVariable variable)
//         {
//             if (variable != null)
//             {
//                 _variableService.Add(variable);
//                 return Ok();
//             }

//             return BadRequest("Variável inválida");
//         }

//         [HttpPut("variables")]
//         public IActionResult UpdateVariable([FromBody] ModbusVariable variable)
//         {
//             if (variable != null)
//             {
//                 _variableService.Update(variable);
//                 return Ok();
//             }

//             return BadRequest("Variável inválida");
//         }

//         [HttpDelete("variables/{deviceName}/{offset}")]
//         public IActionResult DeleteVariable(string deviceName, int offset)
//         {
//             _variableService.Delete(deviceName, offset);
//             return Ok();
//         }
//     }
// }
