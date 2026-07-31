# 🚀 Guía de Optimización Técnica — CRM CallCenter
> **Tech Lead:** Ronald · **Fecha:** 30/07/2026 · **Contexto:** Sprint 2 — Mejoras de Arquitectura y Rendimiento  
> **Stack:** .NET 10 · Dapper 2.1 · Npgsql 10 · Blazor Hybrid · PostgreSQL 17 · SignalR

---

## 1. Diagnóstico Real del Stack de Datos: ¿EF Core o Dapper?

> **Veredicto: El proyecto ya está 100% en Dapper. No hay EntityFrameworkCore en ningún `.csproj` ni `DbContext` en ninguna clase.**

El equipo tomó la decisión arquitectónica correcta. Lo que existe en el `CRM.ApiHub.csproj`:

```xml
<PackageReference Include="Dapper"  Version="2.1.35" />
<PackageReference Include="Npgsql"  Version="10.0.3" />
```

Sin ningún rastro de `Microsoft.EntityFrameworkCore` ni `Npgsql.EntityFrameworkCore.PostgreSQL`.

---

## 2. ¿Dapper vs EF Core? — Análisis para este Sistema Específico

| Criterio | Dapper ✅ (Actual) | EF Core ❌ (Alternativa descartada) |
|---|---|---|
| **Velocidad bruta** | ~2–5x más rápido en lecturas masivas | Overhead de change tracking y materialización |
| **Control de SQL** | SQL explícito, sin sorpresas de traducción | Genera SQL sub-óptimo en queries complejas |
| **PostgreSQL nativo** | Soporte total a JSONB, Arrays, CTE, particiones, FDW | Traducciones limitadas para features PostgreSQL avanzados |
| **Mapeo snake_case** | `MatchNamesWithUnderscores = true` (una línea) | Requiere `UseSnakeCaseNamingConvention()` + package adicional |
| **Transacciones manuales** | Control total con `IDbTransaction` | Abstraído con riesgo de transacciones fantasma |
| **Migrations** | No aplica (BD gestionada externamente por DBA) | Necesario si Dapper gestionara el schema |
| **Curva de aprendizaje** | SQL estándar que cualquier dev conoce | DSL LINQ + debugging de queries generadas |
| **Multi-schema PostgreSQL** | Consultas directas a cualquier schema | Complicado con múltiples schemas (user_service, sales_service, etc.) |

**Conclusión:** Para un CRM con PostgreSQL multi-schema, particiones (`pg_partman`), FDW y procesos almacenados propios, **Dapper es la elección óptima y debe mantenerse**.

---

## 3. Patrón de Paginación Moderno — Best Practice para este Sistema

### ❌ Qué tienen ahora (problemático)
```csharp
// GetByFiltersAsync en SalesOrderRepository.cs
// Devuelve TODOS los registros sin límite
return await connection.QueryAsync<SalesOrder>(
    new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct)
);
```
Con miles de órdenes esto puede saturar memoria y tiempo de respuesta.

### ✅ Patrón Recomendado: Cursor-based Pagination (Keyset) + COUNT paralelo

Para sistemas de alta carga con PostgreSQL, hay dos estrategias principales:

