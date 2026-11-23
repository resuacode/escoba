# Changelog

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/); versionado [SemVer](https://semver.org/lang/es/).

## [Unreleased]
### Added — M2: mesa 2D 1 contra 1 contra la IA
- Escena `Game` jugable: reparto animado, mano propia, dorsos del rival, mesa, mazo, bazas, escobas, marcador, turno, aviso "¡Escoba!", resumen de ronda y fin de partida con revancha.
- Interacción con ratón y táctil: seleccionar carta (las capturas posibles se resaltan y el resto se oscurece), marcar cartas de la mesa, suma "x/15", botones Jugar/Deshacer, doble toque y arrastre a la mesa para jugar directamente.
- Menú principal y panel "Nueva partida" (Clásica 21, Rápida 15, Larga 31; dificultad Fácil) con selector de idioma.
- Localización propia en JSON: español, gallego (revisado con el DRAG y las Normas RAG 2012; ver `docs/LOCALIZATION.md`) e inglés.
- `GameController` (bucle de partida), `IPlayerController` con `HumanLocalController` y `AIController`, `TablePresenter`, `HudView`, `PlaySelection`, `CardView`, `CardSkin`, `RulesPreset`, `GameSettings`, `AppContext`, `SafeArea`.
- Herramientas de editor: `Escoba/Build/Rebuild All` (assets por defecto + escenas + prefab de carta), `Escoba/Cards/Generate Card Skin...`, `Escoba/Cards/Validate Active Card Skin`.
- Recursos esenciales de TextMeshPro importados.
- Tests: `PlaySelectionTests`, `LocalizationTests`, `ProjectAssetsTests` (EditMode) y `GameSmokeTests` (PlayMode). Total: 126 EditMode + 2 PlayMode.

### Changed
- `PlayerSettings.runInBackground = true` (la partida sigue sin foco; necesario también para probar con el editor en segundo plano).

### Added — M1: núcleo de reglas
- `Escoba.Core`: `Card`, `Suit`, `Deck`, `Pcg32Random`, `RulesConfig`, `GameState`, `GameEngine`, `LegalMoves`, `Setenta`, `RoundScorer`, `PlayerView`, comandos (`PlayCardCommand`, `StartNextRoundCommand`) y eventos de partida.
- Reglas implementadas: reparto 3+4, escoba en el reparto (15 o 30), captura a 15 con elección de combinación, captura obligatoria, escobas, última jugada, mesa al último que captura, puntuación (escobas, cartas, oros, siete de oros, setenta clásica, bonificaciones opcionales), fin de partida sin empate en cabeza, 2-4 jugadores y parejas.
- `Escoba.AI`: `IBotStrategy` y `EasyBot`.
- 96 tests EditMode: cartas, RNG, reglas, capturas, motor, puntuación/setenta, vista filtrada y simulaciones bot-vs-bot (determinismo e invariantes en 2, 3 y 4 jugadores).

### Added — M0: setup
- `CLAUDE.md` con directrices del proyecto (convenciones, arquitectura, flujo de trabajo, MCP, tests, git).
- Skills de Claude Code: `escoba-rules`, `unity-feature`, `unity-test`, `unity-playtest`, `escoba-ai`, `net-ready-check`, `card-skin`, `gdd`, `update-docs`.
- Paquete MCP for Unity (`com.coplaydev.unity-mcp` v10.0.0) y `.mcp.json` con el servidor `unityMCP`.
- Estructura `Assets/_Project` con ensamblados `Escoba.Core`, `Escoba.AI`, `Escoba.Net`, `Escoba.Presentation`, `Escoba.App`, `Escoba.Editor` y de tests.
- `ArchitectureTests`: Core, AI y Net no referencian Unity.
- Documentación: README, GDD, ARCHITECTURE, ROADMAP, CARD_ART_REPLACEMENT.
- Cartas Kenney (CC0) convertidas a baraja española (41 PNG de 42×60, nombres `oros_01.png`…`dorso.png`) en `Assets/_Project/Art/Cards/Kenney/`; el resto del pack se descarta.
- Herramientas de editor: `KenneyCardImporter` (menú `Escoba/Cards/Import Kenney Pack...`) y `CardTexturePostprocessor`.
- Escenas `Boot`, `MainMenu` y `Game` (cámara, Canvas 1920×1080 adaptable, EventSystem con Input System) en build settings.
- Player Settings: nombre de producto "Escoba", orientación móvil solo horizontal.

### Removed
- Contenido de la plantilla URP (`TutorialInfo`, `Readme.asset`, `SampleScene`, `SampleSceneProfile`).

### Changed
- Reglas: el punto de sietes pasa a ser la **Setenta clásica** (una carta por palo, 7 > 6 > 1 > 5 > 4 > 3 > 2 > figuras; empate configurable). Se elimina la variante "más sietes".
- GDD 0.2.0: móvil solo en horizontal; recomendación de Photon Fusion 2 (Shared Mode); notas de arte pixel art de Kenney.
- GDD 0.3.0: **captura obligatoria** por defecto; online con **Photon Fusion 2 (Shared Mode)**; localización con **sistema propio JSON + `LocalizedText`**.
