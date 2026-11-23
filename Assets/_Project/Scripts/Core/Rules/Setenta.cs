using System;
using System.Collections.Generic;
using System.Linq;

namespace Escoba.Core
{
    /// <summary>
    /// Setenta clásica: la mejor carta de cada palo según 7 &gt; 6 &gt; 1 &gt; 5 &gt; 4 &gt; 3 &gt; 2 &gt; figuras,
    /// comparada lexicográficamente de mayor a menor (equivale a "más sietes y, si empatan,
    /// la carta más alta en los palos restantes"). Ver skill <c>escoba-rules</c> §6.
    /// </summary>
    public static class Setenta
    {
        /// <summary>
        /// Devuelve la setenta (una carta por palo, ordenada de mayor a menor fuerza)
        /// o <c>null</c> si falta algún palo.
        /// </summary>
        public static IReadOnlyList<Card> Build(IEnumerable<Card> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            var best = new Card?[Card.SuitCount];
            foreach (Card card in cards)
            {
                int suit = (int)card.Suit;
                Card? current = best[suit];
                if (current == null
                    || card.SetentaStrength > current.Value.SetentaStrength
                    || (card.SetentaStrength == current.Value.SetentaStrength && card.Id < current.Value.Id))
                {
                    best[suit] = card;
                }
            }

            if (best.Any(c => c == null))
            {
                return null;
            }

            return best
                .Select(c => c.Value)
                .OrderByDescending(c => c.SetentaStrength)
                .ThenBy(c => c.Id)
                .ToArray();
        }

        /// <summary>
        /// Compara dos setentas: &gt; 0 si gana <paramref name="a"/>, &lt; 0 si gana <paramref name="b"/>,
        /// 0 si empatan. Una setenta incompleta (<c>null</c>) pierde siempre contra una completa.
        /// </summary>
        public static int Compare(IReadOnlyList<Card> a, IReadOnlyList<Card> b)
        {
            if (a == null || b == null)
            {
                return (a == null ? 0 : 1) - (b == null ? 0 : 1);
            }

            for (int i = 0; i < Card.SuitCount; i++)
            {
                int diff = a[i].SetentaStrength - b[i].SetentaStrength;
                if (diff != 0)
                {
                    return diff;
                }
            }

            return 0;
        }
    }
}
