using Elfie.Serialization;
using Microsoft.AspNetCore.Identity;
using CVSante.Models;
using System.ComponentModel.DataAnnotations;

namespace CVSante.ViewModels
{
    public class ASPUserRolesAndEdit
    {
        public List<ASPUserRoles> Users { get; set; }
        public EditASPUserRoles EditUserRoles { get; set; }
        public List<AspNetRole> AllRoles { get; set; }
    }

    public class EditASPUserRoles
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<AspNetRole> AllRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }

    public class ASPUserRoles
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }

}