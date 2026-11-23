using System;
using System.Collections.Generic;
using System.Diagnostics;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Torneo de balance entre dos estrategias en partidas 1 contra 1 completas, deterministas
    /// (semillas consecutivas) y alternando asientos para anular la ventaja del mano.
    /// Lo usan los tests de balance y el menú de editor <c>Escoba/AI/Run Balance Tournament</c>.
    /// </summary>
    public static class BotTournament
    {
        private const int MaxCommandsPerGame = 20000;

        public static TournamentResult Run(RulesConfig rules, Func<IBotStrategy> createA, Func<IBotStrategy> createB, int games, ulong seedBase = 0)
        {
            if (rules == null || rules.PlayerCount != 2)
            {
                throw new ArgumentException("El torneo es 1 contra 1 (PlayerCount = 2).", nameof(rules));
            }

            var result = new TournamentResult();
            double pointsA = 0, pointsB = 0, escobasA = 0, escobasB = 0;
            double timeA = 0, timeB = 0;
            long decisionsA = 0, decisionsB = 0;
            var stopwatch = new Stopwatch();

            for (int game = 0; game < games; game++)
            {
                ulong seed = seedBase + (ulong)game;
                int seatA = game % 2;
                var bots = new IBotStrategy[2];
                bots[seatA] = createA();
                bots[1 - seatA] = createB();
                var rngs = new IRandom[] { new Pcg32Random(seed, 1000), new Pcg32Random(seed, 1001) };

                var engine = new GameEngine(rules, seed);
                engine.StartGame();
                int commands = 0;
                while (engine.State.Phase != GamePhase.GameOver)
                {
                    if (++commands > MaxCommandsPerGame)
                    {
                        throw new InvalidOperationException($"La partida con semilla {seed} no termina.");
                    }

                    IReadOnlyList<GameEvent> events;
                    if (engine.State.Phase == GamePhase.RoundOver)
                    {
                        events = engine.Apply(new StartNextRoundCommand(0));
                    }
                    else
                    {
                        int seat = engine.State.CurrentSeat;
                        stopwatch.Restart();
                        PlayCardCommand command = bots[seat].ChooseCommand(engine.GetView(seat), rngs[seat]);
                        stopwatch.Stop();
                        double ms = stopwatch.Elapsed.TotalMilliseconds;
                        result.MaxDecisionMs = Math.Max(result.MaxDecisionMs, ms);
                        if (seat == seatA)
                        {
                            timeA += ms;
                            decisionsA++;
                        }
                        else
                        {
                            timeB += ms;
                            decisionsB++;
                        }

                        events = engine.Apply(command);
                    }

                    foreach (GameEvent gameEvent in events)
                    {
                        if (gameEvent is RoundScoredEvent scored)
                        {
                            result.Rounds++;
                            pointsA += scored.Score.Sides[seatA].Total;
                            pointsB += scored.Score.Sides[1 - seatA].Total;
                            escobasA += scored.Score.Sides[seatA].EscobaPoints;
                            escobasB += scored.Score.Sides[1 - seatA].EscobaPoints;
                        }
                    }
                }

                result.Games++;
                if (engine.State.WinnerSide == seatA)
                {
                    result.WinsA++;
                }
                else
                {
                    result.WinsB++;
                }
            }

            int rounds = Math.Max(1, result.Rounds);
            result.PointsPerRoundA = pointsA / rounds;
            result.PointsPerRoundB = pointsB / rounds;
            result.EscobasPerRoundA = escobasA / rounds;
            result.EscobasPerRoundB = escobasB / rounds;
            result.AverageDecisionMsA = decisionsA == 0 ? 0 : timeA / decisionsA;
            result.AverageDecisionMsB = decisionsB == 0 ? 0 : timeB / decisionsB;
            return result;
        }
    }
}
