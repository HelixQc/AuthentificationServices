using System.ComponentModel.DataAnnotations;

namespace JwtTokenAPI.Models
{
    public class ResetPassword
    {

        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string Password { get; set; } = string.Empty;
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
        public string ConfirmPassord { get; set; } = null!;
    }
}
