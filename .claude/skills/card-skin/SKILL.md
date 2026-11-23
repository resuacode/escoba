---
name: card-skin
description: Importa, configura o sustituye la baraja de cartas (sprites) del juego Escoba mediante ScriptableObjects CardSkin. Úsala para preparar la baraja temporal de Kenney, crear la herramienta de generación de CardSkin o sustituirla por los sprites propios de baraja española.
---

# Barajas (CardSkin)

Guía de usuario detallada: [docs/CARD_ART_REPLACEMENT.md](../../../docs/CARD_ART_REPLACEMENT.md). Mantén ambas coherentes.

## Modelo
- `CardSkin` (ScriptableObject, `Escoba.Presentation`): `Sprite back`, `Sprite[] faces` indexado por id de carta (0‑39: `suit * 10 + ordinal`, ordinal 0‑9 para 1,2,3,4,5,6,7,Sota,Caballo,Rey), más metadatos (`displayName`, `aspectRatio`, `author`, `license`).
- `CardView` pide el sprite a `CardSkin.GetFace(Card)`; nunca carga sprites por nombre en runtime.
- El skin activo se asigna en `GameSettings` (App). Cambiar de skin no requiere tocar código.

## Convención de nombres de fichero (baraja española propia)
`<palo>_<NN>.png` con palo ∈ `oros|copas|espadas|bastos` y NN ∈ `01..07, 10, 11, 12`; dorso `dorso.png`. Carpeta: `Assets/_Project/Art/Cards/Spanish/`.

## Mapeo temporal Kenney → española
| Kenney | Española |
|---|---|
| Diamonds | Oros |
| Hearts | Copas |
| Spades | Espadas |
| Clubs | Bastos |
| A, 2‑7 | 1‑7 |
| J / Q / K | Sota / Caballo / Rey |
Se descartan 8, 9, 10, jokers y `card_empty`.

### Baraja Kenney instalada (2026-11-23)
- `Assets/_Project/Art/Cards/Kenney/` contiene **solo** 41 PNG ya convertidos a la **convención española** (`oros_01.png` … `bastos_12.png`, `dorso.png`) + `License.txt` (CC0). El resto del pack (tamaños medium/small, UNO, dados, tilesheets, previews, 8/9/10, jokers) se eliminó para no cargar el repo.
- Conversión hecha con `Escoba.Editor.Cards.KenneyCardImporter` (menú `Escoba/Cards/Import Kenney Pack...`): toma `PNG/Cards (large)/card_<suit>_<rank>.png` del pack original (lienzo 64×64), recorta la carta (`RectInt(11, 2, 42, 60)`, origen abajo‑izquierda) y la guarda renombrada. Para regenerar, descarga el pack de nuevo y apunta el menú a esa carpeta (fuera de `Assets/`).
- Son **pixel art** de **42×60 px** (aspecto 0,7). En pantalla, escalar a múltiplos enteros (×3, ×4…) para que no se vean borrosas.
- Como Kenney ya usa los nombres españoles, el generador de `CardSkin` solo necesita un perfil de nombres (`Spanish`).

### Importación automática
`Escoba.Editor.Cards.CardTexturePostprocessor` aplica los ajustes a todo `Art/Cards/`:
- Común: Sprite (2D and UI), Single, Full Rect, pivot centro, PPU 100, sin mipmaps, Clamp, alpha is transparency.
- `Art/Cards/Kenney/` (pixel art): Filter **Point**, **sin compresión**.
- Resto (baraja propia): Bilinear, compresión automática, Max Size 1024.
El postprocesador **sobrescribe** estos ajustes en cada reimportación; para cambiarlos, edita el postprocesador, no el Inspector.

## Herramienta de editor (Escoba.Editor) — implementada en M2
`Escoba.Editor.Cards.CardSkinGenerator`:
- Menú `Escoba/Cards/Generate Card Skin...`: elige carpeta (convención española); crea/actualiza `Assets/_Project/Data/CardSkins/CardSkin_<NombreCarpeta>.asset`; si faltan imágenes, las lista. Programático: `CardSkinGenerator.Generate(folder, name, pixelArt, author, license)`.
- Menú `Escoba/Cards/Validate Active Card Skin`: 40 caras, dorso y tamaño uniforme (`CardSkinGenerator.Validate(skin)`).
- El skin activo está en `Assets/_Project/Data/GameSettings.asset` (campo `Card Skin`). `DefaultAssetsBuilder` (menú `Escoba/Build/...`) asigna `CardSkin_Kenney`: si cambias de baraja, actualiza también `DefaultAssetsBuilder.CreateAll` o volverá a poner Kenney al reconstruir.
- Test: `ProjectAssetsTests.ActiveCardSkin_IsComplete`.

## Ajustes de importación recomendados para la baraja propia (aplicar con `manage_asset` o un `AssetPostprocessor` para `Art/Cards/Spanish`; Kenney usa el perfil pixel art de arriba)
- Texture Type: Sprite (2D and UI), Sprite Mode: Single, Mesh Type: Full Rect.
- Pixels Per Unit: 100 (irrelevante en uGUI, pero uniforme).
- Filter: Bilinear; Generate Mip Maps: off; Max Size: 512 (1024 si el arte lo merece).
- Compresión: Automatic; override Android/iOS ASTC 6×6, WebGL ASTC/DXT según target.
- Agrupar en un `SpriteAtlas` (`Art/Cards/<Nombre>/CardsAtlas.spriteatlasv2`).

## Verificación
- Test EditMode `CardSkinValidationTests` (cuando exista Presentation testeable) o el menú de validación.
- `unity-playtest`: mesa con todas las cartas visibles (menú debug "Show All Cards").
- Licencias: `License.txt` en la carpeta del arte y créditos en README.
