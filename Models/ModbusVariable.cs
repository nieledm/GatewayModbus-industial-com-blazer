namespace DL6000WebConfig.Models
{
    public class ModbusVariable
    {
        private int _offset;
        public string DeviceName { get; set; } = "";
        public string Name { get; set; } = "";
        public int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                Address = $"40{(_offset + 1).ToString("D3")}";                
            }
        }
        public string Address { get; set; } = "";
        public int Value { get; set; }
        
        #region Memoria de exposição modbus
        public ushort GetRealAddress(DeviceConfigModel config)
        {
            // Apenas lê a configuração do StartIndexDL1 do dispositivo (não altera o arquivo .config)
            if (int.TryParse(config.StartIndexDL1, out int startDL1))
            {
                // Calcula o endereço real baseado no offset
                int realAddress = startDL1 + Offset;

                // Verifica se o cálculo do endereço real é válido (não negativo)
                if (realAddress < 0)
                {
                    return 0; // Retorna 0 se o endereço for inválido
                }

                // Retorna o endereço real calculado como ushort
                return (ushort)realAddress;
            }
            else
            {
                Console.WriteLine($"Erro: StartIndexDL1 não é válido ou não configurado corretamente.");
                return 0;  // Retorna 0 se StartIndexDL1 não for válido
            }
        }

        public void UpdateRealAddress(DeviceConfigModel config)
        {
            // Atualiza o RealAddress usando a configuração lida do .config
            RealAddress = GetRealAddress(config);
        }

        public ushort RealAddress { get; set; }
        #endregion
    }
}
