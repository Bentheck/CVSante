using CVSante.Models;

namespace CVSante.ViewModels
{
    public class EmailGroup
    {
        public string Email { get; set; }
        public List<FAQ> Tickets { get; set; } = new List<FAQ>();
        public List<FaqCommentaires> Comments { get; set; } = new List<FaqCommentaires>();
    }
}