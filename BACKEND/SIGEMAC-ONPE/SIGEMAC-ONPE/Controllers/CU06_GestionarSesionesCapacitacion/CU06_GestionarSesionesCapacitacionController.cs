using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU06_GestionarSesionesCapacitacion;
using System.Threading.Tasks;

namespace SIGEMAC_ONPE.Controllers.CU06_GestionarSesionesCapacitacion
{
    [Route("api/SesionCapacitacion")]
    [ApiController]
    public class CU06_GestionarSesionesCapacitacionController : ControllerBase
    {
        private readonly ICU06_GestionarSesionesCapacitacionService _service;

        public CU06_GestionarSesionesCapacitacionController(ICU06_GestionarSesionesCapacitacionService service)
        {
            _service = service;
        }

        [HttpPost("Nuevo")]
        public async Task<IActionResult> Crear([FromBody] SesionCapacitacionDTO dto)
        {
            var exito = await _service.CrearSesionAsync(dto);
            if (!exito) return BadRequest(new { mensaje = "Error al registrar la sesión." });
            return Ok(new { mensaje = "Sesión creada exitosamente." });
        }

        [HttpPost("Editar/{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] SesionCapacitacionDTO dto)
        {
            var resultado = await _service.ActualizarSesionAsync(id, dto);
            return resultado switch
            {
                CU06_ResultadoActualizacion.Actualizado => Ok(new { mensaje = "Sesión actualizada correctamente." }),
                CU06_ResultadoActualizacion.SesionNoEncontrada => NotFound(new { mensaje = "Sesión no encontrada." }),
                CU06_ResultadoActualizacion.CapacitadorNoEncontrado => NotFound(new { mensaje = "No se encontró un capacitador con ese DNI." }),
                CU06_ResultadoActualizacion.SolicitudInvalida => BadRequest(new { mensaje = "El DNI del capacitador o los datos enviados no son válidos." }),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpDelete("Eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var exito = await _service.EliminarSesionAsync(id);
            if (!exito) return NotFound(new { mensaje = "Sesión no encontrada." });
            return Ok(new { mensaje = "Sesión eliminada con éxito." });
        }
    }
}
