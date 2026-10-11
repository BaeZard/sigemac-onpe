namespace SIGEMAC_ONPE.Services.CU04_RegistrarAsistenciaParticipante;

public sealed class CU04_RegistroAsistenciaRequest
{
    public List<CU04_RegistroAsistenciaDTO>? Registros { get; set; }
}

public sealed class CU04_RegistroAsistenciaDTO
{
    public string Dni { get; set; } = string.Empty;
    public bool Asistio { get; set; }
}

public enum CU04_ResultadoRegistro
{
    Registrado,
    SolicitudInvalida,
    SesionNoEncontrada,
    ParticipanteNoAsignado
}
