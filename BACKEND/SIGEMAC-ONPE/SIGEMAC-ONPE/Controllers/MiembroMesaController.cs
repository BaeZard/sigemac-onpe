using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MiembroMesaController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public MiembroMesaController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        [HttpGet]
        [Route("a")]
        public async Task<IActionResult> Get()
        {
            var listaMiembros = await dbContext.MiembroMesas.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, listaMiembros);
        }

        [HttpGet]
        [Route("Obtener/{dni}")]
        /* Este es tu CUS-01: Consulta por DNI del ciudadano */
        public async Task<IActionResult> Get(string dni)
        {
            // El Include trae automáticamente los datos de la tabla ODPE asociada
            var miembro = await dbContext.MiembroMesas
                .Include(e => e.IdOdpeNavigation)
                .FirstOrDefaultAsync(e => e.Dni == dni);

            return StatusCode(StatusCodes.Status200OK, miembro);
        }

        [HttpPost]
        [Route("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] MiembroMesa objeto)
        {
            await dbContext.MiembroMesas.AddAsync(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPost]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] MiembroMesa objeto)
        {
            dbContext.MiembroMesas.Update(objeto);
            await dbContext.SaveChangesAsync();

            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpDelete]
        [Route("Eliminar/{dni}")]
        public async Task<IActionResult> Eliminar(string dni)
        {
            var miembro = await dbContext.MiembroMesas.FirstOrDefaultAsync(e => e.Dni == dni);

            if (miembro != null)
            {
                dbContext.MiembroMesas.Remove(miembro);
                await dbContext.SaveChangesAsync();
            }

            return StatusCode(StatusCodes.Status200OK, miembro);
        }
    }
}