using JwtTokenAPI.Data;
using JwtTokenAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtTokenAPI.Services
{
    public class AuthorisationServices
    {

        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly LoginResponse? _loginResponse;

        /***Constructor***/
        public AuthorisationServices(IConfiguration config, UserManager<User> userManager, ApplicationDbContext context)
        {
            _config = config;
            _userManager = userManager;
            _context = context;
        }

        /***Login Services Using the Authenticate Methode and generate both tokens we need***/
        [AllowAnonymous]
        [HttpPost]
        public async Task<LoginResponse?> Login(UserLogin userLogin)
        {
            var user = await Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);
                var refreshToken = GenerateRefreshToken();

                _loginResponse!.IsLoggedIn = true;
                _loginResponse.RefreshToken = refreshToken;
                _loginResponse.MyJwtToken = await token;

                user.RefreshToken = refreshToken.Token;
                user.RefreshTokenExpiry = refreshToken.Expired.ToUniversalTime();
                await _userManager.UpdateAsync(user);
                return _loginResponse;
            }
            return null;
        }

        /***Authenticate the user***/
        public async Task<User?> Authenticate(UserLogin userLogin)
        {
            //var currentUser =  dao.Read().FirstOrDefault(a => a.UserName == userLogin.UserName && a.Password == userLogin.Password);
            var currentUser = await _context.Users.Where(a => a.UserName == userLogin.UserName).FirstOrDefaultAsync();

            if (currentUser != null && await _userManager.CheckPasswordAsync(currentUser, userLogin!.Password!))
            {
                return currentUser;
            }
            return null;
        }

        /***Generating the JwtToken***/
        public async Task<MyTokenModel> Generate(User user)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)  ,
                new Claim(ClaimTypes.GivenName, user.FirstName!),
                new Claim(ClaimTypes.Surname, user.LastName!),
                new Claim(ClaimTypes.Role, VerifyRole(user))
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"],
                claims, notBefore: DateTime.UtcNow, expires: DateTime.UtcNow.AddSeconds(40), signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);


            await _userManager.SetAuthenticationTokenAsync(user, "MyWebTest", "MyJwtToken", tokenString);

            return new MyTokenModel
            {
                Token = tokenString,
                Expiry = token.ValidTo
            };
        }

        /***Checking the role of the user***/
        public string VerifyRole(User user)
        {
            var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id);
            if (userRole != null)
            {
                var roleName = _context.Roles.Where(r => r.Id == userRole.RoleId).Select(r => r.Name).FirstOrDefault();

                if (!string.IsNullOrEmpty(roleName))
                {
                    return roleName;
                }
                else
                {
                    return "Unknown";
                }
            }
            else
            {
                return "No Role";
            }
        }

        /***Need Work it not on point yet***/
        [HttpPost]
        public async Task<LoginResponse> RefreshToken(RefreshTokensModels model)
        {
            var principale = GetTokenPrincipale(model.JwtToken);
            var response = new LoginResponse();
            var currentUser = await _userManager.FindByNameAsync(principale!.Identity!.Name!);

            if (principale?.Identity?.Name is null)
            {
                return response;
            }
            /*else if (currentUser != null && currentUser.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return response;
            }*/

            else if (currentUser is null || currentUser.RefreshToken != model.RefreshToken || currentUser.RefreshTokenExpiry > DateTime.UtcNow)
            {
                _loginResponse!.IsLoggedIn = true;
                _loginResponse.RefreshToken = null;
                _loginResponse.MyJwtToken = await Generate(currentUser!);
                await _userManager.UpdateAsync(currentUser!);
                return _loginResponse;
            }
            else
            {
                var token = Generate(currentUser!);
                var refreshToken = GenerateRefreshToken();
                _loginResponse!.IsLoggedIn = true;
                _loginResponse.RefreshToken = refreshToken;
                _loginResponse.MyJwtToken = await token;
                currentUser!.RefreshToken = refreshToken.Token;
                currentUser.RefreshTokenExpiry = refreshToken.Expired.ToUniversalTime();
                await _userManager.UpdateAsync(currentUser);
                return _loginResponse;
            }
        }

        /***Making sure the Jwt have a time to 0 in the principale with the ClockSkew***/
        private ClaimsPrincipal? GetTokenPrincipale(string? jwtToken)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var validation = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = securityKey
            };

            return new JwtSecurityTokenHandler().ValidateToken(jwtToken, validation, out _);
        }

        /***Generating the refresh token***/
        public RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expired = DateTime.Now.AddSeconds(90)
            };

            return refreshToken;
        }
    }
}

