using TMPro;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Agranda el texto con la interfaz grande (<see cref="AccessibilityOptions.TextScale"/>) sin
    /// salirse de su caja: con escala mayor que 1 activa el autoajuste con el tamaño agrandado como
    /// máximo, de modo que el texto crece solo hasta donde cabe. Con escala 1 restaura los valores
    /// originales. Lo añade <c>UiFactory.CreateText</c> a todos los textos de las escenas generadas.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public sealed class ScalableText : MonoBehaviour
    {
        private TMP_Text _text;
        private bool _captured;
        private bool _baseAutoSize;
        private float _baseSize;
        private float _baseMin;
        private float _baseMax;

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
                _text = GetComponent<TMP_Text>();
                _baseAutoSize = _text.enableAutoSizing;
                _baseSize = _text.fontSize;
                _baseMin = _text.fontSizeMin;
                _baseMax = _text.fontSizeMax;
                _captured = true;
            }

            float scale = AccessibilityOptions.TextScale;
            if (scale <= 1.001f)
            {
                _text.enableAutoSizing = _baseAutoSize;
                _text.fontSize = _baseSize;
                _text.fontSizeMin = _baseMin;
                _text.fontSizeMax = _baseMax;
                return;
            }

            _text.enableAutoSizing = true;
            if (_baseAutoSize)
            {
                _text.fontSizeMin = _baseMin;
                _text.fontSizeMax = _baseMax * scale;
            }
            else
            {
                _text.fontSizeMin = Mathf.Min(_baseSize, _baseSize * 0.8f);
                _text.fontSizeMax = _baseSize * scale;
            }
        }
    }
}
