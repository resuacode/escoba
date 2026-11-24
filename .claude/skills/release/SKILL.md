---
name: release
description: Prepara y publica una versión de Escoba (vX.Y.Z) que el workflow de GitHub Actions convierte en release con builds WebGL, Windows y Android, web estable en GitHub Pages e itch.io. Úsala cuando el usuario quiera sacar una versión, etiquetar una release o revisar el estado del CI/CD.
---

# Publicar una versión

Referencia completa: [docs/CI.md](../../../docs/CI.md). Workflows en `.github/workflows/` (`ci.yml`, `web-dev.yml`, `release.yml`).

## 1. Comprobaciones previas
- Rama `master` limpia (`git status`) y al día con `origin/master`.
- Tests en verde en local (skill `unity-test`: EditMode y PlayMode) y consola sin errores.
- Si cambió maquetación o assets: `Escoba/Build/Rebuild All` ya ejecutado y commiteado.
- Decidir el número con SemVer: **parche** (correcciones), **menor** (funcionalidades, p. ej. un hito), **mayor** (cambios incompatibles, p. ej. formato de partidas guardadas). Sufijo `-beta.N` para prerelease.

## 2. Preparar
1. `CHANGELOG.md`: renombrar `## [Unreleased]` → `## [X.Y.Z] - AAAA-MM-DD` y añadir encima un `## [Unreleased]` vacío. El workflow usa esa sección como notas de la release.
2. `PlayerSettings.bundleVersion = "X.Y.Z"` y `PlayerSettings.Android.bundleVersionCode = mayor*10000 + menor*100 + parche` (vía MCP `execute_code` + `AssetDatabase.SaveAssets()`), para que local y CI coincidan.
3. `docs/ROADMAP.md`: anotar la versión en el hito correspondiente.
4. Commit: `chore: versión X.Y.Z`.

## 3. Publicar (solo si el usuario lo pide expresamente)
- `git tag vX.Y.Z` y `git push origin master vX.Y.Z`.
- Seguir el workflow *Release* en GitHub Actions (no hay `gh` instalado en local: indicar al usuario la URL `https://github.com/resuacode/escoba/actions`).

## 4. Después
- Comprobar la release (3 ficheros: `-WebGL.zip`, `-Windows.zip`, `-Android.apk`), la web estable en la raíz de Pages y, si aplica, itch.io.
- Si algo falla, no borrar la etiqueta sin consultar al usuario; corregir y publicar `X.Y.(Z+1)`.

## Notas
- La web de pruebas (`/dev/`) se publica sola tras cada CI en verde en `master`; no requiere release.
- iOS no está en CI (decisión 2026-11-23).
- Nunca commitear keystores, `.ulf` ni contraseñas: van en *GitHub Secrets*.
