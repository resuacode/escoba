using System.Collections.Generic;
using System.Linq;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Valoración inmediata de una jugada (sin mirar al rival): lo que se gana capturando
    /// (escoba, siete de oros, sietes, oros, cartas, setenta) o lo que se regala dejando una carta.
    /// Compartida por los bots Medio y Difícil.
    /// </summary>
    public static class MoveScoring
    {
        /// <summary>Resultado de aplicar una jugada sobre la mesa.</summary>
        public readonly struct Outcome
        {
            public Outcome(Card played, IReadOnlyList<Card> captured, List<Card> tableAfter)
            {
                Played = played;
                Captured = captured;
                TableAfter = tableAfter;
            }

            public Card Played { get; }

            /// <summary>Cartas de la mesa capturadas (vacío si se deja la carta).</summary>
            public IReadOnlyList<Card> Captured { get; }

            /// <summary>Mesa tras la jugada.</summary>
            public List<Card> TableAfter { get; }

            public bool IsCapture => Captured.Count > 0;

            public bool IsEscoba => IsCapture && TableAfter.Count == 0;
        }

        /// <summary>Simula la jugada sobre la mesa.</summary>
        public static Outcome Simulate(PlayCardCommand move, IReadOnlyList<Card> table)
        {
            Card played = Card.FromId(move.CardId);
            Card[] captured = move.CaptureIds.Select(Card.FromId).ToArray();
            var after = new List<Card>(table);
            if (captured.Length == 0)
            {
                after.Add(played);
            }
            else
            {
                foreach (Card card in captured)
                {
                    after.Remove(card);
                }
            }

            return new Outcome(played, captured, after);
        }

        /// <summary>Valor de una carta para quien la captura (sin setenta).</summary>
        public static double CardWorth(Card card, HeuristicWeights w)
        {
            double worth = w.Card;
            if (card.Suit == Suit.Oros)
            {
                worth += w.Gold;
            }

            if (card.IsSeven)
            {
                worth += w.Seven;
            }

            if (card.IsGoldSeven)
            {
                worth += w.GoldSeven;
            }

            return worth;
        }

        /// <summary>
        /// Ganancia inmediata de la jugada. <paramref name="ownCaptured"/> es la baza del bando que
        /// juega (para valorar la mejora de su setenta); puede ser null si se desconoce.
        /// </summary>
        public static double ImmediateGain(Outcome outcome, IReadOnlyList<Card> ownCaptured, HeuristicWeights w)
        {
            if (!outcome.IsCapture)
            {
                return -w.DropFactor * CardWorth(outcome.Played, w);
            }

            double gain = CardWorth(outcome.Played, w);
            foreach (Card card in outcome.Captured)
            {
                gain += CardWorth(card, w);
            }

            if (outcome.IsEscoba)
            {
                gain += w.Escoba;
            }

            if (ownCaptured != null)
            {
                gain += SetentaImprovement(ownCaptured, outcome, w);
            }

            return gain;
        }

        /// <summary>Mejora de la setenta del bando al añadir las cartas capturadas.</summary>
        public static double SetentaImprovement(IReadOnlyList<Card> ownCaptured, Outcome outcome, HeuristicWeights w)
        {
            var best = new int[Card.SuitCount];
            foreach (Card card in ownCaptured)
            {
                int suit = (int)card.Suit;
                if (card.SetentaStrength > best[suit])
                {
                    best[suit] = card.SetentaStrength;
                }
            }

            double improvement = 0;
            foreach (Card card in outcome.Captured.Concat(new[] { outcome.Played }))
            {
                int suit = (int)card.Suit;
                if (best[suit] == 0)
                {
                    improvement += w.SetentaNewSuit;
                }

                if (card.SetentaStrength > best[suit])
                {
                    improvement += w.SetentaStep * (card.SetentaStrength - best[suit]);
                    best[suit] = card.SetentaStrength;
                }
            }

            return improvement;
        }

        /// <summary>Valor de la carta que haría escoba sobre esta mesa (1-10) o 0 si no existe.</summary>
        public static int EscobaValueFor(IReadOnlyList<Card> table, int captureSum)
        {
            if (table.Count == 0)
            {
                return 0;
            }

            int needed = captureSum - table.Sum(c => c.Value);
            return needed >= 1 && needed <= 10 ? needed : 0;
        }

        /// <summary>Mejor ganancia inmediata posible con alguna carta de <paramref name="hand"/> sobre la mesa.</summary>
        public static double BestGain(IReadOnlyList<Card> hand, IReadOnlyList<Card> table, RulesConfig rules, HeuristicWeights w)
        {
            double best = double.NegativeInfinity;
            foreach (PlayCardCommand move in LegalMoves.For(0, hand, table, rules))
            {
                double gain = ImmediateGain(Simulate(move, table), null, w);
                if (gain > best)
                {
                    best = gain;
                }
            }

            return double.IsNegativeInfinity(best) ? 0 : best;
        }
    }
}
