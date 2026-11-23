using System.Collections.Generic;
using System.Linq;
using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class RoundScorerTests
    {
        private static RoundScore Score(RulesConfig rules, string sideA, string sideB, int escobasA = 0, int escobasB = 0)
        {
            var captured = new List<IReadOnlyList<Card>> { Cards(sideA), Cards(sideB) };
            return RoundScorer.Score(rules, captured, new[] { escobasA, escobasB });
        }

        [Test]
        public void Score_EvenSplitOfDeck_TiesOnCardsAndGoldsSoNobodyScoresThem()
        {
            // Ids pares para A e impares para B: 20 cartas y 5 oros cada uno.
            // Los cuatro sietes (ids 6, 16, 26, 36) y el siete de oros son pares: todos para A.
            var deck = Deck.CreateOrdered();
            var a = deck.Where(c => c.Id % 2 == 0).ToArray();
            var b = deck.Where(c => c.Id % 2 == 1).ToArray();

            var score = RoundScorer.Score(Rules(), new IReadOnlyList<Card>[] { a, b }, new[] { 0, 0 });

            SideRoundScore sa = score.Sides[0];
            SideRoundScore sb = score.Sides[1];
            Assert.That(sa.CardCount, Is.EqualTo(20));
            Assert.That(sa.GoldCount, Is.EqualTo(5));
            Assert.That(sa.MostCardsPoints + sb.MostCardsPoints, Is.EqualTo(0));
            Assert.That(sa.MostGoldsPoints + sb.MostGoldsPoints, Is.EqualTo(0));
            Assert.That(sa.GoldSevenPoints, Is.EqualTo(1));
            Assert.That(sa.SetentaPoints, Is.EqualTo(1));
            Assert.That(sa.SevenCount, Is.EqualTo(4));
            Assert.That(sa.Total, Is.EqualTo(2));
            Assert.That(sb.Total, Is.EqualTo(0));
        }

        [Test]
        public void Score_MostCardsMostGoldsGoldSevenAndEscobas_AreAwarded()
        {
            var score = Score(Rules(), "7o 1o 2o 3c", "4e", escobasA: 2, escobasB: 1);

            SideRoundScore a = score.Sides[0];
            Assert.That(a.EscobaPoints, Is.EqualTo(2));
            Assert.That(a.MostCardsPoints, Is.EqualTo(1));
            Assert.That(a.MostGoldsPoints, Is.EqualTo(1));
            Assert.That(a.GoldSevenPoints, Is.EqualTo(1));
            Assert.That(a.Total, Is.EqualTo(5));
            Assert.That(score.Sides[1].Total, Is.EqualTo(1));
        }

        [Test]
        public void Setenta_MoreSevens_Wins()
        {
            // [7,7,5,2] contra [7,6,6,6].
            var score = Score(Rules(), "7o 7c 5e 2b", "7e 6o 6c 6b");
            Assert.That(score.Sides[0].SetentaPoints, Is.EqualTo(1));
            Assert.That(score.Sides[1].SetentaPoints, Is.EqualTo(0));
        }

        [Test]
        public void Setenta_TieOnSevens_DecidedByNextHighestCard()
        {
            // [7,6,1,F] contra [7,6,5,4]: el as supera al cinco en tercera posición.
            var score = Score(Rules(), "7o 6c 1e 10b", "7c 6o 5e 4b");
            Assert.That(score.Sides[0].SetentaPoints, Is.EqualTo(1));
            Assert.That(score.Sides[1].SetentaPoints, Is.EqualTo(0));
        }

        [Test]
        public void Setenta_UsesBestCardOfEachSuit()
        {
            // A tiene varias cartas por palo: su setenta debe ser 7c, 6o, 1e, 2b.
            var score = Score(Rules(), "6o 2o 7c 12c 1e 5e 2b 11b", "7o 5c 5e 5b");

            Assert.That(score.Sides[0].SetentaCards.Select(c => c.SetentaStrength), Is.EqualTo(new[] { 8, 7, 6, 2 }));
            Assert.That(score.Sides[0].SetentaPoints, Is.EqualTo(1), "[7,6,1,2] gana a [7,5,5,5]");
        }

        [TestCase(SetentaTieRule.NoPoint, 0)]
        [TestCase(SetentaTieRule.BothScore, 1)]
        public void Setenta_ExactTie_AppliesTieRule(SetentaTieRule tieRule, int expectedEach)
        {
            var rules = Rules();
            rules.SetentaTieRule = tieRule;

            // [7,6,1,F] contra [7,6,1,F] (figuras distintas pero equivalentes).
            var score = Score(rules, "7o 6c 1e 12b", "7c 6o 1b 11e");

            Assert.That(score.Sides[0].SetentaPoints, Is.EqualTo(expectedEach));
            Assert.That(score.Sides[1].SetentaPoints, Is.EqualTo(expectedEach));
        }

        [Test]
        public void Setenta_MissingSuit_CannotWin()
        {
            // A tiene tres sietes pero ningún basto; B tiene una setenta completa de doses.
            var score = Score(Rules(), "7o 7c 7e", "2o 2c 2e 2b");

            Assert.That(score.Sides[0].SetentaCards, Is.Null);
            Assert.That(score.Sides[0].SetentaPoints, Is.EqualTo(0));
            Assert.That(score.Sides[1].SetentaPoints, Is.EqualTo(1));
        }

        [Test]
        public void Setenta_NobodyComplete_NobodyScores()
        {
            var score = Score(Rules(), "7o 7c", "7e 7b");
            Assert.That(score.Sides.Sum(s => s.SetentaPoints), Is.EqualTo(0));
        }

        [Test]
        public void Setenta_Compare_IsAntisymmetric()
        {
            var a = Setenta.Build(Cards("7o 7c 5e 2b"));
            var b = Setenta.Build(Cards("7e 6o 6c 6b"));
            Assert.That(Setenta.Compare(a, b), Is.GreaterThan(0));
            Assert.That(Setenta.Compare(b, a), Is.LessThan(0));
            Assert.That(Setenta.Compare(a, a), Is.EqualTo(0));
            Assert.That(Setenta.Compare(null, a), Is.LessThan(0));
        }

        [Test]
        public void Bonuses_AllSevensAndAllGolds_AwardConfiguredPoints()
        {
            var rules = Rules();
            rules.AllSevensBonus = 2;
            rules.AllGoldsBonus = 3;

            var score = Score(rules, "1o 2o 3o 4o 5o 6o 7o 10o 11o 12o 7c 7e 7b", "1c");

            SideRoundScore a = score.Sides[0];
            Assert.That(a.AllSevensBonusPoints, Is.EqualTo(2));
            Assert.That(a.AllGoldsBonusPoints, Is.EqualTo(3));
            Assert.That(a.MostGoldsPoints, Is.EqualTo(0), "los diez oros sustituyen al punto de más oros");
        }

        [Test]
        public void Bonuses_Disabled_ByDefault()
        {
            var score = Score(Rules(), "1o 2o 3o 4o 5o 6o 7o 10o 11o 12o 7c 7e 7b", "1c");

            Assert.That(score.Sides[0].AllSevensBonusPoints, Is.EqualTo(0));
            Assert.That(score.Sides[0].AllGoldsBonusPoints, Is.EqualTo(0));
            Assert.That(score.Sides[0].MostGoldsPoints, Is.EqualTo(1));
        }

        [Test]
        public void Score_ThreeSides_UniqueMaximumRequired()
        {
            var captured = new List<IReadOnlyList<Card>> { Cards("1c 2c"), Cards("3c 4c"), Cards("5c") };
            var score = RoundScorer.Score(Rules(3), captured, new[] { 0, 0, 0 });

            Assert.That(score.Sides.Sum(s => s.MostCardsPoints), Is.EqualTo(0), "empate 2-2 en cabeza");
        }

        [TestCase(new[] { 21, 10 }, 21, 0)]
        [TestCase(new[] { 23, 22 }, 21, 0)]
        [TestCase(new[] { 22, 25 }, 21, 1)]
        [TestCase(new[] { 22, 22 }, 21, -1)]
        [TestCase(new[] { 20, 19 }, 21, -1)]
        [TestCase(new[] { 25, 25, 10 }, 21, -1)]
        [TestCase(new[] { 5, 16, 15 }, 15, 1)]
        public void DetermineWinner_ReturnsUniqueLeaderAtOrAboveTarget(int[] scores, int target, int expected)
        {
            Assert.That(GameEngine.DetermineWinner(scores, target), Is.EqualTo(expected));
        }
    }
}
