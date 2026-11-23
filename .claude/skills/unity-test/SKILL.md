---
name: unity-test
description: Ejecuta los tests de Unity (EditMode/PlayMode) del proyecto Escoba vía MCP o por línea de comandos, interpreta resultados y da pautas para escribir tests de reglas, IA y determinismo. Úsala para correr tests, investigar fallos o crear tests nuevos.
---

# Tests en Unity

Argumentos opcionales: `editmode` | `playmode` | `all` (por defecto `editmode`), y un filtro de nombre.

## Ejecutar vía MCP (preferido, con el editor abierto)
1. `mcp__unityMCP__refresh_unity` y `mcp__unityMCP__read_console`: si hay errores de compilación, arréglalos primero (los tests no corren con errores).
2. `mcp__unityMCP__run_tests` con el modo (`EditMode`/`PlayMode`) y filtro si procede → devuelve un job id.
3. Consulta `mcp__unityMCP__get_test_job` hasta que termine.
4. Reporta: total / pasados / fallidos / ignorados y, por cada fallo, test, mensaje y la línea relevante del stack.

## Fallback CLI (editor cerrado)
Unity no permite dos instancias sobre el mismo proyecto: si el editor está abierto, usa el MCP.
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\dresua\escoba" `
  -runTests -testPlatform EditMode -testResults "C:\Users\dresua\escoba\TestResults\editmode.xml" -logFile -
```
- Si la ruta de Unity no existe, búscala en `C:\Program Files\Unity\Hub\Editor\` o pregunta.
- Lee el XML (formato NUnit 3): `test-run` tiene `total`, `passed`, `failed`; los fallos están en `test-case[@result='Failed']/failure/message`.
- `TestResults/` está en `.gitignore`.

## Nunca
- Declarar "tests pasan" sin haberlos ejecutado. Si no se pudieron ejecutar, dilo explícitamente.
- Desactivar, `[Ignore]` o relajar un test para que pase sin acordarlo con el usuario.

## Escribir tests
Ubicación: `Assets/_Project/Tests/EditMode/<Área>/<Tipo>Tests.cs` (namespace `Escoba.Tests.EditMode`).
Nombre: `MethodUnderTest_Scenario_ExpectedResult`.

Pautas:
- **Estados explícitos**: usa `TestGame.Create(rules, hands, table, deck, dealer, lastCapturer, scores)` (en `Tests/EditMode/Support/`) con la notación corta de cartas: número + inicial del palo (`"7o"` = 7 de oros, `"10b"` = sota de bastos, `"11c"` caballo de copas, `"12e"` rey de espadas). Por defecto el asiento 0 es mano. Ayudas: `Cards("7o 5c")`, `Capture(seat, "7o", "5c 3e")`, `Drop(seat, "1c")`, `First<TEvent>(events)`.
- **Partidas completas**: `GameSimulator.Play(rules, seed, onStep)` con bots Fácil; `onStep` permite comprobar invariantes tras cada comando.
- **Una regla, un test**. Cubre la lista de casos límite de la skill `escoba-rules` §9.
- **Determinismo**: test que juega una partida completa con semilla fija y bots, dos veces, y compara el hash/serialización del estado final.
- **Simulación masiva** (IA/balance): N partidas con semillas 0..N‑1 en un `[Test]` con `[Category("Slow")]`; comprueba invariantes (40 cartas siempre, sin duplicados, puntuaciones coherentes) y no solo resultados.
- **Invariantes tras cada comando**: cartas en mano + mesa + mazo + bazas = 40; ninguna carta duplicada; turno válido.
- PlayMode: `[UnityTest]` con `IEnumerator`, carga escena `Game`, arranca partida bot‑vs‑bot acelerada y espera a `GameOver` con timeout.

Plantilla:
```csharp
using NUnit.Framework;

namespace Escoba.Tests.EditMode
{
    public class CaptureRulesTests
    {
        [Test]
        public void Apply_PlayCardThatSumsFifteenWithTwoTableCards_CapturesThem()
        {
            // Arrange: estado explícito
            // Act: aplicar comando
            // Assert: baza, mesa, turno
        }
    }
}
```
