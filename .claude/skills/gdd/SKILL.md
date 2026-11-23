---
name: gdd
description: Amplía, revisa o modifica el Game Design Document (docs/GDD.md) de la Escoba manteniendo su estructura, nivel de detalle y coherencia con las reglas y la arquitectura. Úsala cuando se discutan o decidan cambios de diseño, UX, IA, arte, audio, modos o alcance.
---

# Mantener el GDD

Fichero: `docs/GDD.md`. Idioma: español. Es un documento vivo: refleja **decisiones**, no ideas sueltas.

## Procedimiento
1. Lee el GDD completo y la skill `escoba-rules` (las reglas del GDD §4 deben coincidir con ella; si cambias una, cambia la otra).
2. Identifica las secciones afectadas. No dupliques contenido: enlaza entre secciones.
3. Para cada cambio:
   - Redacta en estilo de especificación: qué, por qué, criterios de aceptación medibles.
   - Si hay decisiones abiertas, añádelas a **§ Preguntas abiertas** con fecha y opciones, en lugar de decidir por tu cuenta.
   - Wireframes en ASCII dentro de bloques de código; flujos con listas numeradas o diagramas Mermaid.
4. Actualiza la cabecera (**versión** semántica del documento y **fecha**) y el **historial de cambios** al final del GDD.
5. Si el cambio afecta al alcance o al calendario, actualiza `docs/ROADMAP.md`; si afecta a la arquitectura, `docs/ARCHITECTURE.md`.

## Estructura que debe mantenerse
1. Visión y pilares · 2. Público y plataformas · 3. Modos de juego · 4. Reglas (resumen + enlace a la skill) · 5. Configuración de partida · 6. Flujo de pantallas · 7. Pantallas y wireframes · 8. Interacción y UX · 9. IA · 10. Arte · 11. Audio · 12. Feedback y animación · 13. Accesibilidad · 14. Localización · 15. Ajustes y persistencia · 16. Multijugador online (futuro) · 17. Rendimiento y técnica · 18. Métricas y balance · 19. Riesgos · 20. Preguntas abiertas · 21. Historial.

## Criterios de calidad
- Cada feature tiene criterios de aceptación comprobables (tests o checklist de playtest).
- Sin contradicciones con CLAUDE.md (arquitectura) ni con `escoba-rules`.
- Nada de texto de relleno; si una sección no está decidida, dilo y enlaza a Preguntas abiertas.
