using System.Collections.Generic;

namespace SIGEMAC_ONPE.Services.CU06_GestionarSesionesCapacitacion
{
    public class SesionCapacitacionDTO
    {
        public int? IdSesion { get; set; }
        public string? Sede { get; set; }
        public string? Direccion { get; set; }
        public string? Fecha { get; set; }      // Viene del input type="date" del Front
        public string? Hora { get; set; }       // Viene del input type="time" del Front
        public string? Modalidad { get; set; }
        public string? Capacitador { get; set; } // DNI recibido al asignar capacitador
        public List<int>? MaterialIds { get; set; }
    }
}