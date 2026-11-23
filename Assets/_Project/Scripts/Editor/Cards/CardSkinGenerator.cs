using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Escoba.App;
using Escoba.Core;
using Escoba.Editor.Build;
using Escoba.Presentation;
using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Cards
{
    /// <summary>
    /// Crea o actualiza un <see cref="CardSkin"/> a partir de una carpeta con los 41 PNG en la
    /// convención española (<c>oros_01.png</c> … <c>bastos_12.png</c>, <c>dorso.png</c>).
    /// Ver docs/CARD_ART_REPLACEMENT.md.
    /// </summary>
    public static class CardSkinGenerator
    {
        public const string SkinsFolder = "Assets/_Project/Data/CardSkins";
        public const string BackFileName = "dorso.png";

        private static readonly string[] SuitFileNames = { "oros", "copas", "espadas", "bastos" };

        /// <summary>Nombre de fichero esperado para una carta.</summary>
        public static string FileNameFor(Card card) => $"{SuitFileNames[(int)card.Suit]}_{card.Rank:00}.png";

        [MenuItem("Escoba/Cards/Generate Card Skin...")]
        private static void GenerateFromDialog()
        {
            string absolute = EditorUtility.OpenFolderPanel("Carpeta con las 41 imágenes de la baraja", "Assets/_Project/Art/Cards", string.Empty);
            if (string.IsNullOrEmpty(absolute))
            {
                return;
            }

            string folder = ToAssetPath(absolute);
            if (folder == null)
            {
                EditorUtility.DisplayDialog("Card Skin", "La carpeta debe estar dentro de Assets/.", "OK");
                return;
            }

            try
            {
                string name = Path.GetFileName(folder);
                bool pixelArt = folder.Contains("/Kenney");
                CardSkin skin = Generate(folder, name, pixelArt, string.Empty, string.Empty);
                Selection.activeObject = skin;
                EditorUtility.DisplayDialog("Card Skin", $"Generado {AssetDatabase.GetAssetPath(skin)}.", "OK");
            }
            catch (FileNotFoundException e)
            {
                EditorUtility.DisplayDialog("Card Skin", e.Message, "OK");
            }
        }

        [MenuItem("Escoba/Cards/Validate Active Card Skin")]
        private static void ValidateActiveSkin()
        {
            GameSettings settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            CardSkin skin = settings != null ? settings.CardSkin : null;
            string report = skin == null ? "No hay CardSkin activo en GameSettings." : Validate(skin);
            Debug.Log($"[CardSkin] {report}");
            EditorUtility.DisplayDialog("Card Skin", report, "OK");
        }

        /// <summary>
        /// Genera el CardSkin. Lanza <see cref="FileNotFoundException"/> listando las imágenes que faltan.
        /// </summary>
        public static CardSkin Generate(string folder, string name, bool pixelArt, string author, string license)
        {
            var faces = new Sprite[Card.DeckSize];
            var missing = new List<string>();
            foreach (Card card in Deck.CreateOrdered())
            {
                string file = FileNameFor(card);
                faces[card.Id] = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{file}");
                if (faces[card.Id] == null)
                {
                    missing.Add(file);
                }
            }

            Sprite back = AssetDatabase.LoadAssetAtPath<Sprite>($"{folder}/{BackFileName}");
            if (back == null)
            {
                missing.Add(BackFileName);
            }

            if (missing.Count > 0)
            {
                throw new FileNotFoundException($"Faltan {missing.Count} imágenes en {folder}: {string.Join(", ", missing)}");
            }

            Directory.CreateDirectory(SkinsFolder);
            string path = $"{SkinsFolder}/CardSkin_{name}.asset";
            CardSkin skin = AssetDatabase.LoadAssetAtPath<CardSkin>(path);
            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<CardSkin>();
                AssetDatabase.CreateAsset(skin, path);
            }

            skin.EditorConfigure(name, author, license, pixelArt, back, faces);
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();
            return skin;
        }

        /// <summary>Comprueba que el CardSkin está completo y con tamaños uniformes.</summary>
        public static string Validate(CardSkin skin)
        {
            IReadOnlyList<int> missing = skin.FindMissingFaces();
            if (skin.Back == null)
            {
                return "Falta el dorso.";
            }

            if (missing.Count > 0)
            {
                return $"Faltan {missing.Count} caras: {string.Join(", ", missing.Select(id => FileNameFor(Card.FromId(id))))}";
            }

            Vector2 size = skin.Back.rect.size;
            var different = Deck.CreateOrdered().Where(c => skin.GetFace(c).rect.size != size).ToList();
            if (different.Count > 0)
            {
                return $"Tamaños distintos al dorso ({size.x}×{size.y}): {string.Join(", ", different.Select(FileNameFor))}";
            }

            return $"OK: 40/40 caras, dorso, tamaño uniforme {size.x}×{size.y} ({skin.DisplayName}).";
        }

        private static string ToAssetPath(string absolute)
        {
            string data = Application.dataPath.Replace('\\', '/');
            absolute = absolute.Replace('\\', '/');
            return absolute.StartsWith(data, StringComparison.OrdinalIgnoreCase) ? "Assets" + absolute.Substring(data.Length) : null;
        }
    }
}
