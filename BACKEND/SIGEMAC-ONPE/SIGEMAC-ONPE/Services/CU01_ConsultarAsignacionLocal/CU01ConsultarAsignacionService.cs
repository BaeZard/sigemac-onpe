namespace SIGEMAC_ONPE.Services.CU01_ConsultarAsignacionLocal
{
    public interface ICU01ConsultarAsignacionService
    {
        Task<object?> ConsultarAsignacionPorDniAsync(string dni);
        Task<IEnumerable<object>> ObtenerTodosLosMiembrosAsync();
    }
}