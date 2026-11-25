using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Medidas de rendimiento para el medidor oculto (<see cref="PerfOverlay"/>): tiempo de
    /// decisión de la IA. Solo diagnóstico; no influye en la partida.
    /// </summary>
    public static class PerfStats
    {
        /// <summary>Si el medidor está visible (se activa tocando 5 veces la versión del menú).</summary>
        public static bool OverlayVisible { get; set; }

        /// <summary>Última decisión de la IA, en milisegundos.</summary>
        public static double LastBotMilliseconds { get; private set; }

        /// <summary>Decisión más lenta de la IA desde que se abrió el juego, en milisegundos.</summary>
        public static double MaxBotMilliseconds { get; private set; }

        /// <summary>Registra una decisión de la IA.</summary>
        public static void RecordBotDecision(double milliseconds)
        {
            LastBotMilliseconds = milliseconds;
            if (milliseconds > MaxBotMilliseconds)
            {
                MaxBotMilliseconds = milliseconds;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OverlayVisible = false;
            LastBotMilliseconds = 0;
            MaxBotMilliseconds = 0;
        }
    }
}
