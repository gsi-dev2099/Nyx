# Testing E2E Flujo Asesor y Formularios Dinámicos

Este documento detalla el diagnóstico de hallazgos, las modificaciones en la lógica de negocio y controladores, y la guía de pruebas E2E correspondientes al módulo de **Asesor** en el CRM (generación de orden, validación de formularios dinámicos, custodia de ventas y subida de documentos).

---

## 1. Puntos Encontrados y Diagnóstico de Hallazgos

Durante la ejecución de las pruebas E2E en el flujo del Asesor, se identificaron los siguientes puntos críticos que bloqueaban o degradaban la experiencia del usuario:

### A. Ausencia de Formularios Dinámicos por Campaña y Etapa
* **Comportamiento observado**: Al ingresar al módulo de *Generar Nueva Orden* (`/asesor/orders/new`) y seleccionar ciertas combinaciones de campaña y etapa, el sistema mostraba la alerta *"No hay un formulario configurado para esta campaña y etapa"*, impidiendo capturar los datos de la venta.
* **Causa raíz**: La tabla `sales_service.sales_form_template` no contaba con registros sembrados o configurados para todas las combinaciones de campañas y etapas en la base de datos de pruebas.

### B. Mensajería de Error Genérica en la Interfaz de Usuario
* **Comportamiento observado**: Al fallar el guardado de los datos del formulario, la notificación en pantalla indicaba únicamente un mensaje vago: *"Orden creada, pero falló al guardar los datos del formulario."* o *"Error al guardar la ficha de venta."*
* **Causa raíz**: Los servicios del cliente Blazor (`SalesOrderService.cs`) capturaban un valor booleano simple (`false`) sin extraer ni propagar el cuerpo del mensaje de error HTTP (`{ "message": "..." }`) retornado por la API REST.

### C. Rechazo por Pérdida de Custodia al Crear Nueva Orden
* **Comportamiento observado**: Al enviar una nueva orden, los datos del formulario no se guardaban y la API devolvía un código `403 Forbidden` (`"No tienes la custodia de esta orden para editar sus campos."`).
* **Causa raíz**: La petición de creación enviada a `/api/orders` no incluía los campos `ownerUserId` ni `custodyUserId`, causando que la orden se registrara con valor `NULL` o `0` en la columna de custodia.

### D. Bloqueo de Permisos de Edición en el Guardado Inicial según Etapa
* **Comportamiento observado**: Si el asesor seleccionaba una etapa del pipeline configurada como restringida para edición posterior (por ejemplo *"En BackOffice"*), la API denegaba el guardado de los datos iniciales con el mensaje *"El estado actual del pedido no permite la edición de campos."*
* **Causa raíz**: El método `FormController.cs` aplicaba la verificación de permisos de edición de campos `access_control.can_user_action` tanto para la grabación inicial de una orden nueva como para ediciones posteriores en órdenes existentes.

### E. Falta de Validación Visual en Tiempo Real y Bloqueo Pre-Submit
* **Comportamiento observado**: Si el usuario ingresaba un DNI o un IBAN con formato incorrecto o con letra de control inválida, la interfaz permitía presionar el botón *"Guardar Orden"*, ejecutando llamadas innecesarias al backend antes de advertir el error.

---

## 2. Cambios y Soluciones Implementadas

### A. Seeding Automático y Estrategia Fallback de Plantillas
* **Archivos modificados**: 
  * [FormRepository.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.ApiHub/Infrastructure/Persistence/FormRepository.cs) (`GetTemplatesByCampaignStageAsync`, `SeedDefaultFormsAsync`)
  * [IFormRepository.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.ApiHub/Domain/Repositories/IFormRepository.cs)
  * [FormController.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.ApiHub/Api/Controllers/FormController.cs) (`POST /api/forms/seed`)
* **Detalle del cambio**:
  1. Se creó el mecanismo `SeedDefaultFormsAsync` que genera automáticamente una plantilla estándar de alta de servicio con los 7 campos esenciales de venta si la tabla de plantillas está vacía.
  2. Se implementó una consulta con fallback jerárquico en `GetTemplatesByCampaignStageAsync`:
     - Consulta 1: Coincidencia exacta por `id_cmpg` e `id_stage`.
     - Consulta 2 (Fallback 1): Coincidencia por `id_cmpg`.
     - Consulta 3 (Fallback 2): Cualquier plantilla activa por defecto en el sistema.

### B. Validación en Tiempo Real en la UI y Resaltado Visual
* **Archivos modificados**: 
  * [NewOrder.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/NewOrder.razor)
  * [AsesorOrderDetail.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/Components/Pages/AsesorOrderDetail.razor)
  * [ValidationHelper.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Helpers/ValidationHelper.cs)
* **Detalle del cambio**:
  1. Evaluación inmediata al escribir (`@oninput` / `@onchange`) de reglas de negocio:
     - **DNI / NIE**: Verificación de 8 números + 1 letra de control según el algoritmo oficial de **Módulo 23** de España.
     - **IBAN**: Verificación de estructura `ES` + 22 dígitos procesados bajo la división de **Módulo 97**.
     - **Teléfono**: Verificación de formato telefónico de 9 dígitos de España (iniciados en 6, 7, 8 o 9).
  2. Aplicación de clase CSS `is-invalid border-danger` al input con mensaje explícito en rojo debajo del campo.
  3. Deshabilitado preventivo del botón de envío (`disabled="@(isSaving || fieldValidationErrors.Any())"`) para impedir intentos de guardado con datos inválidos.

