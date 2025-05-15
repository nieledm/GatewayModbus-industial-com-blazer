// Services/IAuthService.cs
using System.Threading.Tasks;

namespace DL6000WebConfig.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string username, string password);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = null!;
    }
}