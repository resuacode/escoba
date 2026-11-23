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
    /// Resumen de ronda: una fila por concepto y una columna por bando (máximo dos en M2).
    /// Los empates se muestran con "–".
    /// </summary>
    public sealed class RoundSummaryPanel : MonoBehaviour
    {
        private const string NoPoint = "–";

        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _labels;
        [SerializeField] private TMP_Text[] _headers = new TMP_Text[2];
        [SerializeField] private TMP_Text[] _columns = new TMP_Text[2];
        [SerializeField] private Button _nextButton;

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

            var labels = new StringBuilder();
            var values = new StringBuilder[_columns.Length];
            for (int c = 0; c < values.Length; c++)
            {
                values[c] = new StringBuilder();
            }

            void Row(string label, Func<SideRoundScore, string> value)
            {
                labels.AppendLine(label);
                for (int c = 0; c < _columns.Length; c++)
                {
                    values[c].AppendLine(c < columnSides.Count ? value(score.Sides[columnSides[c]]) : string.Empty);
                }
            }

            Row(Localization.Get("summary.escobas"), s => Points(s.EscobaPoints));
            Row(Localization.Get("summary.cards"), s => $"{Points(s.MostCardsPoints)}  <size=70%>({s.CardCount})</size>");
            Row(Localization.Get("summary.golds"), s => $"{Points(s.MostGoldsPoints)}  <size=70%>({s.GoldCount})</size>");
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
            for (int c = 0; c < values.Length; c++)
            {
                values[c].AppendLine();
            }

            Row($"<b>{Localization.Get("summary.round_points")}</b>", s => $"<b>{s.Total}</b>");
            labels.Append($"<b>{Localization.Get("summary.total")}</b>");
            for (int c = 0; c < _columns.Length; c++)
            {
                values[c].Append(c < columnSides.Count ? $"<b>{totals[columnSides[c]]}</b>" : string.Empty);
            }

            _labels.text = labels.ToString();
            for (int c = 0; c < _columns.Length; c++)
            {
                _columns[c].text = values[c].ToString();
                if (c < _headers.Length && _headers[c] != null)
                {
                    _headers[c].text = c < columnNames.Count ? columnNames[c] : string.Empty;
                }
            }

            gameObject.SetActive(true);
        }

        /// <summary>Oculta el panel.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static string Points(int points) => points > 0 ? points.ToString() : NoPoint;

        private void Awake()
        {
            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(OnNextClicked);
            }
        }

        private void OnNextClicked()
        {
            Action callback = _onNext;
            _onNext = null;
            Hide();
            callback?.Invoke();
        }
    }
}
