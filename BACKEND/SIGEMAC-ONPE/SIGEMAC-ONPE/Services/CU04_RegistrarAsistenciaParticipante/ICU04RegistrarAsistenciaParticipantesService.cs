namespace SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante
{
    public interface ICU04RegistrarAsistenciaParticipanteService
    {
        Task<CU04_ResultadoRegistro> RegistrarAsistenciasAsync(
            int idSesion,
            IReadOnlyCollection<CU04_RegistroAsistenciaDTO> registros);
    }
}
