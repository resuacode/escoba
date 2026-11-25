# Guía de arte: tamaños y formatos

Qué imágenes necesita el juego, con qué tamaño y dónde van. Las cartas tienen su guía detallada en [CARD_ART_REPLACEMENT.md](CARD_ART_REPLACEMENT.md); aquí está el resumen de todo lo demás.

El juego es **pixel art**: se dibuja a tamaño pequeño y se amplía por múltiplos enteros, sin suavizado. El diseño de pantalla de referencia es **1920×1080**. En pantallas más panorámicas (móviles 20:9) sobra ancho y en más cuadradas (tablets 4:3) sobra alto, así que lo importante debe ir siempre en el centro 16:9.

## Resumen

| Elemento | Tamaño de dibujo | Formato | Carpeta / fichero |
|---|---|---|---|
| Cartas (40 caras) | **60 × 90 px** cada una | PNG 32 bits con transparencia | `Assets/_Project/Art/Cards/Spanish/oros_01.png`… |
| Dorso de carta | **60 × 90 px** | PNG 32 bits | `Assets/_Project/Art/Cards/Spanish/dorso.png` |
| Fondo de mesa | **720 × 360 px** (lo importante en el centro 480 × 320) | PNG | `Assets/_Project/Art/Table/table_background.png` |
| Icono de la app | **1024 × 1024 px** | PNG **sin transparencia** | `Assets/_Project/Art/Branding/icon_1024.png` |
| Icono adaptativo Android (fondo) | **432 × 432 px** | PNG sin transparencia | `Assets/_Project/Art/Branding/icon_android_background.png` |
| Icono adaptativo Android (primer plano) | **432 × 432 px**, dibujo dentro del círculo central de **264 px** | PNG con transparencia | `Assets/_Project/Art/Branding/icon_android_foreground.png` |
| Logo de la pantalla de arranque | **480 × 160 px** (o 240 × 80 a ×2) | PNG con transparencia | `Assets/_Project/Art/Branding/splash_logo.png` |
| Portada de itch.io | **630 × 500 px** (mínimo; ideal 1260 × 1000) | PNG/JPG | (se sube en la web de itch.io) |
| Capturas para itch.io | **1920 × 1080 px** | PNG | (se suben en la web de itch.io) |

## Cartas y dorso
- **60 × 90 px** (proporción 2:3), las 41 imágenes exactamente iguales. En la mano la carta mide 180 px de alto en el diseño de referencia: cada píxel se ve como **2×2** (4×4 en 4K).
- Esquinas redondeadas transparentes; el borde de la carta toca el borde de la imagen.
- Número y palo legibles en la **esquina superior izquierda**: en la mano las cartas se solapan.
- Nombres, importación y validación: [CARD_ART_REPLACEMENT.md](CARD_ART_REPLACEMENT.md).

## Fondo de mesa
- Dibuja a **720 × 360 px** (2:1). Se amplía ×3 hasta 2160 × 1080 y cubre la pantalla recortando lo que sobre:
  - en 16:9 se ve el centro **640 × 360**;
  - en móviles 20:9 se ve todo el ancho y se recorta un poco arriba y abajo (unos 18 px de dibujo por lado);
  - en tablets 4:3 se ve solo el centro 480 × 360.
- Pon los detalles importantes (logo grabado en el tapete, adornos) en el centro **480 × 320**, que se ve en todas las pantallas. Los bordes deben poder recortarse sin que se note.
- La mesa central (zona de cartas, 1000 × 440 en el diseño) se sigue dibujando encima como un rectángulo verde. Si tu fondo ya la incluye, dímelo y la quito.
- Colores oscuros o medios: encima van cartas claras, textos blancos y dorados.
- **Cómo se integra:** deja el fichero en `Assets/_Project/Art/Table/table_background.png` y ejecuta `Escoba → Build → Rebuild All`. Se importa solo como pixel art (filtro Point, sin compresión) y se pone en el menú y en la mesa. Sin el fichero, se usa el color de tapete actual.

## Icono de la app
- **1024 × 1024 px, sin transparencia** (iOS no la admite y Android la rellena de negro). Unity genera desde él todos los tamaños de Windows, Android y web.
- Si es pixel art, dibújalo a **64 × 64** o **128 × 128** y amplíalo por un entero (×16 u ×8) **sin suavizado** antes de exportar a 1024.
- Debe leerse bien a 48 × 48: una figura grande y simple (por ejemplo, una escoba sobre una carta o el siete de oros), con poco texto o ninguno.
- **Icono adaptativo de Android** (opcional pero recomendado): el móvil recorta el icono con forma de círculo, gota o cuadrado redondeado.
  - **Fondo:** 432 × 432 sin transparencia (puede ser un color liso o el tapete).
  - **Primer plano:** 432 × 432 con transparencia; el dibujo, dentro del círculo central de **264 px de diámetro** (lo de fuera puede recortarse).
- **Cómo se integra:** déjalos en `Assets/_Project/Art/Branding/` y pídeme que los configure en Player Settings.

## Pantalla de arranque
- Unity 6 permite quitar el logo de Unity también con licencia Personal. La pantalla de arranque puede mostrar **solo tu logo** sobre un color de fondo.
- Logo: **480 × 160 px** (3:1) con transparencia. Si es pixel art, dibújalo a 240 × 80 y amplíalo ×2 sin suavizado.
- Indica el **color de fondo** (por ejemplo, el verde tapete `#0D4526`) y si quieres fundido.
- **Cómo se integra:** déjalo en `Assets/_Project/Art/Branding/splash_logo.png` y pídeme que lo configure.

## itch.io
- **Portada** (*Cover image*): 630 × 500 px como mínimo; mejor 1260 × 1000 para pantallas de alta densidad. Es lo que se ve en los listados.
- **Capturas**: 1920 × 1080, de 3 a 5, con el menú, la mesa, una escoba y el resumen de ronda. Se pueden hacer con el medidor oculto apagado.
- **Fondo de página** (opcional): se configura en *Edit theme*.

## Consejos para pixel art
- Paleta limitada (16-32 colores) y la misma para cartas, fondo e iconos: el conjunto se verá coherente.
- Exporta **siempre a escala 1×** para cartas y fondo (el juego amplía). Para icono, logo e itch.io, amplía tú por un entero antes de exportar.
- Sin antialiasing ni suavizado al exportar.
- Herramientas gratuitas: **Pixelorama** (recomendada), **LibreSprite**, **Piskel**. De pago: **Aseprite**.
