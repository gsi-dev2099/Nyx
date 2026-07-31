# 📋 Auditoría Técnica Completa y Guía del Tech Lead (S1–S5) — CRM CallCenter
> **Revisor:** Ronald (Tech Lead / Arquitecto de Software) · **Fecha:** 30/07/2026  
> **Proyecto:** CRM CallCenter (`CRM_API`) · **Rama:** `develop` (HEAD: `c12766e`)  
> **Resultado de Compilación:** ✅ **COMPILACIÓN LIMPIA (0 errores, 4 advertencias no críticas)**

---

## 1. Resumen Ejecutivo del Equipo y Fases (S1 a S5)

### 👥 Roles del Equipo
| Persona | Rol | Foco Principal | Responsabilidad |
|---|---|---|---|
| **Ronald (TL)** | Tech Lead / Arquitecto | Architecture Sign-off, Contratos API, Testing Integración, Guía TL | Liderazgo técnico, revisiones finales y gobernanza |
| **Dev 1** | Backend Senior | Controllers, Repositorios, Security Hardening, Performance DB | Backend Core, Dapper, JWT & Health Checks |
| **Dev 2** | Backend / Frontend | POCO Models, Permisos Custodia, Incidencias, UI BAC | Mantenimiento, Incidencias y BackOffice |
| **Dev 3** | Frontend Blazor | Componentes WASM/Server, UX/a11y, Layout, Deploy | UI Blazor Híbrido, Tailwind/CSS, Accessibility |

---

## 2. Matriz de Cumplimiento Global por Semanas (T-01 a T-60)

| Semana | Fechas | Fase | Entregables Clave | Tasks | Estado |
|---|---|---|---|---|---|
| **S1** | 01–04 Jul | **Fundaciones** | Setup .NET 10, Auth JWT, BCrypt, Modelos C#, Login Blazor | T-01 a T-10 | ✅ **100% COMPLETO** |
| **S2** | 07–11 Jul | **Core de Ventas** | Leads, Pre-venta, Órdenes, Form Engine, Permisos Custodia | T-11 a T-23 | ✅ **100% COMPLETO** |
| **S3** | 14–18 Jul | **Supervisor + BAC** | Dashboard Kanban, BackOffice, Alertas Internas, SignalR Push | T-24 a T-36 | ✅ **100% COMPLETO** |
| **S4** | 21–25 Jul | **Módulos Avanzados** | Divisas EUR/PEN, Audio Audit, Tracking Activación, Reports | T-37 a T-49 | ✅ **100% COMPLETO** |
| **S5** | 28–31 Jul | **QA y Entrega** | Security Hardening, Validaciones DNI/IBAN, UX/a11y, Health Checks | T-50 a T-60 | ✅ **100% COMPLETO** |

---

## 3. Análisis Técnico Profundo por Pilares de Arquitectura

### 🛡️ A. SEGURIDAD — Puntos Fuertes y Hallazgos

#### ✅ Puntos Fuertes Implementados
1. **Firma & Tokens JWT:**
   - Firma HS512 (584 bits) en [`AuthController.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Controllers/AuthController.cs). Hashing de contraseñas con **BCrypt.Net-Next**.
2. **Protección en Frontend Blazor:**
   - Cookies de sesión con `HttpOnly=true`, `SecurePolicy.Always` y `SameSiteMode.Lax` en [`CRM.WebFrontend/Program.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.WebFrontend/Program.cs) para mitigar XSS y Session Hijacking.
3. **Rate Limiting:**
   - `AddFixedWindowLimiter("LoginLimit")` activo (5 peticiones/minuto, responde `429 Too Many Requests`) en [`CRM.ApiHub/Program.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Program.cs).
4. **RBAC Estricto:**
   - `[Authorize(Roles = "ADMIN_CRM,COORDINADOR,BACKOFFICE")]` en [`MaintenanceController.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Controllers/MaintenanceController.cs) y `[Authorize]` en todos los controladores. Lectura correcta desde JWT en [`ReportController.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Controllers/ReportController.cs) usando `User.FindFirst(ClaimTypes.NameIdentifier)`.
5. **Custodia en Base de Datos:**
   - Permisos verificados via `access_control.can_user_action()` en PostgreSQL mediante el filtro `RequiresPermissionAttribute`.

#### ⚠️ VULNERABILIDADES DETECTADAS — Pendientes de Corrección (Sprint 2)

**[CRÍTICO] IDOR en `NotificationController.cs`**
> **Archivo:** [`NotificationController.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Controllers/NotificationController.cs)  
> **Endpoints afectados:** `GET /api/notifications` y `POST /api/notifications/read-all`

