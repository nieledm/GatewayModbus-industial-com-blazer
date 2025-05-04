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

        private void SalvarEquipamento()
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
                    ConfigService.UpdateDevice(form);
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

                devices = ConfigService.GetDevices();
                FecharModal();
            }
            catch (Exception ex)
            {
                mensagemErro = $"Erro ao salvar: {ex.Message}";
            }
    }

        private (bool isValid, string errorMessage) ValidateEquipment(DeviceConfigModel equipment)
        {
            // Validação do Nome
            if (string.IsNullOrWhiteSpace(equipment.Name))
            {
                return (false, "O nome do equipamento é obrigatório.");
            }

            if (!equipment.Name.StartsWith("DL6000_"))
            {
                return (false, "O nome deve começar com 'DL6000_'.");
            }

            var numberPart = equipment.Name.Substring(7);
            if (!int.TryParse(numberPart, out _))
            {
                return (false, "Após 'DL6000_' deve conter apenas números.");
            }

            // Validação do IP (já é string)
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

            // Validação da Porta (string para int)
            if (!int.TryParse(equipment.Port, out int port) || port < 1 || port > 65535)
            {
                return (false, "A porta deve ser um número entre 1 e 65535.");
            }

            // Validação do TimeoutSend (string para int)
            if (!int.TryParse(equipment.TimeoutSend, out int timeoutSend) || timeoutSend <= 0)
            {
                return (false, "Timeout de envio deve ser um número maior que zero.");
            }

            // Validação do TimeoutReceive (string para int)
            if (!int.TryParse(equipment.TimeoutReceive, out int timeoutReceive) || timeoutReceive <= 0)
            {
                return (false, "Timeout de recebimento deve ser um número maior que zero.");
            }

            // Validação do Cycle (string para int)
            if (!int.TryParse(equipment.Cycle, out int cycle) || cycle <= 0)
            {
                return (false, "O ciclo deve ser um número maior que zero.");
            }

            return (true, null);
    }

    
        private void ExcluirEquipamento(int index)
        {
            var device = devices[index];
            ConfigService.DeleteDevice(device.Name);
            VariableService.DeleteAllVariablesForDevice(device.Name);
            devices.RemoveAt(index);
        }

        private async Task ExportJson()
        {
            var json = JsonSerializer.Serialize(devices, new JsonSerializerOptions { WriteIndented = true });
            await JS.InvokeVoidAsync("downloadFile", "config-dl6000.json", "application/json", json);
        }

        private async Task ImportJson(ChangeEventArgs e)
        {
            var file = ((IBrowserFile)e.Value);
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
}