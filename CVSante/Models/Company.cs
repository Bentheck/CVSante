using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class Company
{
    public int IdComp { get; set; }

    public string CompName { get; set; } = null!;

    public string? AdLink { get; set; }

    public string? AdId { get; set; }

    public virtual ICollection<CompanyRole> CompanyRoles { get; set; } = new List<CompanyRole>();

    public virtual ICollection<UserParamedic> UserParamedics { get; set; } = new List<UserParamedic>();
}
