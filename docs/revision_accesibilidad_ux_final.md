# Revisión de Accesibilidad y UX Final

Este documento detalla el diagnóstico de accesibilidad, las mejoras de interfaz de usuario implementadas, el flujo de bienvenida (*Onboarding*) para asesores nuevos y la guía de pruebas de la tarea **Revisión de accesibilidad y UX final** en el CRM (módulo Asesor, formularios dinámicos, pre-ventas y navegación global).

---

## 1. Puntos Encontrados y Diagnóstico de Accesibilidad y UX

Durante el análisis de usabilidad y cumplimiento del estándar de accesibilidad web (WCAG 2.1 AA) se identificaron los siguientes aspectos clave:

### A. Ausencia de Asociación de Labels e IDs en Formularios
* **Comportamiento observado**: Varios selectores e inputs en las pantallas de *Nueva Orden* (`/asesor/orders/new`), *Pre-Ventas* (`/asesor/preventas`) y filtros de búsqueda no contaban con la propiedad `for="id"` vinculada al `id="id"` del elemento de entrada.
* **Causa raíz**: Los componentes gráficos Bootstrap y MudBlazor declaraban etiquetas `<label>` adyacentes pero sin el enlace explícito necesario para tecnologías de asistencia y lectores de pantalla (*Screen Readers*).

### B. Falta de Diálogos de Confirmación en Acciones Destructivas
* **Comportamiento observado**: Al hacer clic en el botón *Cancelar* dentro del formulario de *Nueva Orden* o cerrar los modales de *Pre-Ventas* con datos parcialmente completados, la aplicación descartaba la información e interactuaba de forma inmediata sin solicitar confirmación.
* **Causa raíz**: No existían controladores de confirmación modal (`DialogService.ShowMessageBoxAsync`) previos al descarte de datos.

### C. Ausencia de Guía de Inicio para Asesores Nuevos
* **Comportamiento observado**: Al ingresar un asesor nuevo por primera vez a la bandeja de órdenes, la pantalla se mostraba limpia sin una orientación visual que explicara la secuencia de trabajo (*Pre-Ventas -> Generar Orden -> Base de Conocimiento*).
* **Causa raíz**: No se contaba con un componente interactivo de onboarding tipo banner ilustrado.

### D. Navegación por Teclado e Indicadores de Foco Visual
* **Comportamiento observado**: Al navegar con las teclas `Tab` y `Shift+Tab`, algunos botones y selectores personalizados no mostraban un contorno de foco definido (*focus outline*), dificultando la orientación en pantalla sin mouse.

---

## 2. Cambios y Soluciones Implementadas

### A. Estilo Global de Accesibilidad e Indicadores de Foco
* **Archivo modificado**: [dashboard.css](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/wwwroot/css/dashboard.css)
  * Se incorporó la regla `:focus-visible` con contorno de alto contraste (`outline: 2px solid #003B5C; outline-offset: 2px;`).
  * Se añadieron estilos para botones, entradas y selectores (`.btn:focus-visible`, `.form-control:focus-visible`, `.form-select:focus-visible`).
  * Se incluyó la clase `.sr-only` para texto exclusivo de lectores de pantalla.

### B. Componente de Onboarding de Asesor Nuevo
* **Archivo creado**: [AdvisorOnboardingBanner.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Components/UI/AdvisorOnboardingBanner.razor)
  * Tarjeta de bienvenida con degradado azul (`#003B5C` a `#0284c7`) e ícono animado de saludo.
  * Guía explicativa de 3 pasos:
    1. **Pre-Ventas y Prospectos**: Seguimiento de llamadas y prospección.
    2. **Generar Nueva Orden**: Alta de contratos con validaciones en tiempo real.
    3. **Base de Conocimiento (KB)**: Consulta de promociones, scripts y FAQs.
  * Botón de descarte interactivo para ocultar la guía al completar la lectura.
* **Integración**: Incorporado en [Orders.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/Orders.razor).

### C. Accesibilidad y Diálogo de Confirmación en Nueva Orden
* **Archivo modificado**: [NewOrder.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/NewOrder.razor)
  * Se asignaron atributos `for` e `id` explícitos en todos los campos:
    * `select-lead` -> *Cliente (Lead)*
    * `select-campaign` -> *Campaña*
    * `select-status` -> *Etapa (Pipeline)*
    * `field-{id_fld}` -> *Campos dinámicos del formulario*
  * Se agregaron atributos de descripción accesibles `aria-describedby="help-{id} error-{id}"` y marcado `role="alert"` en mensajes de error.
  * Se implementó el método `ConfirmCancelAsync()` utilizando `DialogService.ShowMessageBoxAsync` para solicitar confirmación antes de cancelar y salir de la pantalla.

### D. Accesibilidad y Confirmación en Pre-Ventas
* **Archivo modificado**: [PreSales.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/PreSales.razor)
  * Se añadieron atributos `for` e `id` en campos de filtrado y modales (`filter-operator`, `filter-coverage`, `presale-phone`, `presale-fname`, `presale-lname`, `presale-operator`, `presale-coverage`, `presale-notes`, `call-notes`).
  * Se implementó `ConfirmCloseNewModalAsync()` para requerir confirmación antes de cerrar el modal si el usuario ha comenzado a escribir datos.

---

## 3. Instructivo de Pruebas

Para verificar las mejoras de accesibilidad y UX implementadas:

### Prueba 1: Navegación por Teclado e Indicadores de Foco
1. Inicia sesión en el CRM como asesor e ingresa a `/asesor/orders`.
2. Presiona repetidamente la tecla `Tab`.
3. **Resultado esperado**: Cada elemento interactivo (filtro de campaña, estado, desde, hasta, botón buscar y botón nueva orden) se resalta con un borde azul visible de alto contraste (`outline: 3px solid #0284c7`).

### Prueba 2: Banner de Onboarding para Asesores
1. Accede a la bandeja de órdenes `/asesor/orders`.
2. **Resultado esperado**: Se muestra la tarjeta de bienvenida con los 3 pasos guiados (*Pre-Ventas*, *Nueva Orden* y *Base de Conocimiento*).
3. Haz clic en *"Entendido, ocultar guía de inicio"*. El banner se oculta suavemente.

### Prueba 3: Confirmación en Acción Destructiva (Cancelar Orden)
1. Navega a `/asesor/orders/new`.
2. Selecciona un cliente, una campaña y una etapa.
3. Rellena algunos campos del formulario dinámico.
4. Haz clic en el botón **Cancelar**.
5. **Resultado esperado**: Aparece la ventana modal de confirmación con el título *"¿Descartar Nueva Orden?"* y el mensaje *"¿Estás seguro de que deseas cancelar? Todos los datos ingresados en esta orden se perderán."*.
6. Haz clic en *"Volver a la orden"*: el diálogo se cierra y los datos permanecen intactos.
7. Haz clic en *"Sí, cancelar"*: la aplicación redirige a la bandeja `/asesor/orders`.

### Prueba 4: Inspección de Labels y Lectores de Pantalla
1. Abre Google Chrome DevTools (`F12`) en la pantalla de *Nueva Orden*.
2. Haz clic en el elemento inspeccionar y selecciona cualquier campo (ej. DNI, IBAN o Teléfono).
3. **Resultado esperado**: El elemento `<input id="field-X">` está asociado a su etiqueta correspondiente `<label for="field-X">` y posee atributos `aria-describedby` que vinculan el texto de ayuda e íconos de tooltip.
