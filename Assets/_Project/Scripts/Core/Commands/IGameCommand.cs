namespace Escoba.Core
{
    /// <summary>
    /// Acción que un jugador (humano, IA o remoto) pide al motor. Debe ser pequeña y serializable:
    /// solo ids y asientos, nunca referencias a objetos.
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>Asiento que emite el comando.</summary>
        int Seat { get; }
    }
}
