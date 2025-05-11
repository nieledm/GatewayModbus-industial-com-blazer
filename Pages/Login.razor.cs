using Microsoft.AspNetCore.Components;
using DL6000WebConfig.Services;
using Microsoft.JSInterop;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace DL6000WebConfig.Pages
{
    public partial class Login
    {
        [Inject] 
        public UserService UserService { get; set; } = null!;
        
        [Inject]
        public NavigationManager Navigation { get; set; } = null!;
        
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;

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
                if (UserService.ValidateUser(loginModel.Username, loginModel.Password))
                {
                    if (loginModel.RememberMe)
                    {
                        await JSRuntime.InvokeVoidAsync("localStorage.setItem", 
                            "rememberedLogin", 
                            JsonSerializer.Serialize(loginModel));
                    }
                    else
                    {
                        await JSRuntime.InvokeVoidAsync("localStorage.removeItem", "rememberedLogin");
                    }

                    Navigation.NavigateTo("/monitor", true);
                }
                else
                {
                    errorMessage = "Usuário ou senha inválidos";
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Erro ao processar login: " + ex.Message;
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
    }
}