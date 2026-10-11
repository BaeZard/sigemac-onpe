using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SIGEMAC_ONPE.Controllers.CU09_GenerarReportesDashboards
{
    [Route("api/Reports")]
    [ApiController]
    public class CU09_GenerarReportesDashboardsController : ControllerBase
    {
        [HttpGet("summary")]
        public IActionResult ObtenerResumen()
        {
            var resumen = new
            {
                miembros = 4,
                sesiones = 1,
                cumplimiento = 100,
                regiones = new[]
                {
                    new { n = "Lima", p = 100 }
                }
            };

            return Ok(resumen);
        }
    }
}
