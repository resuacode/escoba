using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Lo que un bot puede deducir legalmente de su <see cref="PlayerView"/>: las cartas que no ha
    /// visto (manos ajenas + mazo) = 40 − su mano − la mesa − todas las bazas (públicas).
    /// </summary>
    public static class BotKnowledge
    {
        /// <summary>Cartas no vistas por el dueño de la vista, ordenadas por id (determinista).</summary>
        public static List<Card> UnseenCards(PlayerView view)
        {
            var seen = new bool[Card.DeckSize];
            foreach (Card card in view.Hand)
            {
                seen[card.Id] = true;
            }

            foreach (Card card in view.Table)
            {
                seen[card.Id] = true;
            }

            foreach (SideView side in view.Sides)
            {
                foreach (Card card in side.Captured)
                {
                    seen[card.Id] = true;
                }
            }

            var unseen = new List<Card>();
            for (int id = 0; id < Card.DeckSize; id++)
            {
                if (!seen[id])
                {
                    unseen.Add(Card.FromId(id));
                }
            }

            return unseen;
        }

        /// <summary>
        /// Probabilidad de que una mano de <paramref name="handSize"/> cartas, tomada al azar de
        /// <paramref name="unseenCount"/> cartas no vistas, contenga al menos una de
        /// <paramref name="matchingCount"/> cartas buscadas (hipergeométrica).
        /// </summary>
        public static double ProbabilityOfAtLeastOne(int unseenCount, int matchingCount, int handSize)
        {
            if (matchingCount <= 0 || handSize <= 0 || unseenCount <= 0)
            {
                return 0;
            }

            if (handSize > unseenCount - matchingCount)
            {
                return 1;
            }

            // P(ninguna) = C(U-m, h) / C(U, h) = Π_{i=0}^{h-1} (U-m-i) / (U-i)
            double none = 1;
            for (int i = 0; i < handSize; i++)
            {
                none *= (double)(unseenCount - matchingCount - i) / (unseenCount - i);
            }

            return 1 - none;
        }
    }
}
