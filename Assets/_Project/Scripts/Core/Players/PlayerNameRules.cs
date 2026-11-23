using System.Text;

namespace Escoba.Core
{
    /// <summary>
    /// Normas de los nombres de jugador (compartidas por el menú local y, en M6, por la red):
    /// máximo 12 caracteres, sin espacios sobrantes y sin caracteres que alteren el texto
    /// enriquecido o los formatos (<c>&lt; &gt; { } |</c>) ni caracteres de control.
    /// </summary>
    public static class PlayerNameRules
    {
        /// <summary>Longitud máxima de un nombre.</summary>
        public const int MaxLength = 12;

        /// <summary>Indica si un carácter puede formar parte de un nombre.</summary>
        public static bool IsAllowedChar(char c)
        {
            return !char.IsControl(c) && c != '<' && c != '>' && c != '{' && c != '}' && c != '|';
        }

        /// <summary>
        /// Limpia un nombre escrito por el jugador. Devuelve null si queda vacío
        /// (se usará el nombre por defecto del asiento).
        /// </summary>
        public static string Sanitize(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }

            var builder = new StringBuilder(raw.Length);
            bool lastWasSpace = true;
            foreach (char c in raw)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasSpace)
                    {
                        builder.Append(' ');
                        lastWasSpace = true;
                    }

                    continue;
                }

                if (IsAllowedChar(c))
                {
                    builder.Append(c);
                    lastWasSpace = false;
                }
            }

            string clean = builder.ToString().Trim();
            if (clean.Length > MaxLength)
            {
                clean = clean.Substring(0, MaxLength).TrimEnd();
            }

            return clean.Length == 0 ? null : clean;
        }
    }
}
