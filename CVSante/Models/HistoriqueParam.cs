using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class HistoriqueParam
{
    public int FkUserId { get; set; }

    public int FkParamId { get; set; }

    public DateTime Date { get; set; }

    public string Action { get; set; } = null!;

    public virtual UserParamedic FkParam { get; set; } = null!;

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
