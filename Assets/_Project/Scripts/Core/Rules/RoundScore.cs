using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>Resultado de puntuar una ronda: un desglose por bando.</summary>
    public sealed class RoundScore
    {
        internal RoundScore(IReadOnlyList<SideRoundScore> sides)
        {
            Sides = sides;
        }

        /// <summary>Desglose por bando, indexado por bando.</summary>
        public IReadOnlyList<SideRoundScore> Sides { get; }
    }
}
