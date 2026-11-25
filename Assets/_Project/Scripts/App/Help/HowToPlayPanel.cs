using System;
using System.Collections.Generic;
using Escoba.Core;
using Escoba.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Panel "Cómo se juega" (menú principal y pausa): páginas de <see cref="HowToPlayGuide"/> con
    /// texto y ejemplos dibujados con la baraja activa (<see cref="CardSkin"/>), así que los
    /// ejemplos cambian solos al sustituir las cartas.
    /// </summary>
    public sealed class HowToPlayPanel : MonoBehaviour
    {
        [SerializeField] private LocalizedText _title;
        [SerializeField] private LocalizedText _body;
        [SerializeField] private LocalizedText _caption;
        [SerializeField] private LocalizedText _pageLabel;
        [SerializeField] private RectTransform _exampleArea;
        [SerializeField] private Image[] _cardSlots = new Image[6];
        [SerializeField] private RectTransform _playedLabel;
        [SerializeField] private RectTransform _tableLabel;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;

        [Header("Ejemplo (unidades del Canvas de referencia)")]
        [SerializeField] private float _cardHeight = 220f;
        [SerializeField] private float _cardGap = 18f;
        [SerializeField] private float _groupGap = 90f;

        private Action _onClosed;
        private bool _wired;

        /// <summary>Página visible (0 = primera).</summary>
        public int PageIndex { get; private set; }

        /// <summary>Número de páginas.</summary>
        public int PageCount => HowToPlayGuide.Pages.Count;

        /// <summary>Indica si el panel está abierto.</summary>
        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Abre el panel en la primera página.</summary>
        public void Open(Action onClosed = null)
        {
            Wire();
            _onClosed = onClosed;
            gameObject.SetActive(true);
            ShowPage(0);
        }

        /// <summary>Cierra el panel.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            Action callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }

        /// <summary>Oculta el panel sin avisar (estado inicial).</summary>
        public void Hide()
        {
            _onClosed = null;
            gameObject.SetActive(false);
        }

        /// <summary>Muestra una página (se limita al rango válido).</summary>
        public void ShowPage(int index)
        {
            IReadOnlyList<HowToPlayPage> pages = HowToPlayGuide.Pages;
            PageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
            HowToPlayPage page = pages[PageIndex];

            _title.Set(page.TitleKey);
            _body.Set(page.BodyKey);
            _caption.gameObject.SetActive(page.HasExample);
            if (page.HasExample)
            {
                _caption.Set(page.CaptionKey);
            }

            _pageLabel.Set("howto.page", PageIndex + 1, pages.Count);
            _previousButton.interactable = PageIndex > 0;
            _nextButton.interactable = PageIndex < pages.Count - 1;
            LayoutExample(page);
        }

        /// <summary>Página siguiente.</summary>
        public void Next() => ShowPage(PageIndex + 1);

        /// <summary>Página anterior.</summary>
        public void Previous() => ShowPage(PageIndex - 1);

        /// <summary>Sprites mostrados en el ejemplo de la página actual (pruebas).</summary>
        public IEnumerable<Sprite> VisibleCardSprites()
        {
            foreach (Image slot in _cardSlots)
            {
                if (slot.gameObject.activeSelf)
                {
                    yield return slot.sprite;
                }
            }
        }

        private void Wire()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            _previousButton.onClick.AddListener(Previous);
            _nextButton.onClick.AddListener(Next);
            _closeButton.onClick.AddListener(Close);
        }

        private void LayoutExample(HowToPlayPage page)
        {
            CardSkin skin = AppContext.Settings != null ? AppContext.Settings.CardSkin : null;
            Vector2 native = skin != null ? skin.NativeSize : new Vector2(2f, 3f);
            float cardWidth = _cardHeight * native.x / Mathf.Max(1f, native.y);
            var size = new Vector2(cardWidth, _cardHeight);

            int played = page.Played.Count;
            int table = page.Table.Count;
            int total = Mathf.Min(played + table, _cardSlots.Length);
            float GroupWidth(int count) => count <= 0 ? 0f : (count * cardWidth) + ((count - 1) * _cardGap);
            float width = GroupWidth(played) + GroupWidth(table) + (played > 0 && table > 0 ? _groupGap : 0f);
            float x = -width / 2f;

            for (int i = 0; i < _cardSlots.Length; i++)
            {
                Image slot = _cardSlots[i];
                bool visible = i < total;
                slot.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                if (i == played && played > 0)
                {
                    x += _groupGap - _cardGap;
                }

                Card card = i < played ? page.Played[i] : page.Table[i - played];
                slot.sprite = skin != null ? skin.GetFace(card) : null;
                slot.preserveAspect = true;
                slot.rectTransform.sizeDelta = size;
                slot.rectTransform.anchoredPosition = new Vector2(x + (cardWidth / 2f), 0f);
                x += cardWidth + _cardGap;
            }

            bool labels = page.IsPlay;
            _playedLabel.gameObject.SetActive(labels);
            _tableLabel.gameObject.SetActive(labels && table > 0);
            if (labels)
            {
                float labelY = (_cardHeight / 2f) + 34f;
                _playedLabel.anchoredPosition = new Vector2((-width / 2f) + (GroupWidth(played) / 2f), labelY);
                _tableLabel.anchoredPosition = new Vector2((width / 2f) - (GroupWidth(table) / 2f), labelY);
            }
        }
    }
}
