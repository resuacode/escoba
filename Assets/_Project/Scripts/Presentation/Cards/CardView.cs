using System;
using System.Collections;
using Escoba.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Carta en pantalla (uGUI). Muestra cara o dorso a través de un <see cref="CardSkin"/>,
    /// se resalta, se anima entre posiciones y notifica toques y arrastres. No conoce las reglas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class CardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _highlight;
        [SerializeField] private Image _shade;
        [SerializeField] private Color _candidateColor = new Color(0.35f, 0.85f, 1f, 0.9f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.85f, 0.2f, 1f);

        private CardSkin _skin;
        private RectTransform _rect;
        private Coroutine _move;
        private Canvas _canvas;

        /// <summary>Toque o clic sobre la carta.</summary>
        public event Action<CardView, PointerEventData> Clicked;

        /// <summary>Empieza un arrastre.</summary>
        public event Action<CardView> DragStarted;

        /// <summary>Termina un arrastre.</summary>
        public event Action<CardView, PointerEventData> DragEnded;

        /// <summary>RectTransform de la carta.</summary>
        public RectTransform Rect => _rect != null ? _rect : (_rect = (RectTransform)transform);

        /// <summary>Carta mostrada (null para dorsos anónimos, p. ej. la mano del rival).</summary>
        public Card? Card { get; private set; }

        /// <summary>Si muestra la cara.</summary>
        public bool FaceUp { get; private set; }

        /// <summary>Permite arrastrar la carta.</summary>
        public bool DragEnabled { get; set; }

        /// <summary>Indica si está en movimiento animado.</summary>
        public bool IsMoving => _move != null;

        /// <summary>Configura la carta.</summary>
        public void Bind(CardSkin skin, Card? card, bool faceUp, float height)
        {
            _skin = skin;
            Card = card;
            Vector2 native = skin.NativeSize;
            Rect.sizeDelta = new Vector2(height * native.x / native.y, height);
            SetFaceUp(faceUp);
            SetHighlight(CardHighlight.None);
            SetDimmed(false);
            DragEnabled = false;
        }

        /// <summary>Cambia el tamaño manteniendo la proporción de la baraja.</summary>
        public void SetHeight(float height)
        {
            Vector2 native = _skin != null ? _skin.NativeSize : new Vector2(2f, 3f);
            Rect.sizeDelta = new Vector2(height * native.x / native.y, height);
        }

        /// <summary>Muestra cara o dorso.</summary>
        public void SetFaceUp(bool faceUp)
        {
            FaceUp = faceUp && Card.HasValue;
            if (_skin == null)
            {
                return;
            }

            _image.sprite = FaceUp ? _skin.GetFace(Card.Value) : _skin.Back;
        }

        /// <summary>Resalta la carta.</summary>
        public void SetHighlight(CardHighlight highlight)
        {
            if (_highlight == null)
            {
                return;
            }

            _highlight.enabled = highlight != CardHighlight.None;
            _highlight.color = highlight == CardHighlight.Selected ? _selectedColor : _candidateColor;
        }

        /// <summary>Oscurece la carta (no seleccionable).</summary>
        public void SetDimmed(bool dimmed)
        {
            if (_shade != null)
            {
                _shade.enabled = dimmed;
            }
        }

        /// <summary>Coloca la carta sin animación.</summary>
        public void SnapTo(Vector2 position)
        {
            StopMove();
            Rect.anchoredPosition = position;
        }

        /// <summary>Mueve la carta con animación (ease-out) tras un retraso opcional.</summary>
        public void MoveTo(Vector2 position, float duration, float delay = 0f)
        {
            StopMove();
            if (!isActiveAndEnabled || (duration <= 0f && delay <= 0f))
            {
                Rect.anchoredPosition = position;
                return;
            }

            _move = StartCoroutine(MoveRoutine(position, duration, delay));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }

            Clicked?.Invoke(this, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!DragEnabled)
            {
                return;
            }

            StopMove();
            transform.SetAsLastSibling();
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!DragEnabled)
            {
                return;
            }

            if (_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }

            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            Rect.anchoredPosition += eventData.delta / scale;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!DragEnabled)
            {
                return;
            }

            DragEnded?.Invoke(this, eventData);
        }

        private void OnDisable()
        {
            _move = null;
        }

        private void StopMove()
        {
            if (_move != null)
            {
                StopCoroutine(_move);
                _move = null;
            }
        }

        private IEnumerator MoveRoutine(Vector2 target, float duration, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            Vector2 start = Rect.anchoredPosition;
            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                Rect.anchoredPosition = Vector2.LerpUnclamped(start, target, eased);
                yield return null;
            }

            Rect.anchoredPosition = target;
            _move = null;
        }
    }
}
