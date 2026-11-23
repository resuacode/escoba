using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Estado de un bando (jugador o pareja): baza y escobas de la ronda y puntuación acumulada.</summary>
    public sealed class SideState
    {
        internal readonly List<Card> CapturedList = new List<Card>();

        /// <summary>Cartas capturadas en la ronda actual.</summary>
        public IReadOnlyList<Card> Captured => CapturedList;

        /// <summary>Escobas de la ronda actual.</summary>
        public int Escobas { get; internal set; }

        /// <summary>Puntuación acumulada de la partida.</summary>
        public int Score { get; internal set; }
    }
}
