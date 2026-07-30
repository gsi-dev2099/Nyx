# 📚 Documentación Técnica de API (CRM API Hub)
## ⚙️ Variables de Entorno Requeridas
Para ejecutar este proyecto correctamente, se requiere configurar las siguientes variables de entorno (en `appsettings.json` o `.env`):
| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión a PostgreSQL | `Host=localhost;Database=crm_db;Username=postgres;Password=pass` |
| `Jwt:Key` | Clave secreta para firmar los tokens JWT | `SuperSecretKey1234567890!` |
| `Jwt:Issuer` | Emisor válido del token JWT | `CRMApi` |
| `Jwt:Audience` | Audiencia válida del token JWT | `CRMFrontend` |
| `AllowedHosts` | Hosts permitidos para CORS | `*` o `http://localhost:5261` |

## 🚀 Colección Postman
Puedes importar la colección completa directamente a Postman utilizando el estándar OpenAPI (Swagger):
1. Abre Postman y haz clic en **Import**.
2. Selecciona la pestaña **File** o **Raw text**.
3. Importa el archivo `swagger.json` o pega su contenido.
4. Postman generará automáticamente todas las carpetas, endpoints y esquemas de Request/Response.

## 🔗 Listado de Endpoints
### 📦 Módulo: Activation
#### `GET` /api/activations/pending
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idProvider` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/orders/{id}/activations
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/activations/delayed
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/activations/{idItem}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idItem` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: AlternateProfile
#### `POST` /api/orders/{id}/alternate-profile
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/orders/{id}/alternate-profile
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Approval
#### `POST` /api/orders/{id}/approvals
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/approvals/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/approvals/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Audit
#### `GET` /api/audit/checklist/{idCmpg}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idCmpg` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/orders/{id}/audit
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/audit/{id}/items
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/audit/{id}/close
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Auth
#### `POST` /api/Auth/login
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/Auth/me
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/Auth/refresh-token
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/Auth/logout
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/Auth/check-permission
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `permissionKey` | query | No | string |
| `statusId` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Backoffice
#### `GET` /api/backoffice/orders
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |
| `statusId` | query | No | integer |
| `campaignId` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/backoffice/pending-docs
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/backoffice/orders/{id}/status
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/backoffice/documents/{id}/verify
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Campaign
#### `GET` /api/campaigns
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/campaigns/{id}/statuses
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/campaigns/orders
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |
| `statusId` | query | No | integer |
| `campaignId` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/campaigns/advisors
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `role` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Commission
#### `GET` /api/currencies
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/currencies/convert
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `from` | query | No | string |
| `to` | query | No | string |
| `amount` | query | No | number |
| `date` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/commissions/settlements
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/commissions/settlements
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/commissions/settlements/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PUT` /api/commissions/settlements/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `DELETE` /api/commissions/settlements/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/commissions/settlements/{id}/items
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Document
#### `GET` /api/orders/{id}/documents
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/orders/{id}/documents
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/documents/{id}/verify
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/documents/{id}/download
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Form
#### `GET` /api/forms/campaign/{idCmpg}/stage/{idStage}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idCmpg` | path | Sí | integer |
| `idStage` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/forms/{idForm}/fields
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idForm` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/forms/order/{idOrder}/data
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idOrder` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/forms/order/{idOrder}/template/{idForm}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idOrder` | path | Sí | integer |
| `idForm` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PUT` /api/forms/data/{idData}/status
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idData` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/forms/seed
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Incident
#### `GET` /api/incidents/catalog
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idCmpg` | query | No | integer |
| `idStatus` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/incidents/order/{idOrder}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idOrder` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/incidents/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PUT` /api/incidents/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `DELETE` /api/incidents/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/incidents
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/incidents
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `assignedToRole` | query | No | string |
| `status` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/incidents/{id}/responses
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/incidents/{id}/resolve
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/incidents/{id}/kb-suggestions
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: KB
#### `GET` /api/kb/search
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `query` | query | No | string |
| `idCmpg` | query | No | integer |
| `contentType` | query | No | string |
| `incidentId` | query | No | integer |
| `statusId` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/kb/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/kb/{id}/feedback
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Lead
#### `GET` /api/leads
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `searchTerm` | query | No | string |
| `statusId` | query | No | integer |
| `page` | query | No | integer |
| `limit` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/leads
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/leads/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/leads/{id}/status
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Maintenance
#### `GET` /api/maintenance/statuses
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/maintenance/statuses/{id}/toggle
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/maintenance/testschema
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/maintenance/products
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/maintenance/products/{id}/toggle
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/maintenance/incidents
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/maintenance/incidents
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PUT` /api/maintenance/incidents/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `DELETE` /api/maintenance/incidents/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/maintenance/exchange-rates
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/maintenance/exchange-rates
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/maintenance/campaigns
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Notification
#### `GET` /api/notifications
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |
| `limit` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/notifications/{id}/read
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/notifications/read-all
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: PreSale
#### `GET` /api/presales
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/presales
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/presales/{id}/calls
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/presales/{id}/assign
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/presales/{id}/convert
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Provider
#### `GET` /api/providers
**Descripción:** Sin descripción proporcionada.

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/providers/{id}/status-mapping
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/providers/{id}/sync-log
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/providers/{id}/update-order-status
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Report
#### `GET` /api/reports/funnel
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idCmpg` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/reports/sales-by-asesor
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/reports/incidents
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idCmpg` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/reports/activations
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `idProvider` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: SalesOrder
#### `GET` /api/orders
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |
| `statusId` | query | No | integer |
| `campaignId` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/orders
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/orders/{id}
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/orders/{id}/history
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `PATCH` /api/orders/{id}/status
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `id` | path | Sí | integer |

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
### 📦 Módulo: Supervisor
#### `GET` /api/supervisor/orders
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `userId` | query | No | integer |
| `statusId` | query | No | integer |
| `campaignId` | query | No | integer |
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `GET` /api/supervisor/stats
**Descripción:** Sin descripción proporcionada.

**Parámetros:**
| Nombre | Ubicación | Requerido | Tipo |
|--------|-----------|-----------|------|
| `dateFrom` | query | No | string |
| `dateTo` | query | No | string |

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
#### `POST` /api/supervisor/bulk-transfer
**Descripción:** Sin descripción proporcionada.

**Cuerpo de la Petición (Request):**
```json
// Ejemplo de la estructura esperada basada en el esquema OpenAPI
{
  "ejemplo": "Ver esquema detallado en Swagger UI"
}
```

**Códigos de Respuesta Esperados:**
- **200**: OK
- **400 (Bad Request)**: Error de validación en la solicitud.
- **401 (Unauthorized)**: Token no proporcionado o inválido.
- **403 (Forbidden)**: El usuario no tiene permisos para este recurso.
- **404 (Not Found)**: Recurso no encontrado.
- **422 (Unprocessable Entity)**: Error de reglas de negocio al procesar.

---
