using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Punto de acceso a los efectos de sonido para la presentación (como <see cref="Localization"/>).
    /// Lo inicializa el bootstrap de la App; sin servicio (p. ej. en tests de EditMode) no suena nada.
    /// </summary>
    public static class Sounds
    {
        /// <summary>Servicio activo (null antes del bootstrap).</summary>
        public static AudioService Service { get; private set; }

        /// <summary>Establece el servicio activo.</summary>
        public static void Initialize(AudioService service)
        {
            Service = service;
        }

        /// <summary>Reproduce un efecto.</summary>
        public static void Play(SoundCue cue)
        {
            if (Service != null)
            {
                Service.Play(cue);
            }
        }

        /// <summary>Reproduce un efecto con retardo (segundos de juego).</summary>
        public static void Play(SoundCue cue, float delay)
        {
            if (Service != null)
            {
                Service.Play(cue, delay);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Service = null;
        }
    }
}
