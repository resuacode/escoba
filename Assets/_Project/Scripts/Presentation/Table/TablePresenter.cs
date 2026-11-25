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
    /// Mesa de juego para 2-4 asientos: reproduce los <see cref="GameEvent"/> del motor como
    /// animaciones y gestiona la interacción del jugador que mira la pantalla ("viewer", siempre
    /// abajo). El viewer puede cambiar (hot-seat) con <see cref="SetViewer"/>; su mano puede
    /// ocultarse (<see cref="SetViewerHandRevealed"/>) mientras no es su turno.
    /// El modelo visual solo se alimenta de eventos públicos y de la mano del viewer
    /// (nunca lee manos ajenas) y se compara con la <see cref="PlayerView"/> tras cada jugada.
    /// </summary>
    public sealed class TablePresenter : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private CardView _cardPrefab;
        [SerializeField] private RectTransform _cardLayer;
        [Tooltip("Anclas de mano por hueco: abajo, derecha, arriba, izquierda (ver SeatLayout).")]
        [SerializeField] private RectTransform[] _handAnchors = new RectTransform[SeatLayout.SlotCount];
        [Tooltip("Fichas de asiento por hueco: abajo, derecha, arriba, izquierda.")]
        [SerializeField] private SeatPanelView[] _seatPanels = new SeatPanelView[SeatLayout.SlotCount];
        [SerializeField] private RectTransform _tableAnchor;
        [SerializeField] private RectTransform _tableDropZone;
        [SerializeField] private RectTransform _deckAnchor;
        [SerializeField] private HudView _hud;
        [Tooltip("Capa de partículas (escoba, siete de oros). Opcional.")]
        [SerializeField] private UiParticles _effects;

        [Header("Tamaños (unidades del Canvas de referencia 1920×1080)")]
        [SerializeField] private float _cardHeight = 180f;
        [SerializeField] private float _opponentCardHeight = 110f;
        [SerializeField] private float _cardSpacing = 14f;
        [SerializeField] private float _opponentCardSpacing = -30f;
        [SerializeField] private float _sideCardSpacing = -60f;
        [SerializeField] private float _rowSpacing = 16f;
        [SerializeField] private float _selectedLift = 36f;
        [SerializeField] private int _maxTableCardsPerRow = 7;

        [Header("Tiempos (segundos, a velocidad 1)")]
        [SerializeField] private float _dealInterval = 0.08f;
        [SerializeField] private float _moveDuration = 0.25f;
        [SerializeField] private float _capturePause = 0.35f;
        [SerializeField] private float _bannerDuration = 1.0f;
        [SerializeField] private float _escobaBannerDuration = 1.4f;
        [SerializeField] private float _goldSevenDuration = 0.8f;

        private readonly Dictionary<int, CardView> _faceViews = new Dictionary<int, CardView>();
        private readonly List<Card> _viewerHand = new List<Card>();
        private readonly List<Card> _table = new List<Card>();
        private List<CardView>[] _backs = new List<CardView>[0];
        private int[] _pileCounts = new int[0];
        private int[] _escobas = new int[0];

        private CardSkin _skin;
        private RulesConfig _rules;
        private string[] _names = new string[0];
        private int _viewer;
        private bool _revealViewerHand = true;
        private int _deckCount;
        private int _dealer = -1;
        private int _turn = -1;
        private PlaySelection _selection;
        private Action<PlayCardCommand> _submit;

        /// <summary>Multiplicador de velocidad de las animaciones (1 = normal).</summary>
        public float Speed { get; set; } = 1f;

        /// <summary>Indica si está esperando la jugada del viewer.</summary>
        public bool IsWaitingForHuman => _submit != null;

        /// <summary>Asiento que mira la pantalla.</summary>
        public int ViewerSeat => _viewer;

        /// <summary>Indica si la mano del viewer se muestra boca arriba.</summary>
        public bool IsViewerHandRevealed => _revealViewerHand;

        private int PlayerCount => _rules.PlayerCount;

        private float MoveTime => _moveDuration / Mathf.Max(0.01f, Speed);

        private float DealTime => _dealInterval / Mathf.Max(0.01f, Speed);

        /// <summary>Prepara la mesa para una partida.</summary>
        public void Initialize(CardSkin skin, RulesConfig rules, IReadOnlyList<string> seatNames, int viewerSeat, bool revealViewerHand)
        {
            _skin = skin != null ? skin : throw new ArgumentNullException(nameof(skin));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _names = seatNames.ToArray();
            _viewer = viewerSeat;
            _revealViewerHand = revealViewerHand;
            _backs = Enumerable.Range(0, rules.PlayerCount).Select(_ => new List<CardView>()).ToArray();
            _pileCounts = new int[rules.SideCount];
            _escobas = new int[rules.SideCount];

            _hud.SetBackSprite(skin.Back);
            _hud.SetTarget(rules.TargetScore);
            foreach (SeatPanelView panel in _seatPanels)
            {
                panel.SetBackSprite(skin.Back);
            }

            _hud.PlayButton.onClick.RemoveListener(OnPlayClicked);
            _hud.PlayButton.onClick.AddListener(OnPlayClicked);
            _hud.UndoButton.onClick.RemoveListener(OnUndoClicked);
            _hud.UndoButton.onClick.AddListener(OnUndoClicked);
            ClearTable();
        }

        /// <summary>Nombre visible de un asiento.</summary>
        public string SeatName(int seat) => seat >= 0 && seat < _names.Length ? _names[seat] : string.Empty;

        /// <summary>Actualiza el marcador (puntuación acumulada y nombre por bando).</summary>
        public void UpdateScores(IReadOnlyList<string> sideNames, IReadOnlyList<int> scoresBySide)
        {
            _hud.SetScores(sideNames, scoresBySide);
        }

        /// <summary>Muestra que la IA está "pensando".</summary>
        public void ShowThinking(int seat)
        {
            _hud.SetTurn(Localization.Get("game.thinking", SeatName(seat)));
        }

        /// <summary>
        /// Cambia el jugador que mira la pantalla y recoloca toda la mesa desde su punto de vista
        /// (sin animación), con su mano boca arriba o boca abajo.
        /// </summary>
        public void SetViewer(PlayerView view, bool revealHand)
        {
            if (_selection != null)
            {
                EndHumanTurn();
            }

            HideAllCards();
            _viewer = view.Seat;
            _revealViewerHand = revealHand;
            _dealer = view.Dealer;
            _turn = view.CurrentSeat;

            foreach (Card card in view.Hand)
            {
                ShowFace(card, LayerPosition(_deckAnchor), revealHand);
                _viewerHand.Add(card);
            }

            for (int seat = 0; seat < PlayerCount; seat++)
            {
                if (seat == _viewer)
                {
                    continue;
                }

                for (int i = 0; i < view.HandCounts[seat]; i++)
                {
                    _backs[seat].Add(CreateBack(LayerPosition(_deckAnchor)));
                }
            }

            foreach (Card card in view.Table)
            {
                ShowFace(card, LayerPosition(_tableAnchor), true);
                _table.Add(card);
            }

            _deckCount = view.DeckCount;
            for (int side = 0; side < _pileCounts.Length; side++)
            {
                _pileCounts[side] = view.Sides[side].Captured.Count;
                _escobas[side] = view.Sides[side].Escobas;
            }

            _hud.SetDeckCount(_deckCount);
            RefreshSeatPanels();
            LayoutAll(false);
            if (revealHand && _viewerHand.Count > 0)
            {
                Sounds.Play(SoundCue.RevealHand);
            }
        }

        /// <summary>Muestra u oculta (boca abajo) la mano del viewer.</summary>
        public void SetViewerHandRevealed(bool reveal)
        {
            _revealViewerHand = reveal;
            foreach (Card card in _viewerHand)
            {
                _faceViews[card.Id].SetFaceUp(reveal);
            }
        }

        /// <summary>Reproduce un evento del motor (corrutina).</summary>
        public IEnumerator Play(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case RoundStartedEvent e:
                    ClearTable();
                    Sounds.Play(SoundCue.Shuffle);
                    _dealer = e.Dealer;
                    _hud.SetRound(e.RoundNumber);
                    RefreshSeatPanels();
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
                    _turn = e.Seat;
                    _hud.SetTurn(e.Seat == _viewer && _revealViewerHand
                        ? Localization.Get("game.your_turn")
                        : Localization.Get("game.turn_of", SeatName(e.Seat)));
                    RefreshSeatPanels();
                    break;
                case TableAwardedToLastCapturerEvent e:
                    yield return SweepToPile(e.Seat, e.Cards.ToList(), 0, Localization.Get("game.table_to_last", SeatName(e.Seat)));
                    break;
                case RoundScoredEvent _:
                    _turn = -1;
                    Sounds.Play(SoundCue.RoundEnd);
                    RefreshSeatPanels();
                    break;
            }
        }

        /// <summary>Empieza el turno del viewer: habilita la selección y los botones.</summary>
        public void BeginHumanTurn(PlayerView view, Action<PlayCardCommand> submit)
        {
            if (view.Seat != _viewer)
            {
                throw new InvalidOperationException($"La mesa muestra el asiento {_viewer}, no el {view.Seat}: llama antes a SetViewer.");
            }

            if (!_revealViewerHand)
            {
                SetViewerHandRevealed(true);
            }

            _hud.SetTurn(Localization.Get("game.your_turn"));
            _submit = submit ?? throw new ArgumentNullException(nameof(submit));
            _selection = new PlaySelection(view.Hand, view.Table, view.Rules);
            _selection.Changed += RefreshSelection;
            foreach (Card card in _viewerHand)
            {
                _faceViews[card.Id].DragEnabled = true;
            }

            RefreshSelection();
        }

        /// <summary>Termina el turno del viewer (sin jugar).</summary>
        public void EndHumanTurn()
        {
            if (_selection != null)
            {
                _selection.Changed -= RefreshSelection;
            }

            _selection = null;
            _submit = null;

            // Al descargar la escena (p. ej. botón Menú en mitad de un turno) los objetos pueden
            // estar ya destruidos: entonces solo se limpia el estado, sin tocar la parte visual.
            if (!IsSceneAlive())
            {
                return;
            }

            foreach (CardView view in _faceViews.Values)
            {
                if (view == null)
                {
                    continue;
                }

                view.DragEnabled = false;
                view.SetHighlight(CardHighlight.None);
                view.SetDimmed(false);
            }

            _hud.SetHint(null);
            _hud.SetSum(null);
            _hud.SetPlayControls(false, false, false);
            LayoutViewerHand(true);
        }

        /// <summary>
        /// Juega por el viewer sin pasar por la interacción (pruebas automáticas). Devuelve false
        /// si no se esperaba jugada.
        /// </summary>
        public bool SubmitAutomatically(PlayCardCommand command)
        {
            if (_submit == null)
            {
                return false;
            }

            Submit(command);
            return true;
        }

        /// <summary>
        /// Compara el modelo visual con la vista del motor. Devuelve una descripción de las
        /// diferencias o null si coinciden (útil en tests y depuración).
        /// </summary>
        public string FindInconsistency(PlayerView view)
        {
            if (view.Seat != _viewer)
            {
                return $"La vista es del asiento {view.Seat} pero la mesa muestra el {_viewer}.";
            }

            if (!new HashSet<Card>(view.Hand).SetEquals(_viewerHand))
            {
                return "La mano mostrada no coincide con la del motor.";
            }

            if (!new HashSet<Card>(view.Table).SetEquals(_table))
            {
                return "La mesa mostrada no coincide con la del motor.";
            }

            for (int seat = 0; seat < PlayerCount; seat++)
            {
                if (seat != _viewer && view.HandCounts[seat] != _backs[seat].Count)
                {
                    return $"El asiento {seat} tiene {view.HandCounts[seat]} cartas y se muestran {_backs[seat].Count}.";
                }
            }

            if (view.DeckCount != _deckCount)
            {
                return $"El mazo tiene {view.DeckCount} cartas y se muestran {_deckCount}.";
            }

            for (int side = 0; side < view.Sides.Count; side++)
            {
                if (view.Sides[side].Captured.Count != _pileCounts[side] || view.Sides[side].Escobas != _escobas[side])
                {
                    return $"La baza o las escobas del bando {side} no coinciden.";
                }
            }

            return null;
        }

        // ---- Modelo y colocación ----

        private bool IsSceneAlive()
        {
            return this != null && _cardLayer != null && _hud != null && _handAnchors[SeatLayout.Bottom] != null;
        }

        private int SlotOf(int seat) => SeatLayout.SlotOf(seat, _viewer, PlayerCount);

        private Vector2 LayerPosition(RectTransform anchor)
        {
            Vector3 local = _cardLayer.InverseTransformPoint(anchor.position);
            return new Vector2(local.x, local.y);
        }

        private void ClearTable()
        {
            HideAllCards();
            _deckCount = Card.DeckSize;
            for (int side = 0; side < _pileCounts.Length; side++)
            {
                _pileCounts[side] = 0;
                _escobas[side] = 0;
            }

            _hud.SetDeckCount(_deckCount);
            RefreshSeatPanels();
        }

        private void HideAllCards()
        {
            if (_selection != null)
            {
                EndHumanTurn();
            }

            foreach (CardView view in _faceViews.Values)
            {
                view.gameObject.SetActive(false);
            }

            foreach (List<CardView> backs in _backs)
            {
                foreach (CardView back in backs)
                {
                    Destroy(back.gameObject);
                }

                backs.Clear();
            }

            _viewerHand.Clear();
            _table.Clear();
        }

        private void RefreshSeatPanels()
        {
            if (_rules == null)
            {
                return;
            }

            var used = new bool[SeatLayout.SlotCount];
            for (int seat = 0; seat < PlayerCount; seat++)
            {
                int slot = SlotOf(seat);
                used[slot] = true;
                int side = _rules.SideOfSeat(seat);
                SeatPanelView panel = _seatPanels[slot];
                panel.gameObject.SetActive(true);
                panel.SetName(SeatName(seat));
                panel.SetPile(_pileCounts[side], _escobas[side]);
                panel.SetDealer(seat == _dealer);
                panel.SetActiveTurn(seat == _turn);
            }

            for (int slot = 0; slot < used.Length; slot++)
            {
                if (!used[slot])
                {
                    _seatPanels[slot].gameObject.SetActive(false);
                }
            }
        }

        private CardView ShowFace(Card card, Vector2 position, bool faceUp)
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
            view.Bind(_skin, card, faceUp, _cardHeight);
            view.transform.SetAsLastSibling();
            view.SnapTo(position);
            return view;
        }

        private CardView CreateBack(Vector2 position)
        {
            CardView back = Instantiate(_cardPrefab, _cardLayer);
            back.name = "Hidden card";
            back.Bind(_skin, null, false, _opponentCardHeight);
            back.SnapTo(position);
            return back;
        }

        private IEnumerator DealHand(HandDealtEvent e)
        {
            Vector2 deck = LayerPosition(_deckAnchor);
            _deckCount -= e.Cards.Count;
            _hud.SetDeckCount(_deckCount);

            if (e.Seat == _viewer)
            {
                foreach (Card card in e.Cards)
                {
                    ShowFace(card, deck, _revealViewerHand);
                    _viewerHand.Add(card);
                }
            }
            else
            {
                // Las cartas de otros asientos no se leen: solo cuántas son.
                for (int i = 0; i < e.Cards.Count; i++)
                {
                    _backs[e.Seat].Add(CreateBack(deck));
                }
            }

            LayoutSeat(e.Seat, true, DealTime);
            PlayDealSounds(e.Cards.Count);
            yield return Wait(MoveTime + (DealTime * e.Cards.Count));
        }

        private IEnumerator DealTable(IReadOnlyList<Card> cards)
        {
            Vector2 deck = LayerPosition(_deckAnchor);
            _deckCount -= cards.Count;
            _hud.SetDeckCount(_deckCount);
            foreach (Card card in cards)
            {
                ShowFace(card, deck, true);
                _table.Add(card);
            }

            LayoutTable(true, DealTime);
            PlayDealSounds(cards.Count);
            yield return Wait(MoveTime + (DealTime * cards.Count));
        }

        private void PlayDealSounds(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Sounds.Play(SoundCue.Deal, i * DealTime);
            }
        }

        private CardView TakeFromHand(int seat, Card card)
        {
            if (seat == _viewer)
            {
                _viewerHand.Remove(card);
                CardView own = _faceViews[card.Id];
                own.DragEnabled = false;
                own.SetFaceUp(true);
                own.SetHeight(_cardHeight);
                own.transform.SetAsLastSibling();
                return own;
            }

            List<CardView> backs = _backs[seat];
            CardView back = backs[backs.Count - 1];
            backs.RemoveAt(backs.Count - 1);
            Vector2 from = back.Rect.anchoredPosition;
            Destroy(back.gameObject);
            return ShowFace(card, from, true);
        }

        private IEnumerator PlayToTable(int seat, Card card)
        {
            TakeFromHand(seat, card);
            _table.Add(card);
            LayoutTable(true);
            LayoutSeat(seat, true);
            Sounds.Play(SoundCue.PlayCard, MoveTime * 0.8f);
            yield return Wait(MoveTime);
        }

        private IEnumerator Capture(CardsCapturedEvent e)
        {
            CardView played = TakeFromHand(e.Seat, e.PlayedCard);
            LayoutSeat(e.Seat, true);

            // La carta jugada se posa sobre las capturadas, que se resaltan un instante.
            Vector2 target = e.Captured.Select(c => _faceViews[c.Id].Rect.anchoredPosition).Aggregate(Vector2.zero, (a, b) => a + b) / e.Captured.Count;
            played.MoveTo(target + new Vector2(0f, 30f), MoveTime);
            Sounds.Play(SoundCue.PlayCard, MoveTime * 0.8f);
            foreach (Card card in e.Captured)
            {
                _faceViews[card.Id].SetHighlight(CardHighlight.Selected);
            }

            yield return Wait(MoveTime + (_capturePause / Mathf.Max(0.01f, Speed)));
            yield return CelebrateGoldSeven(e);

            var cards = new List<Card> { e.PlayedCard };
            cards.AddRange(e.Captured);
            yield return SweepToPile(e.Seat, cards, e.IsEscoba ? 1 : 0, e.IsEscoba ? Localization.Get("game.escoba") : null);
        }

        /// <summary>Si la captura incluye el siete de oros: la carta late, destellos y aviso corto.</summary>
        private IEnumerator CelebrateGoldSeven(CardsCapturedEvent e)
        {
            int goldSeven = e.PlayedCard.IsGoldSeven ? e.PlayedCard.Id : -1;
            foreach (Card card in e.Captured)
            {
                if (card.IsGoldSeven)
                {
                    goldSeven = card.Id;
                }
            }

            if (goldSeven < 0)
            {
                yield break;
            }

            float duration = _goldSevenDuration / Mathf.Max(0.01f, Speed);
            CardView view = _faceViews[goldSeven];
            view.transform.SetAsLastSibling();
            view.Punch(1.3f, duration * 0.6f);
            Sounds.Play(SoundCue.GoldSeven);
            if (_effects != null)
            {
                _effects.BurstAt(view.Rect, ParticleStyle.Sparkle, 30);
            }

            yield return _hud.ShowBanner(Localization.Get("game.gold_seven"), duration);
        }

        private IEnumerator SweepToPile(int seat, List<Card> cards, int escobas, string banner)
        {
            Vector2 pile = LayerPosition(_seatPanels[SlotOf(seat)].PileAnchor);
            Sounds.Play(SoundCue.Capture);
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

            int side = _rules.SideOfSeat(seat);
            _pileCounts[side] += cards.Count;
            _escobas[side] += escobas;
            RefreshSeatPanels();
            LayoutTable(true);

            if (escobas > 0)
            {
                Sounds.Play(SoundCue.Escoba);
                if (_effects != null)
                {
                    _effects.BurstAt(_tableAnchor, ParticleStyle.Sparkle, 40);
                    _effects.BurstAt(_tableAnchor, ParticleStyle.Confetti, 50);
                }
            }

            if (!string.IsNullOrEmpty(banner))
            {
                float duration = (escobas > 0 ? _escobaBannerDuration : _bannerDuration) / Mathf.Max(0.01f, Speed);
                yield return _hud.ShowBanner(banner, duration, escobas > 0);
            }
        }

        private void LayoutAll(bool animate)
        {
            for (int seat = 0; seat < PlayerCount; seat++)
            {
                LayoutSeat(seat, animate);
            }

            LayoutTable(animate);
        }

        private void LayoutSeat(int seat, bool animate, float stagger = 0f)
        {
            if (seat == _viewer)
            {
                LayoutViewerHand(animate, stagger);
                return;
            }

            int slot = SlotOf(seat);
            bool vertical = SeatLayout.IsVertical(slot);
            float step = vertical
                ? _opponentCardHeight + _sideCardSpacing
                : CardWidth(_opponentCardHeight) + _opponentCardSpacing;
            List<CardView> backs = _backs[seat];
            Vector2[] slots = Row(backs.Count, LayerPosition(_handAnchors[slot]), step, vertical);
            for (int i = 0; i < backs.Count; i++)
            {
                Place(backs[i], slots[i], animate, stagger * i);
            }
        }

        private void LayoutViewerHand(bool animate, float stagger = 0f)
        {
            float width = CardWidth(_cardHeight);
            Vector2[] slots = Row(_viewerHand.Count, LayerPosition(_handAnchors[SeatLayout.Bottom]), width + _cardSpacing, false);
            for (int i = 0; i < _viewerHand.Count; i++)
            {
                Vector2 slot = slots[i];
                if (_selection != null && _selection.SelectedCard == _viewerHand[i])
                {
                    slot.y += _selectedLift;
                }

                CardView view = _faceViews[_viewerHand[i].Id];
                view.SetHeight(_cardHeight);
                Place(view, slot, animate, stagger * i);
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
                float y = center.y + ((((rows - 1) / 2f) - row) * (height + _rowSpacing));
                Vector2[] slots = Row(inRow, new Vector2(center.x, y), width + _cardSpacing, false);
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

        private static Vector2[] Row(int count, Vector2 center, float step, bool vertical)
        {
            var slots = new Vector2[count];
            float start = -(step * (count - 1) / 2f);
            for (int i = 0; i < count; i++)
            {
                float offset = start + (step * i);
                slots[i] = vertical ? new Vector2(center.x, center.y - offset) : new Vector2(center.x + offset, center.y);
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

        // ---- Interacción del viewer ----

        private void RefreshSelection()
        {
            if (_selection == null)
            {
                return;
            }

            bool cardSelected = _selection.SelectedCard.HasValue;
            bool hasCaptures = _selection.Captures.Count > 0;
            foreach (Card card in _viewerHand)
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
            LayoutViewerHand(true);
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
            if (_viewerHand.Contains(card))
            {
                if (eventData.clickCount >= 2)
                {
                    PlayCardCommand quick = _selection.TryQuickPlay(_viewer, card);
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
                PlayCardCommand quick = _selection.TryQuickPlay(_viewer, view.Card.Value);
                if (quick != null)
                {
                    Submit(quick);
                    return;
                }
            }

            LayoutViewerHand(true);
        }

        private void OnPlayClicked()
        {
            if (_selection != null && _selection.CanConfirm)
            {
                Submit(_selection.BuildCommand(_viewer));
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
