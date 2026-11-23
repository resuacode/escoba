using System;
using Escoba.Core;

namespace Escoba.Presentation
{
    /// <summary>Jugador humano en este dispositivo: su jugada llega desde la interacción con la mesa.</summary>
    public sealed class HumanLocalController : IPlayerController
    {
        private readonly TablePresenter _table;

        public HumanLocalController(TablePresenter table)
        {
            _table = table != null ? table : throw new ArgumentNullException(nameof(table));
        }

        /// <inheritdoc />
        public bool IsLocalHuman => true;

        /// <inheritdoc />
        public void RequestCommand(PlayerView view, Action<PlayCardCommand> submit)
        {
            _table.BeginHumanTurn(view, submit);
        }

        /// <inheritdoc />
        public void CancelRequest()
        {
            _table.EndHumanTurn();
        }
    }
}
