using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserFamily
{
    public int FkFamilyId { get; set; }

    public int FkUserId { get; set; }

    public string FamilyRole { get; set; } = null!;

    public bool IsFamilyAccount { get; set; }

    public virtual FamilyList FkFamily { get; set; } = null!;

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
