using System;
using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>
    /// Puntuación de una ronda según <see cref="RulesConfig"/> (skill <c>escoba-rules</c> §6).
    /// Función pura: no depende del estado de la partida.
    /// </summary>
    public static class RoundScorer
    {
        private const int GoldsInDeck = Card.RanksPerSuit;
        private const int SevensInDeck = Card.SuitCount;

        /// <summary>Puntúa la ronda a partir de las bazas y escobas de cada bando.</summary>
        public static RoundScore Score(RulesConfig rules, IReadOnlyList<IReadOnlyList<Card>> capturedBySide, IReadOnlyList<int> escobasBySide)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (capturedBySide == null || escobasBySide == null || capturedBySide.Count != escobasBySide.Count)
            {
                throw new ArgumentException("Debe haber una baza y un recuento de escobas por bando.");
            }

            int sideCount = capturedBySide.Count;
            var sides = new SideRoundScore[sideCount];
            var cardCounts = new int[sideCount];
            var goldCounts = new int[sideCount];

            for (int i = 0; i < sideCount; i++)
            {
                var score = new SideRoundScore(i);
                foreach (Card card in capturedBySide[i])
                {
                    score.CardCount++;
                    if (card.Suit == Suit.Oros)
                    {
                        score.GoldCount++;
                    }

                    if (card.IsSeven)
                    {
                        score.SevenCount++;
                    }

                    if (card.IsGoldSeven)
                    {
                        score.HasGoldSeven = true;
                    }
                }

                score.SetentaCards = Setenta.Build(capturedBySide[i]);
                score.EscobaPoints = escobasBySide[i];
                score.GoldSevenPoints = score.HasGoldSeven ? 1 : 0;
                if (rules.AllSevensBonus > 0 && score.SevenCount == SevensInDeck)
                {
                    score.AllSevensBonusPoints = rules.AllSevensBonus;
                }

                cardCounts[i] = score.CardCount;
                goldCounts[i] = score.GoldCount;
                sides[i] = score;
            }

            int mostCards = UniqueMaxIndex(cardCounts);
            if (mostCards >= 0)
            {
                sides[mostCards].MostCardsPoints = 1;
            }

            int mostGolds = UniqueMaxIndex(goldCounts);
            if (mostGolds >= 0)
            {
                if (rules.AllGoldsBonus > 0 && goldCounts[mostGolds] == GoldsInDeck)
                {
                    sides[mostGolds].AllGoldsBonusPoints = rules.AllGoldsBonus;
                }
                else
                {
                    sides[mostGolds].MostGoldsPoints = 1;
                }
            }

            foreach (int winner in SetentaWinners(sides, rules.SetentaTieRule))
            {
                sides[winner].SetentaPoints = 1;
            }

            return new RoundScore(sides);
        }

        /// <summary>Índice del único máximo, o -1 si el máximo está empatado.</summary>
        internal static int UniqueMaxIndex(IReadOnlyList<int> values)
        {
            int best = -1;
            int max = int.MinValue;
            bool tie = false;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                    best = i;
                    tie = false;
                }
                else if (values[i] == max)
                {
                    tie = true;
                }
            }

            return tie ? -1 : best;
        }

        private static IEnumerable<int> SetentaWinners(SideRoundScore[] sides, SetentaTieRule tieRule)
        {
            IReadOnlyList<Card> best = null;
            var leaders = new List<int>();
            for (int i = 0; i < sides.Length; i++)
            {
                IReadOnlyList<Card> setenta = sides[i].SetentaCards;
                if (setenta == null)
                {
                    continue;
                }

                int cmp = Setenta.Compare(setenta, best);
                if (best == null || cmp > 0)
                {
                    best = setenta;
                    leaders.Clear();
                    leaders.Add(i);
                }
                else if (cmp == 0)
                {
                    leaders.Add(i);
                }
            }

            if (leaders.Count > 1 && tieRule == SetentaTieRule.NoPoint)
            {
                leaders.Clear();
            }

            return leaders;
        }
    }
}
