using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante;

namespace SIGEMAC_ONPE.Controllers.CU04_RegistrarAsistenciaParticipante
{
    // Ruta exacta según tu config.js
    [Route("api/Asistencia")]
    [ApiController]
    public class CU04Controller : ControllerBase
    {
        private readonly ICU04RegistrarAsistenciaParticipanteService _service;

        public CU04Controller(ICU04RegistrarAsistenciaParticipanteService service)
        {
            _service = service;
        }

        public class AsistenciaRequest
        {
            public string DniMiembro { get; set; } = string.Empty;
            public int IdSesion { get; set; }
            public bool Asistio { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAsistencia([FromBody] AsistenciaRequest request)
        {
            var exito = await _service.RegistrarAsistenciaAsync(request.DniMiembro, request.IdSesion, request.Asistio);

            if (!exito)
            {
                return BadRequest(new { message = "No se pudo registrar la asistencia. Verifique los datos." });
            }

            return Ok(new { message = "Asistencia registrada correctamente." });
        }
    }
}