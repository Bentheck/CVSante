using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserHandicap
{
    public int HanId { get; set; }

    public int FkUserId { get; set; }

    public string Definition { get; set; } = null!;

    public string Type { get; set; } = null!;

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