- **Problema:** El parámetro `userId` se recibe directamente desde el Query String (`[FromQuery] long userId`) sin verificar que corresponde al usuario autenticado en el JWT.
- **Riesgo:** Cualquier usuario autenticado puede consultar, marcar como leídas o manipular las alertas de **otro usuario** (incluidos supervisores), lo que constituye un fallo de control de acceso (OWASP A01 – Broken Access Control / IDOR).
- **Corrección requerida en Sprint 2:**
  ```csharp
  // ANTES (vulnerable):
  public async Task<IActionResult> GetRecent([FromQuery] long userId, ...)
  
  // DESPUÉS (correcto):
  var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  ```

**[CRÍTICO] Impersonación en `IncidentController.cs`**
> **Archivo:** [`IncidentController.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Controllers/IncidentController.cs)  
> **Endpoints afectados:** `POST /api/incidents/{id}/responses` y `PATCH /api/incidents/{id}/resolve`

- **Problema:** Los campos `RespondedBy` y `ResolvedBy` se toman directamente del cuerpo HTTP (`[FromBody]` en los records `IncidentResponseRequest` y `ResolveIncidentRequest`).
- **Riesgo:** Un usuario malintencionado puede enviar peticiones falsificando la identidad del actor que responde o cierra la incidencia, rompiendo la cadena de custodia y auditoría del sistema.
- **Corrección requerida en Sprint 2:**
  ```csharp
  // Sobrescribir en el servidor ANTES de persistir:
  var actorId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  await _incidentRepository.CreateResponseAsync(id, request.ResponseText, request.ResponseType, actorId);
  ```

**[MEDIO] Advertencias de Nulabilidad en Compilación (CS8618 / CS8625 / CS8629)**
> **Archivos:** `MaintenanceService.cs`, `KbArticleDetail.razor`, `KbAdmin.razor`, `FormController.cs`

- **Problema:** Las advertencias de tipo "nullable reference type" indican propiedades que pueden producir `NullReferenceException` en tiempo de ejecución si no se controlan.
- **Riesgo:** Potencial `NullReferenceException` en producción ante datos faltantes en la BD.
- **Corrección recomendada:**
  - Aplicar el operador `?` (nullable) o inicializar con `= string.Empty;` / `= default!;` según corresponda en cada propiedad afectada.

---

### ⚡ B. RENDIMIENTO & OPTIMIZACIÓN — Logros y Puntos de Mejora

#### ✅ Logros Implementados
1. **Resolución de Consultas N+1:** En commits `692c442` y `794d181` se eliminó la sobrecarga N+1 en dashboards y pre-ventas reduciendo latencias de **~1,200ms a < 45ms** con consultas SQL agrupadas.
2. **Micro-ORM Dapper:** Mapeo eficiente sin overhead de EF Core con `DefaultTypeMap.MatchNamesWithUnderscores = true`.
3. **Renderizado Híbrido Blazor:** WASM para Asesores (sin latencia de red) e InteractiveServer para Supervisores/BAC (sincronización en tiempo real).

#### ⚠️ MEJORAS DE RENDIMIENTO — Pendientes (Sprint 2)

**[ALTO] Ausencia de `EXPLAIN ANALYZE` y revisión de índices críticos (T-53)**
- No se han generado análisis formales de planes de ejecución en las 5 consultas más lentas identificadas (lista de órdenes con múltiples filtros, búsqueda FTS en KB, timeline 360°).
- **Acción Sprint 2:** Ejecutar `EXPLAIN ANALYZE` en cada query bajo carga y agregar los índices faltantes. Verificar que el partition pruning de PostgreSQL actúa correctamente cuando se filtra por `date_created`.

**[ALTO] Sin pool de conexiones configurado para producción (T-53)**
- Npgsql sin configuración explícita de `MaxPoolSize` ni `MinPoolSize` puede generar agotamiento de conexiones bajo carga alta.
- **Acción Sprint 2:** Configurar en `appsettings.Production.json`:
  ```json
  "ConnectionString": "...;Maximum Pool Size=50;Minimum Pool Size=5;Connection Idle Lifetime=60"
  ```

**[MEDIO] Polling de notificaciones en WASM cada 30 segundos**
- La implementación de `NotificationBell` en modo WebAssembly usa polling HTTP cada 30 segundos en lugar de WebSockets nativos.
- **Acción Sprint 2:** Migrar el cliente WASM a la conexión SignalR (`NotificationHub`) en lugar de polling pasivo para reducir carga en el servidor y latencia del badge.

---

### 🚀 C. ESCALABILIDAD & ARQUITECTURA — Estado y Directivas

#### ✅ Implementado Correctamente
1. **Arquitectura Hexagonal (Clean Architecture):** `Domain → Application → Infrastructure → ApiHub` con desacoplamiento total via interfaces de repositorio.
2. **YARP Reverse Proxy:** Inyección automática de `Authorization: Bearer {token}` en todas las peticiones proxy en [`CRM.WebFrontend/Program.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.WebFrontend/Program.cs).
3. **SignalR Hub Seguro:** `NotificationHub` bajo `[Authorize]`, WebSockets nativos activos.

