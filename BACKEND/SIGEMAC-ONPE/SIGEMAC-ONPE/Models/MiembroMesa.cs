using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class MiembroMesa
{
    public string Dni { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Cargo { get; set; } = null!;

    public string EstadoCapacitacion { get; set; } = null!;

    public int IdOdpe { get; set; }

    public int? IdUsuario { get; set; }

    public virtual ICollection<Asistencium> Asistencia { get; set; } = new List<Asistencium>();

    public virtual Odpe IdOdpeNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
