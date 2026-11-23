namespace Escoba.Core
{
    /// <summary>Resultado de validar un comando contra el estado actual.</summary>
    public readonly struct CommandValidation
    {
        /// <summary>Comando válido.</summary>
        public static readonly CommandValidation Ok = new CommandValidation(CommandError.None, null);

        private CommandValidation(CommandError error, string detail)
        {
            Error = error;
            Detail = detail;
        }

        /// <summary>Motivo del rechazo (<see cref="CommandError.None"/> si es válido).</summary>
        public CommandError Error { get; }

        /// <summary>Explicación para depuración (no se muestra al jugador).</summary>
        public string Detail { get; }

        /// <summary>Indica si el comando es válido.</summary>
        public bool IsValid => Error == CommandError.None;

        /// <summary>Crea un resultado de rechazo.</summary>
        public static CommandValidation Fail(CommandError error, string detail) => new CommandValidation(error, detail);

        public override string ToString() => IsValid ? "Ok" : $"{Error}: {Detail}";
    }
}
