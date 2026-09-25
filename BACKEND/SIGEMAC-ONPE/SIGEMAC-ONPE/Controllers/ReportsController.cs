using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        [HttpGet("summary")]
        public IActionResult GetSummary()
        {
            // Devuelve un objeto de resumen simulado para que el frontend lo lea sin problemas
            var summary = new
            {
                miembros = 4,
                sesiones = 1,
                cumplimiento = 100,
                regiones = new[] {
                    new { n = "Lima", p = 100 }
                }
            };
            return Ok(summary);
        }
    }
}