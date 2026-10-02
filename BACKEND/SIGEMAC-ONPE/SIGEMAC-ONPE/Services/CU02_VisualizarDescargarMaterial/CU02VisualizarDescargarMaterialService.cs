using Microsoft.EntityFrameworkCore;
using SIGEMAC_ONPE.Data;

namespace SIGEMAC_ONPE.Services.CU02_VisualizarDescargarMaterial
{
    public class CU02VisualizarDescargarMaterialService : ICU02VisualizarDescargarMaterialService
    {
        private readonly SigemacOnpeContext _dbContext;

        public CU02VisualizarDescargarMaterialService(SigemacOnpeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<object>> ObtenerMaterialesAsync()
        {
            var materiales = await _dbContext.MaterialCapacitacions.ToListAsync();

            // Aquí hacemos la magia: traducimos tu modelo al JSON del frontend
            return materiales.Select(m => new
            {
                id = m.IdMaterial,
                titulo = m.Titulo,
                tipo = m.Tipo,
                url = m.UrlRecurso, // Usamos tu columna original
                estado = m.Activo ? "Disponible" : "Agotado" // Traducimos el booleano a texto
            });
        }
    }
}