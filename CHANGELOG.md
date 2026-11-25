# Changelog

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/); versionado [SemVer](https://semver.org/lang/es/).

## [Unreleased]

## [0.5.0] - 2026-11-25
Pulido (M5): ajustes, pausa, efectos de sonido y música, «Cómo se juega», créditos, animaciones y partículas, accesibilidad y mejor encaje en pantallas panorámicas. Gallego como idioma por defecto.

### Added — M5: pantallas panorámicas, sonido al pasar el ratón y guía de arte
- **Sonido sutil al pasar el ratón** por las cartas de la mano (solo ratón, no en táctil; `SoundCue.CardHover`).
- **Fondo de mesa opcional**: si existe `Art/Table/table_background.png`, el menú y la mesa lo usan a pantalla completa recortando lo que sobre (`GameSettings.TableBackground`, `TableArtPostprocessor`).
- `docs/ART_GUIDE.md`: tamaños y formatos de cartas, dorso, fondo de mesa, icono (y adaptativo de Android), pantalla de arranque e imágenes de itch.io.
- Tests `SafeCanvasScalerTests` (6 proporciones de pantalla). Total: 198 EditMode + 12 PlayMode.

### Fixed
- En pantallas más panorámicas que 16:9 (móviles 20:9, navegador con barras) los botones de arriba y abajo quedaban pegados al borde o cortados: el Canvas dejaba menos de 1080 de alto de diseño. Ahora `SafeCanvasScaler` escala para que el diseño de 1920×1080 más un margen de 24 quepa siempre entero en la zona segura, y `SafeArea` deja ese margen interior.

### Added — M5: accesibilidad y medidor de rendimiento
- **Alto contraste (daltonismo)** en Ajustes: cartas que se pueden llevar con borde blanco, carta seleccionada en naranja intenso, bordes más gruesos y cartas no elegibles más oscuras; el resaltado ya no depende solo del tono (`AccessibilityOptions`, `CardView`).
- **Interfaz grande** en Ajustes (activada por defecto en móvil): textos hasta un 20 % más grandes sin salirse de su caja (`ScalableText`) y botones Jugar, Deshacer y Menú un 25 % más grandes (`ScalableButton`).
- **Medidor de rendimiento oculto**: tocar 5 veces la versión del menú muestra FPS (medios y mínimos) y el tiempo de decisión de la IA (último y máximo) (`PerfOverlay`, `PerfStats`). Para medir en móvil y web.
- Tests: `Accessibility_DefaultsDependOnPlatformAndPersist` (EditMode) y `Accessibility_HighContrastAndLargeUi_ApplyInGame` (PlayMode). Total: 190 EditMode + 12 PlayMode.

### Changed
- Los interruptores de Ajustes (pantalla completa, alto contraste, interfaz grande) muestran «Sí/No» (gl: «Si/Non»).

### Fixed
- `UiFactory.SetField` no guardaba valores `Vector2`/`float` (quedaban a cero en las escenas generadas).

### Added — M5: animaciones y partículas
- **Partículas de interfaz** (`UiParticles`, `ParticleStyle`): destellos dorados y confeti con gravedad y giro, hechos con imágenes de uGUI (los `ParticleSystem` no se ven en un Canvas de pantalla). Usan tiempo de juego, así que se congelan en pausa.
- **Escoba**: destellos y confeti sobre la mesa, sonido y aviso con entrada con rebote y latido (1,4 s).
- **Siete de oros**: al llevárselo, la carta late, suelta destellos, suena y aparece un aviso corto «¡Siete de oros!».
- **Resumen de ronda animado**: entra con fundido y rebote, y las filas aparecen de una en una con sonido cuando dan puntos. «Siguiente ronda» durante la animación la completa al instante. Sigue la velocidad de animación elegida.
- **Fin de partida**: el título entra con rebote y, si gana un humano, destellos y tres oleadas de confeti desde las esquinas.
- Efectos de sonido nuevos `GoldSeven` y `ScoreTick`; `CardView.Punch`; `HudView.ShowBanner` con rebote (`EaseOutBack`).
- Test `Effects_ParticlesExpireAndRoundSummaryRevealsAllRows` (PlayMode). Total: 189 EditMode + 11 PlayMode.

### Added — M5: «Cómo se juega» y créditos
- **Cómo se juega** (menú principal y pausa): 10 páginas (objetivo, valor de las cartas, turno, captura obligatoria, escoba, fin de ronda, puntos, setenta, jugadores y parejas, controles) con ejemplos dibujados con la baraja activa, así que se actualizan solos al cambiar las cartas (`HowToPlayGuide`, `HowToPlayPage`, `HowToPlayPanel`).
- **Créditos** (en gallego, *Autoría*): botón abajo a la izquierda del menú con la autoría y licencias de cartas, sonidos y música (crédito CC-BY de Kevin MacLeod) (`CreditsPanel`).
- Tests: `HowToPlayGuideTests` (textos, cartas distintas y ejemplos que suman 15) y `Menu_HowToPlayAndCredits_ShowEveryPageTranslated` (PlayMode). Total: 189 EditMode + 10 PlayMode.

### Added — M5: ajustes, pausa y sonido
- Panel **Ajustes** (menú principal y pausa): idioma (galego, español, English), volumen de música y de efectos, velocidad de las animaciones (lenta, normal, rápida) y pantalla completa (escritorio y web). Cada cambio se aplica al momento y se guarda; sin cambios no se escribe ninguna preferencia (`UserSettings`, `SettingsPanel`).
- **Pausa** en la partida (botón "Menú"): continuar, ajustes o salir al menú avisando de que se pierde la partida. Mientras está abierta se detiene el juego (`PausePanel`).
- **Efectos de sonido** con el pack *Casino Audio* de Kenney (CC0): barajar, repartir, jugar carta, llevarse cartas, escoba, mostrar la mano, fin de ronda, fin de partida y clic en los botones (`AudioService`, `AudioLibrary`, `SoundCue`, `Sounds`, `ButtonClickSound`). Preparado para música de fondo en bucle (`Assets/_Project/Audio/Music/`).
- **Música de fondo**: *Lobby Time* en el menú y, en cada partida, una pista al azar entre *Local Forecast – Slower*, *Bossa Antigua*, *Cool Vibes* (Kevin MacLeod, CC-BY 4.0) y *Chill lofi* (omfgdude/qubodup, CC0), que enlaza con otra distinta al terminar. Cambios de pista con fundido cruzado (sin silencios al repetir) que sigue funcionando en pausa. Créditos en `Audio/Music/License.txt`, README y notas de cada release.
- `AudioImportPostprocessor`: importación automática del audio (música Vorbis 0,4 comprimida en memoria y carga en segundo plano; efectos mono descomprimidos al cargar).
- Tests: `UserSettingsTests` y `AudioLibrary_HasClipsForEverySoundCue` (EditMode); `Settings_ChangesApplyAndPersist_PauseFreezesAndResumes` (PlayMode). Total: 186 EditMode + 9 PlayMode.
- Workflow manual `itch.yml` (*itch.io (manual)*): sube a itch.io los ficheros de una release ya publicada, sin recompilar.

### Changed
- **Idioma por defecto: gallego** (antes español, o inglés si el sistema estaba en inglés). La preferencia del jugador se sigue recordando entre sesiones.
- El botón "Idioma" del menú principal pasa a ser **Ajustes**.
- La carpeta `Art/Cards/Spanish/` se importa con el perfil *pixel art* (filtro Point, sin compresión), como Kenney.
- Acciones de GitHub actualizadas a sus versiones con Node 24 (`checkout@v7`, `cache@v6`, `upload-artifact@v7`, `download-artifact@v8`, `configure-pages@v6`, `upload-pages-artifact@v5`, `deploy-pages@v5`): sin avisos de Node 20 obsoleto.

### Fixed
- `release.yml`: butler se descarga de `broth.itch.zone` (el dominio `broth.itch.ovh` ya no resuelve y hacía fallar el paso de itch.io de la `v0.4.0`).

## [0.4.0] - 2026-11-24
Primera versión publicada: Escoba jugable contra la IA (Fácil, Media y Difícil) o en un mismo dispositivo, de 2 a 4 jugadores y por parejas, en español, gallego e inglés.

### Added — CI/CD
- GitHub Actions con GameCI: `ci.yml` (tests EditMode + PlayMode en cada push/PR), `web-dev.yml` (WebGL a GitHub Pages `/dev/` tras cada CI verde en `master`) y `release.yml` (etiqueta `vX.Y.Z` → builds WebGL, Windows y Android APK, release de GitHub con notas del CHANGELOG, web estable en la raíz de Pages e itch.io opcional).
- `docs/CI.md` (configuración: secrets de Unity, Pages en `resuacode.es/escoba`, itch.io, keystore de Android) y skill `/release`.
- Etiqueta de versión en el menú principal (`VersionLabel`).

### Changed
- Player Settings: compañía `resuacode`, identificador `es.resuacode.escoba` (antes el de la plantilla), versión `0.4.0` (`versionCode` 400) y WebGL con *Decompression Fallback* activado (necesario en GitHub Pages).

### Added — M4: IA Media y Difícil
- `MediumBot`: heurística con pesos configurables (`HeuristicWeights`): escobas, siete de oros, sietes, oros, cartas y mejora de la setenta; penaliza dejar escoba o sietes/oros al rival.
- `HardBot`: conteo de cartas no vistas (`BotKnowledge`) y muestreo Monte Carlo de la mano del siguiente rival para restar su mejor respuesta.
- `MoveScoring`, `BotGuard`, `BotTournament`/`TournamentResult` y menú `Escoba/AI/Run Balance Tournament` (tabla Markdown para el GDD).
- Dificultades Media y Difícil disponibles en "Nueva partida".
- Balance (400 partidas): Medio gana al Fácil el 98,5 %; Difícil gana al Medio el 80 %; ~0,16 ms por decisión del Difícil en editor.
- Tests: `BotTests` (comportamiento, legalidad en partidas completas, probabilidades) y `BalanceTests` (`[Category("Slow")]`); las partidas automáticas de PlayMode usan Fácil, Medio y Difícil.

### Fixed
- Salir al menú (botón "Menú") durante el turno de un humano lanzaba `MissingReferenceException` al recolocar la mano con la escena ya descargada. `TablePresenter.EndHumanTurn` ya no toca la parte visual si la escena está destruida. Test de regresión `Game_LeavingToMenuMidTurnOrMidAnimation_DoesNotThrow`.

### Added — M3: hot-seat, 3-4 jugadores y parejas
- "Jugar en este dispositivo" activado. "Nueva partida" permite elegir 2, 3 o 4 jugadores, individual o por parejas (con 4) y Humano/IA en cada asiento (al menos un humano).
- Pantalla de pase opaca antes de cada turno humano cuando hay varios humanos, con la última jugada ("X jugó el 5 de copas y se llevó 2 cartas"); la mano solo se ve durante el turno de su dueño.
- La mesa se dibuja desde el jugador que mira (siempre abajo) y rota al cambiar de jugador; manos ajenas arriba o en columna a los lados; ficha por asiento con nombre, baza, escobas, "Reparte" y resalte de turno.
- Parejas: marcador "Pareja 1 (Tú y IA 3)", resumen de ronda con hasta 4 columnas, "¡Ha ganado tu pareja!".
- Nombres: "Tú"/"Rival" (1 contra 1), "IA n", "Jugador n", "Pareja n" en es/gl/en.
- `SeatLayout`, `SeatPanelView`, `PassScreenPanel`, `PlayerNaming`, `SeatType`; `MatchConfig` con asientos y parejas.
- Tests: `SeatLayoutTests`, `MatchSetupTests` (EditMode); partidas completas 2/3/4/parejas y `HotSeat_PassScreenHidesHandsAndSwitchesViewer` (PlayMode).
- Nombres de jugador: campo de texto por asiento humano en "Nueva partida" (texto de ayuda = nombre por defecto, máx. 12 caracteres). Se usan en la ficha, el marcador, la pantalla de pase, el resumen y el final; nombres repetidos se distinguen con el número de asiento.
- `PlayerNameRules` (Core, reutilizable en online): recorta espacios, limita a 12 caracteres y elimina `< > { } |` y caracteres de control.
- "Nueva partida" recuerda la última configuración (jugadores, parejas, asientos) por separado para "contra la IA" y "en este dispositivo"; los nombres se comparten (`SetupPreferences`).
- Tests: `PlayerNamesTests` (EditMode) y `Menu_PlayerNames_AreUsedInGameAndRemembered` (PlayMode, restaura las preferencias al terminar).

### Changed
- `HudView` pasa a mostrar un marcador de hasta 4 bandos; la información de cada jugador vive en `SeatPanelView`.
- `TablePresenter` admite N asientos y cambiar de jugador que mira (`SetViewer`, `SetViewerHandRevealed`).
- Menú movido arriba a la izquierda y marcador arriba a la derecha; mazo abajo a la izquierda.

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
