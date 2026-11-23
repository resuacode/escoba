using System;
using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>
    /// Cálculo de capturas y jugadas legales. Es la única fuente de verdad de "qué se puede hacer":
    /// la usan el motor (validación), la IA y la presentación (resaltado de capturas).
    /// </summary>
    public static class LegalMoves
    {
        /// <summary>
        /// Devuelve todas las combinaciones de cartas de la mesa que, sumadas a la carta jugada,
        /// alcanzan <paramref name="captureSum"/>. Orden determinista (por posición en la mesa).
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<Card>> FindCaptures(Card played, IReadOnlyList<Card> table, int captureSum)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var results = new List<IReadOnlyList<Card>>();
            int target = captureSum - played.Value;
            if (target <= 0 || table.Count == 0)
            {
                return results;
            }

            Search(table, 0, target, new List<Card>(), results);
            return results;
        }

        /// <summary>Indica si la carta jugada puede capturar alguna combinación de la mesa.</summary>
        public static bool CanCapture(Card played, IReadOnlyList<Card> table, int captureSum)
        {
            return FindCaptures(played, table, captureSum).Count > 0;
        }

        /// <summary>Todas las jugadas legales de un asiento con su mano y la mesa actual.</summary>
        public static IReadOnlyList<PlayCardCommand> For(int seat, IReadOnlyList<Card> hand, IReadOnlyList<Card> table, RulesConfig rules)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            var moves = new List<PlayCardCommand>();
            foreach (Card card in hand)
            {
                IReadOnlyList<IReadOnlyList<Card>> captures = FindCaptures(card, table, rules.CaptureSum);
                foreach (IReadOnlyList<Card> capture in captures)
                {
                    moves.Add(PlayCardCommand.Capture(seat, card, capture));
                }

                if (captures.Count == 0 || !rules.CaptureIsMandatory)
                {
                    moves.Add(PlayCardCommand.Drop(seat, card));
                }
            }

            return moves;
        }

        /// <summary>Jugadas legales del jugador dueño de la vista.</summary>
        public static IReadOnlyList<PlayCardCommand> For(PlayerView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            return For(view.Seat, view.Hand, view.Table, view.Rules);
        }

        private static void Search(IReadOnlyList<Card> table, int start, int remaining, List<Card> current, List<IReadOnlyList<Card>> results)
        {
            for (int i = start; i < table.Count; i++)
            {
                int value = table[i].Value;
                if (value > remaining)
                {
                    continue;
                }

                current.Add(table[i]);
                if (value == remaining)
                {
                    results.Add(current.ToArray());
                }
                else
                {
                    Search(table, i + 1, remaining - value, current, results);
                }

                current.RemoveAt(current.Count - 1);
            }
        }
    }
}
