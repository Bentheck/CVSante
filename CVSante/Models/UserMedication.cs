using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserMedication
{
    public int FkUserId { get; set; }

    public string MedicamentProduitNat { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Posologie { get; set; } = null!;

    public string? Raison { get; set; }

    public virtual UserCitoyen FkUser { get; set; } = null!;
}
