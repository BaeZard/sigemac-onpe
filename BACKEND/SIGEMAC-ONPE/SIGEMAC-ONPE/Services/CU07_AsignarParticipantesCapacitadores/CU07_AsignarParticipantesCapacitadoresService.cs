using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Models;

namespace SIGEMAC_ONPE.Services.CU07_AsignarParticipantesCapacitadores;

public sealed class CU07_AsignarParticipantesCapacitadoresService : ICU07_AsignarParticipantesCapacitadoresService
{
    private static readonly Regex DniPattern = new("^\\d{8}$", RegexOptions.Compiled);
    private readonly SigemacOnpeContext _dbContext;

    public CU07_AsignarParticipantesCapacitadoresService(SigemacOnpeContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CU07_ResultadoAsignacion> AsignarParticipanteAsync(string dni, int sesionId)
    {
        dni = dni?.Trim() ?? string.Empty;
        if (!DniPattern.IsMatch(dni) || sesionId <= 0)
        {
            return CU07_ResultadoAsignacion.SolicitudInvalida;
        }

        var miembroExiste = await _dbContext.MiembroMesas.AnyAsync(m => m.Dni == dni);
        if (!miembroExiste)
        {
            return CU07_ResultadoAsignacion.MiembroNoEncontrado;
        }

        var sesionExiste = await _dbContext.SesionCapacitacions.AnyAsync(s => s.IdSesion == sesionId);
        if (!sesionExiste)
        {
            return CU07_ResultadoAsignacion.SesionNoEncontrada;
        }

        var yaAsignado = await _dbContext.Asistencia.AnyAsync(a =>
            a.DniMiembroMesa == dni && a.IdSesion == sesionId);
        if (yaAsignado)
        {
            return CU07_ResultadoAsignacion.YaAsignado;
        }

        var asignacion = new Asistencium
        {
            DniMiembroMesa = dni,
            IdSesion = sesionId,
            Asistio = false,
            FechaRegistro = DateTime.Now
        };

        _dbContext.Asistencia.Add(asignacion);

        try
        {
            await _dbContext.SaveChangesAsync();
            return CU07_ResultadoAsignacion.Exitoso;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(asignacion).State = EntityState.Detached;

            // La restricción única también protege frente a solicitudes concurrentes.
            if (await _dbContext.Asistencia.AnyAsync(a =>
                    a.DniMiembroMesa == dni && a.IdSesion == sesionId))
            {
                return CU07_ResultadoAsignacion.YaAsignado;
            }

            throw;
        }
    }

    public async Task<CU07_ResultadoAsignacion> AsignarCapacitadorAsync(int sesionId, string dni)
    {
        dni = dni?.Trim() ?? string.Empty;
        if (!DniPattern.IsMatch(dni) || sesionId <= 0)
        {
            return CU07_ResultadoAsignacion.SolicitudInvalida;
        }

        var sesion = await _dbContext.SesionCapacitacions.FindAsync(sesionId);
        if (sesion is null)
        {
            return CU07_ResultadoAsignacion.SesionNoEncontrada;
        }

        var capacitador = await _dbContext.Capacitadors
            .Include(c => c.IdUsuarioNavigation)
            .FirstOrDefaultAsync(c =>
                c.IdUsuarioNavigation.Username == dni &&
                c.IdUsuarioNavigation.Rol.ToLower() == "capacitador");

        if (capacitador is null)
        {
            return CU07_ResultadoAsignacion.CapacitadorNoEncontrado;
        }

        sesion.IdCapacitador = capacitador.IdCapacitador;
        await _dbContext.SaveChangesAsync();

        return CU07_ResultadoAsignacion.Exitoso;
    }

    public async Task<IReadOnlyList<CU07_MiembroAsignadoDTO>> ObtenerParticipantesPorSesionAsync(int sesionId)
    {
        return await _dbContext.Asistencia
            .AsNoTracking()
            .Where(a => a.IdSesion == sesionId)
            .OrderBy(a => a.DniMiembroMesa)
            .Select(a => new CU07_MiembroAsignadoDTO
            {
                Dni = a.DniMiembroMesa,
                Nombre = (a.DniMiembroMesaNavigation.Nombres + " " + a.DniMiembroMesaNavigation.Apellidos).Trim(),
                Asistio = a.Asistio,
                SesionId = a.IdSesion
            })
            .ToListAsync();
    }
}
