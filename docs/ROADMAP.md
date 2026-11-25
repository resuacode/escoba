# Roadmap

Estado: **M0–M4 completados, versión 0.4.0; siguiente M5** · Última actualización: 2026-11-24

## M0 — Setup del proyecto
- [x] CLAUDE.md, skills de proyecto y `.mcp.json`
- [x] Paquete MCP for Unity en `Packages/manifest.json`
- [x] Estructura `Assets/_Project` y asmdefs (Core, AI, Net, Presentation, App, Editor, Tests)
- [x] Test de arquitectura (`ArchitectureTests`)
- [x] README, GDD, ARCHITECTURE, ROADMAP, CARD_ART_REPLACEMENT, CHANGELOG
- [x] Paquete MCP instalado y servidor HTTP respondiendo en `localhost:8080/mcp` (2026-11-23)
- [x] Herramientas `unityMCP` disponibles en la sesión de Claude Code (2026-11-23)
- [x] `ArchitectureTests` en verde vía MCP (3/3, 2026-11-23)
- [x] Importar cartas Kenney (CC0) en `Art/Cards/Kenney/`
- [x] Cartas Kenney recortadas y renombradas a convención española (41 PNG); resto del pack eliminado
- [x] `CardTexturePostprocessor` (ajustes de importación automáticos) y `KenneyCardImporter`
- [x] Limpiar contenido de plantilla y crear escenas `Boot`, `MainMenu`, `Game` (en build settings)
- [x] Orientación móvil solo horizontal en Player Settings
- [x] Commit inicial

## M1 — Core de reglas + tests
- [x] `Card`, `Suit`, `Deck`, `IRandom` / `Pcg32Random`
- [x] `RulesConfig` (los presets como ScriptableObject pasan a M2, junto a la UI de configuración)
- [x] `GameState`, comandos, `GameEngine` (reparto, escoba de mano, turno, capturas, escobas, fin de ronda y partida)
- [x] `LegalMoves`, `Setenta`, `RoundScorer` (todas las variantes y bonificaciones)
- [x] `PlayerView` filtrada y eventos con marca de privacidad (`IsPrivateTo`)
- [x] Tests de los casos límite de `escoba-rules` §9 + determinismo + invariantes (96 tests EditMode en verde, 2026-11-23)
- [x] Bot Fácil (`EasyBot`) y simulación bot‑vs‑bot en EditMode

## M2 — Mesa 2D 1v1 PvE
- [x] `RulesPreset` (ScriptableObject) → `RulesConfig`: Clásica 21, Rápida 15, Larga 31
- [x] `CardSkin` + `CardSkinGenerator` (menú) + skin Kenney validado (40/40)
- [x] Escena `Game`: mesa, manos, mazo, bazas, marcador (uGUI responsive, safe area)
- [x] Selección de carta y combinación (clic/toque, doble toque, arrastre a la mesa)
- [x] Reproducción de eventos con animaciones básicas y comprobación mesa ↔ motor
- [x] Resumen de ronda y fin de partida (revancha / menú)
- [x] Menú principal y configuración de partida (presets, dificultad)
- [x] Localización propia: `LocalizationService`, `LocalizedText`, `es.json`, `gl.json` (revisado con el DRAG), `en.json` y tests
- [x] Escenas generadas por código (`Escoba/Build/Rebuild All`)
- [x] 126 tests EditMode + 2 PlayMode (partida completa bot‑vs‑bot en la escena real) en verde (2026-11-23)
- [ ] Pendiente de M5: arte de mesa definitivo, iconos y revisión en resoluciones móviles reales

