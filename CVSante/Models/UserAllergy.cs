using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserAllergy
{
    public int FkUserId { get; set; }

    public string AllergieIntolerance { get; set; } = null!;

    public string? Gravite { get; set; }

    public string Produit { get; set; } = null!;

    public int AlId { get; set; }

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
