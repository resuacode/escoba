using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Escala el Canvas para que el diseño de referencia (1920×1080) más un margen quepa siempre
    /// entero dentro de la zona segura de la pantalla, sea cual sea su proporción: en pantallas
    /// muy panorámicas (móviles 20:9, navegador con barras) manda la altura y sobra ancho; en
    /// pantallas más cuadradas (tablets 4:3) manda el ancho y sobra alto. El espacio sobrante
    /// queda en los bordes, donde se anclan marcador, menú y créditos.
    /// Sustituye al modo "Scale With Screen Size" (que con mezcla 0,5 dejaba menos de 1080 de alto
    /// en pantallas panorámicas y pegaba los botones a los bordes).
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class SafeCanvasScaler : MonoBehaviour
    {
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920f, 1080f);

        [Tooltip("Margen mínimo (unidades de referencia) entre el diseño y el borde de la zona segura.")]
        [SerializeField, Min(0f)] private float _margin = 24f;

        private CanvasScaler _scaler;
        private Rect _lastSafe;
        private Vector2Int _lastScreen;

        /// <summary>Margen mínimo en unidades de referencia.</summary>
        public float Margin => _margin;

        /// <summary>
        /// Factor de escala para que <paramref name="reference"/> + 2·<paramref name="margin"/>
        /// quepa en <paramref name="safeSizePixels"/>.
        /// </summary>
        public static float ComputeScale(Vector2 safeSizePixels, Vector2 reference, float margin)
        {
            float width = reference.x + (2f * margin);
            float height = reference.y + (2f * margin);
            if (safeSizePixels.x <= 0f || safeSizePixels.y <= 0f || width <= 0f || height <= 0f)
            {
                return 1f;
            }

            return Mathf.Min(safeSizePixels.x / width, safeSizePixels.y / height);
        }

        private void OnEnable()
        {
            _scaler = GetComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            Apply();
        }

        private void Update()
        {
            if (_lastSafe != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafe = Screen.safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (_scaler == null)
            {
                return;
            }

            _scaler.scaleFactor = Mathf.Max(0.01f, ComputeScale(_lastSafe.size, _referenceResolution, _margin));
        }
    }
}
