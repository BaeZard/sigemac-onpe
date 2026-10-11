using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;

namespace SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante
{
    public class CU04RegistrarAsistenciaParticipanteService : ICU04RegistrarAsistenciaParticipanteService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU04RegistrarAsistenciaParticipanteService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CU04_ResultadoRegistro> RegistrarAsistenciasAsync(
            int idSesion,
            IReadOnlyCollection<CU04_RegistroAsistenciaDTO> registros)
        {
            if (idSesion <= 0 || registros is null || registros.Count == 0)
            {
                return CU04_ResultadoRegistro.SolicitudInvalida;
            }

            var normalizados = registros
                .Select(r => new CU04_RegistroAsistenciaDTO
                {
                    Dni = r.Dni?.Trim() ?? string.Empty,
                    Asistio = r.Asistio
                })
                .ToList();

            if (normalizados.Any(r => r.Dni.Length != 8 || !r.Dni.All(char.IsAsciiDigit)) ||
                normalizados.Select(r => r.Dni).Distinct(StringComparer.Ordinal).Count() != normalizados.Count)
            {
                return CU04_ResultadoRegistro.SolicitudInvalida;
            }

            var sesionExiste = await _dbContext.SesionCapacitacions.AnyAsync(s => s.IdSesion == idSesion);
            if (!sesionExiste)
            {
                return CU04_ResultadoRegistro.SesionNoEncontrada;
            }

            var dnis = normalizados.Select(r => r.Dni).ToList();
            var asistencias = await _dbContext.Asistencia
                .Where(a => a.IdSesion == idSesion && dnis.Contains(a.DniMiembroMesa))
                .ToListAsync();

            if (asistencias.Count != dnis.Count)
            {
                return CU04_ResultadoRegistro.ParticipanteNoAsignado;
            }

            var estadoPorDni = normalizados.ToDictionary(r => r.Dni, r => r.Asistio, StringComparer.Ordinal);
            foreach (var asistencia in asistencias)
            {
                asistencia.Asistio = estadoPorDni[asistencia.DniMiembroMesa];
                asistencia.FechaRegistro = DateTime.Now;
            }

            await _dbContext.SaveChangesAsync();
            return CU04_ResultadoRegistro.Registrado;
        }
    }
}
