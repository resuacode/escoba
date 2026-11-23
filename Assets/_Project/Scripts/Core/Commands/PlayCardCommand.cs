using System;
using System.Collections.Generic;
using System.Linq;

namespace Escoba.Core
{
    /// <summary>
    /// Jugar una carta de la mano. Si <see cref="CaptureIds"/> no está vacío, la carta captura
    /// esas cartas de la mesa; si está vacío, la carta se deja en la mesa.
    /// </summary>
    public sealed class PlayCardCommand : IGameCommand
    {
        private readonly int[] _captureIds;

        public PlayCardCommand(int seat, int cardId, IEnumerable<int> captureIds = null)
        {
            Seat = seat;
            CardId = cardId;
            _captureIds = captureIds == null ? Array.Empty<int>() : captureIds.ToArray();
        }

        /// <inheritdoc />
        public int Seat { get; }

        /// <summary>Id de la carta jugada.</summary>
        public int CardId { get; }

        /// <summary>Ids de las cartas de la mesa que se capturan (vacío = dejar carta).</summary>
        public IReadOnlyList<int> CaptureIds => _captureIds;

        /// <summary>Indica si la jugada es una captura.</summary>
        public bool IsCapture => _captureIds.Length > 0;

        /// <summary>Crea una jugada que deja la carta en la mesa.</summary>
        public static PlayCardCommand Drop(int seat, Card card) => new PlayCardCommand(seat, card.Id);

        /// <summary>Crea una jugada que captura las cartas indicadas.</summary>
        public static PlayCardCommand Capture(int seat, Card card, IEnumerable<Card> captured)
        {
            if (captured == null)
            {
                throw new ArgumentNullException(nameof(captured));
            }

            return new PlayCardCommand(seat, card.Id, captured.Select(c => c.Id));
        }

        public override string ToString()
        {
            string card = Card.IsValidId(CardId) ? Card.FromId(CardId).ToString() : CardId.ToString();
            if (!IsCapture)
            {
                return $"Seat {Seat}: drop {card}";
            }

            string captured = string.Join(", ", _captureIds.Select(id => Card.IsValidId(id) ? Card.FromId(id).ToString() : id.ToString()));
            return $"Seat {Seat}: {card} captures [{captured}]";
        }
    }
}
