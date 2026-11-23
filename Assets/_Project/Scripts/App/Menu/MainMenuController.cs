using System.Collections.Generic;
using System.Linq;
using Escoba.AI;
using Escoba.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Menú principal y panel "Nueva partida": número de jugadores (2-4), individual o por parejas
    /// (solo con 4), quién ocupa cada asiento (humano o IA), reglas y dificultad.
    /// "Jugar contra la IA" abre el panel con un humano y el resto IA; "Jugar en este dispositivo",
    /// con todos humanos (hot-seat).
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private const int MaxPlayers = 4;

        [Header("Menú")]
        [SerializeField] private GameObject _title;
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private Button _playAiButton;
        [SerializeField] private Button _playLocalButton;
        [SerializeField] private Button[] _comingSoonButtons = new Button[0];
        [SerializeField] private Button _languageButton;
        [SerializeField] private TMP_Text _languageLabel;
        [SerializeField] private Button _quitButton;

        [Header("Nueva partida")]
        [SerializeField] private GameObject _setupPanel;
        [Tooltip("Botones 2, 3 y 4 jugadores.")]
        [SerializeField] private Button[] _playerCountButtons = new Button[3];
        [Tooltip("Botones Individual y Por parejas.")]
        [SerializeField] private Button[] _modeButtons = new Button[2];
        [SerializeField] private GameObject[] _seatRows = new GameObject[MaxPlayers];
        [SerializeField] private LocalizedText[] _seatLabels = new LocalizedText[MaxPlayers];
        [SerializeField] private Button[] _seatButtons = new Button[MaxPlayers];
        [SerializeField] private Button[] _presetButtons = new Button[0];
        [SerializeField] private Button[] _difficultyButtons = new Button[0];
        [SerializeField] private GameObject _needHumanMessage;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Color _selectedColor = new Color(0.95f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color _normalColor = Color.white;

        private readonly SeatType[] _seats = new SeatType[MaxPlayers];
        private int _playerCount = 2;
        private bool _teams;
        private int _presetIndex;
        private BotDifficulty _difficulty = BotDifficulty.Easy;

        private void Start()
        {
            GameSettings settings = AppContext.Settings;
            _presetIndex = Mathf.Clamp(AppContext.Store.GetInt(SettingsKeys.LastPreset, settings.DefaultPresetIndex), 0, settings.Presets.Count - 1);

            _playAiButton.onClick.AddListener(() => OpenSetup(false));
            _playLocalButton.onClick.AddListener(() => OpenSetup(true));
            foreach (Button button in _comingSoonButtons)
            {
                button.interactable = false;
            }

            _languageButton.onClick.AddListener(CycleLanguage);
            _quitButton.onClick.AddListener(Application.Quit);
            _quitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer && !Application.isMobilePlatform);

            for (int i = 0; i < _playerCountButtons.Length; i++)
            {
                int count = i + 2;
                _playerCountButtons[i].onClick.AddListener(() => SetPlayerCount(count));
            }

            _modeButtons[0].onClick.AddListener(() => SetTeams(false));
            _modeButtons[1].onClick.AddListener(() => SetTeams(true));

            for (int i = 0; i < MaxPlayers; i++)
            {
                int seat = i;
                _seatLabels[i].Set("player.human", i + 1);
                _seatButtons[i].onClick.AddListener(() => ToggleSeat(seat));
            }

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
            CloseSetup();
        }

        private void OnDestroy()
        {
            Localization.Changed -= RefreshLanguageLabel;
        }

        /// <summary>Abre "Nueva partida": contra la IA (un humano) o en este dispositivo (todos humanos).</summary>
        public void OpenSetup(bool allHumans)
        {
            for (int i = 0; i < MaxPlayers; i++)
            {
                _seats[i] = i == 0 || allHumans ? SeatType.Human : SeatType.Bot;
            }

            _teams = false;
            _playerCount = 2;
            _title.SetActive(false);
            _mainPanel.SetActive(false);
            _setupPanel.SetActive(true);
            Refresh();
        }

        private void CloseSetup()
        {
            _setupPanel.SetActive(false);
            _mainPanel.SetActive(true);
            _title.SetActive(true);
        }

        private void SetPlayerCount(int count)
        {
            _playerCount = count;
            if (count != MaxPlayers)
            {
                _teams = false;
            }

            Refresh();
        }

        private void SetTeams(bool teams)
        {
            _teams = teams && _playerCount == MaxPlayers;
            Refresh();
        }

        private void ToggleSeat(int seat)
        {
            _seats[seat] = _seats[seat] == SeatType.Human ? SeatType.Bot : SeatType.Human;
            Refresh();
        }

        private void SelectPreset(int index)
        {
            _presetIndex = index;
            Refresh();
        }

        private void SelectDifficulty(BotDifficulty difficulty)
        {
            _difficulty = difficulty;
            Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < _playerCountButtons.Length; i++)
            {
                _playerCountButtons[i].image.color = i + 2 == _playerCount ? _selectedColor : _normalColor;
            }

            _modeButtons[0].image.color = _teams ? _normalColor : _selectedColor;
            _modeButtons[1].image.color = _teams ? _selectedColor : _normalColor;
            _modeButtons[1].interactable = _playerCount == MaxPlayers;

            for (int i = 0; i < MaxPlayers; i++)
            {
                _seatRows[i].SetActive(i < _playerCount);
                _seatButtons[i].GetComponentInChildren<LocalizedText>().Set(_seats[i] == SeatType.Human ? "setup.seat.human" : "setup.seat.bot");
                _seatButtons[i].image.color = _seats[i] == SeatType.Human ? _selectedColor : _normalColor;
            }

            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].image.color = i == _presetIndex ? _selectedColor : _normalColor;
            }

            for (int i = 0; i < _difficultyButtons.Length; i++)
            {
                _difficultyButtons[i].image.color = i == (int)_difficulty ? _selectedColor : _normalColor;
            }

            bool hasHuman = _seats.Take(_playerCount).Any(s => s == SeatType.Human);
            _needHumanMessage.SetActive(!hasHuman);
            _startButton.interactable = hasHuman;
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
            AppContext.PendingMatch = new MatchConfig(settings.Presets[_presetIndex], _difficulty, _seats.Take(_playerCount).ToArray(), _teams);
            SceneManager.LoadScene(SceneNames.Game);
        }
    }
}
