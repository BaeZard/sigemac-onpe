namespace SIGEMAC_ONPE.Services.CU05_ConsultarPadronSesion
{
    public interface ICU05ConsultarPadronSesionService
    {
        Task<IEnumerable<object>> ObtenerSesionesConPadronAsync(string? dniCapacitador = null);
    }
}