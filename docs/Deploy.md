# Documentación de Despliegue (Producción / Staging)

Este documento detalla los pasos necesarios para desplegar **CRM.ApiHub** en un entorno productivo o de staging.

## 1. Requisitos Previos

*   **.NET SDK 10.0** instalado en el servidor (o el Runtime de ASP.NET Core 10.0).
*   Acceso a la base de datos PostgreSQL (`nyx_crm`).
*   Configuración del FDW (`ext_ecosystem`) ya funcional en la base de datos.
*   Redis server (para rate limiting y SignalR, si aplica).

## 2. Variables de Entorno Requeridas

El entorno debe tener configuradas las siguientes variables de entorno para sobreescribir los valores por defecto de `appsettings.Production.json`. No se deben almacenar secretos en el código fuente.

| Variable de Entorno | Descripción | Ejemplo |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | Define el entorno de ejecución. | `Production` |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a PostgreSQL principal (`nyx_crm`). | `Host=localhost;Database=nyx_crm;Username=usr;Password=pass` |
| `JwtSettings__SecretKey` | Clave secreta (256-bit min) para firmar los JWT. | `tu_super_clave_secreta_larga_2026!` |
| `RedisSettings__ConnectionString` | Cadena de conexión al servidor Redis. | `localhost:6379,abortConnect=false` |

*(Nota: En Linux/Docker, los doble guiones bajos `__` se utilizan para navegar la jerarquía del JSON).*

## 3. Publicación del Proyecto

Para generar los binarios listos para producción:

```bash
dotnet publish CRM.ApiHub/CRM.ApiHub.csproj -c Release -o ./publish
```

Esto generará los archivos en la carpeta `./publish`.

## 4. Ejecución de la Aplicación

Navega a la carpeta de publicación y ejecuta el binario. Puedes usar `systemd` en Linux o IIS en Windows para mantener el proceso vivo.

```bash
cd ./publish
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=tu-host;Database=nyx_crm;Username=user;Password=pass"
export JwtSettings__SecretKey="TU-CLAVE-SECRETA"
export RedisSettings__ConnectionString="localhost:6379"

dotnet CRM.ApiHub.dll
```

## 5. Verificación de Salud (Health Check)

Una vez que la aplicación esté corriendo, puedes verificar que todo funciona correctamente consultando el endpoint de *Health Check*. Este script verifica la conexión tanto a la base de datos local como al ecosistema externo (FDW).

**Endpoint:**
`GET /api/health`

**Ejemplo usando cURL:**
```bash
curl -i http://localhost:5068/api/health
```

**Respuesta Esperada (200 OK):**
```json
{
  "status": "Healthy",
  "timestamp": "2026-07-30T10:00:00.0000000Z",
  "checks": {
    "database": {
      "status": "Healthy",
      "message": "OK"
    },
    "fdwEcosystem": {
      "status": "Healthy",
      "message": "OK"
    }
  }
}
```

Si alguno de los servicios (BD o FDW) falla, el endpoint devolverá un código de estado `503 Service Unavailable` y detallará el problema en el JSON de respuesta.

## 6. Logs Estructurados (Serilog)

En producción, la aplicación está configurada para usar **Serilog**.
Los logs se imprimirán en la consola en formato **JSON Compacto** (`CompactJsonFormatter`), ideal para ser recolectados por agentes como Promtail, Fluentd, Filebeat, o Datadog.

Además, los logs se guardarán localmente de forma rotativa (diariamente) en la carpeta `logs/` (ej. `logs/crm-api-20260730.log`).
