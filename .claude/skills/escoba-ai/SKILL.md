---
name: escoba-ai
description: Guía para diseñar, implementar, ajustar y medir los bots de la Escoba (niveles Fácil, Medio, Difícil) en el ensamblado Escoba.AI, incluyendo heurísticas, información imperfecta y simulaciones de balance. Úsala al trabajar en la IA del modo PvE o en bots de prueba.
---

# IA de la Escoba

## Contrato
- Ensamblado `Escoba.AI` (C# puro). Un bot implementa `IBotStrategy`:
  `PlayCardCommand ChooseCommand(PlayerView view, IRandom rng)` (las reglas van en `view.Rules`).
- Implementado (M4): `EasyBot`, `MediumBot`, `HardBot`; creación por nivel con `BotFactory.Create(BotDifficulty)`.
- Piezas compartidas: `HeuristicWeights` (pesos), `MoveScoring` (simular jugada, ganancia inmediata, mejora de setenta, valor de escoba), `BotKnowledge` (cartas no vistas, probabilidad hipergeométrica), `BotGuard.PickBest` (mejor jugada con desempate aleatorio determinista).
- Simular: `Tests/EditMode/Support/GameSimulator.cs` (acepta `botForSeat`) y `BotTournament.Run(rules, crearA, crearB, partidas, semillaBase)` (1 contra 1, alterna asientos, devuelve `TournamentResult`).
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

**Difícil** (implementado) — ganancia inmediata − `OpponentFactor` × ganancia media de la mejor respuesta del siguiente rival, estimada muestreando `HardSamples` (24) manos posibles entre las **cartas no vistas** (40 − mano − mesa − bazas). Las mismas muestras para todas las candidatas (comparación estable). Presupuesto: < 50 ms por decisión en móvil/WebGL (sin hilos en WebGL); en editor ~0,2 ms de media.

**Medio** (implementado) usa riesgos simples: `MediumEscobaRiskProbability` si la mesa resultante permite escoba con una sola carta (1-10) y `MediumExposureProbability` × valor de cada siete/oro que quede en la mesa.

**Parejas**: tener en cuenta al compañero (no dejarle la mesa peor; bonificar dejar sumas que el compañero aún no ha mostrado no poder completar).

## Medir
- Menú `Escoba/AI/Run Balance Tournament` (400 partidas por enfrentamiento) → tabla Markdown en consola para el GDD §9.
- Tests EditMode `BalanceTests` (`[Category("Slow")]`, 200 partidas, semillas distintas del menú): Medio ≥ 75 % contra Fácil, Difícil ≥ 60 % contra Medio, tiempo de decisión, y control Fácil vs Fácil equilibrado.
- Tests de comportamiento `BotTests`: escoba disponible, preferir el 7 de oros, no dejar escoba, jugadas siempre legales.
- Objetivos orientativos: Medio gana a Fácil > 75%; Difícil gana a Medio > 60%.
- Registra % victorias, puntos medios por ronda, escobas por ronda y tiempo medio por decisión. Documenta resultados en `docs/GDD.md` § IA (tabla de balance) con la fecha.
- Cualquier ajuste de pesos debe venir acompañado de una nueva medición.
