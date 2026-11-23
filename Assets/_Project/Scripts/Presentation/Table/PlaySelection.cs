using System;
using System.Collections.Generic;
using System.Linq;
using Escoba.Core;

namespace Escoba.Presentation
{
    /// <summary>
    /// Estado de la selección del jugador humano durante su turno (C# puro, testeable):
    /// carta de la mano elegida, cartas de la mesa marcadas, suma, si se puede confirmar y qué
    /// indicación mostrar. Usa <see cref="LegalMoves"/> como única fuente de verdad.
    /// </summary>
    public sealed class PlaySelection
    {
        private readonly IReadOnlyList<Card> _hand;
        private readonly IReadOnlyList<Card> _table;
        private readonly RulesConfig _rules;
        private readonly List<Card> _selectedTable = new List<Card>();
        private IReadOnlyList<IReadOnlyList<Card>> _captures = Array.Empty<IReadOnlyList<Card>>();
        private HashSet<Card> _candidates = new HashSet<Card>();

        public PlaySelection(IReadOnlyList<Card> hand, IReadOnlyList<Card> table, RulesConfig rules)
        {
            _hand = hand ?? throw new ArgumentNullException(nameof(hand));
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>Se lanza cuando cambia la selección.</summary>
        public event Action Changed;

        /// <summary>Carta de la mano seleccionada (null si ninguna).</summary>
        public Card? SelectedCard { get; private set; }

        /// <summary>Cartas de la mesa marcadas.</summary>
        public IReadOnlyList<Card> SelectedTableCards => _selectedTable;

        /// <summary>Capturas posibles con la carta seleccionada.</summary>
        public IReadOnlyList<IReadOnlyList<Card>> Captures => _captures;

        /// <summary>Suma de la carta seleccionada y las cartas marcadas.</summary>
        public int Sum => (SelectedCard?.Value ?? 0) + _selectedTable.Sum(c => c.Value);

        /// <summary>Suma objetivo de las reglas.</summary>
        public int CaptureSum => _rules.CaptureSum;

        /// <summary>La selección marcada es una captura válida.</summary>
        public bool IsValidCapture => SelectedCard.HasValue && _selectedTable.Count > 0 && Sum == _rules.CaptureSum;

        /// <summary>Dejar la carta en la mesa está permitido con la carta seleccionada.</summary>
        public bool CanDrop => SelectedCard.HasValue && (_captures.Count == 0 || !_rules.CaptureIsMandatory);

        /// <summary>Se puede confirmar la jugada.</summary>
        public bool CanConfirm => IsValidCapture || (_selectedTable.Count == 0 && CanDrop);

        /// <summary>Indicación para el jugador.</summary>
        public SelectionHint Hint
        {
            get
            {
                if (!SelectedCard.HasValue)
                {
                    return SelectionHint.SelectCard;
                }

                if (_captures.Count == 0)
                {
                    return SelectionHint.WillDrop;
                }

                if (IsValidCapture)
                {
                    return SelectionHint.Ready;
                }

                return _rules.CaptureIsMandatory ? SelectionHint.MustCapture : SelectionHint.ChooseCapture;
            }
        }

        /// <summary>Indica si una carta de la mesa forma parte de alguna captura posible.</summary>
        public bool IsCandidate(Card tableCard) => _candidates.Contains(tableCard);

        /// <summary>Indica si una carta de la mesa está marcada.</summary>
        public bool IsSelected(Card tableCard) => _selectedTable.Contains(tableCard);

        /// <summary>
        /// Selecciona una carta de la mano (o la deselecciona si ya lo estaba). Si solo hay una
        /// captura posible, la marca automáticamente.
        /// </summary>
        public void SelectHandCard(Card card)
        {
            if (!_hand.Contains(card))
            {
                throw new ArgumentException($"{card} no está en la mano.", nameof(card));
            }

            _selectedTable.Clear();
            if (SelectedCard == card)
            {
                SelectedCard = null;
                _captures = Array.Empty<IReadOnlyList<Card>>();
                _candidates = new HashSet<Card>();
            }
            else
            {
                SelectedCard = card;
                _captures = LegalMoves.FindCaptures(card, _table, _rules.CaptureSum);
                _candidates = new HashSet<Card>(_captures.SelectMany(c => c));
                if (_captures.Count == 1)
                {
                    _selectedTable.AddRange(_captures[0]);
                }
            }

            Changed?.Invoke();
        }

        /// <summary>Marca o desmarca una carta de la mesa. Solo admite candidatas. Devuelve si cambió algo.</summary>
        public bool ToggleTableCard(Card card)
        {
            if (!SelectedCard.HasValue || !_candidates.Contains(card))
            {
                return false;
            }

            if (!_selectedTable.Remove(card))
            {
                _selectedTable.Add(card);
            }

            Changed?.Invoke();
            return true;
        }

        /// <summary>Quita toda la selección.</summary>
        public void Clear()
        {
            SelectedCard = null;
            _selectedTable.Clear();
            _captures = Array.Empty<IReadOnlyList<Card>>();
            _candidates = new HashSet<Card>();
            Changed?.Invoke();
        }

        /// <summary>Construye el comando de la selección. Lanza si no se puede confirmar.</summary>
        public PlayCardCommand BuildCommand(int seat)
        {
            if (!CanConfirm || !SelectedCard.HasValue)
            {
                throw new InvalidOperationException("La selección no es una jugada válida.");
            }

            return _selectedTable.Count > 0
                ? PlayCardCommand.Capture(seat, SelectedCard.Value, _selectedTable)
                : PlayCardCommand.Drop(seat, SelectedCard.Value);
        }

        /// <summary>
        /// Jugada directa (doble toque o arrastre a la mesa): si la carta no captura, la deja;
        /// si tiene una única captura, la hace. Devuelve null si hay varias capturas (hay que elegir).
        /// </summary>
        public PlayCardCommand TryQuickPlay(int seat, Card card)
        {
            IReadOnlyList<IReadOnlyList<Card>> captures = LegalMoves.FindCaptures(card, _table, _rules.CaptureSum);
            if (captures.Count == 0)
            {
                return PlayCardCommand.Drop(seat, card);
            }

            if (captures.Count == 1 && _rules.CaptureIsMandatory)
            {
                return PlayCardCommand.Capture(seat, card, captures[0]);
            }

            return null;
        }
    }
}
