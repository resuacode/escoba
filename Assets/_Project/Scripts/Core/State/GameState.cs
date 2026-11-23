using System.Collections.Generic;

namespace Escoba.Core
{
    /// <summary>
    /// Estado completo de una partida. Solo <see cref="GameEngine"/> lo modifica (miembros internos);
    /// el resto del juego lo lee o, mejor, usa una <see cref="PlayerView"/> filtrada.
    /// </summary>
    public sealed class GameState
    {
        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        internal readonly List<Card>[] HandsList;
        internal readonly List<Card> TableList = new List<Card>();
        internal readonly List<Card> DeckList = new List<Card>();
        internal readonly SideState[] SidesList;

        internal GameState(RulesConfig rules, Pcg32Random rng)
        {
            Rules = rules;
            Rng = rng;
            HandsList = new List<Card>[rules.PlayerCount];
            for (int i = 0; i < HandsList.Length; i++)
            {
                HandsList[i] = new List<Card>();
            }

            SidesList = new SideState[rules.SideCount];
            for (int i = 0; i < SidesList.Length; i++)
            {
                SidesList[i] = new SideState();
            }
        }

        /// <summary>Reglas de la partida (copia propia del motor: no modificar).</summary>
        public RulesConfig Rules { get; }

        internal Pcg32Random Rng { get; set; }

        /// <summary>Número de jugadores.</summary>
        public int PlayerCount => HandsList.Length;

        /// <summary>Número de bandos que puntúan.</summary>
        public int SideCount => SidesList.Length;

        /// <summary>Cartas boca arriba en la mesa.</summary>
        public IReadOnlyList<Card> Table => TableList;

        /// <summary>Cartas que quedan en el mazo (su orden es información oculta).</summary>
        public int DeckCount => DeckList.Count;

        /// <summary>Fase actual.</summary>
        public GamePhase Phase { get; internal set; } = GamePhase.NotStarted;

        /// <summary>Ronda actual (1 en la primera).</summary>
        public int RoundNumber { get; internal set; }

        /// <summary>Asiento del repartidor (-1 antes de empezar).</summary>
        public int Dealer { get; internal set; } = -1;

        /// <summary>Asiento al que le toca jugar (-1 si no se esperan jugadas).</summary>
        public int CurrentSeat { get; internal set; } = -1;

        /// <summary>Último asiento que capturó en la ronda (-1 si nadie).</summary>
        public int LastCapturerSeat { get; internal set; } = -1;

        /// <summary>Bando ganador (-1 mientras la partida sigue).</summary>
        public int WinnerSide { get; internal set; } = -1;

        /// <summary>Puntuación de la última ronda terminada (null si aún no hay).</summary>
        public RoundScore LastRoundScore { get; internal set; }

        /// <summary>Asiento "mano": el siguiente al repartidor, que juega primero.</summary>
        public int Mano => Dealer < 0 ? 0 : (Dealer + 1) % PlayerCount;

        /// <summary>Mano del asiento indicado.</summary>
        public IReadOnlyList<Card> GetHand(int seat) => HandsList[seat];

        /// <summary>Estado del bando indicado.</summary>
        public SideState GetSide(int side) => SidesList[side];

        /// <summary>Bando al que pertenece un asiento.</summary>
        public int SideOfSeat(int seat) => Rules.SideOfSeat(seat);

        /// <summary>Total de cartas en manos, mesa, mazo y bazas (debe ser 40 en una partida real).</summary>
        public int TotalCardCount()
        {
            int total = TableList.Count + DeckList.Count;
            foreach (List<Card> hand in HandsList)
            {
                total += hand.Count;
            }

            foreach (SideState side in SidesList)
            {
                total += side.CapturedList.Count;
            }

            return total;
        }

        /// <summary>
        /// Huella determinista del estado (FNV-1a). Sirve para tests de determinismo
        /// y para detectar desincronización en red.
        /// </summary>
        public ulong ComputeHash()
        {
            ulong hash = FnvOffset;
            hash = Mix(hash, (long)Phase);
            hash = Mix(hash, RoundNumber);
            hash = Mix(hash, Dealer);
            hash = Mix(hash, CurrentSeat);
            hash = Mix(hash, LastCapturerSeat);
            hash = Mix(hash, WinnerSide);
            foreach (List<Card> hand in HandsList)
            {
                hash = MixCards(hash, hand);
            }

            hash = MixCards(hash, TableList);
            hash = MixCards(hash, DeckList);
            foreach (SideState side in SidesList)
            {
                hash = MixCards(hash, side.CapturedList);
                hash = Mix(hash, side.Escobas);
                hash = Mix(hash, side.Score);
            }

            hash = Mix(hash, (long)Rng.State);
            return hash;
        }

        private static ulong MixCards(ulong hash, List<Card> cards)
        {
            hash = Mix(hash, cards.Count);
            foreach (Card card in cards)
            {
                hash = Mix(hash, card.Id);
            }

            return hash;
        }

        private static ulong Mix(ulong hash, long value)
        {
            unchecked
            {
                for (int i = 0; i < 8; i++)
                {
                    hash ^= (byte)(value >> (i * 8));
                    hash *= FnvPrime;
                }
            }

            return hash;
        }
    }
}
