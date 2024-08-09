using CVSante.Models;

namespace CVSante.ViewModels
{
    public class ManageCompany
    {
        public Company Company { get; set; }
        public List<UserParamedic> Paramedics { get; set; }
    }
}