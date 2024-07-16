using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserCitoyen
{
    public int UserId { get; set; }

    public string? FkIdentityUser { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual AspNetUser? FkIdentityUserNavigation { get; set; }

    public virtual UserAdresse? UserAdresse { get; set; }

    public virtual UserAllergy? UserAllergy { get; set; }

    public virtual UserAntecedent? UserAntecedent { get; set; }

    public virtual UserHandicap? UserHandicap { get; set; }

    public virtual UserInfo? UserInfo { get; set; }

    public virtual UserMedication? UserMedication { get; set; }
}
