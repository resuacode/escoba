using Escoba.Core;

namespace Escoba.Presentation
{
    /// <summary>Nombre localizado de una carta ("caballo de copas", "7 de oros").</summary>
    public static class CardNames
    {
        /// <summary>Nombre completo de la carta en el idioma activo.</summary>
        public static string Get(Card card)
        {
            string rank = card.Rank == 1 || card.IsFigure
                ? Localization.Get("card.rank." + card.Rank)
                : card.Rank.ToString();
            string suit = Localization.Get("card.suit." + card.Suit.ToString().ToLowerInvariant());
            return Localization.Get("card.name", rank, suit);
        }
    }
}
