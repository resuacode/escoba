using System;
using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Creación y barajado de la baraja de 40 cartas.</summary>
    public static class Deck
    {
        /// <summary>Devuelve las 40 cartas ordenadas por id.</summary>
        public static List<Card> CreateOrdered()
        {
            var cards = new List<Card>(Card.DeckSize);
            for (int id = 0; id < Card.DeckSize; id++)
            {
                cards.Add(Card.FromId(id));
            }

            return cards;
        }

        /// <summary>Baraja la lista en el sitio (Fisher-Yates) con el generador dado.</summary>
        public static void Shuffle<T>(IList<T> items, IRandom rng)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            for (int i = items.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = items[i];
                items[i] = items[j];
                items[j] = tmp;
            }
        }
    }
}
