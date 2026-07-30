# 📚 Documentación Técnica Integral de API (CRM API Hub)

Esta es la documentación técnica y oficial de **TODOS los endpoints** del sistema Nyx CRM, orientada a Frontend y QA. Incluye contratos, estructuras y reglas de negocio.

## ⚙️ Variables de Entorno Requeridas

| Variable | Descripción | Ejemplo de Valor |
|----------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Conexión a PostgreSQL. | `Host=localhost;Database=nyx_crm;Username=postgres;Password=Secret123!` |
| `Jwt:Key` | Clave secreta para JWT. | `TuClaveSecretaSuperSeguraParaElCRM_2099!` |
| `Jwt:Issuer` | Emisor autorizado. | `CRMApiHub` |
| `Jwt:Audience` | Audiencia permitida. | `CRMWebFrontend` |
| `AllowedHosts` | Control de CORS. | `*` o `http://localhost:5261` |
| `NYX_DB_ENCRYPTION_KEY` | Encriptación de datos sensibles. | `KeyDe32BytesParaEncriptacionFuerte==` |

## 🚀 Uso de la Colección OpenAPI (Postman)

Para probar todos estos endpoints sin configurar los JSON a mano:
1. Ve a `http://localhost:5068/swagger/v1/swagger.json` y guarda el archivo.
2. Abre **Postman** -> **Import** -> Arrastra el archivo.
3. Postman generará **todas las peticiones y variables** automáticamente.

## 📋 Diccionario de Códigos HTTP Generales

- **200/201**: Éxito.
- **400 (Bad Request)**: Error de validación (DTO mal formado, JSON inválido).
- **401 (Unauthorized)**: Falta token JWT o expiró.
- **403 (Forbidden)**: Token válido, pero sin permisos (Rol incorrecto).
- **404 (Not Found)**: Recurso no existe.
- **422 (Unprocessable Entity)**: Regla de negocio rota (Ej: Cambiar estado no permitido).

--- 

## 🔗 Endpoints Detallados (Total 95)

### 📦 Módulo: Activation

#### `GET` /api/activations/pending
**Descripción de Negocio:** Operación GET para el recurso /api/activations/pending.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/orders/{id}/activations
**Descripción de Negocio:** Operación GET para el recurso /api/orders/{id}/activations.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/activations/delayed
**Descripción de Negocio:** Operación GET para el recurso /api/activations/delayed.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/activations/{idItem}
**Descripción de Negocio:** Operación PATCH para el recurso /api/activations/{idItem}.

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value",
  "actualDate": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: AlternateProfile

#### `POST` /api/orders/{id}/alternate-profile
**Descripción de Negocio:** Operación POST para el recurso /api/orders/{id}/alternate-profile.

