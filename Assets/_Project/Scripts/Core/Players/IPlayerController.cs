using System;

namespace Escoba.Core
{
    /// <summary>
    /// Fuente de jugadas de un asiento: humano local, IA o jugador remoto. El bucle de partida no
    /// distingue tipos: pide una jugada con la vista del asiento y espera la respuesta (asíncrona).
    /// </summary>
    public interface IPlayerController
    {
        /// <summary>Indica si es un humano en este dispositivo (afecta a la pantalla de pase y a los retardos).</summary>
        bool IsLocalHuman { get; }

        /// <summary>Pide una jugada. El controlador llamará a <paramref name="submit"/> una vez.</summary>
        void RequestCommand(PlayerView view, Action<PlayCardCommand> submit);

        /// <summary>Cancela una petición en curso (p. ej. al salir de la partida).</summary>
        void CancelRequest();
    }
}
