using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.App
{
    /// <summary>
    /// Contenido de "Cómo se juega", en orden. Los textos viven en los JSON de idioma
    /// (<c>howto.*</c>); aquí solo se define qué páginas hay y qué cartas ilustran cada una.
    /// Las reglas descritas siguen la skill <c>escoba-rules</c> con los valores por defecto.
    /// </summary>
    public static class HowToPlayGuide
    {
        /// <summary>Páginas de la guía.</summary>
        public static readonly IReadOnlyList<HowToPlayPage> Pages = new[]
        {
            new HowToPlayPage("howto.goal", table: new[] { Card.Of(Suit.Oros, 1), Card.Of(Suit.Copas, 1), Card.Of(Suit.Espadas, 1), Card.Of(Suit.Bastos, 1) }),
            new HowToPlayPage("howto.values", table: new[] { Card.Of(Suit.Copas, 10), Card.Of(Suit.Copas, 11), Card.Of(Suit.Copas, 12) }),
            new HowToPlayPage("howto.turn", new[] { Card.Of(Suit.Oros, 5) }, new[] { Card.Of(Suit.Copas, 7), Card.Of(Suit.Bastos, 3) }),
            new HowToPlayPage("howto.mandatory", new[] { Card.Of(Suit.Espadas, 12) }, new[] { Card.Of(Suit.Copas, 5), Card.Of(Suit.Oros, 2), Card.Of(Suit.Bastos, 3) }),
            new HowToPlayPage("howto.escoba", new[] { Card.Of(Suit.Oros, 6) }, new[] { Card.Of(Suit.Copas, 4), Card.Of(Suit.Bastos, 5) }),
            new HowToPlayPage("howto.round_end"),
            new HowToPlayPage("howto.points", table: new[] { Card.Of(Suit.Oros, 7) }),
            new HowToPlayPage("howto.setenta", table: new[] { Card.Of(Suit.Oros, 7), Card.Of(Suit.Copas, 7), Card.Of(Suit.Espadas, 6), Card.Of(Suit.Bastos, 1) }),
            new HowToPlayPage("howto.players"),
            new HowToPlayPage("howto.controls"),
        };
    }
}
