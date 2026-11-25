using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Botón que crece con la interfaz grande (más fácil de pulsar en móvil). Puede desplazarse a
    /// la vez para no pisar a sus vecinos. Se usa en los botones de jugada y en "Menú".
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScalableButton : MonoBehaviour
    {
        [SerializeField] private float _largeScale = 1.25f;
        [SerializeField] private Vector2 _largeOffset;

        private RectTransform _rect;
        private Vector2 _basePosition;
        private bool _captured;

        /// <summary>Escala con la interfaz grande.</summary>
        public float LargeScale => _largeScale;

        private void OnEnable()
        {
            AccessibilityOptions.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            AccessibilityOptions.Changed -= Apply;
        }

        private void Apply()
        {
            if (!_captured)
            {
                _rect = (RectTransform)transform;
                _basePosition = _rect.anchoredPosition;
                _captured = true;
            }

            bool large = AccessibilityOptions.LargeUi;
            _rect.localScale = Vector3.one * (large ? _largeScale : 1f);
            _rect.anchoredPosition = _basePosition + (large ? _largeOffset : Vector2.zero);
        }
    }
}
