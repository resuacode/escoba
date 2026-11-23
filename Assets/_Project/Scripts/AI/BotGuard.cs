using System;
using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>Utilidades comunes de los bots: comprobaciones y elección de la mejor jugada.</summary>
    internal static class BotGuard
    {
        private const double TieEpsilon = 1e-6;

        public static void CheckTurn(PlayerView view, IRandom rng)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            if (!view.IsMyTurn || view.Hand.Count == 0)
            {
                throw new InvalidOperationException($"No es el turno del asiento {view.Seat}.");
            }
        }

        /// <summary>Jugada de mayor puntuación; los empates se deciden al azar (determinista con la semilla).</summary>
        public static PlayCardCommand PickBest(IReadOnlyList<PlayCardCommand> moves, IRandom rng, Func<PlayCardCommand, double> score)
        {
            var best = new List<PlayCardCommand>();
            double bestScore = double.NegativeInfinity;
            foreach (PlayCardCommand move in moves)
            {
                double value = score(move);
                if (value > bestScore + TieEpsilon)
                {
                    bestScore = value;
                    best.Clear();
                    best.Add(move);
                }
                else if (Math.Abs(value - bestScore) <= TieEpsilon)
                {
                    best.Add(move);
                }
            }

            if (best.Count == 0)
            {
                throw new InvalidOperationException("No hay jugadas legales.");
            }

            return best[rng.Next(best.Count)];
        }
    }
}
