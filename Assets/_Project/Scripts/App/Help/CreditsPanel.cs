using System;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Panel "Créditos": autoría y licencias del arte, el sonido y la música (las pistas CC-BY
    /// exigen mostrar este crédito en el juego). El texto es la clave <c>credits.body</c>.
    /// </summary>
    public sealed class CreditsPanel : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;

        private Action _onClosed;
        private bool _wired;

        /// <summary>Indica si el panel está abierto.</summary>
        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Abre el panel.</summary>
        public void Open(Action onClosed = null)
        {
            if (!_wired)
            {
                _wired = true;
                _closeButton.onClick.AddListener(Close);
            }

            _onClosed = onClosed;
            gameObject.SetActive(true);
        }

        /// <summary>Cierra el panel.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            Action callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }

        /// <summary>Oculta el panel sin avisar (estado inicial).</summary>
        public void Hide()
        {
            _onClosed = null;
            gameObject.SetActive(false);
        }
    }
}
