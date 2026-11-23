using System.Collections.Generic;
using Escoba.Presentation;

namespace Escoba.App
{
    /// <summary>
    /// Nombres visibles de asientos y bandos:
    /// con un solo humano, "Tú" y "Rival" (1 contra 1) o "IA n"; con varios humanos,
    /// "Jugador n" / "IA n" (n = asiento + 1). Con parejas, los bandos son "Pareja 1" y "Pareja 2".
    /// </summary>
    public static class PlayerNaming
    {
        /// <summary>Nombre de cada asiento.</summary>
        public static string[] SeatNames(IReadOnlyList<SeatType> seats)
        {
            int humans = 0;
            foreach (SeatType seat in seats)
            {
                if (seat == SeatType.Human)
                {
                    humans++;
                }
            }

            var names = new string[seats.Count];
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i] == SeatType.Human)
                {
                    names[i] = humans == 1 ? Localization.Get("game.you") : Localization.Get("player.human", i + 1);
                }
                else
                {
                    names[i] = humans == 1 && seats.Count == 2 ? Localization.Get("game.opponent") : Localization.Get("player.bot", i + 1);
                }
            }

            return names;
        }

        /// <summary>Nombre de cada bando (pareja o jugador).</summary>
        public static string[] SideNames(IReadOnlyList<string> seatNames, bool teams)
        {
            if (!teams)
            {
                var copy = new string[seatNames.Count];
                for (int i = 0; i < copy.Length; i++)
                {
                    copy[i] = seatNames[i];
                }

                return copy;
            }

            return new[] { Localization.Get("team.name", 1), Localization.Get("team.name", 2) };
        }

        /// <summary>Miembros de una pareja ("Jugador 1 y Jugador 3").</summary>
        public static string TeamMembers(IReadOnlyList<string> seatNames, int side)
        {
            return Localization.Get("team.members", seatNames[side], seatNames[side + 2]);
        }
    }
}
