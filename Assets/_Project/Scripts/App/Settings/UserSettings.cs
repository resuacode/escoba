using System;
using System.Globalization;

namespace Escoba.App
{
    /// <summary>
    /// Ajustes del jugador (idioma, volúmenes, velocidad de animación). Los valores por defecto
    /// solo se usan mientras el jugador no cambie nada: cada cambio se guarda al momento en el
    /// <see cref="ISettingsStore"/> y se recupera en la siguiente sesión.
    /// </summary>
    public sealed class UserSettings
    {
        /// <summary>Idioma por defecto (gallego).</summary>
        public const string DefaultLanguage = "gl";

        /// <summary>Volumen de música por defecto (0-1).</summary>
        public const float DefaultMusicVolume = 0.5f;

        /// <summary>Volumen de efectos por defecto (0-1).</summary>
        public const float DefaultSfxVolume = 0.8f;

        /// <summary>Idiomas en el orden en que se ofrecen en Ajustes.</summary>
        public static readonly string[] LanguageOrder = { "gl", "es", "en" };

        private readonly ISettingsStore _store;

        public UserSettings(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            Language = _store.GetString(SettingsKeys.Language, DefaultLanguage);
            MusicVolume = ReadVolume(SettingsKeys.MusicVolume, DefaultMusicVolume);
            SfxVolume = ReadVolume(SettingsKeys.SfxVolume, DefaultSfxVolume);
            int speed = _store.GetInt(SettingsKeys.AnimationSpeed, (int)AnimationSpeedLevel.Normal);
            Speed = Enum.IsDefined(typeof(AnimationSpeedLevel), speed) ? (AnimationSpeedLevel)speed : AnimationSpeedLevel.Normal;
        }

        /// <summary>Se lanza tras cambiar cualquier ajuste.</summary>
        public event Action Changed;

        /// <summary>Código del idioma elegido.</summary>
        public string Language { get; private set; }

        /// <summary>Volumen de la música (0-1).</summary>
        public float MusicVolume { get; private set; }

        /// <summary>Volumen de los efectos (0-1).</summary>
        public float SfxVolume { get; private set; }

        /// <summary>Velocidad de las animaciones.</summary>
        public AnimationSpeedLevel Speed { get; private set; }

        /// <summary>Multiplicador de velocidad de animaciones y esperas.</summary>
        public float SpeedMultiplier => SpeedMultiplierOf(Speed);

        /// <summary>Multiplicador de una velocidad (lenta 0,7×, normal 1×, rápida 1,6×).</summary>
        public static float SpeedMultiplierOf(AnimationSpeedLevel speed)
        {
            switch (speed)
            {
                case AnimationSpeedLevel.Slow:
                    return 0.7f;
                case AnimationSpeedLevel.Fast:
                    return 1.6f;
                default:
                    return 1f;
            }
        }

        /// <summary>Cambia el idioma.</summary>
        public void SetLanguage(string language)
        {
            if (string.IsNullOrEmpty(language) || language == Language)
            {
                return;
            }

            Language = language;
            _store.SetString(SettingsKeys.Language, language);
            Commit();
        }

        /// <summary>Cambia el volumen de la música (se limita a 0-1).</summary>
        public void SetMusicVolume(float volume)
        {
            volume = Clamp01(volume);
            if (Math.Abs(volume - MusicVolume) < 0.0001f)
            {
                return;
            }

            MusicVolume = volume;
            _store.SetString(SettingsKeys.MusicVolume, Format(volume));
            Commit();
        }

        /// <summary>Cambia el volumen de los efectos (se limita a 0-1).</summary>
        public void SetSfxVolume(float volume)
        {
            volume = Clamp01(volume);
            if (Math.Abs(volume - SfxVolume) < 0.0001f)
            {
                return;
            }

            SfxVolume = volume;
            _store.SetString(SettingsKeys.SfxVolume, Format(volume));
            Commit();
        }

        /// <summary>Cambia la velocidad de las animaciones.</summary>
        public void SetSpeed(AnimationSpeedLevel speed)
        {
            if (speed == Speed || !Enum.IsDefined(typeof(AnimationSpeedLevel), speed))
            {
                return;
            }

            Speed = speed;
            _store.SetInt(SettingsKeys.AnimationSpeed, (int)speed);
            Commit();
        }

        private void Commit()
        {
            _store.Save();
            Changed?.Invoke();
        }

        // Los volúmenes se guardan como texto invariante: ISettingsStore no tiene floats.
        private float ReadVolume(string key, float defaultValue)
        {
            string raw = _store.GetString(key, null);
            return raw != null && float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                ? Clamp01(value)
                : defaultValue;
        }

        private static string Format(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static float Clamp01(float value) => float.IsNaN(value) ? 0f : Math.Max(0f, Math.Min(1f, value));
    }
}
