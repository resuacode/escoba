using Escoba.Core;

namespace Escoba.App
{
    /// <summary>
    /// Recuerda la última configuración de "Nueva partida", por separado para "Jugar contra la IA"
    /// y "Jugar en este dispositivo". Los nombres son comunes a ambos modos.
    /// Datos corruptos o incoherentes se sustituyen por los valores por defecto.
    /// </summary>
    public static class SetupPreferences
    {
        private const char HumanCode = 'H';
        private const char BotCode = 'B';

        /// <summary>Carga la última selección del modo indicado.</summary>
        public static SetupSelection Load(ISettingsStore store, bool allHumans)
        {
            SetupSelection selection = SetupSelection.Default(allHumans);
            string prefix = Prefix(allHumans);

            int count = store.GetInt(prefix + "players", selection.PlayerCount);
            if (count >= 2 && count <= SetupSelection.MaxPlayers)
            {
                selection.PlayerCount = count;
            }

            selection.Teams = selection.PlayerCount == SetupSelection.MaxPlayers && store.GetInt(prefix + "teams", 0) == 1;

            string seats = store.GetString(prefix + "seats", string.Empty);
            if (seats.Length == SetupSelection.MaxPlayers && IsValidSeatCode(seats))
            {
                for (int i = 0; i < SetupSelection.MaxPlayers; i++)
                {
                    selection.Seats[i] = seats[i] == HumanCode ? SeatType.Human : SeatType.Bot;
                }
            }

            if (selection.HumanCount == 0)
            {
                selection.Seats[0] = SeatType.Human;
            }

            for (int i = 0; i < SetupSelection.MaxPlayers; i++)
            {
                selection.Names[i] = PlayerNameRules.Sanitize(store.GetString("setup.name." + i, string.Empty));
            }

            return selection;
        }

        /// <summary>Guarda la selección del modo indicado.</summary>
        public static void Save(ISettingsStore store, bool allHumans, SetupSelection selection)
        {
            string prefix = Prefix(allHumans);
            store.SetInt(prefix + "players", selection.PlayerCount);
            store.SetInt(prefix + "teams", selection.Teams ? 1 : 0);
            var seats = new char[SetupSelection.MaxPlayers];
            for (int i = 0; i < seats.Length; i++)
            {
                seats[i] = selection.Seats[i] == SeatType.Human ? HumanCode : BotCode;
            }

            store.SetString(prefix + "seats", new string(seats));
            for (int i = 0; i < SetupSelection.MaxPlayers; i++)
            {
                store.SetString("setup.name." + i, PlayerNameRules.Sanitize(selection.Names[i]) ?? string.Empty);
            }

            store.Save();
        }

        private static string Prefix(bool allHumans) => allHumans ? "setup.local." : "setup.ai.";

        private static bool IsValidSeatCode(string seats)
        {
            foreach (char c in seats)
            {
                if (c != HumanCode && c != BotCode)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
