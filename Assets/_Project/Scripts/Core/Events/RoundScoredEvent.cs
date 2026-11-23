namespace Escoba.Core
{
    /// <summary>La ronda ha terminado y se ha puntuado.</summary>
    public sealed class RoundScoredEvent : GameEvent
    {
        public RoundScoredEvent(RoundScore score)
        {
            Score = score;
        }

        public RoundScore Score { get; }
    }
}
