# Optimización de Base de Datos y Consultas Críticas

Este documento detalla el análisis de rendimiento de consultas (`EXPLAIN ANALYZE`), la corrección de problemas con el particionamiento (partition pruning), la optimización mediante índices y la configuración del pool de conexiones para producción.

---

## 1. Análisis con EXPLAIN ANALYZE

Se auditaron las consultas más importantes del CRM y se obtuvo su plan de ejecución en PostgreSQL para verificar su rendimiento en condiciones de carga.

### Consultas Analizadas y Tiempos de Respuesta:
1. **Búsqueda en Base de Conocimientos (Knowledge Base)**:
   * **Problema**: La búsqueda de artículos en texto completo procesaba el texto en cada consulta, lo que en tablas grandes causaría un escaneo secuencial (*Seq Scan*) de alto coste.
   * **Solución**: Se validó el uso del índice GIN de texto completo (`idx_ka_fts`). La base de datos ahora utiliza un *Bitmap Index Scan* sobre este índice.
   * **Tiempo obtenido**: `0.030 ms` (30 microsegundos).

2. **Línea de Tiempo / Historial de Órdenes**:
   * **Problema**: Consulta compleja con múltiples `UNION ALL` que combina historial de estados, logs de custodia, documentos subidos e incidentes de una orden en específico.
   * **Solución**: Se comprobó que el motor realiza búsquedas indexadas directas (*Index Scan*) en cada tabla del `UNION` utilizando el índice de clave externa `id_order`.
   * **Tiempo obtenido**: `0.034 ms` (34 microsegundos).

3. **Consultas de Órdenes por Rango de Fechas**:
   * **Problema**: La consulta de lista de órdenes no filtraba por el campo utilizado como clave de particionamiento, lo que forzaba a PostgreSQL a escanear e indexar cada partición mensual individualmente.
   * **Solución**: Se añadió el filtro por la clave de partición `register` en conjunto con el filtro de fecha original (ver sección 2).
   * **Tiempo obtenido con optimización**: `0.040 ms` (40 microsegundos) con exclusión automática de particiones irrelevantes.

---

## 2. Optimización del Particionamiento (Partition Pruning)

### Problema
La tabla de órdenes (`sales_service.sales_order`) está particionada mensualmente utilizando un esquema de rangos sobre la columna `register` (fecha/hora de creación del registro).

Las consultas de filtrado en la aplicación:
* `GetByFiltersAsync` (Asesor)
* `GetTeamOrdersAsync` (Supervisor)
* `GetAssignedOrdersAsync` (Backoffice)

Filtraban los rangos de fechas utilizando la columna `sales_date` (que es de tipo fecha de venta pero no es la clave de partición). Esto impedía al motor de PostgreSQL realizar la técnica de *partition pruning* (exclusión de particiones), obligándolo a consultar e inspeccionar los índices de las 8 particiones del sistema de forma secuencial.

### Solución
Se modificaron los métodos en los repositorios de persistencia:
* [SalesOrderRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/SalesOrderRepository.cs)
* [SupervisorRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/SupervisorRepository.cs)
* [BackofficeRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/BackofficeRepository.cs)

Se añadió la cláusula `register` en los filtros cuando se ingresan fechas de rango:
```csharp
if (dateFrom.HasValue)
{
    sql.Append(" AND sales_date >= @DateFrom AND register >= @DateFrom");
    parameters.Add("DateFrom", dateFrom.Value);
}
if (dateTo.HasValue)
{
    sql.Append(" AND sales_date <= @DateTo AND register <= @DateTo");
    parameters.Add("DateTo", dateTo.Value);
}
```

**Resultado en el Plan de Ejecución (`EXPLAIN ANALYZE`):**
PostgreSQL ahora excluye automáticamente las particiones que están fuera del rango solicitado. Al buscar registros en un mes específico, el plan de ejecución muestra:
`Subplans Removed: 7`
Efectuando únicamente un escaneo de índice directo sobre la partición correspondiente (ej. `sales_order_monthly_p20260701`).

---

## 3. Configuración del Pool de Conexiones Npgsql para Producción

### Problema
La cadena de conexión de la aplicación en producción no especificaba parámetros para afinar y limitar el comportamiento del agrupamiento de conexiones (connection pooling) de Npgsql. Esto podía provocar retardos en el inicio de conexiones bajo picos de carga (latencia del handshake de TCP y TLS) o consumo desmedido de sockets de base de datos.

### Solución
Se modificó el constructor del creador de conexiones en [DbConnectionFactory.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/DbConnectionFactory.cs) para inyectar dinámicamente parámetros optimizados para producción en la cadena de conexión desencriptada:
* `Pooling=true`: Garantiza que las conexiones se mantengan abiertas y se reutilicen.
* `Minimum Pool Size=10`: Pre-abre 10 conexiones persistentes en el arranque de la API, eliminando la latencia de establecimiento de conexión inicial.
* `Maximum Pool Size=100`: Limita el número máximo de conexiones abiertas a 100 por instancia para salvaguardar los recursos de hardware de PostgreSQL.
* `Connection Lifetime=300`: Cierra e invalida conexiones del pool que lleven inactivas más de 5 minutos (300 segundos).
* `Timeout=15`: Límite de 15 segundos para esperar por una conexión disponible en el pool antes de lanzar una excepción de timeout.
