using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class Asistencium
{
    public int IdAsistencia { get; set; }

    public string DniMiembroMesa { get; set; } = null!;

    public int IdSesion { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool Asistio { get; set; }

    public string? Observacion { get; set; }

    public virtual MiembroMesa DniMiembroMesaNavigation { get; set; } = null!;

    public virtual SesionCapacitacion IdSesionNavigation { get; set; } = null!;
}
