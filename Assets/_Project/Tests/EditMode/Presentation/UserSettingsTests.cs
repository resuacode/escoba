using System.Collections.Generic;
using Escoba.App;
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    public class UserSettingsTests
    {
        private sealed class MemoryStore : ISettingsStore
        {
            public readonly Dictionary<string, string> Strings = new Dictionary<string, string>();
            public readonly Dictionary<string, int> Ints = new Dictionary<string, int>();
            public int SaveCount;

            public string GetString(string key, string defaultValue) => Strings.TryGetValue(key, out string v) ? v : defaultValue;

            public void SetString(string key, string value) => Strings[key] = value;

            public int GetInt(string key, int defaultValue) => Ints.TryGetValue(key, out int v) ? v : defaultValue;

            public void SetInt(string key, int value) => Ints[key] = value;

            public void Save() => SaveCount++;

            public int KeyCount => Strings.Count + Ints.Count;
        }

        [Test]
        public void NewPlayer_UsesDefaults_GalicianAndNothingIsSaved()
        {
            var store = new MemoryStore();
            var settings = new UserSettings(store);

            Assert.That(settings.Language, Is.EqualTo("gl"));
            Assert.That(settings.MusicVolume, Is.EqualTo(UserSettings.DefaultMusicVolume));
            Assert.That(settings.SfxVolume, Is.EqualTo(UserSettings.DefaultSfxVolume));
            Assert.That(settings.Speed, Is.EqualTo(AnimationSpeedLevel.Normal));
            Assert.That(settings.SpeedMultiplier, Is.EqualTo(1f));
            Assert.That(store.KeyCount, Is.Zero, "sin cambios no se escribe ninguna preferencia");
            Assert.That(store.SaveCount, Is.Zero);
        }

        [Test]
        public void Changes_AreSavedAndRestoredInNextSession()
        {
            var store = new MemoryStore();
            var settings = new UserSettings(store);
            int changes = 0;
            settings.Changed += () => changes++;

            settings.SetLanguage("en");
            settings.SetMusicVolume(0.2f);
            settings.SetSfxVolume(1f);
            settings.SetSpeed(AnimationSpeedLevel.Fast);

            Assert.That(changes, Is.EqualTo(4));
            Assert.That(store.SaveCount, Is.EqualTo(4));

            var nextSession = new UserSettings(store);
            Assert.That(nextSession.Language, Is.EqualTo("en"));
            Assert.That(nextSession.MusicVolume, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(nextSession.SfxVolume, Is.EqualTo(1f));
            Assert.That(nextSession.Speed, Is.EqualTo(AnimationSpeedLevel.Fast));
            Assert.That(nextSession.SpeedMultiplier, Is.GreaterThan(1f));
        }

        [Test]
        public void SettingSameValue_DoesNotSaveOrNotify()
        {
            var store = new MemoryStore();
            var settings = new UserSettings(store);
            settings.Changed += () => Assert.Fail("no debería notificar");

            settings.SetLanguage("gl");
            settings.SetMusicVolume(UserSettings.DefaultMusicVolume);
            settings.SetSpeed(AnimationSpeedLevel.Normal);

            Assert.That(store.SaveCount, Is.Zero);
        }

        [Test]
        public void Volumes_AreClampedAndInvalidStoredValuesFallBack()
        {
            var store = new MemoryStore();
            store.Strings["settings.musicVolume"] = "no-es-un-número";
            store.Ints["settings.animationSpeed"] = 42;
            var settings = new UserSettings(store);

            Assert.That(settings.MusicVolume, Is.EqualTo(UserSettings.DefaultMusicVolume));
            Assert.That(settings.Speed, Is.EqualTo(AnimationSpeedLevel.Normal));

            settings.SetSfxVolume(3f);
            Assert.That(settings.SfxVolume, Is.EqualTo(1f));
            settings.SetMusicVolume(-1f);
            Assert.That(settings.MusicVolume, Is.EqualTo(0f));
        }

        [Test]
        public void Accessibility_DefaultsDependOnPlatformAndPersist()
        {
            var store = new MemoryStore();
            Assert.That(new UserSettings(store).LargeUi, Is.False, "escritorio: interfaz normal");
            Assert.That(new UserSettings(store, largeUiDefault: true).LargeUi, Is.True, "móvil: interfaz grande");
            Assert.That(new UserSettings(store).HighContrast, Is.False);
            Assert.That(store.KeyCount, Is.Zero);

            var settings = new UserSettings(store, largeUiDefault: true);
            settings.SetLargeUi(false);
            settings.SetHighContrast(true);

            var nextSession = new UserSettings(store, largeUiDefault: true);
            Assert.That(nextSession.LargeUi, Is.False, "la elección del jugador manda sobre el valor por defecto");
            Assert.That(nextSession.HighContrast, Is.True);
        }

        [Test]
        public void SpeedLevels_AreOrdered()
        {
            Assert.That(UserSettings.SpeedMultiplierOf(AnimationSpeedLevel.Slow), Is.LessThan(UserSettings.SpeedMultiplierOf(AnimationSpeedLevel.Normal)));
            Assert.That(UserSettings.SpeedMultiplierOf(AnimationSpeedLevel.Normal), Is.LessThan(UserSettings.SpeedMultiplierOf(AnimationSpeedLevel.Fast)));
        }

        [Test]
        public void LanguageOrder_StartsWithGalicianAndHasAllLanguages()
        {
            Assert.That(UserSettings.LanguageOrder, Is.EqualTo(new[] { "gl", "es", "en" }));
        }
    }
}
