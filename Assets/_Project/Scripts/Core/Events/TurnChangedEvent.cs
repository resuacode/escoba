namespace Escoba.Core
{
    /// <summary>Le toca jugar a otro asiento.</summary>
    public sealed class TurnChangedEvent : GameEvent
    {
        public TurnChangedEvent(int seat)
        {
            Seat = seat;
        }

        public int Seat { get; }
    }
}
