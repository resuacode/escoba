using System;
using Escoba.App;
using Escoba.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Escoba.Editor.Build.UiFactory;

namespace Escoba.Editor.Build
{
    /// <summary>
    /// Construye las escenas Boot, MainMenu y Game y el prefab de carta de forma reproducible
    /// (menú <c>Escoba/Build/...</c>). Sobrescribe las escenas: los ajustes visuales que deban
    /// conservarse se hacen aquí, no a mano en la escena.
    /// </summary>
    public static class SceneBuilder
    {
        public const string ScenesFolder = "Assets/_Project/Scenes";
        public const string CardPrefabPath = "Assets/_Project/Prefabs/CardView.prefab";

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color FeltColor = new Color(0.05f, 0.27f, 0.15f, 1f);
        private static readonly Color TableAreaColor = new Color(0.08f, 0.35f, 0.2f, 1f);

        [MenuItem("Escoba/Build/Rebuild All (assets + scenes)")]
        public static void RebuildAll()
        {
            GameSettings settings = DefaultAssetsBuilder.CreateAll();
            CardView cardPrefab = BuildCardPrefab();
            BuildBoot(settings);
            BuildMainMenu(settings);
            BuildGame(settings, cardPrefab);
            EditorSceneManager.OpenScene($"{ScenesFolder}/{SceneNames.Boot}.unity");
            Debug.Log("[Escoba] Escenas reconstruidas.");
        }

        [MenuItem("Escoba/Build/Rebuild Game Scene")]
        public static void RebuildGame()
        {
            GameSettings settings = DefaultAssetsBuilder.CreateAll();
            BuildGame(settings, BuildCardPrefab());
        }

        [MenuItem("Escoba/Build/Rebuild Main Menu Scene")]
        public static void RebuildMainMenu()
        {
            BuildMainMenu(DefaultAssetsBuilder.CreateAll());
        }

        // ---------------------------------------------------------------- Prefab de carta

        public static CardView BuildCardPrefab()
        {
            var root = new GameObject("CardView", typeof(RectTransform));
            root.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(126f, 180f);

            Image highlight = CreateImage(root.transform, "Highlight", Gold, RoundedSprite);
            Stretch(highlight.rectTransform, -8f);
            highlight.enabled = false;

            Image face = CreateImage(root.transform, "Face", Color.white, null, true);
            Stretch(face.rectTransform);

            Image shade = CreateImage(root.transform, "Shade", new Color(0f, 0f, 0f, 0.45f));
            Stretch(shade.rectTransform);
            shade.enabled = false;

            var view = root.AddComponent<CardView>();
            SetField(view, "_image", face);
            SetField(view, "_highlight", highlight);
            SetField(view, "_shade", shade);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab.GetComponent<CardView>();
        }

        // ---------------------------------------------------------------- Escenas

        private static void BuildBoot(GameSettings settings)
        {
            Scene scene = NewScene();
            // Crear una escena nueva descarga los assets no usados: recargar referencias por ruta.
            settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            CreateCommon(settings, false);
            new GameObject("BootLoader").AddComponent<BootLoader>();
            Save(scene, SceneNames.Boot);
        }

        private static void BuildMainMenu(GameSettings settings)
        {
            Scene scene = NewScene();
            // Crear una escena nueva descarga los assets no usados: recargar referencias por ruta.
            settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            RectTransform safe = CreateCommon(settings, true);

            TextMeshProUGUI title = CreateText(safe, "Title", "app.title", 150f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(1200f, 200f));
            title.fontStyle = FontStyles.Bold;

            // Menú principal
            RectTransform main = CreateRect("MainPanel", safe);
            Place(main, new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(720f, 700f));
            VerticalLayout(main, 22f);

            Button playAi = CreateButton(main, "PlayAiButton", "menu.play_ai", new Vector2(680f, 96f));
            Button playLocal = ComingSoonButton(main, "PlayLocalButton", "menu.play_local");
            Button playOnline = ComingSoonButton(main, "PlayOnlineButton", "menu.play_online");
            Button howTo = ComingSoonButton(main, "HowToPlayButton", "menu.how_to_play");
            Button language = CreateButton(main, "LanguageButton", null, new Vector2(680f, 96f));
            Button quit = CreateButton(main, "QuitButton", "menu.quit", new Vector2(680f, 96f));

            // Nueva partida
            RectTransform setup = CreateRect("SetupPanel", safe);
            Place(setup, new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(1300f, 700f));
            Image setupBackground = setup.gameObject.AddComponent<Image>();
            setupBackground.sprite = RoundedSprite;
            setupBackground.type = UnityEngine.UI.Image.Type.Sliced;
            setupBackground.color = PanelColor;

            TextMeshProUGUI setupTitle = CreateText(setup, "Title", "setup.title", 64f, TextAlignmentOptions.Center, Gold);
            Place(setupTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1000f, 90f));

