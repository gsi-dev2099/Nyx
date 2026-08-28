# 📋 Matriz Integral de Endpoints y Cobertura Total de la API Central

> **Catálogo Oficial de Contratos, Endpoints, Parámetros y Roles de Seguridad**  
> **API Central**: `CRM.ApiHub` (.NET 10 REST API) | **Base URL**: `http://localhost:5068`  
> **Formato de Carga Útil**: `application/json` (UTF-8) | **Autenticación**: `Authorization: Bearer <JWT_TOKEN>`

---

## 📑 Índice de Módulos (24 Controladores REST + 1 Hub SignalR)

1. [Autenticación y Sesiones (`/api/auth`)](#1-autenticación-y-sesiones)
2. [Gestión de Leads (`/api/leads`)](#2-gestión-de-leads)
3. [Preventa y Contactabilidad (`/api/leads/{id}/presale`)](#3-preventa-y-contactabilidad)
4. [Órdenes de Venta (`/api/orders`)](#4-órdenes-de-venta)
5. [Gestión Documental y Grabaciones (`/api/documents`)](#5-gestión-documental-y-grabaciones)
6. [Supervisión de Equipos (`/api/supervisor`)](#6-supervisión-de-equipos)
7. [Mesa de Control y Backoffice (`/api/backoffice`)](#7-mesa-de-control-y-backoffice)
8. [Auditoría de Calidad y Scoring (`/api/audit`)](#8-auditoría-de-calidad-y-scoring)
9. [Gestión de Incidencias (`/api/incidents`)](#9-gestión-de-incidencias)
10. [Formularios Dinámicos por Etapa (`/api/forms`)](#10-formularios-dinámicos-por-etapa)
11. [Aprobaciones y Segregación de Deberes (`/api/approvals`)](#11-aprobaciones-y-segregación-de-deberes)
12. [Base de Conocimiento y Sugerencias (`/api/kb`)](#12-base-de-conocimiento-y-sugerencias)
13. [Comisiones, Divisas y Liquidaciones (`/api/commissions`, `/api/currencies`)](#13-comisiones-divisas-y-liquidaciones)
14. [Proveedores Satélites e Integraciones (`/api/providers`)](#14-proveedores-satélites-e-integraciones)
15. [Activaciones y Aprovisionamiento (`/api/activations`)](#15-activaciones-y-aprovisionamiento)
16. [Reportes y Analítica (`/api/reports`)](#16-reportes-y-analítica)
17. [Campañas y Catálogos Maestros (`/api/campaigns`, `/api/catalogs`)](#17-campañas-y-catálogos-maestros)
18. [Perfiles Alternos y Reemplazos (`/api/alternate-profiles`)](#18-perfiles-alternos-y-reemplazos)
19. [Carteras y Portafolios (`/api/portfolios`)](#19-carteras-y-portafolios)
20. [Notificaciones Internas (`/api/notifications`)](#20-notificaciones-internas)
21. [Diagnóstico y Salud (`/health`, `/api/health`)](#21-diagnóstico-y-salud)
22. [Administración y Mantenimiento del Sistema (`/api/maintenance`)](#22-administración-y-mantenimiento-del-sistema)
23. [Gestión de Motores Autónomos (`/api/engines`)](#23-gestión-de-motores-autónomos)
24. [Canal en Tiempo Real SignalR (`/notificationHub`)](#24-canal-en-tiempo-real-signalr)

---

## 1. 🔒 Autenticación y Sesiones

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa (200/201) |
|:---:|---|:---:|---|---|---|
| `POST` | `/api/auth/login` | Público | Autentica usuario y genera JWT + Refresh Token. | `{ "username": "...", "password": "..." }` | `{ "token": "...", "refreshToken": "...", "expiration": "..." }` |
| `GET` | `/api/auth/me` | Autenticado | Devuelve perfil, rol y campaña del usuario. | *Ninguno (Bearer Header)* | `{ "id": 10, "username": "...", "role": "ASESOR", ... }` |
| `POST` | `/api/auth/refresh-token` | Público | Refresca JWT con session binding (IP + User-Agent). | `{ "accessToken": "...", "refreshToken": "..." }` | `{ "token": "...", "refreshToken": "...", "expiration": "..." }` |
| `POST` | `/api/auth/logout` | Autenticado | Invalida y elimina el Refresh Token de Redis. | `{ "token": "..." }` | `{ "message": "Sesión revocada exitosamente." }` |

---

## 2. 📋 Gestión de Leads

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/leads` | Autenticado | Lista leads paginados y filtrables. | `?page=1&pageSize=20&campaignId=1&search=...` | `{ "data": [...], "pagination": { "page": 1, ... } }` |
| `POST` | `/api/leads` | Autenticado | Crea un nuevo prospecto o lead. | `{ "firstName": "...", "lastName": "...", "phoneNumber": "...", "campaignId": 1 }` | `{ "id": 42, "message": "Lead creado exitosamente." }` |
| `GET` | `/api/leads/{id}` | Autenticado | Obtiene datos detallados de un lead. | `id` en URL | `{ "id": 42, "firstName": "...", ... }` |
| `PATCH` | `/api/leads/{id}/status` | Autenticado | Actualiza el estado de gestión de un lead. | `{ "statusId": 2, "notes": "..." }` | `{ "success": true }` |

---

## 3. 📞 Preventa y Contactabilidad

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `POST` | `/api/leads/{leadId}/presale` | Autenticado | Registra una gestión de preventa/llamada. | `{ "assignedUserId": 10, "notes": "...", "scheduledContactDate": "..." }` | `{ "id": 5, "message": "Pre-venta registrada." }` |
| `GET` | `/api/leads/{leadId}/presale` | Autenticado | Historial completo de preventas de un lead. | `leadId` en URL | `{ "data": [ { "id": 5, "notes": "...", ... } ] }` |

---

## 4. 🛒 Órdenes de Venta

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/orders` | Autenticado | Consulta órdenes con filtros avanzados de fecha, campaña y estado. | `?userId=10&statusId=2&campaignId=1&dateFrom=...&page=1` | `{ "data": [...], "totalCount": 150, "page": 1 }` |
| `GET` | `/api/orders/{id}` | Autenticado | Detalle completo de orden, productos y cliente. | `id` en URL | `{ "idOrder": 101, "leadName": "...", "totalValue": 250.00, ... }` |
| `GET` | `/api/orders/{id}/history` | Autenticado | Auditoría histórica de cambios de estado y comentarios. | `id` en URL | `[ { "idHistory": 1, "idStatus": 2, "comment": "...", "changedAt": "..." } ]` |
| `POST` | `/api/orders` | ASESOR, BACKOFFICE | Crea una orden de venta e inicia el flujo y SLA automáticamente. | `{ "idLead": 42, "idCmpg": 1, "totalValue": 100, ... }` | `{ "idOrder": 101, "status": "BORRADOR", ... }` |
| `PATCH` | `/api/orders/{id}/status` | Autenticado | Transición de estado con validación de custodia y checkpoints de FlowEngine. | `{ "toStatusId": 2, "toSubstatusId": 4, "comment": "..." }` | `{ "message": "Estado de orden actualizado correctamente." }` |

---

## 5. 📁 Gestión Documental y Grabaciones (MinIO S3)

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `POST` | `/api/documents/upload` | Autenticado | Sube contrato, audio o documento a MinIO S3 y registra metadatos. | `Multipart/Form-Data` (`file`, `idOrder`, `docType`) | `{ "idDocument": 88, "fileUrl": "...", "message": "Archivo subido." }` |
| `GET` | `/api/documents/order/{idOrder}` | Autenticado | Lista documentos con URLs de descarga seguras pre-firmadas. | `idOrder` en URL | `[ { "idDocument": 88, "fileName": "...", "downloadUrl": "..." } ]` |
| `PATCH` | `/api/documents/{id}/verify` | BACKOFFICE, SUPERVISOR | Valida o rechaza un documento adjunto. | `{ "status": "VERIFICADO", "notes": "Firma conforme" }` | `{ "success": true }` |

---

## 6. 👥 Supervisión de Equipos

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/supervisor/team-orders` | SUPERVISOR, COORDINADOR | Monitoreo en vivo de órdenes del equipo. | `?campaignId=1&statusId=2&page=1` | `{ "orders": [...], "total": 45 }` |
| `GET` | `/api/supervisor/team-stats` | SUPERVISOR, COORDINADOR | KPIs en tiempo real (Ventas del día, tasa de conversión, KOs). | `?campaignId=1` | `{ "totalSales": 120, "conversionRate": 0.18, ... }` |
| `POST` | `/api/supervisor/bulk-transfer` | SUPERVISOR | Transferencia masiva de órdenes a Backoffice. | `{ "orderIds": [101, 102], "targetUserId": 237 }` | `{ "transferredCount": 2, "message": "Órdenes transferidas." }` |

---

## 7. ⚙️ Mesa de Control y Backoffice

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/backoffice/assigned` | BACKOFFICE | Lista órdenes asignadas en custodia al analista BAC. | `?statusId=3&page=1` | `{ "orders": [...] }` |
| `GET` | `/api/backoffice/pending-verification`| BACKOFFICE | Órdenes en cola pendientes de validación documental. | `?campaignId=1` | `{ "pendingCount": 15, "orders": [...] }` |
| `PATCH` | `/api/backoffice/orders/{id}/status` | BACKOFFICE | Actualización técnica de estado tras validación con operadora. | `{ "toStatusId": 5, "comment": "Validado con Vodafone" }` | `{ "success": true }` |

---

## 8. 🎧 Auditoría de Calidad y Scoring

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/audit/checklists` | CALIDAD, SUPERVISOR | Catálogo de checklists de evaluación por campaña. | `?idCmpg=1` | `[ { "idChecklist": 1, "name": "Matriz Calidad Telecom", ... } ]` |
| `POST` | `/api/audit/evaluations` | CALIDAD | Inicia una evaluación de audio de una orden. | `{ "idOrder": 101, "idChecklist": 1, "audioDuration": 320 }` | `{ "idEvaluation": 12, "status": "IN_PROGRESS" }` |
| `POST` | `/api/audit/evaluations/{id}/items` | CALIDAD | Guarda puntuación de un ítem de la pauta. | `{ "idItem": 4, "score": 10, "isFatal": false, "notes": "..." }` | `{ "saved": true }` |
| `POST` | `/api/audit/evaluations/{id}/close` | CALIDAD | Cierra auditoría y calcula nota final y veredicto. | `{ "feedback": "Excelente modulación" }` | `{ "finalScore": 95.5, "result": "APROBADO" }` |

---

## 9. 🚨 Gestión de Incidencias

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/incidents/catalog` | Autenticado | Catálogo de tipos de incidencias por campaña y estado. | `?idCmpg=1&idStatus=2` | `[ { "idIncident": 3, "code": "INC_DOC_ILEGIBLE", ... } ]` |
| `GET` | `/api/incidents/order/{idOrder}` | Autenticado | Lista incidencias abiertas/cerradas de una orden. | `idOrder` en URL | `[ { "idOrderIncident": 1, "customName": "...", "status": "ABIERTA" } ]` |
| `POST` | `/api/incidents` | Autenticado | Registra una incidencia, activa SLA y notifica al asesor. | `{ "idOrder": 101, "idIncident": 3, "customName": "...", ... }` | `{ "idOrderIncident": 15, "kbSuggestions": [...] }` |
| `POST` | `/api/incidents/{id}/responses` | Autenticado | Agrega respuesta/comentario al hilo de la incidencia. | `{ "responseText": "...", "responseType": "ASESOR" }` | `{ "message": "Respuesta registrada." }` |
| `PATCH` | `/api/incidents/{id}/resolve` | Autenticado | Resuelve la incidencia y detiene el reloj SLA. | `{ "notes": "Documento corregido y re-subido." }` | `{ "message": "Incidencia resuelta correctamente." }` |

---

## 10. 📝 Formularios Dinámicos por Etapa

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/forms/campaign/{idCmpg}/stage/{idStage}` | Autenticado | Obtiene plantilla de formulario configurada para la etapa. | Parámetros en URL | `[ { "idForm": 1, "formName": "Datos de Instalación Fibra", ... } ]` |
| `GET` | `/api/forms/{idForm}/fields` | Autenticado | Obtiene campos, validaciones y tipos de entrada. | `idForm` en URL | `[ { "idField": 10, "label": "Número de Serie Router", "fieldType": "TEXT", ... } ]` |
| `GET` | `/api/forms/order/{idOrder}/data` | Autenticado | Obtiene valores guardados en los formularios de la orden. | `idOrder` en URL | `[ { "idData": 105, "fieldName": "router_sn", "fieldValue": "VF88219" } ]` |
| `POST` | `/api/forms/order/{idOrder}/template/{idForm}` | Autenticado | Guarda valores con validación de custodia y permisos. | `[ { "idField": 10, "fieldValue": "VF88219" } ]` | `{ "message": "Datos del formulario guardados exitosamente." }` |

---

## 11. 🛡️ Aprobaciones y Segregación de Deberes

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `POST` | `/api/orders/{id}/approvals` | ASESOR, BACKOFFICE | Solicita aprobación jerárquica (ej. Descuento Alto o Excepción). | `{ "comments": "Descuento 20% autorizado por cliente vip" }` | `{ "id": 8, "message": "Solicitud registrada como PENDING." }` |
| `PATCH` | `/api/approvals/{id}` | SUPERVISOR | Aprueba o rechaza con validación SOX de no auto-aprobación. | `{ "status": "APPROVED", "comments": "Conforme" }` | `{ "message": "Aprobación actualizada correctamente." }` |
| `GET` | `/api/approvals/{id}` | Autenticado | Consulta estado y decisión de una solicitud. | `id` en URL | `{ "idApproval": 8, "status": "APPROVED", ... }` |

---

## 12. 📚 Base de Conocimiento y Sugerencias (KB)

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/kb/articles` | Autenticado | Búsqueda de artículos de soporte por texto y campaña. | `?query=fibra&campaignId=1` | `[ { "idArticle": 5, "title": "Guía de Portabilidad Fibra Vodafone", ... } ]` |
| `GET` | `/api/kb/articles/{id}` | Autenticado | Lectura completa de artículo con contenido enriquecido. | `id` en URL | `{ "idArticle": 5, "contentHtml": "...", "helpfulCount": 42 }` |
| `POST` | `/api/kb/articles/{id}/feedback` | Autenticado | Voto de utilidad (Útil / No Útil) por asesores. | `{ "isHelpful": true, "comment": "Muy claro" }` | `{ "success": true }` |

---

## 13. 💰 Comisiones, Divisas y Liquidaciones

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/currencies` | Autenticado | Catálogo de monedas soportadas (PEN, EUR, USD). | *Ninguno* | `[ { "code": "EUR", "symbol": "€", "name": "Euro" } ]` |
| `GET` | `/api/currencies/convert` | Autenticado | Conversión de importes según tasa de cambio activa. | `?from=EUR&to=PEN&amount=100` | `{ "convertedAmount": 412.50, "rate": 4.125 }` |
| `GET` | `/api/commissions/settlements` | SUPERVISOR, BACKOFFICE | Lotes de liquidaciones generados. | `?userId=10&period=2026-07` | `[ { "idSettlement": 1, "totalAmount": 1500.00, ... } ]` |
| `POST` | `/api/commissions/settlements` | SUPERVISOR | Genera nuevo lote de liquidación de comisiones. | `{ "idUser": 10, "period": "2026-07", "items": [...] }` | `{ "idSettlement": 2, "status": "BORRADOR" }` |

---

## 14. 🔌 Proveedores Satélites e Integraciones

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/providers` | ADMIN_CRM, BACKOFFICE | Lista de proveedores satélites (Vodafone, Securitas, etc.). | *Ninguno* | `[ { "idProvider": 1, "code": "VODAFONE", ... } ]` |
| `GET` | `/api/providers/{id}/mappings` | ADMIN_CRM, BACKOFFICE | Mapeo de estados externos a estados internos del CRM. | `id` en URL | `[ { "providerCode": "INST_OK", "internalStatusId": 9 } ]` |
| `POST` | `/api/providers/sync-log` | Webhooks / Sistema | Registra evento de sincronización o payload de proveedor. | `{ "idProvider": 1, "payload": "...", "status": "OK" }` | `{ "success": true }` |

---

## 15. 🚀 Activaciones y Aprovisionamiento

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/activations/pending` | BACKOFFICE, COORDINADOR | Cola de servicios pendientes de confirmación de activación. | `?providerId=1&page=1` | `{ "data": [...], "count": 28 }` |
| `GET` | `/api/activations/delayed` | BACKOFFICE, COORDINADOR | Servicios con retraso de activación según SLA de proveedor. | `?thresholdDays=5` | `[ { "idOrder": 101, "daysPending": 7, ... } ]` |
| `PATCH` | `/api/activations/{id}` | BACKOFFICE | Confirma activación formal con ID de contrato del proveedor. | `{ "status": "ACTIVADO", "providerContractId": "VF-9981" }` | `{ "success": true }` |

---

## 16. 📊 Reportes y Analítica

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/reports/funnel` | SUPERVISOR, COORDINADOR | Embudo de conversión por etapas y campañas. | `?idCmpg=1&dateFrom=...&dateTo=...` | `{ "stages": [ { "stage": "Preventa", "count": 500 }, ... ] }` |
| `GET` | `/api/reports/sales-by-asesor` | SUPERVISOR, COORDINADOR | Ranking de ventas, montos y efectividad por asesor. | `?dateFrom=...&dateTo=...` | `[ { "asesorId": 10, "name": "...", "sales": 45 } ]` |
| `GET` | `/api/reports/incident-stats` | SUPERVISOR, COORDINADOR | Distribución de incidencias por tipología y tiempos de resolución. | `?dateFrom=...` | `{ "total": 35, "avgResolutionHours": 4.2 }` |

---

## 17. 🗂️ Campañas y Catálogos Maestros

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/campaigns` | Autenticado | Catálogo de campañas activas del call center. | *Ninguno* | `[ { "idCmpg": 1, "name": "Vodafone Portabilidad" } ]` |
| `GET` | `/api/catalogs/statuses` | Autenticado | Catálogo maestro de estados de orden. | *Ninguno* | `[ { "id": 1, "name": "Pendiente", "color": "#ffaa00" } ]` |
| `GET` | `/api/catalogs/statuses/{id}/substatuses` | Autenticado | Subestados dependientes de un estado de orden. | `id` en URL | `[ { "id": 4, "name": "Pago Verificado" } ]` |
| `GET` | `/api/catalogs/campaigns/{id}/products` | Autenticado | Productos y planes comerciales de una campaña. | `id` en URL | `[ { "sku": "VF-FIBRA-600", "price": 35.00 } ]` |

---

## 18. 🔄 Perfiles Alternos y Reemplazos

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/alternate-profiles` | Autenticado | Lista perfiles alternos configurados para el usuario actual. | *Ninguno* | `[ { "idProfile": 2, "role": "SUPERVISOR_GUARDIA" } ]` |
| `POST` | `/api/alternate-profiles` | SUPERVISOR, ADMIN_CRM | Configura asignación temporal de rol por suplencia/vacaciones. | `{ "userId": 10, "alternateRoleId": 2, "validUntil": "..." }` | `{ "success": true }` |

---

## 19. 💼 Carteras y Portafolios

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/portfolios` | Autenticado | Lista carteras comerciales de clientes asignadas. | *Ninguno* | `[ { "idPortfolio": 1, "code": "RESIDENCIAL", "name": "B2C Residencial" } ]` |

---

## 20. 🔔 Notificaciones Internas

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/api/notifications` | Autenticado | Lista notificaciones del usuario logueado. | `?unreadOnly=true&limit=20` | `[ { "idNotification": 101, "title": "Orden Aprobada", "isRead": false } ]` |
| `PATCH` | `/api/notifications/{id}/read` | Autenticado | Marca una notificación específica como leída. | `id` en URL | `{ "success": true }` |
| `GET` | `/api/notifications/unread-count` | Autenticado | Contador de notificaciones pendientes para el badge de campana. | *Ninguno* | `{ "unreadCount": 3 }` |

---

## 21. 🩺 Diagnóstico y Salud

| Método | Endpoint | Roles Permitidos | Descripción | Request Body / Query | Respuesta Exitosa |
|:---:|---|:---:|---|---|---|
| `GET` | `/health` | Público | Liveness probe para balanceadores y Docker. | *Ninguno* | `Healthy` |
| `GET` | `/api/health` | Público | Readiness probe con verificación de DB PostgreSQL y Redis. | *Ninguno* | `{ "status": "Healthy", "postgres": "UP", "redis": "UP" }` |

---

## 22. 🛠️ Administración y Mantenimiento del Sistema (`/api/maintenance`)

> **Acceso Restringido**: Exclusivo para usuarios con roles `ADMIN_CRM` o `BACKOFFICE`.

| Método | Endpoint | Descripción | Entidad Administrada |
|:---:|---|---|---|
| `GET / POST` | `/api/maintenance/statuses` | Listado y creación de estados maestros de orden. | Estados de Venta |
| `PUT / PATCH` | `/api/maintenance/statuses/{id}` | Modificación y activación/desactivación de estados. | Estados de Venta |
| `GET / POST` | `/api/maintenance/substatuses` | Listado y creación de subestados asociados a estados. | Subestados |
| `PUT / PATCH` | `/api/maintenance/substatuses/{id}` | Modificación y cambio de orden de subestados. | Subestados |
| `GET / POST` | `/api/maintenance/transitions` | Matriz de transiciones permitidas y roles autorizados. | Transiciones de Estado |
| `PUT / DELETE` | `/api/maintenance/transitions/{id}` | Edición de reglas de transición (requiere comentario/formulario). | Transiciones de Estado |
| `GET / POST` | `/api/maintenance/stages` | Etapas del ciclo de vida del pipeline comercial. | Etapas de Pipeline |
| `PUT / PATCH` | `/api/maintenance/stages/{id}` | Edición de SLAs de etapa y disparadores de entrada/salida. | Etapas de Pipeline |
| `GET / POST` | `/api/maintenance/providers` | Catálogo de proveedores e integraciones API. | Proveedores |
| `GET / POST` | `/api/maintenance/provider-mappings` | Mapeo de estados externos de operadoras a internos. | Mapeos de Proveedor |
| `GET / POST` | `/api/maintenance/roles` | Catálogo de roles de seguridad del sistema. | Roles |
| `GET / POST` | `/api/maintenance/permissions` | Matriz granular de permisos por rol y estado de orden. | Permisos RBAC |
| `GET / POST` | `/api/maintenance/shifts` | Horarios laborales y turnos de agentes para cálculo SLA. | Turnos Laborales |
| `GET / POST` | `/api/maintenance/exchange-rates` | Tasas de cambio históricas y vigentes (EUR/PEN/USD). | Tipos de Cambio |
| `GET / POST` | `/api/maintenance/incident-catalog` | Tipologías de incidencia, SLAs asociados y pautas. | Catálogo Incidencias |
| `GET / POST` | `/api/maintenance/quality-checklists`| Plantillas de checklists de calidad y puntaje meta. | Pautas de Calidad |

---

## 23. ⚡ Gestión de Motores Autónomos (`/api/engines`)

| Método | Endpoint | Roles Permitidos | Descripción |
|:---:|---|:---:|---|
| `GET` | `/api/engines/status` | Autenticado | Diagnóstico de salud de `Nyx.SlaEngine`, `Nyx.ApprovalEngine` y `Nyx.FlowEngine`. |
| `GET` | `/api/engines/flow/stages` | Autenticado | Lista etapas dinámicas registradas en el catálogo de flujos. |
| `POST` | `/api/engines/flow/stages` | SUPERVISOR, BACKOFFICE, ADMIN | Creación de nuevas etapas en el motor de flujos. |
| `PATCH` | `/api/engines/flow/stages/{id}/move` | SUPERVISOR, BACKOFFICE, ADMIN | Reordena posición de etapa (dirección `up` o `down`). |
| `PATCH` | `/api/engines/flow/stages/{id}/order` | SUPERVISOR, BACKOFFICE, ADMIN | Fija índice ordinal exacto de una etapa. |
| `GET` | `/api/engines/flow/catalogs` | Autenticado | Catálogo de checkpoints por flujo. |
| `POST` | `/api/engines/flow/catalogs` | SUPERVISOR, BACKOFFICE, ADMIN | Crea un nuevo checkpoint bloqueante o informativo. |
| `GET` | `/api/engines/flow/catalogs/full` | Autenticado | Catálogo completo de checkpoints con sus pasos secuenciales. |
| `GET` | `/api/engines/flow/catalogs/{id}/steps` | Autenticado | Pasos secuenciales de un checkpoint específico. |
| `POST` | `/api/engines/flow/catalogs/{id}/steps` | SUPERVISOR, BACKOFFICE, ADMIN | Guarda lista de pasos con instrucciones y obligatoriedad. |
| `GET` | `/api/engines/flow/instances/{id}` | Autenticado | Consulta estado de instancia activa de flujo por ID de instancia. |
| `GET` | `/api/engines/flow/instances/by-entity/{entityType}/{entityId}` | Autenticado | Consulta instancia de flujo y estado de checkpoints por Orden de Venta. |
| `POST` | `/api/engines/flow/instances/{cpInstanceId}/resolve` | ASESOR, SUPERVISOR, BAC | Resuelve o subsana un checkpoint en la línea de tiempo de la orden. |
| `POST` | `/api/engines/flow/checkpoints/instances/{cpInstanceId}/steps/{stepId}/toggle` | ASESOR, SUPERVISOR, BAC | Marca o desmarca un paso secuencial (checkbox interactivo). |
| `GET` | `/api/engines/flow/checkpoints/instances/{cpInstanceId}/steps` | Autenticado | Consulta progreso de pasos completados en un checkpoint de orden. |
| `POST` | `/api/engines/flow/instances/{id}/advance` | SUPERVISOR, BACKOFFICE | Avanza manualmente la instancia de flujo a la siguiente etapa. |
| `GET` | `/api/engines/approval/pending` | SUPERVISOR, COORDINADOR | Bandeja de solicitudes de aprobación pendientes asignadas al rol. |

---

## 24. 📡 Canal en Tiempo Real SignalR (`/notificationHub`)

- **Protocolo**: WebSockets con fallback a Server-Sent Events / Long Polling.
- **Eventos Emitidos desde el Servidor**:
  - `ReceiveNotification`: Notificación instantánea al asesor o supervisor ante cambios en órdenes, incidencias o aprobaciones.
  - `OrderStatusChanged`: Alerta en vivo en el Dashboard de supervisores ante nuevas ventas o transferencias a Backoffice.
  - `IncidentAlert`: Notificación de nueva incidencia crítica con timer SLA activo.

---

> 📄 **Documentación Generada por**: Tech Lead Agent / Antigravity AI  
> 🏷️ **Versión de la Matriz**: v2.1.0-COMPLETE-CATALOG  
