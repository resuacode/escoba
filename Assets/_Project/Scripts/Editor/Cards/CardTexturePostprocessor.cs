using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Cards
{
    /// <summary>
    /// Aplica automáticamente los ajustes de importación a las imágenes de cartas
    /// (<c>Assets/_Project/Art/Cards/</c>). Las carpetas <c>Kenney</c> y <c>Spanish</c> (la baraja
    /// española propia, en pixel art) usan el perfil pixel art: filtro Point y sin compresión.
    /// Cualquier otra carpeta usa el perfil de arte ilustrado.
    /// Ver docs/CARD_ART_REPLACEMENT.md.
    /// </summary>
    public sealed class CardTexturePostprocessor : AssetPostprocessor
    {
        private const string CardsRoot = "Assets/_Project/Art/Cards/";
        private static readonly string[] PixelArtRoots = { "Assets/_Project/Art/Cards/Kenney/", "Assets/_Project/Art/Cards/Spanish/" };
        private const int IllustratedMaxSize = 1024;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(CardsRoot, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            bool isPixelArt = System.Array.Exists(PixelArtRoots, root => assetPath.StartsWith(root, System.StringComparison.Ordinal));

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
