# Escoba

Juego de cartas tradicional **Escoba** con la baraja española, hecho en **Unity 6**. Modos contra la IA y multijugador local (hot‑seat) para 2, 3 o 4 jugadores (parejas), con reglas configurables. Multijugador online con Photon previsto.

> Estado: **M0 — Setup**. Ver [docs/ROADMAP.md](docs/ROADMAP.md).

## Documentación
| Documento | Contenido |
|---|---|
| [docs/GDD.md](docs/GDD.md) | Game Design Document |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Ensamblados, flujo de comandos, red |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Hitos y tareas |
| [docs/CARD_ART_REPLACEMENT.md](docs/CARD_ART_REPLACEMENT.md) | Sustituir la baraja por sprites propios |
| [CHANGELOG.md](CHANGELOG.md) | Historial de cambios |
| [CLAUDE.md](CLAUDE.md) | Directrices para desarrollar con Claude Code |

## Requisitos
- **Unity 6000.3.11f1** (Unity Hub) con módulos Android, iOS y WebGL según plataforma de build.
- **Git**.
- **uv** (gestor de Python de Astral), necesario para MCP for Unity: `winget install astral-sh.uv`.
- **Claude Code** (CLI o extensión de VS Code) para el flujo con IA.

## Puesta en marcha
1. Clona el repositorio y ábrelo con Unity Hub (versión 6000.3.11f1).
2. La primera apertura descarga los paquetes, incluido **MCP for Unity** desde GitHub (`com.coplaydev.unity-mcp`, `v10.0.0`).
3. Abre la escena `Assets/_Project/Scenes/Boot.unity` y pulsa Play (de momento las escenas están vacías).

## Entorno de desarrollo con Claude Code + MCP
1. En Unity: **Window → MCP for Unity** → **Start Server** (HTTP en `http://localhost:8080/mcp`).
2. El repositorio incluye [.mcp.json](.mcp.json) con el servidor `unityMCP`; Claude Code pedirá aprobarlo la primera vez. Alternativa: en la ventana de MCP for Unity pulsa **Configure** junto a *Claude Code*.
3. En Claude Code ejecuta `/mcp` y comprueba que `unityMCP` está **connected**.
4. Si cambias de transporte (HTTP/stdio) en la ventana de MCP for Unity, reinicia Claude Code.

Skills del proyecto (en `.claude/skills/`, invocables como `/nombre`):
`escoba-rules`, `unity-feature`, `unity-test`, `unity-playtest`, `escoba-ai`, `net-ready-check`, `card-skin`, `gdd`, `update-docs`.

## Estructura
```
Assets/_Project/
  Scripts/Core          Reglas y estado (C# puro, determinista)
  Scripts/AI            Bots (C# puro)
  Scripts/Net           Abstracción de red (C# puro)
  Scripts/Presentation  UI uGUI, vistas y animaciones
  Scripts/App           Arranque, escenas, ajustes
  Scripts/Editor        Herramientas de editor
  Tests/EditMode        Tests de lógica
  Tests/PlayMode        Tests de flujo/UI
  Data/ Art/ Prefabs/ Scenes/
docs/                   Documentación
.claude/skills/         Skills de Claude Code
```

## Tests
- Desde Unity: **Window → General → Test Runner** → EditMode / PlayMode → Run All.
- Desde Claude Code: skill `/unity-test`.
- Línea de comandos (con el editor cerrado):
  ```powershell
  & "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults\editmode.xml -logFile -
  ```

## Builds
| Plataforma | Notas |
|---|---|
| Windows | File → Build Profiles → Windows; 1920×1080 base, ventana redimensionable |
| Android | Orientación horizontal, IL2CPP, ARM64 |
| iOS | Orientación horizontal, requiere macOS + Xcode para compilar |
| WebGL | Compresión Brotli; sin hilos |

## Créditos y licencias
- Cartas temporales: **Kenney — Playing Cards Pack** ([kenney.nl](https://kenney.nl/assets/playing-cards-pack)), licencia **CC0**.
- MCP for Unity: [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) (MIT), solo herramienta de desarrollo.
