using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;

namespace SIGEMAC_ONPE.Services.CU03_AutenticarUsuario
{
    public class CU03AutenticarUsuarioService : ICU03AutenticarUsuarioService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU03AutenticarUsuarioService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object?> AutenticarAsync(string rol, string dni, string password)
        {
            // 1. Buscamos usando TUS nombres de propiedades y validamos que Estado sea true (activo)
            var usuario = await _dbContext.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Username == dni &&
                    u.Rol == rol &&
                    u.PasswordHash == password &&
                    u.Estado == true); // Solo dejamos entrar a usuarios activos

            // 2. Si no existe o la contraseña es incorrecta, devolvemos null
            if (usuario == null)
            {
                return null;
            }

            // 3. Armamos el objeto para el frontend
            return new
            {
                dni = usuario.Username,
                nombre = usuario.Username, // Temporalmente mandamos el Username como nombre, ya que no hay campo Nombre aquí
                rol = usuario.Rol,
                token = "token-jwt-simulado-sigemac-2026"
            };
        }
    }
}