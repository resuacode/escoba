namespace Escoba.Presentation
{
    /// <summary>Resaltado visual de una carta.</summary>
    public enum CardHighlight
    {
        None,

        /// <summary>Carta de la mesa que puede formar parte de una captura.</summary>
        Candidate,

        /// <summary>Carta seleccionada por el jugador.</summary>
        Selected,
    }
}
