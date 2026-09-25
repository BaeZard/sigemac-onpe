using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Models;
using Microsoft.EntityFrameworkCore;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SigemacOnpeContext dbContext;

        public AuthController(SigemacOnpeContext context)
        {
            dbContext = context;
        }

        // POST: api/Auth/login (o api/Auth según tu repositorio)
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Buscamos al usuario validando el DNI (guardado en Username), el rol y que esté activo
            var usuario = await dbContext.Usuarios
                .FirstOrDefaultAsync(u => u.Username == request.Dni &&
                                          u.Rol.ToLower() == request.Rol.ToLower() &&
                                          u.Estado == true);

            if (usuario == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "No se encontró el registro." });
            }

            // Validamos la contraseña (en tu base de datos usamos 'hash_12345')
            if (usuario.PasswordHash != request.Password)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { mensaje = "Contraseña incorrecta." });
            }

            // Devolvemos el objeto de sesión que el frontend espera guardar
            var resultado = new
            {
                dni = usuario.Username,
                rol = usuario.Rol,
                nombre = "Personal Autorizado ONPE",
                token = "real-jwt-token"
            };

            return StatusCode(StatusCodes.Status200OK, resultado);
        }
    }

    // DTO para recibir los datos enviados por el frontend
    public class LoginRequestDto
    {
        public string Rol { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}