#### Opción A: Offset Pagination (simple, adecuado para < 500k registros)
```csharp
// Modelo de respuesta paginada — genérico y reutilizable
public sealed record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

// En SalesOrderRepository — query con LIMIT/OFFSET + COUNT paralelo
public async Task<PagedResult<SalesOrder>> GetByFiltersPaginatedAsync(
    long? userId, long? statusId, long? campaignId,
    DateTime? dateFrom, DateTime? dateTo,
    int page = 1, int pageSize = 50,
    CancellationToken ct = default)
{
    using var connection = _connectionFactory.CreateConnection();
    var where = new StringBuilder("WHERE 1=1");
    var parameters = new DynamicParameters();

    if (userId.HasValue)    { where.Append(" AND id_user = @UserId");       parameters.Add("UserId", userId.Value); }
    if (statusId.HasValue)  { where.Append(" AND id_status = @StatusId");   parameters.Add("StatusId", statusId.Value); }
    if (campaignId.HasValue){ where.Append(" AND id_cmpg = @CampaignId");   parameters.Add("CampaignId", campaignId.Value); }
    if (dateFrom.HasValue)  { where.Append(" AND sales_date >= @DateFrom"); parameters.Add("DateFrom", dateFrom.Value); }
    if (dateTo.HasValue)    { where.Append(" AND sales_date <= @DateTo");   parameters.Add("DateTo", dateTo.Value); }

    int offset = (page - 1) * pageSize;
    parameters.Add("Limit",  pageSize);
    parameters.Add("Offset", offset);

    // Una sola consulta con COUNT usando window function (evita doble viaje a BD)
    var sql = $@"
        SELECT *, COUNT(*) OVER() AS total_count
        FROM sales_service.sales_order
        {where}
        ORDER BY sales_date DESC
        LIMIT @Limit OFFSET @Offset;";

    var rows = await connection.QueryAsync<(SalesOrder order, int totalCount)>(
        new CommandDefinition(sql, parameters, cancellationToken: ct),
        map: (row) => /* mapeo custom */
    );

    var totalCount = rows.FirstOrDefault().totalCount;
    return new PagedResult<SalesOrder>(
        rows.Select(r => r.order),
        totalCount,
        page,
        pageSize,
        (int)Math.Ceiling((double)totalCount / pageSize)
    );
}
```

#### Opción B: Keyset Pagination (óptima para > 1M registros, sin OFFSET)
```csharp
// Usar el último ID/fecha visto como cursor — evita degradación con OFFSET grandes
// Ideal para scroll infinito en dashboards en tiempo real
public async Task<IEnumerable<SalesOrder>> GetNextPageAsync(
    long? lastIdOrder,       // cursor: ID del último elemento de la página anterior
    DateTime? lastSalesDate, // cursor secundario para orden estable
    int pageSize = 50,
    CancellationToken ct = default)
{
    using var connection = _connectionFactory.CreateConnection();
    var sql = lastIdOrder.HasValue
        ? @"SELECT * FROM sales_service.sales_order
            WHERE (sales_date, id_order) < (@LastSalesDate, @LastIdOrder)
            ORDER BY sales_date DESC, id_order DESC
            LIMIT @PageSize;"
        : @"SELECT * FROM sales_service.sales_order
            ORDER BY sales_date DESC, id_order DESC
            LIMIT @PageSize;";

    return await connection.QueryAsync<SalesOrder>(
        new CommandDefinition(sql,
            new { LastIdOrder = lastIdOrder, LastSalesDate = lastSalesDate, PageSize = pageSize },
            cancellationToken: ct)
    );
}
```

### 📐 Recomendación para CRM CallCenter
- **Listados paginados del Asesor y BackOffice** → **Offset Pagination** (simples, < 10k registros por usuario).
- **Dashboard Supervisor con miles de órdenes de equipo** → **Keyset Pagination** (scroll infinito o carga incremental sin lag).

### ✅ Contrato de Respuesta Estandarizado (para todos los endpoints de listado)
```json
{
  "items": [...],
  "totalCount": 847,
  "page": 1,
  "pageSize": 50,
  "totalPages": 17,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 4. Otras Mejoras de Alto Impacto — Sprint 2

### 4.1 🗃️ Dapper — Mejoras de Patrón

**Problema actual:** `SELECT *` en múltiples repositorios.
```csharp
// ACTUAL (evitar en producción)
const string sql = "SELECT * FROM sales_service.sales_order WHERE id_order = @IdOrder";

// RECOMENDADO (columnas explícitas + alias para mapeo limpio)
const string sql = @"
    SELECT id_order AS IdOrder,
           id_lead  AS IdLead,
           id_cmpg  AS IdCmpg,
           id_user  AS IdUser,
           id_status AS IdStatus,
           sales_date AS SalesDate,
           total_value AS TotalValue
    FROM sales_service.sales_order
    WHERE id_order = @IdOrder";
