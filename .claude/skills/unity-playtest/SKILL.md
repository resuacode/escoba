---
name: unity-playtest
description: Prueba el juego Escoba dentro del editor de Unity vía MCP (Play Mode, consola, jerarquía, partidas bot-vs-bot, comprobación de resoluciones y plataformas). Úsala para verificar visualmente o en ejecución un cambio, reproducir un bug o hacer una sesión de QA.
---

# Playtest vía MCP

Argumento opcional: qué verificar (p. ej. "pantalla de pase en hot-seat").

## Preparación
1. `refresh_unity` + `read_console`: 0 errores de compilación.
2. `manage_scene`: abre la escena a probar (`Assets/_Project/Scenes/Boot.unity` para flujo completo, `Game.unity` para mesa) y guarda cambios pendientes.
3. Limpia la consola (`read_console` con acción de limpiar) para leer solo lo nuevo.

## Ejecución
0. Si cambiaste maquetación o assets: `execute_menu_item` → `Escoba/Build/Rebuild All (assets + scenes)`.
1. `manage_editor` → entrar en Play Mode. Comprueba con `execute_code` que `EditorApplication.isPaused` es false (el editor puede arrancar o quedarse en pausa tras una captura); si no, pon `EditorApplication.isPaused = false`.
   - Capturas: `manage_camera` (`screenshot`, `include_image`) o, si sale sin UI, `execute_code` con `ScreenCapture.CaptureScreenshot("Screenshots/<nombre>.png")`, espera 1-2 s y léela con Read (`Screenshots/` está en `.gitignore`).
   - Interacción simulada: por reflexión sobre `TablePresenter._selection` (`SelectHandCard`, `ToggleTableCard`) y `HudView.PlayButton.onClick.Invoke()`; navegación con `GameObject.Find("<Botón>").GetComponent<Button>().onClick.Invoke()`.
   - Partida automática: test PlayMode `GameSmokeTests` o `GameController.ConfigureAutoPlay(true, velocidad)` antes de `Start`.
2. Si existe el menú de depuración, úsalo con `execute_menu_item`:
   - `Escoba/Debug/Start Bot vs Bot` — partida automática acelerada.
   - `Escoba/Debug/Capture Screenshot` — guarda captura en `Screenshots/` (fuera de Assets).
   - `Escoba/Debug/Dump Game State` — escribe el estado actual en consola.
   (Si aún no existen, propón crearlos en `Escoba.Editor` / `Escoba.App` como parte de la tarea.)
3. Observa: `read_console` periódicamente; `find_gameobjects` para comprobar que existen vistas esperadas (manos, mesa, marcador).
4. `manage_editor` → salir de Play Mode al terminar (los cambios hechos en Play Mode se pierden).

## Checklist de QA
- [ ] Consola sin errores ni excepciones; sin warnings nuevos.
- [ ] Reparto correcto (3 por jugador, 4 en mesa en el primer reparto).
- [ ] Selección de carta + combinación de mesa con ratón **y** con toque (arrastrar y tocar).
- [ ] Capturas, escobas y última baza se reflejan en marcador y bazas.
- [ ] Hot‑seat: la pantalla de pase oculta la mano anterior antes de mostrar la siguiente.
- [ ] Resumen de ronda y fin de partida con puntuaciones correctas.
- [ ] Resoluciones (Game view): 1920×1080, 1280×720, 2340×1080 (móvil horizontal), 1080×2340 si se soporta vertical, 1024×768 (tablet). Safe area respetada.
- [ ] WebGL: sin APIs no soportadas (hilos, System.IO en runtime).

## Informe
Lista: qué se probó, resultado (OK/FALLO), mensajes de consola relevantes, rutas de capturas y pasos para reproducir cualquier fallo. No afirmes que algo "se ve bien" si no lo has podido comprobar (sin captura no hay verificación visual): dilo y pide al usuario que lo revise.
