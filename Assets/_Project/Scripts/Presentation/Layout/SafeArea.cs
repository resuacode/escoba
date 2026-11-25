using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Ajusta este RectTransform a la zona segura de la pantalla (muescas, esquinas redondeadas,
    /// barras del sistema) y deja además un margen interior para que nada quede pegado al borde.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        [Tooltip("Margen interior en unidades del Canvas (el mismo que SafeCanvasScaler.Margin).")]
        [SerializeField, Min(0f)] private float _padding = 24f;

        private Rect _applied;
        private Vector2Int _screen;

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (_applied != Screen.safeArea || _screen.x != Screen.width || _screen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect safe = Screen.safeArea;
            _applied = safe;
            _screen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rect.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rect.offsetMin = new Vector2(_padding, _padding);
            rect.offsetMax = new Vector2(-_padding, -_padding);
        }
    }
}
