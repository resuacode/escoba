using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Información de la mesa alrededor de las cartas: nombres, marcador, turno, mazo, bazas,
    /// indicaciones, suma, botones de jugada y el aviso grande ("¡Escoba!"). Solo muestra datos.
    /// Índice 0 = lado inferior (el jugador local), 1 = lado superior (rival).
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [Header("Jugadores (0 = abajo, 1 = arriba)")]
        [SerializeField] private TMP_Text[] _names = new TMP_Text[2];
        [SerializeField] private TMP_Text[] _scores = new TMP_Text[2];
        [SerializeField] private TMP_Text[] _pileCounts = new TMP_Text[2];
        [SerializeField] private TMP_Text[] _escobas = new TMP_Text[2];
        [SerializeField] private Image[] _pileBacks = new Image[2];
        [SerializeField] private GameObject[] _dealerBadges = new GameObject[2];

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

        /// <summary>Configura los dorsos de mazo y bazas.</summary>
        public void SetBackSprite(Sprite back)
        {
            if (_deckBack != null)
            {
                _deckBack.sprite = back;
                _deckBack.preserveAspect = true;
            }

            foreach (Image pile in _pileBacks)
            {
                if (pile != null)
                {
                    pile.sprite = back;
                    pile.preserveAspect = true;
                }
            }
        }

        public void SetName(int position, string text) => SetText(_names, position, text);

        public void SetScore(int position, int score) => SetText(_scores, position, score.ToString());

        public void SetEscobas(int position, int escobas) => SetText(_escobas, position, Localization.Get("game.escobas", escobas));

        public void SetPileCount(int position, int count)
        {
            SetText(_pileCounts, position, Localization.GetPlural("game.cards", count));
            if (position < _pileBacks.Length && _pileBacks[position] != null)
            {
                _pileBacks[position].enabled = count > 0;
            }
        }

        public void SetDealer(int position)
        {
            for (int i = 0; i < _dealerBadges.Length; i++)
            {
                if (_dealerBadges[i] != null)
                {
                    _dealerBadges[i].SetActive(i == position);
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

        private static void SetText(TMP_Text[] texts, int index, string value)
        {
            if (texts != null && index >= 0 && index < texts.Length && texts[index] != null)
            {
                texts[index].text = value;
            }
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