```

**Beneficio:** Reduce ancho de banda entre BD y aplicación. Evita materializar columnas innecesarias. Hace explícito el contrato de datos.

---

**Patrón de Multi-mapping para JOINs (en lugar de múltiples queries):**
```csharp
// Una sola query para obtener Orden + Status + Campaign
var sql = @"
    SELECT o.*, s.name AS StatusName, s.color AS StatusColor, c.name AS CampaignName
    FROM sales_service.sales_order o
    JOIN sales_service.order_status s ON o.id_status = s.id_status
    JOIN campaign_service.campaign c  ON o.id_cmpg   = c.id_cmpg
    WHERE o.id_order = @IdOrder";

var result = await connection.QueryAsync<SalesOrder, OrderStatusInfo, CampaignInfo, SalesOrder>(
    sql,
    (order, status, campaign) => {
        order.StatusName    = status.Name;
        order.StatusColor   = status.Color;
        order.CampaignName  = campaign.Name;
        return order;
    },
    new { IdOrder = idOrder },
    splitOn: "StatusName,CampaignName"
);
```

---

### 4.2 🏎️ Caché de Catálogos con IMemoryCache

Los catálogos (estados de venta, productos, divisas, campañas) no cambian con frecuencia pero se consultan en cada renderizado de formulario.

```csharp
// En CatalogRepository — agregar IMemoryCache
public async Task<IEnumerable<OrderStatus>> GetOrderStatusesAsync(CancellationToken ct = default)
{
    const string cacheKey = "catalog:order_statuses";
    if (_cache.TryGetValue(cacheKey, out IEnumerable<OrderStatus>? cached))
        return cached!;

    using var conn = _connectionFactory.CreateConnection();
    var statuses = await conn.QueryAsync<OrderStatus>("SELECT * FROM sales_service.order_status WHERE is_active = true ORDER BY order_index", cancellationToken: ct);

    _cache.Set(cacheKey, statuses, TimeSpan.FromMinutes(30));
    return statuses;
}
```

**Aplica también a:** `GetCurrencies()`, `GetProducts()`, `GetActiveKbCategories()`.

---

### 4.3 🔄 Operaciones Masivas con Dapper TVP (Table-Valued Parameters)

El `BulkTransferToBackoffice` actual procesa los IDs en un loop. En lotes grandes esto genera N queries:

```csharp
// ACTUAL (problemático para lotes grandes)
foreach (var orderId in orderIds)
    await connection.ExecuteAsync("UPDATE sales_service.sales_order SET ...", ...);

// RECOMENDADO — Array de PostgreSQL en una sola query
public async Task<int> BulkTransferToBackofficeAsync(long[] orderIds, long supervisorId, Guid batchId, ...)
{
    const string sql = @"
        UPDATE sales_service.sales_order
        SET    id_status = @TargetStatus,
               custody_user_id = @BackofficeUserId,
               last_update = NOW()
        WHERE  id_order = ANY(@OrderIds)
          AND  id_status = @CurrentStatus;";

    return await connection.ExecuteAsync(
        new CommandDefinition(sql, new {
            OrderIds     = orderIds,  // Npgsql pasa int[] nativamente como PostgreSQL array
            TargetStatus = TARGET_STATUS,
            CurrentStatus = SOURCE_STATUS,
            BackofficeUserId = supervisorId
        }, cancellationToken: ct)
    );
}
```

**Beneficio:** 1 sola query vs N queries. Para lotes de 100 órdenes, pasa de ~100 roundtrips a 1.

---

### 4.4 ⚡ CancellationToken en TODOS los repositorios

El `ActivationRepository` carece de `ILogger<T>` y no todos los métodos pasan `CancellationToken` a Dapper. Los `CommandDefinition` con `cancellationToken` permiten cancelar queries largas cuando el usuario navega fuera de la página.

```csharp
// PATRÓN CORRECTO — ya aplicado en SalesOrderRepository y SupervisorRepository
// Debe uniformizarse en TODOS los repositorios:
return await connection.QueryAsync<T>(
    new CommandDefinition(sql, parameters, cancellationToken: ct)  // ct siempre propagado
);
```

---

### 4.5 🔐 Output Caching para Endpoints de Solo Lectura (.NET 10)

.NET 10 incluye `OutputCache` nativo. Para endpoints que cambian poco (KB search, catálogos, reportes con filtros de fecha pasada):

```csharp
// En Program.cs de CRM.ApiHub
builder.Services.AddOutputCache(options => {
    options.AddPolicy("Catalogs", builder => builder.Expire(TimeSpan.FromMinutes(15)));
    options.AddPolicy("Reports",  builder => builder.Expire(TimeSpan.FromMinutes(5)));
});

