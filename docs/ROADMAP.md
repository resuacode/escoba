# Roadmap

Estado: **M0–M3 completados; siguiente M4** · Última actualización: 2026-11-23

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
- [ ] Pendiente: nombres personalizados de jugadores (campo de texto) y recordar la última configuración de asientos

## M4 — IA Media y Difícil
- [ ] Heurística Media con pesos configurables
- [ ] Difícil con conteo de cartas y Monte Carlo
- [ ] Torneos de balance y tabla en GDD §9

## M5 — Pulido
- [ ] Audio, partículas, "¡Escoba!"
- [ ] Pantalla de ajustes y accesibilidad (idiomas es/gl/en ya disponibles desde M2)
- [ ] Tutorial y "Cómo se juega"
- [ ] Builds Windows, Android, iOS, WebGL y optimización
- [ ] Sustitución por la baraja española propia

## M6 — Online con Photon Fusion 2 (Shared Mode)
- [ ] Instalar Fusion 2, crear App ID (plan gratuito) y revisar límites vigentes
- [ ] `Escoba.Net.Photon`, salas por código, partida rápida
- [ ] Autoridad en host, vistas filtradas, reconexión y abandono
