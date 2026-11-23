using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>
    /// Información pública de un bando. Las bazas se consideran públicas porque cada carta
    /// capturada estuvo boca arriba en la mesa (cualquiera puede recordarla).
    /// </summary>
    public sealed class SideView
    {
        internal SideView(IReadOnlyList<Card> captured, int escobas, int score)
        {
            Captured = captured;
            Escobas = escobas;
            Score = score;
        }

        /// <summary>Cartas capturadas en la ronda.</summary>
        public IReadOnlyList<Card> Captured { get; }

        /// <summary>Escobas de la ronda.</summary>
        public int Escobas { get; }

        /// <summary>Puntuación acumulada.</summary>
        public int Score { get; }
    }
}
