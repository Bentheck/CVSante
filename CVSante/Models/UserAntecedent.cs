using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserAntecedent
{
    public int AnId { get; set; }

    public int FkUserId { get; set; }

    public string Antecedent { get; set; } = null!;

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
