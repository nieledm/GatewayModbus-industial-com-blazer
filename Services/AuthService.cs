// Services/AuthService.cs
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace DL6000WebConfig.Services
{
    public class AuthService : IAuthService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            NavigationManager navigationManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _navigationManager = navigationManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResult> LoginAsync(string username, string password)
        {
            try
            {
                // Implemente sua lógica real de autenticação aqui
                if (username == "admin" && password == "admin123")
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "Administrator")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await _httpContextAccessor.HttpContext.SignInAsync(
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
            catch (Exception ex)
            {
                return new AuthResult { Success = false, ErrorMessage = "Erro durante o login" };
            }
        }

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _navigationManager.NavigateTo("/login");
        }
    }
}