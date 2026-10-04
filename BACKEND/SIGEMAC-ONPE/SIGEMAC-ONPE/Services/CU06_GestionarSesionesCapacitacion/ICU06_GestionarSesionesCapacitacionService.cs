using System.Threading.Tasks;

namespace SIGEMAC_ONPE.Services.CU06_GestionarSesionesCapacitacion
{
    public interface ICU06_GestionarSesionesCapacitacionService
    {
        Task<bool> CrearSesionAsync(SesionCapacitacionDTO dto);
        Task<bool> ActualizarSesionAsync(int idSesion, SesionCapacitacionDTO dto);
        Task<bool> EliminarSesionAsync(int idSesion);
    }
}