using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserParamedic
{
    public int ParamId { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string Ville { get; set; } = null!;

    public string Telephone { get; set; } = null!;

    public bool ParamIsActive { get; set; }

    public string? Matricule { get; set; }

    public int? FkCompany { get; set; }

    public string FkIdentityUser { get; set; } = null!;

    public int FkRole { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual Company? FkCompanyNavigation { get; set; }

    public virtual AspNetUser FkIdentityUserNavigation { get; set; } = null!;

    public virtual CompanyRole FkRoleNavigation { get; set; } = null!;

    public virtual ICollection<HistoriqueParam> HistoriqueParams { get; set; } = new List<HistoriqueParam>();
}
