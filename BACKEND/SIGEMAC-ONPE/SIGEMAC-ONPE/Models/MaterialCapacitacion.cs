using System;
using System.Collections.Generic;

namespace SIGEMAC_ONPE.Models;

public partial class MaterialCapacitacion
{
    public int IdMaterial { get; set; }

    public string Titulo { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public string UrlRecurso { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<SesionMaterial> SesionMaterials { get; set; } = new List<SesionMaterial>();
}
