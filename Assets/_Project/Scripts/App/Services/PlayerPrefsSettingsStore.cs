using UnityEngine;

namespace Escoba.App
{
    /// <summary>Preferencias en <see cref="PlayerPrefs"/> (funciona en escritorio, móvil y WebGL).</summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        private const string Prefix = "escoba.";

        public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(Prefix + key, defaultValue);

        public void SetString(string key, string value) => PlayerPrefs.SetString(Prefix + key, value);

        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(Prefix + key, defaultValue);

        public void SetInt(string key, int value) => PlayerPrefs.SetInt(Prefix + key, value);

        public void Save() => PlayerPrefs.Save();
    }
}
