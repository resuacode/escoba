using System.Linq;
using Escoba.Core;
using NUnit.Framework;
using static Escoba.Tests.EditMode.TestGame;

namespace Escoba.Tests.EditMode
{
    public class GameEnginePlayTests
    {
        private sealed class UnknownCommand : IGameCommand
        {
            public int Seat => 0;
        }

        [Test]
        public void Apply_CaptureSummingFifteen_MovesCardsToSideAndPassesTurn()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "5c 3e 10b");

            var events = engine.Apply(Capture(0, "7o", "5c 3e"));

            var captured = First<CardsCapturedEvent>(events);
            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.IsEscoba, Is.False);
            Assert.That(engine.State.GetSide(0).Captured, Is.EquivalentTo(Cards("7o 5c 3e")));
            Assert.That(engine.State.Table, Is.EqualTo(Cards("10b")));
            Assert.That(engine.State.LastCapturerSeat, Is.EqualTo(0));
            Assert.That(engine.State.CurrentSeat, Is.EqualTo(1));
        }

        [Test]
        public void Validate_CaptureNotSummingFifteen_IsRejected()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "5c 3e 10b");
            Assert.That(engine.Validate(Capture(0, "7o", "5c")).Error, Is.EqualTo(CommandError.CaptureSumMismatch));
        }

        [Test]
        public void Validate_CaptureCardNotOnTable_IsRejected()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "5c 3e 10b");
            Assert.That(engine.Validate(Capture(0, "7o", "10o")).Error, Is.EqualTo(CommandError.CaptureCardNotOnTable));
        }

        [Test]
        public void Validate_DuplicateCaptureCard_IsRejected()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "5c 3e 10b");
            var command = new PlayCardCommand(0, C("7o").Id, new[] { C("10b").Id, C("10b").Id });
            Assert.That(engine.Validate(command).Error, Is.EqualTo(CommandError.DuplicateCaptureCard));
        }

        [Test]
        public void Validate_TurnSeatAndCardErrors_AreRejected()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "12e");

            Assert.That(engine.Validate(Drop(1, "1c")).Error, Is.EqualTo(CommandError.NotYourTurn));
            Assert.That(engine.Validate(Drop(5, "1c")).Error, Is.EqualTo(CommandError.InvalidSeat));
            Assert.That(engine.Validate(Drop(0, "2o")).Error, Is.EqualTo(CommandError.CardNotInHand));
            Assert.That(engine.Validate(new PlayCardCommand(0, 99)).Error, Is.EqualTo(CommandError.CardNotInHand));
            Assert.That(engine.Validate(new UnknownCommand()).Error, Is.EqualTo(CommandError.UnknownCommand));
            Assert.That(engine.Validate(new StartNextRoundCommand(0)).Error, Is.EqualTo(CommandError.WrongPhase));
        }

        [Test]
        public void Apply_InvalidCommand_ThrowsAndLeavesStateUnchanged()
        {
            var engine = Create(Rules(), new[] { "7o", "1c" }, table: "5c 3e 10b");
            ulong before = engine.State.ComputeHash();

            var ex = Assert.Throws<InvalidCommandException>(() => engine.Apply(Capture(0, "7o", "5c")));

            Assert.That(ex.Error, Is.EqualTo(CommandError.CaptureSumMismatch));
            Assert.That(engine.State.ComputeHash(), Is.EqualTo(before));
        }

        [Test]
        public void Validate_DropCardThatCanCapture_IsRejectedWhenCaptureIsMandatory()
        {
            var engine = Create(Rules(), new[] { "5o 1c", "2b" }, table: "12e");

            Assert.That(engine.Validate(Drop(0, "5o")).Error, Is.EqualTo(CommandError.CaptureIsMandatory));
            Assert.That(engine.Validate(Drop(0, "1c")).IsValid, Is.True, "1c no puede capturar: se puede dejar");
            Assert.That(engine.Validate(Capture(0, "5o", "12e")).IsValid, Is.True);
        }

        [Test]
        public void Validate_DropCardThatCanCapture_IsAllowedWhenCaptureIsOptional()
        {
            var rules = Rules();
            rules.CaptureIsMandatory = false;
            var engine = Create(rules, new[] { "5o 1c", "2b" }, table: "12e");

            Assert.That(engine.Validate(Drop(0, "5o")).IsValid, Is.True);
        }

        [Test]
        public void Apply_CaptureWithFourTableCardsClearingTable_IsEscoba()
        {
            var engine = Create(Rules(), new[] { "1o", "2b" }, table: "2c 3e 4b 5o");

            var events = engine.Apply(Capture(0, "1o", "2c 3e 4b 5o"));

            Assert.That(First<CardsCapturedEvent>(events).IsEscoba, Is.True);
            Assert.That(engine.State.GetSide(0).Escobas, Is.EqualTo(1));
            Assert.That(engine.State.Table, Is.Empty);
        }

        [TestCase(true, 1)]
        [TestCase(false, 0)]
        public void Apply_LastPlayOfRoundClearingTable_CountsEscobaOnlyIfRuleEnabled(bool lastPlayEscobaCounts, int expectedEscobas)
        {
            var rules = Rules();
            rules.LastPlayEscobaCounts = lastPlayEscobaCounts;
            var engine = Create(rules, new[] { "2c", "3o" }, table: "12e");

            engine.Apply(Drop(0, "2c"));
            var events = engine.Apply(Capture(1, "3o", "12e 2c"));

            Assert.That(First<CardsCapturedEvent>(events).IsEscoba, Is.EqualTo(expectedEscobas == 1));
            Assert.That(engine.State.LastRoundScore.Sides[1].EscobaPoints, Is.EqualTo(expectedEscobas));
            Assert.That(engine.State.Phase, Is.EqualTo(GamePhase.RoundOver));
        }

        [Test]
        public void EndRound_RemainingTableCards_GoToLastCapturerWithoutEscoba()
        {
            var engine = Create(Rules(), new[] { "7o", "3c" }, table: "10o");

            engine.Apply(Capture(0, "7o", "10o"));
            var events = engine.Apply(Drop(1, "3c"));

            var awarded = First<TableAwardedToLastCapturerEvent>(events);
            Assert.That(awarded, Is.Not.Null);
            Assert.That(awarded.Seat, Is.EqualTo(0));
            Assert.That(awarded.Cards, Is.EqualTo(Cards("3c")));
            Assert.That(engine.State.Table, Is.Empty);

            // Ronda: escoba (7o+sota) + más cartas + más oros + siete de oros = 4; sin setenta (faltan palos).
            SideRoundScore side0 = engine.State.LastRoundScore.Sides[0];
            Assert.That(side0.EscobaPoints, Is.EqualTo(1));
            Assert.That(side0.CardCount, Is.EqualTo(3));
            Assert.That(side0.SetentaPoints, Is.EqualTo(0));
            Assert.That(side0.Total, Is.EqualTo(4));
            Assert.That(engine.State.GetSide(0).Score, Is.EqualTo(4));
            Assert.That(engine.State.Phase, Is.EqualTo(GamePhase.RoundOver));
            Assert.That(First<RoundScoredEvent>(events), Is.Not.Null);
        }

        [Test]
        public void EndRound_WhenNobodyCaptured_TableStaysAndNobodyScores()
        {
            var engine = Create(Rules(), new[] { "1o", "1c" });

            engine.Apply(Drop(0, "1o"));
            var events = engine.Apply(Drop(1, "1c"));

            Assert.That(First<TableAwardedToLastCapturerEvent>(events), Is.Null);
            Assert.That(engine.State.Table.Count, Is.EqualTo(2));
            Assert.That(engine.State.LastRoundScore.Sides.Sum(s => s.Total), Is.EqualTo(0));
            Assert.That(engine.State.Phase, Is.EqualTo(GamePhase.RoundOver));
        }

        [Test]
        public void EndRound_ReachingTarget_EndsGameWithWinner()
        {
            var engine = Create(Rules(), new[] { "7o", "3c" }, table: "10o", scores: new[] { 17, 0 });

            engine.Apply(Capture(0, "7o", "10o"));
            var events = engine.Apply(Drop(1, "3c"));

            Assert.That(engine.State.Phase, Is.EqualTo(GamePhase.GameOver));
            Assert.That(engine.State.WinnerSide, Is.EqualTo(0));
            Assert.That(First<GameOverEvent>(events).WinnerSide, Is.EqualTo(0));
            Assert.That(engine.Validate(Drop(0, "1o")).Error, Is.EqualTo(CommandError.WrongPhase));
        }

        [Test]
        public void Apply_WhenAllHandsEmptyAndDeckHasCards_DealsNewHandsAndManoPlays()
        {
            var engine = Create(Rules(), new[] { "1o", "1c" }, table: "12e 12c", deck: "2o 2c 2e 2b 3o 3c");

            engine.Apply(Drop(0, "1o"));
            var events = engine.Apply(Drop(1, "1c"));

            Assert.That(events.OfType<HandDealtEvent>().Count(), Is.EqualTo(2));
            Assert.That(engine.State.GetHand(0).Count, Is.EqualTo(3));
            Assert.That(engine.State.GetHand(1).Count, Is.EqualTo(3));
            Assert.That(engine.State.DeckCount, Is.EqualTo(0));
            Assert.That(engine.State.CurrentSeat, Is.EqualTo(engine.State.Mano));
            Assert.That(engine.State.Phase, Is.EqualTo(GamePhase.Playing));
        }

        [Test]
        public void Apply_WithThreePlayers_TurnRotatesThroughAllSeats()
        {
            var engine = Create(Rules(3), new[] { "1o", "1c", "1e" }, table: "12e");

            engine.Apply(Drop(0, "1o"));
            Assert.That(engine.State.CurrentSeat, Is.EqualTo(1));
            engine.Apply(Drop(1, "1c"));
            Assert.That(engine.State.CurrentSeat, Is.EqualTo(2));
        }

        [Test]
        public void Apply_WithTeams_CaptureGoesToPartnersSharedSide()
        {
            var engine = Create(Rules(4, teams: true), new[] { "1o", "1c", "5e", "1b" }, table: "12e");

            engine.Apply(Drop(0, "1o"));
            engine.Apply(Drop(1, "1c"));
            engine.Apply(Capture(2, "5e", "12e"));

            Assert.That(engine.State.SideCount, Is.EqualTo(2));
            Assert.That(engine.State.GetSide(0).Captured, Is.EquivalentTo(Cards("5e 12e")));
            Assert.That(engine.State.GetSide(1).Captured, Is.Empty);
        }

        [Test]
        public void StartNextRound_AfterRoundOver_RotatesDealerResetsRoundAndKeepsScores()
        {
            var engine = Create(Rules(), new[] { "7o", "3c" }, table: "10o");
            engine.Apply(Capture(0, "7o", "10o"));
            engine.Apply(Drop(1, "3c"));
            int previousDealer = engine.State.Dealer;

            var events = engine.Apply(new StartNextRoundCommand(0));

            GameState state = engine.State;
            Assert.That(state.Dealer, Is.EqualTo((previousDealer + 1) % 2));
            Assert.That(state.RoundNumber, Is.EqualTo(2));
            Assert.That(state.Phase, Is.EqualTo(GamePhase.Playing));
            Assert.That(state.CurrentSeat, Is.EqualTo(state.Mano));
            Assert.That(state.GetHand(0).Count, Is.EqualTo(3));
            Assert.That(state.GetHand(1).Count, Is.EqualTo(3));
            Assert.That(state.TotalCardCount(), Is.EqualTo(40));
            Assert.That(state.GetSide(0).Score, Is.EqualTo(4));
            Assert.That(state.GetSide(0).Escobas + state.GetSide(1).Escobas, Is.LessThanOrEqualTo(2), "solo puede haber escobas del reparto");
            Assert.That(First<RoundStartedEvent>(events).RoundNumber, Is.EqualTo(2));
        }
    }
}