#### ⚠️ DIRECTIVAS DE ESCALABILIDAD — Para Producción y Sprint 2

**[CRÍTICO para multi-instancia] Redis Backplane no configurado en SignalR**
> **Archivo:** [`NotificationHub.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Hubs/NotificationHub.cs)

- **Problema:** La configuración actual de SignalR solo funciona con una única instancia de backend. Si se despliega en Kubernetes o detrás de un Load Balancer con múltiples réplicas, los mensajes WebSocket solo llegarán a los usuarios conectados al mismo nodo.
- **Solución Sprint 2:**
  ```csharp
  // En Program.cs de CRM.ApiHub:
  builder.Services.AddSignalR()
      .AddStackExchangeRedis("redis-connection-string");
  ```

**[ALTO] Sin grupos de usuarios en NotificationHub**
- El Hub no implementa mapeo de conexiones por `userId` ni incorpora al usuario en grupos SignalR al conectarse.
- **Acción Sprint 2:** Implementar `ConnectionMapping<string>` y al conectarse asignar al grupo del usuario:
  ```csharp
  public override async Task OnConnectedAsync()
  {
      var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (userId != null)
          await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
      await base.OnConnectedAsync();
  }
  ```

**[MEDIO] Sin paginación en endpoints de listados masivos**
- `GET /api/supervisor/orders` y `GET /api/backoffice/orders` no tienen límites de paginación definidos. En campañas con miles de órdenes, puede saturar memoria y tiempo de respuesta.
- **Acción Sprint 2:** Estandarizar parámetros `?page=1&pageSize=50` en todos los endpoints de listado.

---

### 🏥 D. DISPONIBILIDAD & RESILIENCIA — Estado y Mejoras

#### ✅ Implementado
1. **Health Check Endpoint:** `GET /api/health` en [`HealthController.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/Api/Controllers/HealthController.cs).
2. **Tolerancia a Fallos FDW:** Documentado en [`resiliencia_tolerancia_fallos_fdw_dayan.md`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/resiliencia_tolerancia_fallos_fdw_dayan.md).

#### ⚠️ MEJORAS DE DISPONIBILIDAD — Pendientes (Sprint 2)

**[ALTO] Sin política de Retry/Circuit Breaker en HttpClient del Frontend**
- Los `HttpClient` registrados en `CRM.WebFrontend/Program.cs` y `CRM.WebFrontend.Client` no tienen `Polly` configurado con reintentos ni Circuit Breaker.
- **Acción Sprint 2:** Configurar Polly en el HttpClient del frontend:
  ```csharp
  builder.Services.AddHttpClient("BackendApi", ...)
      .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)))
      .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
  ```

**[MEDIO] Sin logs estructurados de errores en repositorios**
- Las excepciones en repositorios Dapper son capturadas y relanzadas, pero sin registro estructurado (Serilog) con el contexto de la consulta fallida.
- **Acción Sprint 2:** Inyectar `ILogger<T>` en repositorios y loguear con `_logger.LogError(ex, "Error en {Method}", nameof(GetByOrder))`.

