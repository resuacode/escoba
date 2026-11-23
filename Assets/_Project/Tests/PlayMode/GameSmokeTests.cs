using System.Collections;
using System.Collections.Generic;
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

        // { jugadores, parejas (0/1) }
        private static readonly int[][] BotConfigurations = { new[] { 2, 0 }, new[] { 3, 0 }, new[] { 4, 0 }, new[] { 4, 1 } };

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
            yield return EnsureAppContext();
            GameController controller = null;
            yield return LoadGame(MatchConfig.Versus(DefaultPreset(), BotDifficulty.Easy, players, teams), true, 40f, c => controller = c);

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

        [UnityTest]
        public IEnumerator Menu_PlayerNames_AreUsedInGameAndRemembered()
        {
            string[] keys =
            {
                "escoba.setup.local.players", "escoba.setup.local.teams", "escoba.setup.local.seats",
                "escoba.setup.name.0", "escoba.setup.name.1", "escoba.setup.name.2", "escoba.setup.name.3", "escoba.lastPreset",
            };
            Dictionary<string, string> backup = BackupPrefs(keys);
            try
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
            finally
            {
                RestorePrefs(backup);
            }
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
