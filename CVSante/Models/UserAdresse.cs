using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserAdresse
{
    public int FkUserId { get; set; }

    public string Ville { get; set; } = null!;

    public string CodePostal { get; set; } = null!;

    public string NumCivic { get; set; } = null!;

    public string Rue { get; set; } = null!;

    public string? Appartement { get; set; }

    public bool AdressePrimaire { get; set; }

    public string? TelphoneAdresse { get; set; }

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
