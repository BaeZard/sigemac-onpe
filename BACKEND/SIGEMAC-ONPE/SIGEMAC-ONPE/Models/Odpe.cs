using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class Odpe
{
    public int IdOdpe { get; set; }

    public string NombreOdpe { get; set; } = null!;

    public string Region { get; set; } = null!;

    public string? Direccion { get; set; }

    public virtual ICollection<MiembroMesa> MiembroMesas { get; set; } = new List<MiembroMesa>();
}
