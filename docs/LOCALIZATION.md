# Localización

Idiomas: **español** (`es`, base y respaldo), **gallego** (`gl`) e **inglés** (`en`).

## Cómo funciona
- Un fichero por idioma en `Assets/_Project/Localization/<código>.json`:
  ```json
  { "language": "gl", "entries": [ { "key": "menu.play_ai", "value": "Xogar contra a IA" } ] }
  ```
- `LocalizationService` (Presentation) carga los JSON listados en `GameSettings`, resuelve claves con respaldo en español y, si no existe, muestra `[clave]` y avisa en consola (solo en el editor).
- `LocalizedText` (componente sobre `TMP_Text`) muestra una clave y se actualiza al cambiar de idioma. Desde código: `Localization.Get("clave", args)` y `Localization.GetPlural("game.cards", n)`.
- Parámetros `{0}`, `{1}` con `string.Format`. Plurales con sufijos `.one` / `.other`.
- El idioma elegido se guarda en `PlayerPrefs` (`escoba.language`). Por defecto: inglés si el sistema está en inglés; si no, español. Galicia no tiene `SystemLanguage` propio en Unity, así que el gallego se elige en el menú (botón "Idioma").

## Añadir o cambiar textos
1. Añade la clave en **los tres** JSON (mismo nombre de clave, mismos marcadores `{n}`).
2. Ejecuta los tests EditMode: `LocalizationTests` comprueba que todos los idiomas tienen las mismas claves, ningún valor vacío y los mismos marcadores.
3. Para el gallego, respeta las decisiones de abajo y consulta el DRAG ante cualquier duda.

Convenciones de clave: `menu.*`, `setup.*`, `common.*`, `game.*`, `summary.*`, `gameover.*`, `card.*`, `language.name`.

## Gallego: decisiones normativas (revisión del 2026-11-23)
Norma de referencia: **Normas ortográficas e morfolóxicas do idioma galego (RAG/ILG, ed. 2012)** y **Dicionario da Real Academia Galega (DRAG)**. Evitar lusismos y castellanismos.

| Término | Decisión | Fuente |
|---|---|---|
| Nombre del juego | **escoba** (no "vasoira") | DRAG, *escoba* acep. 2: "Xogo de baralla… consiste en sumar quince puntos" — https://academia.gal/dicionario/-/termo/busca/escoba |
| Palos | **ouros, copas, espadas, bastos** ("paus" sería portugués como nombre del palo; en gallego *pau* = palo de la baraja en general) | DRAG *ouro* acep. 2, *basto* acep. 2 |
| Figuras | **sota, cabalo, rei** | DRAG *sota*, *cabalo* acep. 3, *rei* acep. 3 |
| Carta número un | **ás** (tilde diacrítica: *ás* sust. frente a *as* artículo) | DRAG *ás*; Normas §2.7.1 |
| Mazo de cartas por repartir | **baralla** ("mazo" y "monte" no tienen acepción de naipes) | DRAG *baralla*, *mazo*, *monte* |
| Llevarse cartas | **levar** transitivo: "{0} leva a mesa" (no "lévase": *levarse* = llevarse bien) | DRAG *levar* |
| Turno | **quenda** (no "vez") | DRAG *quenda* |
| Ronda | **rolda** ("ronda" → forma más recomendable *rolda*) | DRAG *ronda* |
| Revancha | **desquite** ("revancha" → forma correcta *desquite*) | DRAG *revancha*, *desquite* acep. 2 |
| En línea | **en liña** | DRAG *liña* (Informática) |
| Proximamente | sin tilde (adverbios en *-mente*) | Normas §2.7.3 |
| Interrogativos | sin tilde: "Como se xoga", "Escolle que cartas" | Normas §2.7.2 |
| máis / mais | **máis** (adverbio) con tilde | Normas §2.7.1 |
| Signos ¡ ¿ | **no se usan** en la interfaz (son opcionales según las Normas, §7) | Normas 2012, §7 |
| "está a pensar" | perífrasis *estar a + infinitivo* (también válida "está pensando") | uso normativo habitual |

`LocalizationTests.Galician_FollowsRagNormDecisions` vigila estas decisiones y la ausencia de grafías portuguesas (ç, ã, õ, lh...).

## Inglés
Terminología de la baraja española en inglés: *coins, cups, swords, clubs*; *jack, knight, king*. "Escoba" y "Setenta" se mantienen como nombres propios del juego.
