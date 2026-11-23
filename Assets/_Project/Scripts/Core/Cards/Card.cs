using System;

namespace Escoba.Core
{
    /// <summary>
    /// Carta de la baraja española de 40 cartas. Se identifica por un id estable (0-39):
    /// <c>palo * 10 + ordinal</c>, con ordinal 0-9 para 1, 2, 3, 4, 5, 6, 7, sota, caballo y rey.
    /// El id es lo que se serializa y se envía por red.
    /// </summary>
    public readonly struct Card : IEquatable<Card>
    {
        /// <summary>Número de cartas de la baraja.</summary>
        public const int DeckSize = 40;

        /// <summary>Cartas por palo.</summary>
        public const int RanksPerSuit = 10;

        /// <summary>Número de palos.</summary>
        public const int SuitCount = 4;

        private static readonly int[] RankByOrdinal = { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };

        private Card(int id)
        {
            Id = id;
        }

        /// <summary>Id estable de la carta (0-39).</summary>
        public int Id { get; }

        /// <summary>Palo de la carta.</summary>
        public Suit Suit => (Suit)(Id / RanksPerSuit);

        /// <summary>Posición dentro del palo (0-9).</summary>
        public int Ordinal => Id % RanksPerSuit;

        /// <summary>Número impreso: 1-7, 10 (sota), 11 (caballo) o 12 (rey).</summary>
        public int Rank => RankByOrdinal[Ordinal];

        /// <summary>Valor para sumar 15: 1-7 su número, sota 8, caballo 9, rey 10.</summary>
        public int Value => Ordinal + 1;

        /// <summary>Indica si es sota, caballo o rey.</summary>
        public bool IsFigure => Ordinal >= 7;

        /// <summary>Indica si es un siete.</summary>
        public bool IsSeven => Rank == 7;

        /// <summary>Indica si es el siete de oros (el "velo").</summary>
        public bool IsGoldSeven => Suit == Suit.Oros && Rank == 7;

        /// <summary>
        /// Fuerza de la carta para la setenta: 7 &gt; 6 &gt; 1 &gt; 5 &gt; 4 &gt; 3 &gt; 2 &gt; figuras.
        /// Valores de 8 (siete) a 1 (figuras); las figuras empatan entre sí.
        /// </summary>
        public int SetentaStrength
        {
            get
            {
                switch (Rank)
                {
                    case 7: return 8;
                    case 6: return 7;
                    case 1: return 6;
                    case 5: return 5;
                    case 4: return 4;
                    case 3: return 3;
                    case 2: return 2;
                    default: return 1;
                }
            }
        }

        /// <summary>Crea una carta a partir de su id (0-39).</summary>
        public static Card FromId(int id)
        {
            if (!IsValidId(id))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "El id de carta debe estar entre 0 y 39.");
            }

            return new Card(id);
        }

        /// <summary>Crea una carta a partir de su palo y su número impreso (1-7, 10, 11, 12).</summary>
        public static Card Of(Suit suit, int rank)
        {
            if ((int)suit < 0 || (int)suit >= SuitCount)
            {
                throw new ArgumentOutOfRangeException(nameof(suit), suit, "Palo no válido.");
            }

            int ordinal = Array.IndexOf(RankByOrdinal, rank);
            if (ordinal < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, "Número no válido en la baraja española (1-7, 10, 11, 12).");
            }

            return new Card(((int)suit * RanksPerSuit) + ordinal);
        }

        /// <summary>Indica si un id corresponde a una carta válida.</summary>
        public static bool IsValidId(int id) => id >= 0 && id < DeckSize;

        public bool Equals(Card other) => Id == other.Id;

        public override bool Equals(object obj) => obj is Card other && Equals(other);

        public override int GetHashCode() => Id;

        public override string ToString() => $"{Rank}-{Suit}";

        public static bool operator ==(Card left, Card right) => left.Id == right.Id;

        public static bool operator !=(Card left, Card right) => left.Id != right.Id;
    }
}
