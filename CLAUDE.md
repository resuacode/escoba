# CLAUDE.md — Escoba (Unity 6)

Directrices para Claude Code en este repositorio. Léelas antes de cualquier tarea.

## 1. Proyecto
Juego de cartas **Escoba** con la baraja española (40 cartas), 2D con uGUI.
- Modos: **PvE** (contra IA), **PvP local hot‑seat** (con pantalla de pase), **PvP online con Photon Fusion 2, Shared Mode** (futuro, hito M6).
- 2, 3 y 4 jugadores (4 = parejas). Reglas **configurables** (objetivo de puntos, variantes).
- Plataformas: **Windows (ratón), Android/iOS (táctil), WebGL**.
- Hito actual y siguientes: [docs/ROADMAP.md](docs/ROADMAP.md). Diseño: [docs/GDD.md](docs/GDD.md). Arquitectura: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
- **Reglas del juego (fuente de verdad):** skill `escoba-rules`. Si el GDD y la skill discrepan, avisa al usuario antes de implementar.

## 2. Entorno
- Unity **6000.3.11f1**, URP (plantilla), Input System 1.19, uGUI 2.0 (incluye TextMeshPro), Test Framework 1.6.
- MCP: **MCP for Unity** (`com.coplaydev.unity-mcp`, fijado en `#v10.0.0`), servidor `unityMCP` por HTTP en `http://localhost:8080/mcp` (ver [.mcp.json](.mcp.json)). Requiere `uv` y el editor abierto con el servidor iniciado (`Window → MCP for Unity`).
- Shell: Windows. Usa rutas absolutas o relativas a la raíz del repo.

## 3. Idioma y convenciones
- **Código en inglés** (tipos, métodos, variables, nombres de assets técnicos). **Comentarios, XML docs, documentación, commits y mensajes al usuario en español.**
- Texto visible al jugador: nunca hardcodeado en código ni en escenas; usa claves del sistema de localización propio (`Assets/_Project/Localization/{es,gl,en}.json` + componente `LocalizedText` / `Localization.Get`). Al añadir una clave, añádela en **los tres** JSON. El **gallego debe ser normativo (RAG), no portugués**: sigue las decisiones y fuentes de [docs/LOCALIZATION.md](docs/LOCALIZATION.md) y consulta el DRAG ante cualquier término nuevo.
- C#: `PascalCase` para tipos/métodos/propiedades, `_camelCase` para campos privados, `camelCase` locales/parámetros; `[SerializeField] private` en vez de campos públicos; un tipo público por fichero, fichero = nombre del tipo.
- Namespaces = ensamblado: `Escoba.Core`, `Escoba.AI`, `Escoba.Net`, `Escoba.Presentation`, `Escoba.App`, `Escoba.Editor`, `Escoba.Tests.*`.
- Sin `FindObjectOfType`/`Find`/`GameObject.Find` en runtime; inyecta referencias por inspector o desde el bootstrap.
- Sin singletons estáticos con estado de partida. Sin `async void` salvo handlers de eventos de UI.

