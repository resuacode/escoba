# Roadmap

Estado: **M0 en curso** · Última actualización: 2026-11-23

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
- [ ] `Card`, `Suit`, `Rank`, `Deck`, `SeededRandom`
- [ ] `RulesConfig` + presets
- [ ] `GameState`, comandos, `GameEngine` (reparto, turno, capturas, escobas, fin de ronda)
- [ ] `LegalMoves`, `RoundScorer` (todas las variantes)
- [ ] `PlayerView` filtrada
- [ ] Tests de todos los casos límite de `escoba-rules` §9 + determinismo + invariantes
- [ ] Bot Fácil y simulación bot‑vs‑bot en EditMode

## M2 — Mesa 2D 1v1 PvE
- [ ] `CardSkin` + herramienta de editor + skin Kenney
- [ ] Escena `Game`: mesa, manos, mazo, bazas, marcador (uGUI responsive)
- [ ] Selección de carta y combinación (ratón/táctil), arrastre
- [ ] Cola de eventos y animaciones básicas
- [ ] Resumen de ronda y fin de partida
- [ ] Menú principal y configuración de partida (presets)
- [ ] Localización propia: `LocalizationService`, `LocalizedText`, `es.json` y test de claves

## M3 — Hot‑seat, 3‑4 jugadores y parejas
- [ ] `HumanLocalController` múltiple + pantalla de pase
- [ ] Layouts de 3 y 4 jugadores, marcador por parejas
- [ ] Mezcla humanos/bots por asiento

## M4 — IA Media y Difícil
- [ ] Heurística Media con pesos configurables
- [ ] Difícil con conteo de cartas y Monte Carlo
- [ ] Torneos de balance y tabla en GDD §9

## M5 — Pulido
- [ ] Audio, partículas, "¡Escoba!"
- [ ] Ajustes, accesibilidad, traducción al inglés (`en.json`) y selector de idioma
- [ ] Tutorial y "Cómo se juega"
- [ ] Builds Windows, Android, iOS, WebGL y optimización
- [ ] Sustitución por la baraja española propia

## M6 — Online con Photon Fusion 2 (Shared Mode)
- [ ] Instalar Fusion 2, crear App ID (plan gratuito) y revisar límites vigentes
- [ ] `Escoba.Net.Photon`, salas por código, partida rápida
- [ ] Autoridad en host, vistas filtradas, reconexión y abandono
