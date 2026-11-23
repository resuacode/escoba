using Escoba.AI;

namespace Escoba.App
{
    /// <summary>Configuración de la partida que se va a jugar (elegida en el menú).</summary>
    public sealed class MatchConfig
    {
        public MatchConfig(RulesPreset preset, BotDifficulty difficulty, ulong? seed = null)
        {
            Preset = preset;
            Difficulty = difficulty;
            Seed = seed;
        }

        /// <summary>Reglas.</summary>
        public RulesPreset Preset { get; }

        /// <summary>Dificultad del rival.</summary>
        public BotDifficulty Difficulty { get; }

        /// <summary>Semilla fija (para reproducir una partida) o null para una aleatoria.</summary>
        public ulong? Seed { get; }

        /// <summary>Número de jugadores (1 contra 1 en M2).</summary>
        public int PlayerCount => 2;
    }
}
