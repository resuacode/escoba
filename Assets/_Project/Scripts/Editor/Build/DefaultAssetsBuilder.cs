using System.IO;
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

        private static readonly string[] LanguageFiles = { "es.json", "gl.json", "en.json" };

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

            GameSettings settings = LoadOrCreate<GameSettings>(GameSettingsPath);
            settings.EditorConfigure(skin, presets, languages);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Escoba] Assets por defecto creados/actualizados.");
            return settings;
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