### C. Asignación de Custodia y Propietario en la Creación
* **Archivo modificado**: 
  * [NewOrder.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/NewOrder.razor) (`HandleSubmit`)
* **Detalle del cambio**: Al construir el DTO de creación de la orden, se asignan explícitamente `ownerUserId = currentUserId` y `custodyUserId = currentUserId`, garantizando que la orden pertenezca desde su origen al asesor autenticado.

### D. Control de Permisos Diferenciado para Guardado Inicial
* **Archivo modificado**: 
  * [FormController.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.ApiHub/Api/Controllers/FormController.cs) (`SaveData`)
* **Detalle del cambio**:
  Se incorporó la verificación `isInitialSave`:
  ```csharp
  var existingData = await _orderDataRepository.GetByOrderAsync(idOrder);
  bool isInitialSave = (existingData == null || !existingData.Any());

  if (!isInitialSave)
  {
      var statusId = (int)(order.IdStatus ?? 0);
      var hasPermission = await _permissionService.CanUserActionAsync((int)userId, "sales.order.edit.field", statusId);
      if (!hasPermission)
          return StatusCode(403, new { message = "El estado actual del pedido no permite la edición de campos." });
  }
  ```
  De esta forma, la captura inicial de los campos del formulario al crear la orden es siempre permitida, independientemente de la etapa inicial seleccionada.

### E. Propagación de Errores Detallados de la API a las Alertas Snackbar
* **Archivos modificados**: 
  * [ISalesOrderService.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/Services/ISalesOrderService.cs) (`SaveOrderDataWithDetailsAsync`)
  * [SalesOrderService.cs](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/Services/SalesOrderService.cs)
  * [NewOrder.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend.Client/Pages/Asesor/NewOrder.razor)
  * [AsesorOrderDetail.razor](file:///home/hackyou/Documentos/CRM/CRM_API/CRM.WebFrontend/Components/Pages/AsesorOrderDetail.razor)
* **Detalle del cambio**: El cliente Blazor decodifica las respuestas HTTP fallidas y extrae la clave `message` devuelta por la API, mostrándola directamente en el Snackbar de notificación (ej. *"Orden #28 creada, pero falló al guardar los datos del formulario: [Motivo exacto]"*).

---

## 3. Instructivo para Validación y Pruebas E2E (Flujo Asesor)

Para ejecutar y certificar el flujo completo del Asesor, realizar el siguiente procedimiento de pruebas:

### Paso 1: Autenticación de Asesor
1. Navega a la pantalla de Login (`/login`).
2. Ingresa las credenciales del asesor de pruebas:
   * **Usuario**: `test.asesor`
   * **Contraseña**: `password123`
3. Confirma el redireccionamiento exitoso al dashboard del Asesor (`/asesor`).

### Paso 2: Generación de Nueva Orden (`/asesor/orders/new`)
1. Haz clic en **Generar Nueva Orden**.
2. En la sección **1. Selección de Cliente, Campaña y Etapa**:
   - Selecciona un **Cliente (Lead)** de la lista.
   - Selecciona una **Campaña** (ej. *VODAFONE*).
   - Selecciona una **Etapa (Pipeline)** (ej. *En BackOffice* o *En Proceso*).
3. Verifica que la sección **2. Datos de la Orden** cargue dinámicamente todos los campos del formulario.

### Paso 3: Validación de Formato en Tiempo Real (DNI / IBAN)
1. **Prueba de DNI Inválido**: Escribe `12345678A` en el campo *DNI / NIE del Titular*.
   - **Resultado esperado**: El campo se resalta en borde rojo y muestra el texto *"DNI/NIE no es válido (letra de control incorrecta según Módulo 23 de España, ej. 12345678Z)"*. El botón *Guardar Orden* se encuentra deshabilitado.
2. **Prueba de DNI Válido**: Corrige el valor a `12345678Z`. El borde rojo desaparece.
3. **Prueba de IBAN Inválido**: Escribe `ES1234` en el campo *Cuenta Bancaria (IBAN)*.
   - **Resultado esperado**: El campo se resalta en rojo con el aviso de Módulo 97.
4. **Prueba de IBAN Válido**: Ingresa `ES9121000418450200051332`.
5. Completa los campos adicionales obligatorios (NombreCompleto, Teléfono, Tipo de Contrato).

### Paso 4: Envío de Orden y Confirmación
1. Haz clic en **Guardar Orden**.
2. **Resultado esperado**:
   - Se muestra la notificación verde: *"Orden #X creada y formulario guardado exitosamente."*
   - Redirecciona automáticamente a la bandeja de órdenes del asesor (`/asesor/orders`).

### Paso 5: Verificación de Detalle y Custodia (`/asesor/orders/{id}`)
1. Ingresa a la orden recién creada desde el listado.
2. Verifica que todos los datos capturados en el formulario dinámico se visualicen correctamente en la pestaña de datos.
3. Confirma que la orden mantenga la custodia del asesor actual y permita adjuntar documentos o solicitar transferencias según la lógica del flujo de venta.
