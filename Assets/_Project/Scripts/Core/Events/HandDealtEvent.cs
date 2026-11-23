using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Un asiento recibe cartas. Privado: solo su dueño debe ver las cartas.</summary>
    public sealed class HandDealtEvent : GameEvent
    {
        public HandDealtEvent(int seat, IReadOnlyList<Card> cards)
        {
            Seat = seat;
            Cards = cards;
        }

        public int Seat { get; }

        public IReadOnlyList<Card> Cards { get; }

        public override int IsPrivateTo => Seat;
    }
}
