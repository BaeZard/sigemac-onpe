using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OdpeController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public OdpeController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        [HttpGet]
        [Route("a")]
        public async Task<IActionResult> Get()
        {
            var lista = await dbContext.Odpes.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        [Route("Obtener/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var odpe = await dbContext.Odpes.FirstOrDefaultAsync(o => o.IdOdpe == id);
            return StatusCode(StatusCodes.Status200OK, odpe);
        }

        [HttpPost]
        [Route("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] Odpe objeto)
        {
            await dbContext.Odpes.AddAsync(objeto);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPost]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] Odpe objeto)
        {
            dbContext.Odpes.Update(objeto);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var odpe = await dbContext.Odpes.FirstOrDefaultAsync(o => o.IdOdpe == id);
            if (odpe != null)
            {
                dbContext.Odpes.Remove(odpe);
                await dbContext.SaveChangesAsync();
            }
            return StatusCode(StatusCodes.Status200OK, odpe);
        }
    }
}