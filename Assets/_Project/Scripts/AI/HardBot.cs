using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Bot de nivel Difícil: heurística del Medio + conteo de cartas y muestreo Monte Carlo.
    /// Para cada jugada candidata muestrea manos posibles del siguiente rival entre las cartas no
    /// vistas (<see cref="BotKnowledge"/>) y resta la ganancia media de su mejor respuesta
    /// (escobas, sietes, oros, cartas). No hace trampas: solo usa su <see cref="PlayerView"/>.
    /// </summary>
    public sealed class HardBot : IBotStrategy
    {
        private readonly HeuristicWeights _weights;

        public HardBot(HeuristicWeights weights = null)
        {
            _weights = weights ?? HeuristicWeights.Default;
        }

        /// <inheritdoc />
        public PlayCardCommand ChooseCommand(PlayerView view, IRandom rng)
        {
            BotGuard.CheckTurn(view, rng);
            IReadOnlyList<Card> own = view.Sides[view.Side].Captured;
            int next = (view.Seat + 1) % view.PlayerCount;
            bool opponentNext = view.Rules.SideOfSeat(next) != view.Side && view.HandCounts[next] > 0;
            Card[] unseen = BotKnowledge.UnseenCards(view).ToArray();
            int opponentHand = view.HandCounts[next];

            // Las mismas manos muestreadas para todas las candidatas: comparación más justa y estable.
            List<Card[]> samples = opponentNext ? SampleHands(unseen, opponentHand, rng) : null;

            return BotGuard.PickBest(LegalMoves.For(view), rng, move =>
            {
                MoveScoring.Outcome outcome = MoveScoring.Simulate(move, view.Table);
                double score = MoveScoring.ImmediateGain(outcome, own, _weights);
                if (samples != null)
                {
                    score -= _weights.OpponentFactor * ExpectedOpponentGain(outcome.TableAfter, samples, view.Rules);
                }

                return score;
            });
        }

        private List<Card[]> SampleHands(Card[] unseen, int handSize, IRandom rng)
        {
            var samples = new List<Card[]>(_weights.HardSamples);
            if (handSize <= 0 || unseen.Length == 0)
            {
                return samples;
            }

            handSize = System.Math.Min(handSize, unseen.Length);
            var pool = (Card[])unseen.Clone();
            for (int s = 0; s < _weights.HardSamples; s++)
            {
                // Fisher-Yates parcial: las primeras handSize posiciones forman la mano muestreada.
                for (int i = 0; i < handSize; i++)
                {
                    int j = i + rng.Next(pool.Length - i);
                    Card tmp = pool[i];
                    pool[i] = pool[j];
                    pool[j] = tmp;
                }

                var hand = new Card[handSize];
                System.Array.Copy(pool, hand, handSize);
                samples.Add(hand);
            }

            return samples;
        }

        private double ExpectedOpponentGain(List<Card> tableAfter, List<Card[]> samples, RulesConfig rules)
        {
            if (samples.Count == 0)
            {
                return 0;
            }

            double total = 0;
            foreach (Card[] hand in samples)
            {
                total += MoveScoring.BestGain(hand, tableAfter, rules, _weights);
            }

            return total / samples.Count;
        }
    }
}
