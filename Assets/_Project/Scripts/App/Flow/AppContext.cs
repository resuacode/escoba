using System;
using Escoba.Presentation;
using UnityEngine;

namespace Escoba.App
{
    /// <summary>
    /// Servicios de la aplicación compartidos entre escenas (ajustes, preferencias, localización,
    /// audio) y la partida pendiente de jugar. No contiene estado de partida: ese vive en el motor
    /// de la escena Game. Se inicializa desde el <see cref="SceneBootstrap"/> de cualquier escena,
    /// de modo que se puede pulsar Play directamente en Game o MainMenu.
    /// </summary>
    public static class AppContext
    {
        /// <summary>Indica si ya se inicializó.</summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>Ajustes globales.</summary>
        public static GameSettings Settings { get; private set; }

        /// <summary>Preferencias del jugador.</summary>
        public static ISettingsStore Store { get; private set; }

        /// <summary>Ajustes elegidos por el jugador (idioma, volúmenes, velocidad).</summary>
        public static UserSettings User { get; private set; }

        /// <summary>Audio (efectos y música).</summary>
        public static AudioService Audio { get; private set; }

        /// <summary>Partida elegida en el menú (null = usar valores por defecto).</summary>
        public static MatchConfig PendingMatch { get; set; }

        /// <summary>Multiplicador de velocidad de animaciones: el del asset por el elegido por el jugador.</summary>
        public static float AnimationSpeed => (Settings != null ? Settings.AnimationSpeed : 1f) * (User?.SpeedMultiplier ?? 1f);

        /// <summary>Inicializa los servicios una sola vez.</summary>
        public static void EnsureInitialized(GameSettings settings)
        {
            if (IsInitialized)
            {
                return;
            }

            Settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            Store = new PlayerPrefsSettingsStore();
            User = new UserSettings(Store, largeUiDefault: Application.isMobilePlatform);
            Localization.Initialize(LocalizationService.FromTextAssets(settings.Languages, User.Language));

            Audio = AudioService.Create(settings.Audio);
            Sounds.Initialize(Audio);
            User.Changed += ApplyUserSettings;
            ApplyUserSettings();
            IsInitialized = true;
        }

        /// <summary>Cambia el idioma y lo guarda.</summary>
        public static void SetLanguage(string language)
        {
            User?.SetLanguage(language);
        }

        private static void ApplyUserSettings()
        {
            Localization.Service?.SetLanguage(User.Language);
            AccessibilityOptions.Set(User.HighContrast, User.LargeUi);
            if (Audio != null)
            {
                Audio.SetVolumes(User.MusicVolume, User.SfxVolume);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Necesario si "Enter Play Mode Options" desactiva la recarga de dominio.
            IsInitialized = false;
            Settings = null;
            Store = null;
            User = null;
            Audio = null;
            PendingMatch = null;
        }
    }
}
