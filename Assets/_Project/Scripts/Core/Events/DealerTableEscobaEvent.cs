using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>La mesa inicial sumaba 15 (1 escoba) o 30 (2 escobas) y el repartidor se la lleva.</summary>
    public sealed class DealerTableEscobaEvent : GameEvent
    {
        public DealerTableEscobaEvent(int seat, IReadOnlyList<Card> cards, int escobas)
        {
            Seat = seat;
            Cards = cards;
            Escobas = escobas;
        }

        public int Seat { get; }

        public IReadOnlyList<Card> Cards { get; }

        public int Escobas { get; }
    }
}
