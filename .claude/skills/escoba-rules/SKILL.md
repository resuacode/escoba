---
name: escoba-rules
description: Referencia canónica de las reglas y la puntuación de la Escoba (baraja española, 2-4 jugadores, parejas, variantes configurables, casos límite). Úsala siempre que implementes, testees o documentes lógica de reglas, puntuación, reparto o IA, o cuando haya dudas sobre cómo se juega.
---

# Reglas de la Escoba (fuente de verdad del proyecto)

Si una regla no está aquí o es ambigua: **no inventes**, pregunta al usuario y, tras su respuesta, actualiza esta skill y el GDD (§ Reglas).

## 1. Baraja
- 40 cartas: palos **Oros, Copas, Espadas, Bastos**; rangos 1‑7, **Sota (10)**, **Caballo (11)**, **Rey (12)**.
- Valor en juego (para sumar 15): 1‑7 → su número; Sota → **8**; Caballo → **9**; Rey → **10**.
- En código: `Suit { Oros, Copas, Espadas, Bastos }`, `Rank` con el número impreso (1‑7, 10, 11, 12) y `CardValue` derivado. Id estable de carta: `suitIndex * 10 + ordinal` (0‑39) para serialización/red.

## 2. Jugadores, asientos y turnos
- 2, 3 o 4 jugadores. Con 4: **parejas** fijas, compañeros sentados enfrente (asientos 0‑2 y 1‑3). Una pareja comparte baza y puntos.
- Orden de juego **antihorario** (hacia la derecha del repartidor), como es costumbre en España. En código: siguiente asiento = `(seat + 1) % n`, con la mesa dibujada de forma que esto se vea antihorario.
- El repartidor rota cada ronda; empieza jugando el siguiente al repartidor (**mano**). El primer repartidor se elige al azar (RNG con semilla).

## 3. Reparto
1. Se baraja (RNG con semilla).
2. Primer reparto de la ronda: **3 cartas a cada jugador** y **4 cartas boca arriba en la mesa**.
3. Cuando todos se quedan sin cartas y quedan cartas en el mazo: 3 cartas más a cada jugador, **sin** añadir a la mesa.
4. Cuentas exactas (36 cartas tras la mesa): 2 jug. → 6 repartos; 3 jug. → 4; 4 jug. → 3.

**Escoba en el reparto** (variante `DealerTableEscoba`, por defecto **activada**): si las 4 cartas iniciales de la mesa suman **15**, el repartidor se las lleva y anota **1 escoba**; si suman **30** (dos grupos de 15 disjuntos que cubren las 4 cartas), se las lleva y anota **2 escobas**. Si solo un subconjunto suma 15, no pasa nada.

## 4. Turno
En su turno el jugador **juega exactamente una carta** de su mano:
- **Captura**: si el valor de la carta jugada + el valor de **una o más** cartas de la mesa suma **exactamente 15**, se lleva la carta jugada y esas cartas a su baza.
  - Si hay varias combinaciones posibles, el jugador **elige una** (solo una combinación por jugada).
  - **Captura obligatoria** (`CaptureIsMandatory`, por defecto **activada**, decidido 2026-11-23): si la carta jugada puede sumar 15 con alguna combinación de la mesa, **debe** capturar (eligiendo combinación si hay varias). El jugador sí puede elegir qué carta de su mano juega. Con la opción desactivada podría dejarla en la mesa aunque pudiera capturar. De momento la opción no se expone en la configuración de partida (se decidirá más adelante); el motor la soporta igualmente.
- **Escoba**: si tras la captura la mesa queda vacía → **+1 escoba** para el jugador/pareja. Se registra la carta para mostrarla girada en la baza (feedback visual).
- **Dejar carta**: si no captura, la carta queda boca arriba en la mesa.
- Restricción del motor: una captura válida es `{cartaJugada} ∪ S`, con `S ⊆ mesa`, `S ≠ ∅`, `valor(cartaJugada) + Σ valor(S) = 15`.

## 5. Fin de ronda
- Cuando el mazo está vacío y todos han jugado su última carta: las cartas que queden en la mesa son para **el último jugador/pareja que capturó**. Esto **no** es escoba.
- Variante `LastPlayEscobaCounts` (por defecto **activada**): si la última jugada de la ronda deja la mesa vacía por captura, cuenta como escoba.
- Si nadie capturó en toda la ronda (caso extremo), las cartas de la mesa no se asignan.

## 6. Puntuación por ronda
Se puntúa por jugador o pareja:

