using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DL6000WebConfig.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest model)
        {
            
            if (_userService.ValidateUser(model.Username, model.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                try
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    Console.WriteLine($"Cookies: {CookieAuthenticationDefaults.AuthenticationScheme}");
                    Console.WriteLine($"Principal: {principal}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Erro ao criar cookie: {ex.Message}");
                    throw;
                }

                return Redirect("/monitor");
            }

            return Unauthorized(new { Success = false, Message = "Credenciais inválidas" });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

}