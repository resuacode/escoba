using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Cards
{
    /// <summary>
    /// Aplica automáticamente los ajustes de importación a las imágenes de cartas
    /// (<c>Assets/_Project/Art/Cards/</c>). La carpeta <c>Kenney</c> usa el perfil pixel art;
    /// el resto (p. ej. la baraja española propia) usa el perfil de arte ilustrado.
    /// Ver docs/CARD_ART_REPLACEMENT.md.
    /// </summary>
    public sealed class CardTexturePostprocessor : AssetPostprocessor
    {
        private const string CardsRoot = "Assets/_Project/Art/Cards/";
        private const string PixelArtRoot = "Assets/_Project/Art/Cards/Kenney/";
        private const int IllustratedMaxSize = 1024;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(CardsRoot, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            bool isPixelArt = assetPath.StartsWith(PixelArtRoot, System.StringComparison.Ordinal);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;

            if (isPixelArt)
            {
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
            }
            else
            {
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.maxTextureSize = IllustratedMaxSize;
            }
        }
    }
}
