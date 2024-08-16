using CVSante.Models;

namespace CVSante.ViewModels
{
    public class Paramedic
    {
        public UserParamedic paramInfo { get; set; }
        public List<HistoriqueParam> historique { get; set; }
        public CompanyRole compRole { get; set; }
        public Company company { get; set; }
        public List<Commentaire> commentaires { get; set; }
    }
}