// En KBController
[HttpGet("search")]
[OutputCache(PolicyName = "Reports", VaryByQueryKeys = new[] { "q", "idCmpg" })]
public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] long idCmpg, ...) { }
```

---

### 4.6 🗜️ Response Compression

Agregar compresión Brotli/Gzip en el API para reducir el tamaño de las respuestas JSON en endpoints de listados grandes:

```csharp
// En Program.cs de CRM.ApiHub
builder.Services.AddResponseCompression(options => {
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
// En pipeline:
app.UseResponseCompression();
```

**Impacto esperado:** Reducción de payload de 60–75% en listados JSON de órdenes/supervisión.

---

### 4.7 📊 Índices PostgreSQL Recomendados

Basado en los patrones de query observados en los repositorios:

```sql
-- Índice compuesto para el listado principal de órdenes del asesor
CREATE INDEX CONCURRENTLY idx_sales_order_user_status_date
    ON sales_service.sales_order (id_user, id_status, sales_date DESC);

-- Índice para el dashboard del supervisor (filtra por múltiples asesores)
CREATE INDEX CONCURRENTLY idx_sales_order_status_cmpg
    ON sales_service.sales_order (id_status, id_cmpg, sales_date DESC);

-- Índice para el módulo de activaciones
CREATE INDEX CONCURRENTLY idx_activation_provider_status
    ON sales_service.product_activation_tracking (id_provider, activation_status, expected_activation_date);

-- Índice para búsqueda FTS en KB (si no existe)
CREATE INDEX CONCURRENTLY idx_kb_article_fts
    ON knowledge_service.kb_article USING GIN (to_tsvector('spanish', title || ' ' || content));
```

---

## 5. Resumen de Prioridades de Optimización para Sprint 2

| Prioridad | Mejora | Impacto Esperado | Complejidad |
|---|---|---|---|
| 🔴 **1** | Paginación (`LIMIT/OFFSET` con `COUNT() OVER()`) en todos los listados | Elimina saturación de memoria y timeouts | Baja |
| 🔴 **2** | Array PostgreSQL para `BulkTransfer` (1 query vs N) | De N roundtrips a 1 en envíos masivos | Baja |
| 🟠 **3** | `IMemoryCache` en catálogos (estados, productos, divisas) | -90% de queries repetidas en catálogos | Baja |
| 🟠 **4** | Reemplazar `SELECT *` por columnas explícitas | Menor ancho de banda BD→App | Media |
| 🟠 **5** | Índices SQL compuestos (`CONCURRENTLY`) | -60% tiempo queries de listados | Media (requiere DBA) |
| 🟡 **6** | `OutputCache` en endpoints de KB y reportes | Elimina carga repetida en reportes históricos | Baja |
| 🟡 **7** | `Response Compression` Brotli/Gzip | -60% payload en listados JSON | Muy Baja (2 líneas) |
| 🟡 **8** | Keyset Pagination para Dashboard Supervisor | Sin degradación con datasets > 50k órdenes | Media |
| 🟡 **9** | Multi-mapping Dapper en `GetById` (JOIN en 1 query) | Elimina 2-3 roundtrips por detalle de orden | Media |
| 🟢 **10** | `CancellationToken` uniforme en todos los repositorios | Libera recursos en navegación rápida | Muy Baja |

---

*Guía redactada por Ronald (Tech Lead) para el Sprint 2 del proyecto CRM CallCenter.*
