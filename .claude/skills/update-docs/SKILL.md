---
name: update-docs
description: Sincroniza la documentación del proyecto Escoba (README.md, docs/GDD.md, docs/ARCHITECTURE.md, docs/ROADMAP.md, docs/CARD_ART_REPLACEMENT.md, CHANGELOG.md y CLAUDE.md) con los cambios recientes del código. Úsala al terminar una feature, antes de un commit o cuando el usuario pida actualizar la documentación.
---

# Actualizar documentación

## 1. Detectar cambios
- `git status` y `git diff` (más `git diff --staged`). Si se indica, desde una rama/commit base: `git diff <base>...HEAD`.
- Clasifica: nueva feature, cambio de reglas, cambio de arquitectura/ensamblados, nueva herramienta de editor, cambio de assets/arte, cambio de flujo de trabajo, dependencias/paquetes.

## 2. Qué actualizar
| Cambio | Documento |
|---|---|
| Cualquier cambio visible para usuario o dev | `CHANGELOG.md` → sección `[Unreleased]` (Added/Changed/Fixed/Removed) |
| Reglas o puntuación | `docs/GDD.md` §4 **y** `.claude/skills/escoba-rules/SKILL.md` |
| Diseño, UX, pantallas, IA | `docs/GDD.md` (usa la skill `gdd`) |
| Ensamblados, flujo de comandos, controladores, red | `docs/ARCHITECTURE.md` |
| Progreso de hito | `docs/ROADMAP.md` (marcar `[x]`, mover tareas) |
| Puesta en marcha, requisitos, paquetes, builds, créditos | `README.md` |
| Arte de cartas / CardSkin | `docs/CARD_ART_REPLACEMENT.md` y skill `card-skin` |
| Textos, idiomas o términos en gallego | `docs/LOCALIZATION.md` (decisiones normativas y fuentes) |
| Convenciones o flujo de trabajo de Claude | `CLAUDE.md` (mantenerlo conciso) |

## 3. Reglas
- Español, conciso, sin inventar features que no existen en el código: documenta lo implementado; lo planificado va en ROADMAP/GDD marcado como tal.
- Enlaces relativos entre documentos; nombres de tipos y rutas en `código`.
- No borres historial (CHANGELOG, historial del GDD).
- Comprueba que los enlaces relativos apuntan a ficheros existentes.

## 4. Cierre
Lista al usuario los documentos modificados y un resumen de una línea por cada uno.
