using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DL6000WebConfig.Pages
{
    public partial class Sobre : ComponentBase, IAsyncDisposable
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        private IJSObjectReference? module;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try 
                {
                    var module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                        "import", "./js/site.js");
                    await module.InvokeVoidAsync("setupContactButtons");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao carregar JS: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }

        private async Task HandleEmailClick()
        {
            try 
            {
                // Força a abertura do cliente de email
                await JSRuntime.InvokeVoidAsync("openEmailClient", "dev@exemplo.com");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao abrir email: {ex.Message}");
            }
        }

        private async Task OpenEmail(string email)
        {
            try 
            {
                // Solução mais direta e confiável
                await JSRuntime.InvokeVoidAsync("eval", 
                    $"window.location.href = 'mailto:{email}'");
            }
            catch
            {
                // Fallback extremamente simples que sempre funciona
                var mailtoUri = $"mailto:{email}";
                await JSRuntime.InvokeVoidAsync("open", mailtoUri, "_blank");
            }
        }
     }
}