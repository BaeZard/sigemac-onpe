using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;
using SIGEMAC_ONPE.Models;

namespace SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante
{
    public class CU04RegistrarAsistenciaParticipanteService : ICU04RegistrarAsistenciaParticipanteService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU04RegistrarAsistenciaParticipanteService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> RegistrarAsistenciaAsync(string dniMiembro, int idSesion, bool asistio)
        {
            try
            {
                var registroExistente = await _dbContext.Set<Asistencium>()
                    .FirstOrDefaultAsync(a => a.DniMiembroMesa == dniMiembro && a.IdSesion == idSesion);

                if (registroExistente != null)
                {
                    registroExistente.Asistio = asistio;
                    registroExistente.FechaRegistro = DateTime.Now;
                    _dbContext.Set<Asistencium>().Update(registroExistente);
                }
                else
                {
                    var nuevaAsistencia = new Asistencium
                    {
                        DniMiembroMesa = dniMiembro,
                        IdSesion = idSesion,
                        Asistio = asistio,
                        FechaRegistro = DateTime.Now
                    };
                    await _dbContext.Set<Asistencium>().AddAsync(nuevaAsistencia);
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}