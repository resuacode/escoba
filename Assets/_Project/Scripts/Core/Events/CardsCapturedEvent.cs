using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Un jugador captura cartas de la mesa con la carta jugada (y quizá hace escoba).</summary>
    public sealed class CardsCapturedEvent : GameEvent
    {
        public CardsCapturedEvent(int seat, Card playedCard, IReadOnlyList<Card> captured, bool isEscoba)
        {
            Seat = seat;
            PlayedCard = playedCard;
            Captured = captured;
            IsEscoba = isEscoba;
        }

        public int Seat { get; }

        public Card PlayedCard { get; }

        /// <summary>Cartas de la mesa capturadas (sin incluir la jugada).</summary>
        public IReadOnlyList<Card> Captured { get; }

        /// <summary>La captura dejó la mesa vacía y cuenta como escoba.</summary>
        public bool IsEscoba { get; }
    }
}
