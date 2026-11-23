using System;
using Escoba.Presentation;
using UnityEngine;

namespace Escoba.App
{
    /// <summary>
    /// Servicios de la aplicación compartidos entre escenas (ajustes, preferencias, localización)
    /// y la partida pendiente de jugar. No contiene estado de partida: ese vive en el motor de la
    /// escena Game. Se inicializa desde el <see cref="SceneBootstrap"/> de cualquier escena, de modo
    /// que se puede pulsar Play directamente en Game o MainMenu.
    /// </summary>
    public static class AppContext
    {
        /// <summary>Indica si ya se inicializó.</summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>Ajustes globales.</summary>
        public static GameSettings Settings { get; private set; }

        /// <summary>Preferencias del jugador.</summary>
        public static ISettingsStore Store { get; private set; }

        /// <summary>Partida elegida en el menú (null = usar valores por defecto).</summary>
        public static MatchConfig PendingMatch { get; set; }

        /// <summary>Inicializa los servicios una sola vez.</summary>
        public static void EnsureInitialized(GameSettings settings)
        {
            if (IsInitialized)
            {
                return;
            }

            Settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            Store = new PlayerPrefsSettingsStore();
            string language = Store.GetString(SettingsKeys.Language, DetectLanguage());
            Localization.Initialize(LocalizationService.FromTextAssets(settings.Languages, language));
            IsInitialized = true;
        }

        /// <summary>Cambia el idioma y lo guarda.</summary>
        public static void SetLanguage(string language)
        {
            Localization.Service?.SetLanguage(language);
            Store?.SetString(SettingsKeys.Language, language);
            Store?.Save();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Necesario si "Enter Play Mode Options" desactiva la recarga de dominio.
            IsInitialized = false;
            Settings = null;
            Store = null;
            PendingMatch = null;
        }

        private static string DetectLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.English:
                    return "en";
                default:
                    return LocalizationService.FallbackLanguage;
            }
        }
    }
}
