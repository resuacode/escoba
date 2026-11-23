using System;

namespace Escoba.Core
{
    /// <summary>
    /// Generador PCG32 (XSH-RR). Determinista en cualquier plataforma, rápido y con estado
    /// serializable (<see cref="State"/> + <see cref="Increment"/>), apto para red y repeticiones.
    /// </summary>
    public sealed class Pcg32Random : IRandom
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private const ulong DefaultStream = 54UL;

        private ulong _state;

        /// <summary>Crea un generador con semilla y, opcionalmente, un flujo independiente.</summary>
        public Pcg32Random(ulong seed, ulong stream = DefaultStream)
        {
            Increment = (stream << 1) | 1UL;
            _state = 0UL;
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }

        private Pcg32Random(ulong state, ulong increment, bool restored)
        {
            _state = state;
            Increment = increment;
        }

        /// <summary>Estado interno actual.</summary>
        public ulong State => _state;

        /// <summary>Incremento (flujo) del generador.</summary>
        public ulong Increment { get; }

        /// <summary>Restaura un generador a partir de un estado guardado.</summary>
        public static Pcg32Random FromState(ulong state, ulong increment)
        {
            if ((increment & 1UL) == 0UL)
            {
                throw new ArgumentException("El incremento de PCG32 debe ser impar.", nameof(increment));
            }

            return new Pcg32Random(state, increment, true);
        }

        /// <summary>Devuelve 32 bits aleatorios.</summary>
        public uint NextUInt()
        {
            unchecked
            {
                ulong old = _state;
                _state = (old * Multiplier) + Increment;
                uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
                int rot = (int)(old >> 59);
                return (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
            }
        }

        /// <inheritdoc />
        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Debe ser mayor que 0.");
            }

            // Muestreo por rechazo para evitar el sesgo del módulo.
            uint bound = (uint)maxExclusive;
            uint threshold = (uint)((0x100000000UL - bound) % bound);
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold)
                {
                    return (int)(r % bound);
                }
            }
        }
    }
}
