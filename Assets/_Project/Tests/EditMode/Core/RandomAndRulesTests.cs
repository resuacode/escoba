using System;
using System.Linq;
using Escoba.Core;
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    public class RandomAndRulesTests
    {
        [Test]
        public void Pcg32Random_SameSeed_ProducesSameSequence()
        {
            var a = new Pcg32Random(123);
            var b = new Pcg32Random(123);
            for (int i = 0; i < 100; i++)
            {
                Assert.That(a.Next(1000), Is.EqualTo(b.Next(1000)));
            }
        }

        [Test]
        public void Pcg32Random_DifferentSeeds_ProduceDifferentSequences()
        {
            var a = new Pcg32Random(1);
            var b = new Pcg32Random(2);
            int[] sa = Enumerable.Range(0, 20).Select(_ => a.Next(1000)).ToArray();
            int[] sb = Enumerable.Range(0, 20).Select(_ => b.Next(1000)).ToArray();
            Assert.That(sa, Is.Not.EqualTo(sb));
        }

        [Test]
        public void Pcg32Random_FromState_ContinuesSameSequence()
        {
            var a = new Pcg32Random(99);
            a.Next(10);
            var b = Pcg32Random.FromState(a.State, a.Increment);
            for (int i = 0; i < 50; i++)
            {
                Assert.That(b.Next(1 << 20), Is.EqualTo(a.Next(1 << 20)));
            }
        }

        [Test]
        public void Pcg32Random_Next_StaysWithinBoundsAndCoversAllValues()
        {
            var rng = new Pcg32Random(7);
            var seen = new bool[6];
            for (int i = 0; i < 1000; i++)
            {
                int v = rng.Next(6);
                Assert.That(v, Is.InRange(0, 5));
                seen[v] = true;
            }

            Assert.That(seen, Is.All.True);
            Assert.Throws<ArgumentOutOfRangeException>(() => rng.Next(0));
        }

        [Test]
        public void Shuffle_SameSeed_IsDeterministicAndKeepsAllCards()
        {
            var a = Deck.CreateOrdered();
            var b = Deck.CreateOrdered();
            Deck.Shuffle(a, new Pcg32Random(5));
            Deck.Shuffle(b, new Pcg32Random(5));

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.Select(c => c.Id).OrderBy(id => id), Is.EqualTo(Enumerable.Range(0, 40)));
            Assert.That(a.Select(c => c.Id), Is.Not.EqualTo(Enumerable.Range(0, 40)));
        }

        [Test]
        public void Validate_DefaultRules_IsValid()
        {
            Assert.DoesNotThrow(() => new RulesConfig().Validate());
            Assert.DoesNotThrow(() => new RulesConfig { PlayerCount = 3 }.Validate());
            Assert.DoesNotThrow(() => new RulesConfig { PlayerCount = 4, Teams = true }.Validate());
        }

        [Test]
        public void Validate_InvalidConfigurations_Throw()
        {
            Assert.Throws<ArgumentException>(() => new RulesConfig { PlayerCount = 1 }.Validate());
            Assert.Throws<ArgumentException>(() => new RulesConfig { PlayerCount = 5 }.Validate());
            Assert.Throws<ArgumentException>(() => new RulesConfig { PlayerCount = 3, Teams = true }.Validate());
            Assert.Throws<ArgumentException>(() => new RulesConfig { TargetScore = 0 }.Validate());
            Assert.Throws<ArgumentException>(() => new RulesConfig { HandSize = 4 }.Validate(), "36 cartas no se reparten en bloques de 8");
        }

        [Test]
        public void Defaults_MatchDecidedRules()
        {
            var rules = new RulesConfig();
            Assert.That(rules.TargetScore, Is.EqualTo(21));
            Assert.That(rules.CaptureIsMandatory, Is.True);
            Assert.That(rules.DealerTableEscoba, Is.True);
            Assert.That(rules.LastPlayEscobaCounts, Is.True);
            Assert.That(rules.SetentaTieRule, Is.EqualTo(SetentaTieRule.NoPoint));
            Assert.That(rules.CaptureSum, Is.EqualTo(15));
        }

        [Test]
        public void SideOfSeat_WithTeams_PairsOppositeSeats()
        {
            var rules = new RulesConfig { PlayerCount = 4, Teams = true };
            Assert.That(rules.SideCount, Is.EqualTo(2));
            Assert.That(new[] { 0, 1, 2, 3 }.Select(rules.SideOfSeat), Is.EqualTo(new[] { 0, 1, 0, 1 }));
        }
    }
}
