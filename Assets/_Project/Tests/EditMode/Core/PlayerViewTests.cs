using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class PlayerViewTests
    {
        [Test]
        public void GetView_ShowsOwnHandAndOnlyCountsOfOthers()
        {
            var engine = Create(Rules(3), new[] { "1o 2o", "3c", "4e 5e 6e" }, table: "12b", deck: "7b 1b");

            PlayerView view = engine.GetView(1);

            Assert.That(view.Seat, Is.EqualTo(1));
            Assert.That(view.Hand, Is.EqualTo(Cards("3c")));
            Assert.That(view.HandCounts, Is.EqualTo(new[] { 2, 1, 3 }));
            Assert.That(view.DeckCount, Is.EqualTo(2));
            Assert.That(view.Table, Is.EqualTo(Cards("12b")));
            Assert.That(view.IsMyTurn, Is.False);
            Assert.That(engine.GetView(0).IsMyTurn, Is.True);
        }

        [Test]
        public void GetView_IsSnapshotNotAffectedByLaterCommands()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "5c 3e 10b");
            PlayerView before = engine.GetView(0);

            engine.Apply(Capture(0, "7o", "5c 3e"));

            Assert.That(before.Hand, Is.EqualTo(Cards("7o")));
            Assert.That(before.Table.Count, Is.EqualTo(3));
            Assert.That(engine.GetView(0).Hand, Is.Empty);
        }

        [Test]
        public void GetView_RulesAreACopy()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" });
            engine.GetView(0).Rules.CaptureSum = 99;
            Assert.That(engine.State.Rules.CaptureSum, Is.EqualTo(15));
        }

        [Test]
        public void HandDealtEvent_IsPrivateToItsSeat()
        {
            var events = new GameEngine(Rules(), 3).StartGame();
            foreach (GameEvent e in events)
            {
                if (e is HandDealtEvent dealt)
                {
                    Assert.That(e.IsPrivateTo, Is.EqualTo(dealt.Seat));
                }
                else
                {
                    Assert.That(e.IsPrivateTo, Is.EqualTo(-1), e.GetType().Name);
                }
            }
        }
    }
}
