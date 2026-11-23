namespace Escoba.Core
{
    /// <summary>La partida ha terminado.</summary>
    public sealed class GameOverEvent : GameEvent
    {
        public GameOverEvent(int winnerSide)
        {
            WinnerSide = winnerSide;
        }

        public int WinnerSide { get; }
    }
}
