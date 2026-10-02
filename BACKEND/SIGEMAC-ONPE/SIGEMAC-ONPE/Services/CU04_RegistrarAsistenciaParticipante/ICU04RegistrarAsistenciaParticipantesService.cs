namespace SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante
{
    public interface ICU04RegistrarAsistenciaParticipanteService
    {
        Task<bool> RegistrarAsistenciaAsync(string dniMiembro, int idSesion, bool asistio);
    }
}