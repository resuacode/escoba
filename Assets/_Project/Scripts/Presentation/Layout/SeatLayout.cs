namespace Escoba.Presentation
{
    /// <summary>
    /// Posición en pantalla de cada asiento respecto al jugador que mira (siempre abajo).
    /// El turno pasa a la derecha (sentido antihorario, como se juega en España):
    /// 2 jugadores: abajo/arriba; 3: abajo/derecha/izquierda; 4: abajo/derecha/arriba/izquierda.
    /// </summary>
    public static class SeatLayout
    {
        public const int Bottom = 0;
        public const int Right = 1;
        public const int Top = 2;
        public const int Left = 3;
        public const int SlotCount = 4;

        /// <summary>Hueco de pantalla del asiento <paramref name="seat"/> visto desde <paramref name="viewerSeat"/>.</summary>
        public static int SlotOf(int seat, int viewerSeat, int playerCount)
        {
            int offset = (((seat - viewerSeat) % playerCount) + playerCount) % playerCount;
            switch (playerCount)
            {
                case 2:
                    return offset == 0 ? Bottom : Top;
                case 3:
                    return offset == 0 ? Bottom : offset == 1 ? Right : Left;
                default:
                    return offset;
            }
        }

        /// <summary>Los huecos laterales colocan la mano en columna.</summary>
        public static bool IsVertical(int slot) => slot == Right || slot == Left;
    }
}
