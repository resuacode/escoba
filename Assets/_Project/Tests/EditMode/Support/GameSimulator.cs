using System;
using System.Collections.Generic;
using Escoba.AI;
using Escoba.Core;

namespace Escoba.Tests.EditMode
{
    /// <summary>Juega partidas completas bot contra bot de forma determinista.</summary>
    internal static class GameSimulator
    {
        private const ulong BotStream = 7UL;
        private const int MaxCommands = 20000;

        /// <summary>
        /// Juega una partida completa. <paramref name="botForSeat"/> elige la estrategia de cada asiento
        /// (por defecto, Fácil). <paramref name="onStep"/> se llama tras cada comando (y tras el inicio)
        /// para comprobar invariantes.
        /// </summary>
        public static Result Play(RulesConfig rules, ulong seed, Action<GameEngine, IGameCommand, IReadOnlyList<GameEvent>> onStep = null,
            Func<int, IBotStrategy> botForSeat = null)
        {
            var engine = new GameEngine(rules, seed);
            var botRng = new Pcg32Random(seed, BotStream);
            var bots = new IBotStrategy[rules.PlayerCount];
            for (int seat = 0; seat < bots.Length; seat++)
            {
                bots[seat] = botForSeat != null ? botForSeat(seat) : new EasyBot();
            }

            var allEvents = new List<GameEvent>();
            IReadOnlyList<GameEvent> events = engine.StartGame();
            allEvents.AddRange(events);
            onStep?.Invoke(engine, null, events);

            int commands = 0;
            while (engine.State.Phase != GamePhase.GameOver)
            {
                if (++commands > MaxCommands)
                {
                    throw new InvalidOperationException($"La partida con semilla {seed} no termina.");
                }

                IGameCommand command = engine.State.Phase == GamePhase.RoundOver
                    ? new StartNextRoundCommand(0)
                    : (IGameCommand)bots[engine.State.CurrentSeat].ChooseCommand(engine.GetView(engine.State.CurrentSeat), botRng);

                events = engine.Apply(command);
                allEvents.AddRange(events);
                onStep?.Invoke(engine, command, events);
            }

            return new Result(engine, allEvents, commands);
        }

        internal sealed class Result
        {
            public Result(GameEngine engine, List<GameEvent> events, int commands)
            {
                Engine = engine;
                Events = events;
                Commands = commands;
            }

            public GameEngine Engine { get; }

            public List<GameEvent> Events { get; }

            public int Commands { get; }
        }
    }
}