## M3 — Hot‑seat, 3‑4 jugadores y parejas
- [x] Varios `HumanLocalController` + pantalla de pase opaca con la última jugada; mano oculta fuera de tu turno
- [x] Mesa para 2, 3 y 4 jugadores (`SeatLayout`: el que mira siempre abajo, el siguiente a su derecha) con fichas de asiento (`SeatPanelView`)
- [x] Parejas: baza compartida, marcador "Pareja n (miembros)", resumen y fin de partida por bandos
- [x] Mezcla humanos/IA por asiento en "Nueva partida" (número de jugadores, individual/por parejas, Humano/IA por asiento)
- [x] Textos nuevos en es/gl/en (gallego revisado con el DRAG)
- [x] 141 tests EditMode + 6 PlayMode (partidas completas 2/3/4/parejas y hot-seat) en verde (2026-11-23)
- [x] Nombres de jugador en "Nueva partida" (máx. 12 caracteres, saneados con `PlayerNameRules`) y recuerdo de la última configuración por modo (`SetupPreferences`)
- [x] 159 tests EditMode + 7 PlayMode en verde (2026-11-23)

## M4 — IA Media y Difícil
- [x] Heurística Media con pesos configurables (`HeuristicWeights`, `MoveScoring`)
- [x] Difícil con conteo de cartas y Monte Carlo (`BotKnowledge`, `HardBot`)
- [x] Torneos de balance (`BotTournament`, menú `Escoba/AI/Run Balance Tournament`) y tabla en GDD §9: Medio 98,5 % vs Fácil, Difícil 80 % vs Medio
- [x] Dificultades Media y Difícil activadas en "Nueva partida"
- [x] 178 tests EditMode (incl. `BotTests`, `BalanceTests`) + 8 PlayMode en verde (2026-11-23)
- [ ] Pendiente (M5): medir tiempos en móvil/WebGL; valorar un Fácil algo más fuerte según playtests

## CI/CD (entre M4 y M5)
- [x] Tests automáticos en cada push/PR (`ci.yml`)
- [x] Web de pruebas en GitHub Pages `/dev/` tras cada CI verde en `master` (`web-dev.yml`)
- [x] Releases por etiqueta con WebGL, Windows y Android APK + web estable + itch.io (`release.yml`)
- [x] Secrets de Unity, regla `v*` del entorno github-pages e itch.io configurados (2026-11-24)
- [x] Keystore de Android generado (fuera del repo), *Custom Keystore* activado y sus 4 secrets en GitHub (2026-11-24)
- [x] Pages: origen GitHub Actions + HTTPS; web en `resuacode.es/escoba/` (estable) y `resuacode.es/escoba/dev/` (pruebas)
- [x] Primera ejecución de CI y Web (dev): tests en verde y web de pruebas funcionando (2026-11-24)
- [ ] Primera release **`v0.4.0`** (M0–M4 + CI/CD): etiqueta creada el 2026-11-24; pendiente de comprobar release, web estable e itch.io
- [ ] iOS: fuera de CI por ahora

## M5 — Pulido
- [x] Pantalla de ajustes (idioma, volúmenes, velocidad, pantalla completa) con persistencia; gallego por defecto (2026-11-25)
- [x] Pausa en partida con aviso al salir (2026-11-25)
- [x] Efectos de sonido (Kenney *Casino Audio*, CC0) (2026-11-25)
- [x] Música de fondo: tema de menú + 4 pistas de partida al azar con fundido cruzado (2026-11-25)
- [x] Animación destacada de "¡Escoba!" y del siete de oros, partículas, resumen de ronda y fin de partida animados (2026-11-25)
- [ ] Accesibilidad: tamaño de texto, modo daltónico, botones táctiles grandes
- [ ] Créditos (necesarios si la música es CC-BY)
- [x] "Cómo se juega" (10 páginas con ejemplos) y Créditos/Autoría (2026-11-25)
- [ ] Tutorial guiado interactivo (se valorará tras probar "Cómo se juega")
- [ ] Builds Windows, Android, iOS, WebGL y optimización
- [ ] Sustitución por la baraja española propia

## M6 — Online con Photon Fusion 2 (Shared Mode)
- [ ] Instalar Fusion 2, crear App ID (plan gratuito) y revisar límites vigentes
- [ ] `Escoba.Net.Photon`, salas por código, partida rápida
- [ ] Autoridad en host, vistas filtradas, reconexión y abandono
