using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Partículas para uGUI (los <c>ParticleSystem</c> no se ven en un Canvas Screen Space Overlay):
    /// un conjunto reutilizable de <see cref="Image"/> que se animan por código. Va en una capa a
    /// pantalla completa por encima de la mesa; usa tiempo de juego, así que se congela en pausa.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UiParticles : MonoBehaviour
    {
        private static readonly Color[] SparkleColors =
        {
            new Color(1f, 0.86f, 0.35f, 1f), new Color(1f, 0.97f, 0.8f, 1f), new Color(0.95f, 0.7f, 0.2f, 1f),
        };

        private static readonly Color[] ConfettiColors =
        {
            new Color(0.95f, 0.78f, 0.25f, 1f), new Color(0.86f, 0.22f, 0.26f, 1f), new Color(0.98f, 0.96f, 0.9f, 1f),
            new Color(0.3f, 0.75f, 0.7f, 1f), new Color(0.98f, 0.55f, 0.2f, 1f), new Color(0.45f, 0.6f, 0.95f, 1f),
        };

        [Tooltip("Sprite redondo de los destellos (el confeti usa rectángulos sin sprite).")]
        [SerializeField] private Sprite _sparkleSprite;
        [SerializeField, Min(16)] private int _maxParticles = 240;

        private readonly List<Particle> _active = new List<Particle>();
        private readonly Stack<Particle> _pool = new Stack<Particle>();

        /// <summary>Partículas vivas (pruebas).</summary>
        public int ActiveCount => _active.Count;

        /// <summary>Ráfaga en un punto de la capa (coordenadas locales, origen en el centro).</summary>
        /// <param name="direction">Dirección media en grados (90 = hacia arriba); solo para confeti.</param>
        public void Burst(Vector2 position, ParticleStyle style, int count, float direction = 90f)
        {
            transform.SetAsLastSibling();
            for (int i = 0; i < count && _active.Count < _maxParticles; i++)
            {
                Particle p = Take();
                p.Age = 0f;
                p.Position = position;
                if (style == ParticleStyle.Sparkle)
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    p.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(250f, 950f);
                    p.Gravity = 0f;
                    p.Drag = 3.2f;
                    p.Life = Random.Range(0.45f, 0.9f);
                    p.Spin = 0f;
                    p.Shrink = true;
                    float size = Random.Range(10f, 28f);
                    p.Rect.sizeDelta = new Vector2(size, size);
                    p.Image.sprite = _sparkleSprite;
                    p.Image.color = SparkleColors[Random.Range(0, SparkleColors.Length)];
                }
                else
                {
                    float angle = (direction + Random.Range(-35f, 35f)) * Mathf.Deg2Rad;
                    p.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(800f, 1500f);
                    p.Gravity = 1500f;
                    p.Drag = 0.9f;
                    p.Life = Random.Range(1.3f, 2.2f);
                    p.Spin = Random.Range(-720f, 720f);
                    p.Shrink = false;
                    p.Rect.sizeDelta = new Vector2(Random.Range(12f, 22f), Random.Range(7f, 13f));
                    p.Image.sprite = null;
                    p.Image.color = ConfettiColors[Random.Range(0, ConfettiColors.Length)];
                }

                p.BaseColor = p.Image.color;
                p.Rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                p.Rect.localScale = Vector3.one;
                p.Rect.anchoredPosition = position;
                p.Rect.gameObject.SetActive(true);
                _active.Add(p);
            }
        }

        /// <summary>Ráfaga sobre un elemento de la interfaz.</summary>
        public void BurstAt(RectTransform target, ParticleStyle style, int count)
        {
            if (target == null)
            {
                return;
            }

            Vector3 local = ((RectTransform)transform).InverseTransformPoint(target.position);
            Burst(new Vector2(local.x, local.y), style, count);
        }

        /// <summary>Tamaño de la capa (para colocar ráfagas en los bordes).</summary>
        public Vector2 Size => ((RectTransform)transform).rect.size;

        /// <summary>Elimina todas las partículas.</summary>
        public void Clear()
        {
            foreach (Particle p in _active)
            {
                p.Rect.gameObject.SetActive(false);
                _pool.Push(p);
            }

            _active.Clear();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Particle p = _active[i];
                p.Age += dt;
                if (p.Age >= p.Life)
                {
                    p.Rect.gameObject.SetActive(false);
                    _active.RemoveAt(i);
                    _pool.Push(p);
                    continue;
                }

                p.Velocity.y -= p.Gravity * dt;
                p.Velocity *= Mathf.Max(0f, 1f - (p.Drag * dt));
                p.Position += p.Velocity * dt;
                p.Rect.anchoredPosition = p.Position;
                if (p.Spin != 0f)
                {
                    p.Rect.localRotation *= Quaternion.Euler(0f, 0f, p.Spin * dt);
                }

                float t = p.Age / p.Life;
                Color color = p.BaseColor;
                color.a = 1f - (t * t);
                p.Image.color = color;
                if (p.Shrink)
                {
                    p.Rect.localScale = Vector3.one * (1f - (0.7f * t));
                }
            }
        }

        private Particle Take()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            var go = new GameObject("Particle", typeof(RectTransform));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);
            var image = go.AddComponent<Image>();
            image.raycastTarget = false;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            return new Particle { Rect = rect, Image = image };
        }

        private sealed class Particle
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Position;
            public Vector2 Velocity;
            public Color BaseColor;
            public float Age;
            public float Life;
            public float Gravity;
            public float Drag;
            public float Spin;
            public bool Shrink;
        }
    }
}
