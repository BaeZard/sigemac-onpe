namespace SIGEMAC_ONPE.Services.CU07_AsignarParticipantesCapacitadores;

public interface ICU07_AsignarParticipantesCapacitadoresService
{
    Task<CU07_ResultadoAsignacion> AsignarParticipanteAsync(string dni, int sesionId);
    Task<CU07_ResultadoAsignacion> AsignarCapacitadorAsync(int sesionId, string dni);
    Task<IReadOnlyList<CU07_MiembroAsignadoDTO>> ObtenerParticipantesPorSesionAsync(int sesionId);
}
