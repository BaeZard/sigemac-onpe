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

        // GET: api/SesionCapacitacion -> Resuelve SessionService.list() en el Front
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await dbContext.SesionCapacitacions.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // GET: api/SesionCapacitacion/Obtener/5
        [HttpGet("Obtener/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var sesion = await dbContext.SesionCapacitacions
                .Include(s => s.IdCapacitadorNavigation)
                .FirstOrDefaultAsync(s => s.IdSesion == id);

            if (sesion == null) return NotFound(new { mensaje = "Sesión no encontrada" });

            return StatusCode(StatusCodes.Status200OK, sesion);
        }

        // POST: api/SesionCapacitacion/Nuevo
        [HttpPost("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] SesionCapacitacion objeto)
        {
            await dbContext.SesionCapacitacions.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        // POST: api/SesionCapacitacion/Editar
        [HttpPost("Editar")]
        public async Task<IActionResult> Editar([FromBody] SesionCapacitacion objeto)
        {
            dbContext.SesionCapacitacions.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        // DELETE: api/SesionCapacitacion/Eliminar/5
        [HttpDelete("Eliminar/{id:int}")]
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