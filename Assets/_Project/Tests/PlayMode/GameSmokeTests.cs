using System.Collections;
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
        private const float GameTimeoutSeconds = 120f;

        [UnityTest]
        public IEnumerator GameScene_BotVersusBot_PlaysFullGameWithoutErrors()
        {
            GameController controller = null;
            void OnLoaded(Scene scene, LoadSceneMode mode)
            {
                controller = Object.FindFirstObjectByType<GameController>();
                controller.ConfigureAutoPlay(true, 25f);
            }

            SceneManager.sceneLoaded += OnLoaded;
            SceneManager.LoadScene(SceneNames.Game);
            yield return null;
            SceneManager.sceneLoaded -= OnLoaded;
            Assert.That(controller, Is.Not.Null);

            float start = Time.realtimeSinceStartup;
            yield return new WaitUntil(() =>
                (controller.State != null && controller.State.Phase == GamePhase.GameOver)
                || Time.realtimeSinceStartup - start > GameTimeoutSeconds);

            Assert.That(controller.State.Phase, Is.EqualTo(GamePhase.GameOver), $"semilla {controller.Seed}: la partida no terminó a tiempo");
            Assert.That(controller.State.GetSide(controller.State.WinnerSide).Score, Is.GreaterThanOrEqualTo(controller.State.Rules.TargetScore));

            // El panel aparece cuando terminan las animaciones de la última jugada.
            float panelStart = Time.realtimeSinceStartup;
            yield return new WaitUntil(() =>
                Object.FindFirstObjectByType<GameOverPanel>() != null || Time.realtimeSinceStartup - panelStart > 10f);
            Assert.That(Object.FindFirstObjectByType<GameOverPanel>(), Is.Not.Null, "debe mostrarse la pantalla de fin");
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
            }
        }
    }
}
