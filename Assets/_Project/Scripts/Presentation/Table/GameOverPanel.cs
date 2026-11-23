using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>Pantalla de fin de partida: ganador, puntuación final, revancha o menú.</summary>
    public sealed class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _scores;
        [SerializeField] private Button _rematchButton;
        [SerializeField] private Button _menuButton;

        private Action _onRematch;
        private Action _onMenu;

        /// <summary>Muestra el resultado. <paramref name="names"/> y <paramref name="totals"/> van en el mismo orden.</summary>
        public void Show(bool localPlayerWon, string winnerName, IReadOnlyList<string> names, IReadOnlyList<int> totals,
            Action onRematch, Action onMenu)
        {
            _onRematch = onRematch;
            _onMenu = onMenu;
            _title.text = localPlayerWon ? Localization.Get("gameover.you_win") : Localization.Get("gameover.winner", winnerName);

            var text = new StringBuilder();
            for (int i = 0; i < names.Count; i++)
            {
                text.AppendLine($"{names[i]}: <b>{totals[i]}</b>");
            }

            _scores.text = text.ToString();
            gameObject.SetActive(true);
        }

        /// <summary>Oculta el panel.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            if (_rematchButton != null)
            {
                _rematchButton.onClick.AddListener(() => Fire(ref _onRematch));
            }

            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(() => Fire(ref _onMenu));
            }
        }

        private void Fire(ref Action action)
        {
            Action callback = action;
            _onRematch = null;
            _onMenu = null;
            callback?.Invoke();
        }
    }
}
