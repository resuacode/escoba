using System;

namespace Escoba.AI
{
    /// <summary>
    /// Pesos de la heurística de los bots Medio y Difícil. Clase serializable para poder
    /// ajustarlos sin tocar la lógica. Cualquier cambio debe ir acompañado de una nueva medición
    /// de balance (ver skill escoba-ai y docs/GDD.md §9).
    /// </summary>
    [Serializable]
    public sealed class HeuristicWeights
    {
        /// <summary>Hacer escoba.</summary>
        public double Escoba = 100;

        /// <summary>Capturar el siete de oros.</summary>
        public double GoldSeven = 40;

        /// <summary>Cada siete capturado.</summary>
        public double Seven = 12;

        /// <summary>Cada oro capturado.</summary>
        public double Gold = 6;

        /// <summary>Cada carta capturada.</summary>
        public double Card = 2;

        /// <summary>Por cada punto de mejora de la setenta propia (fuerza 1-8 por palo).</summary>
        public double SetentaStep = 3;

        /// <summary>Completar un palo que faltaba en la baza (requisito de la setenta).</summary>
        public double SetentaNewSuit = 6;

        /// <summary>Fracción del valor de una carta que se "regala" al dejarla en la mesa.</summary>
        public double DropFactor = 0.5;

        /// <summary>Medio: probabilidad supuesta de que el rival pueda hacer escoba si la suma de la mesa lo permite.</summary>
        public double MediumEscobaRiskProbability = 0.5;

        /// <summary>Medio: probabilidad supuesta de que el rival se lleve una carta valiosa expuesta.</summary>
        public double MediumExposureProbability = 0.5;

        /// <summary>Difícil: peso de la ganancia esperada del siguiente rival tras nuestra jugada.</summary>
        public double OpponentFactor = 0.8;

        /// <summary>Difícil: número de manos rivales muestreadas por jugada candidata.</summary>
        public int HardSamples = 24;

        /// <summary>Pesos por defecto.</summary>
        public static HeuristicWeights Default => new HeuristicWeights();
    }
}
