using Escoba.Core;
using UnityEngine;

namespace Escoba.App
{
    /// <summary>
    /// Conjunto de reglas predefinido (Clásica 21, Rápida 15, Larga 31...). Se edita en el
    /// Inspector y produce un <see cref="RulesConfig"/> para el motor.
    /// </summary>
    [CreateAssetMenu(menuName = "Escoba/Rules Preset", fileName = "RulesPreset")]
    public sealed class RulesPreset : ScriptableObject
    {
        [Tooltip("Clave de localización del nombre del preset.")]
        [SerializeField] private string _nameKey = "setup.preset.classic21";

        [SerializeField, Min(1)] private int _targetScore = 21;
        [SerializeField] private bool _dealerTableEscoba = true;

        [Tooltip("Fija en v1 (no se expone en la configuración de partida).")]
        [SerializeField] private bool _captureIsMandatory = true;

        [SerializeField] private bool _lastPlayEscobaCounts = true;
        [SerializeField] private SetentaTieRule _setentaTieRule = SetentaTieRule.NoPoint;
        [SerializeField, Min(0)] private int _allSevensBonus;
        [SerializeField, Min(0)] private int _allGoldsBonus;

        /// <summary>Clave de localización del nombre.</summary>
        public string NameKey => _nameKey;

        /// <summary>Puntos para ganar.</summary>
        public int TargetScore => _targetScore;

        /// <summary>Crea la configuración de reglas para el número de jugadores indicado.</summary>
        public RulesConfig ToRulesConfig(int playerCount, bool teams)
        {
            return new RulesConfig
            {
                PlayerCount = playerCount,
                Teams = teams,
                TargetScore = _targetScore,
                DealerTableEscoba = _dealerTableEscoba,
                CaptureIsMandatory = _captureIsMandatory,
                LastPlayEscobaCounts = _lastPlayEscobaCounts,
                SetentaTieRule = _setentaTieRule,
                AllSevensBonus = _allSevensBonus,
                AllGoldsBonus = _allGoldsBonus,
            };
        }

#if UNITY_EDITOR
        /// <summary>Solo editor: configura el preset (usado al generar los assets por defecto).</summary>
        public void EditorConfigure(string nameKey, int targetScore)
        {
            _nameKey = nameKey;
            _targetScore = targetScore;
        }
#endif
    }
}
