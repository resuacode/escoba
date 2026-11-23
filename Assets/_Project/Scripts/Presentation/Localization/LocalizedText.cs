using System;
using TMPro;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Pone en un <see cref="TMP_Text"/> el texto de una clave de localización y lo actualiza al
    /// cambiar de idioma. Para textos con parámetros, usar <see cref="Set"/> desde código.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key = string.Empty;

        private object[] _args = Array.Empty<object>();
        private TMP_Text _text;

        /// <summary>Clave actual.</summary>
        public string Key => _key;

        /// <summary>Cambia la clave y los argumentos y refresca el texto.</summary>
        public void Set(string key, params object[] args)
        {
            _key = key;
            _args = args ?? Array.Empty<object>();
            Refresh();
        }

        /// <summary>Cambia solo los argumentos.</summary>
        public void SetArgs(params object[] args)
        {
            _args = args ?? Array.Empty<object>();
            Refresh();
        }

        private void OnEnable()
        {
            Localization.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Localization.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }

            if (Localization.Service == null || string.IsNullOrEmpty(_key))
            {
                return;
            }

            _text.text = Localization.Get(_key, _args);
        }
    }
}
