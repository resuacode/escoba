using System.Collections.Generic;
using System.Linq;
using Escoba.Core;
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    /// <summary>Partidas completas bot contra bot: determinismo e invariantes.</summary>
    public class SimulationTests
    {
        private const int GamesPerConfiguration = 50;

        private static IEnumerable<TestCaseData> Configurations()
        {
            yield return new TestCaseData(2, false);
            yield return new TestCaseData(3, false);
            yield return new TestCaseData(4, false);
            yield return new TestCaseData(4, true);
        }

        [TestCaseSource(nameof(Configurations))]
        public void Play_SameSeedTwice_ProducesIdenticalGame(int players, bool teams)
        {
            var rules = new RulesConfig { PlayerCount = players, Teams = teams };

            var a = GameSimulator.Play(rules, 12345);
            var b = GameSimulator.Play(rules, 12345);
            var c = GameSimulator.Play(rules, 54321);

            Assert.That(b.Engine.State.ComputeHash(), Is.EqualTo(a.Engine.State.ComputeHash()));
            Assert.That(b.Commands, Is.EqualTo(a.Commands));
            Assert.That(b.Events.Count, Is.EqualTo(a.Events.Count));
            Assert.That(c.Engine.State.ComputeHash(), Is.Not.EqualTo(a.Engine.State.ComputeHash()));
        }

        [TestCaseSource(nameof(Configurations))]
        public void Play_ManySeeds_KeepsInvariantsAndEndsWithValidWinner(int players, bool teams)
        {
            var rules = new RulesConfig { PlayerCount = players, Teams = teams };
            int totalEscobas = 0;

            for (ulong seed = 0; seed < GamesPerConfiguration; seed++)
            {
                int[] lastScores = new int[rules.SideCount];
                var result = GameSimulator.Play(rules, seed, (engine, command, events) =>
                {
                    GameState state = engine.State;
                    AssertAllCardsAccountedFor(state, seed);

                    if (state.Phase == GamePhase.Playing)
                    {
                        Assert.That(state.GetHand(state.CurrentSeat).Count, Is.GreaterThan(0), $"semilla {seed}: le toca a quien no tiene cartas");
                    }

                    for (int side = 0; side < state.SideCount; side++)
                    {
                        Assert.That(state.GetSide(side).Score, Is.GreaterThanOrEqualTo(lastScores[side]), $"semilla {seed}: la puntuación no puede bajar");
                        lastScores[side] = state.GetSide(side).Score;
                    }
                });

                totalEscobas += result.Events.OfType<CardsCapturedEvent>().Count(e => e.IsEscoba);
                GameState final = result.Engine.State;
                int[] scores = Enumerable.Range(0, final.SideCount).Select(s => final.GetSide(s).Score).ToArray();
                Assert.That(final.WinnerSide, Is.InRange(0, final.SideCount - 1));
                Assert.That(scores[final.WinnerSide], Is.GreaterThanOrEqualTo(rules.TargetScore), $"semilla {seed}");
                Assert.That(scores.Count(s => s == scores[final.WinnerSide]), Is.EqualTo(1), $"semilla {seed}: empate en cabeza");
            }

            Assert.That(totalEscobas, Is.GreaterThan(0), "en tantas partidas debería haber escobas");
        }

        [TestCaseSource(nameof(Configurations))]
        public void LegalMoves_DuringRealGames_AreAllAcceptedByEngine(int players, bool teams)
        {
            var rules = new RulesConfig { PlayerCount = players, Teams = teams };
            for (ulong seed = 0; seed < 10; seed++)
            {
                GameSimulator.Play(rules, seed, (engine, command, events) =>
                {
                    if (engine.State.Phase != GamePhase.Playing)
                    {
                        return;
                    }

                    PlayerView view = engine.GetView(engine.State.CurrentSeat);
                    IReadOnlyList<PlayCardCommand> moves = LegalMoves.For(view);
                    Assert.That(moves, Is.Not.Empty);
                    foreach (PlayCardCommand move in moves)
                    {
                        Assert.That(engine.Validate(move).IsValid, Is.True, $"semilla {seed}: {move}");
                    }
                });
            }
        }

        private static void AssertAllCardsAccountedFor(GameState state, ulong seed)
        {
            var ids = new List<int>();
            for (int seat = 0; seat < state.PlayerCount; seat++)
            {
                ids.AddRange(state.GetHand(seat).Select(c => c.Id));
            }

            for (int side = 0; side < state.SideCount; side++)
            {
                ids.AddRange(state.GetSide(side).Captured.Select(c => c.Id));
            }

            ids.AddRange(state.Table.Select(c => c.Id));
            ids.AddRange(state.DeckList.Select(c => c.Id));

            Assert.That(ids.Count, Is.EqualTo(40), $"semilla {seed}: total de cartas");
            Assert.That(ids.Distinct().Count(), Is.EqualTo(40), $"semilla {seed}: cartas duplicadas");
        }
    }
}
