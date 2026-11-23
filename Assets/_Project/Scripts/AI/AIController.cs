using System;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Controlador de un asiento manejado por la IA. Responde inmediatamente; el retardo de
    /// "pensar" lo añade la capa de presentación/App para no mezclar tiempo real con la lógica.
    /// </summary>
    public sealed class AIController : IPlayerController
    {
        private readonly IBotStrategy _strategy;
        private readonly IRandom _rng;

        public AIController(IBotStrategy strategy, IRandom rng)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <inheritdoc />
        public bool IsLocalHuman => false;

        /// <inheritdoc />
        public void RequestCommand(PlayerView view, Action<PlayCardCommand> submit)
        {
            if (submit == null)
            {
                throw new ArgumentNullException(nameof(submit));
            }

            submit(_strategy.ChooseCommand(view, _rng));
        }

        /// <inheritdoc />
        public void CancelRequest()
        {
        }
    }
}
