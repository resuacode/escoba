using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Fin de ronda: las cartas que quedan en la mesa van al último que capturó (no es escoba).</summary>
    public sealed class TableAwardedToLastCapturerEvent : GameEvent
    {
        public TableAwardedToLastCapturerEvent(int seat, IReadOnlyList<Card> cards)
        {
            Seat = seat;
            Cards = cards;
        }

        public int Seat { get; }

        public IReadOnlyList<Card> Cards { get; }
    }
}