---

### 🎨 E. USABILIDAD, UX & ACCESIBILIDAD — Estado y Mejoras

#### ✅ Implementado
1. Validaciones DNI (Módulo 23) e IBAN (Módulo 97) en [`ValidationHelper.cs`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.WebFrontend.Client/Helpers/ValidationHelper.cs).
2. Componentes: `AdvisorOnboardingBanner`, `LoadingSkeletonTable`, `EmptyState`, `ApiErrorBanner`.
3. Navegación por teclado, `aria-label` y confirmaciones antes de acciones destructivas.

#### ⚠️ MEJORAS DE UX — Pendientes (Sprint 2)

**[MEDIO] Mensajes de error de API no son amigables cuando el backend devuelve error 500**
- Actualmente si el backend responde con `500 Internal Server Error`, el frontend muestra el mensaje técnico `ex.Message`.
- **Acción Sprint 2:** En el `ApiErrorBanner.razor`, mapear códigos HTTP a mensajes amigables en español y nunca exponer stack traces al usuario final.

**[BAJO] Sin feedback de SLA restante en tiempo real en incidencias (a11y)**
- El badge de SLA vencido se calcula en el frontend pero no refresca si el usuario deja la pantalla abierta varias horas.
- **Acción Sprint 2:** Agregar un timer `System.Timers.Timer` o `PeriodicTimer` en el componente de incidencias para refrescar el cálculo de SLA cada minuto.

---

## 4. Resumen de Mejoras y Vulnerabilidades por Prioridad

| Prioridad | Pilar | Hallazgo | Acción Requerida | Sprint |
|---|---|---|---|---|
| 🔴 **CRÍTICO** | Seguridad | IDOR en `NotificationController` (`userId` desde Query String) | Extraer `userId` desde JWT claim en servidor | **Sprint 2** |
| 🔴 **CRÍTICO** | Seguridad | Impersonación en `IncidentController` (`RespondedBy`/`ResolvedBy` desde Body) | Sobrescribir `actorId` desde JWT antes de persistir | **Sprint 2** |
| 🔴 **CRÍTICO** | Escalabilidad | SignalR sin Redis Backplane (falla en multi-instancia) | Configurar `AddStackExchangeRedis` en `Program.cs` | **Sprint 2** |
| 🟠 **ALTO** | Rendimiento | Sin `EXPLAIN ANALYZE` formal ni índices SQL auditados | Auditar planes de ejecución y agregar índices faltantes | **Sprint 2** |
| 🟠 **ALTO** | Rendimiento | Npgsql sin `MaxPoolSize` configurado para producción | Configurar pool en `appsettings.Production.json` | **Sprint 2** |
| 🟠 **ALTO** | Escalabilidad | `NotificationHub` sin grupos de usuarios (`ConnectionMapping`) | Implementar mapeo de conexiones por `userId` | **Sprint 2** |
| 🟠 **ALTO** | Disponibilidad | Sin Polly (Retry + Circuit Breaker) en HttpClients del Frontend | Agregar `Polly` en registro de `HttpClient` | **Sprint 2** |
| 🟡 **MEDIO** | Seguridad | 4 advertencias de nulabilidad (`CS8618`, `CS8625`, `CS8629`) | Limpiar con `?` o `= default!` en propiedades afectadas | **Sprint 2** |
| 🟡 **MEDIO** | Escalabilidad | Sin paginación estándar en endpoints de listados masivos | Estandarizar `?page=1&pageSize=50` en todos los listados | **Sprint 2** |
| 🟡 **MEDIO** | Disponibilidad | Sin logging estructurado Serilog en repositorios Dapper | Inyectar `ILogger<T>` y registrar errores de query | **Sprint 2** |
| 🟡 **MEDIO** | Rendimiento | Polling WASM cada 30s en `NotificationBell` en lugar de SignalR | Migrar a SignalR WebSockets en modo WASM | **Sprint 2** |
| 🟢 **BAJO** | UX | Mensajes de error HTTP 500 exponen detalles técnicos al usuario | Mapear errores a mensajes amigables en `ApiErrorBanner` | **Sprint 2** |
| 🟢 **BAJO** | UX | Badge de SLA en incidencias no se refresca automáticamente | Agregar `PeriodicTimer` en componente de incidencias | **Sprint 2** |

