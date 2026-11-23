namespace Escoba.Core
{
    /// <summary>
    /// Algo que ha ocurrido en la partida como resultado de un comando. La presentación los
    /// consume en orden para animar. Algunos contienen información oculta (p. ej. la mano
    /// repartida): en online deben filtrarse por destinatario (ver <see cref="IsPrivateTo"/>).
    /// </summary>
    public abstract class GameEvent
    {
        /// <summary>
        /// Si el evento revela información que solo debe ver un asiento, devuelve ese asiento;
        /// si es público, -1.
        /// </summary>
        public virtual int IsPrivateTo => -1;
    }
}
