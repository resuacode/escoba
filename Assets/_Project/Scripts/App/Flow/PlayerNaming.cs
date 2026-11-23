using System.Collections.Generic;
using Escoba.Core;
using Escoba.Presentation;

namespace Escoba.App
{
    /// <summary>
    /// Nombres visibles de asientos y bandos. Un humano con nombre elegido usa ese nombre; si no,
    /// con un solo humano es "Tú", y con varios, "Jugador n". La IA es "Rival" (1 contra 1) o
    /// "IA n" (n = asiento + 1). Si dos nombres coinciden, se añade el número de asiento.
    /// Con parejas, los bandos son "Pareja 1" y "Pareja 2".
    /// </summary>
    public static class PlayerNaming
    {
        /// <summary>Nombre por defecto de un asiento humano (también es el texto de ayuda del campo de nombre).</summary>
        public static string DefaultHumanName(int seat, int humanCount)
        {
            return humanCount == 1 ? Localization.Get("game.you") : Localization.Get("player.human", seat + 1);
        }

        /// <summary>Nombre de cada asiento.</summary>
        public static string[] SeatNames(IReadOnlyList<SeatType> seats, IReadOnlyList<string> customNames = null)
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
                    string custom = customNames != null && i < customNames.Count ? PlayerNameRules.Sanitize(customNames[i]) : null;
                    names[i] = custom ?? DefaultHumanName(i, humans);
                }
                else
                {
                    names[i] = humans == 1 && seats.Count == 2 ? Localization.Get("game.opponent") : Localization.Get("player.bot", i + 1);
                }
            }

            // Nombres repetidos (p. ej. dos "Ana" o alguien que se llama "IA 2"): se distinguen por asiento.
            for (int i = 0; i < names.Length; i++)
            {
                for (int j = i + 1; j < names.Length; j++)
                {
                    if (string.Equals(names[i], names[j], System.StringComparison.OrdinalIgnoreCase))
                    {
                        names[j] = $"{names[j]} ({j + 1})";
                    }
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

        /// <summary>Miembros de una pareja ("Laura y Pablo").</summary>
        public static string TeamMembers(IReadOnlyList<string> seatNames, int side)
        {
            return Localization.Get("team.members", seatNames[side], seatNames[side + 2]);
        }
    }
}
