using CVSante.Models;
using Microsoft.AspNetCore.Identity;

namespace CVSante.ViewModels
{
    internal class ManageCompanyRoles
    {
        public List<CompanyRole> Roles { get; set; } = new List<CompanyRole>();
        public CompanyRole SelectedRole { get; set; } = new CompanyRole();
        public  UserParamedic userParamedic { get; set; } = new UserParamedic();
    }
}