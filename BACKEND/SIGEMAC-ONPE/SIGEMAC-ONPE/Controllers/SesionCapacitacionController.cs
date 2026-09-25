using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SesionCapacitacionController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public SesionCapacitacionController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        [HttpGet]
        [Route("a")]
        public async Task<IActionResult> Get()
        {
            // El Include nos trae los datos del Capacitador asignado a la sesión
            var listaSesiones = await dbContext.SesionCapacitacions
                .Include(s => s.IdCapacitadorNavigation)
                .ToListAsync();

            return StatusCode(StatusCodes.Status200OK, listaSesiones);
        }

        [HttpGet]
        [Route("Obtener/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var sesion = await dbContext.SesionCapacitacions
                .Include(s => s.IdCapacitadorNavigation)
                .FirstOrDefaultAsync(s => s.IdSesion == id);

            return StatusCode(StatusCodes.Status200OK, sesion);
        }

        [HttpPost]
        [Route("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] SesionCapacitacion objeto)
        {
            await dbContext.SesionCapacitacions.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPost]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] SesionCapacitacion objeto)
        {
            dbContext.SesionCapacitacions.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var sesion = await dbContext.SesionCapacitacions.FirstOrDefaultAsync(s => s.IdSesion == id);

            if (sesion != null)
            {
                dbContext.SesionCapacitacions.Remove(sesion);
                await dbContext.SaveChangesAsync();
            }

            return StatusCode(StatusCodes.Status200OK, sesion);
        }
    }
}