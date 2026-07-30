# Resiliencia y Tolerancia a Fallos ante la caída de FDW

Este documento detalla la investigación de dependencias del Foreign Data Wrapper (FDW) en el esquema `ext_ecosystem`, la simulación de caídas del servidor externo y las soluciones de degradación gradual implementadas para asegurar la disponibilidad continua de los servicios esenciales del CRM.

---

## 1. Dependencias Críticas de FDW

El esquema `ext_ecosystem` de la base de datos `nyx_crm` enlaza a un servidor remoto mediante `postgres_fdw`:
* **Servidor Remoto**: `ecosystem_server`
* **Conexión**: `host=187.77.197.169, port=5433, dbname=nx_ecosystem`

### Tablas Externas Importadas:
* `ext_ecosystem.collaborators` (Información de empleados/colaboradores) - **Crítica**.
* `ext_ecosystem.admin_divisions` (Divisiones político-administrativas) - *No utilizada actualmente*.
* `ext_ecosystem.structural_divisions` (Estructura organizativa) - *No utilizada actualmente*.
* `ext_ecosystem.countries` / `nationalities` / `user_trajectory` - *No utilizadas actualmente*.

### Impacto de Fallo de Conexión:
Dado que la tabla `ext_ecosystem.collaborators` se cruza a través de sentencias SQL (`LEFT JOIN`) en consultas críticas del CRM, un fallo de conexión física o lógica con el servidor `187.77.197.169` provocaría el colapso de toda la consulta de base de datos. Como consecuencia:
* Los usuarios no podrían iniciar sesión (fallo en la recuperación del perfil de usuario).
* Los supervisores no podrían ver los dashboards ni reportes de ventas.
* Los asesores no podrían ver el historial ni la línea de tiempo de los pedidos.

---

## 2. Estrategia de Degradación Gradual (Fallback)

Para evitar que una falla externa interrumpa las operaciones clave del negocio, se implementó un mecanismo de **Fallback en consultas** que entra en acción cuando el FDW arroja una excepción de base de datos (`DbException`).

### Puntos de Fallback Implementados:

1. **Autenticación e Inicio de Sesión de Usuarios**:
   * **Archivo**: [UserRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/UserRepository.cs) (`GetUserDetailByIdAsync`)
   * **Comportamiento**: Si ocurre un error al consultar la tabla externa de colaboradores, se captura la excepción, se registra una advertencia en el sistema de logs (`ILogger`), y se ejecuta una consulta local sobre la tabla `user_service.users`. En lugar del nombre completo real del colaborador, se asigna temporalmente el nombre de usuario local (`u.username`). Esto permite a los usuarios iniciar sesión sin interrupciones.

2. **Línea de Tiempo y Auditoría de Órdenes**:
   * **Archivo**: [SalesOrderRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/SalesOrderRepository.cs) (`GetOrderHistoryTimelineAsync`)
   * **Comportamiento**: La consulta original realiza múltiples `LEFT JOIN` a la tabla externa `collaborators` para resolver los nombres de quienes realizaron los cambios. Al capturar una falla del FDW, la consulta de respaldo realiza las uniones directamente a la tabla local `user_service.users` y mapea los nombres de los actores con su respectivo `username` local.

3. **Reportes de Ventas por Asesor**:
   * **Archivo**: [ReportRepository.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Infrastructure/Persistence/ReportRepository.cs) (`GetSalesByAsesorAsync`)
   * **Comportamiento**: Si el FDW no responde, el reporte se genera omitiendo el cruce con la tabla externa de colaboradores y calculando los indicadores agrupados y ordenados mediante la información de usuarios locales.

4. **Listado de Asesores Activos**:
   * **Archivo**: [CampaignController.cs](file:///c:/Users/RRHH/Downloads/newCRM/CRM.ApiHub/Api/Controllers/CampaignController.cs) (`GetAdvisors`)
   * **Comportamiento**: En caso de error, el endpoint devuelve los usuarios con rol "ASESOR" consultando la tabla local de usuarios y exponiendo sus nombres de usuario, en lugar de bloquear el flujo.

---

## 3. Instructivo para Simulación de Caída de FDW

Para evaluar y certificar el comportamiento de degradación en ambientes de prueba, se pueden usar los siguientes comandos de simulación:

### Método A: Redireccionamiento del Servidor de FDW (Recomendado)
Consiste en alterar los parámetros del servidor extranjero en la base de datos local para forzar un error de conexión de red (timeout):
1. **Ejecutar simulación (desactivar FDW)**:
   ```sql
   ALTER SERVER ecosystem_server OPTIONS (SET host '192.0.2.1', SET port '9999');
   ```
2. **Prueba**: Inicia sesión en el CRM o ingresa a ver los detalles de una orden. La operación tardará un breve momento en dar timeout contra el FDW, pero seguidamente completará la acción exitosamente haciendo uso de la información local. En el log del backend se observarán los warnings descritos.
3. **Revertir simulación (restaurar FDW)**:
   ```sql
   ALTER SERVER ecosystem_server OPTIONS (SET host '187.77.197.169', SET port '5433');
   ```

### Método B: Renombrar la Tabla Externa
Simula un fallo donde el esquema remoto ha cambiado o no está disponible localmente (da un fallo inmediato sin esperar timeout de red):
1. **Ejecutar simulación**:
   ```sql
   ALTER FOREIGN TABLE ext_ecosystem.collaborators RENAME TO collaborators_temp;
   ```
2. **Prueba**: Realiza las mismas validaciones. El fallo es instantáneo y se activa inmediatamente el flujo de respaldo de datos locales.
3. **Revertir simulación**:
   ```sql
   ALTER FOREIGN TABLE ext_ecosystem.collaborators_temp RENAME TO collaborators;
   ```
