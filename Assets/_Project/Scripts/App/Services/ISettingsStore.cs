namespace Escoba.App
{
    /// <summary>Persistencia de preferencias del jugador (idioma, último preset...).</summary>
    public interface ISettingsStore
    {
        string GetString(string key, string defaultValue);

        void SetString(string key, string value);

        int GetInt(string key, int defaultValue);

        void SetInt(string key, int value);

        /// <summary>Escribe los cambios pendientes.</summary>
        void Save();
    }
}
