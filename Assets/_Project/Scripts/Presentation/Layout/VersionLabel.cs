using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Escoba.Presentation
{
    /// <summary>
    /// Muestra la versión de la build (<see cref="Application.version"/>). En CI la versión incluye
    /// el número de ejecución y el commit (p. ej. "0.4.0-dev.12+cace7b2") para saber qué se prueba.
    /// Tocarla 5 veces seguidas muestra u oculta el medidor de rendimiento (<see cref="PerfOverlay"/>).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class VersionLabel : MonoBehaviour, IPointerClickHandler
    {
        private const int TapsToToggle = 5;
        private const float TapWindow = 2f;

        private int _taps;
        private float _firstTap;

        private void OnEnable()
        {
            GetComponent<TMP_Text>().text = "v" + Application.version;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            float now = Time.unscaledTime;
            if (_taps == 0 || now - _firstTap > TapWindow)
            {
                _taps = 0;
                _firstTap = now;
            }

            _taps++;
            if (_taps >= TapsToToggle)
            {
                _taps = 0;
                PerfStats.OverlayVisible = !PerfStats.OverlayVisible;
            }
        }
    }
}
