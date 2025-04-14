using DL6000WebConfig.Models;
using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Mvc;

namespace DL6000WebConfig.Controllers
{
    [Route("api/modbus")]
    [ApiController]
    public class ModbusController : ControllerBase
    {
        private static readonly List<ModbusVariable> manualVariables = new();
        private readonly ConfigService _configService;

        public ModbusController(ConfigService configService)
        {
            _configService = configService;
        }

        [HttpGet("variables")]
        public ActionResult<List<ModbusVariable>> GetVariables()
        {
            var allVariables = new List<ModbusVariable>();

            // Adiciona as variáveis configuradas no .config
            var configured = _configService.GetConfiguredVariables();
            if (configured != null)
                allVariables.AddRange(configured);

            // Adiciona as variáveis adicionadas pela interface (dinamicamente)
            allVariables.AddRange(manualVariables);

            // Atualiza o valor e o endereço
            foreach (var v in allVariables)
            {
                v.Address = $"4000{v.Offset + 1}";
                v.Value = MosbusSlaveTcpWrapper.GetValue(v.Offset);
            }

            return allVariables;
        }

        [HttpPost("variables")]
        public IActionResult AddVariable([FromBody] ModbusVariable newVar)
        {
            if (newVar != null)
                manualVariables.Add(newVar);
            return Ok();
        }

        [HttpDelete("variables/{offset}")]
        public IActionResult DeleteVariable(int offset)
        {
            var v = manualVariables.FirstOrDefault(x => x.Offset == offset);
            if (v != null)
                manualVariables.Remove(v);
            return Ok();
        }
    }
}
