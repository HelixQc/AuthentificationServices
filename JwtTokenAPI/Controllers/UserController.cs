using JwtTokenAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtTokenAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : Controller
    {
        [HttpPost]
        [Authorize(Policy = "AdminAccess")]
        public IActionResult Admins()
        {
            var authorizationHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
            string jwtTokenString = authorizationHeader!.Replace("Bearer ", "");
            var jwt = new JwtSecurityToken(jwtTokenString);
            var response = $"Authenticated! {Environment.NewLine}";
            response += $"{Environment.NewLine}Exp Time : {jwt.ValidTo.ToLongTimeString()}, Time: {DateTime.UtcNow.ToLongTimeString()}";
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser!.FirstName}, you are an admin , here the response: {response}");
        }


        [HttpPost]
        [Authorize(Policy = "ModsAccess")]
        public IActionResult Mods()
        {
            var authorizationHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
            string jwtTokenString = authorizationHeader!.Replace("Bearer ", "");
            var jwt = new JwtSecurityToken(jwtTokenString);
            var response = $"Authenticated! {Environment.NewLine}";
            response += $"{Environment.NewLine}Exp Time : {jwt.ValidTo.ToLongTimeString()}, Time: {DateTime.UtcNow.ToLongTimeString()}";
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser!.FirstName}, you are a mod , here the response: {response}");
        }

        [HttpPost]
        [Authorize(Policy = "ModsAccess")]
        public IActionResult Member()
        {
            var authorizationHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
            string jwtTokenString = authorizationHeader!.Replace("Bearer ", "");
            var jwt = new JwtSecurityToken(jwtTokenString);
            var response = $"Authenticated! {Environment.NewLine}";
            response += $"{Environment.NewLine}Exp Time : {jwt.ValidTo.ToLongTimeString()}, Time: {DateTime.UtcNow.ToLongTimeString()}";
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser!.FirstName}, you are a member , here the response: {response}");
        }

        [HttpGet]
        public IActionResult Public()
        {
            return Ok("Hi, you're on pubic proprety");
        }

        private User? GetCurrentUser()
        {
            if (HttpContext.User?.Identity is ClaimsIdentity identity)
            {
                var userClaims = identity.Claims;

                return new User
                {
                    UserName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    FirstName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    LastName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value
                };
            }
            return null;
        }
    }
}