using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Bot de nivel Medio: puntúa cada jugada legal con la heurística (<see cref="MoveScoring"/>) y
    /// resta riesgos estimados de forma sencilla: dejar al siguiente rival una mesa que suma
    /// ≤ 14 (posible escoba) y dejar sietes u oros a su alcance. Desempate aleatorio.
    /// </summary>
    public sealed class MediumBot : IBotStrategy
    {
        private readonly HeuristicWeights _weights;

        public MediumBot(HeuristicWeights weights = null)
        {
            _weights = weights ?? HeuristicWeights.Default;
        }

        /// <inheritdoc />
        public PlayCardCommand ChooseCommand(PlayerView view, IRandom rng)
        {
            BotGuard.CheckTurn(view, rng);
            IReadOnlyList<Card> own = view.Sides[view.Side].Captured;
            bool opponentNext = NextIsOpponent(view);
            return BotGuard.PickBest(LegalMoves.For(view), rng, move =>
            {
                MoveScoring.Outcome outcome = MoveScoring.Simulate(move, view.Table);
                double score = MoveScoring.ImmediateGain(outcome, own, _weights);
                if (opponentNext)
                {
                    score -= Risk(outcome, view.Rules);
                }

                return score;
            });
        }

        /// <summary>El siguiente en jugar es un rival que aún tiene (o tendrá) cartas.</summary>
        internal static bool NextIsOpponent(PlayerView view)
        {
            int next = (view.Seat + 1) % view.PlayerCount;
            bool rival = view.Rules.SideOfSeat(next) != view.Side;
            bool willPlay = view.HandCounts[next] > 0 || view.DeckCount > 0;
            return rival && willPlay;
        }

        private double Risk(MoveScoring.Outcome outcome, RulesConfig rules)
        {
            double risk = 0;
            if (MoveScoring.EscobaValueFor(outcome.TableAfter, rules.CaptureSum) > 0)
            {
                risk += _weights.Escoba * _weights.MediumEscobaRiskProbability;
            }

            foreach (Card card in outcome.TableAfter)
            {
                if (card.IsSeven || card.Suit == Suit.Oros)
                {
                    risk += _weights.MediumExposureProbability * MoveScoring.CardWorth(card, _weights);
                }
            }

            return risk;
        }
    }
}
