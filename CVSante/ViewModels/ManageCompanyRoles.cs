using CVSante.Models;

namespace CVSante.ViewModels
{
    public class ManageCompanyRoles
    {
        public List<UserParamedic> Paramedics { get; set; }
        public UserParamedic SelectedParamedic { get; set; }
        public CompanyRole SelectedRole { get; set; }
        public int CompanyId { get; set; }
    }
}