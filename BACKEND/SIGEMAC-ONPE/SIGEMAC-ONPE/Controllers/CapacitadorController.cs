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

        [HttpGet]
        [Route("a")]

        public async Task<IActionResult> Get()
        {
            var listaCapacitador = await dbContext.Capacitadors.ToListAsync();
            return StatusCode(StatusCodes.Status200OK,listaCapacitador);
        }

        [HttpGet]
        [Route("Obtener/{IdCapacitador:int}")]

        /*a*/
        public async Task<IActionResult> Get(int idCapacitador)
        {
            var Capacitador= await dbContext.Capacitadors.FirstOrDefaultAsync(e => e.IdCapacitador==idCapacitador);
            return StatusCode(StatusCodes.Status200OK, Capacitador);
        }

        [HttpPost]
        [Route("Nuevo")]


        public async Task<IActionResult> Nuevo([FromBody] Capacitador objeto)
        {
            await dbContext.Capacitadors.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new {mensaje = "ok"});
        }

        [HttpPost]
        [Route("Editar")]


        public async Task<IActionResult> Editar([FromBody] Capacitador objeto)
        {
            dbContext.Capacitadors.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpDelete]
        [Route("Eliminar/{IdCapacitador:int}")]


        public async Task<IActionResult> Eliminar(int idCapacitador)
        {
            var Capacitador = await dbContext.Capacitadors.FirstOrDefaultAsync(e => e.IdCapacitador == idCapacitador);
            dbContext.Capacitadors.Remove(Capacitador);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, Capacitador);
        }
    }
}
