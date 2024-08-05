using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserCitoyen
{
    public int UserId { get; set; }

    public string? FkIdentityUser { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual AspNetUser? FkIdentityUserNavigation { get; set; }

    public virtual ICollection<HistoriqueParam> HistoriqueParams { get; set; } = new List<HistoriqueParam>();

    public virtual ICollection<UserAdresse> UserAdresses { get; set; } = new List<UserAdresse>();

    public virtual ICollection<UserAllergy> UserAllergies { get; set; } = new List<UserAllergy>();

    public virtual ICollection<UserAntecedent> UserAntecedents { get; set; } = new List<UserAntecedent>();

    public virtual ICollection<UserFamily> UserFamilies { get; set; } = new List<UserFamily>();

    public virtual ICollection<UserHandicap> UserHandicaps { get; set; } = new List<UserHandicap>();

    public virtual UserInfo? UserInfo { get; set; }

    public virtual ICollection<UserMedication> UserMedications { get; set; } = new List<UserMedication>();
}
