namespace SIGEMAC_ONPE.Services.CU02_VisualizarDescargarMaterial
{
    public interface ICU02VisualizarDescargarMaterialService
    {
        Task<IEnumerable<object>> ObtenerMaterialesAsync();
    }
}