using DL6000WebConfig.Models;
using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Mvc;

namespace DL6000WebConfig.Controllers
{
    public class VariableUpdatePayload
    {
        public ModbusVariable Original { get; set; }
        public ModbusVariable Updated { get; set; }
    }

    [Route("api/modbus")]
    [ApiController]
    public class ModbusController : ControllerBase
    {
        private readonly ModbusVariableService _variableService;

        public ModbusController(ModbusVariableService variableService)
        {
            _variableService = variableService;
        }

        [HttpGet("variables")]
        public ActionResult<List<ModbusVariable>> GetVariables()
        {
            var variables = _variableService.GetAll();            
            foreach (var v in variables)
            {
                var config = _variableService.GetDeviceConfig(v.DeviceName);
                if (config != null){
                    v.Address = $"40{(v.Offset + 1):D3}";
                    v.RealAddress = v.GetRealAddress(config);
                }
                else{
                    Console.WriteLine($"Configuração não encontrada para o dispositivo {v.DeviceName}");
                }                                   
            }
            return variables;
        }

        [HttpPost("variables")]
        public IActionResult AddVariable([FromBody] ModbusVariable variable)
        {
            _variableService.Add(variable);

            var config = _variableService.GetDeviceConfig();
            variable.RealAddress = variable.GetRealAddress(config);

            return Ok(variable);
        }

        [HttpPut("variables")]
        public IActionResult UpdateVariable([FromBody] VariableUpdatePayload payload)
        {            
            _variableService.Update(payload.Updated, payload.Original);
            return Ok();
        }

        [HttpDelete("variables/{deviceName}/{offset}")]
        public IActionResult DeleteVariable(string deviceName, int offset)
        {
            _variableService.Delete(deviceName, offset);
            return Ok();
        }
    }
}