using System;

namespace Escoba.Presentation
{
    /// <summary>Entrada clave/valor de un fichero de idioma (formato compatible con JsonUtility).</summary>
    [Serializable]
    internal sealed class LocalizationEntry
    {
        public string key;
        public string value;
    }
}
