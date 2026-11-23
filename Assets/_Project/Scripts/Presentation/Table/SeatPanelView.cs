using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.Presentation
{
    /// <summary>
    /// Ficha de un asiento junto a su mano: nombre, baza (dorso + recuento), escobas de su bando,
    /// indicador de repartidor y resalte de turno. La baza es también el destino de las capturas.
    /// </summary>
    public sealed class SeatPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _pileBack;
        [SerializeField] private TMP_Text _pileCount;
        [SerializeField] private TMP_Text _escobas;
        [SerializeField] private GameObject _dealerBadge;
        [SerializeField] private Image _turnHighlight;

        /// <summary>Destino de las cartas capturadas.</summary>
        public RectTransform PileAnchor => _pileBack != null ? _pileBack.rectTransform : (RectTransform)transform;

        public void SetBackSprite(Sprite back)
        {
            if (_pileBack != null)
            {
                _pileBack.sprite = back;
                _pileBack.preserveAspect = true;
            }
        }

        public void SetName(string playerName)
        {
            if (_name != null)
            {
                _name.text = playerName;
            }
        }

        public void SetPile(int cards, int escobas)
        {
            if (_pileCount != null)
            {
                _pileCount.text = Localization.GetPlural("game.cards", cards);
            }

            if (_escobas != null)
            {
                _escobas.text = Localization.Get("game.escobas", escobas);
            }

            if (_pileBack != null)
            {
                _pileBack.enabled = cards > 0;
            }
        }

        public void SetDealer(bool isDealer)
        {
            if (_dealerBadge != null)
            {
                _dealerBadge.SetActive(isDealer);
            }
        }

        public void SetActiveTurn(bool active)
        {
            if (_turnHighlight != null)
            {
                _turnHighlight.enabled = active;
            }
        }
    }
}
