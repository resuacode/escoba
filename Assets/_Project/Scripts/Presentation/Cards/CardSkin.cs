using System.Collections.Generic;
using Escoba.Core;
using UnityEngine;

namespace Escoba.Presentation
{
    /// <summary>
    /// Baraja visual: un sprite por carta (indexado por <see cref="Card.Id"/>) y el dorso.
    /// Las vistas nunca referencian sprites de cartas directamente: siempre pasan por aquí.
    /// Se genera con el menú <c>Escoba/Cards/Generate Card Skin...</c>. Ver docs/CARD_ART_REPLACEMENT.md.
    /// </summary>
    [CreateAssetMenu(menuName = "Escoba/Card Skin", fileName = "CardSkin")]
    public sealed class CardSkin : ScriptableObject
    {
        [SerializeField] private string _displayName = "Baraja";
        [SerializeField] private string _author = string.Empty;
        [SerializeField] private string _license = string.Empty;

        [Tooltip("Arte pixelado: se escala por múltiplos enteros para que no se vea borroso.")]
        [SerializeField] private bool _pixelArt;

        [SerializeField] private Sprite _back;
        [SerializeField] private Sprite[] _faces = new Sprite[Card.DeckSize];

        /// <summary>Nombre visible de la baraja.</summary>
        public string DisplayName => _displayName;

        /// <summary>Autor del arte.</summary>
        public string Author => _author;

        /// <summary>Licencia del arte.</summary>
        public string License => _license;

        /// <summary>Indica si el arte es pixel art.</summary>
        public bool PixelArt => _pixelArt;

        /// <summary>Sprite del dorso.</summary>
        public Sprite Back => _back;

        /// <summary>Tamaño nativo en píxeles de una carta (tomado del dorso).</summary>
        public Vector2 NativeSize => _back != null ? _back.rect.size : new Vector2(200f, 300f);

        /// <summary>Sprite de la cara de una carta (null si falta).</summary>
        public Sprite GetFace(Card card)
        {
            return _faces != null && card.Id < _faces.Length ? _faces[card.Id] : null;
        }

        /// <summary>Ids de las cartas sin sprite.</summary>
        public IReadOnlyList<int> FindMissingFaces()
        {
            var missing = new List<int>();
            for (int id = 0; id < Card.DeckSize; id++)
            {
                if (_faces == null || id >= _faces.Length || _faces[id] == null)
                {
                    missing.Add(id);
                }
            }

            return missing;
        }

#if UNITY_EDITOR
        /// <summary>Solo editor: rellena la baraja (usado por el generador de CardSkin).</summary>
        public void EditorConfigure(string displayName, string author, string license, bool pixelArt, Sprite back, Sprite[] faces)
        {
            _displayName = displayName;
            _author = author;
            _license = license;
            _pixelArt = pixelArt;
            _back = back;
            _faces = faces;
        }
#endif
    }
}
