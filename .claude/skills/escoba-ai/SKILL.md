---
name: escoba-ai
description: Guía para diseñar, implementar, ajustar y medir los bots de la Escoba (niveles Fácil, Medio, Difícil) en el ensamblado Escoba.AI, incluyendo heurísticas, información imperfecta y simulaciones de balance. Úsala al trabajar en la IA del modo PvE o en bots de prueba.
---

# IA de la Escoba

## Contrato
- Ensamblado `Escoba.AI` (C# puro). Un bot implementa `IBotStrategy`:
  `PlayCardCommand ChooseCommand(PlayerView view, IRandom rng)` (las reglas van en `view.Rules`).
- Implementado: `EasyBot`. Referencia para simular: `Tests/EditMode/Support/GameSimulator.cs`.
- Recibe una **`PlayerView`** (lo que ese jugador ve: su mano, mesa, bazas visibles, cartas jugadas, recuento del mazo). **Nunca** el `GameState` completo: un bot no puede hacer trampas, y así el mismo código sirve en online.
- Devuelve siempre un comando **válido** (usa el generador de jugadas legales del Core: `LegalMoves.For(view)` o `LegalMoves.FindCaptures`).
- Determinista dado el `IRandom` con semilla. Nada de `System.Random` propio ni reloj.
- `AIController : IPlayerController` envuelve la estrategia y añade un retardo "humano" configurable **en la capa de presentación**, no en el bot.

## Niveles
**Fácil** — elige al azar qué carta jugar; si la carta puede capturar, la captura es obligatoria (`CaptureIsMandatory`) y elige combinación al azar. Con la regla desactivada, captura con un 60% de probabilidad.

**Medio** — puntúa cada jugada legal con heurística y elige la mejor (desempate aleatorio):
| Factor | Peso orientativo |
|---|---|
| Hace escoba | +100 |
| Captura el 7 de oros | +40 |
| Cada siete capturado | +12 |
| Cada oro capturado | +6 |
| Cada carta capturada | +2 |
| Deja en mesa una suma ≤ 14 alcanzable con una carta (riesgo de escoba rival) | −(probabilidad estimada × 80) |
| Deja un 7 / el 7 de oros expuesto sin capturar | −15 / −30 |
| Mejora de su **setenta** (más sietes; si no, mejor carta por palo según 7 > 6 > 1 > 5 > 4 > 3 > 2 > figuras; completar un palo que falta) | +k |

Los pesos viven en una clase `HeuristicWeights` (serializable) para poder ajustarlos sin tocar lógica.

**Difícil** — Medio + **conteo de cartas** (cartas no vistas = 40 − mano − mesa − jugadas) para estimar la probabilidad real de que el rival pueda hacer escoba o capturar sietes/oros; opcional **determinización Monte Carlo** (muestrear manos rivales compatibles con lo visto, simular 1‑2 jugadas, promediar). Presupuesto: < 50 ms por decisión en móvil/WebGL (sin hilos en WebGL).

**Parejas**: tener en cuenta al compañero (no dejarle la mesa peor; bonificar dejar sumas que el compañero aún no ha mostrado no poder completar).

## Medir
- Tests EditMode `[Category("Slow")]`: torneos `Bot A vs Bot B` con semillas 0..N‑1 (N ≥ 1000), intercambiando asientos.
- Objetivos orientativos: Medio gana a Fácil > 75%; Difícil gana a Medio > 60%.
- Registra % victorias, puntos medios por ronda, escobas por ronda y tiempo medio por decisión. Documenta resultados en `docs/GDD.md` § IA (tabla de balance) con la fecha.
- Cualquier ajuste de pesos debe venir acompañado de una nueva medición.
