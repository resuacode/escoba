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
            Button howTo = CreateButton(main, "HowToPlayButton", "menu.how_to_play", new Vector2(680f, 96f));
            Button settingsButton = CreateButton(main, "SettingsButton", "menu.settings", new Vector2(680f, 96f));
            Button quit = CreateButton(main, "QuitButton", "menu.quit", new Vector2(680f, 96f));

            TextMeshProUGUI version = CreateText(safe, "Version", null, 24f, TextAlignmentOptions.BottomRight, new Color(1f, 1f, 1f, 0.6f));
            Place(version.rectTransform, new Vector2(1f, 0f), new Vector2(-220f, 30f), new Vector2(420f, 40f));
            version.gameObject.AddComponent<VersionLabel>();

            Button credits = CreateButton(safe, "CreditsButton", "menu.credits", new Vector2(240f, 64f), 30f);
            Place((RectTransform)credits.transform, new Vector2(0f, 0f), new Vector2(150f, 62f), new Vector2(240f, 64f));

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
            var nameInputs = new Object[4];
            var namePlaceholders = new Object[4];
            var botLabels = new Object[4];
            var seatButtons = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransform row = CreateRect($"SeatRow{i}", setup);
                Place(row, new Vector2(0.5f, 1f), new Vector2(leftX, -430f - (i * 82f)), new Vector2(640f, 72f));

                TMP_InputField nameInput = CreateInputField(row, $"NameInput{i}", "player.human", new Vector2(320f, 64f), 32f);
                Place((RectTransform)nameInput.transform, Center, new Vector2(-150f, 0f), new Vector2(320f, 64f));
                TextMeshProUGUI botLabel = CreateText(row, "BotLabel", "player.bot", 34f, TextAlignmentOptions.Left, TextLight);
                Place(botLabel.rectTransform, Center, new Vector2(-150f, 0f), new Vector2(320f, 64f));

                Button seatButton = CreateButton(row, "SeatButton", "setup.seat.human", new Vector2(260f, 68f), 32f);
                Place((RectTransform)seatButton.transform, Center, new Vector2(180f, 0f), new Vector2(260f, 68f));
                seatRows[i] = row.gameObject;
                nameInputs[i] = nameInput;
                namePlaceholders[i] = nameInput.placeholder.GetComponent<LocalizedText>();
                botLabels[i] = botLabel.GetComponent<LocalizedText>();
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

            SettingsPanel settingsPanel = BuildSettingsPanel(safe);
            HowToPlayPanel howToPlayPanel = BuildHowToPlayPanel(safe);
            CreditsPanel creditsPanel = BuildCreditsPanel(safe);

            var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            SetField(controller, "_title", title.gameObject);
            SetField(controller, "_mainPanel", main.gameObject);
            SetField(controller, "_playAiButton", playAi);
            SetField(controller, "_playLocalButton", playLocal);
            SetField(controller, "_comingSoonButtons", new Object[] { playOnline });
            SetField(controller, "_howToPlayButton", howTo);
            SetField(controller, "_howToPlayPanel", howToPlayPanel);
            SetField(controller, "_creditsButton", credits);
            SetField(controller, "_creditsPanel", creditsPanel);
            SetField(controller, "_settingsButton", settingsButton);
            SetField(controller, "_settingsPanel", settingsPanel);
            SetField(controller, "_quitButton", quit);
            SetField(controller, "_setupPanel", setup.gameObject);
            SetField(controller, "_playerCountButtons", counts);
            SetField(controller, "_modeButtons", modes);
            SetField(controller, "_seatRows", seatRows);
            SetField(controller, "_nameInputs", nameInputs);
            SetField(controller, "_namePlaceholders", namePlaceholders);
            SetField(controller, "_botLabels", botLabels);
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
            var menuButton = menu.gameObject.AddComponent<GameMenuButton>();

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
            PausePanel pause = BuildPausePanel(safe);
            SetField(menuButton, "_pause", pause);

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

        private static PausePanel BuildPausePanel(RectTransform parent)
        {
            Image overlay = CreateImage(parent, "Pause", OverlayColor, null, true);
            Stretch(overlay.rectTransform);

            Image content = CreateImage(overlay.transform, "Content", PanelColor, RoundedSprite, true);
            Place(content.rectTransform, Center, Vector2.zero, new Vector2(900f, 800f));
            Label(content.rectTransform, "Title", "pause.title", 64f, new Vector2(0f, -80f), new Vector2(800f, 90f), Gold);

            Button resume = CreateButton(content.transform, "ResumeButton", "pause.resume", new Vector2(560f, 96f));
            Place((RectTransform)resume.transform, Center, new Vector2(0f, 175f), new Vector2(560f, 96f));
            resume.image.color = Gold;
            Button howTo = CreateButton(content.transform, "HowToPlayButton", "menu.how_to_play", new Vector2(560f, 96f));
            Place((RectTransform)howTo.transform, Center, new Vector2(0f, 60f), new Vector2(560f, 96f));
            Button settings = CreateButton(content.transform, "SettingsButton", "menu.settings", new Vector2(560f, 96f));
            Place((RectTransform)settings.transform, Center, new Vector2(0f, -55f), new Vector2(560f, 96f));
            Button quit = CreateButton(content.transform, "QuitButton", "pause.quit", new Vector2(560f, 96f));
            Place((RectTransform)quit.transform, Center, new Vector2(0f, -170f), new Vector2(560f, 96f));

            TextMeshProUGUI warning = CreateText(content.transform, "Warning", "pause.quit_warning", 28f, TextAlignmentOptions.Center, TextLight);
            Place(warning.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(800f, 50f));

            SettingsPanel settingsPanel = BuildSettingsPanel((RectTransform)overlay.transform);
            HowToPlayPanel howToPlayPanel = BuildHowToPlayPanel((RectTransform)overlay.transform);

            var pause = overlay.gameObject.AddComponent<PausePanel>();
            SetField(pause, "_content", content.gameObject);
            SetField(pause, "_resumeButton", resume);
            SetField(pause, "_settingsButton", settings);
            SetField(pause, "_quitButton", quit);
            SetField(pause, "_settings", settingsPanel);
            SetField(pause, "_howToPlayButton", howTo);
            SetField(pause, "_howToPlay", howToPlayPanel);
            overlay.gameObject.SetActive(false);
            return pause;
        }

        /// <summary>Panel de Ajustes (idioma, volúmenes, velocidad, pantalla completa). Se crea oculto.</summary>
        private static SettingsPanel BuildSettingsPanel(RectTransform parent)
        {
            Image background = CreateImage(parent, "SettingsPanel", PanelColor, RoundedSprite, true);
            RectTransform panel = background.rectTransform;
            Place(panel, Center, Vector2.zero, new Vector2(1300f, 940f));
            Label(panel, "Title", "menu.settings", 60f, new Vector2(0f, -60f), new Vector2(1000f, 80f), Gold);

            const float labelX = -380f;
            const float controlX = 200f;
            const float controlWidth = 700f;

            SettingsLabel(panel, "LanguageLabel", "menu.language", labelX, -180f);
            Button[] languages = ButtonRow(panel, "Language", new[] { (string)null, null, null }, new Vector2(controlX, -180f), new Vector2(220f, 80f), 20f);

            SettingsLabel(panel, "MusicLabel", "settings.music", labelX, -300f);
            Slider music = CreateSlider(panel, "MusicSlider", new Vector2(controlWidth, 40f));
            Place((RectTransform)music.transform, new Vector2(0.5f, 1f), new Vector2(controlX, -300f), new Vector2(controlWidth, 40f));

            SettingsLabel(panel, "SfxLabel", "settings.sfx", labelX, -410f);
            Slider sfx = CreateSlider(panel, "SfxSlider", new Vector2(controlWidth, 40f));
            Place((RectTransform)sfx.transform, new Vector2(0.5f, 1f), new Vector2(controlX, -410f), new Vector2(controlWidth, 40f));

            SettingsLabel(panel, "SpeedLabel", "settings.speed", labelX, -530f);
            Button[] speeds = ButtonRow(panel, "Speed", new[] { "settings.speed.slow", "settings.speed.normal", "settings.speed.fast" },
                new Vector2(controlX, -530f), new Vector2(220f, 80f), 20f);

            RectTransform fullscreenRow = CreateRect("FullscreenRow", panel);
            Place(fullscreenRow, new Vector2(0.5f, 1f), new Vector2(0f, -650f), new Vector2(1200f, 90f));
            TextMeshProUGUI fullscreenText = CreateText(fullscreenRow, "Label", "settings.fullscreen", 36f, TextAlignmentOptions.Left, TextLight);
            Place(fullscreenText.rectTransform, Center, new Vector2(labelX, 0f), new Vector2(440f, 80f));
            AutoSize(fullscreenText, 22f, 36f);
            Button fullscreen = CreateButton(fullscreenRow, "FullscreenButton", "settings.fullscreen.off", new Vector2(controlWidth, 80f), 34f);
            Place((RectTransform)fullscreen.transform, Center, new Vector2(controlX, 0f), new Vector2(controlWidth, 80f));

            Button close = CreateButton(panel, "CloseButton", "common.back", new Vector2(360f, 96f));
            Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(360f, 96f));
            close.image.color = Gold;

            var settingsPanel = background.gameObject.AddComponent<SettingsPanel>();
            SetField(settingsPanel, "_languageButtons", languages);
            SetField(settingsPanel, "_musicSlider", music);
            SetField(settingsPanel, "_sfxSlider", sfx);
            SetField(settingsPanel, "_speedButtons", speeds);
            SetField(settingsPanel, "_fullscreenRow", fullscreenRow.gameObject);
            SetField(settingsPanel, "_fullscreenButton", fullscreen);
            SetField(settingsPanel, "_fullscreenLabel", fullscreen.GetComponentInChildren<LocalizedText>());
            SetField(settingsPanel, "_closeButton", close);
            background.gameObject.SetActive(false);
            return settingsPanel;
        }

        /// <summary>Panel "Cómo se juega": título, texto, ejemplo con cartas y navegación. Se crea oculto.</summary>
        private static HowToPlayPanel BuildHowToPlayPanel(RectTransform parent)
        {
            Image background = CreateImage(parent, "HowToPlayPanel", PanelColor, RoundedSprite, true);
            RectTransform panel = background.rectTransform;
            Place(panel, Center, Vector2.zero, new Vector2(1640f, 980f));

            TextMeshProUGUI title = Label(panel, "Title", "howto.goal.title", 58f, new Vector2(0f, -65f), new Vector2(1400f, 80f), Gold);
            TextMeshProUGUI body = CreateText(panel, "Body", "howto.goal.body", 36f, TextAlignmentOptions.Top, TextLight);
            Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -235f), new Vector2(1440f, 230f));
            AutoSize(body, 24f, 36f);

            RectTransform example = CreateRect("Example", panel);
            Place(example, Center, new Vector2(0f, -70f), new Vector2(1400f, 300f));
            var slots = new Object[6];
            for (int i = 0; i < slots.Length; i++)
            {
                Image slot = CreateImage(example, $"Card{i}", Color.white);
                Place(slot.rectTransform, Center, Vector2.zero, new Vector2(154f, 220f));
                slot.preserveAspect = true;
                slots[i] = slot;
            }

            TextMeshProUGUI playedLabel = CreateText(example, "PlayedLabel", "howto.your_card", 30f, TextAlignmentOptions.Center, Gold);
            Place(playedLabel.rectTransform, Center, new Vector2(-200f, 145f), new Vector2(360f, 44f));
            TextMeshProUGUI tableLabel = CreateText(example, "TableLabel", "howto.on_table", 30f, TextAlignmentOptions.Center, Gold);
            Place(tableLabel.rectTransform, Center, new Vector2(200f, 145f), new Vector2(360f, 44f));

            TextMeshProUGUI caption = CreateText(panel, "Caption", "howto.goal.caption", 34f, TextAlignmentOptions.Center, Gold);
            Place(caption.rectTransform, Center, new Vector2(0f, -275f), new Vector2(1440f, 70f));
            AutoSize(caption, 22f, 34f);

            TextMeshProUGUI page = CreateText(panel, "Page", "howto.page", 28f, TextAlignmentOptions.Center, TextLight);
            Place(page.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(240f, 50f));

            Button previous = CreateButton(panel, "PreviousButton", "howto.previous", new Vector2(320f, 96f));
            Place((RectTransform)previous.transform, new Vector2(0.5f, 0f), new Vector2(-560f, 85f), new Vector2(320f, 96f));
            Button close = CreateButton(panel, "CloseButton", "common.back", new Vector2(320f, 96f));
            Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(-200f, 85f), new Vector2(320f, 96f));
            Button next = CreateButton(panel, "NextButton", "howto.next", new Vector2(320f, 96f));
            Place((RectTransform)next.transform, new Vector2(0.5f, 0f), new Vector2(560f, 85f), new Vector2(320f, 96f));
            next.image.color = Gold;
            Place(page.rectTransform, new Vector2(0.5f, 0f), new Vector2(180f, 85f), new Vector2(240f, 50f));

            var howTo = background.gameObject.AddComponent<HowToPlayPanel>();
            SetField(howTo, "_title", title.GetComponent<LocalizedText>());
            SetField(howTo, "_body", body.GetComponent<LocalizedText>());
            SetField(howTo, "_caption", caption.GetComponent<LocalizedText>());
            SetField(howTo, "_pageLabel", page.GetComponent<LocalizedText>());
            SetField(howTo, "_exampleArea", example);
            SetField(howTo, "_cardSlots", slots);
            SetField(howTo, "_playedLabel", playedLabel.rectTransform);
            SetField(howTo, "_tableLabel", tableLabel.rectTransform);
            SetField(howTo, "_previousButton", previous);
            SetField(howTo, "_nextButton", next);
            SetField(howTo, "_closeButton", close);
            background.gameObject.SetActive(false);
            return howTo;
        }

        /// <summary>Panel de créditos (texto <c>credits.body</c>). Se crea oculto.</summary>
        private static CreditsPanel BuildCreditsPanel(RectTransform parent)
        {
            Image background = CreateImage(parent, "CreditsPanel", PanelColor, RoundedSprite, true);
            RectTransform panel = background.rectTransform;
            Place(panel, Center, Vector2.zero, new Vector2(1640f, 980f));
            Label(panel, "Title", "menu.credits", 58f, new Vector2(0f, -65f), new Vector2(1400f, 80f), Gold);

            TextMeshProUGUI body = CreateText(panel, "Body", "credits.body", 30f, TextAlignmentOptions.Top, TextLight);
            Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -490f), new Vector2(1480f, 700f));
            AutoSize(body, 18f, 30f);
            body.lineSpacing = 6f;

            Button close = CreateButton(panel, "CloseButton", "common.back", new Vector2(360f, 96f));
            Place((RectTransform)close.transform, new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(360f, 96f));
            close.image.color = Gold;

            var credits = background.gameObject.AddComponent<CreditsPanel>();
            SetField(credits, "_closeButton", close);
            background.gameObject.SetActive(false);
            return credits;
        }

        private static void SettingsLabel(RectTransform parent, string name, string key, float x, float y)
        {
            TextMeshProUGUI text = CreateText(parent, name, key, 36f, TextAlignmentOptions.Left, TextLight);
            Place(text.rectTransform, new Vector2(0.5f, 1f), new Vector2(x, y), new Vector2(440f, 80f));
            AutoSize(text, 22f, 36f);
        }

        private static void AutoSize(TextMeshProUGUI text, float min, float max)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
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