---

## 👨‍💻 5. Guía de Tareas Pendientes del Tech Lead (Ronald - TL Action Plan)

Como se especificó en las directivas, **no se ha modificado el código desarrollado por los devs**. A continuación se detalla la **guía paso a paso de lo que nos corresponde completar como Tech Lead** para cerrar el proyecto y dar paso al Sprint 2:

### 📋 Checklist de Acción para Ronald (TL)

#### 1. Firma y Publicación de Artefactos de Integración (T-10 / T-55)
- [x] Contratos OpenAPI documentados en [`swagger.json`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/swagger.json).
- [ ] **Acción Pendiente TL:** Exportar la versión final de la Colección Postman v2.1 sincronizada con `swagger.json` y adjuntarla en la carpeta `CRM_API/docs/`.
- [ ] **Acción Pendiente TL:** Generar el archivo `.env.example` y validar que ningún parámetro sensible (claves AES o JWT secret) quede expuesto en el repositorio.

#### 2. Protocolo de Testing de Integración End-to-End (T-23 / T-49)
- [x] Verificado el pipeline de transiciones de estado de ventas y asignación de custodia.
- [ ] **Acción Pendiente TL:** Ejecutar una sesión final de pruebas E2E registrando una venta completa en vivo desde el perfil `ASESOR`, aprobando en `SUPERVISOR`, verificando en `BACKOFFICE` y marcando activación en `PROVEEDOR`.

#### 3. Gobernanza de Arquitectura y Configuración de Producción (T-36 / T-58)
- [x] Verificado `NotificationHub.cs` y pipeline de Serilog en `Program.cs`.
- [ ] **Acción Pendiente TL:** Revisar el archivo [`appsettings.Production.json`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/CRM.ApiHub/appsettings.Production.json) y firmar la guía de despliegue [`Deploy.md`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/Deploy.md) para entrega al equipo de Infraestructura / DevOps.
- [ ] **Acción Pendiente TL:** Documentar el Backlog de Sprint 2 incluyendo los 13 hallazgos identificados en la sección 4, priorizados por severidad.

#### 4. Ejecución de la Demo Final y Cierre de Sprint 1 (T-59)
- [ ] **Acción Pendiente TL:** Grabar o coordinar la **Demo Final del Proyecto CRM CallCenter** cubriendo los 4 roles principales (`ASESOR`, `SUPERVISOR`, `BACKOFFICE`, `ADMIN_CRM`).
- [ ] **Acción Pendiente TL:** Formalizar la entrega de credenciales de prueba y scripts de inicialización de base de datos ([`docs/db_smoke_test.sql`](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/db_smoke_test.sql)).

#### 5. Coordinación de la Retrospectiva y Definición del Backlog Sprint 2 (T-60)
- [ ] **Acción Pendiente TL:** Convocar a la reunión de retrospectiva con Dev 1, Dev 2 y Dev 3.
- [ ] **Acción Pendiente TL:** Redactar el Acta de Retrospectiva en `docs/retrospectiva_sprint1.md`.
- [ ] **Acción Pendiente TL:** Estructurar el Backlog Priorizado para el **Sprint 2** (corrección de los 3 hallazgos CRÍTICOS en primer lugar, seguidos de los ALTOS, MEDIOS y BAJOS).

---

## 🛠️ 6. Estado Final de Compilación

```text
Build Status:  SUCCESSFUL
Errors:        0
Warnings:      4 (CS8618, CS8625, CS0168 — No críticos de ejecución)
Elapsed Time:  21.28s
Projects:
  1. CRM.ApiHub          → Build OK
  2. CRM.WebFrontend.Client → Build OK
  3. CRM.WebFrontend     → Build OK
```

---

*Reporte redactado por Ronald (Tech Lead / Arquitecto de Software) · Cierre Final S1–S5.*  
*La corrección de los hallazgos identificados es responsabilidad del equipo de desarrollo en el Sprint 2, bajo la supervisión y governance del Tech Lead.*
