using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class CompanyRole
{
    public int IdRole { get; set; }

    public string RoleName { get; set; } = null!;

    public bool CreateParamedic { get; set; }

    public bool EditParamedic { get; set; }

    public bool GetHistorique { get; set; }

    public bool GetCitoyen { get; set; }

    public bool EditRole { get; set; }

    public int FkCompany { get; set; }

    public virtual Company FkCompanyNavigation { get; set; } = null!;

    public virtual ICollection<UserParamedic> FkParams { get; set; } = new List<UserParamedic>();
}
