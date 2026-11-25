using System.Collections.Generic;
using Escoba.Presentation;
using UnityEngine;

namespace Escoba.App
{
    /// <summary>
    /// Configuración global del juego: baraja activa, presets de reglas, idiomas y ritmo.
    /// Un único asset en <c>Assets/_Project/Data/GameSettings.asset</c>, referenciado por el
    /// <see cref="SceneBootstrap"/> de cada escena.
    /// </summary>
    [CreateAssetMenu(menuName = "Escoba/Game Settings", fileName = "GameSettings")]
    public sealed class GameSettings : ScriptableObject
    {
        [SerializeField] private CardSkin _cardSkin;
        [SerializeField] private RulesPreset[] _presets = new RulesPreset[0];
        [SerializeField, Min(0)] private int _defaultPresetIndex;
        [SerializeField] private TextAsset[] _languages = new TextAsset[0];
        [SerializeField] private AudioLibrary _audio;
        [Tooltip("Fondo de mesa a pantalla completa (opcional; sin él se usa el color de tapete).")]
        [SerializeField] private Sprite _tableBackground;

        [Header("Ritmo")]
        [SerializeField, Min(0f)] private float _botThinkSecondsMin = 0.6f;
        [SerializeField, Min(0f)] private float _botThinkSecondsMax = 1.1f;
        [SerializeField, Min(0.1f)] private float _animationSpeed = 1f;

        /// <summary>Baraja activa.</summary>
        public CardSkin CardSkin => _cardSkin;

        /// <summary>Presets de reglas disponibles.</summary>
        public IReadOnlyList<RulesPreset> Presets => _presets;

        /// <summary>Índice del preset por defecto.</summary>
        public int DefaultPresetIndex => Mathf.Clamp(_defaultPresetIndex, 0, Mathf.Max(0, _presets.Length - 1));

        /// <summary>Ficheros JSON de idioma.</summary>
        public IReadOnlyList<TextAsset> Languages => _languages;

        /// <summary>Efectos de sonido y música.</summary>
        public AudioLibrary Audio => _audio;

        /// <summary>Fondo de mesa (puede ser null).</summary>
        public Sprite TableBackground => _tableBackground;

        /// <summary>Tiempo mínimo que "piensa" la IA.</summary>
        public float BotThinkSecondsMin => _botThinkSecondsMin;

        /// <summary>Tiempo máximo que "piensa" la IA.</summary>
        public float BotThinkSecondsMax => Mathf.Max(_botThinkSecondsMin, _botThinkSecondsMax);

        /// <summary>Velocidad de las animaciones (1 = normal).</summary>
        public float AnimationSpeed => _animationSpeed;

#if UNITY_EDITOR
        /// <summary>Solo editor: rellena los ajustes (usado al generar los assets por defecto).</summary>
        public void EditorConfigure(CardSkin skin, RulesPreset[] presets, TextAsset[] languages, AudioLibrary audio, Sprite tableBackground)
        {
            _tableBackground = tableBackground;
            _cardSkin = skin;
            _presets = presets;
            _languages = languages;
            _audio = audio;
            _defaultPresetIndex = 0;
        }
#endif
    }
}
