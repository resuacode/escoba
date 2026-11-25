# Escoba

Juego de cartas tradicional **Escoba** con la baraja española, hecho en **Unity 6**. Modos contra la IA y multijugador local (hot‑seat) para 2, 3 o 4 jugadores (parejas), con reglas configurables. Multijugador online con Photon previsto.

> Estado: **M3 completado** — contra la IA (nivel Fácil) o varios jugadores en el mismo dispositivo con pantalla de pase; 2, 3 o 4 jugadores, individual o por parejas; en español, gallego e inglés. Ver [docs/ROADMAP.md](docs/ROADMAP.md).

## Documentación
| Documento | Contenido |
|---|---|
| [docs/GDD.md](docs/GDD.md) | Game Design Document |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Ensamblados, flujo de comandos, red |
| [docs/ROADMAP.md](docs/ROADMAP.md) | Hitos y tareas |
| [docs/CARD_ART_REPLACEMENT.md](docs/CARD_ART_REPLACEMENT.md) | Sustituir la baraja por sprites propios |
| [docs/LOCALIZATION.md](docs/LOCALIZATION.md) | Idiomas (es, gl, en), formato JSON y decisiones normativas del gallego |
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
3. Abre la escena `Assets/_Project/Scenes/Boot.unity` y pulsa Play: menú principal → "Jugar contra la IA" (tú contra la IA) o "Jugar en este dispositivo" (varios humanos, con pantalla de pase) → elige jugadores, individual o por parejas, Humano/IA por asiento y reglas → "Empezar".
4. Si cambias assets o maquetación, regenera todo con **Escoba → Build → Rebuild All (assets + scenes)**.

### Cómo se juega en la mesa
- Toca/clica una carta de tu mano: se resaltan en azul las cartas de la mesa que puede llevarse y se oscurecen las demás. Si solo hay una combinación posible, se marca sola.
- Toca las cartas de la mesa para marcarlas (la suma aparece como "x/15") y pulsa **Jugar**. **Deshacer** quita la selección.
- Atajos: doble toque en una carta, o arrastrarla a la mesa, la juega directamente si no hay que elegir combinación.
- Idiomas: galego (por defecto), español e English, en **Ajustes**, junto con el volumen de música y efectos, la velocidad de las animaciones y la pantalla completa. Los cambios se recuerdan entre sesiones.
- Botón **Menú** de la mesa: pausa la partida (continuar, ajustes o salir al menú).

### Menús de editor
| Menú | Qué hace |
|---|---|
| Escoba → Build → Rebuild All (assets + scenes) | Crea presets, `CardSkin_Kenney`, `GameSettings`, el prefab de carta y las escenas Boot/MainMenu/Game |
| Escoba → Cards → Generate Card Skin... | Crea un `CardSkin` desde una carpeta con las 41 imágenes |
| Escoba → Cards → Validate Active Card Skin | Comprueba que la baraja activa está completa |
| Escoba → Cards → Import Kenney Pack... | Reconvierte el pack de Kenney original a la convención española |

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

## CI/CD
- **CI**: tests EditMode + PlayMode en cada push y pull request (GitHub Actions + GameCI).
- **Web**: estable en <https://resuacode.es/escoba/> y versión de pruebas (cada push a `master` con tests en verde) en <https://resuacode.es/escoba/dev/>.
- **Releases**: una etiqueta `vX.Y.Z` genera la release de GitHub con WebGL, Windows y Android (APK), publica la web estable y sube a itch.io.
- Configuración (secrets de Unity, Pages en `resuacode.es/escoba`, itch.io, keystore) y proceso: [docs/CI.md](docs/CI.md) y skill `/release`.

## Créditos y licencias
- Cartas temporales: **Kenney — Playing Cards Pack** ([kenney.nl](https://kenney.nl/assets/playing-cards-pack)), licencia **CC0**.
- Efectos de sonido: **Kenney — Casino Audio** ([kenney.nl](https://kenney.nl/assets/casino-audio)), licencia **CC0**.
- Música:
  - "Lobby Time", "Local Forecast - Slower", "Bossa Antigua" y "Cool Vibes" — Kevin MacLeod ([incompetech.com](https://incompetech.com)), licencia [Creative Commons: By Attribution 4.0](http://creativecommons.org/licenses/by/4.0/).
  - "Chill lofi inspired" — omfgdude, versión en bucle de qubodup ([OpenGameArt](https://opengameart.org/content/chill-lofi-inspired-loop-edit)), licencia **CC0**.
- MCP for Unity: [CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp) (MIT), solo herramienta de desarrollo.
