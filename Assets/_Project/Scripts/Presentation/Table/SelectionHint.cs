namespace Escoba.Presentation
{
    /// <summary>Indicación que se muestra al jugador según su selección actual.</summary>
    public enum SelectionHint
    {
        /// <summary>No hay carta de la mano seleccionada.</summary>
        SelectCard,

        /// <summary>La carta puede capturar y el jugador debe elegir qué cartas (o la selección no suma 15).</summary>
        ChooseCapture,

        /// <summary>La carta puede capturar y la captura es obligatoria, pero aún no hay combinación válida.</summary>
        MustCapture,

        /// <summary>La carta no captura nada: se dejará en la mesa.</summary>
        WillDrop,

        /// <summary>La selección es una captura válida: se puede confirmar.</summary>
        Ready,
    }
}
