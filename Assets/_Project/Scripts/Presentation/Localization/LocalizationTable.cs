using System;
using System.Collections.Generic;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Textos de un idioma, cargados de un JSON:
    /// <c>{ "language": "es", "entries": [ { "key": "menu.play_ai", "value": "Jugar contra la IA" } ] }</c>.
    /// </summary>
    public sealed class LocalizationTable
    {
        private readonly Dictionary<string, string> _values;

        private LocalizationTable(string language, Dictionary<string, string> values)
        {
            Language = language;
            _values = values;
        }

        /// <summary>Código de idioma (es, gl, en).</summary>
        public string Language { get; }

        /// <summary>Claves definidas.</summary>
        public IEnumerable<string> Keys => _values.Keys;

        /// <summary>Número de textos.</summary>
        public int Count => _values.Count;

        /// <summary>
        /// Lee un fichero de idioma. Lanza <see cref="FormatException"/> si falta el código de idioma,
        /// hay claves vacías o repetidas, o algún valor es nulo.
        /// </summary>
        public static LocalizationTable FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("El fichero de idioma está vacío.");
            }

            LocalizationFile file = JsonUtility.FromJson<LocalizationFile>(json);
            if (file == null || string.IsNullOrWhiteSpace(file.language))
            {
                throw new FormatException("El fichero de idioma no tiene 'language'.");
            }

            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            if (file.entries != null)
            {
                foreach (LocalizationEntry entry in file.entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    {
                        throw new FormatException($"[{file.language}] Hay una entrada sin clave.");
                    }

                    if (entry.value == null)
                    {
                        throw new FormatException($"[{file.language}] La clave '{entry.key}' no tiene valor.");
                    }

                    if (values.ContainsKey(entry.key))
                    {
                        throw new FormatException($"[{file.language}] Clave repetida: '{entry.key}'.");
                    }

                    values.Add(entry.key, entry.value);
                }
            }

            return new LocalizationTable(file.language, values);
        }

        /// <summary>Busca un texto.</summary>
        public bool TryGet(string key, out string value) => _values.TryGetValue(key, out value);

        /// <summary>Indica si la clave existe.</summary>
        public bool Contains(string key) => _values.ContainsKey(key);
    }
}
