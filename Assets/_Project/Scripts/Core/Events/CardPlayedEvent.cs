namespace Escoba.Core
{
    /// <summary>Un jugador deja una carta en la mesa sin capturar.</summary>
    public sealed class CardPlayedEvent : GameEvent
    {
        public CardPlayedEvent(int seat, Card card)
        {
            Seat = seat;
            Card = card;
        }

        public int Seat { get; }

        public Card Card { get; }
    }
}
