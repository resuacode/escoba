using Escoba.Presentation;
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    public class SeatLayoutTests
    {
        [TestCase(2, new[] { SeatLayout.Bottom, SeatLayout.Top })]
        [TestCase(3, new[] { SeatLayout.Bottom, SeatLayout.Right, SeatLayout.Left })]
        [TestCase(4, new[] { SeatLayout.Bottom, SeatLayout.Right, SeatLayout.Top, SeatLayout.Left })]
        public void SlotOf_FromSeatZero_PlacesNextSeatToTheRight(int players, int[] expected)
        {
            for (int seat = 0; seat < players; seat++)
            {
                Assert.That(SeatLayout.SlotOf(seat, 0, players), Is.EqualTo(expected[seat]), $"asiento {seat}");
            }
        }

        [Test]
        public void SlotOf_ViewerIsAlwaysAtTheBottom()
        {
            for (int players = 2; players <= 4; players++)
            {
                for (int viewer = 0; viewer < players; viewer++)
                {
                    Assert.That(SeatLayout.SlotOf(viewer, viewer, players), Is.EqualTo(SeatLayout.Bottom));
                }
            }
        }

        [Test]
        public void SlotOf_FourPlayers_PartnerSitsOpposite()
        {
            // Con parejas (0-2 y 1-3) el compañero del que mira queda arriba.
            Assert.That(SeatLayout.SlotOf(3, 1, 4), Is.EqualTo(SeatLayout.Top));
            Assert.That(SeatLayout.SlotOf(2, 0, 4), Is.EqualTo(SeatLayout.Top));
        }

        [Test]
        public void SlotOf_DifferentSeatsNeverShareASlot()
        {
            for (int players = 2; players <= 4; players++)
            {
                for (int viewer = 0; viewer < players; viewer++)
                {
                    var used = new bool[SeatLayout.SlotCount];
                    for (int seat = 0; seat < players; seat++)
                    {
                        int slot = SeatLayout.SlotOf(seat, viewer, players);
                        Assert.That(used[slot], Is.False, $"{players} jugadores, viewer {viewer}, asiento {seat}");
                        used[slot] = true;
                    }
                }
            }
        }

        [Test]
        public void IsVertical_OnlyForSideSlots()
        {
            Assert.That(SeatLayout.IsVertical(SeatLayout.Left), Is.True);
            Assert.That(SeatLayout.IsVertical(SeatLayout.Right), Is.True);
            Assert.That(SeatLayout.IsVertical(SeatLayout.Top), Is.False);
            Assert.That(SeatLayout.IsVertical(SeatLayout.Bottom), Is.False);
        }
    }
}
