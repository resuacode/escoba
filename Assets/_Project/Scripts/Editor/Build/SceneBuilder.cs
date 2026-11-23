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
using Object = UnityEngine.Object;

namespace Escoba.Editor.Build
{
    /// <summary>
    /// Construye las escenas Boot, MainMenu y Game y el prefab de carta de forma reproducible
    /// (menú <c>Escoba/Build/...</c>). Sobrescribe las escenas: los ajustes visuales que deban
    /// conservarse se hacen aquí, no a mano en la escena.
    /// Coordenadas en unidades del Canvas de referencia 1920×1080, con origen en el centro.
    /// </summary>
    public static class SceneBuilder
    {
        public const string ScenesFolder = "Assets/_Project/Scenes";
        public const string CardPrefabPath = "Assets/_Project/Prefabs/CardView.prefab";

        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        private static readonly Color FeltColor = new Color(0.05f, 0.27f, 0.15f, 1f);
        private static readonly Color TableAreaColor = new Color(0.08f, 0.35f, 0.2f, 1f);
        private static readonly Color SeatPanelColor = new Color(0.04f, 0.12f, 0.08f, 1f);

        // Huecos de asiento en el orden de SeatLayout: abajo, derecha, arriba, izquierda.
        private static readonly Vector2[] HandPositions = { new Vector2(0f, -400f), new Vector2(720f, 20f), new Vector2(0f, 420f), new Vector2(-720f, 20f) };
        private static readonly Vector2[] SeatPanelPositions = { new Vector2(-470f, -430f), new Vector2(820f, -230f), new Vector2(-470f, 430f), new Vector2(-820f, 230f) };
        private static readonly string[] SlotNames = { "Bottom", "Right", "Top", "Left" };

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
            Object.DestroyImmediate(root);
            return prefab.GetComponent<CardView>();
        }

        // ---------------------------------------------------------------- Boot

        private static void BuildBoot(GameSettings settings)
        {
            Scene scene = NewScene();
            // Crear una escena nueva descarga los assets no usados: recargar referencias por ruta.
            settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            CreateCommon(settings, false);
            new GameObject("BootLoader").AddComponent<BootLoader>();
            Save(scene, SceneNames.Boot);
        }

        // ---------------------------------------------------------------- Menú principal

