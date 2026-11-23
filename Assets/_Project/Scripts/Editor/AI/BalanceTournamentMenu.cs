using System;
using System.Text;
using Escoba.AI;
using Escoba.Core;
using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.AI
{
    /// <summary>
    /// Menú <c>Escoba/AI/Run Balance Tournament</c>: enfrenta los niveles de IA (1 contra 1,
    /// reglas por defecto) y escribe en consola una tabla Markdown lista para docs/GDD.md §9.
    /// </summary>
    public static class BalanceTournamentMenu
    {
        public const int GamesPerMatchup = 400;

        [MenuItem("Escoba/AI/Run Balance Tournament")]
        public static string Run()
        {
            var rules = new RulesConfig();
            var matchups = new (string name, Func<IBotStrategy> a, Func<IBotStrategy> b)[]
            {
                ("Medio vs Fácil", () => new MediumBot(), () => new EasyBot()),
                ("Difícil vs Medio", () => new HardBot(), () => new MediumBot()),
                ("Difícil vs Fácil", () => new HardBot(), () => new EasyBot()),
            };

            var table = new StringBuilder();
            table.AppendLine($"| Enfrentamiento ({GamesPerMatchup} partidas, a 21) | % victorias A | Puntos/ronda A–B | Escobas/ronda A–B | ms/decisión A (máx) |");
            table.AppendLine("|---|---|---|---|---|");
            for (int i = 0; i < matchups.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Torneo de IA", matchups[i].name, (float)i / matchups.Length);
                TournamentResult r = BotTournament.Run(rules, matchups[i].a, matchups[i].b, GamesPerMatchup, 10_000);
                table.AppendLine($"| {matchups[i].name} | {r.WinRateA:P1} | {r.PointsPerRoundA:F2} – {r.PointsPerRoundB:F2} | {r.EscobasPerRoundA:F2} – {r.EscobasPerRoundB:F2} | {r.AverageDecisionMsA:F2} ({r.MaxDecisionMs:F1}) |");
            }

            EditorUtility.ClearProgressBar();
            string report = table.ToString();
            Debug.Log("[Escoba] Torneo de balance de IA:\n" + report);
            return report;
        }
    }
}
