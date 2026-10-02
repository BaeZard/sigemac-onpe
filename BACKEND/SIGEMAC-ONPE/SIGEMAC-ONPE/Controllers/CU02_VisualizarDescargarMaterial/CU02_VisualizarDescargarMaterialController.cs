using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIGEMAC_ONPE.Services.CU02_VisualizarDescargarMaterial;

namespace SIGEMAC_ONPE.Controllers.CU02_VisualizarDescargarMaterial
{
    [Route("api/cus02")]
    [ApiController]
    public class CU02Controller : ControllerBase
    {
        private readonly ICU02VisualizarDescargarMaterialService _service;

        public CU02Controller(ICU02VisualizarDescargarMaterialService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMateriales()
        {
            var materiales = await _service.ObtenerMaterialesAsync();
            return StatusCode(StatusCodes.Status200OK, materiales);
        }
    }
}