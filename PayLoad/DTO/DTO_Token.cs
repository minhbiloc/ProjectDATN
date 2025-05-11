namespace BigProject.PayLoad.DTO
{
    public class DTO_Token
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public bool requireActivation { get; set; }
        public string? Email { get; set; }
    }
}
