# Integración y entrega continuas (CI/CD)

Tres workflows de GitHub Actions en `.github/workflows/`, basados en [GameCI](https://github.com/game-ci) (`game-ci/unity-test-runner@v4`, `game-ci/unity-builder@v6`, imágenes `unityci/editor` de Unity 6000.3.11f1):

| Workflow | Cuándo | Qué hace |
|---|---|---|
| `ci.yml` — **CI** | Cada push (cualquier rama) y cada pull request; ignora cambios solo de documentación | Tests EditMode + PlayMode; resultados como *check* en GitHub y como artefacto |
| `web-dev.yml` — **Web (dev)** | Cuando CI termina en verde en `master` (o a mano) | Build WebGL con versión `X.Y.Z-dev.N+commit` → GitHub Pages en **`/dev/`** (la raíz conserva la última release) |
| `release.yml` — **Release** | Al subir una etiqueta `vX.Y.Z` | Tests → builds **WebGL, Windows y Android (APK)** → release de GitHub con los ficheros y las notas del CHANGELOG → web estable en la **raíz** de Pages → itch.io (si está configurado) |

iOS queda fuera de CI por ahora (decisión 2026-11-23): se compilará en local cuando haga falta.

```
https://resuacode.es/escoba/        ← última release estable
https://resuacode.es/escoba/dev/    ← último commit de master con tests en verde
```
`resuacode.es` es el dominio del sitio de usuario (`resuacode.github.io`); GitHub sirve cada repositorio con Pages en `resuacode.es/<nombre-del-repo>/`. La ruta del proyecto es siempre el nombre del repo: una ruta hermana como `/escoba-dev` exigiría otro repositorio, por eso la versión de pruebas va en la subcarpeta `/dev/`.

El menú principal muestra la versión en la esquina inferior derecha (`VersionLabel`), para saber qué build se está probando.

---

## Configuración inicial (una vez)

### 1. Licencia de Unity (obligatorio)
Licencia **Personal** (gratuita), según la [guía de activación de GameCI](https://github.com/game-ci/documentation/blob/main/docs/03-github/02-activation.mdx):
1. En Unity Hub: *Preferences → Licenses → Add → Get a free personal license*. **Aunque ya aparezca una licencia, pulsa *Add***: Unity Hub 3.x guarda la licencia normal en `%LOCALAPPDATA%\Unity\licenses\UnityEntitlementLicense.xml`, que **no sirve** para CI; el fichero `.ulf` solo se crea al añadirla con *Add*. Si no aparece la opción, la guía de GameCI remite a su Discord.
2. Copia el contenido de `C:\ProgramData\Unity\Unity_lic.ulf` (carpeta oculta: escribe la ruta directamente en el explorador).
3. En GitHub: *Settings → Secrets and variables → Actions → New repository secret*:
   | Secret | Valor |
   |---|---|
   | `UNITY_LICENSE` | Contenido completo del `.ulf` |
   | `UNITY_EMAIL` | Correo de tu cuenta de Unity |
   | `UNITY_PASSWORD` | Contraseña de tu cuenta de Unity |

Los secrets no son visibles aunque el repositorio sea público, y no se pasan a pull requests de forks (en esos PR los tests de CI fallarán por falta de licencia; es lo esperado).
Si CI falla con un error de "seats"/activaciones, revisa la página *Returning a license* de GameCI.

### 2. GitHub Pages en `resuacode.es/escoba`
1. *Settings → Pages → Build and deployment → Source:* **GitHub Actions**.
2. *Settings → Pages → Custom domain:* **dejarlo vacío** en este repositorio. El dominio `resuacode.es` ya está en el repo `resuacode.github.io` y los proyectos lo heredan como `resuacode.es/escoba/`. Poner `resuacode.es` aquí entraría en conflicto con el sitio de usuario. No hace falta tocar el DNS.
3. *Settings → Environments → github-pages → Deployment branches and tags:* añade la regla de etiqueta **`v*`** además de `master`, para que el workflow de release pueda publicar la web estable.

La web usa compresión Brotli con **Decompression Fallback** activado (`PlayerSettings.WebGL.decompressionFallback`), porque GitHub Pages no permite configurar la cabecera `Content-Encoding`.

### 3. itch.io (opcional, solo releases)
*Butler* es la herramienta oficial de itch.io para subir builds desde la línea de comandos; el workflow la descarga sola. Solo necesita saber **a qué juego** subir (`ITCH_TARGET`) y **con qué permiso** (`BUTLER_API_KEY`).

1. **Crear el juego en itch.io**: inicia sesión en <https://itch.io> → menú de tu usuario → *Upload new project*.
   - *Title*: Escoba. *Project URL*: queda como `https://<tu-usuario>.itch.io/escoba`. La parte final (`escoba`) es el **slug**.
   - *Kind of project*: **HTML**.
   - *Visibility*: **Draft** (solo tú) o **Restricted** (con contraseña o lista de testers) mientras pruebas.
   - Guarda sin subir nada: los ficheros los sube el workflow.
2. **`ITCH_TARGET`** = `<tu-usuario-de-itch>/<slug>`. Por ejemplo, si la URL es `https://resuacode.itch.io/escoba`, el valor es `resuacode/escoba`. En GitHub: *Settings → Secrets and variables → Actions → pestaña **Variables** → New repository variable*.
3. **`BUTLER_API_KEY`**: en itch.io, menú de usuario → *Settings* → *API keys* (<https://itch.io/user/settings/api-keys>) → *Generate new API key* y copia la clave. En GitHub: pestaña **Secrets** → *New repository secret*.
4. Tras la primera release, en *Edit game → Uploads* marca el fichero del canal `html5` como **This file will be played in the browser** y guarda.

Canales: `html5` (web), `windows`, `android`. Sin la variable `ITCH_TARGET`, el paso de itch.io se omite y el resto de la release funciona igual.

### 4. Firma de Android (recomendado antes de distribuir)
Android solo instala apps **firmadas**, y solo deja **actualizar** una app con otra versión firmada **con la misma clave**. El *keystore* es el fichero que guarda esa clave (como un certificado con contraseña).
- **Sin keystore** (lo que pasa ahora): CI firma el APK con una clave de depuración temporal. Sirve para probar, pero cada build puede llevar una clave distinta, así que para instalar una nueva a veces habrá que desinstalar la anterior.
- **Con keystore propio**: todas las versiones se firman igual y se actualizan sin desinstalar. Es lo que exigirá Google Play si algún día se publica allí.

**Estado (2026-11-24):** keystore ya generado en `Documents\escoba-android-keystore\` del equipo de desarrollo (`escoba.keystore`, alias `escoba`, PKCS12, RSA 2048, validez ~27 años). La carpeta incluye `ANDROID_KEYSTORE_BASE64.txt` y `LEEME-secrets.txt` con los valores de los cuatro secrets. El proyecto tiene activado *Custom Keystore* (`escoba.keystore`, alias `escoba`); las contraseñas no se guardan en el proyecto. Hay que guardar esa carpeta con copia de seguridad.

Para crear otro (solo si se perdiera el actual, sabiendo que no podrá actualizar instalaciones firmadas con el anterior):
1. Genera el keystore (con el JDK que incluye Unity):
   ```powershell
   & "C:\Program Files\Unity\Hub\Editor\6000.3.11f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe" `
     -genkeypair -v -keystore escoba.keystore -alias escoba -keyalg RSA -keysize 2048 -validity 10000
   ```
2. **Guarda el keystore y las contraseñas en un lugar seguro y fuera del repositorio** (si se pierden, no se puede volver a firmar la misma app).
3. Secrets: `ANDROID_KEYSTORE_BASE64` (`[Convert]::ToBase64String([IO.File]::ReadAllBytes("escoba.keystore"))`), `ANDROID_KEYSTORE_PASS`, `ANDROID_KEYALIAS_NAME` (`escoba`), `ANDROID_KEYALIAS_PASS`.

Identificador de la app: `es.resuacode.escoba` (dominio `resuacode.es` al revés; no cambiarlo tras publicar).

---

## Publicar una versión
Usa la skill **`/release`** (o sigue estos pasos):
1. En `CHANGELOG.md`, renombra `## [Unreleased]` a `## [X.Y.Z] - AAAA-MM-DD` y deja un `## [Unreleased]` vacío encima.
2. Pon `X.Y.Z` en *Player Settings → Version* (`bundleVersion`), para que coincida en local.
3. Commit en `master` y etiqueta: `git tag vX.Y.Z && git push origin master vX.Y.Z`.
4. Sigue el workflow *Release* en la pestaña *Actions*. Al terminar: release en GitHub con `Escoba-X.Y.Z-WebGL.zip`, `Escoba-X.Y.Z-Windows.zip` y `Escoba-X.Y.Z-Android.apk`; web estable en la raíz de Pages; itch.io actualizado.

Versiones con sufijo (`v0.5.0-beta.1`) se publican como *prerelease*. El `versionCode` de Android se calcula como `mayor·10000 + menor·100 + parche`.

## Tiempos y costes
- Repositorio público: minutos de GitHub Actions gratuitos.
- Cada build de Unity tarda ~15-30 min la primera vez; la caché de `Library` (por plataforma) los reduce en ejecuciones siguientes.
- El workflow de release compila las tres plataformas en paralelo.

## Problemas frecuentes
| Síntoma | Solución |
|---|---|
| CI falla en la activación | Revisa los tres secrets de Unity; vuelve a generar el `.ulf` |
| La web carga en blanco en Pages | Comprueba que *Decompression Fallback* sigue activado |
| *Release* no puede desplegar Pages | Añade la regla de etiquetas `v*` al entorno `github-pages` |
| Paso de itch.io falla | Falta `BUTLER_API_KEY` o `ITCH_TARGET` no es `usuario/juego`. Tras corregirlo, resube sin recompilar con *Actions → itch.io (manual) → Run workflow* indicando la etiqueta (`itch.yml`); *Re-run* del job usaría el workflow antiguo de la etiqueta |
| `Could not resolve host` al instalar butler | El servidor de descargas de itch.io es `broth.itch.zone` (el antiguo `broth.itch.ovh` ya no existe) |
| El APK no se puede actualizar | Se instaló uno firmado con otra clave: desinstala y reinstala |
