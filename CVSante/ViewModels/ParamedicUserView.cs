using CVSante.Models;

namespace CVSante.ViewModels
{
    public class ParamedicUserView
    {
        public UserInfo UserInfo { get; set; }
        public List<UserAdresse> Addresses { get; set; }
        public List<UserAllergy> Allergies { get; set; }
        public UserAntecedent Antecedent { get; set; }
        public List<UserMedication> Medications { get; set; }
        public List<UserHandicap> Handicaps { get; set; }
        public List<Commentaire> Commentaires { get; set; }
        public int CurrentUserParamId { get; set; }
    }
}