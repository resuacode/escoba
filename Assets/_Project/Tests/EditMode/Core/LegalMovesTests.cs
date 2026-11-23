using System.Linq;
using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class LegalMovesTests
    {
        [Test]
        public void FindCaptures_WithTwoPossibleCombinations_ReturnsBoth()
        {
            // 7 necesita 8: la sota (8) o 5 + 3.
            var captures = LegalMoves.FindCaptures(C("7o"), Cards("5c 3e 10b"), 15);

            Assert.That(captures.Count, Is.EqualTo(2));
            Assert.That(captures.Any(c => c.SequenceEqual(Cards("5c 3e"))), Is.True);
            Assert.That(captures.Any(c => c.SequenceEqual(Cards("10b"))), Is.True);
        }

        [Test]
        public void FindCaptures_WithFourCards_FindsFullTable()
        {
            // 1 necesita 14 = 2 + 3 + 4 + 5.
            var captures = LegalMoves.FindCaptures(C("1o"), Cards("2c 3e 4b 5o"), 15);

            Assert.That(captures.Count, Is.EqualTo(1));
            Assert.That(captures[0].Count, Is.EqualTo(4));
        }

        [Test]
        public void FindCaptures_WhenNothingSumsFifteen_ReturnsEmpty()
        {
            Assert.That(LegalMoves.FindCaptures(C("1o"), Cards("12e 12c"), 15), Is.Empty);
            Assert.That(LegalMoves.FindCaptures(C("5o"), Cards(""), 15), Is.Empty);
        }

        [Test]
        public void FindCaptures_WithEqualValueCards_ReturnsOneCaptureEach()
        {
            // 10 necesita 5: cualquiera de los dos cincos.
            var captures = LegalMoves.FindCaptures(C("12o"), Cards("5c 5e"), 15);
            Assert.That(captures.Count, Is.EqualTo(2));
        }

        [Test]
        public void For_WithMandatoryCapture_OmitsDropForCardsThatCanCapture()
        {
            var rules = Rules();
            var moves = LegalMoves.For(0, Cards("5o 1c"), Cards("12e"), rules);

            // 5o solo puede capturar el rey; 1c solo puede dejarse.
            Assert.That(moves.Count, Is.EqualTo(2));
            Assert.That(moves.Single(m => m.CardId == C("5o").Id).IsCapture, Is.True);
            Assert.That(moves.Single(m => m.CardId == C("1c").Id).IsCapture, Is.False);
        }

        [Test]
        public void For_WithoutMandatoryCapture_IncludesDropForEveryCard()
        {
            var rules = Rules();
            rules.CaptureIsMandatory = false;
            var moves = LegalMoves.For(0, Cards("5o 1c"), Cards("12e"), rules);

            Assert.That(moves.Count, Is.EqualTo(3));
            Assert.That(moves.Count(m => !m.IsCapture), Is.EqualTo(2));
        }
    }
}
