using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<Capacitador> Capacitadors { get; set; } = new List<Capacitador>();

    public virtual ICollection<MiembroMesa> MiembroMesas { get; set; } = new List<MiembroMesa>();
}
