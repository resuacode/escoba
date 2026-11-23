using System;
using System.Collections.Generic;
using System.Linq;

namespace Escoba.Core
{
    /// <summary>
    /// Motor de reglas de la Escoba. Es el único que modifica <see cref="GameState"/>:
    /// valida cada <see cref="IGameCommand"/>, lo aplica y devuelve los <see cref="GameEvent"/>
    /// resultantes. Determinista: misma semilla + mismos comandos = misma partida.
    /// </summary>
    public sealed class GameEngine
    {
        private const int DoubleEscobaMultiplier = 2;

        private readonly GameState _state;

        /// <summary>Crea una partida nueva con una copia validada de las reglas.</summary>
        public GameEngine(RulesConfig rules, ulong seed)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            RulesConfig copy = rules.Clone();
            copy.Validate();
            _state = new GameState(copy, new Pcg32Random(seed));
        }

        /// <summary>Crea un motor sobre un estado ya construido (tests e instantáneas).</summary>
        internal GameEngine(GameState state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        /// <summary>Estado completo (solo lectura). Para jugadores, usar <see cref="GetView"/>.</summary>
        public GameState State => _state;

        /// <summary>Elige al azar el primer repartidor y empieza la primera ronda.</summary>
        public IReadOnlyList<GameEvent> StartGame()
        {
            if (_state.Phase != GamePhase.NotStarted)
            {
                throw new InvalidOperationException("La partida ya ha empezado.");
            }

            var events = new List<GameEvent>();
            _state.Dealer = _state.Rng.Next(_state.PlayerCount);
            StartRound(events);
            return events;
        }

        /// <summary>Vista filtrada del estado para un asiento.</summary>
        public PlayerView GetView(int seat) => new PlayerView(_state, seat);

        /// <summary>Comprueba si un comando es válido en el estado actual, sin aplicarlo.</summary>
        public CommandValidation Validate(IGameCommand command)
        {
            switch (command)
            {
                case PlayCardCommand play:
                    return ValidatePlay(play);
                case StartNextRoundCommand _:
                    return _state.Phase == GamePhase.RoundOver
                        ? CommandValidation.Ok
                        : CommandValidation.Fail(CommandError.WrongPhase, $"No se puede empezar ronda en fase {_state.Phase}.");
                default:
                    return CommandValidation.Fail(CommandError.UnknownCommand, command?.GetType().Name ?? "null");
            }
        }

        /// <summary>
        /// Valida y aplica un comando. Lanza <see cref="InvalidCommandException"/> si no es válido
        /// (el estado no cambia en ese caso).
        /// </summary>
        public IReadOnlyList<GameEvent> Apply(IGameCommand command)
        {
            CommandValidation validation = Validate(command);
            if (!validation.IsValid)
            {
                throw new InvalidCommandException(validation);
            }

            var events = new List<GameEvent>();
            if (command is PlayCardCommand play)
            {
                ApplyPlay(play, events);
            }
            else
            {
                _state.Dealer = (_state.Dealer + 1) % _state.PlayerCount;
                StartRound(events);
            }

            return events;
        }

        /// <summary>
        /// Determina el bando ganador: el que alcanza el objetivo con la puntuación más alta,
        /// sin empate en cabeza. Devuelve -1 si la partida debe continuar.
        /// </summary>
        internal static int DetermineWinner(IReadOnlyList<int> scores, int targetScore)
        {
            int leader = RoundScorer.UniqueMaxIndex(scores);
            return leader >= 0 && scores[leader] >= targetScore ? leader : -1;
        }

        /// <summary>
        /// Si la mesa inicial suma 15 (o 30 en dos grupos de 15), el repartidor se la lleva con 1 (o 2) escobas.
        /// Devuelve el número de escobas anotadas.
        /// </summary>
        internal int ApplyDealerTableEscoba(List<GameEvent> events)
        {
            List<Card> table = _state.TableList;
            int target = _state.Rules.CaptureSum;
            int sum = table.Sum(c => c.Value);

            int escobas = 0;
            if (sum == target)
            {
                escobas = 1;
            }
            else if (sum == target * DoubleEscobaMultiplier && HasSubsetWithSum(table, target))
            {
                escobas = DoubleEscobaMultiplier;
            }

            if (escobas == 0)
            {
                return 0;
            }

            int dealer = _state.Dealer;
            SideState side = _state.SidesList[_state.SideOfSeat(dealer)];
            Card[] cards = table.ToArray();
            side.CapturedList.AddRange(cards);
            side.Escobas += escobas;
            table.Clear();
            _state.LastCapturerSeat = dealer;
            events.Add(new DealerTableEscobaEvent(dealer, cards, escobas));
            return escobas;
        }

        private void StartRound(List<GameEvent> events)
        {
            RulesConfig rules = _state.Rules;
            _state.RoundNumber++;
            foreach (List<Card> hand in _state.HandsList)
            {
                hand.Clear();
            }

            foreach (SideState side in _state.SidesList)
            {
                side.CapturedList.Clear();
                side.Escobas = 0;
            }

            _state.TableList.Clear();
            _state.LastCapturerSeat = -1;
            _state.DeckList.Clear();
            _state.DeckList.AddRange(Deck.CreateOrdered());
            Deck.Shuffle(_state.DeckList, _state.Rng);
            _state.Phase = GamePhase.Playing;
            events.Add(new RoundStartedEvent(_state.RoundNumber, _state.Dealer));

            DealHands(events);

            Card[] tableCards = DrawCards(rules.InitialTableCards);
            _state.TableList.AddRange(tableCards);
            events.Add(new TableDealtEvent(tableCards));

            if (rules.DealerTableEscoba)
            {
                ApplyDealerTableEscoba(events);
            }

            _state.CurrentSeat = _state.Mano;
            events.Add(new TurnChangedEvent(_state.CurrentSeat));
        }

        private void DealHands(List<GameEvent> events)
        {
            int playerCount = _state.PlayerCount;
            int mano = _state.Mano;
            for (int round = 0; round < _state.Rules.HandSize; round++)
            {
                for (int offset = 0; offset < playerCount; offset++)
                {
                    _state.HandsList[(mano + offset) % playerCount].Add(DrawCards(1)[0]);
                }
            }

            for (int offset = 0; offset < playerCount; offset++)
            {
                int seat = (mano + offset) % playerCount;
                events.Add(new HandDealtEvent(seat, _state.HandsList[seat].ToArray()));
            }
        }

        private Card[] DrawCards(int count)
        {
            List<Card> deck = _state.DeckList;
            if (count > deck.Count)
            {
                throw new InvalidOperationException($"No quedan {count} cartas en el mazo.");
            }

            // El "tope" del mazo es el final de la lista (eliminación O(1)).
            var cards = new Card[count];
            for (int i = 0; i < count; i++)
            {
                cards[i] = deck[deck.Count - 1];
                deck.RemoveAt(deck.Count - 1);
            }

            return cards;
        }

        private CommandValidation ValidatePlay(PlayCardCommand play)
        {
            if (_state.Phase != GamePhase.Playing)
            {
                return CommandValidation.Fail(CommandError.WrongPhase, $"No se puede jugar en fase {_state.Phase}.");
            }

            if (play.Seat < 0 || play.Seat >= _state.PlayerCount)
            {
                return CommandValidation.Fail(CommandError.InvalidSeat, $"Asiento {play.Seat} no existe.");
            }

            if (play.Seat != _state.CurrentSeat)
            {
                return CommandValidation.Fail(CommandError.NotYourTurn, $"Turno del asiento {_state.CurrentSeat}.");
            }

            if (!Card.IsValidId(play.CardId) || !_state.HandsList[play.Seat].Contains(Card.FromId(play.CardId)))
            {
                return CommandValidation.Fail(CommandError.CardNotInHand, $"La carta {play.CardId} no está en la mano.");
            }

            Card card = Card.FromId(play.CardId);
            int captureSum = _state.Rules.CaptureSum;

            if (!play.IsCapture)
            {
                if (_state.Rules.CaptureIsMandatory && LegalMoves.CanCapture(card, _state.TableList, captureSum))
                {
                    return CommandValidation.Fail(CommandError.CaptureIsMandatory, $"{card} puede capturar: la captura es obligatoria.");
                }

                return CommandValidation.Ok;
            }

            var seen = new HashSet<int>();
            int total = card.Value;
            foreach (int id in play.CaptureIds)
            {
                if (!seen.Add(id))
                {
                    return CommandValidation.Fail(CommandError.DuplicateCaptureCard, $"Carta {id} repetida en la captura.");
                }

                if (!Card.IsValidId(id) || !_state.TableList.Contains(Card.FromId(id)))
                {
                    return CommandValidation.Fail(CommandError.CaptureCardNotOnTable, $"La carta {id} no está en la mesa.");
                }

                total += Card.FromId(id).Value;
            }

            if (total != captureSum)
            {
                return CommandValidation.Fail(CommandError.CaptureSumMismatch, $"La captura suma {total}, no {captureSum}.");
            }

            return CommandValidation.Ok;
        }

        private void ApplyPlay(PlayCardCommand play, List<GameEvent> events)
        {
            int seat = play.Seat;
            Card card = Card.FromId(play.CardId);
            _state.HandsList[seat].Remove(card);
            bool isLastPlayOfRound = _state.DeckList.Count == 0 && AllHandsEmpty();

            if (play.IsCapture)
            {
                Card[] captured = play.CaptureIds.Select(Card.FromId).ToArray();
                foreach (Card c in captured)
                {
                    _state.TableList.Remove(c);
                }

                SideState side = _state.SidesList[_state.SideOfSeat(seat)];
                side.CapturedList.Add(card);
                side.CapturedList.AddRange(captured);
                _state.LastCapturerSeat = seat;

                bool isEscoba = _state.TableList.Count == 0
                                && (!isLastPlayOfRound || _state.Rules.LastPlayEscobaCounts);
                if (isEscoba)
                {
                    side.Escobas++;
                }

                events.Add(new CardsCapturedEvent(seat, card, captured, isEscoba));
            }
            else
            {
                _state.TableList.Add(card);
                events.Add(new CardPlayedEvent(seat, card));
            }

            AdvanceAfterPlay(seat, events);
        }

        private void AdvanceAfterPlay(int seat, List<GameEvent> events)
        {
            int next = (seat + 1) % _state.PlayerCount;
            if (!AllHandsEmpty())
            {
                SetTurn(next, events);
                return;
            }

            if (_state.DeckList.Count > 0)
            {
                DealHands(events);
                SetTurn(next, events);
                return;
            }

            EndRound(events);
        }

        private void SetTurn(int seat, List<GameEvent> events)
        {
            _state.CurrentSeat = seat;
            events.Add(new TurnChangedEvent(seat));
        }

        private void EndRound(List<GameEvent> events)
        {
            _state.CurrentSeat = -1;
            int lastCapturer = _state.LastCapturerSeat;
            if (_state.TableList.Count > 0 && lastCapturer >= 0)
            {
                Card[] remaining = _state.TableList.ToArray();
                _state.SidesList[_state.SideOfSeat(lastCapturer)].CapturedList.AddRange(remaining);
                _state.TableList.Clear();
                events.Add(new TableAwardedToLastCapturerEvent(lastCapturer, remaining));
            }

            RoundScore score = RoundScorer.Score(
                _state.Rules,
                _state.SidesList.Select(s => (IReadOnlyList<Card>)s.CapturedList).ToArray(),
                _state.SidesList.Select(s => s.Escobas).ToArray());

            for (int i = 0; i < _state.SideCount; i++)
            {
                _state.SidesList[i].Score += score.Sides[i].Total;
            }

            _state.LastRoundScore = score;
            events.Add(new RoundScoredEvent(score));

            int winner = DetermineWinner(_state.SidesList.Select(s => s.Score).ToArray(), _state.Rules.TargetScore);
            if (winner >= 0)
            {
                _state.Phase = GamePhase.GameOver;
                _state.WinnerSide = winner;
                events.Add(new GameOverEvent(winner));
            }
            else
            {
                _state.Phase = GamePhase.RoundOver;
            }
        }

        private bool AllHandsEmpty()
        {
            foreach (List<Card> hand in _state.HandsList)
            {
                if (hand.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSubsetWithSum(IReadOnlyList<Card> cards, int target)
        {
            int combinations = 1 << cards.Count;
            for (int mask = 1; mask < combinations - 1; mask++)
            {
                int sum = 0;
                for (int i = 0; i < cards.Count; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        sum += cards[i].Value;
                    }
                }

                if (sum == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