| Concepto | Puntos | Empate |
|---|---|---|
| Cada escoba | 1 | — |
| Más cartas | 1 | Nadie puntúa |
| Más oros | 1 | Nadie puntúa |
| Siete de oros (velo) | 1 | — (solo uno lo tiene) |
| **Setenta** | 1 | Según `SetentaTieRule` (por defecto nadie puntúa) |

### Setenta (regla clásica, decidida por el usuario 2026-11-23)
- Cada jugador/pareja forma su setenta con **cuatro cartas, una de cada palo**: de cada palo toma su mejor carta según el orden **7 > 6 > 1 > 5 > 4 > 3 > 2 > figuras** (sota, caballo y rey empatan entre sí).
- Comparación: se ordenan las 4 cartas de la setenta de mayor a menor y se comparan **lexicográficamente** con las del rival. Esto equivale a: gana quien tenga **más sietes**; si hay empate en sietes, decide la carta más alta en los palos restantes (más seises, luego más ases, etc.).
  - Ej.: `[7,7,5,2]` gana a `[7,6,6,6]` (2 sietes contra 1). `[7,6,1,F]` gana a `[7,6,5,4]` (el 1 supera al 5 en el tercer puesto).
- Si a un jugador le falta algún palo en su baza, **no puede ganar** la setenta (no tiene setenta completa). Si nadie tiene setenta completa, nadie puntúa.
- Empate exacto (mismos cuatro rangos en la comparación): `SetentaTieRule`:
  - `NoPoint` (**por defecto**): nadie recibe el punto.
  - `BothScore`: todos los empatados en cabeza reciben el punto ("según acuerdo previo").
- Con 3 jugadores o parejas se compara la mejor setenta de cada jugador/pareja; si el máximo está empatado, se aplica `SetentaTieRule`.
- No hay variante de "más sietes" a secas: la Setenta clásica es la única regla.

Bonificaciones opcionales (por defecto **desactivadas**, `int` de puntos, 0 = off):
- `AllSevensBonus`: tener los 4 sietes.
- `AllGoldsBonus`: tener los 10 oros (sustituye al punto de "más oros" si se activa).

## 7. Fin de partida
- Objetivo `TargetScore`: **15, 21 (por defecto) o 31**, configurable.
- Se comprueba **al final de cada ronda**. Gana quien alcance o supere el objetivo; si varios lo superan, gana el de mayor puntuación; si hay empate en cabeza, se juega otra ronda.
- Variante futura (no implementar sin pedirlo): cantar la victoria a mitad de ronda.

## 8. `RulesConfig` (POCO en Core)
```
PlayerCount (2|3|4)          Teams (bool, solo con 4)
TargetScore (15|21|31|custom)
DealerTableEscoba = true     CaptureIsMandatory = true
LastPlayEscobaCounts = true  SetentaTieRule = NoPoint | BothScore
AllSevensBonus = 0           AllGoldsBonus = 0
HandSize = 3                 InitialTableCards = 4   CaptureSum = 15
```
Los presets (`RulesPreset_Classic21`, `RulesPreset_Quick15`, …) son ScriptableObjects en `Assets/_Project/Data/Rules/` que producen un `RulesConfig`.

## 9. Casos límite que deben tener test
- Mesa inicial suma 15 / suma 30 / subconjunto suma 15 (no captura automática).
- Carta que puede capturar con varias combinaciones → el comando elige; combinación inválida rechazada.
- Captura obligatoria: dejar en mesa una carta que puede capturar → rechazado con `CaptureIsMandatory = true`, aceptado con `false`.
- Captura con 1 carta de mesa y con 3+ cartas de mesa.
- Escoba a mitad de ronda; escoba en la última jugada con `LastPlayEscobaCounts` on/off.
- Cartas restantes al último que capturó; ronda sin capturas.
- Empates en cartas (20‑20) y oros (5‑5).
- Setenta: gana por más sietes; empate en sietes resuelto por seises/ases/…; figuras equivalentes; empate exacto con `NoPoint` y con `BothScore`.
- Setenta con un palo ausente (no puede ganar) y nadie con setenta completa.
- Parejas: baza y puntos compartidos; turnos alternos entre equipos.
- Fin de partida: uno supera objetivo, ambos superan, empate en cabeza.
- Repartos exactos para 2/3/4 jugadores; rotación de repartidor y mano.
- Determinismo: misma semilla + mismos comandos ⇒ mismo estado final (hash).
