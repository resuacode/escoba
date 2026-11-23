---
name: unity-feature
description: Flujo completo para implementar una feature del juego Escoba en Unity (diseño → tests → código → escena/UI vía MCP → verificación → documentación). Úsala cuando el usuario pida añadir o cambiar funcionalidad de juego, UI, IA o flujo de partida.
---

# Implementar una feature

Argumento opcional: descripción o id de la tarea del ROADMAP.

## 0. Entender
1. Lee la sección relevante de `docs/GDD.md` y, si toca reglas, la skill `escoba-rules`.
2. Localiza el hito en `docs/ROADMAP.md`. Si la feature no está definida o es ambigua, pregunta antes de escribir código.
3. Decide el ensamblado de cada pieza (ver CLAUDE.md §4):
   - ¿Es una regla, dato o cálculo? → `Core` (C# puro).
   - ¿Decide jugadas? → `AI`.
   - ¿Mueve mensajes entre jugadores? → `Net`.
   - ¿Dibuja, anima o recoge input? → `Presentation`.
   - ¿Arranca, navega entre escenas, guarda ajustes, ScriptableObjects? → `App`.
   - ¿Herramienta de editor? → `Editor`.
4. Para cambios no triviales, presenta un mini‑plan (ficheros + tests) y espera confirmación.

## 1. Tests primero (Core/AI)
- Crea/edita tests en `Assets/_Project/Tests/EditMode/` siguiendo la skill `unity-test`.
- Construye estados explícitos (cartas concretas en mano/mesa) en vez de depender del barajado.

## 2. Implementar
- Respeta las reglas de arquitectura: estado solo cambia vía comandos, RNG con semilla, sin `UnityEngine` en Core/AI/Net, textos visibles localizables, sprites vía `CardSkin`.
- Un tipo público por fichero; XML docs en español en la API pública.
- Ficheros `.cs`/`.asmdef`: herramientas de fichero normales. Tras crearlos, Unity genera los `.meta`.

## 3. Compilar y revisar consola
1. `mcp__unityMCP__refresh_unity` (con compilación).
2. `mcp__unityMCP__read_console` (errores y warnings). Corrige hasta **0 errores**; warnings nuevos, también salvo justificación.
3. Si el MCP no está disponible, dilo y pide al usuario que inicie el servidor (`Window → MCP for Unity → Start Server`).

## 4. Escena, prefabs y UI (si aplica)
- Usa `manage_scene`, `manage_gameobject`, `manage_components`, `manage_prefabs`, `manage_asset`. No edites YAML a mano.
- UI: Canvas con `CanvasScaler` (Scale With Screen Size, referencia 1920×1080, match 0.5), anclas por esquina/lado, `SafeArea` en móvil, `EventSystem` con `InputSystemUIInputModule`.
- Prefabs en `Assets/_Project/Prefabs/`, ScriptableObjects en `Assets/_Project/Data/`.
- Guarda la escena tras los cambios.

## 5. Verificar
1. Ejecuta tests con la skill `unity-test` (EditMode siempre; PlayMode si tocaste flujo/UI).
2. Si hay cambios visibles, usa `unity-playtest`.
3. Si tocaste estado, comandos, IA o información visible: pasa la checklist de `net-ready-check`.

## 6. Documentar y cerrar
- Ejecuta la skill `update-docs` (README/GDD/ARCHITECTURE/ROADMAP/CHANGELOG según corresponda).
- Resumen al usuario: qué cambió (con enlaces a ficheros), resultado de tests (números reales), qué queda pendiente. No commitees salvo que lo pida.
