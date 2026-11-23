namespace Escoba.Core
{
    /// <summary>
    /// Pasa a la siguiente ronda tras el resumen. Solo es válido en <see cref="GamePhase.RoundOver"/>.
    /// En local lo emite cualquier jugador; en online, la autoridad (Master Client).
    /// </summary>
    public sealed class StartNextRoundCommand : IGameCommand
    {
        public StartNextRoundCommand(int seat)
        {
            Seat = seat;
        }

        /// <inheritdoc />
        public int Seat { get; }
    }
}
