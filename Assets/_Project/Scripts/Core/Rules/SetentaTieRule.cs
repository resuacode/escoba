namespace Escoba.Core
{
    /// <summary>Qué ocurre si varios jugadores/parejas empatan con la mejor setenta.</summary>
    public enum SetentaTieRule
    {
        /// <summary>Nadie recibe el punto (por defecto).</summary>
        NoPoint = 0,

        /// <summary>Todos los empatados en cabeza reciben el punto.</summary>
        BothScore = 1,
    }
}
