using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Models;

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
                .Include(m => m.Asistencia)
                    .ThenInclude(a => a.IdSesionNavigation)
                .FirstOrDefaultAsync(m => m.Dni == dni);

            if (miembro == null) return null;

            return MapearMiembro(miembro);
        }

        public async Task<IEnumerable<object>> ObtenerTodosLosMiembrosAsync()
        {
            var miembros = await _dbContext.MiembroMesas
                .Include(m => m.IdOdpeNavigation)
                .Include(m => m.Asistencia)
                    .ThenInclude(a => a.IdSesionNavigation)
                .ToListAsync();

            return miembros.Select(MapearMiembro);
        }

        private static object MapearMiembro(MiembroMesa miembro)
        {
            var asignacion = miembro.Asistencia
                .OrderByDescending(a => a.FechaRegistro)
                .FirstOrDefault();
            var sesion = asignacion?.IdSesionNavigation;

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
                sesionId = asignacion?.IdSesion,
                asistio = asignacion?.Asistio ?? false,
                sesion = sesion == null ? null : new
                {
                    id = sesion.IdSesion,
                    idSesion = sesion.IdSesion,
                    sede = sesion.Sede,
                    direccion = sesion.Direccion,
                    fecha = sesion.FechaHora.ToString("yyyy-MM-dd"),
                    hora = sesion.FechaHora.ToString("HH:mm"),
                    modalidad = sesion.Modalidad
                }
            };
        }
    }
}
