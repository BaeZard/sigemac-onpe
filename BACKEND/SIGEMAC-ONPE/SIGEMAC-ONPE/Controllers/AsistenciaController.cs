using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciaController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public AsistenciaController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        [HttpGet]
        [Route("PorSesion/{idSesion:int}")]
        /* CUS-04: Carga la lista de ciudadanos designados a la sesión */
        public async Task<IActionResult> ObtenerPorSesion(int idSesion)
        {
            // Usamos dbContext.Asistencia en lugar de Asistencias
            var lista = await dbContext.Asistencia
                .Include(a => a.DniMiembroMesaNavigation)
                .Where(a => a.IdSesion == idSesion)
                .ToListAsync();

            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpPost]
        [Route("Registrar")]
        /* CUS-04: Procesa la transacción de asistencia usando el modelo Asistencium */
        public async Task<IActionResult> Registrar([FromBody] Asistencium objeto)
        {
            var asistenciaExistente = await dbContext.Asistencia
                .FirstOrDefaultAsync(a => a.DniMiembroMesa == objeto.DniMiembroMesa && a.IdSesion == objeto.IdSesion);

            if (asistenciaExistente != null)
            {
                asistenciaExistente.Asistio = objeto.Asistio;
                asistenciaExistente.FechaRegistro = DateTime.Now;
                asistenciaExistente.Observacion = objeto.Observacion;
                dbContext.Asistencia.Update(asistenciaExistente);
            }
            else
            {
                objeto.FechaRegistro = DateTime.Now;
                await dbContext.Asistencia.AddAsync(objeto);
            }

            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "Asistencia guardada correctamente" });
        }
    }
}