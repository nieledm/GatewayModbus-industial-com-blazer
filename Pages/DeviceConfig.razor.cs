using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using DL6000WebConfig.Models;
using DL6000WebConfig.Services;

namespace DL6000WebConfig.Pages
{
    public partial class DeviceConfig : ComponentBase
    {
        [Inject]
        public IJSRuntime JS { get; set; } = null!;

        [Inject] 
        public ConfigService ConfigService { get; set; } = null!;

        [Inject]
        public ModbusVariableService VariableService { get; set; } = null!;

        private string? mensagemErro;
        private DeviceConfigModel form = new();
        private List<DeviceConfigModel> devices = new();
        private bool mostrarModal = false;
        private int editandoIndex = -1;

        protected override void OnInitialized()
        {
            devices = ConfigService.GetDevices();
        }
        
        private void AbrirModal(int? index)
        {
            if (index.HasValue && index.Value >= 0 && index.Value < devices.Count)
            {
                var device = devices[index.Value];
                form = new DeviceConfigModel
                {
                    Name = device.Name.Replace("DL6000_", ""),
                    Ip = device.Ip,
                    Port = device.Port,
                    UnitId1 = device.UnitId1,
                    UnitId2 = device.UnitId2,
                    StartIndexDL1 = device.StartIndexDL1,
                    StartIndexDL2 = device.StartIndexDL2,
                    Cycle = device.Cycle,
                    TimeoutSend = device.TimeoutSend,
                    TimeoutReceive = device.TimeoutReceive
                };
                editandoIndex = index.Value;
            }
            else
            {
                form = new DeviceConfigModel();
                editandoIndex = -1;
            }
            mostrarModal = true;
            mensagemErro = null;
        }

        private void FecharModal()
        {
            mostrarModal = false;
            form = new DeviceConfigModel();
            editandoIndex = -1;
            mensagemErro = null;
        }

        private async Task  SalvarEquipamento()
        {
            try
            {
                // Validação dos campos
                var validation = ValidateEquipment(form);
                if (!validation.isValid)
                {
                    mensagemErro = validation.errorMessage;
                    return;
                }

                // Conversão dos valores para garantir consistência
                int port = int.Parse(form.Port);
                int timeoutSend = int.Parse(form.TimeoutSend);
                int timeoutReceive = int.Parse(form.TimeoutReceive);
                int cycle = int.Parse(form.Cycle);

                if (!int.TryParse(form.StartIndexDL1, out var novoDL1))
                {
                    mensagemErro = "Start Index DL1 deve ser um número inteiro válido.";
                    return;
                }

                // Garante que o novo DL1 não sobrepõe com nenhum DL2 existente
                bool sobreposicao = devices
                    .Where((d, index) => index != editandoIndex) // Ignora o próprio item em edição
                    .Any(d => int.TryParse(d.StartIndexDL2, out var existenteDL2) && novoDL1 <= existenteDL2);

                if (sobreposicao)
                {
                    mensagemErro = "Sobreposição de memórias: o Start Index deve ser maior que os End Index dos outros equipamentos .";
                    return;
                }

                await Task.Run(() =>
                {
                    // Verificação de duplicatas
                    if (editandoIndex >= 0)
                    {
                        var antigoNome = devices[editandoIndex].Name;
                        if (form.Name != antigoNome && devices.Any(d => d.Name == form.Name))
                        {
                            mensagemErro = "Já existe um equipamento com este nome.";
                            return;
                        }

                        // Atualiza com os valores convertidos
                        ConfigService.UpdateDevice(form, antigoNome);
                    }
                    else
                    {
                        if (devices.Any(d => d.Name == form.Name))
                        {
                            mensagemErro = "Já existe um equipamento com este nome.";
                            return;
                        }

                        // Adiciona com os valores convertidos
                        ConfigService.AddDevice(form);
                    }
                });

                devices = ConfigService.GetDevices();
                FecharModal();
            }
            catch (Exception ex)
            {
                mensagemErro = $"Erro ao salvar: {ex.Message}";
            }
        }

