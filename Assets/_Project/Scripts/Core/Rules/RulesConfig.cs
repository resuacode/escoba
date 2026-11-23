using System;

namespace Escoba.Core
{
    /// <summary>
    /// Opciones de reglas de una partida. Ver la skill <c>escoba-rules</c> §8.
    /// El motor trabaja siempre con una copia validada (<see cref="Clone"/> + <see cref="Validate"/>).
    /// </summary>
    public sealed class RulesConfig
    {
        /// <summary>Número de jugadores (2, 3 o 4).</summary>
        public int PlayerCount { get; set; } = 2;

        /// <summary>Juego por parejas (solo con 4 jugadores; compañeros en asientos 0-2 y 1-3).</summary>
        public bool Teams { get; set; }

        /// <summary>Puntos para ganar la partida.</summary>
        public int TargetScore { get; set; } = 21;

        /// <summary>Si la mesa inicial suma 15 (o 30), el repartidor se la lleva con escoba(s).</summary>
        public bool DealerTableEscoba { get; set; } = true;

        /// <summary>Si la carta jugada puede capturar, la captura es obligatoria.</summary>
        public bool CaptureIsMandatory { get; set; } = true;

        /// <summary>Si la última jugada de la ronda deja la mesa vacía por captura, cuenta como escoba.</summary>
        public bool LastPlayEscobaCounts { get; set; } = true;

        /// <summary>Resolución de empates en la setenta.</summary>
        public SetentaTieRule SetentaTieRule { get; set; } = SetentaTieRule.NoPoint;

        /// <summary>Puntos extra por tener los cuatro sietes (0 = desactivado).</summary>
        public int AllSevensBonus { get; set; }

        /// <summary>Puntos por tener los diez oros, en lugar del punto de "más oros" (0 = desactivado).</summary>
        public int AllGoldsBonus { get; set; }

        /// <summary>Cartas que recibe cada jugador en cada reparto.</summary>
        public int HandSize { get; set; } = 3;

        /// <summary>Cartas boca arriba en la mesa al inicio de cada ronda.</summary>
        public int InitialTableCards { get; set; } = 4;

        /// <summary>Suma que hay que alcanzar para capturar.</summary>
        public int CaptureSum { get; set; } = 15;

        /// <summary>Número de bandos que puntúan (jugadores o parejas).</summary>
        public int SideCount => Teams ? 2 : PlayerCount;

        /// <summary>Bando (jugador o pareja) al que pertenece un asiento.</summary>
        public int SideOfSeat(int seat) => Teams ? seat % 2 : seat;

        /// <summary>Copia independiente de la configuración.</summary>
        public RulesConfig Clone() => (RulesConfig)MemberwiseClone();

        /// <summary>Lanza <see cref="ArgumentException"/> si la configuración no es jugable.</summary>
        public void Validate()
        {
            if (PlayerCount < 2 || PlayerCount > 4)
            {
                throw new ArgumentException("PlayerCount debe ser 2, 3 o 4.");
            }

            if (Teams && PlayerCount != 4)
            {
                throw new ArgumentException("Las parejas solo son posibles con 4 jugadores.");
            }

            if (TargetScore < 1)
            {
                throw new ArgumentException("TargetScore debe ser al menos 1.");
            }

            if (HandSize < 1 || InitialTableCards < 0 || CaptureSum < 1)
            {
                throw new ArgumentException("HandSize y CaptureSum deben ser positivos e InitialTableCards no negativo.");
            }

            if (AllSevensBonus < 0 || AllGoldsBonus < 0)
            {
                throw new ArgumentException("Las bonificaciones no pueden ser negativas.");
            }

            int cardsPerDeal = HandSize * PlayerCount;
            int remaining = Card.DeckSize - InitialTableCards;
            if (remaining < cardsPerDeal || remaining % cardsPerDeal != 0)
            {
                throw new ArgumentException(
                    $"El mazo no se reparte exacto: {remaining} cartas para repartos de {cardsPerDeal}.");
            }
        }
    }
}
