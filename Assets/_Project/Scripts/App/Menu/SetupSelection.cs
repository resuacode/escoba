using System.Linq;

namespace Escoba.App
{
    /// <summary>Selección del panel "Nueva partida" (jugadores, parejas, asientos y nombres).</summary>
    public sealed class SetupSelection
    {
        public const int MaxPlayers = 4;

        /// <summary>Número de jugadores (2-4).</summary>
        public int PlayerCount { get; set; } = 2;

        /// <summary>Por parejas (solo con 4).</summary>
        public bool Teams { get; set; }

        /// <summary>Tipo de cada uno de los 4 asientos posibles.</summary>
        public SeatType[] Seats { get; } = new SeatType[MaxPlayers];

        /// <summary>Nombre escrito para cada asiento (null = por defecto).</summary>
        public string[] Names { get; } = new string[MaxPlayers];

        /// <summary>Número de humanos entre los asientos en uso.</summary>
        public int HumanCount => Seats.Take(PlayerCount).Count(s => s == SeatType.Human);

        /// <summary>Selección por defecto: contra la IA (un humano) o en este dispositivo (todos humanos).</summary>
        public static SetupSelection Default(bool allHumans)
        {
            var selection = new SetupSelection();
            for (int i = 0; i < MaxPlayers; i++)
            {
                selection.Seats[i] = i == 0 || allHumans ? SeatType.Human : SeatType.Bot;
            }

            return selection;
        }
    }
}
