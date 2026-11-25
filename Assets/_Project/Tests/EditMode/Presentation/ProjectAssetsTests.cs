using System.Linq;
using Escoba.App;
using Escoba.Core;
using Escoba.Editor.Build;
using Escoba.Editor.Cards;
using Escoba.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Escoba.Tests.EditMode
{
    /// <summary>Comprueba los assets por defecto generados con Escoba/Build/Create Default Assets.</summary>
    public class ProjectAssetsTests
    {
        private static GameSettings Settings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            Assert.That(settings, Is.Not.Null, "Falta GameSettings: ejecuta Escoba/Build/Create Default Assets");
            return settings;
        }

        [Test]
        public void GameSettings_HasSkinPresetsAndThreeLanguages()
        {
            GameSettings settings = Settings();
            Assert.That(settings.CardSkin, Is.Not.Null);
            Assert.That(settings.Presets.Count, Is.EqualTo(3));
            Assert.That(settings.Presets.All(p => p != null), Is.True);
            Assert.That(settings.Languages.Count(l => l != null), Is.EqualTo(3));
        }

        [Test]
        public void AudioLibrary_HasClipsForEverySoundCue()
        {
            AudioLibrary audio = Settings().Audio;
            Assert.That(audio, Is.Not.Null, "Falta AudioLibrary en GameSettings");
            foreach (SoundCue cue in System.Enum.GetValues(typeof(SoundCue)))
            {
                AudioLibrary.CueClips entry = audio.Get(cue);
                Assert.That(entry, Is.Not.Null, $"sin entrada para {cue}");
                Assert.That(entry.Clips.Count, Is.GreaterThan(0), $"sin clips para {cue}");
                Assert.That(entry.Clips.All(c => c != null), Is.True, $"clip nulo en {cue}");
            }
        }

        [Test]
        public void AudioLibrary_HasMenuMusicAndSeveralGameTracks()
        {
            AudioLibrary audio = Settings().Audio;
            Assert.That(audio.MenuMusic, Is.Not.Null, "falta la música del menú (Audio/Music/Menu)");
            Assert.That(audio.GameMusic.Count, Is.GreaterThanOrEqualTo(2), "la partida alterna varias pistas (Audio/Music/Game)");
            Assert.That(audio.GameMusic.All(c => c != null), Is.True);
            Assert.That(audio.GameMusic.Contains(audio.MenuMusic), Is.False, "la música del menú no se repite en partida");
        }

        [Test]
        public void ActiveCardSkin_IsComplete()
        {
            string report = CardSkinGenerator.Validate(Settings().CardSkin);
            Assert.That(report, Does.StartWith("OK"), report);
        }

        [Test]
        public void CardSkinGenerator_FileNames_FollowSpanishConvention()
        {
            Assert.That(CardSkinGenerator.FileNameFor(Card.Of(Suit.Oros, 1)), Is.EqualTo("oros_01.png"));
            Assert.That(CardSkinGenerator.FileNameFor(Card.Of(Suit.Bastos, 12)), Is.EqualTo("bastos_12.png"));
            Assert.That(CardSkinGenerator.FileNameFor(Card.Of(Suit.Copas, 10)), Is.EqualTo("copas_10.png"));
        }

        [TestCase(0, 21)]
        [TestCase(1, 15)]
        [TestCase(2, 31)]
        public void Presets_ProduceValidRules(int index, int target)
        {
            RulesConfig rules = Settings().Presets[index].ToRulesConfig(2, false);
            Assert.DoesNotThrow(rules.Validate);
            Assert.That(rules.TargetScore, Is.EqualTo(target));
            Assert.That(rules.CaptureIsMandatory, Is.True);
        }

        [Test]
        public void KenneyCards_UsePixelArtImportSettings()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/_Project/Art/Cards/Kenney/oros_07.png");
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.mipmapEnabled, Is.False);
        }
    }
}