            TextMeshProUGUI rulesLabel = CreateText(setup, "RulesLabel", "setup.rules", 40f, TextAlignmentOptions.Center, TextLight);
            Place(rulesLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -165f), new Vector2(1000f, 60f));
            Button[] presets = ButtonRow(setup, "Preset", new[] { "setup.preset.classic21", "setup.preset.quick15", "setup.preset.long31" }, -250f);

            TextMeshProUGUI difficultyLabel = CreateText(setup, "DifficultyLabel", "setup.difficulty", 40f, TextAlignmentOptions.Center, TextLight);
            Place(difficultyLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -355f), new Vector2(1000f, 60f));
            Button[] difficulties = ButtonRow(setup, "Difficulty", new[] { "setup.difficulty.easy", "setup.difficulty.medium", "setup.difficulty.hard" }, -440f);

            Button back = CreateButton(setup, "BackButton", "common.back", new Vector2(320f, 96f));
            Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(-220f, 90f), new Vector2(320f, 96f));
            Button start = CreateButton(setup, "StartButton", "setup.start", new Vector2(320f, 96f));
            Place((RectTransform)start.transform, new Vector2(0.5f, 0f), new Vector2(220f, 90f), new Vector2(320f, 96f));
            start.image.color = Gold;

            var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            SetField(controller, "_mainPanel", main.gameObject);
            SetField(controller, "_playAiButton", playAi);
            SetField(controller, "_comingSoonButtons", new UnityEngine.Object[] { playLocal, playOnline, howTo });
            SetField(controller, "_languageButton", language);
            SetField(controller, "_languageLabel", language.GetComponentInChildren<TMP_Text>());
            SetField(controller, "_quitButton", quit);
            SetField(controller, "_setupPanel", setup.gameObject);
            SetField(controller, "_presetButtons", presets);
            SetField(controller, "_difficultyButtons", difficulties);
            SetField(controller, "_startButton", start);
            SetField(controller, "_backButton", back);

            Save(scene, SceneNames.MainMenu);
        }

        private static void BuildGame(GameSettings settings, CardView cardPrefab)
        {
            Scene scene = NewScene();
            // Crear una escena nueva descarga los assets no usados: recargar referencias por ruta.
            settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath).GetComponent<CardView>();
            RectTransform safe = CreateCommon(settings, true);

            // Zona de mesa (también es la zona donde soltar cartas arrastradas).
            Image tableArea = CreateImage(safe, "TableArea", TableAreaColor, RoundedSprite);
            Place(tableArea.rectTransform, Center, new Vector2(-40f, 30f), new Vector2(1380f, 500f));

            RectTransform myHand = Anchor(safe, "MyHandAnchor", new Vector2(-40f, -400f));
            RectTransform opponentHand = Anchor(safe, "OpponentHandAnchor", new Vector2(-40f, 420f));
            RectTransform tableAnchor = Anchor(safe, "TableAnchor", new Vector2(-40f, 30f));
            RectTransform deckAnchor = Anchor(safe, "DeckAnchor", new Vector2(-830f, 30f));
            RectTransform myPile = Anchor(safe, "MyPileAnchor", new Vector2(820f, -170f));
            RectTransform opponentPile = Anchor(safe, "OpponentPileAnchor", new Vector2(820f, 330f));

            // Mazo y bazas (dorsos + recuentos)
            Image deckBack = CreateImage(safe, "DeckBack", Color.white);
            Place(deckBack.rectTransform, Center, new Vector2(-830f, 30f), new Vector2(126f, 180f));
            TextMeshProUGUI deckCount = CreateText(safe, "DeckCount", null, 34f, TextAlignmentOptions.Center, TextLight);
            Place(deckCount.rectTransform, Center, new Vector2(-830f, -90f), new Vector2(200f, 50f));
            TextMeshProUGUI deckLabel = CreateText(safe, "DeckLabel", "game.deck", 28f, TextAlignmentOptions.Center, TextLight);
            Place(deckLabel.rectTransform, Center, new Vector2(-830f, 150f), new Vector2(220f, 50f));

            Image myPileBack = PileBack(safe, "MyPileBack", new Vector2(820f, -170f));
            Image opponentPileBack = PileBack(safe, "OpponentPileBack", new Vector2(820f, 330f));
            TextMeshProUGUI myPileCount = CreateText(safe, "MyPileCount", null, 28f, TextAlignmentOptions.Center, TextLight);
            Place(myPileCount.rectTransform, Center, new Vector2(820f, -255f), new Vector2(240f, 40f));
            TextMeshProUGUI myEscobas = CreateText(safe, "MyEscobas", null, 28f, TextAlignmentOptions.Center, Gold);
            Place(myEscobas.rectTransform, Center, new Vector2(820f, -292f), new Vector2(240f, 40f));
            TextMeshProUGUI opponentPileCount = CreateText(safe, "OpponentPileCount", null, 28f, TextAlignmentOptions.Center, TextLight);
            Place(opponentPileCount.rectTransform, Center, new Vector2(820f, 245f), new Vector2(240f, 40f));
            TextMeshProUGUI opponentEscobas = CreateText(safe, "OpponentEscobas", null, 28f, TextAlignmentOptions.Center, Gold);
            Place(opponentEscobas.rectTransform, Center, new Vector2(820f, 208f), new Vector2(240f, 40f));

            // Capa de cartas (por encima de la mesa y por debajo del HUD)
            RectTransform cardLayer = CreateRect("CardLayer", safe);
            Stretch(cardLayer);

            // Marcador (arriba a la izquierda)
            Image board = CreateImage(safe, "Scoreboard", PanelColor, RoundedSprite);
            Place(board.rectTransform, new Vector2(0f, 1f), new Vector2(230f, -150f), new Vector2(420f, 260f));
            TextMeshProUGUI name0 = CreateText(board.transform, "Name0", null, 34f, TextAlignmentOptions.Left, TextLight);
            Place(name0.rectTransform, new Vector2(0f, 1f), new Vector2(200f, -40f), new Vector2(200f, 50f));
            TextMeshProUGUI score0 = CreateText(board.transform, "Score0", null, 40f, TextAlignmentOptions.Right, Gold);
            Place(score0.rectTransform, new Vector2(1f, 1f), new Vector2(-60f, -40f), new Vector2(100f, 50f));
            TextMeshProUGUI name1 = CreateText(board.transform, "Name1", null, 34f, TextAlignmentOptions.Left, TextLight);
            Place(name1.rectTransform, new Vector2(0f, 1f), new Vector2(200f, -95f), new Vector2(200f, 50f));
            TextMeshProUGUI score1 = CreateText(board.transform, "Score1", null, 40f, TextAlignmentOptions.Right, Gold);
            Place(score1.rectTransform, new Vector2(1f, 1f), new Vector2(-60f, -95f), new Vector2(100f, 50f));
            GameObject dealer0 = DealerBadge(board.transform, "DealerBadge0", -40f);
            GameObject dealer1 = DealerBadge(board.transform, "DealerBadge1", -95f);
            TextMeshProUGUI target = CreateText(board.transform, "Target", null, 26f, TextAlignmentOptions.Left, TextLight);
            Place(target.rectTransform, new Vector2(0f, 1f), new Vector2(200f, -150f), new Vector2(360f, 40f));
            TextMeshProUGUI round = CreateText(board.transform, "Round", null, 26f, TextAlignmentOptions.Left, TextLight);
            Place(round.rectTransform, new Vector2(0f, 1f), new Vector2(200f, -190f), new Vector2(360f, 40f));
            TextMeshProUGUI turn = CreateText(board.transform, "Turn", null, 28f, TextAlignmentOptions.Left, Gold);
            Place(turn.rectTransform, new Vector2(0f, 1f), new Vector2(200f, -230f), new Vector2(360f, 40f));

            // Indicaciones y botones de jugada
            TextMeshProUGUI hint = CreateText(safe, "Hint", null, 30f, TextAlignmentOptions.Center, TextLight);
            Place(hint.rectTransform, Center, new Vector2(-40f, -250f), new Vector2(1100f, 50f));
            TextMeshProUGUI sum = CreateText(safe, "Sum", null, 34f, TextAlignmentOptions.Center, Gold);
            Place(sum.rectTransform, Center, new Vector2(400f, -400f), new Vector2(240f, 60f));
            Button undo = CreateButton(safe, "UndoButton", "game.undo", new Vector2(220f, 84f), 36f);
            Place((RectTransform)undo.transform, Center, new Vector2(580f, -470f), new Vector2(220f, 84f));
            Button play = CreateButton(safe, "PlayButton", "game.play", new Vector2(220f, 84f), 36f);
            Place((RectTransform)play.transform, Center, new Vector2(820f, -470f), new Vector2(220f, 84f));
            play.image.color = Gold;
            Button menu = CreateButton(safe, "MenuButton", "game.menu", new Vector2(180f, 70f), 32f);
            Place((RectTransform)menu.transform, new Vector2(1f, 1f), new Vector2(-110f, -55f), new Vector2(180f, 70f));
            menu.gameObject.AddComponent<GameMenuButton>();

            // Aviso grande
            Image bannerBackground = CreateImage(safe, "Banner", new Color(0f, 0f, 0f, 0.6f), RoundedSprite);
            Place(bannerBackground.rectTransform, Center, new Vector2(-40f, 30f), new Vector2(1100f, 200f));
            CanvasGroup bannerGroup = bannerBackground.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.blocksRaycasts = false;
            TextMeshProUGUI bannerText = CreateText(bannerBackground.transform, "Text", null, 90f, TextAlignmentOptions.Center, Gold);
            Stretch(bannerText.rectTransform, 10f);
            bannerText.fontStyle = FontStyles.Bold;
            bannerText.enableAutoSizing = true;
            bannerText.fontSizeMin = 30f;
            bannerText.fontSizeMax = 90f;

            // HUD
            var hud = new GameObject("Hud").AddComponent<HudView>();
            SetField(hud, "_names", new UnityEngine.Object[] { name0, name1 });
            SetField(hud, "_scores", new UnityEngine.Object[] { score0, score1 });
            SetField(hud, "_pileCounts", new UnityEngine.Object[] { myPileCount, opponentPileCount });
            SetField(hud, "_escobas", new UnityEngine.Object[] { myEscobas, opponentEscobas });
            SetField(hud, "_pileBacks", new UnityEngine.Object[] { myPileBack, opponentPileBack });
            SetField(hud, "_dealerBadges", new UnityEngine.Object[] { dealer0, dealer1 });
            SetField(hud, "_target", target);
            SetField(hud, "_round", round);
            SetField(hud, "_turn", turn);
            SetField(hud, "_deckCount", deckCount);
            SetField(hud, "_deckBack", deckBack);
            SetField(hud, "_hint", hint);
            SetField(hud, "_sum", sum);
            SetField(hud, "_playButton", play);
            SetField(hud, "_undoButton", undo);
            SetField(hud, "_menuButton", menu);
            SetField(hud, "_bannerGroup", bannerGroup);
            SetField(hud, "_banner", bannerText);

            // Paneles
            RoundSummaryPanel summary = BuildRoundSummary(safe);
            GameOverPanel gameOver = BuildGameOver(safe);

            // Mesa y controlador
            var table = new GameObject("Table").AddComponent<TablePresenter>();
            SetField(table, "_cardPrefab", cardPrefab);
            SetField(table, "_cardLayer", cardLayer);
            SetField(table, "_myHandAnchor", myHand);
            SetField(table, "_opponentHandAnchor", opponentHand);
            SetField(table, "_tableAnchor", tableAnchor);
            SetField(table, "_tableDropZone", tableArea.rectTransform);
            SetField(table, "_deckAnchor", deckAnchor);
            SetField(table, "_pileAnchors", new UnityEngine.Object[] { myPile, opponentPile });
            SetField(table, "_hud", hud);

            var controller = new GameObject("GameController").AddComponent<GameController>();
            SetField(controller, "_table", table);
            SetField(controller, "_roundSummary", summary);
            SetField(controller, "_gameOver", gameOver);

            Save(scene, SceneNames.Game);
        }

        private static RoundSummaryPanel BuildRoundSummary(RectTransform parent)
        {
            Image overlay = CreateImage(parent, "RoundSummary", OverlayColor, null, true);
            Stretch(overlay.rectTransform);
            Image panel = CreateImage(overlay.transform, "Panel", PanelColor, RoundedSprite);
            Place(panel.rectTransform, Center, Vector2.zero, new Vector2(1000f, 820f));

            TextMeshProUGUI title = CreateText(panel.transform, "Title", null, 60f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(900f, 80f));
            TextMeshProUGUI header0 = CreateText(panel.transform, "Header0", null, 36f, TextAlignmentOptions.Center, Gold);
            Place(header0.rectTransform, new Vector2(0.5f, 1f), new Vector2(170f, -140f), new Vector2(200f, 50f));
            TextMeshProUGUI header1 = CreateText(panel.transform, "Header1", null, 36f, TextAlignmentOptions.Center, Gold);
            Place(header1.rectTransform, new Vector2(0.5f, 1f), new Vector2(370f, -140f), new Vector2(200f, 50f));

            TextMeshProUGUI labels = CreateText(panel.transform, "Labels", null, 36f, TextAlignmentOptions.TopLeft, TextLight);
            Place(labels.rectTransform, new Vector2(0.5f, 1f), new Vector2(-210f, -440f), new Vector2(500f, 560f));
            TextMeshProUGUI column0 = CreateText(panel.transform, "Column0", null, 36f, TextAlignmentOptions.Top, TextLight);
            Place(column0.rectTransform, new Vector2(0.5f, 1f), new Vector2(170f, -440f), new Vector2(200f, 560f));
            TextMeshProUGUI column1 = CreateText(panel.transform, "Column1", null, 36f, TextAlignmentOptions.Top, TextLight);
            Place(column1.rectTransform, new Vector2(0.5f, 1f), new Vector2(370f, -440f), new Vector2(200f, 560f));
            labels.lineSpacing = column0.lineSpacing = column1.lineSpacing = 10f;

            Button next = CreateButton(panel.transform, "NextButton", "summary.next", new Vector2(420f, 90f));
            Place((RectTransform)next.transform, new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(420f, 90f));
            next.image.color = Gold;

            var summary = overlay.gameObject.AddComponent<RoundSummaryPanel>();
            SetField(summary, "_title", title);
            SetField(summary, "_labels", labels);
            SetField(summary, "_headers", new UnityEngine.Object[] { header0, header1 });
            SetField(summary, "_columns", new UnityEngine.Object[] { column0, column1 });
            SetField(summary, "_nextButton", next);
            overlay.gameObject.SetActive(false);
            return summary;
        }

        private static GameOverPanel BuildGameOver(RectTransform parent)
        {
            Image overlay = CreateImage(parent, "GameOver", OverlayColor, null, true);
            Stretch(overlay.rectTransform);
            Image panel = CreateImage(overlay.transform, "Panel", PanelColor, RoundedSprite);
            Place(panel.rectTransform, Center, Vector2.zero, new Vector2(900f, 600f));

            TextMeshProUGUI title = CreateText(panel.transform, "Title", null, 80f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(820f, 120f));
            title.fontStyle = FontStyles.Bold;
            TextMeshProUGUI scores = CreateText(panel.transform, "Scores", null, 44f, TextAlignmentOptions.Center, TextLight);
            Place(scores.rectTransform, Center, new Vector2(0f, 20f), new Vector2(800f, 180f));

            Button menu = CreateButton(panel.transform, "MenuButton", "gameover.main_menu", new Vector2(340f, 90f));
            Place((RectTransform)menu.transform, new Vector2(0.5f, 0f), new Vector2(-200f, 80f), new Vector2(340f, 90f));
            Button rematch = CreateButton(panel.transform, "RematchButton", "gameover.rematch", new Vector2(340f, 90f));
            Place((RectTransform)rematch.transform, new Vector2(0.5f, 0f), new Vector2(200f, 80f), new Vector2(340f, 90f));
            rematch.image.color = Gold;

            var gameOver = overlay.gameObject.AddComponent<GameOverPanel>();
            SetField(gameOver, "_title", title);
            SetField(gameOver, "_scores", scores);
            SetField(gameOver, "_rematchButton", rematch);
            SetField(gameOver, "_menuButton", menu);
            overlay.gameObject.SetActive(false);
            return gameOver;
        }

        // ---------------------------------------------------------------- Utilidades

        private static Vector2 Center => new Vector2(0.5f, 0.5f);

        private static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        private static void Save(Scene scene, string name)
        {
            if (!EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{name}.unity"))
            {
                throw new InvalidOperationException($"No se pudo guardar la escena {name}.");
            }
        }

        /// <summary>Cámara, bootstrap y, si hace falta, Canvas + EventSystem. Devuelve la zona segura.</summary>
        private static RectTransform CreateCommon(GameSettings settings, bool withCanvas)
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FeltColor;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var bootstrap = new GameObject("SceneBootstrap").AddComponent<SceneBootstrap>();
            SetField(bootstrap, "_settings", settings);

            if (!withCanvas)
            {
                return null;
            }

            var canvasObject = new GameObject("Canvas", typeof(RectTransform)) { layer = LayerMask.NameToLayer("UI") };
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            RectTransform safe = CreateRect("SafeArea", canvasObject.transform);
            Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();
            return safe;
        }

        private static RectTransform Anchor(RectTransform parent, string name, Vector2 position)
        {
            return Place(CreateRect(name, parent), Center, position, new Vector2(10f, 10f));
        }

        private static Image PileBack(RectTransform parent, string name, Vector2 position)
        {
            Image image = CreateImage(parent, name, Color.white);
            Place(image.rectTransform, Center, position, new Vector2(84f, 120f));
            image.preserveAspect = true;
            return image;
        }

        private static GameObject DealerBadge(Transform parent, string name, float y)
        {
            Image badge = CreateImage(parent, name, Gold, RoundedSprite);
            Place(badge.rectTransform, new Vector2(0f, 1f), new Vector2(45f, y), new Vector2(60f, 36f));
            TextMeshProUGUI label = CreateText(badge.transform, "Label", "game.dealer", 18f, TextAlignmentOptions.Center, TextDark);
            Stretch(label.rectTransform, 2f);
            label.enableAutoSizing = true;
            label.fontSizeMin = 10f;
            label.fontSizeMax = 18f;
            return badge.gameObject;
        }

        private static Button ComingSoonButton(Transform parent, string name, string key)
        {
            Button button = CreateButton(parent, name, key, new Vector2(680f, 96f));
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            label.rectTransform.offsetMin = new Vector2(8f, 30f);
            label.rectTransform.offsetMax = new Vector2(-8f, -2f);
            TextMeshProUGUI soon = CreateText(button.transform, "ComingSoon", "menu.coming_soon", 24f, TextAlignmentOptions.Bottom, new Color(0.3f, 0.22f, 0.05f, 1f));
            Stretch(soon.rectTransform, 6f);
            return button;
        }

        private static Button[] ButtonRow(RectTransform parent, string prefix, string[] keys, float y)
        {
            var buttons = new Button[keys.Length];
            const float width = 380f;
            const float gap = 30f;
            float start = -((keys.Length - 1) * (width + gap)) / 2f;
            for (int i = 0; i < keys.Length; i++)
            {
                buttons[i] = CreateButton(parent, $"{prefix}{i}", keys[i], new Vector2(width, 90f), 34f);
                Place((RectTransform)buttons[i].transform, new Vector2(0.5f, 1f), new Vector2(start + (i * (width + gap)), y), new Vector2(width, 90f));
            }

            return buttons;
        }

        private static void VerticalLayout(RectTransform rect, float spacing)
        {
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }
    }
}
