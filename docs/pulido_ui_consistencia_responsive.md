# Pulido General de UI: Consistencia y Responsive

Este documento detalla el diagnóstico de hallazgos de diseño, las soluciones de interfaz reutilizables implementadas, los ajustes de responsive móvil y el instructivo de pruebas de la tarea **Pulido general de UI: consistencia y responsive** en el CRM (módulo Asesor, listas de ventas, pre-ventas y navegación global).

---

## 1. Puntos Encontrados y Diagnóstico de Hallazgos

Durante el análisis y pruebas de la interfaz de usuario se identificaron los siguientes puntos de mejora visual, rendimiento de percepción y usabilidad:

### A. Ausencia de Skeletons de Carga en Listas
* **Comportamiento observado**: Al cargar el listado de ventas (`/asesor/orders`) o pre-ventas (`/asesor/preventas`), el sistema mostraba spinners planos o filas de tabla estáticas sin animación, provocando saltos de diseño (*layout shift*) y una experiencia de carga tosca.
* **Causa raíz**: No existía un componente estandarizado de carga tipo *Shimmer* (Skeleton loader) en el frontend del cliente Blazor WASM / InteractiveServer.

### B. Falta de Presentación Visual en Estados Vacíos (Empty States)
* **Comportamiento observado**: Cuando una búsqueda o filtro no devolvía resultados (o el usuario no tenía registros asignados), la pantalla mostraba únicamente un mensaje plano de texto o una celda de tabla vacía sin guía de acción.
* **Causa raíz**: No se contaba con un componente ilustrado reutilizable que guiara al usuario para crear un nuevo registro o limpiar sus filtros.

### C. Manejo Genérico o Nulo de Errores de Conexión de API
* **Comportamiento observado**: Ante una pérdida de red o caída del servicio backend REST, las listas quedaban en estado indefinido de carga o mostraban errores no formateados en consola.
* **Causa raíz**: Las capturas de excepción en los componentes Blazor no tenían un estado visual dedicado con botón de reintento directo para recuperar la comunicación con el servidor.

### D. Inconsistencia en Colores de Badges de Estado
* **Comportamiento observado**: Los badges de estado en tablas y tarjetas utilizaban colores planos de Bootstrap o fallbacks oscuros que dificultaban la lectura rápida o rompían la armonía del diseño.
* **Causa raíz**: No se aplicaba de forma uniforme la propiedad `order_status.color` proveniente del catálogo del servidor con mezclas dinámicas de transparencia (`color-mix`).

### E. Ausencia del Menú de Navegación en Vista Móvil (< 768px)
* **Comportamiento observado**: Al visualizar la aplicación en dispositivos móviles o emuladores de pantallas pequeñas (< 768px), el menú lateral (`.custom-sidebar`) se ocultaba completamente sin ofrecer una alternativa de navegación.
* **Causa raíz**: Las reglas de diseño responsive en CSS aplicaban `display: none` a la barra lateral sin incluir un botón hamburguesa ni un panel lateral desplegable (*Offcanvas Drawer*).

---

## 2. Cambios y Soluciones Implementadas

### A. Creación de Componentes Reutilizables de UI
* **Archivos creados**:
  * [EmptyState.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Components/UI/EmptyState.razor): Componente con ícono ilustrado en contenedor circular, título, mensaje descriptivo y botón de llamada a la acción (*Call-to-Action*).
  * [LoadingSkeletonTable.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Components/UI/LoadingSkeletonTable.razor): Marcador de posición de tabla con animación de efecto *Shimmer* (`@@keyframes skeleton-shimmer-anim`).
  * [ApiErrorBanner.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Components/UI/ApiErrorBanner.razor): Banner de aviso ante errores de red/API con botón de reintento (*"Reintentar conexión"*).

### B. Integración en Listados de Ventas y Pre-Ventas
* **Archivos modificados**:
  * [Orders.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/Orders.razor)
  * [PreSales.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/PreSales.razor)
* **Detalle del cambio**:
  1. Integración de `<LoadingSkeletonTable>` durante `isLoading == true`.
  2. Integración de `<EmptyState>` cuando la lista filtrada no contiene registros.
  3. Integración de `<ApiErrorBanner>` al capturar excepciones HTTP en `LoadOrdersAsync` y `LoadPreSalesAsync`.
  4. Aplicación de insignias tipo píldora con `color-mix(in srgb, StatusColor 15%, transparent)` y bordes sutiles a juego con el color oficial del estado.

