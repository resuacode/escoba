using System.Collections.Generic;
using Escoba.AI;
using Escoba.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Menú principal y panel "Nueva partida" (contra la IA). Los modos aún no disponibles se
    /// muestran desactivados con "Próximamente".
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Menú")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private Button _playAiButton;
        [SerializeField] private Button[] _comingSoonButtons = new Button[0];
        [SerializeField] private Button _languageButton;
        [SerializeField] private TMP_Text _languageLabel;
        [SerializeField] private Button _quitButton;

        [Header("Nueva partida")]
        [SerializeField] private GameObject _setupPanel;
        [SerializeField] private Button[] _presetButtons = new Button[0];
        [SerializeField] private Button[] _difficultyButtons = new Button[0];
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Color _selectedColor = new Color(0.95f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color _normalColor = Color.white;

        private int _presetIndex;
        private BotDifficulty _difficulty = BotDifficulty.Easy;

        private void Start()
        {
            GameSettings settings = AppContext.Settings;
            _presetIndex = Mathf.Clamp(AppContext.Store.GetInt(SettingsKeys.LastPreset, settings.DefaultPresetIndex), 0, settings.Presets.Count - 1);

            _playAiButton.onClick.AddListener(OpenSetup);
            foreach (Button button in _comingSoonButtons)
            {
                button.interactable = false;
            }

            _languageButton.onClick.AddListener(CycleLanguage);
            _quitButton.onClick.AddListener(Application.Quit);
            _quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer
                                             && !Application.isMobilePlatform);

            IReadOnlyList<RulesPreset> presets = settings.Presets;
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                int index = i;
                bool exists = i < presets.Count;
                _presetButtons[i].gameObject.SetActive(exists);
                if (exists)
                {
                    _presetButtons[i].GetComponentInChildren<LocalizedText>().Set(presets[i].NameKey);
                    _presetButtons[i].onClick.AddListener(() => SelectPreset(index));
                }
            }

            for (int i = 0; i < _difficultyButtons.Length; i++)
            {
                var difficulty = (BotDifficulty)i;
                _difficultyButtons[i].interactable = BotFactory.IsAvailable(difficulty);
                _difficultyButtons[i].onClick.AddListener(() => SelectDifficulty(difficulty));
            }

            _startButton.onClick.AddListener(StartGame);
            _backButton.onClick.AddListener(CloseSetup);

            Localization.Changed += RefreshLanguageLabel;
            RefreshLanguageLabel();
            RefreshSelection();
            CloseSetup();
        }

        private void OnDestroy()
        {
            Localization.Changed -= RefreshLanguageLabel;
        }

        private void OpenSetup()
        {
            _mainPanel.SetActive(false);
            _setupPanel.SetActive(true);
        }

        private void CloseSetup()
        {
            _setupPanel.SetActive(false);
            _mainPanel.SetActive(true);
        }

        private void SelectPreset(int index)
        {
            _presetIndex = index;
            RefreshSelection();
        }

        private void SelectDifficulty(BotDifficulty difficulty)
        {
            _difficulty = difficulty;
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].image.color = i == _presetIndex ? _selectedColor : _normalColor;
            }

            for (int i = 0; i < _difficultyButtons.Length; i++)
            {
                _difficultyButtons[i].image.color = i == (int)_difficulty ? _selectedColor : _normalColor;
            }
        }

        private void CycleLanguage()
        {
            LocalizationService service = Localization.Service;
            IReadOnlyList<string> languages = service.AvailableLanguages;
            int current = 0;
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i] == service.CurrentLanguage)
                {
                    current = i;
                }
            }

            AppContext.SetLanguage(languages[(current + 1) % languages.Count]);
        }

        private void RefreshLanguageLabel()
        {
            LocalizationService service = Localization.Service;
            if (service != null && _languageLabel != null)
            {
                _languageLabel.text = $"{service.Get("menu.language")}: {service.GetLanguageName(service.CurrentLanguage)}";
            }
        }

        private void StartGame()
        {
            GameSettings settings = AppContext.Settings;
            AppContext.Store.SetInt(SettingsKeys.LastPreset, _presetIndex);
            AppContext.Store.Save();
            AppContext.PendingMatch = new MatchConfig(settings.Presets[_presetIndex], _difficulty);
            SceneManager.LoadScene(SceneNames.Game);
        }
    }
}
