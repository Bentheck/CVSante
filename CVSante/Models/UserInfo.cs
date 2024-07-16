using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserInfo
{
    public int FkUserId { get; set; }

    public string Sexe { get; set; } = null!;

    public string DateNaissance { get; set; } = null!;

    public string Poids { get; set; } = null!;

    public string Taille { get; set; } = null!;

    public string TypeSanguin { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string TelephoneCell { get; set; } = null!;

    public string? Pronoms { get; set; }

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
