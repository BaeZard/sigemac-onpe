using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU01_ConsultarAsignacionLocal;

namespace SIGEMAC_ONPE.Controllers.CU01_ConsultarAsignacionLocal
{
    [Route("api/cus01")]
    [ApiController]
    public class CU01Controller : ControllerBase
    {
        private readonly ICU01ConsultarAsignacionService _service;

        public CU01Controller(ICU01ConsultarAsignacionService service)
        {
            _service = service;
        }

        [HttpGet("{dni}")]
        public async Task<IActionResult> GetByDni(string dni)
        {
            var resultado = await _service.ConsultarAsignacionPorDniAsync(dni);

            if (resultado == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "No se encontró el registro." });
            }

            return StatusCode(StatusCodes.Status200OK, resultado);
        }
    }
}