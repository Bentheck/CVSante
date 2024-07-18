using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserParamedic
{
    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Ville { get; set; } = null!;

    public string Telephone { get; set; } = null!;

    public int ParamId { get; set; }

    public bool ParamIsActive { get; set; }

    public string? Matricule { get; set; }

    public int? FkCompany { get; set; }

    public string? FkIdentityUser { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual Company? FkCompanyNavigation { get; set; }

    public virtual AspNetUser? FkIdentityUserNavigation { get; set; }

    public virtual ICollection<HistoriqueParam> HistoriqueParams { get; set; } = new List<HistoriqueParam>();

    public virtual ICollection<CompanyRole> FkRoles { get; set; } = new List<CompanyRole>();
}
