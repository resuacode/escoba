using System;
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
    /// Los empates y conceptos sin punto se muestran con "–".
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

        private Action _onNext;

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
            var values = new StringBuilder[used];
            for (int c = 0; c < used; c++)
            {
                values[c] = new StringBuilder();
            }

            void Row(string label, Func<SideRoundScore, string> value)
            {
                labels.AppendLine(label);
                for (int c = 0; c < used; c++)
                {
                    values[c].AppendLine(value(score.Sides[columnSides[c]]));
                }
            }

            Row(Localization.Get("summary.escobas"), s => Points(s.EscobaPoints));
            Row(Localization.Get("summary.cards"), s => $"{Points(s.MostCardsPoints)} <size=65%>({s.CardCount})</size>");
            Row(Localization.Get("summary.golds"), s => $"{Points(s.MostGoldsPoints)} <size=65%>({s.GoldCount})</size>");
            Row(Localization.Get("summary.gold_seven"), s => Points(s.GoldSevenPoints));
            Row(Localization.Get("summary.setenta"), s => Points(s.SetentaPoints));
            if (rules.AllSevensBonus > 0)
            {
                Row(Localization.Get("summary.bonus_sevens"), s => Points(s.AllSevensBonusPoints));
            }

            if (rules.AllGoldsBonus > 0)
            {
                Row(Localization.Get("summary.bonus_golds"), s => Points(s.AllGoldsBonusPoints));
            }

            labels.AppendLine();
            for (int c = 0; c < used; c++)
            {
                values[c].AppendLine();
            }

            Row($"<b>{Localization.Get("summary.round_points")}</b>", s => $"<b>{s.Total}</b>");
            labels.Append($"<b>{Localization.Get("summary.total")}</b>");
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
        }

        /// <summary>Oculta el panel.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>Pulsa "Siguiente ronda" (también usado en pruebas).</summary>
        public void Continue()
        {
            Action callback = _onNext;
            _onNext = null;
            Hide();
            callback?.Invoke();
        }

        private static string Points(int points) => points > 0 ? points.ToString() : NoPoint;

        private void Awake()
        {
            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(Continue);
            }
        }
    }
}
