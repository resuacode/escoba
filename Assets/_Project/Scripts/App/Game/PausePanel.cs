using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Pausa de la partida (botón "Menú" de la mesa): continuar, abrir Ajustes o salir al menú
    /// principal, avisando de que la partida en curso se pierde. Mientras está abierto el tiempo
    /// de juego se detiene (<see cref="Time.timeScale"/> = 0): ni animaciones ni turnos de la IA.
    /// </summary>
    public sealed class PausePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _content;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private SettingsPanel _settings;
        [SerializeField] private Button _howToPlayButton;
        [SerializeField] private HowToPlayPanel _howToPlay;

        private bool _wired;

        /// <summary>Indica si la pausa está abierta.</summary>
        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Abre la pausa y detiene el tiempo de juego.</summary>
        public void Open()
        {
            Wire();
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _content.SetActive(true);
            _settings.Hide();
            _howToPlay.Hide();
            Time.timeScale = 0f;
        }

        /// <summary>Cierra la pausa y reanuda la partida.</summary>
        public void Resume()
        {
            _settings.Hide();
            _howToPlay.Hide();
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

        /// <summary>Abandona la partida y vuelve al menú principal.</summary>
        public void QuitToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        /// <summary>Estado inicial: cerrada.</summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Nunca dejar el juego congelado al cambiar de escena.
            Time.timeScale = 1f;
        }

        private void Wire()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            _resumeButton.onClick.AddListener(Resume);
            _quitButton.onClick.AddListener(QuitToMenu);
            _settingsButton.onClick.AddListener(() =>
            {
                _content.SetActive(false);
                _settings.Open(() => _content.SetActive(true));
            });
            _howToPlayButton.onClick.AddListener(() =>
            {
                _content.SetActive(false);
                _howToPlay.Open(() => _content.SetActive(true));
            });
        }
    }
}
