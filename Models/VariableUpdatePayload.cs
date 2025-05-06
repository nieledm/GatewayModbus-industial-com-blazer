namespace DL6000WebConfig.Models
{
    // / <summary>
    // / Representa um payload contendo uma variável Modbus original e a atualizada.
    // / Usado em operações de atualização de variáveis Modbus.
    // / </summary>
    public class VariableUpdatePayload
    {
        public ModbusVariable Original { get; }
        public ModbusVariable Updated { get; }

        /// <summary>
        /// Construtor da classe VariableUpdatePayload.
        /// </summary>
        /// <param name="original">A variável Modbus original.</param>
        /// <param name="updated">A variável Modbus atualizada.</param>
        /// <exception cref="ArgumentNullException">Lançado se original ou updated for null.</exception>
        public VariableUpdatePayload(ModbusVariable original, ModbusVariable updated)
        {
            Original = original ?? throw new ArgumentNullException(nameof(original), "Original cannot be null.");
            Updated = updated ?? throw new ArgumentNullException(nameof(updated), "Updated cannot be null.");
        }
    }
}
