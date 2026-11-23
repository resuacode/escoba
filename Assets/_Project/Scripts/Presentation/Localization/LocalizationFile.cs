using System;
using System.Collections.Generic;

namespace Escoba.Presentation
{
    /// <summary>Contenido de <c>Assets/_Project/Localization/&lt;idioma&gt;.json</c>.</summary>
    [Serializable]
    internal sealed class LocalizationFile
    {
        public string language;
        public List<LocalizationEntry> entries;
    }
}
