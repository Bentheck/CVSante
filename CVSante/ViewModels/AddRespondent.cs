using CVSante.Models;

namespace CVSante.ViewModels
{
    public class AddRespondent
    {
        public UserParamedic UserParamedic { get; set; } = new UserParamedic();
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}