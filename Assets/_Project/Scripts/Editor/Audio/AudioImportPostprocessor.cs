using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Audio
{
    /// <summary>
    /// Ajustes de importación automáticos del audio de <c>Assets/_Project/Audio/</c>:
    /// <list type="bullet">
    /// <item>Música (<c>Music/</c>): Vorbis calidad 0,4 comprimido en memoria y carga en segundo
    /// plano, para no alargar la carga ni inflar la build web (~3 MB por pista de 3 min).</item>
    /// <item>Efectos (<c>Sfx/</c>): mono y descomprimidos al cargar (cortos y con latencia mínima).</item>
    /// </list>
    /// </summary>
    public sealed class AudioImportPostprocessor : AssetPostprocessor
    {
        private const string MusicRoot = "Assets/_Project/Audio/Music/";
        private const string SfxRoot = "Assets/_Project/Audio/Sfx/";
        private const float MusicQuality = 0.4f;
        private const float SfxQuality = 0.7f;

        private void OnPreprocessAudio()
        {
            bool isMusic = assetPath.StartsWith(MusicRoot, System.StringComparison.Ordinal);
            bool isSfx = assetPath.StartsWith(SfxRoot, System.StringComparison.Ordinal);
            if (!isMusic && !isSfx)
            {
                return;
            }

            var importer = (AudioImporter)assetImporter;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;

            if (isMusic)
            {
                importer.forceToMono = false;
                importer.loadInBackground = true;
                settings.loadType = AudioClipLoadType.CompressedInMemory;
                settings.quality = MusicQuality;
                settings.preloadAudioData = false;
            }
            else
            {
                importer.forceToMono = true;
                importer.loadInBackground = false;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.quality = SfxQuality;
                settings.preloadAudioData = true;
            }

            importer.defaultSampleSettings = settings;
        }
    }
}
