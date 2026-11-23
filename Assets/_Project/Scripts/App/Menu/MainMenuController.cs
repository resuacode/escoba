using System.Collections.Generic;
using System.Linq;
using Escoba.AI;
using Escoba.Core;
using Escoba.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Menú principal y panel "Nueva partida": número de jugadores (2-4), individual o por parejas
    /// (solo con 4), quién ocupa cada asiento (humano o IA) y su nombre, reglas y dificultad.
    /// "Jugar contra la IA" y "Jugar en este dispositivo" recuerdan cada uno su última configuración.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        private const int MaxPlayers = SetupSelection.MaxPlayers;

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
        [Tooltip("Campo de nombre de cada asiento (visible si es humano).")]
        [SerializeField] private TMP_InputField[] _nameInputs = new TMP_InputField[MaxPlayers];
        [Tooltip("Texto de ayuda del campo de nombre (nombre por defecto).")]
        [SerializeField] private LocalizedText[] _namePlaceholders = new LocalizedText[MaxPlayers];
        [Tooltip("Nombre del asiento cuando es IA.")]
        [SerializeField] private LocalizedText[] _botLabels = new LocalizedText[MaxPlayers];
        [SerializeField] private Button[] _seatButtons = new Button[MaxPlayers];
        [SerializeField] private Button[] _presetButtons = new Button[0];
        [SerializeField] private Button[] _difficultyButtons = new Button[0];
        [SerializeField] private GameObject _needHumanMessage;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Color _selectedColor = new Color(0.95f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color _normalColor = Color.white;

        private SetupSelection _selection = SetupSelection.Default(false);
        private bool _allHumansMode;
        private int _presetIndex;
        private BotDifficulty _difficulty = BotDifficulty.Easy;

        /// <summary>Selección actual del panel (pruebas).</summary>
        public SetupSelection Selection => _selection;

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
                _botLabels[i].Set("player.bot", i + 1);
                _seatButtons[i].onClick.AddListener(() => ToggleSeat(seat));
                TMP_InputField input = _nameInputs[i];
                input.characterLimit = PlayerNameRules.MaxLength;
                input.onValidateInput = (text, index, c) => PlayerNameRules.IsAllowedChar(c) ? c : '\0';
                input.onValueChanged.AddListener(value => _selection.Names[seat] = PlayerNameRules.Sanitize(value));
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

            Localization.Changed += OnLanguageChanged;
            RefreshLanguageLabel();
            CloseSetup();
        }

        private void OnDestroy()
        {
            Localization.Changed -= OnLanguageChanged;
        }

        /// <summary>Abre "Nueva partida" con la última configuración del modo elegido.</summary>
        public void OpenSetup(bool allHumans)
        {
            _allHumansMode = allHumans;
            _selection = SetupPreferences.Load(AppContext.Store, allHumans);
            for (int i = 0; i < MaxPlayers; i++)
            {
                _nameInputs[i].SetTextWithoutNotify(_selection.Names[i] ?? string.Empty);
            }

            _title.SetActive(false);
            _mainPanel.SetActive(false);
            _setupPanel.SetActive(true);
            Refresh();
        }

        /// <summary>Empieza la partida con la selección actual (si hay al menos un humano).</summary>
        public void StartGame()
        {
            if (_selection.HumanCount == 0)
            {
                return;
            }

            GameSettings settings = AppContext.Settings;
            AppContext.Store.SetInt(SettingsKeys.LastPreset, _presetIndex);
            SetupPreferences.Save(AppContext.Store, _allHumansMode, _selection);
            int count = _selection.PlayerCount;
            AppContext.PendingMatch = new MatchConfig(
                settings.Presets[_presetIndex],
                _difficulty,
                _selection.Seats.Take(count).ToArray(),
                _selection.Teams,
                null,
                _selection.Names.Take(count).ToArray());
            SceneManager.LoadScene(SceneNames.Game);
        }

        private void CloseSetup()
        {
            _setupPanel.SetActive(false);
            _mainPanel.SetActive(true);
            _title.SetActive(true);
        }

        private void SetPlayerCount(int count)
        {
            _selection.PlayerCount = count;
            if (count != MaxPlayers)
            {
                _selection.Teams = false;
            }

            Refresh();
        }

        private void SetTeams(bool teams)
        {
            _selection.Teams = teams && _selection.PlayerCount == MaxPlayers;
            Refresh();
        }

        private void ToggleSeat(int seat)
        {
            _selection.Seats[seat] = _selection.Seats[seat] == SeatType.Human ? SeatType.Bot : SeatType.Human;
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
                _playerCountButtons[i].image.color = i + 2 == _selection.PlayerCount ? _selectedColor : _normalColor;
            }

            _modeButtons[0].image.color = _selection.Teams ? _normalColor : _selectedColor;
            _modeButtons[1].image.color = _selection.Teams ? _selectedColor : _normalColor;
            _modeButtons[1].interactable = _selection.PlayerCount == MaxPlayers;

            int humans = _selection.HumanCount;
            for (int i = 0; i < MaxPlayers; i++)
            {
                bool human = _selection.Seats[i] == SeatType.Human;
                _seatRows[i].SetActive(i < _selection.PlayerCount);
                _nameInputs[i].gameObject.SetActive(human);
                _botLabels[i].gameObject.SetActive(!human);
                _namePlaceholders[i].Set(humans == 1 ? "game.you" : "player.human", i + 1);
                _seatButtons[i].GetComponentInChildren<LocalizedText>().Set(human ? "setup.seat.human" : "setup.seat.bot");
                _seatButtons[i].image.color = human ? _selectedColor : _normalColor;
            }

            for (int i = 0; i < _presetButtons.Length; i++)
            {
                _presetButtons[i].image.color = i == _presetIndex ? _selectedColor : _normalColor;
            }

            for (int i = 0; i < _difficultyButtons.Length; i++)
            {
                _difficultyButtons[i].image.color = i == (int)_difficulty ? _selectedColor : _normalColor;
            }

            bool hasHuman = humans > 0;
            _needHumanMessage.SetActive(!hasHuman);
            _startButton.interactable = hasHuman;
        }

        private void OnLanguageChanged()
        {
            RefreshLanguageLabel();
            if (_setupPanel.activeSelf)
            {
                Refresh();
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
    }
}
