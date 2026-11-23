namespace Escoba.AI
{
    /// <summary>Estadísticas de un torneo A contra B (1 contra 1).</summary>
    public sealed class TournamentResult
    {
        public int Games { get; internal set; }

        public int WinsA { get; internal set; }

        public int WinsB { get; internal set; }

        public int Rounds { get; internal set; }

        public double PointsPerRoundA { get; internal set; }

        public double PointsPerRoundB { get; internal set; }

        public double EscobasPerRoundA { get; internal set; }

        public double EscobasPerRoundB { get; internal set; }

        /// <summary>Tiempo medio por decisión de A, en milisegundos.</summary>
        public double AverageDecisionMsA { get; internal set; }

        /// <summary>Tiempo medio por decisión de B, en milisegundos.</summary>
        public double AverageDecisionMsB { get; internal set; }

        /// <summary>Tiempo máximo de una decisión de A o B, en milisegundos.</summary>
        public double MaxDecisionMs { get; internal set; }

        /// <summary>Proporción de victorias de A (0-1).</summary>
        public double WinRateA => Games == 0 ? 0 : (double)WinsA / Games;

        public override string ToString() =>
            $"{Games} partidas: A gana {WinRateA:P1} ({WinsA}-{WinsB}); puntos/ronda {PointsPerRoundA:F2} vs {PointsPerRoundB:F2}; " +
            $"escobas/ronda {EscobasPerRoundA:F2} vs {EscobasPerRoundB:F2}; ms/decisión {AverageDecisionMsA:F2} vs {AverageDecisionMsB:F2} (máx {MaxDecisionMs:F1})";
    }
}
