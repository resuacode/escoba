using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Escoba.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Resumen de ronda: una fila por concepto y una columna por bando (hasta 4).
    /// Los empates y conceptos sin punto se muestran con "–". Entra con un fundido y las filas
    /// aparecen de una en una (con sonido si dan puntos); "Siguiente ronda" durante la
    /// animación la completa al instante.
    /// </summary>
    public sealed class RoundSummaryPanel : MonoBehaviour
    {
        private const string NoPoint = "–";

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _labels;
        [SerializeField] private TMP_Text[] _headers = new TMP_Text[4];
        [SerializeField] private TMP_Text[] _columns = new TMP_Text[4];
        [SerializeField] private Button _nextButton;

        [Tooltip("Separación horizontal entre columnas; se centran según cuántas haya.")]
        [SerializeField] private float _columnSpacing = 190f;

        [SerializeField] private float _columnsCenterX = 285f;

        [Header("Animación")]
        [Tooltip("Panel interior (crece al entrar). Opcional.")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private float _enterDuration = 0.25f;
        [SerializeField] private float _rowDelay = 0.22f;

        private readonly List<bool> _lineHasPoints = new List<bool>();
        private Action _onNext;
        private Coroutine _reveal;

        /// <summary>Multiplicador de velocidad de la animación (el de la mesa).</summary>
        public float Speed { get; set; } = 1f;

        /// <summary>Indica si las filas se están revelando todavía.</summary>
        public bool IsRevealing => _reveal != null;

        /// <summary>
        /// Muestra el resumen. <paramref name="columnSides"/> indica qué bando va en cada columna
        /// y <paramref name="columnNames"/> su nombre.
        /// </summary>
        public void Show(int round, RoundScore score, IReadOnlyList<int> totals, IReadOnlyList<int> columnSides,
            IReadOnlyList<string> columnNames, RulesConfig rules, Action onNext)
        {
            _onNext = onNext;
            _title.text = Localization.Get("summary.title", round);
            int used = Mathf.Min(columnSides.Count, _columns.Length);

            var labels = new StringBuilder();
            _lineHasPoints.Clear();
            var values = new StringBuilder[used];
            for (int c = 0; c < used; c++)
            {
                values[c] = new StringBuilder();
            }

            void Row(string label, Func<SideRoundScore, string> value, Func<SideRoundScore, int> points)
            {
                labels.AppendLine(label);
                bool any = false;
                for (int c = 0; c < used; c++)
                {
                    SideRoundScore side = score.Sides[columnSides[c]];
                    values[c].AppendLine(value(side));
                    any |= points(side) > 0;
                }

                _lineHasPoints.Add(any);
            }

            Row(Localization.Get("summary.escobas"), s => Points(s.EscobaPoints), s => s.EscobaPoints);
            Row(Localization.Get("summary.cards"), s => $"{Points(s.MostCardsPoints)} <size=65%>({s.CardCount})</size>", s => s.MostCardsPoints);
            Row(Localization.Get("summary.golds"), s => $"{Points(s.MostGoldsPoints)} <size=65%>({s.GoldCount})</size>", s => s.MostGoldsPoints);
            Row(Localization.Get("summary.gold_seven"), s => Points(s.GoldSevenPoints), s => s.GoldSevenPoints);
            Row(Localization.Get("summary.setenta"), s => Points(s.SetentaPoints), s => s.SetentaPoints);
            if (rules.AllSevensBonus > 0)
            {
                Row(Localization.Get("summary.bonus_sevens"), s => Points(s.AllSevensBonusPoints), s => s.AllSevensBonusPoints);
            }

            if (rules.AllGoldsBonus > 0)
            {
                Row(Localization.Get("summary.bonus_golds"), s => Points(s.AllGoldsBonusPoints), s => s.AllGoldsBonusPoints);
            }

            labels.AppendLine();
            _lineHasPoints.Add(false);
            for (int c = 0; c < used; c++)
            {
                values[c].AppendLine();
            }

            Row($"<b>{Localization.Get("summary.round_points")}</b>", s => $"<b>{s.Total}</b>", s => s.Total);
            labels.Append($"<b>{Localization.Get("summary.total")}</b>");
            _lineHasPoints.Add(true);
            for (int c = 0; c < used; c++)
            {
                values[c].Append($"<b>{totals[columnSides[c]]}</b>");
            }

            _labels.text = labels.ToString();
            float firstX = _columnsCenterX - ((used - 1) * _columnSpacing / 2f);
            for (int c = 0; c < _columns.Length; c++)
            {
                bool visible = c < used;
                _columns[c].gameObject.SetActive(visible);
                _headers[c].gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                _columns[c].text = values[c].ToString();
                _headers[c].text = c < columnNames.Count ? columnNames[c] : string.Empty;
                float x = firstX + (c * _columnSpacing);
                _columns[c].rectTransform.anchoredPosition = new Vector2(x, _columns[c].rectTransform.anchoredPosition.y);
                _headers[c].rectTransform.anchoredPosition = new Vector2(x, _headers[c].rectTransform.anchoredPosition.y);
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StopReveal();
            _reveal = StartCoroutine(Reveal());
        }

        /// <summary>Oculta el panel.</summary>
        public void Hide()
        {
            StopReveal();
            gameObject.SetActive(false);
        }

        /// <summary>Muestra todas las filas sin esperar a la animación.</summary>
        public void FinishReveal()
        {
            StopReveal();
            SetVisibleLines(int.MaxValue);
            if (_group != null)
            {
                _group.alpha = 1f;
            }

            if (_panel != null)
            {
                _panel.localScale = Vector3.one;
            }
        }

        /// <summary>Pulsa "Siguiente ronda" (también usado en pruebas).</summary>
        public void Continue()
        {
            StopReveal();
            Action callback = _onNext;
            _onNext = null;
            Hide();
            callback?.Invoke();
        }

        private static string Points(int points) => points > 0 ? points.ToString() : NoPoint;

        private IEnumerator Reveal()
        {
            float speed = Mathf.Max(0.01f, Speed);
            SetVisibleLines(0);
            float enter = _enterDuration / speed;
            for (float time = 0f; time < enter; time += Time.deltaTime)
            {
                float t = time / enter;
                if (_group != null)
                {
                    _group.alpha = t;
                }

                if (_panel != null)
                {
                    _panel.localScale = Vector3.one * Mathf.LerpUnclamped(0.9f, 1f, HudView.EaseOutBack(t));
                }

                yield return null;
            }

            if (_group != null)
            {
                _group.alpha = 1f;
            }

            if (_panel != null)
            {
                _panel.localScale = Vector3.one;
            }

            for (int line = 0; line < _lineHasPoints.Count; line++)
            {
                SetVisibleLines(line + 1);
                if (_lineHasPoints[line])
                {
                    Sounds.Play(SoundCue.ScoreTick);
                }

                yield return new WaitForSeconds(_rowDelay / speed);
            }

            SetVisibleLines(int.MaxValue);
            _reveal = null;
        }

        private void SetVisibleLines(int lines)
        {
            _labels.maxVisibleLines = lines;
            foreach (TMP_Text column in _columns)
            {
                column.maxVisibleLines = lines;
            }
        }

        private void StopReveal()
        {
            if (_reveal != null)
            {
                StopCoroutine(_reveal);
                _reveal = null;
            }
        }

        private void OnNextClicked()
        {
            // Primer toque durante la animación: mostrar todo; segundo: continuar.
            if (IsRevealing)
            {
                FinishReveal();
                return;
            }

            Continue();
        }

        private void Awake()
        {
            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(OnNextClicked);
            }
        }
    }
}