        private static void BuildMainMenu(GameSettings settings)
        {
            Scene scene = NewScene();
            settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            RectTransform safe = CreateCommon(settings, true);

            TextMeshProUGUI title = CreateText(safe, "Title", "app.title", 150f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(1200f, 200f));
            title.fontStyle = FontStyles.Bold;

            // Menú principal
            RectTransform main = CreateRect("MainPanel", safe);
            Place(main, Center, new Vector2(0f, -90f), new Vector2(720f, 700f));
            VerticalLayout(main, 22f);

            Button playAi = CreateButton(main, "PlayAiButton", "menu.play_ai", new Vector2(680f, 96f));
            Button playLocal = CreateButton(main, "PlayLocalButton", "menu.play_local", new Vector2(680f, 96f));
            Button playOnline = ComingSoonButton(main, "PlayOnlineButton", "menu.play_online");
            Button howTo = ComingSoonButton(main, "HowToPlayButton", "menu.how_to_play");
            Button language = CreateButton(main, "LanguageButton", null, new Vector2(680f, 96f));
            Button quit = CreateButton(main, "QuitButton", "menu.quit", new Vector2(680f, 96f));

            // Nueva partida (dos columnas)
            Image setupBackground = CreateImage(safe, "SetupPanel", PanelColor, RoundedSprite, true);
            RectTransform setup = setupBackground.rectTransform;
            Place(setup, Center, new Vector2(0f, -10f), new Vector2(1640f, 960f));

            Label(setup, "Title", "setup.title", 60f, new Vector2(0f, -60f), new Vector2(1000f, 80f), Gold);

            // Columna izquierda: jugadores, modo y asientos
            const float leftX = -400f;
            Label(setup, "PlayersLabel", "setup.players", 38f, new Vector2(leftX, -150f), new Vector2(700f, 56f), TextLight);
            Button[] counts = ButtonRow(setup, "PlayerCount", new[] { (string)null, null, null }, new Vector2(leftX, -230f), new Vector2(150f, 80f), 20f);
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i].GetComponentInChildren<TMP_Text>().text = (i + 2).ToString();
            }

            Button[] modes = ButtonRow(setup, "Mode", new[] { "setup.mode.individual", "setup.mode.teams" }, new Vector2(leftX, -330f), new Vector2(300f, 80f), 30f);

            var seatRows = new Object[4];
            var seatLabels = new Object[4];
            var seatButtons = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransform row = CreateRect($"SeatRow{i}", setup);
                Place(row, new Vector2(0.5f, 1f), new Vector2(leftX, -430f - (i * 82f)), new Vector2(640f, 72f));
                TextMeshProUGUI label = CreateText(row, "Label", "player.human", 34f, TextAlignmentOptions.Left, TextLight);
                Place(label.rectTransform, Center, new Vector2(-160f, 0f), new Vector2(300f, 64f));
                Button seatButton = CreateButton(row, "SeatButton", "setup.seat.human", new Vector2(280f, 68f), 32f);
                Place((RectTransform)seatButton.transform, Center, new Vector2(170f, 0f), new Vector2(280f, 68f));
                seatRows[i] = row.gameObject;
                seatLabels[i] = label.GetComponent<LocalizedText>();
                seatButtons[i] = seatButton;
            }

            // Columna derecha: reglas y dificultad
            const float rightX = 400f;
            Label(setup, "RulesLabel", "setup.rules", 38f, new Vector2(rightX, -150f), new Vector2(700f, 56f), TextLight);
            var presets = new Object[3];
            string[] presetKeys = { "setup.preset.classic21", "setup.preset.quick15", "setup.preset.long31" };
            for (int i = 0; i < presets.Length; i++)
            {
                Button preset = CreateButton(setup, $"Preset{i}", presetKeys[i], new Vector2(560f, 78f), 34f);
                Place((RectTransform)preset.transform, new Vector2(0.5f, 1f), new Vector2(rightX, -230f - (i * 92f)), new Vector2(560f, 78f));
                presets[i] = preset;
            }

            Label(setup, "DifficultyLabel", "setup.difficulty", 38f, new Vector2(rightX, -520f), new Vector2(700f, 56f), TextLight);
            Button[] difficulties = ButtonRow(setup, "Difficulty", new[] { "setup.difficulty.easy", "setup.difficulty.medium", "setup.difficulty.hard" },
                new Vector2(rightX, -600f), new Vector2(180f, 78f), 16f);

            TextMeshProUGUI needHuman = CreateText(setup, "NeedHuman", "setup.need_human", 30f, TextAlignmentOptions.Center, Gold);
            Place(needHuman.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(1200f, 50f));

            Button back = CreateButton(setup, "BackButton", "common.back", new Vector2(320f, 96f));
            Place((RectTransform)back.transform, new Vector2(0.5f, 0f), new Vector2(-220f, 85f), new Vector2(320f, 96f));
            Button start = CreateButton(setup, "StartButton", "setup.start", new Vector2(320f, 96f));
            Place((RectTransform)start.transform, new Vector2(0.5f, 0f), new Vector2(220f, 85f), new Vector2(320f, 96f));
            start.image.color = Gold;

            var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            SetField(controller, "_title", title.gameObject);
            SetField(controller, "_mainPanel", main.gameObject);
            SetField(controller, "_playAiButton", playAi);
            SetField(controller, "_playLocalButton", playLocal);
            SetField(controller, "_comingSoonButtons", new Object[] { playOnline, howTo });
            SetField(controller, "_languageButton", language);
            SetField(controller, "_languageLabel", language.GetComponentInChildren<TMP_Text>());
            SetField(controller, "_quitButton", quit);
            SetField(controller, "_setupPanel", setup.gameObject);
            SetField(controller, "_playerCountButtons", counts);
            SetField(controller, "_modeButtons", modes);
            SetField(controller, "_seatRows", seatRows);
            SetField(controller, "_seatLabels", seatLabels);
            SetField(controller, "_seatButtons", seatButtons);
            SetField(controller, "_presetButtons", presets);
            SetField(controller, "_difficultyButtons", difficulties);
            SetField(controller, "_needHumanMessage", needHuman.gameObject);
            SetField(controller, "_startButton", start);
            SetField(controller, "_backButton", back);

            Save(scene, SceneNames.MainMenu);
        }

        // ---------------------------------------------------------------- Mesa

        private static void BuildGame(GameSettings settings, CardView cardPrefab)
        {
            Scene scene = NewScene();
            settings = AssetDatabase.LoadAssetAtPath<GameSettings>(DefaultAssetsBuilder.GameSettingsPath);
            cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath).GetComponent<CardView>();
            RectTransform safe = CreateCommon(settings, true);

            // Zona de mesa (también es la zona donde soltar cartas arrastradas).
            Image tableArea = CreateImage(safe, "TableArea", TableAreaColor, RoundedSprite);
            Place(tableArea.rectTransform, Center, new Vector2(0f, 20f), new Vector2(1000f, 440f));
            RectTransform tableAnchor = Anchor(safe, "TableAnchor", new Vector2(0f, 20f));

            // Mazo (abajo a la izquierda)
            Vector2 deckPosition = new Vector2(-820f, -330f);
            RectTransform deckAnchor = Anchor(safe, "DeckAnchor", deckPosition);
            Image deckBack = CreateImage(safe, "DeckBack", Color.white);
            Place(deckBack.rectTransform, Center, deckPosition, new Vector2(126f, 180f));
            TextMeshProUGUI deckLabel = CreateText(safe, "DeckLabel", "game.deck", 28f, TextAlignmentOptions.Center, TextLight);
            Place(deckLabel.rectTransform, Center, deckPosition + new Vector2(0f, 115f), new Vector2(220f, 44f));
            TextMeshProUGUI deckCount = CreateText(safe, "DeckCount", null, 34f, TextAlignmentOptions.Center, TextLight);
            Place(deckCount.rectTransform, Center, deckPosition + new Vector2(0f, -118f), new Vector2(200f, 50f));

            // Huecos de asiento: ancla de mano + ficha (nombre, baza, escobas, repartidor, turno)
            var handAnchors = new Object[4];
            var seatPanels = new Object[4];
            for (int slot = 0; slot < 4; slot++)
            {
                handAnchors[slot] = Anchor(safe, $"HandAnchor_{SlotNames[slot]}", HandPositions[slot]);
                seatPanels[slot] = BuildSeatPanel(safe, $"SeatPanel_{SlotNames[slot]}", SeatPanelPositions[slot]);
            }

            // Capa de cartas (por encima de la mesa y por debajo del HUD)
            RectTransform cardLayer = CreateRect("CardLayer", safe);
            Stretch(cardLayer);

            // Marcador (arriba a la derecha)
            Image board = CreateImage(safe, "Scoreboard", PanelColor, RoundedSprite);
            Place(board.rectTransform, new Vector2(1f, 1f), new Vector2(-240f, -190f), new Vector2(440f, 340f));
            TextMeshProUGUI target = BoardText(board.transform, "Target", -30f, 26f, TextLight);
            TextMeshProUGUI round = BoardText(board.transform, "Round", -68f, 26f, TextLight);
            TextMeshProUGUI turn = BoardText(board.transform, "Turn", -106f, 28f, Gold);
            var scoreRows = new Object[4];
            var scoreNames = new Object[4];
            var scoreValues = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransform row = CreateRect($"ScoreRow{i}", board.transform);
                Place(row, new Vector2(0.5f, 1f), new Vector2(0f, -160f - (i * 46f)), new Vector2(400f, 44f));
                TextMeshProUGUI rowName = CreateText(row, "Name", null, 28f, TextAlignmentOptions.Left, TextLight);
                Place(rowName.rectTransform, Center, new Vector2(-40f, 0f), new Vector2(320f, 44f));
                rowName.textWrappingMode = TextWrappingModes.NoWrap;
                rowName.overflowMode = TextOverflowModes.Ellipsis;
                TextMeshProUGUI rowScore = CreateText(row, "Score", null, 34f, TextAlignmentOptions.Right, Gold);
                Place(rowScore.rectTransform, Center, new Vector2(160f, 0f), new Vector2(80f, 44f));
                scoreRows[i] = row.gameObject;
                scoreNames[i] = rowName;
                scoreValues[i] = rowScore;
            }

            // Indicaciones y botones de jugada
            TextMeshProUGUI hint = CreateText(safe, "Hint", null, 30f, TextAlignmentOptions.Center, TextLight);
            Place(hint.rectTransform, Center, new Vector2(0f, -248f), new Vector2(1100f, 50f));
            TextMeshProUGUI sum = CreateText(safe, "Sum", null, 34f, TextAlignmentOptions.Center, Gold);
            Place(sum.rectTransform, Center, new Vector2(420f, -330f), new Vector2(240f, 56f));
            Button undo = CreateButton(safe, "UndoButton", "game.undo", new Vector2(220f, 84f), 36f);
            Place((RectTransform)undo.transform, Center, new Vector2(580f, -470f), new Vector2(220f, 84f));
            Button play = CreateButton(safe, "PlayButton", "game.play", new Vector2(220f, 84f), 36f);
            Place((RectTransform)play.transform, Center, new Vector2(820f, -470f), new Vector2(220f, 84f));
            play.image.color = Gold;
            Button menu = CreateButton(safe, "MenuButton", "game.menu", new Vector2(180f, 70f), 32f);
            Place((RectTransform)menu.transform, new Vector2(0f, 1f), new Vector2(110f, -55f), new Vector2(180f, 70f));
            menu.gameObject.AddComponent<GameMenuButton>();

            // Aviso grande
            Image bannerBackground = CreateImage(safe, "Banner", new Color(0f, 0f, 0f, 0.6f), RoundedSprite);
            Place(bannerBackground.rectTransform, Center, new Vector2(0f, 20f), new Vector2(1100f, 200f));
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
            SetField(hud, "_scoreRows", scoreRows);
            SetField(hud, "_scoreNames", scoreNames);
            SetField(hud, "_scoreValues", scoreValues);
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

            // Paneles superpuestos
            RoundSummaryPanel summary = BuildRoundSummary(safe);
            GameOverPanel gameOver = BuildGameOver(safe);
            PassScreenPanel passScreen = BuildPassScreen(safe);

            // Mesa y controlador
            var table = new GameObject("Table").AddComponent<TablePresenter>();
            SetField(table, "_cardPrefab", cardPrefab);
            SetField(table, "_cardLayer", cardLayer);
            SetField(table, "_handAnchors", handAnchors);
            SetField(table, "_seatPanels", seatPanels);
            SetField(table, "_tableAnchor", tableAnchor);
            SetField(table, "_tableDropZone", tableArea.rectTransform);
            SetField(table, "_deckAnchor", deckAnchor);
            SetField(table, "_hud", hud);

            var controller = new GameObject("GameController").AddComponent<GameController>();
            SetField(controller, "_table", table);
            SetField(controller, "_roundSummary", summary);
            SetField(controller, "_gameOver", gameOver);
            SetField(controller, "_passScreen", passScreen);

            Save(scene, SceneNames.Game);
        }

        private static SeatPanelView BuildSeatPanel(RectTransform parent, string name, Vector2 position)
        {
            Image highlight = CreateImage(parent, name, Gold, RoundedSprite);
            Place(highlight.rectTransform, Center, position, new Vector2(272f, 162f));
            Image background = CreateImage(highlight.transform, "Background", SeatPanelColor, RoundedSprite);
            Stretch(background.rectTransform, 6f);

            TextMeshProUGUI playerName = CreateText(highlight.transform, "Name", null, 30f, TextAlignmentOptions.Left, TextLight);
            Place(playerName.rectTransform, new Vector2(0.5f, 1f), new Vector2(-20f, -32f), new Vector2(210f, 44f));
            playerName.textWrappingMode = TextWrappingModes.NoWrap;
            playerName.overflowMode = TextOverflowModes.Ellipsis;

            Image dealer = CreateImage(highlight.transform, "DealerBadge", Gold, RoundedSprite);
            Place(dealer.rectTransform, new Vector2(1f, 1f), new Vector2(-38f, -32f), new Vector2(64f, 32f));
            TextMeshProUGUI dealerLabel = CreateText(dealer.transform, "Label", "game.dealer", 18f, TextAlignmentOptions.Center, TextDark);
            Stretch(dealerLabel.rectTransform, 2f);
            dealerLabel.enableAutoSizing = true;
            dealerLabel.fontSizeMin = 10f;
            dealerLabel.fontSizeMax = 18f;

            Image pileBack = CreateImage(highlight.transform, "PileBack", Color.white);
            Place(pileBack.rectTransform, new Vector2(0f, 0f), new Vector2(52f, 56f), new Vector2(49f, 70f));
            pileBack.preserveAspect = true;
            TextMeshProUGUI pileCount = CreateText(highlight.transform, "PileCount", null, 26f, TextAlignmentOptions.Left, TextLight);
            Place(pileCount.rectTransform, new Vector2(0f, 0f), new Vector2(170f, 74f), new Vector2(170f, 36f));
            TextMeshProUGUI escobas = CreateText(highlight.transform, "Escobas", null, 26f, TextAlignmentOptions.Left, Gold);
            Place(escobas.rectTransform, new Vector2(0f, 0f), new Vector2(170f, 38f), new Vector2(170f, 36f));

            var panel = highlight.gameObject.AddComponent<SeatPanelView>();
            SetField(panel, "_name", playerName);
            SetField(panel, "_pileBack", pileBack);
            SetField(panel, "_pileCount", pileCount);
            SetField(panel, "_escobas", escobas);
            SetField(panel, "_dealerBadge", dealer.gameObject);
            SetField(panel, "_turnHighlight", highlight);
            highlight.enabled = false;
            return panel;
        }

        private static RoundSummaryPanel BuildRoundSummary(RectTransform parent)
        {
            Image overlay = CreateImage(parent, "RoundSummary", OverlayColor, null, true);
            Stretch(overlay.rectTransform);
            Image panel = CreateImage(overlay.transform, "Panel", PanelColor, RoundedSprite);
            Place(panel.rectTransform, Center, Vector2.zero, new Vector2(1440f, 860f));

            TextMeshProUGUI title = CreateText(panel.transform, "Title", null, 60f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1200f, 80f));

            TextMeshProUGUI labels = CreateText(panel.transform, "Labels", null, 36f, TextAlignmentOptions.TopLeft, TextLight);
            Place(labels.rectTransform, new Vector2(0.5f, 1f), new Vector2(-400f, -470f), new Vector2(540f, 580f));
            labels.lineSpacing = 10f;

            var headers = new Object[4];
            var columns = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                TextMeshProUGUI header = CreateText(panel.transform, $"Header{i}", null, 30f, TextAlignmentOptions.Center, Gold);
                Place(header.rectTransform, new Vector2(0.5f, 1f), new Vector2(285f, -140f), new Vector2(185f, 70f));
                header.enableAutoSizing = true;
                header.fontSizeMin = 18f;
                header.fontSizeMax = 30f;
                TextMeshProUGUI column = CreateText(panel.transform, $"Column{i}", null, 36f, TextAlignmentOptions.Top, TextLight);
                Place(column.rectTransform, new Vector2(0.5f, 1f), new Vector2(285f, -470f), new Vector2(185f, 580f));
                column.lineSpacing = 10f;
                headers[i] = header;
                columns[i] = column;
            }

            Button next = CreateButton(panel.transform, "NextButton", "summary.next", new Vector2(420f, 90f));
            Place((RectTransform)next.transform, new Vector2(0.5f, 0f), new Vector2(0f, 75f), new Vector2(420f, 90f));
            next.image.color = Gold;

            var summary = overlay.gameObject.AddComponent<RoundSummaryPanel>();
            SetField(summary, "_title", title);
            SetField(summary, "_labels", labels);
            SetField(summary, "_headers", headers);
            SetField(summary, "_columns", columns);
            SetField(summary, "_nextButton", next);
            overlay.gameObject.SetActive(false);
            return summary;
        }

        private static GameOverPanel BuildGameOver(RectTransform parent)
        {
            Image overlay = CreateImage(parent, "GameOver", OverlayColor, null, true);
            Stretch(overlay.rectTransform);
            Image panel = CreateImage(overlay.transform, "Panel", PanelColor, RoundedSprite);
            Place(panel.rectTransform, Center, Vector2.zero, new Vector2(1000f, 680f));

            TextMeshProUGUI title = CreateText(panel.transform, "Title", null, 76f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(920f, 120f));
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 40f;
            title.fontSizeMax = 76f;
            TextMeshProUGUI scores = CreateText(panel.transform, "Scores", null, 40f, TextAlignmentOptions.Center, TextLight);
            Place(scores.rectTransform, Center, new Vector2(0f, 20f), new Vector2(900f, 260f));

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

        private static PassScreenPanel BuildPassScreen(RectTransform parent)
        {
            // Opaco: no debe dejar ver ninguna carta mientras se pasa el dispositivo.
            Image overlay = CreateImage(parent, "PassScreen", FeltColor, null, true);
            Stretch(overlay.rectTransform);

            TextMeshProUGUI title = CreateText(overlay.transform, "Title", null, 72f, TextAlignmentOptions.Center, Gold);
            Place(title.rectTransform, Center, new Vector2(0f, 200f), new Vector2(1600f, 120f));
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 36f;
            title.fontSizeMax = 72f;

            TextMeshProUGUI lastPlay = CreateText(overlay.transform, "LastPlay", null, 40f, TextAlignmentOptions.Center, TextLight);
            Place(lastPlay.rectTransform, Center, new Vector2(0f, 60f), new Vector2(1500f, 120f));

            TextMeshProUGUI hint = CreateText(overlay.transform, "Hint", "pass.hint", 30f, TextAlignmentOptions.Center, TextLight);
            Place(hint.rectTransform, Center, new Vector2(0f, -40f), new Vector2(1200f, 50f));

            Button confirm = CreateButton(overlay.transform, "ConfirmButton", "pass.confirm", new Vector2(760f, 110f), 40f);
            Place((RectTransform)confirm.transform, Center, new Vector2(0f, -200f), new Vector2(760f, 110f));
            confirm.image.color = Gold;

            var pass = overlay.gameObject.AddComponent<PassScreenPanel>();
            SetField(pass, "_title", title);
            SetField(pass, "_lastPlay", lastPlay);
            SetField(pass, "_confirmButton", confirm);
            SetField(pass, "_confirmLabel", confirm.GetComponentInChildren<LocalizedText>());
            overlay.gameObject.SetActive(false);
            return pass;
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

        private static TextMeshProUGUI Label(RectTransform parent, string name, string key, float size, Vector2 position, Vector2 dimensions, Color color)
        {
            TextMeshProUGUI text = CreateText(parent, name, key, size, TextAlignmentOptions.Center, color);
            Place(text.rectTransform, new Vector2(0.5f, 1f), position, dimensions);
            return text;
        }

        private static TextMeshProUGUI BoardText(Transform board, string name, float y, float size, Color color)
        {
            TextMeshProUGUI text = CreateText(board, name, null, size, TextAlignmentOptions.Left, color);
            Place(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(400f, 38f));
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
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

        /// <summary>Fila de botones centrada en <paramref name="center"/> (anclada arriba del padre).</summary>
        private static Button[] ButtonRow(RectTransform parent, string prefix, string[] keys, Vector2 center, Vector2 size, float gap)
        {
            var buttons = new Button[keys.Length];
            float start = center.x - ((keys.Length - 1) * (size.x + gap) / 2f);
            for (int i = 0; i < keys.Length; i++)
            {
                buttons[i] = CreateButton(parent, $"{prefix}{i}", keys[i], size, 34f);
                Place((RectTransform)buttons[i].transform, new Vector2(0.5f, 1f), new Vector2(start + (i * (size.x + gap)), center.y), size);
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
