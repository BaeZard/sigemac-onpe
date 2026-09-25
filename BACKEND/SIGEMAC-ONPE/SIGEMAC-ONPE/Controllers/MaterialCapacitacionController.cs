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

        [HttpGet]
        [Route("a")]
        public async Task<IActionResult> Get()
        {
            var listaMateriales = await dbContext.MaterialCapacitacions.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, listaMateriales);
        }

        [HttpGet]
        [Route("Obtener/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var material = await dbContext.MaterialCapacitacions.FirstOrDefaultAsync(m => m.IdMaterial == id);
            return StatusCode(StatusCodes.Status200OK, material);
        }

        [HttpPost]
        [Route("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] MaterialCapacitacion objeto)
        {
            await dbContext.MaterialCapacitacions.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPost]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] MaterialCapacitacion objeto)
        {
            dbContext.MaterialCapacitacions.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
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