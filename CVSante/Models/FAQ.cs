namespace CVSante.Models
{
    public class FAQ
    {
        public int Id { get; set; }
        public string Prenom { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string ville { get; set; } = null!;
        public string Courriel { get; set; } = null!;
        public string Question { get; set; } = null!;
        public bool IsNew { get; set;}
        public string Sujet { get; set; } = null!;

        public virtual ICollection<FaqCommentaires> FaqCommentaires { get; set; } = new HashSet<FaqCommentaires>();
    }
}
