# Hardening de Seguridad de la API

Este documento detalla los problemas de seguridad detectados y las soluciones implementadas en el módulo de la API (`CRM.ApiHub`) para robustecer la autenticación, autorización y control de acceso.

---

## 1. Rate Limiting en Endpoints de Login

### Problema
El endpoint de inicio de sesión (`/api/auth/login`) carecía de límites en las solicitudes entrantes, lo que exponía al sistema a ataques de fuerza bruta y denegación de servicio (DoS) en el punto de entrada crítico de autenticación.

### Solución
Se implementó un middleware de limitación de tasa (Rate Limiting) nativo de ASP.NET Core:
1. **Configuración en [Program.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Program.cs):**
   Se registró una política llamada `"LoginLimit"` de ventana fija (*Fixed Window*):
   * **Límite de peticiones:** Máximo 5 solicitudes.
   * **Ventana de tiempo:** 1 minuto.
   * **Respuesta en caso de exceso:** Código de estado HTTP `429 Too Many Requests`.
2. **Aplicación en [AuthController.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Api/Controllers/AuthController.cs):**
   Se decoró la acción `Login` con la anotación `[EnableRateLimiting("LoginLimit")]`.

---

## 2. Auditoría de Endpoints sin [Authorize]

### Problema
Durante la auditoría de controladores se identificó un endpoint de depuración/mantenimiento público que omitía la seguridad de la aplicación:
* `GET /api/maintenance/testschema` en [MaintenanceController.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Api/Controllers/MaintenanceController.cs) estaba decorado con `[AllowAnonymous]`, permitiendo el acceso a información de la estructura de base de datos sin credenciales.

### Solución
Se eliminó la anotación `[AllowAnonymous]` de la acción `TestSchema` en `MaintenanceController.cs`. Al quitar esta directiva, el endpoint hereda automáticamente la seguridad a nivel de controlador `[Authorize(Roles = "ADMIN_CRM,COORDINADOR,BACKOFFICE")]`, garantizando que únicamente usuarios autorizados y con los roles apropiados tengan acceso.

---

## 3. Verificación de Permisos en Operaciones Sensibles (`PermissionService`)

### Problema
Diversas operaciones críticas de negocio no validaban de forma activa si el usuario poseía los permisos requeridos correspondientes al estado de la orden en el momento exacto de ejecutar la acción.

### Solución
Se integraron llamadas directas a la función de base de datos `access_control.can_user_action` en las capas de persistencia y servicios para las siguientes operaciones críticas:
1. **Transferencia Masiva de Órdenes (Supervisor)**:
   * **Archivo:** [SupervisorRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/SupervisorRepository.cs) (`BulkTransferToBackofficeAsync`)
   * **Cambio:** Se consulta el estado actual de cada orden con bloqueo `FOR UPDATE` y se comprueba si el supervisor tiene el permiso `"sales.order.bulk_transfer"` para ese estado específico. Si la validación falla, esa orden se marca como fallida en el reporte de la transacción.
2. **Cambio de Estado de Órdenes (Backoffice)**:
   * **Archivo:** [BackofficeRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/BackofficeRepository.cs) (`UpdateOrderStatusAsync`)
   * **Cambio:** Antes de guardar la actualización en base de datos, se verifica que el analista de Backoffice cuente con el permiso `"sales.order.edit.backoffice"` para el estado del cual proviene la orden. De no ser así, se revierte la transacción.

---

## 4. Configuración de CORS Restringido

### Problema
El sistema requería restringir los accesos por CORS para permitir solicitudes únicamente desde los orígenes del frontend del proyecto (previniendo accesos cruzados no autorizados en navegadores).

### Solución
1. **Configuración en [appsettings.json](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/appsettings.json):**
   Se agregó una sección `CorsSettings:AllowedOrigins` para almacenar los orígenes válidos (por ejemplo, las direcciones locales del cliente Blazor `http://localhost:5261` y `https://localhost:7285`).
2. **Registro de políticas en [Program.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Program.cs):**
   Se definió la política `"FrontendCorsPolicy"` mapeando los orígenes permitidos por archivo de configuración con credenciales y cualquier tipo de cabecera y método HTTP. Se habilitó el middleware `app.UseCors("FrontendCorsPolicy")` en el pipeline de la aplicación.

---

## 5. Prevención de Inyección SQL por Concatenación

### Problema
Verificar que ninguna consulta a base de datos utilizara concatenación manual de texto para construir las sentencias SQL, evitando vulnerabilidades de Inyección SQL.

### Solución
Se auditó todo el código de persistencia basado en Dapper (`SalesOrderRepository`, `UserRepository`, `SupervisorRepository`, `BackofficeRepository`, `ReportRepository`, `KnowledgeBaseRepository`). Se constató que todas las consultas usan parámetros tipados nativos de Dapper (ej. `@UserId`, `@OrderId`, `@DateFrom`), lo cual delega la parametrización correcta de datos al motor de base de datos e invalida cualquier intento de inyección de código malicioso.
