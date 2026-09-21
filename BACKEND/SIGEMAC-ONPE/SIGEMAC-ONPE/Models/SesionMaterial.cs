using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class SesionMaterial
{
    public int IdSesion { get; set; }

    public int IdMaterial { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public virtual MaterialCapacitacion IdMaterialNavigation { get; set; } = null!;

    public virtual SesionCapacitacion IdSesionNavigation { get; set; } = null!;
}
