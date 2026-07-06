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
├── CRM.WebFrontend/             # Servidor de Hosting Blazor (Supervisor Dashboard)
│   └── Components/Pages/        # Dashboard del Supervisor y Vista de Login Premium
└── CRM.WebFrontend.Client/      # Cliente Blazor WebAssembly (Asesor Dashboard)
    └── Pages/                   # Vistas interactivas de cliente de baja latencia
```

---

## 🔒 Seguridad y Endurecimiento

La plataforma implementa las siguientes medidas de seguridad para entornos productivos:

*   **Autenticación Seguro por Cookies (HttpOnly)**: Los tokens JWT de sesión no se almacenan en el navegador (`localStorage`), mitigando ataques XSS. El servidor de Blazor actúa como proxy seguro y escribe cookies cifradas con atributos `HttpOnly`, `Secure` y `SameSite=Lax`.
*   **Mitigación de Session Hijacking (Session Binding)**: El almacén de refresh tokens (`InMemoryRefreshTokenStore`) vincula y valida en cada petición de refresco la **Dirección IP del cliente** (con soporte para cabeceras de proxy `X-Forwarded-For`) y el **User-Agent**.
*   **Firmado Criptográfico Robusto**: Firma del token JWT respaldada por una clave secreta de **584 bits** configurada en variables de entorno o almacén seguro.
*   **Revocación de Sesiones e Inactividad**: Endpoint `/api/auth/logout` para invalidación inmediata de tokens en memoria y cookies configuradas con `SlidingExpiration` de **20 minutos**.

---

## 🚀 Instalación y Configuración

### 📋 Prerrequisitos
*   SDK de .NET 10.0
*   Base de datos PostgreSQL (con esquemas de negocio y administración de acceso configurados)

### ⚙️ Configuración inicial
1.  En `CRM_API/CRM.ApiHub/appsettings.json`, asegúrate de tener configurada la cadena de conexión cifrada mediante AES-256 en la propiedad `ConnectionStrings:DefaultConnection`.
2.  Configura el JWT SecretKey y la configuración de expiración de sesión.

### 🛠️ Compilación y Ejecución

Compilar toda la solución:
```bash
dotnet build CRM.sln
```

Ejecutar el Backend (ApiHub):
```bash
dotnet run --project CRM.ApiHub/CRM.ApiHub.csproj --launch-profile "http"
```
*   **API Hub**: [http://localhost:5068](http://localhost:5068)
*   **Documentación Swagger**: [http://localhost:5068/swagger](http://localhost:5068/swagger)

Ejecutar el Frontend (Blazor Web):
```bash
dotnet run --project CRM.WebFrontend/CRM.WebFrontend.csproj --launch-profile "http"
```
*   **Frontend Web**: [http://localhost:5261](http://localhost:5261)

---

## 🧪 Pruebas de Desarrollo y Entornos

### Credenciales de Bypass de Desarrollo (Developer Fallback)
En entornos locales (`ASPNETCORE_ENVIRONMENT = Development`), se cuenta con usuarios de prueba sin requerir conexión a base de datos externa:
*   **Supervisor**: `test.supervisor` / `password123`
*   **Asesor**: `test.asesor` / `password123`

### Colección de Postman y Contratos
*   La colección oficial de endpoints está exportada en `docs/CRM_CallCenter_Semana1.postman_collection.json`.
*   La especificación formal de todos los Request/Response del API se encuentra detallada en `docs/contratos_api.md`.
