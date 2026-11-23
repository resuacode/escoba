using System;
using System.Linq;
using Escoba.AI;
using Escoba.App;
using Escoba.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Escoba.Tests.EditMode
{
    /// <summary>Configuración de partida (asientos, parejas) y nombres de jugadores y bandos.</summary>
    public class MatchSetupTests
    {
        private static readonly SeatType H = SeatType.Human;
        private static readonly SeatType B = SeatType.Bot;

        [SetUp]
        public void LoadSpanish()
        {
            var tables = new[] { "es", "gl", "en" }
                .Select(code => AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/_Project/Localization/{code}.json"))
                .Select(asset => LocalizationTable.FromJson(asset.text));
            Localization.Initialize(new LocalizationService(tables, "es"));
        }

        [TearDown]
        public void ResetLocalization()
        {
            Localization.Initialize(null);
        }

        [Test]
        public void MatchConfig_InvalidSeatsOrTeams_Throw()
        {
            Assert.Throws<ArgumentException>(() => new MatchConfig(null, BotDifficulty.Easy, new[] { H }));
            Assert.Throws<ArgumentException>(() => new MatchConfig(null, BotDifficulty.Easy, new[] { H, B, B, B, B }));
            Assert.Throws<ArgumentException>(() => new MatchConfig(null, BotDifficulty.Easy, new[] { H, B, B }, teams: true));
            Assert.DoesNotThrow(() => new MatchConfig(null, BotDifficulty.Easy, new[] { H, B, H, B }, teams: true));
        }

        [Test]
        public void MatchConfig_HotSeatOnlyWithTwoOrMoreHumans()
        {
            Assert.That(MatchConfig.Versus(null, BotDifficulty.Easy, 4).IsHotSeat, Is.False);
            Assert.That(new MatchConfig(null, BotDifficulty.Easy, new[] { H, B, H }).IsHotSeat, Is.True);
            Assert.That(new MatchConfig(null, BotDifficulty.Easy, new[] { H, H }).HumanCount, Is.EqualTo(2));
        }

        [Test]
        public void MatchConfig_Versus_HasOneHumanInSeatZero()
        {
            MatchConfig config = MatchConfig.Versus(null, BotDifficulty.Easy, 3);
            Assert.That(config.Seats, Is.EqualTo(new[] { H, B, B }));
            Assert.That(config.WithSeed(7).Seed, Is.EqualTo(7UL));
            Assert.That(config.WithSeed(7).Seats, Is.EqualTo(config.Seats));
        }

        [Test]
        public void SeatNames_OneHumanAgainstOneBot_AreYouAndOpponent()
        {
            Assert.That(PlayerNaming.SeatNames(new[] { H, B }), Is.EqualTo(new[] { "Tú", "Rival" }));
        }

        [Test]
        public void SeatNames_OneHumanAgainstSeveralBots_NumbersBotsBySeat()
        {
            Assert.That(PlayerNaming.SeatNames(new[] { H, B, B, B }), Is.EqualTo(new[] { "Tú", "IA 2", "IA 3", "IA 4" }));
        }

        [Test]
        public void SeatNames_SeveralHumans_NumbersEveryoneBySeat()
        {
            Assert.That(PlayerNaming.SeatNames(new[] { H, B, H }), Is.EqualTo(new[] { "Jugador 1", "IA 2", "Jugador 3" }));
        }

        [Test]
        public void SideNames_WithTeams_AreTeamNamesWithMembers()
        {
            string[] seats = PlayerNaming.SeatNames(new[] { H, H, H, H });
            Assert.That(PlayerNaming.SideNames(seats, true), Is.EqualTo(new[] { "Pareja 1", "Pareja 2" }));
            Assert.That(PlayerNaming.TeamMembers(seats, 1), Is.EqualTo("Jugador 2 y Jugador 4"));
            Assert.That(PlayerNaming.SideNames(seats, false), Is.EqualTo(seats));
        }

        [Test]
        public void SideNames_InGalician_UseParella()
        {
            Localization.Service.SetLanguage("gl");
            string[] seats = PlayerNaming.SeatNames(new[] { H, H, H, H });
            Assert.That(PlayerNaming.SideNames(seats, true)[0], Is.EqualTo("Parella 1"));
            Assert.That(PlayerNaming.TeamMembers(seats, 0), Is.EqualTo("Xogador 1 e Xogador 3"));
        }
    }
}
