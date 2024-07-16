using CVSante.Models;

namespace CVSante.ViewModels
{
    internal class User
    {
        public UserInfo userInfo { get; set; }
        public UserAdresse adresse { get; set; }
        public IEnumerable<UserAllergy> allergies { get; set; }
        public IEnumerable<UserAntecedent> antecedents { get; set; }
        public IEnumerable<UserMedication> medications { get; set; }
        public IEnumerable<UserHandicap> handicaps { get; set; }
    }
}