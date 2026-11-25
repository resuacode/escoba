using System.Globalization;
using TMPro;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Medidor oculto de rendimiento (FPS medios y mínimos, tiempo de decisión de la IA) para
    /// probar en móvil y web. Se activa y desactiva tocando 5 veces la versión del menú principal.
    /// Texto técnico, no localizado (solo diagnóstico).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class PerfOverlay : MonoBehaviour
    {
        private const float Window = 1f;

        private TMP_Text _text;
        private float _elapsed;
        private int _frames;
        private float _worstFrame;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            bool visible = PerfStats.OverlayVisible;
            _text.enabled = visible;
            if (!visible)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;
            _elapsed += dt;
            _frames++;
            _worstFrame = Mathf.Max(_worstFrame, dt);
            if (_elapsed < Window)
            {
                return;
            }

            float fps = _frames / _elapsed;
            float minFps = _worstFrame > 0f ? 1f / _worstFrame : 0f;
            _text.text = string.Format(CultureInfo.InvariantCulture,
                "{0:0} FPS (mín. {1:0}) · IA {2:0.0} ms (máx. {3:0.0} ms)",
                fps, minFps, PerfStats.LastBotMilliseconds, PerfStats.MaxBotMilliseconds);
            _elapsed = 0f;
            _frames = 0;
            _worstFrame = 0f;
        }
    }
}
