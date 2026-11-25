namespace Escoba.Presentation
{
    /// <summary>Efectos de sonido del juego. Cada uno se asocia a uno o varios clips en <see cref="AudioLibrary"/>.</summary>
    public enum SoundCue
    {
        /// <summary>Pulsar un botón de la interfaz.</summary>
        Click = 0,

        /// <summary>Barajar al empezar una ronda.</summary>
        Shuffle = 1,

        /// <summary>Repartir una carta.</summary>
        Deal = 2,

        /// <summary>Dejar una carta en la mesa.</summary>
        PlayCard = 3,

        /// <summary>Llevarse cartas a la baza.</summary>
        Capture = 4,

        /// <summary>Hacer escoba.</summary>
        Escoba = 5,

        /// <summary>Mostrar la mano (abrir las cartas en abanico).</summary>
        RevealHand = 6,

        /// <summary>Fin de ronda (recuento de puntos).</summary>
        RoundEnd = 7,

        /// <summary>Fin de partida.</summary>
        GameOver = 8,

        /// <summary>Llevarse el siete de oros.</summary>
        GoldSeven = 9,

        /// <summary>Aparece una fila con puntos en el resumen de ronda.</summary>
        ScoreTick = 10,
    }
}
