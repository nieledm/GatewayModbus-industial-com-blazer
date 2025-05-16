using Microsoft.AspNetCore.Components;
using DL6000WebConfig.Services;
using Microsoft.JSInterop;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;



namespace DL6000WebConfig.Pages
{
    public partial class Login
    {
        [Inject] public IAuthService AuthService { get; set; } = null!;


        [Inject]
        public UserService UserService { get; set; } = null!;

        [Inject]
        public NavigationManager Navigation { get; set; } = null!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;

        [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = null!;

        [Inject] public HttpClient Http { get; set; } = null!;

        private LoginModel loginModel = new();
        private string errorMessage = string.Empty;
        private bool isLoading = false;
        private bool isInitialized = false;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadRememberedCredentials();
                isInitialized = true;
                StateHasChanged();
            }
        }

        public class ErrorResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = null!;
        }

        private async Task LoadRememberedCredentials()
        {
            try
            {
                var savedLogin = await JSRuntime.InvokeAsync<string>("localStorage.getItem", "rememberedLogin");
                if (!string.IsNullOrEmpty(savedLogin))
                {
                    var credentials = JsonSerializer.Deserialize<LoginModel>(savedLogin);
                    if (credentials != null)
                    {
                        loginModel = credentials;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar credenciais: {ex.Message}");
            }
        }

        private async Task HandleLogin()
        {
            isLoading = true;
            errorMessage = string.Empty;
            StateHasChanged();

            try
            {
                var response = await Http.PostAsJsonAsync("api/auth/login", loginModel);

                if (response.IsSuccessStatusCode)
                {
                    Navigation.NavigateTo("/monitor", true);
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    errorMessage = errorResponse?.Message ?? "Usuário ou senha inválidos.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Erro durante o login: " + ex.Message;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }

        public class LoginModel
        {
            [Required(ErrorMessage = "Usuário é obrigatório")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Senha é obrigatória")]
            [MinLength(6, ErrorMessage = "Senha deve ter pelo menos 6 caracteres")]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        private async Task Logout()
        {
            await HttpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Navigation.NavigateTo("/");
        }
        
        protected override void OnInitialized()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            if (queryParams.TryGetValue("mensagem", out var mensagem))
            {
                if (mensagem == "1")
                {
                    errorMessage = "Usuário ou senha inválidos.";
                }
            }
        }
    }
}