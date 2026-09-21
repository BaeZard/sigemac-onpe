using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class Capacitador
{
    public int IdCapacitador { get; set; }

    public string CodigoCapacitador { get; set; } = null!;

    public string? Especialidad { get; set; }

    public int IdUsuario { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<SesionCapacitacion> SesionCapacitacions { get; set; } = new List<SesionCapacitacion>();
}
