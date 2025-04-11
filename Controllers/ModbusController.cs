using DL6000WebConfig.Models;
using Microsoft.AspNetCore.Mvc;

namespace DL6000WebConfig.Controllers
{
    [Route("api/modbus")]
    [ApiController]
    public class ModbusController : ControllerBase
    {
        [HttpGet("variables")]
        public ActionResult<List<ModbusVariable>> GetVariables()
        {
            // Aqui você vai acessar o DataStore
            var list = new List<ModbusVariable>
            {
                new ModbusVariable { Name = "Vazão", FunctionCode = "19", Offset = 4, Value = MosbusSlaveTcpWrapper.GetValue(4) },
                new ModbusVariable { Name = "Temperatura", FunctionCode = "1A", Offset = 5, Value = MosbusSlaveTcpWrapper.GetValue(5) }
            };

            return list;
        }
    }
}
