using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class CompanyRole
{
    public int IdRole { get; set; }

    public string RoleName { get; set; } = null!;

    public string RoleDroits { get; set; } = null!;

    public virtual ICollection<UserParamedic> FkParams { get; set; } = new List<UserParamedic>();
}
