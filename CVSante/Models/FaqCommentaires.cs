namespace CVSante.Models
{
    public class FaqCommentaires
    {
        public int Id { get; set; }
        public int FK_FAQ_ID { get; set; }
        public string FK_ASP_ID { get; set; } = null!;
        public string Comentaire { get; set; } = null!;

        // Navigation properties
        public virtual FAQ? FAQNavigation { get; set; } = null!;
        public virtual AspNetUser? FkIdentityUserNavigation { get; set; } = null!;
    }
}
