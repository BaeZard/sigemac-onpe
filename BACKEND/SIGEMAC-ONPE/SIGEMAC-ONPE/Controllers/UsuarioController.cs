using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public UsuarioController(SigemacOnpeContext _dbContext)
        {
            dbContext = _dbContext;
        }

        [HttpGet]
        [Route("a")]
        public async Task<IActionResult> Get()
        {
            var lista = await dbContext.Usuarios.ToListAsync();
            return StatusCode(StatusCodes.Status200OK, lista);
        }

        [HttpGet]
        [Route("Obtener/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            return StatusCode(StatusCodes.Status200OK, usuario);
        }

        [HttpPost]
        [Route("Nuevo")]
        public async Task<IActionResult> Nuevo([FromBody] Usuario objeto)
        {
            await dbContext.Usuarios.AddAsync(objeto);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpPost]
        [Route("Editar")]
        public async Task<IActionResult> Editar([FromBody] Usuario objeto)
        {
            dbContext.Usuarios.Update(objeto);
            await dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
        }

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await dbContext.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario != null)
            {
                dbContext.Usuarios.Remove(usuario);
                await dbContext.SaveChangesAsync();
            }
            return StatusCode(StatusCodes.Status200OK, usuario);
        }
    }
}