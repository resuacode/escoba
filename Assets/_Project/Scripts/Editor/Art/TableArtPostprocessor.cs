using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Art
{
    /// <summary>
    /// Ajustes de importación automáticos del fondo de mesa (<c>Assets/_Project/Art/Table/</c>):
    /// sprite pixel art (filtro Point, sin mipmaps ni compresión), igual que las cartas.
    /// Ver docs/ART_GUIDE.md.
    /// </summary>
    public sealed class TableArtPostprocessor : AssetPostprocessor
    {
        private const string TableRoot = "Assets/_Project/Art/Table/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TableRoot, System.StringComparison.Ordinal))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