        private (bool isValid, string? errorMessage) ValidateEquipment(DeviceConfigModel equipment)
        {
            #region Validação do device.name
            if (string.IsNullOrWhiteSpace(equipment.Name))
            {
                return (false, "O nome do equipamento é obrigatório.");
            }

            var numberPart = equipment.Name.Replace("DL6000_", "");
            if (!int.TryParse(numberPart, out _))
            {
                return (false, "Após 'DL6000_' deve conter apenas números.");
            }
            #endregion
            #region  Validação do IP
            if (string.IsNullOrWhiteSpace(equipment.Ip))
            {
                return (false, "O endereço IP é obrigatório.");
            }

            var ipParts = equipment.Ip.Split('.');
            if (ipParts.Length != 4)
            {
                return (false, "IP inválido. Use o formato: 192.168.1.1");
            }

            foreach (var part in ipParts)
            {
                if (!byte.TryParse(part, out _))
                {
                    return (false, $"Parte inválida do IP: '{part}'. Deve ser entre 0 e 255.");
                }
            }
            #endregion
            #region  Validação da Porta
            if (!int.TryParse(equipment.Port, out int port) || port < 1 || port > 65535)
            {
                return (false, "A porta deve ser um número entre 1 e 65535.");
            }
            #endregion
            #region Validação dos Timeouts
            if (!int.TryParse(equipment.TimeoutSend, out int timeoutSend) || timeoutSend <= 0)
            {
                return (false, "Timeout de envio deve ser um número maior que zero.");
            }

            if (!int.TryParse(equipment.TimeoutReceive, out int timeoutReceive) || timeoutReceive <= 0)
            {
                return (false, "Timeout de recebimento deve ser um número maior que zero.");
            }
            #endregion
            #region  Validação do Cycle
            if (!int.TryParse(equipment.Cycle, out int cycle) || cycle <= 0)
            {
                return (false, "O ciclo deve ser um número maior que zero.");
            }
            #endregion
            #region  Validações dos StartIndexDL
            if (string.IsNullOrWhiteSpace(equipment.StartIndexDL1))
            {
                return (false, "O campo Start Index DL1 não pode estar vazio.");
            }

            if (!int.TryParse(equipment.StartIndexDL1, out int startDL1))
            {
                return (false, "Start Index DL1 deve ser um número inteiro.");
            }

            if (string.IsNullOrWhiteSpace(equipment.StartIndexDL2))
            {
                return (false, "O campo Start Index DL2 não pode estar vazio.");
            }

            if (!int.TryParse(equipment.StartIndexDL2, out int startDL2))
            {
                return (false, "Start Index DL2 deve ser um número inteiro.");
            }

            // Validação adicional dos ranges
            if (startDL1 < 0)
            {
                return (false, "Start Index DL1 não pode ser negativo.");
            }

            if (startDL2 < 0)
            {
                return (false, "Start Index DL2 não pode ser negativo.");
            }

            if (startDL2 <= startDL1)
            {
                return (false, "Start Index DL2 deve ser maior que DL1.");
            }
            #endregion
            return (true, null);
        }   
        private Task ExcluirEquipamento(int index)
        {
            var device = devices[index];
            ConfigService.DeleteDevice(device.Name);
            VariableService.DeleteAllVariablesForDevice(device.Name);
            devices.RemoveAt(index);
            return Task.CompletedTask;
        }

        private async Task ExportJson()
        {
            var json = JsonSerializer.Serialize(devices, new JsonSerializerOptions { WriteIndented = true });
            await JS.InvokeVoidAsync("downloadFile", "config-dl6000.json", "application/json", json);
        }

        private async Task ImportJson(ChangeEventArgs e)
        {            
            if (e.Value is IBrowserFile file)
            {
                var buffer = new byte[file.Size];
                await file.OpenReadStream().ReadAsync(buffer);
                var json = System.Text.Encoding.UTF8.GetString(buffer);
                var importedDevices = JsonSerializer.Deserialize<List<DeviceConfigModel>>(json);

                if (importedDevices != null)
                {
                    devices = importedDevices;
                }
            }
        }

        private async Task ExcluirEquipamentoComConfirmacao(int index)
        {
            try 
            {
                var confirmed = await JS.InvokeAsync<bool>("confirm", 
                    "Cuidado!!!\nAo excluir o equipamento todas as suas informações serão perdidas e a operação não poderá ser desfeita.\nTem certeza que deseja continuar?");
                
                if (confirmed)
                {
                    await ExcluirEquipamento(index);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na confirmação: {ex.Message}");
            }
        }

        // Método de salvamento com confirmação
        private async Task SalvarEquipamentoComConfirmacao()
        {
            try
            {
                var confirmed = await JS.InvokeAsync<bool>("confirm", 
                    "Confirmar alterações nesta variável?");
                
                if (confirmed)
                {
                    await SalvarEquipamento();
                    FecharModal();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na confirmação: {ex.Message}");
            }
        }

    }
    
}