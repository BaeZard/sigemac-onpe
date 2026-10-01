using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;

namespace SIGEMAC_ONPE.Services.CU01_ConsultarAsignacionLocal
{
    public class CU01ConsultarAsignacionService : ICU01ConsultarAsignacionService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU01ConsultarAsignacionService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<object?> ConsultarAsignacionPorDniAsync(string dni)
        {
            var miembro = await _dbContext.MiembroMesas
                .Include(m => m.IdOdpeNavigation)
                .FirstOrDefaultAsync(m => m.Dni == dni);

            if (miembro == null) return null;

            return new
            {
                dni = miembro.Dni,
                nombre = $"{miembro.Nombres} {miembro.Apellidos}",
                nombres = miembro.Nombres,
                apellidos = miembro.Apellidos,
                cargo = miembro.Cargo,
                estadoCapacitacion = miembro.EstadoCapacitacion,
                local = miembro.IdOdpeNavigation?.Direccion ?? "Sede Principal ODPE",
                region = miembro.IdOdpeNavigation?.Region ?? "Lima",
                sesionId = 1
            };
        }
    }
}