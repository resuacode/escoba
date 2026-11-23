using System;
using System.Collections.Generic;
using System.Linq;
using Escoba.AI;

namespace Escoba.App
{
    /// <summary>
    /// Configuración de la partida elegida en el menú: reglas, asientos (humano o IA), parejas,
    /// dificultad de la IA y semilla opcional.
    /// </summary>
    public sealed class MatchConfig
    {
        public MatchConfig(RulesPreset preset, BotDifficulty difficulty, IReadOnlyList<SeatType> seats, bool teams = false, ulong? seed = null)
        {
            if (seats == null || seats.Count < 2 || seats.Count > 4)
            {
                throw new ArgumentException("Debe haber entre 2 y 4 asientos.", nameof(seats));
            }

            if (teams && seats.Count != 4)
            {
                throw new ArgumentException("Las parejas solo son posibles con 4 jugadores.", nameof(teams));
            }

            Preset = preset;
            Difficulty = difficulty;
            Seats = seats.ToArray();
            Teams = teams;
            Seed = seed;
        }

        /// <summary>Reglas.</summary>
        public RulesPreset Preset { get; }

        /// <summary>Dificultad de los asientos de IA.</summary>
        public BotDifficulty Difficulty { get; }

        /// <summary>Tipo de cada asiento.</summary>
        public IReadOnlyList<SeatType> Seats { get; }

        /// <summary>Partida por parejas (asientos 0-2 contra 1-3).</summary>
        public bool Teams { get; }

        /// <summary>Semilla fija (para reproducir una partida) o null para una aleatoria.</summary>
        public ulong? Seed { get; }

        /// <summary>Número de jugadores.</summary>
        public int PlayerCount => Seats.Count;

        /// <summary>Número de humanos.</summary>
        public int HumanCount => Seats.Count(s => s == SeatType.Human);

        /// <summary>Varios humanos comparten el dispositivo: hace falta pantalla de pase.</summary>
        public bool IsHotSeat => HumanCount >= 2;

        /// <summary>Partida contra la IA: el asiento 0 es humano y el resto, IA.</summary>
        public static MatchConfig Versus(RulesPreset preset, BotDifficulty difficulty, int playerCount = 2, bool teams = false)
        {
            var seats = new SeatType[playerCount];
            for (int i = 1; i < playerCount; i++)
            {
                seats[i] = SeatType.Bot;
            }

            return new MatchConfig(preset, difficulty, seats, teams);
        }

        /// <summary>Misma configuración con otra semilla (null = aleatoria), para la revancha.</summary>
        public MatchConfig WithSeed(ulong? seed) => new MatchConfig(Preset, Difficulty, Seats, Teams, seed);
    }
}
