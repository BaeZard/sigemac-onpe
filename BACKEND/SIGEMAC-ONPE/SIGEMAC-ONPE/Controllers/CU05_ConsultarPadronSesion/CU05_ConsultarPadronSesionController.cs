using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU05_ConsultarPadronSesion;

namespace SIGEMAC_ONPE.Controllers.CU05_ConsultarPadronSesion
{
    [Route("api/SesionCapacitacion")]
    [ApiController]
    public class CU05Controller : ControllerBase
    {
        private readonly ICU05ConsultarPadronSesionService _service;

        public CU05Controller(ICU05ConsultarPadronSesionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetSesionesConPadron()
        {
            var resultado = await _service.ObtenerSesionesConPadronAsync();
            return Ok(resultado);
        }
    }
}