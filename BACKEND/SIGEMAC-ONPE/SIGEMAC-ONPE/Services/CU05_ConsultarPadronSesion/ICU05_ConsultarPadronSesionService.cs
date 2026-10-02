using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;

namespace SIGEMAC_ONPE.Services.CU05_ConsultarPadronSesion
{
    public class CU05ConsultarPadronSesionService : ICU05ConsultarPadronSesionService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU05ConsultarPadronSesionService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<object>> ObtenerSesionesConPadronAsync(string? dniCapacitador = null)
        {
            var query = _dbContext.SesionCapacitacions
                .Include(s => s.IdCapacitadorNavigation)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .Include(s => s.Asistencia)
                    .ThenInclude(a => a.DniMiembroMesaNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dniCapacitador))
            {
                query = query.Where(s => s.IdCapacitadorNavigation.IdUsuarioNavigation.Username == dniCapacitador);
            }

            var sesiones = await query.ToListAsync();

            return sesiones.Select(s => new
            {
                id = s.IdSesion,
                idSesion = s.IdSesion,
                sede = s.Sede,
                direccion = s.Direccion,
                fecha = s.FechaHora.ToString("yyyy-MM-dd"),
                hora = s.FechaHora.ToString("HH:mm"),
                fechaHora = s.FechaHora,
                modalidad = s.Modalidad,
                idCapacitador = s.IdCapacitador,
                capacitador = s.IdCapacitadorNavigation?.IdUsuarioNavigation?.Username ?? string.Empty,
                materialIds = new List<int>(),
                padronMiembros = s.Asistencia.Select(a => new
                {
                    dni = a.DniMiembroMesa,
                    nombres = a.DniMiembroMesaNavigation != null ? a.DniMiembroMesaNavigation.Nombres : string.Empty,
                    apellidos = a.DniMiembroMesaNavigation != null ? a.DniMiembroMesaNavigation.Apellidos : string.Empty,
                    cargo = a.DniMiembroMesaNavigation != null ? a.DniMiembroMesaNavigation.Cargo : string.Empty,
                    asistio = a.Asistio
                })
            });
        }
    }
}