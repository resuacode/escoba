# Cómo sustituir las cartas de Kenney por tu baraja española

Esta guía explica, paso a paso, cómo cambiar las cartas temporales (Kenney Playing Cards Pack, póker) por tus propios sprites de la baraja española **sin tocar código**.

> Estado: la herramienta `Escoba/Cards/Generate Card Skin...` y el `CardSkin` se implementan en el hito **M2**. Hasta entonces, esta guía sirve como especificación para preparar tu arte.

## Cómo funciona (en 30 segundos)
El juego nunca usa los sprites directamente. Usa un asset **`CardSkin`** que dice "para el 7 de oros usa este sprite, para el dorso este otro". Cambiar de baraja = crear un `CardSkin` nuevo con tus sprites y seleccionarlo en `GameSettings`.

---

## Paso 1 — Prepara tus imágenes
Necesitas **41 imágenes**: 40 caras + 1 dorso.

| Requisito | Valor recomendado |
|---|---|
| Formato | PNG de 32 bits con transparencia (esquinas redondeadas transparentes) |
| Tamaño (pixel art) | **60 × 90 px** (relación 2:3), exportado a escala 1×, sin suavizado. En la mesa la carta mide 180 px de alto en la resolución de referencia (1920×1080): cada píxel se ve exactamente como 2×2 (y 4×4 en 4K) |
| Tamaño (si no fuese pixel art) | 256 × 384 px (relación 2:3); opcional HD 512 × 768 |
| Todas iguales | Mismo tamaño exacto para las 41 imágenes |
| Margen | Sin márgenes transparentes extra alrededor de la carta (el borde de la carta toca el borde de la imagen) |
| Espacio de color | sRGB |
| Legibilidad | Número y palo visibles en la esquina superior izquierda (en la mano las cartas se solapan) |

## Paso 2 — Nómbralas así
Formato: `<palo>_<número>.png`, todo en minúsculas, número con dos dígitos.

| Palo | Ficheros |
|---|---|
| Oros | `oros_01.png` … `oros_07.png`, `oros_10.png` (sota), `oros_11.png` (caballo), `oros_12.png` (rey) |
| Copas | `copas_01.png` … `copas_12.png` (mismos números) |
| Espadas | `espadas_01.png` … `espadas_12.png` |
| Bastos | `bastos_01.png` … `bastos_12.png` |
| Dorso | `dorso.png` |

> No existen el 8 ni el 9: tras el 7 van 10, 11 y 12.

Lista de comprobación rápida (10 por palo): `01 02 03 04 05 06 07 10 11 12`.

## Paso 3 — Cópialas al proyecto
1. Con Unity **abierto**, arrastra las 41 imágenes a la carpeta del Project window:
   `Assets/_Project/Art/Cards/Spanish/`
2. Añade en esa carpeta un `License.txt` indicando autor y licencia de tu arte.

## Paso 4 — Ajustes de importación (automáticos)
No tienes que tocar nada: el postprocesador del proyecto (`CardTexturePostprocessor`) aplica estos ajustes al importar cualquier imagen dentro de `Art/Cards/`. Si quieres comprobarlo, selecciona una imagen y revisa en el Inspector:
- **Texture Type:** Sprite (2D and UI) · **Sprite Mode:** Single · **Mesh Type:** Full Rect
- **Generate Mip Maps:** desactivado · **Filter Mode:** Point (no filter) · **Compression:** None

La carpeta `Spanish/` usa el perfil *pixel art*, igual que Kenney (decisión del 2026-11-25). Cualquier otra carpeta de `Art/Cards/` usa el perfil de arte ilustrado: Bilinear, Max Size 1024, compresión automática.

> Si cambias algo en el Inspector, se perderá en la siguiente reimportación: los ajustes se cambian en el postprocesador.

## Paso 5 — Genera el CardSkin
1. Menú **`Escoba → Cards → Generate Card Skin...`**
2. Carpeta: `Assets/_Project/Art/Cards/Spanish` · Nombre: `Spanish`
3. Pulsa **Generate**. Se crea `Assets/_Project/Data/CardSkins/CardSkin_Spanish.asset`.
4. Si falta alguna imagen, la ventana lista exactamente cuáles (p. ej. `copas_11.png`). Añádelas y repite.

## Paso 6 — Actívalo
1. Selecciona `Assets/_Project/Data/GameSettings.asset`.
2. En **Active Card Skin** arrastra `CardSkin_Spanish`.
3. Guarda (Ctrl+S).

## Paso 7 — Valida
1. Menú **`Escoba → Cards → Validate Active Card Skin`** → debe decir "40/40 caras, dorso OK, tamaños consistentes".
2. Pulsa Play y juega una ronda; o usa `Escoba → Debug → Show All Cards` para ver las 40 cartas en la mesa.
3. (Opcional) Pide a Claude: *"usa la skill card-skin y unity-playtest para validar la nueva baraja"*.

## Paso 8 — Retira Kenney (opcional)
Cuando estés satisfecho:
1. Borra `Assets/_Project/Art/Cards/Kenney/` y `CardSkin_Kenney.asset` desde Unity (no desde el explorador de Windows, para que se borren los `.meta`).
2. Elimina la línea de Kenney en la sección de créditos del `README.md` (si ya no se usa).
3. Haz commit incluyendo los `.meta`.

---

## Referencia: mapeo temporal Kenney → española
| Póker (Kenney) | Española |
|---|---|
| ♦ Diamonds | Oros |
| ♥ Hearts | Copas |
| ♠ Spades | Espadas |
| ♣ Clubs | Bastos |
| A, 2, 3, 4, 5, 6, 7 | 1, 2, 3, 4, 5, 6, 7 |
| J | Sota (10, vale 8) |
| Q | Caballo (11, vale 9) |
| K | Rey (12, vale 10) |
| 8, 9, 10, Jokers, `card_empty` | No se usan |

Las cartas de Kenney ya están convertidas con este mapeo en `Assets/_Project/Art/Cards/Kenney/`, recortadas a 42×60 px y **con los mismos nombres que tu baraja** (`oros_07.png`, `dorso.png`…). Por eso, sustituirlas consiste solo en poner tus 41 imágenes en `Art/Cards/Spanish/` y generar su `CardSkin`. Kenney es pixel art y se importa con filtro *Point*; tu baraja no tiene por qué serlo: el `CardSkin` guarda la relación de aspecto de cada baraja y el layout se adapta.

## Problemas frecuentes
| Síntoma | Causa | Solución |
|---|---|---|
| Carta rosa o vacía | Falta sprite o nombre mal escrito | Revisa el informe de `Generate Card Skin` |
| Cartas borrosas | Max Size bajo o compresión agresiva | Sube Max Size a 1024 / usa ASTC 4×4 |
| Cartas estiradas | Imágenes con distinto tamaño | Exporta las 41 con el mismo tamaño (60 × 90) |
| Píxeles de distinto grosor | El tamaño en pantalla no es múltiplo entero del de la imagen | Mantén 60 × 90 (×2 en la mano). Las manos rivales (110 px de alto) no son múltiplo exacto; si se nota, se ajustan a 90 px (×1) en `TablePresenter` |
| Bordes blancos | Sin transparencia en esquinas | Exporta PNG con canal alfa |
