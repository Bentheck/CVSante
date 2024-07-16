using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class UserCitoyen
{
    public int UserId { get; set; }

    public string? FkIdentityUser { get; set; }

    public virtual ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

    public virtual AspNetUser? FkIdentityUserNavigation { get; set; }
}
