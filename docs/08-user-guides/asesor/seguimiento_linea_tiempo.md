# Seguimiento de Línea de Tiempo (Bandeja del Asesor)

## Objetivo
Visualizar el progreso de una orden de venta de manera rápida e intuitiva directamente desde la bandeja principal, y acceder al detalle completo de las etapas sin necesidad de navegar hacia la ficha de la orden.

## Requisitos
- Permiso de Asesor o rol superior.
- Módulo: Bandeja del Asesor (`AsesorDashboard.razor`)

## Pasos
1. Abrir la Bandeja del Asesor.
2. Ubicar la orden de venta que deseas consultar en la tabla principal.
3. Observar la columna **Línea de Tiempo**, la cual muestra una ruta interactiva de puntos animados:
   - **Puntos dorados**: Representan las etapas que ya han sido completadas.
   - **Punto dorado con animación (pulso)**: Representa la etapa en la que se encuentra la orden actualmente.
   - **Puntos grises**: Representan las etapas pendientes.
4. Presionar sobre la tarjeta animada de la Línea de Tiempo de la orden deseada.
5. Se abrirá un modal flotante con la barra de progreso horizontal completa (mostrando el nombre de todas las etapas).
6. Para salir, presionar la **"X"** (Cerrar) en el modal para volver a la bandeja, o presionar el botón **"Ir a Ficha"** para entrar al detalle completo de la orden si se requiere más información.

## Resultado esperado
El asesor puede identificar instantáneamente en qué etapa exacta de la línea de tiempo se encuentra cualquier orden gracias a la animación y los puntos de color, ahorrando el tiempo que tomaba cargar la pantalla de ficha para cada orden individual.

## Problemas frecuentes
- **El modal no se abre o no hay animación**: Verificar que se haya refrescado el caché del navegador (Ctrl + F5).
- **Los puntos aparecen desalineados**: Asegurarse de no tener un zoom excesivo en el navegador que rompa la tabla responsiva.

## Última actualización
2026-08-14
