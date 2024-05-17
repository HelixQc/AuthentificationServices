using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtTokenAPI.Models
{
    public class User : IdentityUser
    {

        /***Propreties***/
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }

        /***Empty***/
        public User() { }

        /***This constructor was only to test an in memory repository***/
        /***The Final version wont need this, please delete it!!!***/
        public User(string username, string password, string firstname, string lastname, string email, string phone, string role)
        {
            UserName = username;
            PasswordHash = password;
            FirstName = firstname;
            LastName = lastname;
            Email = email;
            PhoneNumber = phone;
        }
    }
}
