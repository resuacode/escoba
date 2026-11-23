using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Pantalla de pase (hot-seat): tapa la mesa entera y pide pasar el dispositivo al siguiente
    /// jugador humano, recordándole la última jugada. Sus cartas solo se muestran tras confirmar.
    /// </summary>
    public sealed class PassScreenPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _lastPlay;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private LocalizedText _confirmLabel;

        private Action _onConfirm;

        /// <summary>Indica si está visible.</summary>
        public bool IsShown => gameObject.activeSelf;

        /// <summary>Muestra la pantalla para <paramref name="playerName"/>.</summary>
        public void Show(string playerName, string lastPlay, Action onConfirm)
        {
            _onConfirm = onConfirm;
            _title.text = Localization.Get("pass.title", playerName);
            _lastPlay.text = lastPlay ?? string.Empty;
            _confirmLabel.Set("pass.confirm", playerName);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        /// <summary>Confirma (equivale a pulsar el botón).</summary>
        public void Confirm()
        {
            Action callback = _onConfirm;
            _onConfirm = null;
            gameObject.SetActive(false);
            callback?.Invoke();
        }

        /// <summary>Oculta sin confirmar.</summary>
        public void Hide()
        {
            _onConfirm = null;
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(Confirm);
            }
        }
    }
}
