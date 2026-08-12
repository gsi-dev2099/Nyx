# Guía de Clonación, Despliegue y Restauración de Datos con Docker

Esta guía documenta paso a paso el procedimiento estándar para cualquier desarrollador u operador que clone el repositorio **Nyx CRM** (`https://github.com/gsi-dev2099/Nyx.git`) y requiera poner en marcha la solución completa con sus bases de datos restauradas y datos de prueba.

---

## 📋 1. Prerrequisitos del Sistema

Antes de comenzar, asegúrate de contar con los siguientes componentes instalados en tu máquina local o servidor:

* **Git**: v2.30+
* **Docker Engine**: v24.0+ / **Docker Desktop** (Windows / macOS / Linux)
* **Docker Compose**: v2.20+ (`docker compose` v2 integrados)

---

## 📥 2. Clonación del Repositorio

Abre una terminal y ejecuta:

```bash
# Clonar el repositorio oficial
git clone https://github.com/gsi-dev2099/Nyx.git

# Entrar a la carpeta del proyecto
cd Nyx
```

> [!NOTE]
> Si deseas trabajar en la rama activa de desarrollo, cambia a la rama de características:
> ```bash
> git checkout feature/flujos-incidencias-referidos
> ```

---

## ⚙️ 3. Configuración de Variables de Entorno

El proyecto incluye un archivo de plantilla `.env.example` con las credenciales y puertos por defecto preconfigurados para desarrollo.

```bash
# Copiar el archivo de ejemplo a .env
cp .env.example .env
```

Contenido por defecto de `.env`:
```env
POSTGRES_USER=ronald
POSTGRES_PASSWORD=Gs1$2099Zx23rO24M4r25
POSTGRES_MULTIPLE_DATABASES=nyx_crm,nx_ecosystem
POSTGRES_PORT=5432
REDIS_PORT=6379
ASPNETCORE_ENVIRONMENT=Production
HTTP_PORT=80
```

---

## 🐳 4. Levantamiento Automático con Docker Compose

Toda la infraestructura está containerizada en 5 servicios interconectados:
1. **`crm_postgres`**: Base de datos PostgreSQL 16 Alpine con esquemas y datos iniciales.
2. **`crm_redis`**: Almacén Redis 7 Alpine para caché de sesión y SignalR.
3. **`crm_apihub`**: API Backend REST en .NET 10.
4. **`crm_webfrontend`**: Servidor Web Blazor (Supervisores y Asesores) con Proxy YARP.
5. **`sla_engine_api`**: API para motor de cómputo SLA y tiempos de atención.

Para construir e iniciar todos los servicios:

```bash
docker compose up -d --build
```

---

## 🗄️ 5. ¿Cómo Funciona la Restauración Automática de Datos?

Al ejecutar `docker compose up -d`, la base de datos PostgreSQL lee el volumen montado `./db_export:/docker-entrypoint-initdb.d`.

Durante el primer arranque (o tras limpiar volúmenes):
1. **`01_init_databases.sh`**:
   - Crea los roles de sistema (`postgres`, `ext_nyx`, `usr_consultas`, `web_seleccion`, `nexus`, `api_srv`).
   - Crea las bases de datos `nyx_crm` y `nx_ecosystem`.
   - Importa los archivos semilla `nyx_crm_backup.sql` y `nx_ecosystem_backup.sql`.
2. **`02_update_roles_substatuses.sql`**:
   - Aplica parches de subestados (ej. `POR_CORREGIR_ASESOR`) y vinculaciones de roles de usuarios.
3. **`03_fix_passwords.sql`**:
   - Garantiza que las contraseñas del entorno de desarrollo para los usuarios de prueba estén homologadas.

---

## 🔍 6. Verificación de Estado y Salud de Servicios

### Verificar Contenedores en Ejecución
```bash
docker ps
```
Deberías ver los 5 contenedores en estado `Up` o `healthy`.

### Ver Logs de Inicialización de PostgreSQL
```bash
docker compose logs -f crm_postgres
```
Busca las líneas finales:
```text
=== Databases restored successfully ===
```

### Probar Conexión e Inspeccionar Tablas en PostgreSQL
```bash
# Consultar esquemas en nyx_crm
docker exec -it crm_postgres psql -U ronald -d nyx_crm -c "\dn"

# Contar usuarios cargados en nx_ecosystem
docker exec -it crm_postgres psql -U ronald -d nx_ecosystem -c "SELECT count(*) FROM access_control.app_user;"
```

---

## 🌐 7. Endpoints y Accesos de la Aplicación

Una vez iniciados los contenedores, la aplicación estará disponible en los siguientes puertos:

| Servicio | URL / Puerto | Descripción |
| :--- | :--- | :--- |
| **Web Frontend (Blazor)** | [http://localhost:5261](http://localhost:5261) | Panel principal para Supervisores y Asesores |
| **Backend REST (ApiHub)** | [http://localhost:5068](http://localhost:5068) | API REST principal (.NET 10) |
| **Swagger UI (Docs API)** | [http://localhost:5068/swagger](http://localhost:5068/swagger) | Interfaz interactiva de endpoints |
| **SLA Engine API** | [http://localhost:5070](http://localhost:5070) | Engine de cálculo de incidencias |
| **PostgreSQL DB** | `localhost:5432` | DB Server (`nyx_crm`, `nx_ecosystem`) |
| **Redis Cache** | `localhost:6379` | Almacén Key-Value de sesiones |

---

## 🔑 8. Credenciales de Prueba Preconfiguradas

Puedes iniciar sesión en la aplicación ([http://localhost:5261](http://localhost:5261)) usando cualquiera de los siguientes usuarios:

| Rol | Usuario | Contraseña |
| :--- | :--- | :--- |
| **Supervisor** | `patricia` | `password123` |
| **Supervisor / Admin** | `cnaranjo` | `password123` |
| **Supervisor** | `jhuby` | `password123` |
| **Asesor** | `dramos` | `password123` |
| **Asesor** | `rurbina` | `password123` |
| **Fallback Dev** | `test.supervisor` | `password123` |

---

## 🛠️ 9. Solución de Problemas y Restauración Manual

### Opción A: Reiniciar Completo a Estado Semilla Original (Recomendado)
Si realizaste pruebas y deseas borrar todos los cambios locales y volver a dejar la base de datos limpia como al clonar:

```bash
# Detener contenedores y eliminar volúmenes de datos
docker compose down -v

# Volver a levantar (volverá a ejecutar el initdb con las semillas SQL)
docker compose up -d
```

### Opción B: Restaurar Manualmente los dumps SQL (Sin borrar contenedores)
Si el contenedor PostgreSQL ya estaba corriendo y deseas reimportar las semillas manualmente:

```bash
# Restaurar la base de datos nyx_crm
docker exec -i crm_postgres psql -U ronald -d nyx_crm < db_export/nyx_crm_backup.sql

# Restaurar la base de datos nx_ecosystem
docker exec -i crm_postgres psql -U ronald -d nx_ecosystem < db_export/nx_ecosystem_backup.sql

# Aplicar parches finales
docker exec -i crm_postgres psql -U ronald -d nyx_crm < db_export/02_update_roles_substatuses.sql
docker exec -i crm_postgres psql -U ronald -d nx_ecosystem < db_export/03_fix_passwords.sql
```
