namespace JwtTokenAPI.Models
{
    public class MyTokenModel
    {
        public string? Token { get; set; }
        public DateTime Expiry { get; set; }
    }
}
