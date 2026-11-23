using System;

namespace Escoba.AI
{
    /// <summary>Crea la estrategia correspondiente a cada nivel de dificultad.</summary>
    public static class BotFactory
    {
        /// <summary>Indica si el nivel está implementado.</summary>
        public static bool IsAvailable(BotDifficulty difficulty) =>
            difficulty == BotDifficulty.Easy || difficulty == BotDifficulty.Medium || difficulty == BotDifficulty.Hard;

        /// <summary>Crea la estrategia del nivel indicado.</summary>
        public static IBotStrategy Create(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.Easy:
                    return new EasyBot();
                case BotDifficulty.Medium:
                    return new MediumBot();
                case BotDifficulty.Hard:
                    return new HardBot();
                default:
                    throw new NotSupportedException($"Nivel desconocido: {difficulty}.");
            }
        }
    }
}
