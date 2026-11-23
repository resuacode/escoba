using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Información general de la mesa: marcador (hasta 4 bandos), objetivo, ronda, turno, mazo,
    /// indicaciones, suma, botones de jugada y aviso grande ("¡Escoba!"). Solo muestra datos.
    /// La información de cada asiento (nombre, baza, escobas) está en <see cref="SeatPanelView"/>.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [Header("Marcador (una fila por bando)")]
        [SerializeField] private GameObject[] _scoreRows = new GameObject[4];
        [SerializeField] private TMP_Text[] _scoreNames = new TMP_Text[4];
        [SerializeField] private TMP_Text[] _scoreValues = new TMP_Text[4];

        [Header("Mesa")]
        [SerializeField] private TMP_Text _target;
        [SerializeField] private TMP_Text _round;
        [SerializeField] private TMP_Text _turn;
        [SerializeField] private TMP_Text _deckCount;
        [SerializeField] private Image _deckBack;

        [Header("Jugada")]
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private TMP_Text _sum;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _menuButton;

        [Header("Aviso")]
        [SerializeField] private CanvasGroup _bannerGroup;
        [SerializeField] private TMP_Text _banner;

        /// <summary>Botón de confirmar jugada.</summary>
        public Button PlayButton => _playButton;

        /// <summary>Botón de deshacer selección.</summary>
        public Button UndoButton => _undoButton;

        /// <summary>Botón de menú.</summary>
        public Button MenuButton => _menuButton;

        /// <summary>Dorso del mazo.</summary>
        public void SetBackSprite(Sprite back)
        {
            if (_deckBack != null)
            {
                _deckBack.sprite = back;
                _deckBack.preserveAspect = true;
            }
        }

        /// <summary>Muestra las filas del marcador (nombre y puntos por bando).</summary>
        public void SetScores(System.Collections.Generic.IReadOnlyList<string> names, System.Collections.Generic.IReadOnlyList<int> scores)
        {
            for (int i = 0; i < _scoreRows.Length; i++)
            {
                bool visible = i < names.Count;
                if (_scoreRows[i] != null)
                {
                    _scoreRows[i].SetActive(visible);
                }

                if (!visible)
                {
                    continue;
                }

                if (_scoreNames[i] != null)
                {
                    _scoreNames[i].text = names[i];
                }

                if (_scoreValues[i] != null)
                {
                    _scoreValues[i].text = i < scores.Count ? scores[i].ToString() : "0";
                }
            }
        }

        public void SetDeckCount(int count)
        {
            if (_deckCount != null)
            {
                _deckCount.text = count.ToString();
            }

            if (_deckBack != null)
            {
                _deckBack.enabled = count > 0;
            }
        }

        public void SetTarget(int target) => SetSingle(_target, Localization.Get("game.target", target));

        public void SetRound(int round) => SetSingle(_round, Localization.Get("game.round", round));

        public void SetTurn(string text) => SetSingle(_turn, text);

        /// <summary>Muestra u oculta la indicación al jugador.</summary>
        public void SetHint(string text)
        {
            if (_hint == null)
            {
                return;
            }

            _hint.text = text ?? string.Empty;
            _hint.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        /// <summary>Muestra u oculta la suma de la selección.</summary>
        public void SetSum(string text)
        {
            if (_sum == null)
            {
                return;
            }

            _sum.text = text ?? string.Empty;
            _sum.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        /// <summary>Muestra u oculta los botones de jugada.</summary>
        public void SetPlayControls(bool visible, bool canPlay, bool canUndo)
        {
            if (_playButton != null)
            {
                _playButton.gameObject.SetActive(visible);
                _playButton.interactable = canPlay;
            }

            if (_undoButton != null)
            {
                _undoButton.gameObject.SetActive(visible);
                _undoButton.interactable = canUndo;
            }
        }

        /// <summary>Muestra un aviso grande durante un tiempo (con fundido).</summary>
        public IEnumerator ShowBanner(string text, float duration)
        {
            if (_bannerGroup == null || _banner == null || duration <= 0f)
            {
                yield break;
            }

            _banner.text = text;
            _bannerGroup.gameObject.SetActive(true);
            const float fade = 0.15f;
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Min(1f, Mathf.Min(time / fade, (duration - time) / fade));
                _bannerGroup.alpha = Mathf.Clamp01(alpha);
                yield return null;
            }

            _bannerGroup.alpha = 0f;
            _bannerGroup.gameObject.SetActive(false);
        }

        private void Awake()
        {
            if (_bannerGroup != null)
            {
                _bannerGroup.alpha = 0f;
                _bannerGroup.gameObject.SetActive(false);
            }

            SetHint(null);
            SetSum(null);
            SetPlayControls(false, false, false);
        }

        private static void SetSingle(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