**Ejemplo Request (JSON):**
```json
{
  "alternateType": "string_value",
  "alternateData": "string_value",
  "originalData": "string_value",
  "reason": "string_value",
  "createdBy": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/orders/{id}/alternate-profile
**Descripción de Negocio:** Operación GET para el recurso /api/orders/{id}/alternate-profile.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Approval

#### `POST` /api/orders/{id}/approvals
**Descripción de Negocio:** Operación POST para el recurso /api/orders/{id}/approvals.

**Ejemplo Request (JSON):**
```json
{
  "authorizedBy": 0,
  "comments": "string_value",
  "isApproved": true
}
```

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/approvals/{id}
**Descripción de Negocio:** Operación PATCH para el recurso /api/approvals/{id}.

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value",
  "comments": "string_value",
  "authorizedBy": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/approvals/{id}
**Descripción de Negocio:** Operación GET para el recurso /api/approvals/{id}.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Audit

#### `GET` /api/audit/checklist/{idCmpg}
**Descripción de Negocio:** Operación GET para el recurso /api/audit/checklist/{idCmpg}.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/orders/{id}/audit
**Descripción de Negocio:** Operación POST para el recurso /api/orders/{id}/audit.

**Ejemplo Request (JSON):**
```json
{
  "idChecklist": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/audit/{id}/items
**Descripción de Negocio:** Operación POST para el recurso /api/audit/{id}/items.

**Ejemplo Request (JSON):**
```json
{
  "idItem": 0,
  "result": "string_value",
  "observation": "string_value",
  "audioTimestamp": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/audit/{id}/close
**Descripción de Negocio:** Operación PATCH para el recurso /api/audit/{id}/close.

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Auth

#### `POST` /api/Auth/login
**Descripción de Negocio:** Valida credenciales y devuelve JWT con perfil de usuario.

**Ejemplo Request (JSON):**
```json
{
  "username": "string_value",
  "password": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/Auth/me
**Descripción de Negocio:** Obtiene la información del usuario autenticado actual.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/Auth/refresh-token
**Descripción de Negocio:** Operación POST para el recurso /api/Auth/refresh-token.

**Ejemplo Request (JSON):**
```json
{
  "refreshToken": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/Auth/logout
**Descripción de Negocio:** Operación POST para el recurso /api/Auth/logout.

**Ejemplo Request (JSON):**
```json
{
  "refreshToken": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/Auth/check-permission
**Descripción de Negocio:** Operación GET para el recurso /api/Auth/check-permission.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Backoffice

#### `GET` /api/backoffice/orders
**Descripción de Negocio:** Bandeja de entrada del Analista BackOffice. Filtra por custodia.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/backoffice/pending-docs
**Descripción de Negocio:** Operación GET para el recurso /api/backoffice/pending-docs.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/backoffice/orders/{id}/status
**Descripción de Negocio:** Operación PATCH para el recurso /api/backoffice/orders/{id}/status.

**Ejemplo Request (JSON):**
```json
{
  "toStatusId": 0,
  "toSubstatusId": 0,
  "comment": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/backoffice/documents/{id}/verify
**Descripción de Negocio:** Operación PATCH para el recurso /api/backoffice/documents/{id}/verify.

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value",
  "notes": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Campaign

#### `GET` /api/campaigns
**Descripción de Negocio:** Operación GET para el recurso /api/campaigns.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/campaigns/{id}/statuses
**Descripción de Negocio:** Operación GET para el recurso /api/campaigns/{id}/statuses.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/campaigns/orders
**Descripción de Negocio:** Operación GET para el recurso /api/campaigns/orders.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/campaigns/advisors
**Descripción de Negocio:** Operación GET para el recurso /api/campaigns/advisors.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Commission

#### `GET` /api/currencies
**Descripción de Negocio:** Operación GET para el recurso /api/currencies.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/currencies/convert
**Descripción de Negocio:** Operación GET para el recurso /api/currencies/convert.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/commissions/settlements
**Descripción de Negocio:** Operación GET para el recurso /api/commissions/settlements.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/commissions/settlements
**Descripción de Negocio:** Operación POST para el recurso /api/commissions/settlements.

**Ejemplo Request (JSON):**
```json
{
  "userId": 0,
  "periodStart": "string_value",
  "periodEnd": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/commissions/settlements/{id}
**Descripción de Negocio:** Operación GET para el recurso /api/commissions/settlements/{id}.

**Respuestas:**
- **200**: OK

---

#### `PUT` /api/commissions/settlements/{id}
**Descripción de Negocio:** Operación PUT para el recurso /api/commissions/settlements/{id}.

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value",
  "notes": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `DELETE` /api/commissions/settlements/{id}
**Descripción de Negocio:** Operación DELETE para el recurso /api/commissions/settlements/{id}.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/commissions/settlements/{id}/items
**Descripción de Negocio:** Operación POST para el recurso /api/commissions/settlements/{id}/items.

**Ejemplo Request (JSON):**
```json
{
  "orderIds": [
    "{}"
  ]
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Document

#### `GET` /api/orders/{id}/documents
**Descripción de Negocio:** Operación GET para el recurso /api/orders/{id}/documents.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/orders/{id}/documents
**Descripción de Negocio:** Operación POST para el recurso /api/orders/{id}/documents.

**Ejemplo Request (JSON):**
```json
"{}"
```

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/documents/{id}/verify
**Descripción de Negocio:** Aprueba o rechaza documentos adjuntos a una venta (DNI, Contrato).

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value",
  "notes": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/documents/{id}/download
**Descripción de Negocio:** Operación GET para el recurso /api/documents/{id}/download.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Form

#### `GET` /api/forms/campaign/{idCmpg}/stage/{idStage}
**Descripción de Negocio:** Operación GET para el recurso /api/forms/campaign/{idCmpg}/stage/{idStage}.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/forms/{idForm}/fields
**Descripción de Negocio:** Operación GET para el recurso /api/forms/{idForm}/fields.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/forms/order/{idOrder}/data
**Descripción de Negocio:** Operación GET para el recurso /api/forms/order/{idOrder}/data.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/forms/order/{idOrder}/template/{idForm}
**Descripción de Negocio:** Operación POST para el recurso /api/forms/order/{idOrder}/template/{idForm}.

**Ejemplo Request (JSON):**
```json
[
  {
    "idOrddata": "{}",
    "idOrder": "{}",
    "idFld": "{}",
    "valueText": "{}",
    "valueJson": "{}",
    "fieldStatus": "{}",
    "validatedBy": "{}",
    "validatedAt": "{}",
    "version": "{}",
    "sourceFormId": "{}",
    "register": "{}"
  }
]
```

**Respuestas:**
- **200**: OK

---

#### `PUT` /api/forms/data/{idData}/status
**Descripción de Negocio:** Operación PUT para el recurso /api/forms/data/{idData}/status.

**Ejemplo Request (JSON):**
```json
{
  "status": "string_value",
  "validatedBy": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/forms/seed
**Descripción de Negocio:** Operación POST para el recurso /api/forms/seed.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Incident

#### `GET` /api/incidents/catalog
**Descripción de Negocio:** Operación GET para el recurso /api/incidents/catalog.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/incidents/order/{idOrder}
**Descripción de Negocio:** Operación GET para el recurso /api/incidents/order/{idOrder}.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/incidents/{id}
**Descripción de Negocio:** Operación GET para el recurso /api/incidents/{id}.

**Respuestas:**
- **200**: OK

---

#### `PUT` /api/incidents/{id}
**Descripción de Negocio:** Operación PUT para el recurso /api/incidents/{id}.

**Ejemplo Request (JSON):**
```json
{
  "customName": "string_value",
  "customDescription": "string_value",
  "customSolution": "string_value",
  "assignedToRole": "string_value",
  "dueAt": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `DELETE` /api/incidents/{id}
**Descripción de Negocio:** Operación DELETE para el recurso /api/incidents/{id}.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/incidents
**Descripción de Negocio:** Gestión de incidencias y observaciones en ventas.

**Ejemplo Request (JSON):**
```json
{
  "idOrderIncident": 0,
  "idOrder": 0,
  "idIncident": 0,
  "customName": "string_value",
  "customDescription": "string_value",
  "customSolution": "string_value",
  "incidentStatus": "string_value",
  "detectedBy": 0,
  "assignedToRole": "string_value",
  "resolvedBy": 0,
  "resolvedAt": "string_value",
  "resolutionNotes": "string_value",
  "escalatedBy": 0,
  "escalatedAt": "string_value",
  "escalationReason": "string_value",
  "dueAt": "string_value",
  "register": "string_value",
  "priority": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/incidents
**Descripción de Negocio:** Gestión de incidencias y observaciones en ventas.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/incidents/{id}/responses
**Descripción de Negocio:** Operación POST para el recurso /api/incidents/{id}/responses.

**Ejemplo Request (JSON):**
```json
{
  "responseText": "string_value",
  "responseType": "string_value",
  "respondedBy": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/incidents/{id}/resolve
**Descripción de Negocio:** Operación PATCH para el recurso /api/incidents/{id}/resolve.

**Ejemplo Request (JSON):**
```json
{
  "notes": "string_value",
  "resolvedBy": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/incidents/{id}/kb-suggestions
**Descripción de Negocio:** Operación GET para el recurso /api/incidents/{id}/kb-suggestions.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: KB

#### `GET` /api/kb/search
**Descripción de Negocio:** Operación GET para el recurso /api/kb/search.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/kb/{id}
**Descripción de Negocio:** Operación GET para el recurso /api/kb/{id}.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/kb/{id}/feedback
**Descripción de Negocio:** Operación POST para el recurso /api/kb/{id}/feedback.

**Ejemplo Request (JSON):**
```json
{
  "isHelpful": true,
  "comment": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Lead

#### `GET` /api/leads
**Descripción de Negocio:** Operación GET para el recurso /api/leads.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/leads
**Descripción de Negocio:** Operación POST para el recurso /api/leads.

**Ejemplo Request (JSON):**
```json
{
  "firstName": "string_value",
  "lastName": "string_value",
  "email": "string_value",
  "phone": "string_value",
  "idCmpg": 0,
  "idSrc": 0,
  "documentNumber": "string_value",
  "rawData": "string_value",
  "assignedUserId": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/leads/{id}
**Descripción de Negocio:** Operación GET para el recurso /api/leads/{id}.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/leads/{id}/status
**Descripción de Negocio:** Operación PATCH para el recurso /api/leads/{id}/status.

**Ejemplo Request (JSON):**
```json
{
  "idStatus": 0,
  "comment": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Maintenance

#### `GET` /api/maintenance/statuses
**Descripción de Negocio:** Operación GET para el recurso /api/maintenance/statuses.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/maintenance/statuses/{id}/toggle
**Descripción de Negocio:** Operación PATCH para el recurso /api/maintenance/statuses/{id}/toggle.

**Ejemplo Request (JSON):**
```json
{
  "isActive": true
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/maintenance/testschema
**Descripción de Negocio:** Operación GET para el recurso /api/maintenance/testschema.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/maintenance/products
**Descripción de Negocio:** Operación GET para el recurso /api/maintenance/products.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/maintenance/products/{id}/toggle
**Descripción de Negocio:** Operación PATCH para el recurso /api/maintenance/products/{id}/toggle.

**Ejemplo Request (JSON):**
```json
{
  "isActive": true
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/maintenance/incidents
**Descripción de Negocio:** Operación GET para el recurso /api/maintenance/incidents.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/maintenance/incidents
**Descripción de Negocio:** Operación POST para el recurso /api/maintenance/incidents.

**Ejemplo Request (JSON):**
```json
{
  "idCmpg": 0,
  "idStatus": 0,
  "code": "string_value",
  "name": "string_value",
  "description": "string_value",
  "solutionTemplate": "string_value",
  "resolutionType": "string_value",
  "requiresResponse": true,
  "isRecurrent": true,
  "priority": 0,
  "slaHours": 0
}
```

**Respuestas:**
- **200**: OK

---

#### `PUT` /api/maintenance/incidents/{id}
**Descripción de Negocio:** Operación PUT para el recurso /api/maintenance/incidents/{id}.

**Ejemplo Request (JSON):**
```json
{
  "name": "string_value",
  "description": "string_value",
  "solutionTemplate": "string_value",
  "resolutionType": "string_value",
  "requiresResponse": true,
  "isRecurrent": true,
  "priority": 0,
  "slaHours": 0,
  "isActive": true
}
```

**Respuestas:**
- **200**: OK

---

#### `DELETE` /api/maintenance/incidents/{id}
**Descripción de Negocio:** Operación DELETE para el recurso /api/maintenance/incidents/{id}.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/maintenance/exchange-rates
**Descripción de Negocio:** Operación GET para el recurso /api/maintenance/exchange-rates.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/maintenance/exchange-rates
**Descripción de Negocio:** Operación POST para el recurso /api/maintenance/exchange-rates.

**Ejemplo Request (JSON):**
```json
{
  "fromCurrency": "string_value",
  "toCurrency": "string_value",
  "rate": "",
  "validFrom": "string_value",
  "validTo": "string_value",
  "source": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/maintenance/campaigns
**Descripción de Negocio:** Operación GET para el recurso /api/maintenance/campaigns.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Notification

#### `GET` /api/notifications
**Descripción de Negocio:** Operación GET para el recurso /api/notifications.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/notifications/{id}/read
**Descripción de Negocio:** Operación PATCH para el recurso /api/notifications/{id}/read.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/notifications/read-all
**Descripción de Negocio:** Operación POST para el recurso /api/notifications/read-all.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: PreSale

#### `GET` /api/presales
**Descripción de Negocio:** Operación GET para el recurso /api/presales.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/presales
**Descripción de Negocio:** Operación POST para el recurso /api/presales.

**Ejemplo Request (JSON):**
```json
{
  "idCmpg": 0,
  "phone": "string_value",
  "operator": "string_value",
  "firstName": "string_value",
  "lastName": "string_value",
  "address": "string_value",
  "province": "string_value",
  "coverageStatus": "string_value",
  "idStatus": 0,
  "ownerUserId": 0,
  "currentUserId": 0,
  "notes": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/presales/{id}/calls
**Descripción de Negocio:** Operación POST para el recurso /api/presales/{id}/calls.

**Ejemplo Request (JSON):**
```json
{
  "callLog": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/presales/{id}/assign
**Descripción de Negocio:** Operación POST para el recurso /api/presales/{id}/assign.

**Ejemplo Request (JSON):**
```json
{
  "toUserId": 0,
  "context": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/presales/{id}/convert
**Descripción de Negocio:** Operación POST para el recurso /api/presales/{id}/convert.

**Ejemplo Request (JSON):**
```json
{
  "userId": 0
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Provider

#### `GET` /api/providers
**Descripción de Negocio:** Operación GET para el recurso /api/providers.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/providers/{id}/status-mapping
**Descripción de Negocio:** Operación GET para el recurso /api/providers/{id}/status-mapping.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/providers/{id}/sync-log
**Descripción de Negocio:** Operación POST para el recurso /api/providers/{id}/sync-log.

**Ejemplo Request (JSON):**
```json
{
  "idOrder": 0,
  "statusCode": "string_value",
  "result": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

#### `POST` /api/providers/{id}/update-order-status
**Descripción de Negocio:** Operación POST para el recurso /api/providers/{id}/update-order-status.

**Ejemplo Request (JSON):**
```json
{
  "idOrder": 0,
  "providerStatusCode": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Report

#### `GET` /api/reports/funnel
**Descripción de Negocio:** Operación GET para el recurso /api/reports/funnel.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/reports/sales-by-asesor
**Descripción de Negocio:** Operación GET para el recurso /api/reports/sales-by-asesor.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/reports/incidents
**Descripción de Negocio:** Operación GET para el recurso /api/reports/incidents.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/reports/activations
**Descripción de Negocio:** Operación GET para el recurso /api/reports/activations.

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: SalesOrder

#### `GET` /api/orders
**Descripción de Negocio:** Crea o lista órdenes de venta.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/orders
**Descripción de Negocio:** Crea o lista órdenes de venta.

**Ejemplo Request (JSON):**
```json
{
  "idLead": 0,
  "idCmpg": 0,
  "idUser": 0,
  "ownerUserId": 0,
  "custodyUserId": 0,
  "idStatus": 0,
  "idSubstatus": 0,
  "currencyCode": "string_value",
  "commissionCurrency": "string_value",
  "status": "string_value",
  "salesDate": "string_value",
  "totalProducts": 0,
  "totalValue": "",
  "isAlternate": true
}
```

**Respuestas:**
- **200**: OK

---

#### `GET` /api/orders/{id}
**Descripción de Negocio:** Operación GET para el recurso /api/orders/{id}.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/orders/{id}/history
**Descripción de Negocio:** Operación GET para el recurso /api/orders/{id}/history.

**Respuestas:**
- **200**: OK

---

#### `PATCH` /api/orders/{id}/status
**Descripción de Negocio:** Actualiza el estado de la venta. Dispara transferencia de custodia a BAC si el estado es 3.

**Ejemplo Request (JSON):**
```json
{
  "toStatusId": 0,
  "toSubstatusId": 0,
  "comment": "string_value",
  "isBulk": true
}
```

**Respuestas:**
- **200**: OK

---

### 📦 Módulo: Supervisor

#### `GET` /api/supervisor/orders
**Descripción de Negocio:** Operación GET para el recurso /api/supervisor/orders.

**Respuestas:**
- **200**: OK

---

#### `GET` /api/supervisor/stats
**Descripción de Negocio:** Operación GET para el recurso /api/supervisor/stats.

**Respuestas:**
- **200**: OK

---

#### `POST` /api/supervisor/bulk-transfer
**Descripción de Negocio:** Transfiere múltiples ventas masivamente a un Analista BackOffice, asignando custodia.

**Ejemplo Request (JSON):**
```json
{
  "orderIds": [
    "{}"
  ],
  "backofficeUserId": 0,
  "comment": "string_value"
}
```

**Respuestas:**
- **200**: OK

---

