using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Servicio de textos localizados. Idioma de respaldo: español. Si una clave no existe en el
    /// idioma activo se usa la española; si tampoco existe, se muestra <c>[clave]</c> y se avisa
    /// en consola (solo en el editor). Plurales con sufijos <c>.one</c> / <c>.other</c>.
    /// </summary>
    public sealed class LocalizationService
    {
        /// <summary>Idioma de respaldo y por defecto.</summary>
        public const string FallbackLanguage = "es";

        private readonly Dictionary<string, LocalizationTable> _tables;
        private readonly HashSet<string> _reportedMissing = new HashSet<string>();

        public LocalizationService(IEnumerable<LocalizationTable> tables, string initialLanguage)
        {
            if (tables == null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            _tables = tables.ToDictionary(t => t.Language, StringComparer.Ordinal);
            if (_tables.Count == 0)
            {
                throw new ArgumentException("Se necesita al menos un idioma.", nameof(tables));
            }

            AvailableLanguages = _tables.Keys.OrderBy(code => code == FallbackLanguage ? 0 : 1).ThenBy(code => code).ToArray();
            CurrentLanguage = ResolveLanguage(initialLanguage);
        }

        /// <summary>Se lanza al cambiar de idioma.</summary>
        public event Action LanguageChanged;

        /// <summary>Idioma activo.</summary>
        public string CurrentLanguage { get; private set; }

        /// <summary>Idiomas disponibles (el de respaldo primero).</summary>
        public IReadOnlyList<string> AvailableLanguages { get; }

        /// <summary>Crea el servicio a partir de los JSON de idioma.</summary>
        public static LocalizationService FromTextAssets(IEnumerable<TextAsset> assets, string initialLanguage)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            return new LocalizationService(
                assets.Where(a => a != null).Select(a => LocalizationTable.FromJson(a.text)),
                initialLanguage);
        }

        /// <summary>Cambia el idioma activo (si no existe, no hace nada).</summary>
        public void SetLanguage(string language)
        {
            if (language == CurrentLanguage || language == null || !_tables.ContainsKey(language))
            {
                return;
            }

            CurrentLanguage = language;
            LanguageChanged?.Invoke();
        }

        /// <summary>Texto de una clave, con argumentos opcionales para <c>{0}</c>, <c>{1}</c>...</summary>
        public string Get(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (!TryGetRaw(key, out string value))
            {
                ReportMissing(key);
                return $"[{key}]";
            }

            if (args == null || args.Length == 0)
            {
                return value;
            }

            try
            {
                return string.Format(CultureInfo.InvariantCulture, value, args);
            }
            catch (FormatException)
            {
                ReportMissing(key + " (formato)");
                return value;
            }
        }

        /// <summary>Texto en plural según <paramref name="count"/> (<c>clave.one</c> o <c>clave.other</c>).</summary>
        public string GetPlural(string keyBase, int count, params object[] extraArgs)
        {
            string key = keyBase + (count == 1 ? ".one" : ".other");
            var args = new object[1 + (extraArgs?.Length ?? 0)];
            args[0] = count;
            extraArgs?.CopyTo(args, 1);
            return Get(key, args);
        }

        /// <summary>Nombre del idioma en su propia lengua (clave <c>language.name</c>).</summary>
        public string GetLanguageName(string language)
        {
            return _tables.TryGetValue(language, out LocalizationTable table) && table.TryGet("language.name", out string name)
                ? name
                : language;
        }

        private bool TryGetRaw(string key, out string value)
        {
            if (_tables.TryGetValue(CurrentLanguage, out LocalizationTable current) && current.TryGet(key, out value))
            {
                return true;
            }

            if (_tables.TryGetValue(FallbackLanguage, out LocalizationTable fallback) && fallback.TryGet(key, out value))
            {
                return true;
            }

            value = null;
            return false;
        }

        private string ResolveLanguage(string requested)
        {
            if (requested != null && _tables.ContainsKey(requested))
            {
                return requested;
            }

            return _tables.ContainsKey(FallbackLanguage) ? FallbackLanguage : AvailableLanguages[0];
        }

        private void ReportMissing(string key)
        {
            if (!_reportedMissing.Add(CurrentLanguage + ":" + key))
            {
                return;
            }

#if UNITY_EDITOR
            Debug.LogWarning($"[Localización] Falta la clave '{key}' (idioma {CurrentLanguage}).");
#endif
        }
    }
}
