# Changelog

Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/); versionado [SemVer](https://semver.org/lang/es/).

## [Unreleased]
### Added
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
