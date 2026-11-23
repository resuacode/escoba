using System;
using System.Collections.Generic;
using Escoba.Core;

namespace Escoba.AI
{
    /// <summary>
    /// Bot de nivel Fácil: elige al azar la carta que juega. Si esa carta puede capturar y la
    /// captura es obligatoria, captura con una combinación al azar; si no es obligatoria, captura
    /// con un 60% de probabilidad. Ver skill <c>escoba-ai</c>.
    /// </summary>
    public sealed class EasyBot : IBotStrategy
    {
        private const int CaptureChancePercent = 60;
        private const int PercentRange = 100;

        /// <inheritdoc />
        public PlayCardCommand ChooseCommand(PlayerView view, IRandom rng)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            if (!view.IsMyTurn || view.Hand.Count == 0)
            {
                throw new InvalidOperationException($"No es el turno del asiento {view.Seat}.");
            }

            Card card = view.Hand[rng.Next(view.Hand.Count)];
            IReadOnlyList<IReadOnlyList<Card>> captures = LegalMoves.FindCaptures(card, view.Table, view.Rules.CaptureSum);
            if (captures.Count == 0)
            {
                return PlayCardCommand.Drop(view.Seat, card);
            }

            bool capture = view.Rules.CaptureIsMandatory || rng.Next(PercentRange) < CaptureChancePercent;
            if (!capture)
            {
                return PlayCardCommand.Drop(view.Seat, card);
            }

            return PlayCardCommand.Capture(view.Seat, card, captures[rng.Next(captures.Count)]);
        }
    }
}
