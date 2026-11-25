using System;
using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.App
{
    /// <summary>
    /// Una página de "Cómo se juega": título, texto y, opcionalmente, un ejemplo con cartas.
    /// Si el ejemplo tiene carta jugada, se dibuja como "Tu carta" + "En la mesa"; si no, las
    /// cartas se muestran en una sola fila sin etiquetas.
    /// </summary>
    public sealed class HowToPlayPage
    {
        public HowToPlayPage(string key, IReadOnlyList<Card> played = null, IReadOnlyList<Card> table = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Played = played ?? Array.Empty<Card>();
            Table = table ?? Array.Empty<Card>();
        }

        /// <summary>Prefijo de las claves de texto (<c>{Key}.title</c>, <c>{Key}.body</c>, <c>{Key}.caption</c>).</summary>
        public string Key { get; }

        /// <summary>Carta(s) jugada(s) del ejemplo (vacío si no hay jugada).</summary>
        public IReadOnlyList<Card> Played { get; }

        /// <summary>Cartas de la mesa del ejemplo, o cartas mostradas si no hay jugada.</summary>
        public IReadOnlyList<Card> Table { get; }

        /// <summary>Indica si la página tiene ejemplo con cartas (y por tanto pie de ejemplo).</summary>
        public bool HasExample => Played.Count + Table.Count > 0;

        /// <summary>Indica si el ejemplo es una jugada (carta propia + mesa).</summary>
        public bool IsPlay => Played.Count > 0;

        public string TitleKey => Key + ".title";

        public string BodyKey => Key + ".body";

        public string CaptionKey => Key + ".caption";
    }
}
