using System;
using System.Collections.Generic;
using System.Linq;

namespace Escoba.Core
{
    /// <summary>
    /// Lo que ve un asiento: su mano, la mesa, recuentos de las manos ajenas y del mazo, y la
    /// información pública de cada bando. Nunca contiene manos ajenas ni el orden del mazo.
    /// Es lo único que reciben la IA, las vistas y (en online) cada cliente. Es una instantánea:
    /// no cambia aunque el estado avance.
    /// </summary>
    public sealed class PlayerView
    {
        internal PlayerView(GameState state, int seat)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (seat < 0 || seat >= state.PlayerCount)
            {
                throw new ArgumentOutOfRangeException(nameof(seat), seat, "Asiento no válido.");
            }

            Seat = seat;
            Side = state.SideOfSeat(seat);
            Rules = state.Rules.Clone();
            PlayerCount = state.PlayerCount;
            Hand = state.HandsList[seat].ToArray();
            Table = state.TableList.ToArray();
            HandCounts = state.HandsList.Select(h => h.Count).ToArray();
            DeckCount = state.DeckList.Count;
            Phase = state.Phase;
            RoundNumber = state.RoundNumber;
            Dealer = state.Dealer;
            CurrentSeat = state.CurrentSeat;
            LastCapturerSeat = state.LastCapturerSeat;
            WinnerSide = state.WinnerSide;
            LastRoundScore = state.LastRoundScore;
            Sides = state.SidesList
                .Select(s => new SideView(s.CapturedList.ToArray(), s.Escobas, s.Score))
                .ToArray();
        }

        /// <summary>Asiento dueño de la vista.</summary>
        public int Seat { get; }

        /// <summary>Bando del dueño de la vista.</summary>
        public int Side { get; }

        /// <summary>Copia de las reglas.</summary>
        public RulesConfig Rules { get; }

        /// <summary>Número de jugadores.</summary>
        public int PlayerCount { get; }

        /// <summary>Mano propia.</summary>
        public IReadOnlyList<Card> Hand { get; }

        /// <summary>Cartas de la mesa.</summary>
        public IReadOnlyList<Card> Table { get; }

        /// <summary>Número de cartas en la mano de cada asiento.</summary>
        public IReadOnlyList<int> HandCounts { get; }

        /// <summary>Cartas que quedan en el mazo.</summary>
        public int DeckCount { get; }

        /// <summary>Fase de la partida.</summary>
        public GamePhase Phase { get; }

        /// <summary>Ronda actual.</summary>
        public int RoundNumber { get; }

        /// <summary>Asiento del repartidor.</summary>
        public int Dealer { get; }

        /// <summary>Asiento al que le toca jugar.</summary>
        public int CurrentSeat { get; }

        /// <summary>Último asiento que capturó en la ronda.</summary>
        public int LastCapturerSeat { get; }

        /// <summary>Bando ganador (-1 si la partida sigue).</summary>
        public int WinnerSide { get; }

        /// <summary>Puntuación de la última ronda terminada.</summary>
        public RoundScore LastRoundScore { get; }

        /// <summary>Información pública de cada bando.</summary>
        public IReadOnlyList<SideView> Sides { get; }

        /// <summary>Indica si le toca jugar al dueño de la vista.</summary>
        public bool IsMyTurn => Phase == GamePhase.Playing && CurrentSeat == Seat;
    }
}
