namespace Escoba.Core
{
    /// <summary>
    /// Generador de números aleatorios determinista. Toda la aleatoriedad del Core y de la IA
    /// pasa por esta interfaz para que la misma semilla reproduzca la misma partida.
    /// </summary>
    public interface IRandom
    {
        /// <summary>Devuelve un entero uniforme en [0, maxExclusive).</summary>
        int Next(int maxExclusive);
    }
}
