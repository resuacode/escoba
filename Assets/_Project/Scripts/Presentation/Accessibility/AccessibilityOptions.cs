using System;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Opciones de accesibilidad que lee la presentación (como <see cref="Localization"/>). La App
    /// las fija a partir de los ajustes del jugador; los componentes se actualizan con <see cref="Changed"/>.
    /// </summary>
    public static class AccessibilityOptions
    {
        /// <summary>Escala de los textos con la interfaz grande.</summary>
        public const float LargeTextScale = 1.2f;

        /// <summary>Se lanza al cambiar cualquier opción.</summary>
        public static event Action Changed;

        /// <summary>Resaltados en alto contraste (no dependen solo del color).</summary>
        public static bool HighContrast { get; private set; }

        /// <summary>Interfaz grande (textos y botones de jugada).</summary>
        public static bool LargeUi { get; private set; }

        /// <summary>Multiplicador de tamaño de texto.</summary>
        public static float TextScale => LargeUi ? LargeTextScale : 1f;

        /// <summary>Aplica las opciones (solo avisa si cambian).</summary>
        public static void Set(bool highContrast, bool largeUi)
        {
            if (highContrast == HighContrast && largeUi == LargeUi)
            {
                return;
            }

            HighContrast = highContrast;
            LargeUi = largeUi;
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            HighContrast = false;
            LargeUi = false;
            Changed = null;
        }
    }
}
