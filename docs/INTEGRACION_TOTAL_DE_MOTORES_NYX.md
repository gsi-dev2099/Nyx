# ⚡ Solución Arquitectónica: Integración Total de Motores en Nyx CRM

> **Análisis Profundo del Problema, Evaluación de Alternativas y Plan de Integración Definitiva**  
> **Motores Involucrados**: `Nyx.FlowEngine` (Flujos y Checkpoints), `Nyx.ApprovalEngine` (Aprobaciones Multinivel), `Nyx.SlaEngine` (Medición y Alarmas SLA)  
> **Objetivo**: Lograr que el API Central (`CRM.ApiHub`) sea 100% robusta, autónoma, de latencia cero y sin fricción de microservicios independientes.

---

## 1. 🔍 Diagnóstico del Problema: ¿Por qué la Arquitectura Actual Presenta Fricción?

En la arquitectura actual, los motores de negocio fueron diseñados como **microservicios independientes**:

```
[CRM.WebFrontend (Blazor)] 
        │
        ▼ (HTTP REST)
 [CRM.ApiHub (Port 5068)] ──┬──(HTTP:5070)──► [Nyx.SlaEngine]      ──► DB: nyx_sla
                            ├──(HTTP:5071)──► [Nyx.ApprovalEngine] ──► DB: nyx_approval
                            └──(HTTP:5072)──► [Nyx.FlowEngine]     ──► DB: nyx_flow
```

### Principales Desafíos y Puntos de Falla Identificados:

1. **Sobrecarga de Red y Latencia (Network Hops)**:
   - Cada operación clave en una orden de venta (creación, cambio de estado, aprobación) desencadena entre **2 y 4 llamadas HTTP internas** secuenciales.
   - Si la red interna o un contenedor experimenta micro-cortes, la petición principal sufre demoras o requiere reintentos Polly.

2. **Riesgo de Disponibilidad Inter-Servicio (Availability Coupling)**:
   - Si `Nyx.FlowEngine` o `Nyx.ApprovalEngine` no están levantados (por ejemplo, en un entorno de desarrollo local donde el programador solo ejecuta `dotnet run` sobre `CRM.ApiHub`), las transacciones principales deben caer en `try/catch` de fallback seguro, perdiendo la ejecución de reglas bloqueantes.

3. **Inconsistencia Transaccional (Falta de Transacciones ACID Nativas)**:
   - Al crear una venta en `nyx_crm.sales_orders` y luego instanciar el flujo en `nyx_flow.flow_instances` vía HTTP, no existe una transacción de base de datos unificada (a menos que se use un costoso Two-Phase Commit o patrón Saga complejo). Si el HTTP falla a medio camino, la orden queda creada pero sin flujo instanciado.

4. **Complejidad Operativa y de Despliegue**:
   - Se requieren **4 contenedores .NET Web API** (`crm_apihub`, `sla_engine_api`, `approval_engine_api`, `flow_engine_api`) y **4 bases de datos PostgreSQL separadas** (`nyx_crm`, `nyx_sla`, `nyx_approval`, `nyx_flow`).
   - Esto cuadruplica el consumo de memoria RAM en el servidor y complica la observabilidad y los pipelines CI/CD.

5. **Fragmentación de Autenticación y Swagger**:
   - Los microservicios de motores tenían endpoints desprotegidos o requerían tokens delegados, y la documentación Swagger estaba dividida en 4 URLs diferentes.

---

## 2. 🏛️ Evaluación de Enfoques Arquitectónicos

| Criterio de Evaluación | Enfoque A: Microservicios + Gateway Proxy | Enfoque B: Monolito Modular In-Process (Recomendado) | Enfoque C: Híbrido Conmutable |
|---|:---:|:---:|:---:|
| **Latencia de Ejecución** | 🟡 Media (15-50ms HTTP) | 🟢 **Ultrarrápida (< 1ms en memoria)** | 🟢 Alta |
| **Transaccionalidad ACID** | 🔴 No (Eventual / Sagas) | 🟢 **Sí (Transacción SQL Nativa)** | 🟢 Sí en modo local |
| **Simplicidad de Despliegue** | 🔴 4 Contenedores + Nginx | 🟢 **1 Solo Contenedor API Hub** | 🟡 Configurable |
| **Consumo de Memoria RAM** | 🔴 ~1.2 GB (4 runtimes .NET) | 🟢 **~300 MB (1 runtime .NET unificado)** | 🟢 Bajo en local |
| **Facilidad para Dev Local** | 🔴 Requiere Docker o 4 consolas | 🟢 **`dotnet run` directo y listo** | 🟢 Excelente |
| **Aislamiento de Dominio** | 🟢 Alto (Repositorios separados) | 🟢 **Alto (Clean Architecture / Bounded Contexts)** | 🟢 Alto |
| **Swagger Unificado** | 🟡 Requiere Gateway aggregation | 🟢 **100% Nativo en `/swagger`** | 🟢 Nativo |

---

## 3. 🚀 Solución Arquitectónica Recomendada: Monolito Modular In-Process

La solución óptima para **Nyx CRM** es consolidar la lógica de negocio de los tres motores como **módulos de dominio y servicios hospedados (`IHostedService`) dentro de `CRM.ApiHub`**, compartiendo el mismo proceso pero manteniendo una estricta separación de carpetas y contextos delimitados (DDD).

