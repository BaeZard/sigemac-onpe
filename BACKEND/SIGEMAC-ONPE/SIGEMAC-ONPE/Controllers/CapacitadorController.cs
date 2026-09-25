using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CapacitadorController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public CapacitadorController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        // GET: api/Capacitador -> Devuelve la lista correcta de capacitadores
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await dbContext.Capacitadors.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // GET: api/Capacitador/Obtener/5
        [HttpGet("Obtener/{IdCapacitador:int}")]
        public async Task<IActionResult> Get(int idCapacitador)
        {
            var capacitador = await dbContext.Capacitadors.FirstOrDefaultAsync(e => e.IdCapacitador == idCapacitador);
            if (capacitador == null) return NotFound(new { mensaje = "No se encontró el registro." });
            return StatusCode(StatusCodes.Status200OK, capacitador);
        }

        // GET: api/Capacitador/PorDni/40125896 -> Resuelve la carga del panel según el DNI logueado
        [HttpGet("PorDni/{dni}")]
        public async Task<IActionResult> GetPorDni(string dni)
        {
            var capacitador = await dbContext.Capacitadors
                .Include(c => c.IdUsuarioNavigation)
                .FirstOrDefaultAsync(c => c.IdUsuarioNavigation.Username == dni);

            if (capacitador == null)
            {
                return NotFound(new { mensaje = "No se encontró el registro del capacitador." });
            }

            return StatusCode(StatusCodes.Status200OK, capacitador);
        }

        // POST: api/Capacitador/Nuevo
        [HttpPost("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] Capacitador objeto)
        {
            await dbContext.Capacitadors.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        // POST: api/Capacitador/Editar
        [HttpPost("Editar")]
        public async Task<IActionResult> Editar([FromBody] Capacitador objeto)
        {
            dbContext.Capacitadors.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        // DELETE: api/Capacitador/Eliminar/5
        [HttpDelete("Eliminar/{IdCapacitador:int}")]
        public async Task<IActionResult> Eliminar(int idCapacitador)
        {
            var capacitador = await dbContext.Capacitadors.FirstOrDefaultAsync(e => e.IdCapacitador == idCapacitador);

            if (capacitador != null)
            {
                dbContext.Capacitadors.Remove(capacitador);
                await dbContext.SaveChangesAsync();
            }

            return StatusCode(StatusCodes.Status200OK, capacitador);
        }
    }
}