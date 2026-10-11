namespace SIGEMAC_ONPE.Services.CU07_AsignarParticipantesCapacitadores;

public sealed class CU07_AsignarDTO
{
    public int SesionId { get; init; }
}

public sealed class CU07_MiembroAsignadoDTO
{
    public string Dni { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public bool Asistio { get; init; }
    public int SesionId { get; init; }
}

public enum CU07_ResultadoAsignacion
{
    Exitoso,
    SolicitudInvalida,
    MiembroNoEncontrado,
    SesionNoEncontrada,
    CapacitadorNoEncontrado,
    YaAsignado
}
