using JwtTokenAPI.Data;
using JwtTokenAPI.Models;
using JwtTokenAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETCore.MailKit.Core;
using System.Web;



namespace JwtTokenAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TokenAPIController : ControllerBase
    {
        //Delete the dao on full release this dao is only for testing with some in memory data
        //private readonly IUserDAO _dao;
        private readonly AuthorisationServices _authServices;
        private readonly UserManager<User> _userManager;
        private readonly IEmailservice _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<TokenAPIController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly LoginResponse? _loginResponse;

        /***Injecting the services we need in the constructor***/
        /***Constructor***/
        public TokenAPIController(ApplicationDbContext context, IConfiguration config, UserManager<User> userManager, IEmailservice emailService, ILogger<TokenAPIController> logger, AuthorisationServices authServices)
        {
            _config = config;
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            var currentUser = await _authServices.Login(userLogin);
            if (currentUser != null)
            {
                SetRefreshToken(currentUser.RefreshToken!);
                return Ok(currentUser);
            }
            return BadRequest("Username or your password is incorrect");
        }


        /***LogOut of the application is deleting all the Tokens of the data base make sure to delete 
         * it on the client side as well to officially disconnect of the web app***/
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LogOut()
        {
            var userTokensManager = _userManager;
            var userName = HttpContext.User!.Identity!.Name!;
            var myUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (myUser == null)
            {
                return BadRequest("User not found.");
            }

            await userTokensManager.RemoveAuthenticationTokenAsync(myUser, "MyWebTest", "MyJwtToken");
            myUser.RefreshToken = null;
            myUser.RefreshTokenExpiry = DateTime.UtcNow;
            await _userManager.UpdateAsync(myUser);
            await _context.SaveChangesAsync();
            return Ok("Token revoked successfully.");
        }

        /***Registering a new user in the database***/
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] User user)
        {

            try
            {
                var users = await _context.Users.ToListAsync();


                // Check if the email is already in use
                if (await _userManager.FindByEmailAsync(user.Email!) != null || await _userManager.FindByNameAsync(user.UserName!) != null)
                {
                    return BadRequest("Email or Username is already used.");
                }



                if (await _userManager.FindByEmailAsync(user.Email!) == null)
                {

                    user.PhoneNumberConfirmed = true;
                    //On full release change this to 3!!!
                    user.AccessFailedCount = 13;
                    user.TwoFactorEnabled = true;
                    user.LockoutEnabled = true;

                    var result = await _userManager.CreateAsync(user, user.PasswordHash!);

                    if (result.Succeeded)
                    {
                        if (users.Count == 0)
                        {
                            await _userManager.AddToRoleAsync(user, "admin");
                        }
                        else
                        {
                            //role by default is member we will need a GUI to set other admin manually in the web app
                            await _userManager.AddToRoleAsync(user, "member");
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return StatusCode(500, "Seems like you incurred an error while creating a new user");

                    }
                }
                VerifyEmail(user);
                return Ok("User have been added with success");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during registration
                _logger.LogError(ex, "Error occurred while registering the user.");
                return StatusCode(500, "An error occurred while registering the user.");
            }
        }

        /***Docu***/
        /***Need work im not sure the RefreshToken work correctly***/
        [HttpPost]
        public async Task<IActionResult> RefreshTokens(RefreshTokensModels refreshTokenModel)
        {
            var currentToken = await _authServices.RefreshToken(refreshTokenModel);
            if (currentToken.RefreshToken == null)
            {
                return Ok(currentToken);
            }
            SetRefreshToken(currentToken.RefreshToken!);
            return Ok(currentToken);
        }

        /***Confirming the email address of a new user***/
        /***For Testing keep user.Email = "yourEmail@example.com" ***/
        /***For Production delete the  line user.Email = "yourEmail@example.com"***/
        /***Need Work Here***/
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(User user)
        {
            //Generate a link to change the email confirmation to true in the data base with the token provide in Identity framework
            //some how the token is always invalide
            var email = user.Email;
            user.Email = "langis_gaby@hotmail.com";
            var currentUser = await _userManager.FindByEmailAsync(email!);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await Console.Out.WriteLineAsync(_userManager.Options.Tokens.EmailConfirmationTokenProvider.ToString());
            var confirmEmailURL = Url.Action(nameof(ConfirmUserEmail), "TokenAPI", new { userId = currentUser!.Id, code = token }, Request.Scheme);
            var message = new Message([email!], "Test", $"<h1>Still a test</h1><br><a href='{confirmEmailURL!}'>Confirm!!!</a>");
            _emailService.SendEmail(message);
            return Ok("Email sent SuccessFully");
        }

        /***Recovering the confirmationToken and set the emailConfirm to true and update the dataBase***/
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmUserEmail(string code, string userId)
        {

            //the token have issue 
            var currentUser = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ConfirmEmailAsync(currentUser!, code);

            if (result.Succeeded)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /***Sending email to reset the password***/
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email address is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Ok("If an account with this email exists, a password reset link has been sent to it.");
            }

            try
            {

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(nameof(ResetPassword), "TokenAPI", new { token, email }, Request.Scheme);
                var message = new Message([email], "Reset your password", $"<h1>Please reset your password by clicking here:</h1><a href='{callbackUrl!}'>Reset Password</a>");
                _emailService.SendEmail(message);

                return Ok($"A password reset link has been sent to {email}. Please check your email.");
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error while generating or sending password reset token.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /***Docu***/
        /***Need Workd here the database does not update something go wrong***/
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            string newPassword = "Password123!";
            string confirmPassword = "Password123!";
            var model = new ResetPassword { Password = newPassword, ConfirmPassord = confirmPassword, Token = HttpUtility.UrlDecode(token), Email = email };
            await ResetMyPassword(model);
            return Ok(new { model });
        }

        /***Need some work when the callBackUrl Will work***/
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetMyPassword(ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user != null)
            {
                var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if (!resetPassResult.Succeeded)
                {
                    foreach (var error in resetPassResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }
                return Ok("Password have been reset succesfuly");
            }
            return BadRequest("Error while reseting the password");
        }

        /***Sending the refreshToken as a cookie to the user***/
        public void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expired
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
        }
    }
}