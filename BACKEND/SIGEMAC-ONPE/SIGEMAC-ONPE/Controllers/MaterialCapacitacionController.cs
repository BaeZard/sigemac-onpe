using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialCapacitacionController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public MaterialCapacitacionController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        // GET: api/MaterialCapacitacion -> Ahora sí consulta la tabla correcta de materiales
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var lista = await dbContext.MaterialCapacitacions.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        // GET: api/MaterialCapacitacion/Obtener/5
        [HttpGet("Obtener/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var material = await dbContext.MaterialCapacitacions.FirstOrDefaultAsync(m => m.IdMaterial == id);
            if (material == null) return NotFound(new { mensaje = "No se encontró el registro." });
            return StatusCode(StatusCodes.Status200OK, material);
        }

        // POST: api/MaterialCapacitacion/Nuevo
        [HttpPost("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] MaterialCapacitacion objeto)
        {
            await dbContext.MaterialCapacitacions.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        // POST: api/MaterialCapacitacion/Editar
        [HttpPost("Editar")]
        public async Task<IActionResult> Editar([FromBody] MaterialCapacitacion objeto)
        {
            dbContext.MaterialCapacitacions.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        // DELETE: api/MaterialCapacitacion/Eliminar/5
        [HttpDelete("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var material = await dbContext.MaterialCapacitacions.FirstOrDefaultAsync(m => m.IdMaterial == id);

            if (material != null)
            {
                dbContext.MaterialCapacitacions.Remove(material);
                await dbContext.SaveChangesAsync();
            }

            return StatusCode(StatusCodes.Status200OK, material);
        }
    }
}