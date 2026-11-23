using System.Collections.Generic;
using System.Linq;
using Escoba.App;
using Escoba.Core;
using Escoba.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Escoba.Tests.EditMode
{
    /// <summary>Nombres de jugador: saneado, uso en partida y preferencias guardadas.</summary>
    public class PlayerNamesTests
    {
        private static readonly SeatType H = SeatType.Human;
        private static readonly SeatType B = SeatType.Bot;

        private sealed class MemoryStore : ISettingsStore
        {
            public readonly Dictionary<string, string> Strings = new Dictionary<string, string>();
            public readonly Dictionary<string, int> Ints = new Dictionary<string, int>();

            public string GetString(string key, string defaultValue) => Strings.TryGetValue(key, out string v) ? v : defaultValue;

            public void SetString(string key, string value) => Strings[key] = value;

            public int GetInt(string key, int defaultValue) => Ints.TryGetValue(key, out int v) ? v : defaultValue;

            public void SetInt(string key, int value) => Ints[key] = value;

            public void Save()
            {
            }
        }

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

        [TestCase("Laura", "Laura")]
        [TestCase("  Ana   María  ", "Ana María")]
        [TestCase("Maximiliano Pérez", "Maximiliano")]
        [TestCase("<b>Pablo</b>", "bPablo/b")]
        [TestCase("{0} | x", "0 x")]
        [TestCase("   ", null)]
        [TestCase("", null)]
        [TestCase(null, null)]
        [TestCase("Uxío", "Uxío")]
        [TestCase("Iñaki", "Iñaki")]
        public void Sanitize_CleansNames(string raw, string expected)
        {
            Assert.That(PlayerNameRules.Sanitize(raw), Is.EqualTo(expected));
        }

        [Test]
        public void Sanitize_NeverExceedsMaxLength()
        {
            string name = PlayerNameRules.Sanitize(new string('a', 40));
            Assert.That(name.Length, Is.EqualTo(PlayerNameRules.MaxLength));
        }

        [Test]
        public void SeatNames_UseCustomNamesForHumansOnly()
        {
            string[] names = PlayerNaming.SeatNames(new[] { H, B, H }, new[] { "Laura", "Robot", null });
            Assert.That(names, Is.EqualTo(new[] { "Laura", "IA 2", "Jugador 3" }));
        }

        [Test]
        public void SeatNames_SingleHumanWithName_UsesNameInsteadOfYou()
        {
            Assert.That(PlayerNaming.SeatNames(new[] { H, B }, new[] { "Laura" }), Is.EqualTo(new[] { "Laura", "Rival" }));
            Assert.That(PlayerNaming.SeatNames(new[] { H, B }, new[] { "  " }), Is.EqualTo(new[] { "Tú", "Rival" }));
        }

        [Test]
        public void SeatNames_DuplicateNames_AreDisambiguatedBySeat()
        {
            string[] names = PlayerNaming.SeatNames(new[] { H, H, B }, new[] { "Ana", "ana", null });
            Assert.That(names, Is.EqualTo(new[] { "Ana", "ana (2)", "IA 3" }));

            string[] clash = PlayerNaming.SeatNames(new[] { H, B, B }, new[] { "IA 2", null, null });
            Assert.That(clash[0], Is.Not.EqualTo(clash[1]));
        }

        [Test]
        public void MatchConfig_SanitizesNamesAndKeepsThemOnRematch()
        {
            var config = new MatchConfig(null, Escoba.AI.BotDifficulty.Easy, new[] { H, H }, false, null, new[] { " Laura ", "<Pablo>" });
            Assert.That(config.PlayerNames, Is.EqualTo(new[] { "Laura", "Pablo" }));
            Assert.That(config.WithSeed(null).PlayerNames, Is.EqualTo(config.PlayerNames));
        }

        [Test]
        public void SetupPreferences_SaveAndLoad_RoundTripsPerMode()
        {
            var store = new MemoryStore();
            SetupSelection local = SetupSelection.Default(true);
            local.PlayerCount = 4;
            local.Teams = true;
            local.Seats[3] = B;
            local.Names[0] = "Laura";
            local.Names[1] = "Pablo";
            SetupPreferences.Save(store, true, local);

            SetupSelection loaded = SetupPreferences.Load(store, true);
            Assert.That(loaded.PlayerCount, Is.EqualTo(4));
            Assert.That(loaded.Teams, Is.True);
            Assert.That(loaded.Seats, Is.EqualTo(new[] { H, H, H, B }));
            Assert.That(loaded.Names.Take(2), Is.EqualTo(new[] { "Laura", "Pablo" }));

            SetupSelection versus = SetupPreferences.Load(store, false);
            Assert.That(versus.PlayerCount, Is.EqualTo(2), "cada modo recuerda su propia configuración");
            Assert.That(versus.Seats, Is.EqualTo(new[] { H, B, B, B }));
            Assert.That(versus.Names[0], Is.EqualTo("Laura"), "los nombres se comparten entre modos");
        }

        [Test]
        public void SetupPreferences_CorruptData_FallsBackToDefaults()
        {
            var store = new MemoryStore();
            store.Ints["setup.local.players"] = 9;
            store.Ints["setup.local.teams"] = 1;
            store.Strings["setup.local.seats"] = "XYZW";
            store.Strings["setup.name.0"] = "<<<";

            SetupSelection loaded = SetupPreferences.Load(store, true);
            Assert.That(loaded.PlayerCount, Is.EqualTo(2));
            Assert.That(loaded.Teams, Is.False, "parejas solo con 4 jugadores");
            Assert.That(loaded.Seats, Is.EqualTo(new[] { H, H, H, H }));
            Assert.That(loaded.Names[0], Is.Null);
        }

        [Test]
        public void SetupPreferences_WithoutHumans_ForcesSeatZeroHuman()
        {
            var store = new MemoryStore();
            store.Strings["setup.ai.seats"] = "BBBB";
            Assert.That(SetupPreferences.Load(store, false).Seats[0], Is.EqualTo(H));
        }
    }
}
