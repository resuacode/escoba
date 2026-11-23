using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Estrategia de un bot. Solo recibe la <see cref="PlayerView"/> de su asiento (no puede
    /// hacer trampas) y un generador con semilla (decisiones reproducibles).
    /// Debe devolver siempre una jugada legal.
    /// </summary>
    public interface IBotStrategy
    {
        /// <summary>Elige la jugada para el turno actual del dueño de la vista.</summary>
        PlayCardCommand ChooseCommand(PlayerView view, IRandom rng);
    }
}
