using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Escoba.Core;
using Escoba.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Escoba.Tests.EditMode
{
    public class LocalizationTests
    {
        private const string Folder = "Assets/_Project/Localization";
        private static readonly string[] Languages = { "es", "gl", "en" };
        private static readonly Regex Placeholder = new Regex(@"\{\d+\}");

        private static LocalizationTable Load(string language)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>($"{Folder}/{language}.json");
            Assert.That(asset, Is.Not.Null, $"Falta {language}.json");
            return LocalizationTable.FromJson(asset.text);
        }

        private static string Value(LocalizationTable table, string key)
        {
            table.TryGet(key, out string value);
            return value;
        }

        [TearDown]
        public void ResetLocalization()
        {
            Localization.Initialize(null);
        }

        [TestCaseSource(nameof(Languages))]
        public void LanguageFile_HasSameKeysAsSpanish(string language)
        {
            LocalizationTable spanish = Load("es");
            LocalizationTable table = Load(language);

            Assert.That(table.Language, Is.EqualTo(language));
            Assert.That(table.Keys.Except(spanish.Keys), Is.Empty, "claves que no existen en español");
            Assert.That(spanish.Keys.Except(table.Keys), Is.Empty, "claves sin traducir");
        }

        [TestCaseSource(nameof(Languages))]
        public void LanguageFile_ValuesAreNotEmptyAndKeepPlaceholders(string language)
        {
            LocalizationTable spanish = Load("es");
            LocalizationTable table = Load(language);
            foreach (string key in table.Keys)
            {
                string value = Value(table, key);
                Assert.That(value, Is.Not.Empty, key);
                var expected = new HashSet<string>(Placeholder.Matches(Value(spanish, key)).Cast<Match>().Select(m => m.Value));
                var actual = new HashSet<string>(Placeholder.Matches(value).Cast<Match>().Select(m => m.Value));
                Assert.That(actual.SetEquals(expected), Is.True, $"[{language}] {key}: marcadores distintos del español");
            }
        }

        [Test]
        public void Galician_FollowsRagNormDecisions()
        {
            LocalizationTable gl = Load("gl");
            string all = string.Join("\n", gl.Keys.Select(k => Value(gl, k)));

            // Sin signos de apertura (decisión de estilo; opcionales según las Normas RAG 2012, §7).
            Assert.That(all, Does.Not.Contain("¡").And.Not.Contain("¿"));

            // Grafías propias del portugués que no existen en gallego normativo.
            foreach (string lusism in new[] { "ç", "ã", "õ", "â", "ê", "ô", "lh", "jog", "vez" })
            {
                Assert.That(all.ToLowerInvariant(), Does.Not.Contain(lusism), $"posible lusismo: '{lusism}'");
            }

            // Decisiones contrastadas con el DRAG (ver docs/LOCALIZATION.md).
            Assert.That(Value(gl, "card.rank.1"), Is.EqualTo("ás"));
            Assert.That(Value(gl, "gameover.rematch"), Is.EqualTo("Desquite"));
            Assert.That(Value(gl, "game.round"), Does.StartWith("Rolda"));
            Assert.That(Value(gl, "game.your_turn"), Does.Contain("quenda"));
            Assert.That(Value(gl, "summary.cards"), Does.StartWith("Máis"));
        }

        [Test]
        public void FromJson_DuplicateKey_Throws()
        {
            const string json = "{\"language\":\"xx\",\"entries\":[{\"key\":\"a\",\"value\":\"1\"},{\"key\":\"a\",\"value\":\"2\"}]}";
            Assert.Throws<FormatException>(() => LocalizationTable.FromJson(json));
        }

        [Test]
        public void Service_MissingKeyFallsBackToSpanishThenToKey()
        {
            LocalizationTable es = LocalizationTable.FromJson("{\"language\":\"es\",\"entries\":[{\"key\":\"only.es\",\"value\":\"hola\"}]}");
            LocalizationTable gl = LocalizationTable.FromJson("{\"language\":\"gl\",\"entries\":[]}");
            var service = new LocalizationService(new[] { es, gl }, "gl");

            Assert.That(service.Get("only.es"), Is.EqualTo("hola"));
            Assert.That(service.Get("nowhere"), Is.EqualTo("[nowhere]"));
        }

        [Test]
        public void Service_UnknownInitialLanguage_UsesSpanish()
        {
            var service = new LocalizationService(Languages.Select(Load), "fr");
            Assert.That(service.CurrentLanguage, Is.EqualTo("es"));
            Assert.That(service.AvailableLanguages[0], Is.EqualTo("es"));
        }

        [Test]
        public void Service_PluralAndArguments_AreFormatted()
        {
            var service = new LocalizationService(Languages.Select(Load), "es");
            Assert.That(service.GetPlural("game.cards", 1), Is.EqualTo("1 carta"));
            Assert.That(service.GetPlural("game.cards", 12), Is.EqualTo("12 cartas"));
            Assert.That(service.Get("game.target", 21), Is.EqualTo("Partida a 21 puntos"));

            service.SetLanguage("gl");
            Assert.That(service.Get("game.dealer_escoba", "Rival"), Is.EqualTo("Rival leva a mesa ao repartir!"));
        }

        [Test]
        public void Service_SetLanguage_RaisesChangedOnlyWhenItChanges()
        {
            var service = new LocalizationService(Languages.Select(Load), "es");
            int changes = 0;
            service.LanguageChanged += () => changes++;

            service.SetLanguage("es");
            service.SetLanguage("gl");
            service.SetLanguage("xx");

            Assert.That(changes, Is.EqualTo(1));
            Assert.That(service.GetLanguageName("gl"), Is.EqualTo("Galego"));
        }

        [TestCase("es", "caballo de copas", "as de oros", "7 de espadas")]
        [TestCase("gl", "cabalo de copas", "ás de ouros", "7 de espadas")]
        [TestCase("en", "knight of cups", "ace of coins", "7 of swords")]
        public void CardNames_AreLocalized(string language, string knightOfCups, string aceOfGolds, string sevenOfSwords)
        {
            Localization.Initialize(new LocalizationService(Languages.Select(Load), language));

            Assert.That(CardNames.Get(Card.Of(Suit.Copas, 11)), Is.EqualTo(knightOfCups));
            Assert.That(CardNames.Get(Card.Of(Suit.Oros, 1)), Is.EqualTo(aceOfGolds));
            Assert.That(CardNames.Get(Card.Of(Suit.Espadas, 7)), Is.EqualTo(sevenOfSwords));
        }
    }
}
