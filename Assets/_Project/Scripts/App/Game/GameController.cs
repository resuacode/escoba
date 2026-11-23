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
    /// asiento, pide jugadas por turnos, las valida y aplica en el motor y entrega los eventos a la
    /// mesa para animarlos. PvE, hot-seat y online comparten este mismo bucle.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        private const int LocalSeat = 0;
        private const ulong BotRandomStream = 99UL;

        [SerializeField] private TablePresenter _table;
        [SerializeField] private RoundSummaryPanel _roundSummary;
        [SerializeField] private GameOverPanel _gameOver;

        [Header("Depuración")]
        [Tooltip("El asiento local también lo juega la IA (pruebas automáticas).")]
        [SerializeField] private bool _autoPlayLocalSeat;

        [Tooltip("Multiplicador de velocidad de animaciones y esperas (pruebas).")]
        [SerializeField, Min(0.1f)] private float _debugSpeed = 1f;

        private GameEngine _engine;
        private IPlayerController[] _controllers;
        private MatchConfig _config;
        private Pcg32Random _pacingRng;

        /// <summary>Estado de la partida en curso (solo lectura, para tests y depuración).</summary>
        public GameState State => _engine?.State;

        /// <summary>Semilla de la partida (para reproducir errores).</summary>
        public ulong Seed { get; private set; }

        /// <summary>Activa el juego automático del asiento local (antes de Start).</summary>
        public void ConfigureAutoPlay(bool autoPlay, float speed)
        {
            _autoPlayLocalSeat = autoPlay;
            _debugSpeed = Mathf.Max(0.1f, speed);
        }

        private void Start()
        {
            GameSettings settings = AppContext.Settings;
            _config = AppContext.PendingMatch
                      ?? new MatchConfig(settings.Presets[settings.DefaultPresetIndex], BotDifficulty.Easy);
            Seed = _config.Seed ?? (ulong)DateTime.UtcNow.Ticks;
            Debug.Log($"[Escoba] Nueva partida. Semilla: {Seed}");

            RulesConfig rules = _config.Preset.ToRulesConfig(_config.PlayerCount, false);
            _engine = new GameEngine(rules, Seed);
            _pacingRng = new Pcg32Random(Seed, BotRandomStream + 1);

            _controllers = new IPlayerController[_config.PlayerCount];
            for (int seat = 0; seat < _controllers.Length; seat++)
            {
                bool isBot = seat != LocalSeat || _autoPlayLocalSeat;
                _controllers[seat] = isBot
                    ? new AIController(BotFactory.Create(_config.Difficulty), new Pcg32Random(Seed, BotRandomStream + (ulong)seat + 2))
                    : (IPlayerController)new HumanLocalController(_table);
            }

            _roundSummary.Hide();
            _gameOver.Hide();
            _table.Speed = settings.AnimationSpeed * _debugSpeed;
            _table.Initialize(settings.CardSkin, rules, LocalSeat);
            StartCoroutine(RunGame());
        }

        private void OnDestroy()
        {
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
                    yield return PlayEvents(_engine.Apply(new StartNextRoundCommand(LocalSeat)));
                    continue;
                }

                int seat = state.CurrentSeat;
                IPlayerController controller = _controllers[seat];
                if (!controller.IsLocalHuman)
                {
                    _table.ShowThinking(seat);
                    yield return new WaitForSeconds(ThinkSeconds());
                }

                PlayCardCommand command = null;
                controller.RequestCommand(_engine.GetView(seat), c => command = c);
                yield return new WaitUntil(() => command != null);

                CommandValidation validation = _engine.Validate(command);
                if (!validation.IsValid)
                {
                    Debug.LogError($"[Escoba] Jugada rechazada ({command}): {validation}");
                    continue;
                }

                yield return PlayEvents(_engine.Apply(command));
            }
        }

        private IEnumerator PlayEvents(IReadOnlyList<GameEvent> events)
        {
            foreach (GameEvent gameEvent in events)
            {
                yield return _table.Play(gameEvent);
            }

            _table.UpdateScores(Scores());

            string inconsistency = _table.FindInconsistency(_engine.GetView(LocalSeat));
            if (inconsistency != null)
            {
                Debug.LogError($"[Escoba] La mesa no coincide con el motor: {inconsistency} (semilla {Seed})");
            }
        }

        private IEnumerator ShowRoundSummary()
        {
            if (_autoPlayLocalSeat)
            {
                yield break;
            }

            GameState state = _engine.State;
            bool next = false;
            int localSide = state.SideOfSeat(LocalSeat);
            int[] columnSides = Enumerable.Range(0, state.SideCount).OrderBy(s => s == localSide ? 0 : 1).ToArray();
            string[] names = columnSides.Select(SideName).ToArray();
            _roundSummary.Show(state.RoundNumber, state.LastRoundScore, Scores(), columnSides, names, state.Rules, () => next = true);
            yield return new WaitUntil(() => next);
        }

        private void ShowGameOver()
        {
            GameState state = _engine.State;
            int localSide = state.SideOfSeat(LocalSeat);
            int[] order = Enumerable.Range(0, state.SideCount).OrderBy(s => s == localSide ? 0 : 1).ToArray();
            int[] scores = Scores();
            _gameOver.Show(
                state.WinnerSide == localSide,
                SideName(state.WinnerSide),
                order.Select(SideName).ToArray(),
                order.Select(s => scores[s]).ToArray(),
                Rematch,
                GoToMenu);
        }

        private string SideName(int side) => Localization.Get(side == _engine.State.SideOfSeat(LocalSeat) ? "game.you" : "game.opponent");

        private int[] Scores() => Enumerable.Range(0, _engine.State.SideCount).Select(s => _engine.State.GetSide(s).Score).ToArray();

        private float ThinkSeconds()
        {
            GameSettings settings = AppContext.Settings;
            float t = _pacingRng.Next(1001) / 1000f;
            return Mathf.Lerp(settings.BotThinkSecondsMin, settings.BotThinkSecondsMax, t) / (_table.Speed > 0f ? _table.Speed : 1f);
        }

        private void Rematch()
        {
            AppContext.PendingMatch = new MatchConfig(_config.Preset, _config.Difficulty);
            SceneManager.LoadScene(SceneNames.Game);
        }

        private void GoToMenu()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