## 4. Arquitectura (obligatoria)
```
Assets/_Project/
  Scripts/Core          Escoba.Core          C# puro (noEngineReferences) — cartas, reglas, estado, comandos, puntuación
  Scripts/AI            Escoba.AI            C# puro — bots
  Scripts/Net           Escoba.Net           C# puro — abstracción de transporte (local hoy, Photon mañana)
  Scripts/Presentation  Escoba.Presentation  MonoBehaviours uGUI: vistas, animaciones, input
  Scripts/App           Escoba.App           Bootstrap, flujo de escenas, menús, ajustes, ScriptableObjects
  Scripts/Editor        Escoba.Editor        Herramientas de editor (generar CardSkin, validadores…)
  Tests/EditMode | Tests/PlayMode
  Data/ Art/ Prefabs/ Scenes/
```
Reglas:
1. **Core, AI y Net no usan `UnityEngine`** (lo vigila `ArchitectureTests`). Nada de `UnityEngine.Random`, `Debug.Log`, `Mathf` ahí: usa `System`.
2. **Determinismo**: toda aleatoriedad del Core va por un RNG con semilla inyectado. Misma semilla + mismos comandos ⇒ misma partida.
3. **Estado + comandos**: el estado de la partida solo cambia aplicando comandos (`IGameCommand`) validados por el motor de reglas. La presentación **lee** estado y **emite** comandos; nunca muta estado.
4. **Controladores de jugador** (`IPlayerController`): humano local, IA y remoto implementan la misma interfaz; PvE, hot‑seat y online comparten el bucle de partida.
5. **Información oculta**: las vistas solo reciben lo que su jugador puede ver (vista filtrada del estado). Imprescindible para hot‑seat y Photon.
6. **Reglas configurables**: `RulesConfig` (POCO en Core) creado desde un `RulesPreset` (ScriptableObject en App). No hay números mágicos de reglas en el código.
7. **Arte desacoplado**: las cartas se pintan a través de un `CardSkin` (ScriptableObject `(Suit, Rank) → Sprite` + dorso). Ningún script referencia sprites de cartas directamente. Ver [docs/CARD_ART_REPLACEMENT.md](docs/CARD_ART_REPLACEMENT.md).
8. UI responsive: `CanvasScaler` en *Constant Pixel Size* gobernado por `SafeCanvasScaler` (el diseño de 1920×1080 más un margen cabe siempre entero en la zona segura), `SafeArea` con margen interior, anclas correctas; input vía `EventSystem` + `InputSystemUIInputModule` (ratón y táctil por igual).
9. **Escenas y prefab de carta generados por código**: `Escoba/Build/Rebuild All` (`Scripts/Editor/Build/SceneBuilder.cs`, `DefaultAssetsBuilder.cs`). Para cambiar la maquetación, edita el constructor y vuelve a ejecutar el menú; no edites las escenas a mano (se sobrescriben).

## 5. Flujo de trabajo
Para cualquier feature usa la skill **`unity-feature`**. Resumen:
1. Revisa GDD + `escoba-rules`; si falta definición, pregunta.
2. **Tests primero** en EditMode para lógica de Core/AI.
3. Implementa en el ensamblado correcto.
4. Espera a la recompilación y revisa la consola vía MCP (`refresh_unity` → `read_console`): **0 errores** antes de seguir.
5. Ejecuta tests (`unity-test`). No declares nada terminado con tests en rojo; si fallan, muéstralo.
6. Escenas/prefabs/UI: vía MCP (`unity-playtest` para verificar en Play Mode).
7. Actualiza documentación (`update-docs`) y CHANGELOG.

## 6. Uso del MCP (`unityMCP`)
- Escenas: `manage_scene`. GameObjects: `manage_gameobject`, `find_gameobjects`, `manage_components`. Prefabs: `manage_prefabs`. Assets/ScriptableObjects: `manage_asset`. Editor/Play Mode: `manage_editor`. Menús propios: `execute_menu_item`. Consola: `read_console`. Tests: `run_tests` + `get_test_job`. Recompilar: `refresh_unity`. Scripts: `validate_script`.
- **No edites YAML de `.unity`/`.prefab`/`.asset` a mano** si el MCP está disponible. Los `.cs`, `.asmdef` y `.md` sí se editan con las herramientas de fichero normales.
- Si el MCP no responde: pide al usuario que abra Unity y pulse *Start Server* en `Window → MCP for Unity`; mientras, usa el fallback por CLI para tests.
- Antes de entrar en Play Mode guarda la escena; al terminar sal de Play Mode (los cambios en Play Mode se pierden).
- Particularidades observadas: al entrar en Play Mode o tras `manage_camera screenshot` el editor puede quedar **en pausa** (`EditorApplication.isPaused`): reanúdalo con `execute_code` (`EditorApplication.isPaused = false`). Si la captura de `manage_camera` sale sin la UI, captura con `ScreenCapture.CaptureScreenshot("Screenshots/x.png")` desde `execute_code` y léela con Read. `execute_code` bloquea `AssetDatabase.DeleteAsset`: borra con `manage_asset delete`. Ejecutar tests de PlayMode con `run_tests` activa *Enter Play Mode Options* en `ProjectSettings/EditorSettings.asset` (`m_EnterPlayModeOptions: 1`): **descarta ese cambio** (`git checkout -- ProjectSettings/EditorSettings.asset`) antes de commitear.

