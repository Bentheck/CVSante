using CVSante.ViewModels;
using System;
using System.Collections.Generic;

namespace CVSante.Models;

public partial class HistoriqueParam
{
    public int HistId { get; set; } 
    public int? FkUserId { get; set; } 
    public int FkParamId { get; set; } 
    public string Action { get; set; }
    public DateTime Date { get; set; }

 
    public virtual UserCitoyen FkUser { get; set; }
    public virtual UserParamedic FkParam { get; set; }
}
