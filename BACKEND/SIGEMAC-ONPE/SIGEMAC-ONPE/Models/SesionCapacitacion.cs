using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class SesionCapacitacion
{
    public int IdSesion { get; set; }

    public string Sede { get; set; } = null!;

    public string? Direccion { get; set; }

    public DateTime FechaHora { get; set; }

    public string Modalidad { get; set; } = null!;

    public int IdCapacitador { get; set; }

    public virtual ICollection<Asistencium> Asistencia { get; set; } = new List<Asistencium>();

    public virtual Capacitador IdCapacitadorNavigation { get; set; } = null!;

    public virtual ICollection<SesionMaterial> SesionMaterials { get; set; } = new List<SesionMaterial>();
}
