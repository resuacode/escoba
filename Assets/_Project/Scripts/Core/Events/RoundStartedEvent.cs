namespace Escoba.Core
{
    /// <summary>Empieza una ronda: se ha barajado y se va a repartir.</summary>
    public sealed class RoundStartedEvent : GameEvent
    {
        public RoundStartedEvent(int roundNumber, int dealer)
        {
            RoundNumber = roundNumber;
            Dealer = dealer;
        }

        public int RoundNumber { get; }

        public int Dealer { get; }
    }
}
