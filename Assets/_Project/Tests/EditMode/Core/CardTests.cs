using System;
using System.Linq;
using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class CardTests
    {
        [Test]
        public void CreateOrdered_Always_Returns40UniqueCardsWithIdsInOrder()
        {
            var deck = Deck.CreateOrdered();

            Assert.That(deck.Count, Is.EqualTo(40));
            Assert.That(deck.Select(c => c.Id), Is.EqualTo(Enumerable.Range(0, 40)));
            Assert.That(deck.Count(c => c.Suit == Suit.Oros), Is.EqualTo(10));
        }

        [TestCase(1, 1)]
        [TestCase(7, 7)]
        [TestCase(10, 8)]
        [TestCase(11, 9)]
        [TestCase(12, 10)]
        public void Value_ForRank_MatchesEscobaValue(int rank, int expectedValue)
        {
            Assert.That(Card.Of(Suit.Copas, rank).Value, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Of_WithRankEightOrNine_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Card.Of(Suit.Oros, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => Card.Of(Suit.Oros, 9));
        }

        [Test]
        public void FromId_OutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Card.FromId(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Card.FromId(40));
        }

        [Test]
        public void Of_ThenId_RoundTripsForAllCards()
        {
            foreach (Card card in Deck.CreateOrdered())
            {
                Assert.That(Card.Of(card.Suit, card.Rank), Is.EqualTo(card));
            }
        }

        [Test]
        public void IsGoldSeven_OnlyForSevenOfOros()
        {
            Assert.That(Deck.CreateOrdered().Count(c => c.IsGoldSeven), Is.EqualTo(1));
            Assert.That(C("7o").IsGoldSeven, Is.True);
            Assert.That(C("7c").IsGoldSeven, Is.False);
        }

        [Test]
        public void SetentaStrength_FollowsClassicOrder()
        {
            string[] order = { "7o", "6o", "1o", "5o", "4o", "3o", "2o" };
            for (int i = 1; i < order.Length; i++)
            {
                Assert.That(C(order[i - 1]).SetentaStrength, Is.GreaterThan(C(order[i]).SetentaStrength), order[i - 1]);
            }

            Assert.That(C("2o").SetentaStrength, Is.GreaterThan(C("10o").SetentaStrength));
            Assert.That(C("10o").SetentaStrength, Is.EqualTo(C("11o").SetentaStrength));
            Assert.That(C("11o").SetentaStrength, Is.EqualTo(C("12o").SetentaStrength));
        }
    }
}
