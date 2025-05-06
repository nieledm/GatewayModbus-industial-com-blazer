using DL6000WebConfig.Models;
using DL6000WebConfig.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DL6000WebConfig.Pages
{
    public partial class Monitor : ComponentBase
    {
        [Inject]
        public ConfigService ConfigService { get; set; } = null!;
        
        [Inject]
        public HttpClient Http { get; set; } = null!;
       
        [Inject]
        public IHttpClientFactory ClientFactory { get; set; } = null!;
       
        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;

        private bool shouldUpdate = true;
        private List<ModbusVariable> variables = new();
        private ModbusVariable newVar = new();
        private List<string> deviceNames = new();
        private System.Threading.Timer? timer;
        private HttpClient? client;
        private bool isModalOpen = false;
        
        // Variáveis para controle do modal
        private bool showEditModal = false;
        private ModbusVariable? editVariable = null;
        private ModbusVariable? originalVariable = null;
        private CancellationTokenSource? timerCancellationToken;

        private void StartTimer()
        {
            timer = new Timer(async _ => 
            {
                if (shouldUpdate) // Só atualiza quando permitido
                {
                    await InvokeAsync(async () =>
                    {
                        await LoadVariables();
                        StateHasChanged();
                    });
                }
            }, null, 0, 3000);
        }

        protected override async Task OnInitializedAsync()
        {
            try 
            {
                client = ClientFactory.CreateClient("api");
                await LoadVariables();

                var devices = ConfigService.GetDevices();
                if (devices != null)
                {
                    deviceNames = devices.Select(d => d.Name).ToList();
                }

                StartTimer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na inicialização: {ex}");
            }        
        }
        private async Task LoadVariables()
        {
            if (client == null) return;

            try
            {
                var data = await client.GetFromJsonAsync<List<ModbusVariable>>("/api/modbus/variables");
                if (data != null)
                {
                    variables = data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar variáveis: {ex}");
            }
        }

        private void OpenEditModal(ModbusVariable variable)
        {
            shouldUpdate = false;
            if (variable == null) return;
            
            isModalOpen = true;
            
            // Faz uma cópia da variável para edição
            originalVariable = new ModbusVariable
            {
                Name = variable.Name,
                Offset = variable.Offset,
                DeviceName = variable.DeviceName,
                Address = variable.Address,
                Value = variable.Value
            };
            
            editVariable = new ModbusVariable
            {
                Name = variable.Name,
                Offset = variable.Offset,
                DeviceName = variable.DeviceName,
                Address = variable.Address,
                Value = variable.Value
            };
            
            showEditModal = true;
            StateHasChanged();
        }

        private void CloseEditModal()
        {
            isModalOpen = false;
            showEditModal = false;
            editVariable = null;
            originalVariable = null;
            StateHasChanged();
            shouldUpdate = true;
        }

        private async Task SaveVariable()
        {
            if (editVariable == null || originalVariable == null) return;

            try
            {
                // Carregar dispositivos apenas para ler as configurações
                var devices = ConfigService.GetDevices();
                if (devices != null)
                {
                    deviceNames = devices.Select(d => d.Name).ToList();
                }

                // Obter a configuração do dispositivo (somente para leitura)
                var config = devices?.FirstOrDefault(d => d.Name == editVariable.DeviceName);

                if (config != null)
                {
                    // Atualizar o RealAddress com base na configuração lida do .config
                    editVariable.UpdateRealAddress(config);
                }

                // Atualizar a variável no JSON sem modificar o arquivo .config
                var payload = new
                {
                    original = originalVariable,
                    updated = editVariable
                };

                var response = await Http.PutAsJsonAsync("/api/modbus/variables", payload);
                if (response.IsSuccessStatusCode)
                {
                    await LoadVariables();  // Recarregar as variáveis
                    CloseEditModal();       // Fechar o modal
                }
                else
                {
                    Console.WriteLine($"Erro ao salvar: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar variável: {ex}");
            }
        }



        private async Task AddVariable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newVar.Name) || newVar.Offset < 0 || string.IsNullOrWhiteSpace(newVar.DeviceName))
                    return;
                
                newVar.Address = $"40{(newVar.Offset + 1).ToString("D3")}";

                var response = await Http.PostAsJsonAsync("/api/modbus/variables", newVar);
                if (response.IsSuccessStatusCode)
                {
                    var addedVar = await response.Content.ReadFromJsonAsync<ModbusVariable>();
                    Console.WriteLine($"Variável adicionada com RealAddress: {addedVar?.RealAddress}");
                    
                    newVar = new();
                    await LoadVariables();
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao adicionar variável: {ex}");
            }
        }

        private async Task RemoveVariable(ModbusVariable variable)
        {
            if (variable == null) return;
            
            try {            
                var response = await Http.DeleteAsync($"/api/modbus/variables/{variable.DeviceName}/{variable.Offset}");
                if (response.IsSuccessStatusCode)
                {
                    await LoadVariables();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao remover variável: {ex}");
            }
        }

        public void Dispose()
        {
            timerCancellationToken?.Cancel();
            timerCancellationToken?.Dispose();
            client?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task RemoveVariableWithConfirm(ModbusVariable variable)
        {
            try 
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", 
                    $"Tem certeza que deseja remover a variável {variable.Name}?");
                
                if (confirmed)
                {
                    await RemoveVariable(variable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na confirmação: {ex.Message}");
            }
        }

        // Método de salvamento com confirmação
        private async Task SaveWithConfirm()
        {
            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", 
                    "Confirmar alterações nesta variável?");
                
                if (confirmed)
                {
                    await SaveVariable();
                    CloseEditModal();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na confirmação: {ex.Message}");
            }
        }
    }
}