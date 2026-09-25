using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SIGEMAC_ONPE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentsController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            // Devuelve una lista vacía para que el frontend no dé error
            return Ok(new List<object>());
        }
    }
}