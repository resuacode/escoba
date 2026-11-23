using System;

namespace Escoba.AI
{
    /// <summary>Crea la estrategia correspondiente a cada nivel de dificultad.</summary>
    public static class BotFactory
    {
        /// <summary>Indica si el nivel ya está implementado.</summary>
        public static bool IsAvailable(BotDifficulty difficulty) => difficulty == BotDifficulty.Easy;

        /// <summary>Crea la estrategia. Lanza <see cref="NotSupportedException"/> para niveles aún no implementados.</summary>
        public static IBotStrategy Create(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.Easy:
                    return new EasyBot();
                default:
                    throw new NotSupportedException($"El nivel {difficulty} llegará en M4.");
            }
        }
    }
}
