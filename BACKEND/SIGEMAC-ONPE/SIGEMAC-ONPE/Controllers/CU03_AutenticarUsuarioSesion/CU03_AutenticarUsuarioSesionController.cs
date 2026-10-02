using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU03_AutenticarUsuario;

namespace SIGEMAC_ONPE.Controllers.CU03_AutenticarUsuario
{
    // Esta ruta hace match perfecto con tu config.js: 'http://localhost:5130/api/Auth'
    [Route("api/Auth")]
    [ApiController]
    public class CU03Controller : ControllerBase
    {
        private readonly ICU03AutenticarUsuarioService _service;

        public CU03Controller(ICU03AutenticarUsuarioService service)
        {
            _service = service;
        }

        // Definimos la estructura exacta que manda tu JavaScript en AuthRepository.login()
        public class LoginRequest
        {
            public string rol { get; set; } = string.Empty;
            public string dni { get; set; } = string.Empty;
            public string password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Ejecutamos el caso de uso
            var resultado = await _service.AutenticarAsync(request.rol, request.dni, request.password);

            if (resultado == null)
            {
                // Si el servicio devuelve null, disparamos el error 401 que tu ApiClient.js captura
                return Unauthorized(new { message = "Credenciales incorrectas o rol no válido." });
            }

            // Si todo está bien, devolvemos el usuario (código HTTP 200 OK)
            return Ok(resultado);
        }
    }
}