using DL6000WebConfig.Models;
using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DL6000WebConfig.Pages
{
    public partial class GeneralSettings : ComponentBase
    {
        [Inject]
        public ConfigService ConfigService { get; set; } = null!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;

    

        private GeneralSettingsModel? settings;
        private string deviceCountInput = "";
        private string? numberError;

        protected override void OnInitialized()
        {
            settings = ConfigService.GetGeneralSettings();
            deviceCountInput = settings?.DeviceCount ?? "";
        }

        private void HandleDeviceCountInput(ChangeEventArgs e)
        {
            deviceCountInput = e.Value?.ToString() ?? "";
            
            // Valida se é número
            if (int.TryParse(deviceCountInput, out int result))
            {
                settings!.DeviceCount = deviceCountInput;
                numberError = null;
            }
            else
            {
                numberError = "Por favor, insira apenas números";
            }
        }

        private async Task HandleSave()
        {
            if (!string.IsNullOrEmpty(numberError))
            {
                await JSRuntime.InvokeVoidAsync("alert", "Corrija os erros antes de salvar");
                return;
            }

            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", 
                "ATENÇÃO: Tem certeza que deseja salvar as alterações?");
            
            if (confirmed)
            {
                ConfigService.SaveGeneralSettings(settings!);
            }
        }

    }
}