# 📚 Documentación Técnica Oficial — Nyx CRM & Ecosystem

Bienvenido al centro de documentación técnica y operativa de la plataforma **Nyx CRM** (.NET 10 Web API + Blazor Server & WASM + Motores Autónomos).

---

## 🗂️ Índice Maestro de Documentos

| Documento | Descripción | Audiencia / Rol |
|---|---|:---:|
| 🌐 **[Plan de Motores Universales Reutilizables](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/PLAN_ARQUITECTURA_MOTORES_REUTILIZABLES.md)** | Estrategia Multi-Host para usar los motores en Nexus WPF, CRM Web, microservicios y sistemas externos. | Arquitectos, Tech Leads, Desarrolladores |
| 📊 **[Estado General del Proyecto](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/ESTADO_GENERAL_DEL_PROYECTO.md)** | Diagnóstico técnico 360°, auditoría de compilación, cobertura funcional y estado de subsistemas. | Directores, Tech Leads, DevOps |
| ⚡ **[Integración Total de Motores Nyx](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/INTEGRACION_TOTAL_DE_MOTORES_NYX.md)** | Análisis del problema de microservicios vs monolito modular, plan de unificación y transaccionalidad ACID. | Arquitectos, Tech Leads, Backend |
| 🏛️ **[Arquitectura de la API Central Robusta](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/ARQUITECTURA_API_CENTRAL_ROBUSTA.md)** | Diseño Clean Architecture, seguridad JWT/HMAC-512, Redis caching, MinIO S3 y resiliencia Polly. | Desarrolladores Backend, Seguridad |
| 📋 **[Matriz de Endpoints y Cobertura Total](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/MATRIZ_ENDPOINTS_Y_COBERTURA_TOTAL.md)** | Catálogo exhaustivo de los 24 controladores, 100+ endpoints, parámetros, request/response y RBAC. | Frontend Devs, QA, Integradores |
| 📑 **[Contratos de API](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/contratos_api.md)** | Especificaciones de contratos JSON para endpoints clave. | Integradores, Frontend |
| 🛡️ **[Hardening y Seguridad](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/hardening_seguridad_api_dayan.md)** | Directivas de seguridad criptográfica, sesión y auditoría. | Seguridad, SysAdmin |
| 🐳 **[Guía de Despliegue en Servidor](file:///c:/Users/dev2099/Documents/project_nyx/CRM_API/docs/Deploy.md)** | Manual de instalación Docker Compose, variables de entorno y Nginx. | DevOps, SysAdmin |

---

## 🛠️ Tecnologías y Dependencias del Ecosistema

- **Backend Principal**: .NET 10.0 C# 14 / ASP.NET Core Web API / Dapper Micro-ORM
- **Frontend Web**: Blazor InteractiveServer (Supervisores) + Blazor WebAssembly (Asesores)
- **Persistencia**: PostgreSQL 16 Multi-Database / Multi-Schema (`crm`, `flow`, `approval`, `sla`)
- **Caché y Mensajería**: Redis 7 Alpine (Sesiones, Refresh Tokens con Session Binding, SignalR Backplane)
- **Almacenamiento de Archivos**: MinIO Object Storage (Compatible AWS S3 API)
- **Observabilidad**: Serilog (JSON Structured Logging) + OpenTelemetry (Métricas y Trazas)
- **Resiliencia**: Microsoft.Extensions.Http.Resilience / Polly v8 (Circuit Breakers & Exponential Retries)

---

> 📄 **Documentación Mantenida por**: Antigravity AI / Tech Lead Team  
> 🕒 **Última Actualización**: 2026-08-20  
