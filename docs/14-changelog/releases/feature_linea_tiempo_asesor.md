# Feature: Integración de Línea de Tiempo para Asesores

## Resumen
Se ha implementado una mejora sustancial en la Experiencia de Usuario (UX) de los Asesores al integrar la visibilidad de la **Línea de Tiempo** directamente en la bandeja principal de órdenes (`AsesorDashboard.razor`) y optimizar la pantalla de detalles (`AsesorOrderDetail.razor`).

## Cambios Realizados

### 1. Bandeja del Asesor (`AsesorDashboard.razor`)
- **Nueva columna "Línea de Tiempo":** Se agregó una columna dedicada para previsualizar el progreso de cada orden de venta.
- **Previsualización Animada (Puntos):** Se implementó un diseño de *mini-timeline* usando puntos (dots) conectados por una línea.
  - El diseño emplea colores institucionales (dorado).
  - Se agregó una animación CSS personalizada (`@@keyframes pulse-dot`) que hace "latir" el punto correspondiente a la etapa actual de la orden.
- **Modal de Progreso Completo:** Al hacer clic en la tarjeta de previsualización (la cual incluye el efecto hover y `stopPropagation` para no cambiar de vista), se despliega un modal emergente que muestra el flujo horizontal completo (`stage-strip`) sin abandonar la bandeja.

### 2. Detalle de Orden (`AsesorOrderDetail.razor`)
- **Rediseño del Layout:** Se limpió el diseño general de la ficha de detalle de la orden.
- **Extracción de la Línea de Tiempo:** Se removió la línea de tiempo gigante del encabezado de la ficha para liberar espacio vertical.
- **Nuevo Botón de Progreso:** Se incorporó un botón específico ("Ver Progreso / Línea de Tiempo") que, al ser presionado, despliega el modal flotante con la barra de etapas, manteniendo la coherencia visual con la bandeja.

## Beneficios
- **Reducción de clics:** El asesor ya no necesita entrar a la ficha de cada orden solo para saber en qué etapa se encuentra.
- **Mayor claridad visual:** Las animaciones y colores dirigen la atención inmediatamente al estado real del proceso.
- **Espacio optimizado:** La ficha de detalle ahora presenta la información de la orden y los documentos adjuntos de manera mucho más limpia y organizada.

## Estado
- **Completado y Desplegado**