## 7. Tests
- EditMode (`Escoba.Tests.EditMode`): reglas, puntuación, IA, determinismo, simulaciones masivas con semilla.
- PlayMode (`Escoba.Tests.PlayMode`): flujo de partida, smoke de UI.
- Vía MCP: `run_tests` (modo `EditMode`/`PlayMode`) y consultar con `get_test_job`.
- Fallback CLI (con el editor **cerrado**):
  `"C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -logFile -`
- Nombres: `MethodUnderTest_Scenario_ExpectedResult`. Un escenario de reglas = estado inicial explícito (no aleatorio) o semilla fija.

## 8. Git
- Nunca commitear `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build*/`. **Siempre** commitear los `.meta` junto a su asset.
- Ramas `feature/<tema>`, `fix/<tema>`; commits pequeños en español (`feat: captura de cartas a 15`). Solo commitear/pushear cuando el usuario lo pida.
- Assets de terceros con su licencia en la carpeta (p. ej. `Art/Cards/Kenney/License.txt`).
- CI/CD en `.github/workflows/` (ver [docs/CI.md](docs/CI.md)): tests en cada push, web de pruebas en GitHub Pages `/dev/` tras cada CI verde en `master`, releases al subir etiquetas `vX.Y.Z` (skill `release`). No pushear etiquetas ni ramas sin que el usuario lo pida. Nunca commitear `.ulf`, keystores ni contraseñas.

## 9. Skills del proyecto (`.claude/skills/`)
| Skill | Cuándo |
|---|---|
| `escoba-rules` | Cualquier duda o implementación de reglas/puntuación. |
| `unity-feature` | Implementar una feature de principio a fin. |
| `unity-test` | Ejecutar/escribir tests y reportar resultados. |
| `unity-playtest` | Probar en Play Mode vía MCP, revisar consola y resoluciones. |
| `escoba-ai` | Diseñar, ajustar o medir bots. |
| `net-ready-check` | Revisar que un cambio no rompe la futura versión online. |
| `card-skin` | Importar o sustituir la baraja (sprites). |
| `gdd` | Ampliar o modificar el GDD. |
| `update-docs` | Sincronizar README, GDD, ARCHITECTURE, ROADMAP y CHANGELOG. |
| `release` | Preparar y publicar una versión (CHANGELOG, versión, etiqueta) y revisar el CI/CD. |

## 10. Glosario
- **Baraja española**: oros, copas, espadas, bastos; 1‑7, sota (10), caballo (11), rey (12). En la Escoba valen 1‑7, sota 8, caballo 9, rey 10.
- **Baza**: cartas capturadas por un jugador/pareja. **Escoba**: captura que deja la mesa vacía (+1 punto).
- **Velo / siete de oros**: punto por capturarlo. **Setenta**: punto por la mejor combinación de una carta de cada palo (7 > 6 > 1 > 5 > 4 > 3 > 2 > figuras); detalle en `escoba-rules`.
- **Mano**: jugador a la derecha del repartidor, juega primero. **Pie / repartidor**: quien reparte.
- **Ronda (mano de reparto)**: desde que se reparte la baraja hasta que se agota. **Partida**: rondas hasta alcanzar el objetivo.
