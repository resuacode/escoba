namespace Escoba.Core
{
    /// <summary>Motivo por el que el motor rechaza un comando.</summary>
    public enum CommandError
    {
        None = 0,
        UnknownCommand,
        WrongPhase,
        InvalidSeat,
        NotYourTurn,
        CardNotInHand,
        CaptureCardNotOnTable,
        DuplicateCaptureCard,
        CaptureSumMismatch,
        CaptureIsMandatory,
    }
}
