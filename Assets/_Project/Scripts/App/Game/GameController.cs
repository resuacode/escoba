using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Escoba.AI;
using Escoba.Core;
using Escoba.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Escoba.App
{
    /// <summary>
    /// Bucle de partida de la escena Game. Crea el motor y un <see cref="IPlayerController"/> por
    /// asiento (humano local o IA), pide jugadas por turnos, las valida y aplica en el motor y
    /// entrega los eventos a la mesa. Con varios humanos (hot-seat) muestra la pantalla de pase
    /// antes de cada turno humano y oculta la mano entre turnos.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        private const ulong BotRandomStream = 99UL;

        [SerializeField] private TablePresenter _table;
        [SerializeField] private RoundSummaryPanel _roundSummary;
        [SerializeField] private GameOverPanel _gameOver;
        [SerializeField] private PassScreenPanel _passScreen;

        [Header("Depuración")]
        [Tooltip("Todos los asientos los juega la IA (pruebas automáticas).")]
        [SerializeField] private bool _autoPlayAllSeats;

        [Tooltip("Multiplicador de velocidad de animaciones y esperas (pruebas).")]
        [SerializeField, Min(0.1f)] private float _debugSpeed = 1f;

        private GameEngine _engine;
        private IPlayerController[] _controllers;
        private MatchConfig _config;
        private Pcg32Random _pacingRng;
        private string[] _seatNames;
        private string[] _sideNames;
        private string[] _scoreboardNames;
        private bool _hotSeat;
        private int _localSeat;
        private string _lastPlay;

        /// <summary>Estado de la partida en curso (solo lectura, para tests y depuración).</summary>
        public GameState State => _engine?.State;

        /// <summary>Semilla de la partida (para reproducir errores).</summary>
        public ulong Seed { get; private set; }

        /// <summary>Configuración de la partida en curso.</summary>
        public MatchConfig Config => _config;

        /// <summary>Vista filtrada de un asiento (pruebas y depuración).</summary>
        public PlayerView ViewFor(int seat) => _engine.GetView(seat);

        /// <summary>Activa el juego automático de todos los asientos (llamar antes de Start).</summary>
        public void ConfigureAutoPlay(bool autoPlay, float speed)
        {
            _autoPlayAllSeats = autoPlay;
            _debugSpeed = Mathf.Max(0.1f, speed);
        }

        /// <summary>Cambia la velocidad de animaciones y esperas en cualquier momento (pruebas).</summary>
        public void SetDebugSpeed(float speed)
        {
            _debugSpeed = Mathf.Max(0.1f, speed);
            ApplySpeed();
        }

        private void ApplySpeed()
        {
            if (_table != null)
            {
                _table.Speed = AppContext.AnimationSpeed * _debugSpeed;
            }
        }

        private void Start()
        {
            GameSettings settings = AppContext.Settings;
            _config = AppContext.PendingMatch
                      ?? MatchConfig.Versus(settings.Presets[settings.DefaultPresetIndex], BotDifficulty.Easy);
            Seed = _config.Seed ?? (ulong)DateTime.UtcNow.Ticks;
            Debug.Log($"[Escoba] Nueva partida ({_config.PlayerCount} jugadores, parejas: {_config.Teams}, humanos: {_config.HumanCount}). Semilla: {Seed}");

            RulesConfig rules = _config.Preset.ToRulesConfig(_config.PlayerCount, _config.Teams);
            _engine = new GameEngine(rules, Seed);
            _pacingRng = new Pcg32Random(Seed, BotRandomStream + 1);

            _controllers = new IPlayerController[_config.PlayerCount];
            for (int seat = 0; seat < _controllers.Length; seat++)
            {
                bool isBot = _autoPlayAllSeats || _config.Seats[seat] == SeatType.Bot;
                _controllers[seat] = isBot
                    ? new AIController(BotFactory.Create(_config.Difficulty), new Pcg32Random(Seed, BotRandomStream + (ulong)seat + 2))
                    : (IPlayerController)new HumanLocalController(_table);
            }

            _seatNames = PlayerNaming.SeatNames(_config.Seats, _config.PlayerNames);
            _sideNames = PlayerNaming.SideNames(_seatNames, _config.Teams);
            _scoreboardNames = _config.Teams
                ? _sideNames.Select((name, side) => $"{name} <size=65%>({PlayerNaming.TeamMembers(_seatNames, side)})</size>").ToArray()
                : _sideNames;
            _hotSeat = !_autoPlayAllSeats && _config.IsHotSeat;
            _localSeat = Mathf.Max(0, _config.Seats.ToList().IndexOf(SeatType.Human));

            _roundSummary.Hide();
            _gameOver.Hide();
            _passScreen.Hide();
            ApplySpeed();
            if (AppContext.User != null)
            {
                AppContext.User.Changed += ApplySpeed;
            }

            if (AppContext.Audio != null)
            {
                AppContext.Audio.PlayGameMusic();
            }

            _table.Initialize(settings.CardSkin, rules, _seatNames, _localSeat, !_hotSeat);
            _table.UpdateScores(_scoreboardNames, Scores());
            StartCoroutine(RunGame());
        }

        private void OnDestroy()
        {
            if (AppContext.User != null)
            {
                AppContext.User.Changed -= ApplySpeed;
            }

            if (_controllers == null)
            {
                return;
            }

            foreach (IPlayerController controller in _controllers)
            {
                controller.CancelRequest();
            }
        }

        private IEnumerator RunGame()
        {
            yield return PlayEvents(_engine.StartGame());

            while (true)
            {
                GameState state = _engine.State;
                if (state.Phase == GamePhase.GameOver)
                {
                    ShowGameOver();
                    yield break;
                }

                if (state.Phase == GamePhase.RoundOver)
                {
                    yield return ShowRoundSummary();
                    _lastPlay = null;
                    yield return PlayEvents(_engine.Apply(new StartNextRoundCommand(_localSeat)));
                    continue;
                }

                int seat = state.CurrentSeat;
                IPlayerController controller = _controllers[seat];
                if (controller.IsLocalHuman && _hotSeat)
                {
                    yield return PassDeviceTo(seat);
                }
                else if (!controller.IsLocalHuman)
                {
                    _table.ShowThinking(seat);
                    yield return new WaitForSeconds(ThinkSeconds());
                }

                PlayCardCommand command = null;
                System.Diagnostics.Stopwatch watch = controller.IsLocalHuman ? null : System.Diagnostics.Stopwatch.StartNew();
                controller.RequestCommand(_engine.GetView(seat), c => command = c);
                if (watch != null)
                {
                    // La IA responde en el acto: así se mide su tiempo de decisión (medidor oculto).
                    PerfStats.RecordBotDecision(watch.Elapsed.TotalMilliseconds);
                }

                yield return new WaitUntil(() => command != null);

                CommandValidation validation = _engine.Validate(command);
                if (!validation.IsValid)
                {
                    Debug.LogError($"[Escoba] Jugada rechazada ({command}): {validation}");
                    continue;
                }

                if (controller.IsLocalHuman && _hotSeat)
                {
                    _table.SetViewerHandRevealed(false);
                }

                IReadOnlyList<GameEvent> events = _engine.Apply(command);
                _lastPlay = DescribePlay(events) ?? _lastPlay;
                yield return PlayEvents(events);
            }
        }

        private IEnumerator PassDeviceTo(int seat)
        {
            bool confirmed = false;
            _passScreen.Show(_seatNames[seat], _lastPlay, () => confirmed = true);
            yield return new WaitUntil(() => confirmed);
            _table.SetViewer(_engine.GetView(seat), true);
        }

        private IEnumerator PlayEvents(IReadOnlyList<GameEvent> events)
        {
            foreach (GameEvent gameEvent in events)
            {
                yield return _table.Play(gameEvent);
            }

            _table.UpdateScores(_scoreboardNames, Scores());

            string inconsistency = _table.FindInconsistency(_engine.GetView(_table.ViewerSeat));
            if (inconsistency != null)
            {
                Debug.LogError($"[Escoba] La mesa no coincide con el motor: {inconsistency} (semilla {Seed})");
            }
        }

        private string DescribePlay(IReadOnlyList<GameEvent> events)
        {
            foreach (GameEvent gameEvent in events)
            {
                switch (gameEvent)
                {
                    case CardPlayedEvent played:
                        return Localization.Get("pass.last_drop", _seatNames[played.Seat], CardNames.Get(played.Card));
                    case CardsCapturedEvent captured when captured.IsEscoba:
                        return Localization.Get("pass.last_escoba", _seatNames[captured.Seat], CardNames.Get(captured.PlayedCard));
                    case CardsCapturedEvent captured:
                        return Localization.Get("pass.last_capture", _seatNames[captured.Seat], CardNames.Get(captured.PlayedCard),
                            Localization.GetPlural("game.cards", captured.Captured.Count));
                }
            }

            return null;
        }

        private IEnumerator ShowRoundSummary()
        {
            if (_autoPlayAllSeats)
            {
                yield break;
            }

            GameState state = _engine.State;
            bool next = false;
            int[] columnSides = SideOrder();
            _roundSummary.Speed = _table.Speed;
            _roundSummary.Show(state.RoundNumber, state.LastRoundScore, Scores(), columnSides,
                columnSides.Select(s => _sideNames[s]).ToArray(), state.Rules, () => next = true);
            yield return new WaitUntil(() => next);
        }

        private void ShowGameOver()
        {
            GameState state = _engine.State;
            int[] order = SideOrder();
            int[] scores = Scores();
            bool singleHuman = !_autoPlayAllSeats && _config.HumanCount == 1;
            bool localWon = singleHuman && state.WinnerSide == state.SideOfSeat(_localSeat);
            string winner = _sideNames[state.WinnerSide];
            Sounds.Play(SoundCue.GameOver);
            string title = localWon
                ? Localization.Get(_config.Teams ? "gameover.team_you_win" : "gameover.you_win")
                : Localization.Get("gameover.winner", winner);
            // Confeti si gana un humano (contra la IA, solo si ganas tú o tu pareja).
            bool humanWon = !_autoPlayAllSeats && Enumerable.Range(0, _config.PlayerCount)
                .Any(seat => _config.Seats[seat] == SeatType.Human && state.SideOfSeat(seat) == state.WinnerSide);
            _gameOver.Show(
                title,
                order.Select(s => _sideNames[s]).ToArray(),
                order.Select(s => scores[s]).ToArray(),
                Rematch,
                GoToMenu,
                humanWon);
        }

        /// <summary>Bandos en orden de columna: con un solo humano, el suyo primero.</summary>
        private int[] SideOrder()
        {
            GameState state = _engine.State;
            int localSide = state.SideOfSeat(_localSeat);
            bool singleHuman = _config.HumanCount == 1;
            return Enumerable.Range(0, state.SideCount)
                .OrderBy(s => singleHuman && s == localSide ? 0 : 1)
                .ThenBy(s => s)
                .ToArray();
        }

        private int[] Scores() => Enumerable.Range(0, _engine.State.SideCount).Select(s => _engine.State.GetSide(s).Score).ToArray();

        private float ThinkSeconds()
        {
            GameSettings settings = AppContext.Settings;
            float t = _pacingRng.Next(1001) / 1000f;
            return Mathf.Lerp(settings.BotThinkSecondsMin, settings.BotThinkSecondsMax, t) / (_table.Speed > 0f ? _table.Speed : 1f);
        }

        private void Rematch()
        {
            AppContext.PendingMatch = _config.WithSeed(null);
            SceneManager.LoadScene(SceneNames.Game);
        }

        private void GoToMenu()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
