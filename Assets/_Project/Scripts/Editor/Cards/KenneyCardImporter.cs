using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Escoba.Editor.Cards
{
    /// <summary>
    /// Convierte el "Playing Cards Pack" de Kenney (póker, pixel art 64×64) en una baraja
    /// española temporal: selecciona las 40 cartas útiles + dorso, recorta la carta (42×60 px)
    /// y las guarda con la convención de nombres del proyecto (<c>oros_01.png</c> … <c>dorso.png</c>).
    /// </summary>
    public static class KenneyCardImporter
    {
        public const string DefaultTargetFolder = "Assets/_Project/Art/Cards/Kenney";

        /// <summary>Zona de la carta dentro del lienzo de 64×64 (origen abajo-izquierda, como Texture2D).</summary>
        private static readonly RectInt CardRect = new RectInt(11, 2, 42, 60);

        // Palo español -> palo de Kenney.
        private static readonly Dictionary<string, string> SuitMap = new Dictionary<string, string>
        {
            { "oros", "diamonds" },
            { "copas", "hearts" },
            { "espadas", "spades" },
            { "bastos", "clubs" },
        };

        // Número de carta española -> rango de Kenney.
        private static readonly Dictionary<int, string> RankMap = new Dictionary<int, string>
        {
            { 1, "A" }, { 2, "02" }, { 3, "03" }, { 4, "04" }, { 5, "05" }, { 6, "06" }, { 7, "07" },
            { 10, "J" }, { 11, "Q" }, { 12, "K" },
        };

        [MenuItem("Escoba/Cards/Import Kenney Pack...")]
        private static void ImportFromDialog()
        {
            string source = EditorUtility.OpenFolderPanel(
                "Carpeta 'PNG/Cards (large)' del pack de Kenney", Application.dataPath, string.Empty);
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            int count = Import(source, DefaultTargetFolder);
            EditorUtility.DisplayDialog("Kenney", $"Importadas {count} imágenes en {DefaultTargetFolder}.", "OK");
        }

        /// <summary>
        /// Recorta y renombra las cartas. Devuelve el número de ficheros escritos (41 si todo va bien).
        /// Lanza <see cref="FileNotFoundException"/> si falta alguna carta en el origen.
        /// </summary>
        public static int Import(string sourceFolder, string targetFolder)
        {
            Directory.CreateDirectory(targetFolder);
            int written = 0;

            foreach (KeyValuePair<string, string> suit in SuitMap)
            {
                foreach (KeyValuePair<int, string> rank in RankMap)
                {
                    string sourceFile = Path.Combine(sourceFolder, $"card_{suit.Value}_{rank.Value}.png");
                    string targetFile = Path.Combine(targetFolder, $"{suit.Key}_{rank.Key:00}.png");
                    CropAndSave(sourceFile, targetFile);
                    written++;
                }
            }

            CropAndSave(Path.Combine(sourceFolder, "card_back.png"), Path.Combine(targetFolder, "dorso.png"));
            written++;

            AssetDatabase.Refresh();
            return written;
        }

        private static void CropAndSave(string sourceFile, string targetFile)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("Falta la carta de Kenney", sourceFile);
            }

            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var cropped = new Texture2D(CardRect.width, CardRect.height, TextureFormat.RGBA32, false);
            try
            {
                source.LoadImage(File.ReadAllBytes(sourceFile));
                Color[] pixels = source.GetPixels(CardRect.x, CardRect.y, CardRect.width, CardRect.height);
                cropped.SetPixels(pixels);
                cropped.Apply();
                File.WriteAllBytes(targetFile, cropped.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(cropped);
            }
        }
    }
}
