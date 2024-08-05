using CVSante.Models;

namespace CVSante.ViewModels
{
    public class User
    {
        public UserInfo UserInfo { get; set; } = null!;
        public List<UserAdresse> Addresses { get; set; } = null!;
        public List<UserAllergy>? Allergies { get; set; }
        public UserAntecedent? Antecedent { get; set; } = null!;
        public List<UserMedication>? Medications { get; set; }
        public List<UserHandicap>? Handicaps { get; set; }
    }
}