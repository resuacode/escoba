using System.Collections.Generic;
using System.Linq;
using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class GameEngineDealTests
    {
        [TestCase(2, false)]
        [TestCase(3, false)]
        [TestCase(4, true)]
        public void StartGame_DealsThreeCardsEachAndFourToTable(int players, bool teams)
        {
            var engine = new GameEngine(Rules(players, teams), 2024);

            engine.StartGame();

            GameState state = engine.State;
            Assert.That(state.Phase, Is.EqualTo(GamePhase.Playing));
            Assert.That(state.Dealer, Is.InRange(0, players - 1));
            Assert.That(state.CurrentSeat, Is.EqualTo(state.Mano));
            for (int seat = 0; seat < players; seat++)
            {
                Assert.That(state.GetHand(seat).Count, Is.EqualTo(3));
            }

            int tableOrTakenByDealer = state.Table.Count + Enumerable.Range(0, state.SideCount).Sum(s => state.GetSide(s).Captured.Count);
            Assert.That(tableOrTakenByDealer, Is.EqualTo(4));
            Assert.That(state.DeckCount, Is.EqualTo(40 - 4 - (3 * players)));
            Assert.That(state.TotalCardCount(), Is.EqualTo(40));
        }

        [Test]
        public void StartGame_Twice_Throws()
        {
            var engine = new GameEngine(Rules(), 1);
            engine.StartGame();
            Assert.Throws<System.InvalidOperationException>(() => engine.StartGame());
        }

        [Test]
        public void DealerTableEscoba_TableSumsFifteen_DealerTakesTableWithOneEscoba()
        {
            var engine = Create(Rules(), new[] { "", "" }, table: "5o 4c 3e 3b", dealer: 1);
            var events = new List<GameEvent>();

            int escobas = engine.ApplyDealerTableEscoba(events);

            Assert.That(escobas, Is.EqualTo(1));
            Assert.That(engine.State.Table, Is.Empty);
            Assert.That(engine.State.GetSide(1).Captured.Count, Is.EqualTo(4));
            Assert.That(engine.State.GetSide(1).Escobas, Is.EqualTo(1));
            Assert.That(engine.State.LastCapturerSeat, Is.EqualTo(1));
            Assert.That(First<DealerTableEscobaEvent>(events).Escobas, Is.EqualTo(1));
        }

        [Test]
        public void DealerTableEscoba_TableSumsThirtyInTwoFifteens_DealerScoresTwoEscobas()
        {
            // 12o (10) + 5c = 15 y 7e + 10b (8) = 15.
            var engine = Create(Rules(), new[] { "", "" }, table: "12o 5c 7e 10b", dealer: 0);

            Assert.That(engine.ApplyDealerTableEscoba(new List<GameEvent>()), Is.EqualTo(2));
            Assert.That(engine.State.GetSide(0).Escobas, Is.EqualTo(2));
        }

        [Test]
        public void DealerTableEscoba_TableSumsThirtyWithoutTwoFifteens_DoesNothing()
        {
            // 10 + 10 + 9 + 1 = 30, pero ningún subconjunto suma 15.
            var engine = Create(Rules(), new[] { "", "" }, table: "12o 12c 11e 1b");

            Assert.That(engine.ApplyDealerTableEscoba(new List<GameEvent>()), Is.EqualTo(0));
            Assert.That(engine.State.Table.Count, Is.EqualTo(4));
        }

        [Test]
        public void DealerTableEscoba_OnlySubsetSumsFifteen_DoesNothing()
        {
            // 7 + sota = 15, pero la mesa entera suma 20.
            var engine = Create(Rules(), new[] { "", "" }, table: "7o 10c 2e 3b");

            Assert.That(engine.ApplyDealerTableEscoba(new List<GameEvent>()), Is.EqualTo(0));
            Assert.That(engine.State.Table.Count, Is.EqualTo(4));
        }

        [Test]
        public void StartGame_ManySeeds_DealerEscobaOnlyWhenTableSumsFifteenOrThirty()
        {
            int occurrences = 0;
            for (ulong seed = 0; seed < 500; seed++)
            {
                var engine = new GameEngine(Rules(), seed);
                var dealerEscoba = First<DealerTableEscobaEvent>(engine.StartGame());
                if (dealerEscoba == null)
                {
                    Assert.That(engine.State.Table.Count, Is.EqualTo(4));
                    continue;
                }

                occurrences++;
                int sum = dealerEscoba.Cards.Sum(c => c.Value);
                Assert.That(sum, Is.EqualTo(15 * dealerEscoba.Escobas), $"semilla {seed}");
                Assert.That(dealerEscoba.Seat, Is.EqualTo(engine.State.Dealer));
            }

            Assert.That(occurrences, Is.GreaterThan(0), "en 500 repartos debería darse alguna escoba de mano");
        }

        [Test]
        public void StartGame_WithDealerTableEscobaDisabled_NeverTakesTable()
        {
            var rules = Rules();
            rules.DealerTableEscoba = false;
            for (ulong seed = 0; seed < 500; seed++)
            {
                var engine = new GameEngine(rules, seed);
                Assert.That(First<DealerTableEscobaEvent>(engine.StartGame()), Is.Null);
                Assert.That(engine.State.Table.Count, Is.EqualTo(4));
            }
        }

        [TestCase(2, false, 6)]
        [TestCase(3, false, 4)]
        [TestCase(4, true, 3)]
        public void FullRound_DealsExpectedNumberOfTimes(int players, bool teams, int expectedDeals)
        {
            var result = GameSimulator.Play(Rules(players, teams), 77);

            // Hasta el primer RoundScored: repartos de la primera ronda.
            int handsDealt = result.Events.TakeWhile(e => !(e is RoundScoredEvent)).OfType<HandDealtEvent>().Count();
            Assert.That(handsDealt, Is.EqualTo(players * expectedDeals));
        }
    }
}
