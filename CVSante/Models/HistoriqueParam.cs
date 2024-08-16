using CVSante.ViewModels;
using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class HistoriqueParam
{
    public int HistId { get; set; }  // Primary key
    public int? FkUserId { get; set; }  // Nullable foreign key
    public int FkParamId { get; set; }  // Foreign key (non-nullable)
    public string Action { get; set; }
    public DateTime Date { get; set; }

    // Navigation properties
    public virtual UserCitoyen FkUser { get; set; }
    public virtual UserParamedic FkParam { get; set; }
}
