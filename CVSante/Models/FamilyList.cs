using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class FamilyList
{
    public int FamilyId { get; set; }

    public string FamilyName { get; set; } = null!;

    public virtual ICollection<UserFamily> UserFamilies { get; set; } = new List<UserFamily>();
}
