using System;
using System.Collections.Generic;
using System.Linq;
using Escoba.Core;

namespace Escoba.Tests.EditMode
{
    /// <summary>
    /// Ayudas para construir estados explícitos (sin barajar) en los tests de reglas.
    /// Notación de cartas: número + inicial del palo, p. ej. "7o" (7 de oros), "10b" (sota de bastos),
    /// "11c" (caballo de copas), "12e" (rey de espadas).
    /// </summary>
    internal static class TestGame
    {
        /// <summary>Carta a partir de la notación corta.</summary>
        public static Card C(string notation)
        {
            if (string.IsNullOrEmpty(notation) || notation.Length < 2)
            {
                throw new ArgumentException($"Notación de carta no válida: '{notation}'.");
            }

            char suitChar = notation[notation.Length - 1];
            int rank = int.Parse(notation.Substring(0, notation.Length - 1));
            Suit suit;
            switch (suitChar)
            {
                case 'o': suit = Suit.Oros; break;
                case 'c': suit = Suit.Copas; break;
                case 'e': suit = Suit.Espadas; break;
                case 'b': suit = Suit.Bastos; break;
                default: throw new ArgumentException($"Palo no válido en '{notation}'.");
            }

            return Card.Of(suit, rank);
        }

        /// <summary>Lista de cartas a partir de notaciones separadas por espacios ("7o 5c 10b").</summary>
        public static Card[] Cards(string notations)
        {
            if (string.IsNullOrWhiteSpace(notations))
            {
                return Array.Empty<Card>();
            }

            return notations.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(C).ToArray();
        }

        /// <summary>Reglas por defecto con el número de jugadores indicado.</summary>
        public static RulesConfig Rules(int players = 2, bool teams = false)
        {
            return new RulesConfig { PlayerCount = players, Teams = teams };
        }

        /// <summary>
        /// Crea un motor en fase <see cref="GamePhase.Playing"/> con manos, mesa y mazo explícitos.
        /// Por defecto el repartidor es el último asiento, así que el asiento 0 es "mano" y juega primero.
        /// </summary>
        public static GameEngine Create(
            RulesConfig rules,
            string[] hands,
            string table = "",
            string deck = "",
            int dealer = -1,
            int lastCapturer = -1,
            int[] scores = null)
        {
            var state = new GameState(rules.Clone(), new Pcg32Random(1));
            if (hands.Length != state.PlayerCount)
            {
                throw new ArgumentException("Debe haber una mano por jugador.");
            }

            for (int seat = 0; seat < hands.Length; seat++)
            {
                state.HandsList[seat].AddRange(Cards(hands[seat]));
            }

            state.TableList.AddRange(Cards(table));
            state.DeckList.AddRange(Cards(deck));
            state.Dealer = dealer >= 0 ? dealer : state.PlayerCount - 1;
            state.LastCapturerSeat = lastCapturer;
            state.RoundNumber = 1;
            state.Phase = GamePhase.Playing;
            state.CurrentSeat = state.Mano;
            if (scores != null)
            {
                for (int i = 0; i < scores.Length; i++)
                {
                    state.SidesList[i].Score = scores[i];
                }
            }

            return new GameEngine(state);
        }

        /// <summary>Comando de captura con notación corta.</summary>
        public static PlayCardCommand Capture(int seat, string card, string captured)
        {
            return PlayCardCommand.Capture(seat, C(card), Cards(captured));
        }

        /// <summary>Comando de dejar carta con notación corta.</summary>
        public static PlayCardCommand Drop(int seat, string card)
        {
            return PlayCardCommand.Drop(seat, C(card));
        }

        /// <summary>Primer evento del tipo indicado, o null.</summary>
        public static T First<T>(IEnumerable<GameEvent> events)
            where T : GameEvent
        {
            return events.OfType<T>().FirstOrDefault();
        }
    }
}
