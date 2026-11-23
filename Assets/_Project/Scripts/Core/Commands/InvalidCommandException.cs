using System;

namespace Escoba.Core
{
    /// <summary>Se lanza al aplicar un comando que no supera la validación.</summary>
    public sealed class InvalidCommandException : Exception
    {
        public InvalidCommandException(CommandValidation validation)
            : base($"Comando no válido: {validation}")
        {
            Error = validation.Error;
        }

        /// <summary>Motivo del rechazo.</summary>
        public CommandError Error { get; }
    }
}
