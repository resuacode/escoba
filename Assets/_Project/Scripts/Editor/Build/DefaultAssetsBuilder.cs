using System.Collections.Generic;
using System.IO;
using System.Linq;
using Escoba.App;
using Escoba.Editor.Cards;
using Escoba.Presentation;
using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Build
{
    /// <summary>
    /// Crea (o actualiza) los assets por defecto: presets de reglas, CardSkin de Kenney y GameSettings.
    /// Idempotente: se puede ejecutar varias veces.
    /// </summary>
    public static class DefaultAssetsBuilder
    {
        public const string DataFolder = "Assets/_Project/Data";
        public const string RulesFolder = "Assets/_Project/Data/Rules";
        public const string GameSettingsPath = "Assets/_Project/Data/GameSettings.asset";
        public const string LocalizationFolder = "Assets/_Project/Localization";
        public const string KenneyFolder = "Assets/_Project/Art/Cards/Kenney";

        public const string AudioLibraryPath = "Assets/_Project/Data/AudioLibrary.asset";
        public const string SfxFolder = "Assets/_Project/Audio/Sfx";
        public const string MusicFolder = "Assets/_Project/Audio/Music";

        private static readonly string[] LanguageFiles = { "es.json", "gl.json", "en.json" };

        // Efecto → prefijo de los clips (pack "Casino Audio" de Kenney, CC0) y volumen base.
        private static readonly (SoundCue Cue, string Prefix, float Volume)[] SfxMapping =
        {
            (SoundCue.Click, "chip-lay-", 0.45f),
            (SoundCue.Shuffle, "card-shuffle", 0.8f),
            (SoundCue.Deal, "card-slide-", 0.55f),
            (SoundCue.PlayCard, "card-place-", 0.9f),
            (SoundCue.Capture, "card-shove-", 0.8f),
            (SoundCue.Escoba, "chips-stack-", 1f),
            (SoundCue.RevealHand, "card-fan-", 0.7f),
            (SoundCue.RoundEnd, "chips-handle-", 0.8f),
            (SoundCue.GameOver, "chips-collide-", 1f),
            (SoundCue.GoldSeven, "chips-collide-", 0.75f),
            (SoundCue.ScoreTick, "chip-lay-", 0.6f),
        };

        [MenuItem("Escoba/Build/Create Default Assets")]
        public static GameSettings CreateAll()
        {
            Directory.CreateDirectory(RulesFolder);
            RulesPreset[] presets =
            {
                Preset("RulesPreset_Classic21", "setup.preset.classic21", 21),
                Preset("RulesPreset_Quick15", "setup.preset.quick15", 15),
                Preset("RulesPreset_Long31", "setup.preset.long31", 31),
            };

            CardSkin skin = CardSkinGenerator.Generate(KenneyFolder, "Kenney", true, "Kenney (www.kenney.nl)", "CC0 1.0");

            var languages = new TextAsset[LanguageFiles.Length];
            for (int i = 0; i < LanguageFiles.Length; i++)
            {
                languages[i] = AssetDatabase.LoadAssetAtPath<TextAsset>($"{LocalizationFolder}/{LanguageFiles[i]}");
                if (languages[i] == null)
                {
                    Debug.LogError($"[Escoba] Falta el fichero de idioma {LanguageFiles[i]}.");
                }
            }

            AudioLibrary audio = CreateAudioLibrary();

            GameSettings settings = LoadOrCreate<GameSettings>(GameSettingsPath);
            settings.EditorConfigure(skin, presets, languages, audio);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Escoba] Assets por defecto creados/actualizados.");
            return settings;
        }

        /// <summary>
        /// Rellena <c>AudioLibrary.asset</c>: cada efecto toma los clips de <see cref="SfxFolder"/> cuyo
        /// nombre empieza por su prefijo. Música del menú: el primer clip de <c>Music/Menu</c>;
        /// músicas de partida: todos los clips de <c>Music/Game</c>.
        /// </summary>
        private static AudioLibrary CreateAudioLibrary()
        {
            var cues = new List<AudioLibrary.CueClips>();
            foreach ((SoundCue cue, string prefix, float volume) in SfxMapping)
            {
                AudioClip[] clips = FindClips(SfxFolder)
                    .Where(clip => clip.name.StartsWith(prefix, System.StringComparison.Ordinal))
                    .OrderBy(clip => clip.name, System.StringComparer.Ordinal)
                    .ToArray();
                if (clips.Length == 0)
                {
                    Debug.LogWarning($"[Escoba] No hay clips '{prefix}*' para el efecto {cue} en {SfxFolder}.");
                }

                cues.Add(new AudioLibrary.CueClips(cue, clips, volume));
            }

            AudioClip menuMusic = FindClips(MusicFolder + "/Menu").OrderBy(clip => clip.name, System.StringComparer.Ordinal).FirstOrDefault();
            AudioClip[] gameMusic = FindClips(MusicFolder + "/Game").OrderBy(clip => clip.name, System.StringComparer.Ordinal).ToArray();
            AudioLibrary library = LoadOrCreate<AudioLibrary>(AudioLibraryPath);
            library.EditorConfigure(cues, menuMusic, gameMusic);
            EditorUtility.SetDirty(library);
            return library;
        }

        private static IEnumerable<AudioClip> FindClips(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                return Enumerable.Empty<AudioClip>();
            }

            return AssetDatabase.FindAssets("t:AudioClip", new[] { folder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(clip => clip != null);
        }

        private static RulesPreset Preset(string fileName, string nameKey, int target)
        {
            RulesPreset preset = LoadOrCreate<RulesPreset>($"{RulesFolder}/{fileName}.asset");
            preset.EditorConfigure(nameKey, target);
            EditorUtility.SetDirty(preset);
            return preset;
        }

        private static T LoadOrCreate<T>(string path)
            where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }

            return asset;
        }
    }
}
