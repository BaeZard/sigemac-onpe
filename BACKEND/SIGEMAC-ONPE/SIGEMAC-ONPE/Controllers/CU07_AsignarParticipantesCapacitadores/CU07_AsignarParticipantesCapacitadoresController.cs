using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU07_AsignarParticipantesCapacitadores;

namespace SIGEMAC_ONPE.Controllers.CU07_AsignarParticipantesCapacitadores
{
    // Se conserva la ruta que utiliza actualmente MemberRepository en el frontend.
    [Route("api/cus01")]
    [ApiController]
    public class CU07_AsignarParticipantesCapacitadoresController : ControllerBase
    {
        private readonly ICU07_AsignarParticipantesCapacitadoresService _service;

        public CU07_AsignarParticipantesCapacitadoresController(
            ICU07_AsignarParticipantesCapacitadoresService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerParticipantes([FromQuery(Name = "sesion")] int sesionId)
        {
            if (sesionId <= 0)
            {
                return BadRequest(new { message = "Debe indicar una sesión válida." });
            }

            var participantes = await _service.ObtenerParticipantesPorSesionAsync(sesionId);
            return Ok(participantes);
        }

        [HttpPost("{dni}/assign")]
        public async Task<IActionResult> AsignarParticipante(
            string dni,
            [FromBody] CU07_AsignarDTO request)
        {
            var resultado = await _service.AsignarParticipanteAsync(dni, request.SesionId);

            return resultado switch
            {
                CU07_ResultadoAsignacion.Exitoso => Ok(new { message = "Participante asignado a la sesión." }),
                CU07_ResultadoAsignacion.SolicitudInvalida => BadRequest(new { message = "El DNI o la sesión no son válidos." }),
                CU07_ResultadoAsignacion.MiembroNoEncontrado => NotFound(new { message = "No se encontró el miembro de mesa." }),
                CU07_ResultadoAsignacion.SesionNoEncontrada => NotFound(new { message = "No se encontró la sesión." }),
                CU07_ResultadoAsignacion.YaAsignado => Conflict(new { message = "El miembro ya está asignado a esta sesión." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
