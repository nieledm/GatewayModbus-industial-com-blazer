using DL6000WebConfig.Models;
using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Mvc;

namespace DL6000WebConfig.Controllers
{
    [Route("api/modbus")]
    [ApiController]
    public class ModbusController : ControllerBase
    {
        private readonly ModbusVariableService _variableService;
        private readonly ILogger<ModbusController> _logger;

        public ModbusController(ModbusVariableService variableService, ILogger<ModbusController> logger)
        {
            _variableService = variableService;
            _logger = logger;
        }

        [HttpGet("variables")]
        public ActionResult<List<ModbusVariable>> GetVariables()
        {
            var variables = _variableService.GetAll();
            foreach (var v in variables)
            {
                var config = _variableService.GetDeviceConfig(v.DeviceName);
                if (config != null)
                {
                    v.Address = $"40{(v.Offset + 1):D3}";
                    v.RealAddress = v.GetRealAddress(config);
                }
                else
                {
                    _logger.LogWarning($"Configuração não encontrada para o dispositivo {v.DeviceName}");
                }                                   
            }
            return Ok(variables);
        }

        [HttpPost("variables")]
        public IActionResult AddVariable([FromBody] ModbusVariable variable)
        {
            _variableService.Add(variable);

            // Obtém a configuração do dispositivo específico
            var config = _variableService.GetDeviceConfig(variable.DeviceName);
            if (config == null)
            {
                _logger.LogWarning($"Configuração não encontrada para o dispositivo {variable.DeviceName}");
                return BadRequest("Configuração do dispositivo não encontrada.");
            }

            variable.RealAddress = variable.GetRealAddress(config);
            return Ok(variable);
        }

        [HttpPut("variables")]
        public IActionResult UpdateVariable([FromBody] VariableUpdatePayload payload)
        {
            if (payload.Original == null || payload.Updated == null)
            {
                return BadRequest("Both Original and Updated variables are required.");
            }
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
