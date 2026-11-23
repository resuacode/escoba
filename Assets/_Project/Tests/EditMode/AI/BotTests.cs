using System.Collections.Generic;
using System.Linq;
using Escoba.AI;
using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    /// <summary>Comportamiento de los bots Medio y Difícil y utilidades de conocimiento.</summary>
    public class BotTests
    {
        private static IEnumerable<TestCaseData> SmartBots()
        {
            yield return new TestCaseData(new MediumBot()).SetName("{m}(Medio)");
            yield return new TestCaseData(new HardBot()).SetName("{m}(Difícil)");
        }

        private static PlayCardCommand Choose(IBotStrategy bot, GameEngine engine, ulong seed = 1)
        {
            return bot.ChooseCommand(engine.GetView(engine.State.CurrentSeat), new Pcg32Random(seed));
        }

        [TestCaseSource(nameof(SmartBots))]
        public void Bot_TakesEscobaWhenAvailable(IBotStrategy bot)
        {
            // 5 + rey (10) = 15 y deja la mesa vacía.
            var engine = Create(Rules(), new[] { "5o 1c 2e", "3c 4c 6c" }, table: "12e", deck: "1o 2o 3o 4o 5c 6o");

            PlayCardCommand move = Choose(bot, engine);

            Assert.That(move.CardId, Is.EqualTo(C("5o").Id));
            Assert.That(move.CaptureIds, Is.EqualTo(new[] { C("12e").Id }));
        }

        [TestCaseSource(nameof(SmartBots))]
        public void Bot_PrefersCapturingTheGoldSeven(IBotStrategy bot)
        {
            // La sota (8) puede llevarse el 7 de oros o el 7 de copas.
            var engine = Create(Rules(), new[] { "10b 12c 11c", "3c 4c 6c" }, table: "7o 1c 7c", deck: "1o 2o 3o 4o 5c 6o");

            PlayCardCommand move = Choose(bot, engine);

            Assert.That(move.CardId, Is.EqualTo(C("10b").Id));
            Assert.That(move.CaptureIds, Is.EqualTo(new[] { C("7o").Id }));
        }

        [TestCaseSource(nameof(SmartBots))]
        public void Bot_AvoidsLeavingAnEscobaToTheOpponent(IBotStrategy bot)
        {
            // Ninguna carta captura. Dejar el 5 deja la mesa en 7 (una sota hace escoba);
            // dejar el 1 deja la mesa en 3 (haría falta un 12: imposible).
            var engine = Create(Rules(), new[] { "1c 5e", "3c 4c 6c" }, table: "2o", deck: "1o 2b 3o 4o 5c 6o");

            PlayCardCommand move = Choose(bot, engine);

            Assert.That(move.IsCapture, Is.False);
            Assert.That(move.CardId, Is.EqualTo(C("1c").Id));
        }

        [Test]
        public void SmartBots_AlwaysPlayLegalMovesInFullGames()
        {
            for (int players = 2; players <= 4; players++)
            {
                var rules = Rules(players, teams: players == 4);
                for (ulong seed = 0; seed < 5; seed++)
                {
                    // Asientos alternos Medio/Difícil; el motor lanza si alguna jugada es ilegal.
                    var result = GameSimulator.Play(rules, seed, null, seat => seat % 2 == 0 ? (IBotStrategy)new MediumBot() : new HardBot());
                    Assert.That(result.Engine.State.Phase, Is.EqualTo(GamePhase.GameOver));
                }
            }
        }

        [Test]
        public void HardBot_SameSeed_SameDecision()
        {
            var engine = Create(Rules(), new[] { "1c 5e 3b", "3c 4c 6c" }, table: "2o 6e", deck: "1o 2b 3o 4o 5c 6o");
            Assert.That(Choose(new HardBot(), engine, 42).ToString(), Is.EqualTo(Choose(new HardBot(), engine, 42).ToString()));
        }

        [Test]
        public void UnseenCards_ExcludeOwnHandTableAndCapturedCards()
        {
            var engine = new GameEngine(Rules(), 99);
            engine.StartGame();
            PlayerView view = engine.GetView(0);

            List<Card> unseen = BotKnowledge.UnseenCards(view);

            Assert.That(unseen.Count, Is.EqualTo(40 - 3 - 4), "40 − mano − mesa inicial (o la baza del repartidor)");
            Assert.That(unseen.Intersect(view.Hand), Is.Empty);
            Assert.That(unseen.Intersect(view.Table), Is.Empty);
            Assert.That(unseen.Intersect(engine.State.GetHand(1)).Count(), Is.EqualTo(3), "la mano rival está entre las no vistas");
        }

        [TestCase(10, 0, 3, 0.0)]
        [TestCase(10, 10, 1, 1.0)]
        [TestCase(4, 1, 1, 0.25)]
        [TestCase(4, 2, 2, 5.0 / 6.0)]
        [TestCase(30, 4, 3, 1 - (26.0 * 25 * 24) / (30.0 * 29 * 28))]
        public void ProbabilityOfAtLeastOne_IsHypergeometric(int unseen, int matching, int hand, double expected)
        {
            Assert.That(BotKnowledge.ProbabilityOfAtLeastOne(unseen, matching, hand), Is.EqualTo(expected).Within(1e-9));
        }

        [Test]
        public void BotFactory_AllLevelsAvailable()
        {
            Assert.That(BotFactory.Create(BotDifficulty.Easy), Is.InstanceOf<EasyBot>());
            Assert.That(BotFactory.Create(BotDifficulty.Medium), Is.InstanceOf<MediumBot>());
            Assert.That(BotFactory.Create(BotDifficulty.Hard), Is.InstanceOf<HardBot>());
            Assert.That(BotFactory.IsAvailable(BotDifficulty.Hard), Is.True);
        }
    }
}
