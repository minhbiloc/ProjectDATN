namespace BigProject.Entities
{
    public class RefreshToken : EntityBase
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime Exprited { get; set; }
        public User? User { get; set; }
    }
}
