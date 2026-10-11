using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante;

namespace SIGEMAC_ONPE.Controllers.CU04_RegistrarAsistenciaParticipante
{
    [Route("api/Asistencia")]
    [ApiController]
    public class CU04Controller : ControllerBase
    {
        private readonly ICU04RegistrarAsistenciaParticipanteService _service;

        public CU04Controller(ICU04RegistrarAsistenciaParticipanteService service)
        {
            _service = service;
        }

        [HttpPost("{sesionId:int}")]
        public async Task<IActionResult> RegistrarAsistenciaLote(
            int sesionId,
            [FromBody] CU04_RegistroAsistenciaRequest request)
        {
            var resultado = await _service.RegistrarAsistenciasAsync(
                sesionId,
                request.Registros ?? new List<CU04_RegistroAsistenciaDTO>());

            return resultado switch
            {
                CU04_ResultadoRegistro.Registrado => Ok(new { message = "Asistencia registrada correctamente." }),
                CU04_ResultadoRegistro.SolicitudInvalida => BadRequest(new { message = "La sesión o los registros enviados no son válidos." }),
                CU04_ResultadoRegistro.SesionNoEncontrada => NotFound(new { message = "No se encontró la sesión." }),
                CU04_ResultadoRegistro.ParticipanteNoAsignado => Conflict(new { message = "Uno o más participantes no están asignados a esta sesión." }),
                _ => StatusCode(500)
            };
        }
    }
}
