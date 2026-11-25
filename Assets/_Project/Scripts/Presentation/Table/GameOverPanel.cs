using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Pantalla de fin de partida: ganador, puntuación final, revancha o menú. El título entra con
    /// rebote y, si hay que celebrar (gana un humano), se lanzan oleadas de confeti.
    /// </summary>
    public sealed class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _scores;
        [SerializeField] private Button _rematchButton;
        [SerializeField] private Button _menuButton;

        [Header("Animación")]
        [SerializeField] private UiParticles _effects;
        [SerializeField, Min(0)] private int _confettiWaves = 3;
        [SerializeField] private float _waveInterval = 0.55f;

        private Action _onRematch;
        private Action _onMenu;

        /// <summary>Muestra el resultado. <paramref name="names"/> y <paramref name="totals"/> van en el mismo orden.</summary>
        public void Show(string title, IReadOnlyList<string> names, IReadOnlyList<int> totals, Action onRematch, Action onMenu, bool celebrate = false)
        {
            _onRematch = onRematch;
            _onMenu = onMenu;
            _title.text = title;

            var text = new StringBuilder();
            for (int i = 0; i < names.Count; i++)
            {
                text.AppendLine($"{names[i]}: <b>{totals[i]}</b>");
            }

            _scores.text = text.ToString();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            StartCoroutine(Animate(celebrate));
        }

        private IEnumerator Animate(bool celebrate)
        {
            Transform title = _title.transform;
            const float pop = 0.45f;
            for (float time = 0f; time < pop; time += Time.deltaTime)
            {
                title.localScale = Vector3.one * Mathf.LerpUnclamped(0.3f, 1f, HudView.EaseOutBack(time / pop));
                yield return null;
            }

            title.localScale = Vector3.one;
            if (!celebrate || _effects == null)
            {
                yield break;
            }

            _effects.BurstAt(_title.rectTransform, ParticleStyle.Sparkle, 40);
            Vector2 half = _effects.Size / 2f;
            for (int wave = 0; wave < _confettiWaves; wave++)
            {
                _effects.Burst(new Vector2(-half.x + 120f, -half.y), ParticleStyle.Confetti, 45, 68f);
                _effects.Burst(new Vector2(half.x - 120f, -half.y), ParticleStyle.Confetti, 45, 112f);
                yield return new WaitForSeconds(_waveInterval);
            }
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
