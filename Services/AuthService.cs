// Services/AuthService.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;

namespace DL6000WebConfig.Services
{
    public class AuthService : IAuthService
    {        
        private readonly IHttpContextAccessor _httpContextAccessor;
    
        public AuthService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            username = username?.Trim() ?? "";
            password = password?.Trim() ?? "";

            Console.WriteLine($"LoginAsync chamado com username='{username}' e password='{password}'");
            
            try
            {
                if (username == "admin" && password == "admin123")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "Administrator")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await _httpContextAccessor.HttpContext!.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                        });

                    return new AuthResult { Success = true };
                }

                return new AuthResult { Success = false, ErrorMessage = "Credenciais inválidas" };
            }
            catch (Exception)
            {
                return new AuthResult { Success = false, ErrorMessage = "Erro durante o login" };
            }
        }
    }
}