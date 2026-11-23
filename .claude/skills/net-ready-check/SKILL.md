---
name: net-ready-check
description: Checklist de revisión para asegurar que el código de Escoba sigue preparado para el futuro multijugador online con Photon (determinismo, comandos serializables, información oculta, autoridad). Úsala tras cambios en estado de partida, comandos, controladores, IA o vistas, y antes de empezar el hito M6.
---

# Revisión de preparación para red (Photon)

Revisa el diff actual (`git diff` y ficheros nuevos) contra esta lista. Informa cada punto como OK / PROBLEMA (fichero:línea + propuesta). No arregles sin confirmar si el cambio es grande.

## Determinismo
- [ ] Toda aleatoriedad del Core/AI usa el `IRandom` con semilla inyectado (no `System.Random` nuevo, no `UnityEngine.Random`, no `Guid.NewGuid`, no `DateTime.Now`).
- [ ] No se itera sobre `HashSet`/`Dictionary` para decidir orden de juego o resultados (orden no garantizado) sin ordenar antes.
- [ ] Sin `float` en lógica de reglas/puntuación (solo enteros).

## Comandos y estado
- [ ] El estado de la partida solo cambia en el motor al aplicar `IGameCommand`.
- [ ] Los comandos son pequeños y serializables: ids de carta (0‑39), asiento, tipo. Sin referencias a objetos ni a MonoBehaviours.
- [ ] Validación completa en el motor (un cliente malicioso no puede capturar algo ilegal).
- [ ] Existe/sigue funcionando la serialización del estado y un hash para detectar desincronización.

## Información oculta
- [ ] Vistas, IA y controladores trabajan con `PlayerView` filtrada, no con `GameState`.
- [ ] Nada en la presentación conserva o muestra cartas de manos ajenas (salvo dorsos y recuentos).
- [ ] El orden del mazo no es deducible desde el cliente no autoritativo (en online, el MasterClient o servidor reparte).

## Controladores y flujo
- [ ] El bucle de partida no distingue tipos de jugador: pide comandos a `IPlayerController` de forma asíncrona (humano, IA, remoto).
- [ ] Las animaciones no bloquean la lógica: la presentación consume una cola de eventos del motor (`CardPlayed`, `Captured`, `Escoba`, `RoundEnded`...).
- [ ] Tiempos de espera/abandono contemplados (o anotados como TODO para M6).

## Plan Photon (referencia para M6)
- **Photon Fusion 2, Shared Mode** (decidido 2026-11-23). El Master Client de la sesión es la autoridad de la partida y ejecuta el motor; los clientes envían comandos por RPC; el Master Client envía a cada jugador su `PlayerView` con RPC dirigidos. Ver [docs/ARCHITECTURE.md](../../../docs/ARCHITECTURE.md) § Red.
- No usar `[Networked]` para el estado de cartas (lo verían todos los clientes): solo RPC dirigidos e instantáneas filtradas.
- Nombres de jugador recibidos por red: sanearlos siempre con `PlayerNameRules.Sanitize` en el Master Client (evita inyección de texto enriquecido TMP).
- `Escoba.Net` define `INetworkTransport`; `LocalTransport` para hot‑seat/PvE; `PhotonTransport` en un ensamblado nuevo `Escoba.Net.Photon` (con referencias a Unity/Photon) para no contaminar Core.
