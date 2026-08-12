# Nyx CRM - Plataforma de Control CallCenter (.NET 10)

[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](#)
[![Security Status](https://img.shields.io/badge/Security-Hardened-blue.svg)](#)
[![Framework](https://img.shields.io/badge/.NET-10.0-purple.svg)](#)

Nyx CRM es una solución empresarial de alta gama diseñada para call centers y equipos de supervisión de ventas. El sistema está estructurado bajo una **Arquitectura Hexagonal (Clean Architecture)** desacoplada y cuenta con un fuerte enfoque en seguridad criptográfica, mitigación de secuestro de sesiones e interfaces fluidas de alto rendimiento mediante Blazor Server y WebAssembly.

---

## 🏗️ Arquitectura del Proyecto

El backend se encuentra desacoplado del frontend bajo los principios del diseño guiado por el dominio (DDD) y arquitectura de puertos y adaptadores:

```
CRM_API/
├── CRM.sln                      # Solución global (.NET 10)
├── CRM.ApiHub/                  # Capa del Backend (API REST)
│   ├── Api/                     # Entrypoint HTTP, Controladores y Filtros
│   ├── Application/             # Casos de Uso, Puertos de Entrada e Interfaces
│   ├── Domain/                  # Modelos Puros, POCOs y Puertos de Repositorio (Agnóstico)
│   └── Infrastructure/          # Adaptadores concretos (Persistencia Dapper, JWT, Cifrado AES)
├── CRM.WebFrontend/             # Servidor Blazor (Supervisores - InteractiveServer / YARP Proxy)
└── CRM.WebFrontend.Client/      # Cliente Blazor WASM (Asesores - InteractiveWebAssembly)
```

---

## 🔒 Seguridad y Endurecimiento

La plataforma implementa las siguientes medidas de seguridad para entornos productivos:

*   **Autenticación Seguro por Cookies (HttpOnly)**: Los tokens JWT de sesión no se almacenan en el navegador (`localStorage`), mitigando ataques XSS. El servidor de Blazor actúa como proxy seguro y escribe cookies cifradas con atributos `HttpOnly`, `Secure` y `SameSite=Lax`.
*   **Mitigación de Session Hijacking (Session Binding)**: El almacén de refresh tokens (`InMemoryRefreshTokenStore`) vincula y valida en cada petición de refresco la **Dirección IP del cliente** (con soporte para cabeceras de proxy `X-Forwarded-For`) y el **User-Agent**.
*   **Firmado Criptográfico Robusto**: Firma del token JWT respaldada por una clave secreta de **584 bits** configurada en variables de entorno o almacén seguro.
*   **Revocación de Sesiones e Inactividad**: Endpoint `/api/auth/logout` para invalidación inmediata de tokens en memoria y cookies configuradas con `SlidingExpiration` de **20 minutos**.

---

## 🐳 Ejecución Completa con Docker (Recomendado)

El proyecto está 100% preparado para ejecutarse mediante Docker Compose. Incluye la inicialización y restauración automática de la base de datos PostgreSQL con datos y estructura inicial de prueba (`db_export/`).

### 1. Despliegue Local Rápido
```bash
# Copiar variables de entorno de ejemplo si es necesario
cp .env.example .env

# Levantar todos los servicios en segundo plano (PostgreSQL, Redis, ApiHub, WebFrontend, SLA Engine)
docker compose up -d --build
```

### 2. Servicios Disponibles
* **Frontend Web (Blazor Supervisors & Advisors)**: [http://localhost:5261](http://localhost:5261)
* **Backend API REST**: [http://localhost:5068](http://localhost:5068)
* **Documentación Swagger**: [http://localhost:5068/swagger](http://localhost:5068/swagger)
* **PostgreSQL (Con Semillas)**: `localhost:5432` (Usuario: `ronald` | Bases de datos: `nyx_crm`, `nx_ecosystem`)
* **Redis**: `localhost:6379`

### 3. Reiniciar / Limpiar Datos de Prueba
Si deseas reiniciar la base de datos a su estado semilla original exportado en `db_export/`:
```bash
docker compose down -v
docker compose up -d
```

---

## 🐙 Vinculación a un Nuevo Repositorio en GitHub

Este proyecto ha sido desvinculado de su repositorio anterior. Para subirlo a tu nuevo repositorio de GitHub:

```bash
# 1. Agregar la URL de tu nuevo repositorio en GitHub
git remote add origin https://github.com/TU_USUARIO/TU_NUEVO_REPOSITO.git

# 2. Subir la rama actual con sus tags y commits
git push -u origin feature/flujos-incidencias-referidos
# o subir la rama main:
# git push -u origin main
```

---

## 🚀 Instalación y Configuración Manual (.NET Local)

### 📋 Prerrequisitos
* SDK de .NET 10.0
* Base de datos PostgreSQL local o vía Docker

### 🛠️ Compilación y Ejecución Manual

Compilar toda la solución:
```bash
dotnet build CRM.sln
```

Ejecutar el Backend (ApiHub):
```bash
dotnet run --project CRM.ApiHub/CRM.ApiHub.csproj --launch-profile "http"
```

Ejecutar el Frontend (Blazor Web):
```bash
dotnet run --project CRM.WebFrontend/CRM.WebFrontend.csproj --launch-profile "http"
```

---

## 🧪 Pruebas de Desarrollo y Entornos

### Credenciales de Prueba (Semilla en DB / Fallback)
* **Supervisor**: `patricia` / `password123` (o `test.supervisor` / `password123`)
* **Asesor**: `test.asesor` / `password123`

### Colección de Postman y Contratos
* La colección oficial de endpoints está exportada en `docs/CRM_CallCenter_Semana1.postman_collection.json`.
* La especificación formal de todos los Request/Response del API se encuentra detallada en `docs/contratos_api.md`.