### C. Menú Hamburguesa y Cajón Desplegable Móvil (Offcanvas Drawer)
* **Archivos modificados**:
  * [MainLayout.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Layout/MainLayout.razor)
  * [dashboard.css](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/wwwroot/css/dashboard.css)
* **Detalle del cambio**:
  1. Se incorporó el botón hamburguesa (`bi-list`) visible únicamente en dispositivos móviles (`d-md-none`) en el encabezado superior.
  2. Se desarrolló el panel flotante lateral `mobile-sidebar-drawer` con animación `slideInLeft` y fondo de desenfoque `mobile-sidebar-backdrop`.
  3. El cajón incluye la navegación completa (`NavMenu`), avatar de usuario, acceso a *Configuración* y *Cerrar Sesión*, y se cierra automáticamente al seleccionar cualquier ruta.

### D. Tooltips de Ayuda y Ajustes Form-Control en Pantallas Pequeñas
* **Archivos modificados**:
  * [NewOrder.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/NewOrder.razor)
  * [dashboard.css](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/wwwroot/css/dashboard.css)
* **Detalle del cambio**:
  1. Incorporación de `<MudTooltip>` sobre los íconos de información en campos con validaciones sintácticas (DNI Módulo 23, IBAN Módulo 97, Teléfono ES, CUPS).
  2. Inclusión de `font-size: 16px !important` en selecciones e inputs para prevenir el zoom automático no deseado en navegadores móviles (iOS/WebKit).
  3. Apilado responsivo de botones inferiores (`w-100 w-sm-auto` y `flex-column flex-sm-row`).

---

## 3. Instructivo para Validación y Pruebas (Pulido UI & Responsive)

Para evaluar y verificar cada una de las mejoras implementadas, seguir la siguiente guía de pruebas:

### Paso 1: Verificación de Skeletons Shimmer
1. Ingresa al CRM como Asesor (`test.asesor` / `password123`).
2. Navega a **Mis Ventas** (`/asesor/orders`) o **Mis Pre-Ventas** (`/asesor/preventas`) y presiona `F5` para recargar.
3. **Resultado esperado**: Durante la respuesta de la API, la tabla muestra líneas animadas en gris translúcido (*Shimmer effect*) sin saltos de estructura.

### Paso 2: Verificación de Badges de Estado Dinámicos
1. Observa la columna **Estado** en **Mis Ventas**.
2. **Resultado esperado**: Cada estado muestra una píldora estilizada con transparencia suave calculada a partir de `order_status.color` y texto en alto contraste.

### Paso 3: Verificación de Estado Vacío (Empty State)
1. En **Mis Ventas** (`/asesor/orders`), aplica un filtro por rango de fechas en el futuro (ej. año 2030) y presiona **Buscar**.
2. **Resultado esperado**: Se despliega la tarjeta ilustrada con ícono circular, título *"No se encontraron órdenes"*, descripción clara y el botón principal *"Generar Nueva Orden"*.
3. Haz clic en **Limpiar** para restaurar los registros.

### Paso 4: Verificación de Tooltips en Formulario
1. Ingresa a **Generar Nueva Orden** (`/asesor/orders/new`).
2. Pasa el cursor o presiona el ícono `(i)` al lado de los campos *DNI*, *IBAN* o *Teléfono*.
3. **Resultado esperado**: Aparece el tooltip emergente indicando la estructura exigida (ej. *DNI/NIE Módulo 23 de España*).

### Paso 5: Verificación de Menú Móvil Hamburguesa (Offcanvas)
1. Abre las herramientas de desarrollador (`F12`) y activa el modo móvil (ej. pantalla 375px).
2. Ubica el botón de tres líneas (hamburguesa) en la esquina superior izquierda al lado de *Inicio*.
3. Haz clic en el botón hamburguesa.
4. **Resultado esperado**: Se abre suavemente desde la izquierda el cajón de navegación móvil con el menú completo (`NavMenu`), perfil del usuario y botones de configuración. Al hacer clic en una opción o en el fondo oscuro, el menú se cierra automáticamente.

### Paso 6: Verificación de Error de API
1. Detén temporalmente la ejecución de `CRM.ApiHub`.
2. Recarga **Mis Ventas** (`/asesor/orders`).
3. **Resultado esperado**: La UI muestra la tarjeta de error `ApiErrorBanner` avisando el problema de conexión con el botón *"Reintentar conexión"*. Vuelve a iniciar el backend y haz clic en reintentar.
