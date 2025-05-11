namespace BigProject.Entities
{
    public class User : EntityBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string MaSV { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }

      
        public ICollection<RefreshToken> refreshTokens { get; set; }
        public ICollection<EmailConfirm> emailConfirms { get; set; }
    }
}
