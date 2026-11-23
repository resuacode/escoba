using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Desglose de la puntuación de un bando (jugador o pareja) en una ronda.</summary>
    public sealed class SideRoundScore
    {
        internal SideRoundScore(int side)
        {
            Side = side;
        }

        /// <summary>Índice del bando.</summary>
        public int Side { get; }

        /// <summary>Cartas capturadas en la ronda.</summary>
        public int CardCount { get; internal set; }

        /// <summary>Oros capturados.</summary>
        public int GoldCount { get; internal set; }

        /// <summary>Sietes capturados.</summary>
        public int SevenCount { get; internal set; }

        /// <summary>Tiene el siete de oros.</summary>
        public bool HasGoldSeven { get; internal set; }

        /// <summary>Su setenta (4 cartas de mayor a menor) o <c>null</c> si le falta algún palo.</summary>
        public IReadOnlyList<Card> SetentaCards { get; internal set; }

        /// <summary>Puntos por escobas.</summary>
        public int EscobaPoints { get; internal set; }

        /// <summary>Punto por más cartas.</summary>
        public int MostCardsPoints { get; internal set; }

        /// <summary>Punto por más oros.</summary>
        public int MostGoldsPoints { get; internal set; }

        /// <summary>Punto por el siete de oros.</summary>
        public int GoldSevenPoints { get; internal set; }

        /// <summary>Punto por la setenta.</summary>
        public int SetentaPoints { get; internal set; }

        /// <summary>Bonificación por los cuatro sietes.</summary>
        public int AllSevensBonusPoints { get; internal set; }

        /// <summary>Bonificación por los diez oros.</summary>
        public int AllGoldsBonusPoints { get; internal set; }

        /// <summary>Total de la ronda.</summary>
        public int Total => EscobaPoints + MostCardsPoints + MostGoldsPoints + GoldSevenPoints
                            + SetentaPoints + AllSevensBonusPoints + AllGoldsBonusPoints;
    }
}
