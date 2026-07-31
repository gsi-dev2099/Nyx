# 🛠️ Reporte de Problemas Resueltos - Sprint 2

Este documento detalla los problemas de seguridad, rendimiento, arquitectura y tolerancia a fallos identificados en la plataforma **Nyx CRM**, junto con las respectivas soluciones e implementaciones técnicas aplicadas durante el Sprint 2.

---

## 🔒 1. Seguridad y Control de Acceso (JWT)

### A. Endpoint de Notificaciones (`NotificationController`)
* **Problema**: El parámetro `userId` se aceptaba directamente desde el Query String en `GET /api/notifications` y `POST /api/notifications/read-all` sin verificar la pertenencia al token JWT. Cualquier usuario autenticado podía leer o marcar como leídas las notificaciones de terceros simplemente alterando el valor numérico en la consulta.
* **Solución**:
  - Se eliminó el parámetro `[FromQuery] long userId` de las firmas de los endpoints.
  - Se implementó la extracción del `userId` en el Backend de forma segura a través de los Claims de identidad de la solicitud autorizada:
    ```csharp
    var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    ```

### B. Creación y Resolución de Incidencias (`IncidentController`)
* **Problema**: Los campos de autoría y resolución (`RespondedBy` y `ResolvedBy`) eran recibidos directamente en el cuerpo JSON de la petición HTTP (`POST /{id}/responses` y `PATCH /{id}/resolve`) sin validación. Un atacante o usuario malintencionado podía suplantar la identidad de la persona que respondía o cerraba el caso.
* **Solución**:
  - Se ignoran los valores provistos por el cliente en el JSON de entrada.
  - El ID del usuario que ejecuta la acción es recuperado de manera íntegra desde los Claims de identidad del JWT en el backend y asignado directamente a las propiedades del objeto de persistencia antes de ser insertado en la base de datos.

### C. Campos Personalizados de Usuario (`AuthController`)
* **Problema**: El endpoint `POST /api/auth/register-custom-fields` carecía de restricciones de rol, lo que posibilitaba que cualquier usuario registrado (como un Asesor) modificara o agregara campos estructurados en la entidad de usuarios.
* **Solución**:
  - Se incorporó el atributo de autorización explícita restringiendo el acceso exclusivamente a los roles administrativos autorizados:
    ```csharp
    [Authorize(Roles = "COORDINADOR,ADMIN_CRM")]
    ```

---

## ⚡ 2. Optimización de Base de Datos y Conexiones

### A. Análisis de Rendimiento de Queries Lentas
* **Problema**: No se disponía de planes de ejecución ni métricas detalladas para las 5 consultas más lentas del sistema (listado multifiltro de órdenes, búsqueda FTS en base de conocimientos y el timeline 360°). Tampoco se tenía garantía de que el *partition pruning* por fecha estuviera actuando efectivamente.
* **Solución**:
  - Se corrieron planes de ejecución detallados mediante `EXPLAIN (ANALYZE, BUFFERS)` en PostgreSQL bajo cargas de estrés simuladas.
  - Se optimizaron las consultas de filtrado añadiendo índices compuestos e índices específicos de búsqueda textual (FTS) sobre las columnas más consultadas.
  - Se verificó y aseguró que las consultas estructuradas por fecha forzaran al motor a ignorar particiones irrelevantes (*partition pruning* activo).

### B. Pool de Conexiones de Base de Datos (Npgsql)
* **Problema**: Al no contar con una configuración explícita para el pool de conexiones en el string de conexión de `Npgsql`, la aplicación corría riesgo de agotar rápidamente los sockets disponibles en producción bajo ráfagas intensas de tráfico.
* **Solución**:
  - Se configuró explícitamente el pool de conexiones dentro de `ConnectionString` en el archivo de configuración para producción:
    ```json
    "ConnectionString": "...;Maximum Pool Size=50;Minimum Pool Size=5;Connection Idle Lifetime=60"
    ```

---

## 📡 3. Escalabilidad de SignalR (WebSockets) y Polling

### A. Polling Pasivo en Barra de Notificaciones (`NotificationBell`)
* **Problema**: El componente `NotificationBell` (Blazor WASM) realizaba polling HTTP pasivo de forma de manera reiterada al servidor API para verificar nuevas notificaciones, lo que saturaba el servidor web con solicitudes HTTP redundantes.
* **Solución**:
  - Se migró el cliente Blazor WASM para consumir en tiempo real el WebSocket del servidor, reutilizando el `NotificationHub` existente mediante el cliente SignalR.

### B. Múltiples Réplicas de Backend (SignalR Scaleout)
* **Problema**: Al desplegar múltiples réplicas del backend en producción, las conexiones WebSockets se distribuían entre distintos nodos. Como resultado, las notificaciones emitidas por un nodo no llegaban a los usuarios con sockets conectados a otras réplicas.
* **Solución**:
  - Se implementó un backplane de SignalR apoyado en Redis para sincronizar todos los nodos de backend. Se registró el servicio en `Program.cs` del API:
    ```csharp
    builder.Services.AddSignalR()
        .AddStackExchangeRedis(redisConnectionString, options => {
            options.Configuration.ChannelPrefix = "NyxCRM";
        });
    ```

### C. Mapeo de Conexiones por Usuario
* **Problema**: El `NotificationHub` no agrupaba a los usuarios en salas/grupos privados ni mantenía un mapeo de conexiones (`ConnectionMapping<string>`), dificultando el envío selectivo y directo de alertas a usuarios específicos.
* **Solución**:
  - Se sobreescribieron los métodos de ciclo de vida de conexión en `NotificationHub.cs` para asociar dinámicamente cada identificador de socket con su respectivo grupo de usuario:
    ```csharp
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }
    ```

---

## 🛡️ 4. Resiliencia HTTP y Observabilidad (Logging)

### A. Políticas de Resiliencia en Frontends (Polly)
* **Problema**: Las conexiones HTTP de `CRM.WebFrontend` y `CRM.WebFrontend.Client` hacia el API backend carecían de tolerancia a fallos transitorios de red o microcaídas.
* **Solución**:
  - Se instaló la dependencia `Microsoft.Extensions.Http.Polly` en ambos proyectos de interfaz.
  - Se configuró la política en la inyección del cliente `"BackendApi"` aplicando reintentos exponenciales y un mecanismo de cortocircuito (*Circuit Breaker*):
    ```csharp
    builder.Services.AddHttpClient("BackendApi", ...)
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)))
        .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
    ```

### B. Logging Contextual de Excepciones (Serilog)
* **Problema**: Los bloques `try-catch` en los repositorios de datos atrapaban errores pero no capturaban el contexto de ejecución (consulta, tabla afectada, parámetros ni usuario solicitante), lo que obstaculizaba el análisis de fallas en entornos productivos.
* **Solución**:
  - Se inyectó `ILogger<T>` en los constructores de repositorios clave (`SupervisorRepository`, `BackofficeRepository` y `SalesOrderRepository`).
  - Se implementaron capturas estructuradas en los métodos principales (ej: consultas paginadas, transferencias masivas), escribiendo logs informativos contextuales a Serilog antes de propagar el error hacia la capa superior:
    ```csharp
    try
    {
        // Operación a base de datos
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en {Method} — userId={UserId}, page={Page}", nameof(GetTeamOrdersAsync), supervisorId, page);
        throw;
    }
    ```
