using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Escoba.AI;
using Escoba.App;
using Escoba.Core;
using Escoba.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Escoba.Tests.PlayMode
{
    /// <summary>
    /// Pruebas de humo en las escenas reales. Cualquier Debug.LogError (p. ej. si la mesa no
    /// coincide con el motor) hace fallar el test.
    /// </summary>
    public class GameSmokeTests
    {
        private const float GameTimeoutSeconds = 150f;

        // { jugadores, parejas (0/1), dificultad (0 Fácil, 1 Medio, 2 Difícil) }
        private static readonly int[][] BotConfigurations = { new[] { 2, 0, 0 }, new[] { 3, 0, 1 }, new[] { 4, 0, 2 }, new[] { 4, 1, 2 } };

        private static RulesPreset DefaultPreset()
        {
            return AppContext.Settings.Presets[AppContext.Settings.DefaultPresetIndex];
        }

        private static IEnumerator EnsureAppContext()
        {
            if (!AppContext.IsInitialized)
            {
                SceneManager.LoadScene(SceneNames.Boot);
                yield return null;
                yield return null;
            }
        }

        private static IEnumerator LoadGame(MatchConfig config, bool autoPlay, float speed, System.Action<GameController> onLoaded)
        {
            AppContext.PendingMatch = config;
            GameController controller = null;
            void OnLoaded(Scene scene, LoadSceneMode mode)
            {
                controller = Object.FindFirstObjectByType<GameController>();
                controller.ConfigureAutoPlay(autoPlay, speed);
            }

            SceneManager.sceneLoaded += OnLoaded;
            SceneManager.LoadScene(SceneNames.Game);
            yield return null;
            SceneManager.sceneLoaded -= OnLoaded;
            Assert.That(controller, Is.Not.Null);
            onLoaded(controller);
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, float seconds)
        {
            float start = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => condition() || Time.realtimeSinceStartup - start > seconds);
        }

        [UnityTest]
        public IEnumerator BootScene_LoadsMainMenuWithTranslatedTexts()
        {
            SceneManager.LoadScene(SceneNames.Boot);
            yield return null;
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNames.MainMenu));
            Assert.That(Localization.Service, Is.Not.Null);
            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                Assert.That(text.text, Does.Not.StartWith("["), $"texto sin traducir en {text.name}: {text.text}");
                Assert.That(text.text, Does.Not.Contain("{0}"), $"texto sin parámetros en {text.name}: {text.text}");
            }
        }

        [UnityTest]
        public IEnumerator GameScene_BotsOnly_PlayFullGameWithoutErrors([ValueSource(nameof(BotConfigurations))] int[] configuration)
        {
            int players = configuration[0];
            bool teams = configuration[1] == 1;
            var difficulty = (BotDifficulty)configuration[2];
            yield return EnsureAppContext();
            GameController controller = null;
            yield return LoadGame(MatchConfig.Versus(DefaultPreset(), difficulty, players, teams), true, 40f, c => controller = c);

            yield return WaitFor(() => controller.State != null && controller.State.Phase == GamePhase.GameOver, GameTimeoutSeconds);

            Assert.That(controller.State.Phase, Is.EqualTo(GamePhase.GameOver), $"semilla {controller.Seed}: la partida no terminó a tiempo");
            Assert.That(controller.State.PlayerCount, Is.EqualTo(players));
            Assert.That(controller.State.SideCount, Is.EqualTo(teams ? 2 : players));
            Assert.That(controller.State.GetSide(controller.State.WinnerSide).Score, Is.GreaterThanOrEqualTo(controller.State.Rules.TargetScore));

            // El panel aparece cuando terminan las animaciones de la última jugada.
            yield return WaitFor(() => Object.FindFirstObjectByType<GameOverPanel>() != null, 10f);
            Assert.That(Object.FindFirstObjectByType<GameOverPanel>(), Is.Not.Null, "debe mostrarse la pantalla de fin");
        }

        [UnityTest]
        public IEnumerator HotSeat_PassScreenHidesHandsAndSwitchesViewer()
        {
            yield return EnsureAppContext();
            var seats = new[] { SeatType.Human, SeatType.Human, SeatType.Bot };
            GameController controller = null;
            yield return LoadGame(new MatchConfig(DefaultPreset(), BotDifficulty.Easy, seats), false, 40f, c => controller = c);

            var table = Object.FindFirstObjectByType<TablePresenter>();
            var pass = Object.FindFirstObjectByType<PassScreenPanel>(FindObjectsInactive.Include);
            var viewersSeen = new HashSet<int>();
            int humanTurns = 0;
            float deadline = Time.realtimeSinceStartup + 120f;

            while (humanTurns < 8 && controller.State.Phase != GamePhase.GameOver && Time.realtimeSinceStartup < deadline)
            {
                yield return WaitFor(() => pass.IsShown || SummaryVisible() || controller.State.Phase == GamePhase.GameOver, 30f);
                if (SummaryVisible())
                {
                    Object.FindFirstObjectByType<RoundSummaryPanel>().Continue();
                    yield return null;
                    continue;
                }

                if (!pass.IsShown)
                {
                    continue;
                }

                Assert.That(table.IsViewerHandRevealed, Is.False, "con la pantalla de pase la mano debe estar oculta");
                int seat = controller.State.CurrentSeat;
                Assert.That(seats[seat], Is.EqualTo(SeatType.Human));
                pass.Confirm();
                yield return WaitFor(() => table.IsWaitingForHuman, 5f);

                Assert.That(table.ViewerSeat, Is.EqualTo(seat), "la mesa debe mostrarse desde el jugador que tiene el turno");
                Assert.That(table.IsViewerHandRevealed, Is.True);
                Assert.That(table.IsWaitingForHuman, Is.True);
                Assert.That(table.FindInconsistency(controller.ViewFor(seat)), Is.Null);
                viewersSeen.Add(seat);
                humanTurns++;

                Assert.That(table.SubmitAutomatically(LegalMoves.For(controller.ViewFor(seat))[0]), Is.True);
                yield return WaitFor(() => !table.IsViewerHandRevealed, 5f);
                Assert.That(table.IsViewerHandRevealed, Is.False, "tras jugar, la mano vuelve a ocultarse");
            }

            Assert.That(humanTurns, Is.GreaterThanOrEqualTo(4), "deberían haberse jugado varios turnos humanos");
            Assert.That(viewersSeen, Is.EquivalentTo(new[] { 0, 1 }), "ambos humanos deben haber jugado con su propia vista");
        }

        // Preferencias que tocan los tests de menú: se guardan antes y se restauran en TearDown
        // (que se ejecuta aunque el test falle) para no alterar las preferencias del desarrollador.
        private static readonly string[] MenuPrefKeys =
        {
            "escoba.setup.local.players", "escoba.setup.local.teams", "escoba.setup.local.seats",
            "escoba.setup.ai.players", "escoba.setup.ai.teams", "escoba.setup.ai.seats",
            "escoba.setup.name.0", "escoba.setup.name.1", "escoba.setup.name.2", "escoba.setup.name.3", "escoba.lastPreset",
            "escoba.language", "escoba.settings.musicVolume", "escoba.settings.sfxVolume", "escoba.settings.animationSpeed",
        };

        private Dictionary<string, string> _prefsBackup;

        [SetUp]
        public void BackupMenuPrefs()
        {
            _prefsBackup = BackupPrefs(MenuPrefKeys);
        }

        [TearDown]
        public void RestoreMenuPrefs()
        {
            if (_prefsBackup != null)
            {
                RestorePrefs(_prefsBackup);
                _prefsBackup = null;
            }
        }

        [UnityTest]
        public IEnumerator Menu_PlayerNames_AreUsedInGameAndRemembered()
        {
            string[] keys = MenuPrefKeys;
            {
                SceneManager.LoadScene(SceneNames.MainMenu);
                yield return null;
                yield return null;

                foreach (string key in keys)
                {
                    PlayerPrefs.DeleteKey(key);
                }

                var menu = Object.FindFirstObjectByType<MainMenuController>();
                menu.OpenSetup(true);
                yield return null;
                Assert.That(menu.Selection.PlayerCount, Is.EqualTo(2));
                Assert.That(menu.Selection.HumanCount, Is.EqualTo(2));
                FindInput("NameInput0").text = "  Laura  ";
                FindInput("NameInput1").text = "Pablo<b>";
                Assert.That(menu.Selection.Names[0], Is.EqualTo("Laura"));
                Assert.That(menu.Selection.Names[1], Is.EqualTo("Pablob"));

                menu.StartGame();
                yield return WaitFor(() => SceneManager.GetActiveScene().name == SceneNames.Game, 5f);
                yield return null;

                var table = Object.FindFirstObjectByType<TablePresenter>();
                Assert.That(table.SeatName(0), Is.EqualTo("Laura"));
                Assert.That(table.SeatName(1), Is.EqualTo("Pablob"));

                var pass = Object.FindFirstObjectByType<PassScreenPanel>(FindObjectsInactive.Include);
                yield return WaitFor(() => pass.IsShown, 30f);
                string title = pass.GetComponentInChildren<TMP_Text>().text;
                Assert.That(title, Does.Contain("Laura").Or.Contain("Pablob"), "la pantalla de pase usa el nombre elegido");

                // Al volver al menú, el modo "en este dispositivo" recuerda los nombres.
                SceneManager.LoadScene(SceneNames.MainMenu);
                yield return null;
                yield return null;
                menu = Object.FindFirstObjectByType<MainMenuController>();
                menu.OpenSetup(true);
                yield return null;
                Assert.That(FindInput("NameInput0").text, Is.EqualTo("Laura"));
            }
        }

        [UnityTest]
        public IEnumerator Game_LeavingToMenuMidTurnOrMidAnimation_DoesNotThrow()
        {
            yield return EnsureAppContext();

            // 1) Durante el turno del humano.
            GameController controller = null;
            yield return LoadGame(MatchConfig.Versus(DefaultPreset(), BotDifficulty.Hard, 4, true), false, 4f, c => controller = c);
            var table = Object.FindFirstObjectByType<TablePresenter>();
            yield return WaitFor(() => table.IsWaitingForHuman, 30f);
            Assert.That(table.IsWaitingForHuman, Is.True);
            ClickMenuButton();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == SceneNames.MainMenu, 5f);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNames.MainMenu));

            // 2) En mitad de las animaciones (partida automática a velocidad normal).
            yield return LoadGame(MatchConfig.Versus(DefaultPreset(), BotDifficulty.Medium, 3), true, 1f, c => controller = c);
            yield return new WaitForSeconds(1.5f);
            ClickMenuButton();
            yield return WaitFor(() => SceneManager.GetActiveScene().name == SceneNames.MainMenu, 5f);
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(SceneNames.MainMenu));
        }

        [UnityTest]
        public IEnumerator Menu_HowToPlayAndCredits_ShowEveryPageTranslated()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return null;
            yield return null;

            var menu = Object.FindFirstObjectByType<MainMenuController>();
            menu.OpenHowToPlay();
            yield return null;
            var howTo = Object.FindFirstObjectByType<HowToPlayPanel>();
            Assert.That(howTo != null && howTo.IsOpen, Is.True, "Cómo se juega se abre desde el menú");

            for (int page = 0; page < howTo.PageCount; page++)
            {
                Assert.That(howTo.PageIndex, Is.EqualTo(page));
                yield return null;
                AssertNoMissingTexts();
                Assert.That(howTo.VisibleCardSprites().All(s => s != null), Is.True, $"página {page + 1}: carta sin sprite");
                howTo.Next();
            }

            Assert.That(howTo.PageIndex, Is.EqualTo(howTo.PageCount - 1), "no pasa de la última página");
            howTo.Close();
            Assert.That(howTo.IsOpen, Is.False);

            menu.OpenCredits();
            yield return null;
            var credits = Object.FindFirstObjectByType<CreditsPanel>();
            Assert.That(credits != null && credits.IsOpen, Is.True);
            AssertNoMissingTexts();
            string text = credits.GetComponentsInChildren<TMP_Text>().Select(t => t.text).Aggregate(string.Empty, (a, b) => a + b);
            Assert.That(text, Does.Contain("Kevin MacLeod").And.Contain("creativecommons.org/licenses/by/4.0"), "crédito CC-BY de la música");
            credits.Close();
        }

        [UnityTest]
        public IEnumerator Effects_ParticlesExpireAndRoundSummaryRevealsAllRows()
        {
            yield return EnsureAppContext();
            GameController controller = null;
            yield return LoadGame(MatchConfig.Versus(DefaultPreset(), BotDifficulty.Easy), false, 40f, c => controller = c);

            // Partículas: aparecen y se apagan solas.
            var effects = Object.FindFirstObjectByType<UiParticles>();
            Assert.That(effects, Is.Not.Null, "la mesa tiene capa de partículas");
            effects.Burst(Vector2.zero, ParticleStyle.Sparkle, 20);
            effects.Burst(Vector2.zero, ParticleStyle.Confetti, 20);
            Assert.That(effects.ActiveCount, Is.GreaterThanOrEqualTo(40));
            yield return WaitFor(() => effects.ActiveCount == 0, 5f);
            Assert.That(effects.ActiveCount, Is.Zero, "las partículas desaparecen");

            // Jugar (siempre la primera jugada legal) hasta el resumen de la primera ronda.
            var table = Object.FindFirstObjectByType<TablePresenter>();
            var summary = Object.FindFirstObjectByType<RoundSummaryPanel>(FindObjectsInactive.Include);
            float deadline = Time.realtimeSinceStartup + 90f;
            while (!summary.gameObject.activeSelf && Time.realtimeSinceStartup < deadline)
            {
                yield return WaitFor(() => table.IsWaitingForHuman || summary.gameObject.activeSelf, 20f);
                if (table.IsWaitingForHuman)
                {
                    table.SubmitAutomatically(LegalMoves.For(controller.ViewFor(0))[0]);
                    yield return null;
                }
            }

            Assert.That(summary.gameObject.activeSelf, Is.True, "debe aparecer el resumen de ronda");
            yield return WaitFor(() => !summary.IsRevealing, 10f);
            Assert.That(summary.IsRevealing, Is.False, "la animación del resumen termina");
            foreach (TMP_Text text in summary.GetComponentsInChildren<TMP_Text>())
            {
                Assert.That(text.maxVisibleLines, Is.GreaterThanOrEqualTo(text.textInfo.lineCount), $"{text.name}: todas las filas visibles");
            }

            summary.Continue();
        }

        private static void AssertNoMissingTexts()
        {
            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                Assert.That(text.text, Does.Not.StartWith("["), $"texto sin traducir en {text.name}: {text.text}");
                Assert.That(text.text, Does.Not.Contain("{0}"), $"texto sin parámetros en {text.name}: {text.text}");
            }
        }

        /// <summary>Pulsa "Menú" (abre la pausa) y después "Salir al menú".</summary>
        private static void ClickMenuButton()
        {
            var menuButton = Object.FindFirstObjectByType<GameMenuButton>();
            Assert.That(menuButton, Is.Not.Null);
            menuButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            var pause = Object.FindFirstObjectByType<PausePanel>(FindObjectsInactive.Include);
            Assert.That(pause.IsOpen, Is.True, "el botón Menú abre la pausa");
            Assert.That(Time.timeScale, Is.EqualTo(0f), "la pausa detiene el tiempo de juego");
            pause.QuitToMenu();
        }

        [UnityTest]
        public IEnumerator Settings_ChangesApplyAndPersist_PauseFreezesAndResumes()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return null;
            yield return null;

            var menu = Object.FindFirstObjectByType<MainMenuController>();
            menu.OpenSettings();
            yield return null;
            var settings = Object.FindFirstObjectByType<SettingsPanel>();
            Assert.That(settings != null && settings.IsOpen, Is.True, "el panel de Ajustes se abre desde el menú");

            UserSettings user = AppContext.User;
            string previousLanguage = user.Language;
            float previousMusic = user.MusicVolume;
            AnimationSpeedLevel previousSpeed = user.Speed;
            user.SetMusicVolume(0.5f);
            Assert.That(AppContext.Audio.CurrentMusic, Is.EqualTo(AppContext.Settings.Audio.MenuMusic), "en el menú suena su tema");
            user.SetLanguage("en");
            user.SetSpeed(AnimationSpeedLevel.Fast);
            user.SetMusicVolume(0f);
            yield return null;
            Assert.That(Localization.Service.CurrentLanguage, Is.EqualTo("en"));
            Assert.That(PlayerPrefs.GetString("escoba.language"), Is.EqualTo("en"), "el idioma se guarda");
            Assert.That(PlayerPrefs.GetInt("escoba.settings.animationSpeed"), Is.EqualTo((int)AnimationSpeedLevel.Fast));
            Assert.That(AppContext.Audio.IsMusicPlaying, Is.False, "con volumen 0 la música se detiene");
            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                Assert.That(text.text, Does.Not.StartWith("["), $"texto sin traducir en {text.name}: {text.text}");
            }

            settings.Close();
            Assert.That(settings.IsOpen, Is.False);
            user.SetMusicVolume(0.5f);

            // En partida: la velocidad elegida se aplica y la pausa congela y reanuda el juego.
            yield return LoadGame(MatchConfig.Versus(DefaultPreset(), BotDifficulty.Easy), false, 1f, c => { });
            var table = Object.FindFirstObjectByType<TablePresenter>();
            Assert.That(table.Speed, Is.EqualTo(AppContext.Settings.AnimationSpeed * UserSettings.SpeedMultiplierOf(AnimationSpeedLevel.Fast)).Within(0.001f));
            Assert.That(System.Linq.Enumerable.Contains(AppContext.Settings.Audio.GameMusic, AppContext.Audio.CurrentMusic), Is.True,
                "en partida suena una de las pistas de partida");

            Object.FindFirstObjectByType<GameMenuButton>().GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            var pause = Object.FindFirstObjectByType<PausePanel>(FindObjectsInactive.Include);
            Assert.That(pause.IsOpen, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
            user.SetSpeed(AnimationSpeedLevel.Slow);
            Assert.That(table.Speed, Is.EqualTo(AppContext.Settings.AnimationSpeed * UserSettings.SpeedMultiplierOf(AnimationSpeedLevel.Slow)).Within(0.001f),
                "el cambio de velocidad se aplica en la partida en curso");
            pause.Resume();
            Assert.That(pause.IsOpen, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));

            // Deja la sesión como estaba (las preferencias guardadas las restaura TearDown).
            user.SetLanguage(previousLanguage);
            user.SetMusicVolume(previousMusic);
            user.SetSpeed(previousSpeed);
        }

        private static TMP_InputField FindInput(string name)
        {
            foreach (TMP_InputField input in Object.FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (input.name == name)
                {
                    return input;
                }
            }

            Assert.Fail($"No se encontró el campo {name}");
            return null;
        }

        private static Dictionary<string, string> BackupPrefs(IEnumerable<string> keys)
        {
            var backup = new Dictionary<string, string>();
            foreach (string key in keys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    backup[key] = PlayerPrefs.GetString(key, null) ?? PlayerPrefs.GetInt(key).ToString();
                }
                else
                {
                    backup[key] = null;
                }
            }

            return backup;
        }

        private static void RestorePrefs(Dictionary<string, string> backup)
        {
            foreach (KeyValuePair<string, string> entry in backup)
            {
                if (entry.Value == null)
                {
                    PlayerPrefs.DeleteKey(entry.Key);
                }
                else if (int.TryParse(entry.Value, out int number) && !entry.Key.Contains("seats") && !entry.Key.Contains("name"))
                {
                    PlayerPrefs.SetInt(entry.Key, number);
                }
                else
                {
                    PlayerPrefs.SetString(entry.Key, entry.Value);
                }
            }

            PlayerPrefs.Save();
        }

        private static bool SummaryVisible() => Object.FindFirstObjectByType<RoundSummaryPanel>() != null;
    }
}