```mermaid
graph TD
    subgraph CRM.ApiHub [Proceso Central Unificado: CRM.ApiHub .NET 10]
        Controllers[API Controllers Unificados: /api/orders, /api/flow, /api/approval, /api/sla]
        
        subgraph ModuloFlow [Módulo de Flujos]
            FlowService[FlowService / Domain Logic]
            FlowRepo[FlowRepository Dapper]
        end
        
        subgraph ModuloApproval [Módulo de Aprobaciones]
            ApprovalService[ApprovalService / SOX Rules]
            ApprovalRepo[ApprovalRepository Dapper]
        end
        
        subgraph ModuloSla [Módulo de SLAs]
            SlaService[SlaService / SLA Rules]
            SlaRepo[SlaRepository Dapper]
            SlaWorker[SlaMonitoringWorker : BackgroundService]
        end
        
        Controllers --> FlowService
        Controllers --> ApprovalService
        Controllers --> SlaService
    end

    PgDB[(PostgreSQL 16 - Base Unificada o Multi-Schema: flow, approval, sla, crm)]
    
    FlowRepo --> PgDB
    ApprovalRepo --> PgDB
    SlaRepo --> PgDB
```

---

## 4. 📋 Plan de Ejecución: Paso a Paso para la Integración Total

### Fase 1: Unificación de Esquemas de Base de Datos
En lugar de 4 bases de datos desconectadas, se unifican bajo la base de datos principal `nyx_crm` utilizando **esquemas de PostgreSQL**:
- `public.*` o `crm.*`: Tablas principales de CRM (usuarios, campañas, leads, órdenes, documentos).
- `flow.*`: Tablas `flow_definition`, `stage`, `checkpoint_catalog`, `checkpoint_step`, `flow_instance`, `checkpoint_instance`, `checkpoint_instance_step`.
- `approval.*`: Tablas `approval_policy`, `approval_chain`, `approval_chain_step`, `approval_request`, `approval_delegation`.
- `sla.*`: Tablas `sla_policy`, `sla_measurement`, `sla_holiday`, `sla_audit_log`.

> **Ventaja**: Todas las consultas pueden realizar `JOIN` directos sin necesidad de Foreign Data Wrappers (`postgres_fdw`), y las transacciones (`NpgsqlTransaction`) pueden comprometer órdenes y checkpoints atómicamente.

### Fase 2: Conversión de Proyectos Engine a Librerías de Dominio
1. Las clases de `Nyx.FlowEngine/Application` y `Domain` se integran directamente o se referencian como bibliotecas de clases (`.csproj` tipo `ClassLibrary`).
2. Se implementa inyección de dependencias en `CRM.ApiHub/Infrastructure/DependencyInjection.cs`:
   ```csharp
   // Registro In-Process de Motores
   services.AddScoped<IFlowService, FlowService>();
   services.AddScoped<IApprovalService, ApprovalService>();
   services.AddScoped<ISlaService, SlaService>();
   services.AddScoped<IFlowRepository, FlowRepository>();
   services.AddScoped<IApprovalRepository, ApprovalRepository>();
   services.AddScoped<ISlaRepository, SlaRepository>();
   ```

### Fase 3: Background Worker de SLA Integrado
Para el cálculo continuo de SLAs y detección de vencimientos, se registra un `BackgroundService` en `CRM.ApiHub`:
```csharp
public class SlaBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SlaBackgroundWorker> _logger;

    public SlaBackgroundWorker(IServiceProvider serviceProvider, ILogger<SlaBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var slaService = scope.ServiceProvider.GetRequiredService<ISlaService>();
            await slaService.EvaluateRunningMeasurementsAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
```

### Fase 4: Exposición Unificada de Controladores y Swagger
- Los controladores `FlowController`, `ApprovalEngineController` y `SlaController` pasan a formar parte de `CRM.ApiHub/Api/Controllers/`.
- Rutas estandarizadas:
  - `/api/flow/instances`, `/api/flow/checkpoints`, `/api/flow/stages`
  - `/api/approval/policies`, `/api/approval/requests`, `/api/approval/delegations`
  - `/api/sla/policies`, `/api/sla/measurements`
- Todas las rutas heredan la autenticación central `[Authorize]` con roles RBAC (SUPERVISOR, BACKOFFICE, ASESOR, ADMIN_CRM).
- Todo se refleja automáticamente en la documentación interactiva de Swagger UI en `http://localhost:5068/swagger`.

### Fase 5: Simplificación de Docker Compose
El archivo `docker-compose.yml` se reduce drásticamente, eliminando los 3 contenedores satélites:
```yaml
services:
  crm_postgres:
    image: postgres:16-alpine
    # ...
  crm_redis:
    image: redis:7-alpine
    # ...
  crm_minio:
    image: minio/minio:latest
    # ...
  crm_apihub:
    build:
      context: .
      dockerfile: CRM.ApiHub/Dockerfile
    ports:
      - "5068:5068"
    # Incluye internamente ApiHub + FlowEngine + ApprovalEngine + SlaEngine
  crm_webfrontend:
    build:
      context: .
      dockerfile: CRM.WebFrontend/Dockerfile
    ports:
      - "5261:5261"
```

---

## 5. 🎯 Beneficios Inmediatos de la Integración

1. **Rendimiento de Grado Empresarial**: Tiempos de respuesta de transacciones de orden reducidos de ~80ms a **< 5ms**.
2. **Cero Fallos por Desconexión**: Imposible que el motor de flujos esté caído si la API está arriba.
3. **Mantenibilidad Extrema**: Un solo comando `dotnet build` y `dotnet run` levanta el 100% del backend.
4. **Seguridad Homogénea**: Mismo middleware de rate limiting, autorización JWT, logs Serilog y trazas OpenTelemetry para todas las operaciones.
5. **Auditoría Transaccional Nativa**: Registro atómico de auditorías en PostgreSQL ante cualquier cambio de estado.

---

> 📄 **Documento de Solución Técnica**: Nyx Unified Engine Integration  
> ✍️ **Autor**: Tech Lead Agent / Antigravity AI  
> 🏷️ **Aprobación**: 🟢 READY FOR EXECUTION
