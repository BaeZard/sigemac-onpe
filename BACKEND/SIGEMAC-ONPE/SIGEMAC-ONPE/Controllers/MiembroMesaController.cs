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
        [Route("{dni}")]
        public async Task<IActionResult> GetByDni(string dni)
        {
            var miembro = await dbContext.MiembroMesas
                .Include(m => m.IdOdpeNavigation)
                .FirstOrDefaultAsync(m => m.Dni == dni);

            if (miembro == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "No se encontró el registro." });
            }

            // Mapeamos los datos combinando la información de la tabla MiembroMesa y su ODPE relacionada
            var resultado = new
            {
                dni = miembro.Dni,
                nombre = $"{miembro.Nombres} {miembro.Apellidos}", // Formato unificado que espera el frontend
                nombres = miembro.Nombres,
                apellidos = miembro.Apellidos,
                cargo = miembro.Cargo,
                estadoCapacitacion = miembro.EstadoCapacitacion,
                local = miembro.IdOdpeNavigation?.Direccion ?? "Sede Principal ODPE",
                region = miembro.IdOdpeNavigation?.Region ?? "Lima",
                sesionId = 1 // Sesión predeterminada o asociada
            };

            return StatusCode(StatusCodes.Status200OK, resultado);
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