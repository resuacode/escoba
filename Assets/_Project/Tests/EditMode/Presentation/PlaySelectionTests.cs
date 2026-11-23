using System.Linq;
using Escoba.Core;
using Escoba.Presentation;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class PlaySelectionTests
    {
        private static PlaySelection Selection(string hand, string table, bool mandatory = true)
        {
            RulesConfig rules = Rules();
            rules.CaptureIsMandatory = mandatory;
            return new PlaySelection(Cards(hand), Cards(table), rules);
        }

        [Test]
        public void SelectHandCard_WithSingleCapture_AutoSelectsItAndIsReady()
        {
            var selection = Selection("5o 1c", "12e 3b");

            selection.SelectHandCard(C("5o"));

            Assert.That(selection.SelectedTableCards, Is.EqualTo(Cards("12e")));
            Assert.That(selection.Hint, Is.EqualTo(SelectionHint.Ready));
            Assert.That(selection.CanConfirm, Is.True);
            Assert.That(selection.Sum, Is.EqualTo(15));
        }

        [Test]
        public void SelectHandCard_WithSeveralCaptures_RequiresChoosingAndHighlightsCandidates()
        {
            // 6 se lleva el caballo (9) o 7 + 2; el rey (10) no participa.
            var selection = Selection("6c", "7c 12c 11o 2e");

            selection.SelectHandCard(C("6c"));

            Assert.That(selection.Captures.Count, Is.EqualTo(2));
            Assert.That(selection.Hint, Is.EqualTo(SelectionHint.MustCapture));
            Assert.That(selection.CanConfirm, Is.False);
            Assert.That(selection.IsCandidate(C("7c")), Is.True);
            Assert.That(selection.IsCandidate(C("11o")), Is.True);
            Assert.That(selection.IsCandidate(C("12c")), Is.False);

            Assert.That(selection.ToggleTableCard(C("12c")), Is.False, "no candidata: se ignora");
            selection.ToggleTableCard(C("7c"));
            Assert.That(selection.CanConfirm, Is.False, "7 + 6 = 13");
            selection.ToggleTableCard(C("2e"));
            Assert.That(selection.Hint, Is.EqualTo(SelectionHint.Ready));

            PlayCardCommand command = selection.BuildCommand(0);
            Assert.That(command.CaptureIds, Is.EquivalentTo(new[] { C("7c").Id, C("2e").Id }));
        }

        [Test]
        public void SelectHandCard_WithoutCaptures_WillDrop()
        {
            var selection = Selection("1o", "12e 12c");

            selection.SelectHandCard(C("1o"));

            Assert.That(selection.Hint, Is.EqualTo(SelectionHint.WillDrop));
            Assert.That(selection.CanConfirm, Is.True);
            Assert.That(selection.BuildCommand(0).IsCapture, Is.False);
        }

        [Test]
        public void SelectHandCard_SameCardTwice_Deselects()
        {
            var selection = Selection("1o", "12e");
            selection.SelectHandCard(C("1o"));
            selection.SelectHandCard(C("1o"));

            Assert.That(selection.SelectedCard, Is.Null);
            Assert.That(selection.Hint, Is.EqualTo(SelectionHint.SelectCard));
            Assert.That(selection.CanConfirm, Is.False);
        }

        [Test]
        public void OptionalCapture_AllowsDroppingACardThatCouldCapture()
        {
            var selection = Selection("6c", "7c 11o 2e", mandatory: false);
            selection.SelectHandCard(C("6c"));

            Assert.That(selection.Hint, Is.EqualTo(SelectionHint.ChooseCapture));
            Assert.That(selection.CanConfirm, Is.True);
            Assert.That(selection.BuildCommand(0).IsCapture, Is.False);
        }

        [Test]
        public void TryQuickPlay_DropsCapturesOrAsksToChoose()
        {
            var selection = Selection("1o 5o 6c", "12e 7c 11o 2e");

            Assert.That(selection.TryQuickPlay(0, C("1o")).IsCapture, Is.False, "1 no suma 15 con nada");
            Assert.That(selection.TryQuickPlay(0, C("5o")).CaptureIds, Is.EqualTo(new[] { C("12e").Id }));
            Assert.That(selection.TryQuickPlay(0, C("6c")), Is.Null, "dos capturas posibles: hay que elegir");
        }

        [Test]
        public void BuildCommand_IsAcceptedByEngine()
        {
            var engine = Create(Rules(), new[] { "6c 1o", "2b" }, table: "7c 12c 11o 2e");
            var selection = new PlaySelection(engine.GetView(0).Hand, engine.GetView(0).Table, engine.State.Rules);

            selection.SelectHandCard(C("6c"));
            selection.ToggleTableCard(C("11o"));

            Assert.That(engine.Validate(selection.BuildCommand(0)).IsValid, Is.True);
        }

        [Test]
        public void Changed_IsRaisedOnEverySelectionChange()
        {
            var selection = Selection("6c", "7c 11o 2e");
            int changes = 0;
            selection.Changed += () => changes++;

            selection.SelectHandCard(C("6c"));
            selection.ToggleTableCard(C("11o"));
            selection.Clear();

            Assert.That(changes, Is.EqualTo(3));
            Assert.That(selection.SelectedTableCards.Any(), Is.False);
        }
    }
}
