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

        // CUS-04: Carga la lista de asistencia por sesión
        [HttpGet("{idSesion:int}")]
        public async Task<IActionResult> ObtenerPorSesion(int idSesion)
        {
            var lista = await dbContext.Asistencia
                .Include(a => a.DniMiembroMesaNavigation)
                .Where(a => a.IdSesion == idSesion)
                .ToListAsync();

            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // CUS-04: Procesa la transacción de asistencia apuntando a la ruta con el ID de sesión que manda el Front
        [HttpPost("{sesionId}")]
        public async Task<IActionResult> Registrar(int sesionId, [FromBody] AsistenciaRequestDto request)
        {
            // Nota: Asegúrate de recibir el objeto que envía el frontend (registros o un objeto individual)
            foreach (var item in request.Registros)
            {
                var asistenciaExistente = await dbContext.Asistencia
                    .FirstOrDefaultAsync(a => a.DniMiembroMesa == item.Dni && a.IdSesion == sesionId);

                if (asistenciaExistente != null)
                {
                    asistenciaExistente.Asistio = item.Asistio;
                    asistenciaExistente.FechaRegistro = DateTime.Now;
                    dbContext.Asistencia.Update(asistenciaExistente);
                }
                else
                {
                    var nuevaAsistencia = new Asistencium
                    {
                        IdSesion = sesionId,
                        DniMiembroMesa = item.Dni,
                        Asistio = item.Asistio,
                        FechaRegistro = DateTime.Now
                    };
                    await dbContext.Asistencia.AddAsync(nuevaAsistencia);
                }
            }

            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "Asistencia guardada correctamente" });
        }
    }

    // DTO auxiliar para mapear lo que envía el frontend en bloque
    public class AsistenciaRequestDto
    {
        public List<RegistroItem> Registros { get; set; }
    }

    public class RegistroItem
    {
        public string Dni { get; set; }
        public bool Asistio { get; set; }
    }
}