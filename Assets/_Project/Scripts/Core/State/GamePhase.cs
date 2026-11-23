namespace Escoba.Core
{
    /// <summary>Fase de la partida.</summary>
    public enum GamePhase
    {
        /// <summary>Creada, sin repartir.</summary>
        NotStarted = 0,

        /// <summary>Ronda en juego: se esperan jugadas.</summary>
        Playing = 1,

        /// <summary>Ronda puntuada; se espera <see cref="StartNextRoundCommand"/>.</summary>
        RoundOver = 2,

        /// <summary>Partida terminada con un ganador.</summary>
        GameOver = 3,
    }
}
