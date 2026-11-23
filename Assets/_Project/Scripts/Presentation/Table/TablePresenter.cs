using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Escoba.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Escoba.Presentation
{
    /// <summary>
    /// Mesa de juego (1 contra 1 en M2): reproduce los <see cref="GameEvent"/> del motor como
    /// animaciones y gestiona la interacción del jugador local en su turno.
    /// Mantiene un modelo visual propio que se alimenta solo de eventos públicos y de la mano del
    /// jugador local (nunca lee las cartas ajenas), y puede compararse con la <see cref="PlayerView"/>.
    /// </summary>
    public sealed class TablePresenter : MonoBehaviour
    {
        private const int BottomPosition = 0;
        private const int TopPosition = 1;

        [Header("Referencias")]
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private RectTransform _cardLayer;
        [SerializeField] private RectTransform _myHandAnchor;
        [SerializeField] private RectTransform _opponentHandAnchor;
        [SerializeField] private RectTransform _tableAnchor;
        [SerializeField] private RectTransform _tableDropZone;
        [SerializeField] private RectTransform _deckAnchor;
        [SerializeField] private RectTransform[] _pileAnchors = new RectTransform[2];
        [SerializeField] private HudView _hud;

        [Header("Tamaños (unidades del Canvas de referencia 1920×1080)")]
        [SerializeField] private float _cardHeight = 180f;
        [SerializeField] private float _opponentCardHeight = 120f;
        [SerializeField] private float _cardSpacing = 14f;
        [SerializeField] private float _opponentCardSpacing = -30f;
        [SerializeField] private float _rowSpacing = 16f;
        [SerializeField] private float _selectedLift = 36f;
        [SerializeField] private int _maxTableCardsPerRow = 8;

        [Header("Tiempos (segundos, a velocidad 1)")]
        [SerializeField] private float _dealInterval = 0.08f;
        [SerializeField] private float _moveDuration = 0.25f;
        [SerializeField] private float _capturePause = 0.35f;
        [SerializeField] private float _bannerDuration = 1.0f;

        private readonly Dictionary<int, CardView> _faceViews = new Dictionary<int, CardView>();
        private readonly List<CardView> _opponentBacks = new List<CardView>();
        private readonly List<Card> _myHand = new List<Card>();
        private readonly List<Card> _table = new List<Card>();
        private readonly int[] _pileCounts = new int[2];
        private readonly int[] _escobas = new int[2];

        private CardSkin _skin;
        private RulesConfig _rules;
        private int _mySeat;
        private int _deckCount;
        private PlaySelection _selection;
        private Action<PlayCardCommand> _submit;

        /// <summary>Multiplicador de velocidad de las animaciones (1 = normal).</summary>
        public float Speed { get; set; } = 1f;

        /// <summary>Indica si está esperando la jugada del jugador local.</summary>
        public bool IsWaitingForHuman => _submit != null;

        private float MoveTime => _moveDuration / Mathf.Max(0.01f, Speed);

        private float DealTime => _dealInterval / Mathf.Max(0.01f, Speed);

        /// <summary>Prepara la mesa para una partida.</summary>
        public void Initialize(CardSkin skin, RulesConfig rules, int mySeat)
        {
            _skin = skin != null ? skin : throw new ArgumentNullException(nameof(skin));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _mySeat = mySeat;
            _hud.SetBackSprite(skin.Back);
            _hud.SetTarget(rules.TargetScore);
            _hud.SetName(BottomPosition, Localization.Get("game.you"));
            _hud.SetName(TopPosition, Localization.Get("game.opponent"));
            _hud.PlayButton.onClick.RemoveListener(OnPlayClicked);
            _hud.PlayButton.onClick.AddListener(OnPlayClicked);
            _hud.UndoButton.onClick.RemoveListener(OnUndoClicked);
            _hud.UndoButton.onClick.AddListener(OnUndoClicked);
            ClearTable();
            UpdateScores(new int[2]);
        }

        /// <summary>Nombre visible de un asiento.</summary>
        public string SeatName(int seat) => Localization.Get(seat == _mySeat ? "game.you" : "game.opponent");

        /// <summary>Actualiza el marcador (puntuación acumulada por bando).</summary>
        public void UpdateScores(IReadOnlyList<int> scoresBySide)
        {
            for (int side = 0; side < scoresBySide.Count && side < 2; side++)
            {
                _hud.SetScore(PositionOfSide(side), scoresBySide[side]);
            }
        }

        /// <summary>Muestra el turno de la IA "pensando".</summary>
        public void ShowThinking(int seat)
        {
            _hud.SetTurn(Localization.Get("game.thinking", SeatName(seat)));
        }

        /// <summary>Reproduce un evento del motor (corrutina).</summary>
        public IEnumerator Play(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case RoundStartedEvent e:
                    ClearTable();
                    _hud.SetRound(e.RoundNumber);
                    _hud.SetDealer(PositionOfSeat(e.Dealer));
                    break;
                case HandDealtEvent e:
                    yield return DealHand(e);
                    break;
                case TableDealtEvent e:
                    yield return DealTable(e.Cards);
                    break;
                case DealerTableEscobaEvent e:
                    yield return SweepToPile(e.Seat, _table.ToList(), e.Escobas, Localization.Get("game.dealer_escoba", SeatName(e.Seat)));
                    break;
                case CardPlayedEvent e:
                    yield return PlayToTable(e.Seat, e.Card);
                    break;
                case CardsCapturedEvent e:
                    yield return Capture(e);
                    break;
                case TurnChangedEvent e:
                    _hud.SetTurn(e.Seat == _mySeat ? Localization.Get("game.your_turn") : Localization.Get("game.turn_of", SeatName(e.Seat)));
                    break;
                case TableAwardedToLastCapturerEvent e:
                    yield return SweepToPile(e.Seat, e.Cards.ToList(), 0, Localization.Get("game.table_to_last", SeatName(e.Seat)));
                    break;
            }
        }

        /// <summary>Empieza el turno del jugador local: habilita la selección y los botones.</summary>
        public void BeginHumanTurn(PlayerView view, Action<PlayCardCommand> submit)
        {
            _submit = submit ?? throw new ArgumentNullException(nameof(submit));
            _selection = new PlaySelection(view.Hand, view.Table, view.Rules);
            _selection.Changed += RefreshSelection;
            foreach (Card card in _myHand)
            {
                _faceViews[card.Id].DragEnabled = true;
            }

            RefreshSelection();
        }

        /// <summary>Termina el turno del jugador local (sin jugar).</summary>
        public void EndHumanTurn()
        {
            if (_selection != null)
            {
                _selection.Changed -= RefreshSelection;
            }

            _selection = null;
            _submit = null;
            foreach (CardView view in _faceViews.Values)
            {
                view.DragEnabled = false;
                view.SetHighlight(CardHighlight.None);
                view.SetDimmed(false);
            }

            _hud.SetHint(null);
            _hud.SetSum(null);
            _hud.SetPlayControls(false, false, false);
            LayoutMyHand(true);
        }

        /// <summary>
        /// Compara el modelo visual con la vista del motor. Devuelve una descripción de las
        /// diferencias o null si coinciden (útil en tests y depuración).
        /// </summary>
        public string FindInconsistency(PlayerView view)
        {
            if (!new HashSet<Card>(view.Hand).SetEquals(_myHand))
            {
                return "La mano mostrada no coincide con la del motor.";
            }

            if (!new HashSet<Card>(view.Table).SetEquals(_table))
            {
                return "La mesa mostrada no coincide con la del motor.";
            }

            int opponentSeat = (_mySeat + 1) % view.PlayerCount;
            if (view.HandCounts[opponentSeat] != _opponentBacks.Count)
            {
                return $"El rival tiene {view.HandCounts[opponentSeat]} cartas y se muestran {_opponentBacks.Count}.";
            }

            if (view.DeckCount != _deckCount)
            {
                return $"El mazo tiene {view.DeckCount} cartas y se muestran {_deckCount}.";
            }

            for (int side = 0; side < view.Sides.Count && side < 2; side++)
            {
                if (view.Sides[side].Captured.Count != _pileCounts[PositionOfSide(side)])
                {
                    return $"La baza del bando {side} no coincide.";
                }
            }

            return null;
        }

        private int PositionOfSeat(int seat) => PositionOfSide(_rules.SideOfSeat(seat));

        private int PositionOfSide(int side) => side == _rules.SideOfSeat(_mySeat) ? BottomPosition : TopPosition;

        private Vector2 LayerPosition(RectTransform anchor)
        {
            Vector3 local = _cardLayer.InverseTransformPoint(anchor.position);
            return new Vector2(local.x, local.y);
        }

        private void ClearTable()
        {
            if (_selection != null)
            {
                EndHumanTurn();
            }

            foreach (CardView view in _faceViews.Values)
            {
                view.gameObject.SetActive(false);
            }

            foreach (CardView back in _opponentBacks)
            {
                Destroy(back.gameObject);
            }

            _opponentBacks.Clear();
            _myHand.Clear();
            _table.Clear();
            _deckCount = Core.Card.DeckSize;
            for (int i = 0; i < 2; i++)
            {
                _pileCounts[i] = 0;
                _escobas[i] = 0;
                _hud.SetPileCount(i, 0);
                _hud.SetEscobas(i, 0);
            }

            _hud.SetDeckCount(_deckCount);
        }

        private CardView ShowFace(Card card, Vector2 position)
        {
            if (!_faceViews.TryGetValue(card.Id, out CardView view))
            {
                view = Instantiate(_cardPrefab, _cardLayer);
                view.name = $"Card {card}";
                view.Clicked += OnCardClicked;
                view.DragStarted += OnCardDragStarted;
                view.DragEnded += OnCardDragEnded;
                _faceViews.Add(card.Id, view);
            }

            view.gameObject.SetActive(true);
            view.Bind(_skin, card, true, _cardHeight);
            view.transform.SetAsLastSibling();
            view.SnapTo(position);
            return view;
        }

        private CardView CreateOpponentBack(Vector2 position)
        {
            CardView back = Instantiate(_cardPrefab, _cardLayer);
            back.name = "Opponent card";
            back.Bind(_skin, null, false, _opponentCardHeight);
            back.SnapTo(position);
            return back;
        }

        private IEnumerator DealHand(HandDealtEvent e)
        {
            Vector2 deck = LayerPosition(_deckAnchor);
            _deckCount -= e.Cards.Count;
            _hud.SetDeckCount(_deckCount);

            if (e.Seat == _mySeat)
            {
                foreach (Card card in e.Cards)
                {
                    ShowFace(card, deck);
                    _myHand.Add(card);
                }

                LayoutMyHand(true, DealTime);
            }
            else
            {
                // Las cartas del rival no se leen: solo cuántas son.
                for (int i = 0; i < e.Cards.Count; i++)
                {
                    _opponentBacks.Add(CreateOpponentBack(deck));
                }

                LayoutOpponentHand(true, DealTime);
            }

            yield return Wait(MoveTime + (DealTime * e.Cards.Count));
        }

        private IEnumerator DealTable(IReadOnlyList<Card> cards)
        {
            Vector2 deck = LayerPosition(_deckAnchor);
            _deckCount -= cards.Count;
            _hud.SetDeckCount(_deckCount);
            foreach (Card card in cards)
            {
                ShowFace(card, deck);
                _table.Add(card);
            }

            LayoutTable(true, DealTime);
            yield return Wait(MoveTime + (DealTime * cards.Count));
        }

        private CardView TakeFromHand(int seat, Card card)
        {
            if (seat == _mySeat)
            {
                _myHand.Remove(card);
                CardView own = _faceViews[card.Id];
                own.DragEnabled = false;
                own.transform.SetAsLastSibling();
                return own;
            }

            CardView back = _opponentBacks[_opponentBacks.Count - 1];
            _opponentBacks.RemoveAt(_opponentBacks.Count - 1);
            Vector2 from = back.Rect.anchoredPosition;
            Destroy(back.gameObject);
            return ShowFace(card, from);
        }

        private IEnumerator PlayToTable(int seat, Card card)
        {
            TakeFromHand(seat, card);
            _table.Add(card);
            LayoutTable(true);
            LayoutHands();
            yield return Wait(MoveTime);
        }

        private IEnumerator Capture(CardsCapturedEvent e)
        {
            CardView played = TakeFromHand(e.Seat, e.PlayedCard);
            LayoutHands();

            // La carta jugada se posa sobre las capturadas, que se resaltan un instante.
            Vector2 target = e.Captured.Select(c => _faceViews[c.Id].Rect.anchoredPosition).Aggregate(Vector2.zero, (a, b) => a + b) / e.Captured.Count;
            played.MoveTo(target + new Vector2(0f, 30f), MoveTime);
            foreach (Card card in e.Captured)
            {
                _faceViews[card.Id].SetHighlight(CardHighlight.Selected);
            }

            yield return Wait(MoveTime + (_capturePause / Mathf.Max(0.01f, Speed)));

            var cards = new List<Card> { e.PlayedCard };
            cards.AddRange(e.Captured);
            yield return SweepToPile(e.Seat, cards, e.IsEscoba ? 1 : 0, e.IsEscoba ? Localization.Get("game.escoba") : null);
        }

        private IEnumerator SweepToPile(int seat, List<Card> cards, int escobas, string banner)
        {
            int position = PositionOfSeat(seat);
            Vector2 pile = LayerPosition(_pileAnchors[position]);
            foreach (Card card in cards)
            {
                _table.Remove(card);
                CardView view = _faceViews[card.Id];
                view.SetHighlight(CardHighlight.None);
                view.MoveTo(pile, MoveTime);
            }

            yield return Wait(MoveTime);
            foreach (Card card in cards)
            {
                _faceViews[card.Id].gameObject.SetActive(false);
            }

            _pileCounts[position] += cards.Count;
            _escobas[position] += escobas;
            _hud.SetPileCount(position, _pileCounts[position]);
            _hud.SetEscobas(position, _escobas[position]);
            LayoutTable(true);

            if (!string.IsNullOrEmpty(banner))
            {
                yield return _hud.ShowBanner(banner, _bannerDuration / Mathf.Max(0.01f, Speed));
            }
        }

        private void LayoutHands()
        {
            LayoutMyHand(true);
            LayoutOpponentHand(true);
        }

        private void LayoutMyHand(bool animate, float stagger = 0f)
        {
            float width = CardWidth(_cardHeight);
            Vector2[] slots = Row(_myHand.Count, LayerPosition(_myHandAnchor), width + _cardSpacing);
            for (int i = 0; i < _myHand.Count; i++)
            {
                Vector2 slot = slots[i];
                if (_selection != null && _selection.SelectedCard == _myHand[i])
                {
                    slot.y += _selectedLift;
                }

                Place(_faceViews[_myHand[i].Id], slot, animate, stagger * i);
            }
        }

        private void LayoutOpponentHand(bool animate, float stagger = 0f)
        {
            float width = CardWidth(_opponentCardHeight);
            Vector2[] slots = Row(_opponentBacks.Count, LayerPosition(_opponentHandAnchor), width + _opponentCardSpacing);
            for (int i = 0; i < _opponentBacks.Count; i++)
            {
                Place(_opponentBacks[i], slots[i], animate, stagger * i);
            }
        }

        private void LayoutTable(bool animate, float stagger = 0f)
        {
            int count = _table.Count;
            if (count == 0)
            {
                return;
            }

            int perRow = Mathf.Max(1, _maxTableCardsPerRow);
            int rows = Mathf.CeilToInt(count / (float)perRow);
            float height = rows > 2 ? _cardHeight * 2f / 3f : _cardHeight;
            float width = CardWidth(height);
            Vector2 center = LayerPosition(_tableAnchor);
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                int inRow = Mathf.Min(perRow, count - index);
                float y = center.y + (((rows - 1) / 2f) - row) * (height + _rowSpacing);
                Vector2[] slots = Row(inRow, new Vector2(center.x, y), width + _cardSpacing);
                for (int i = 0; i < inRow; i++, index++)
                {
                    CardView view = _faceViews[_table[index].Id];
                    view.SetHeight(height);
                    Place(view, slots[i], animate, stagger * index);
                }
            }
        }

        private void Place(CardView view, Vector2 position, bool animate, float delay)
        {
            if (animate)
            {
                view.MoveTo(position, MoveTime, delay);
            }
            else
            {
                view.SnapTo(position);
            }
        }

        private float CardWidth(float height)
        {
            Vector2 native = _skin.NativeSize;
            return height * native.x / native.y;
        }

        private static Vector2[] Row(int count, Vector2 center, float step)
        {
            var slots = new Vector2[count];
            float start = center.x - (step * (count - 1) / 2f);
            for (int i = 0; i < count; i++)
            {
                slots[i] = new Vector2(start + (step * i), center.y);
            }

            return slots;
        }

        private static IEnumerator Wait(float seconds)
        {
            if (seconds > 0f)
            {
                yield return new WaitForSeconds(seconds);
            }
        }

        // ---- Interacción del jugador local ----

        private void RefreshSelection()
        {
            if (_selection == null)
            {
                return;
            }

            bool cardSelected = _selection.SelectedCard.HasValue;
            bool hasCaptures = _selection.Captures.Count > 0;
            foreach (Card card in _myHand)
            {
                _faceViews[card.Id].SetHighlight(_selection.SelectedCard == card ? CardHighlight.Selected : CardHighlight.None);
            }

            foreach (Card card in _table)
            {
                CardView view = _faceViews[card.Id];
                if (_selection.IsSelected(card))
                {
                    view.SetHighlight(CardHighlight.Selected);
                }
                else
                {
                    view.SetHighlight(_selection.IsCandidate(card) ? CardHighlight.Candidate : CardHighlight.None);
                }

                view.SetDimmed(cardSelected && hasCaptures && !_selection.IsCandidate(card));
            }

            _hud.SetHint(HintText(_selection.Hint));
            _hud.SetSum(cardSelected && hasCaptures ? Localization.Get("game.sum", _selection.Sum, _selection.CaptureSum) : null);
            _hud.SetPlayControls(true, _selection.CanConfirm, cardSelected);
            LayoutMyHand(true);
        }

        private static string HintText(SelectionHint hint)
        {
            switch (hint)
            {
                case SelectionHint.SelectCard: return Localization.Get("game.select_card");
                case SelectionHint.ChooseCapture: return Localization.Get("game.choose_capture");
                case SelectionHint.MustCapture: return Localization.Get("game.must_capture");
                case SelectionHint.WillDrop: return Localization.Get("game.drop_hint");
                default: return null;
            }
        }

        private void OnCardClicked(CardView view, PointerEventData eventData)
        {
            if (_selection == null || !view.Card.HasValue)
            {
                return;
            }

            Card card = view.Card.Value;
            if (_myHand.Contains(card))
            {
                if (eventData.clickCount >= 2)
                {
                    PlayCardCommand quick = _selection.TryQuickPlay(_mySeat, card);
                    if (quick != null)
                    {
                        Submit(quick);
                        return;
                    }

                    if (_selection.SelectedCard == card)
                    {
                        return;
                    }
                }

                _selection.SelectHandCard(card);
            }
            else if (_table.Contains(card))
            {
                _selection.ToggleTableCard(card);
            }
        }

        private void OnCardDragStarted(CardView view)
        {
            if (_selection != null && view.Card.HasValue && _selection.SelectedCard != view.Card)
            {
                _selection.SelectHandCard(view.Card.Value);
            }
        }

        private void OnCardDragEnded(CardView view, PointerEventData eventData)
        {
            if (_selection == null || !view.Card.HasValue)
            {
                return;
            }

            bool overTable = _tableDropZone != null
                             && RectTransformUtility.RectangleContainsScreenPoint(_tableDropZone, eventData.position, eventData.pressEventCamera);
            if (overTable)
            {
                PlayCardCommand quick = _selection.TryQuickPlay(_mySeat, view.Card.Value);
                if (quick != null)
                {
                    Submit(quick);
                    return;
                }
            }

            LayoutMyHand(true);
        }

        private void OnPlayClicked()
        {
            if (_selection != null && _selection.CanConfirm)
            {
                Submit(_selection.BuildCommand(_mySeat));
            }
        }

        private void OnUndoClicked()
        {
            _selection?.Clear();
        }

        private void Submit(PlayCardCommand command)
        {
            Action<PlayCardCommand> submit = _submit;
            EndHumanTurn();
            submit?.Invoke(command);
        }
    }
}
