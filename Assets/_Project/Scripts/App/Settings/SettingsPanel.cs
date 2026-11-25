using System;
using System.Collections.Generic;
using Escoba.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Escoba.App
{
    /// <summary>
    /// Panel "Ajustes" (menú principal y pausa de la partida): idioma, volumen de música y de
    /// efectos, velocidad de las animaciones y pantalla completa (solo escritorio y web).
    /// Cada cambio se aplica y se guarda al momento en <see cref="UserSettings"/>.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        private const int VolumeSteps = 10;

        [Tooltip("Un botón por idioma, en el orden de UserSettings.LanguageOrder.")]
        [SerializeField] private Button[] _languageButtons = new Button[0];
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [Tooltip("Botones Lenta, Normal y Rápida.")]
        [SerializeField] private Button[] _speedButtons = new Button[3];
        [SerializeField] private GameObject _fullscreenRow;
        [SerializeField] private Button _fullscreenButton;
        [SerializeField] private LocalizedText _fullscreenLabel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Color _selectedColor = new Color(0.95f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color _normalColor = Color.white;

        private readonly List<string> _languages = new List<string>();
        private Action _onClosed;
        private bool _wired;
        private bool _fullscreen;

        /// <summary>Indica si el panel está abierto.</summary>
        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Abre el panel; <paramref name="onClosed"/> se llama al cerrarlo.</summary>
        public void Open(Action onClosed = null)
        {
            Wire();
            _onClosed = onClosed;
            _fullscreen = Screen.fullScreen;
            gameObject.SetActive(true);
            Refresh();
        }

        /// <summary>Cierra el panel.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            Action callback = _onClosed;
            _onClosed = null;
            callback?.Invoke();
        }

        /// <summary>Oculta el panel sin avisar (estado inicial de la escena).</summary>
        public void Hide()
        {
            _onClosed = null;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (!_wired)
            {
                return;
            }

            Localization.Changed -= Refresh;
            if (AppContext.User != null)
            {
                AppContext.User.Changed -= Refresh;
            }
        }

        private void Wire()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            IReadOnlyList<string> available = Localization.Service != null ? Localization.Service.AvailableLanguages : Array.Empty<string>();
            foreach (string code in UserSettings.LanguageOrder)
            {
                if (ContainsLanguage(available, code))
                {
                    _languages.Add(code);
                }
            }

            for (int i = 0; i < _languageButtons.Length; i++)
            {
                bool exists = i < _languages.Count;
                _languageButtons[i].gameObject.SetActive(exists);
                if (exists)
                {
                    string code = _languages[i];
                    _languageButtons[i].onClick.AddListener(() => AppContext.User.SetLanguage(code));
                }
            }

            ConfigureSlider(_musicSlider, volume => AppContext.User.SetMusicVolume(volume));
            ConfigureSlider(_sfxSlider, volume =>
            {
                AppContext.User.SetSfxVolume(volume);
                Sounds.Play(SoundCue.Click);
            });

            for (int i = 0; i < _speedButtons.Length; i++)
            {
                var speed = (AnimationSpeedLevel)i;
                _speedButtons[i].onClick.AddListener(() => AppContext.User.SetSpeed(speed));
            }

            bool canFullscreen = !Application.isMobilePlatform;
            _fullscreenRow.SetActive(canFullscreen);
            _fullscreenButton.onClick.AddListener(ToggleFullscreen);
            _closeButton.onClick.AddListener(Close);
            AppContext.User.Changed += Refresh;
            Localization.Changed += Refresh;
        }

        private void OnEnable()
        {
            if (_wired)
            {
                Refresh();
            }
        }

        private static void ConfigureSlider(Slider slider, Action<float> onChanged)
        {
            // Pasos enteros: cada cambio se guarda, y en WebGL guardar es relativamente costoso.
            slider.minValue = 0f;
            slider.maxValue = VolumeSteps;
            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener(value => onChanged(value / VolumeSteps));
        }

        private void ToggleFullscreen()
        {
            // Screen.fullScreen se actualiza en el siguiente frame: se muestra el valor pedido.
            _fullscreen = !_fullscreen;
            Screen.fullScreen = _fullscreen;
            Refresh();
        }

        private void Refresh()
        {
            UserSettings user = AppContext.User;
            if (user == null || this == null)
            {
                return;
            }

            LocalizationService service = Localization.Service;
            for (int i = 0; i < _languages.Count && i < _languageButtons.Length; i++)
            {
                string code = _languages[i];
                _languageButtons[i].image.color = code == user.Language ? _selectedColor : _normalColor;
                TMP_Text label = _languageButtons[i].GetComponentInChildren<TMP_Text>();
                if (label != null && service != null)
                {
                    label.text = service.GetLanguageName(code);
                }
            }

            _musicSlider.SetValueWithoutNotify(Mathf.Round(user.MusicVolume * VolumeSteps));
            _sfxSlider.SetValueWithoutNotify(Mathf.Round(user.SfxVolume * VolumeSteps));
            for (int i = 0; i < _speedButtons.Length; i++)
            {
                _speedButtons[i].image.color = i == (int)user.Speed ? _selectedColor : _normalColor;
            }

            _fullscreenLabel.Set(_fullscreen ? "settings.fullscreen.on" : "settings.fullscreen.off");
            _fullscreenButton.image.color = _fullscreen ? _selectedColor : _normalColor;
        }

        private static bool ContainsLanguage(IReadOnlyList<string> available, string code)
        {
            for (int i = 0; i < available.Count; i++)
            {
                if (available[i] == code)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
