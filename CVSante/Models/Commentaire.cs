using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class Commentaire
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Comment { get; set; } = null!;

    public int FkUserparamedic { get; set; }

    public int FkUserId { get; set; }

    public virtual UserCitoyen FkUser { get; set; } = null!;

    public virtual UserParamedic FkUserparamedicNavigation { get; set; } = null!;
}
