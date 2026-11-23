using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Se colocan las cartas iniciales boca arriba en la mesa.</summary>
    public sealed class TableDealtEvent : GameEvent
    {
        public TableDealtEvent(IReadOnlyList<Card> cards)
        {
            Cards = cards;
        }

        public IReadOnlyList<Card> Cards { get; }
    }
}
