# Contratos de API — CRM CallCenter
> **Especificación de Contratos de API para Módulos de Autenticación, Leads, Pre-Venta, Órdenes y Catálogos.**

---

## 📌 Configuración General
* **URL Base en Desarrollo**: `http://localhost:5068`
* **Formato de Envío/Recepción**: `application/json`
* **Codificación**: `UTF-8`

---

## 🔒 Módulo: Autenticación

### 1. Iniciar Sesión (Login)
* **Endpoint**: `POST /api/auth/login`
* **Descripción**: Valida las credenciales del usuario y genera un JWT de acceso junto con un Refresh Token.
* **Request Body**:
```json
{
  "username": "test.asesor",
  "password": "password123"
}
```
* **Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "7c9f8021-d68a-4936-a36c-94cf13a5a7b6",
  "expiration": "2026-07-06T12:00:00Z"
}
```
* **Response (401 Unauthorized)**:
```json
{
  "message": "Credenciales inválidas."
}
```

---

### 2. Obtener Datos de Usuario Autenticado (/me)
* **Endpoint**: `GET /api/auth/me`
* **Descripción**: Obtiene los datos de sesión, rol y campaña del usuario autenticado a partir de su JWT.
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
{
  "id": 10,
  "username": "test.asesor",
  "name": "Juan Pérez",
  "role": "ASESOR",
  "assignedCampaign": {
    "id": 1,
    "name": "Campaña Portabilidad Claro"
  }
}
```

---

### 3. Refrescar Token
* **Endpoint**: `POST /api/auth/refresh-token`
* **Descripción**: Solicita un nuevo Access Token JWT a partir de un Refresh Token válido con Session Binding (IP y User-Agent).
* **Request Body**:
```json
{
  "accessToken": "eyJhbGciOiJIUz...",
  "refreshToken": "7c9f8021-d68a-4936-a36c-94cf13a5a7b6"
}
```
* **Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5c...",
  "refreshToken": "f89d3a42-bc89-49ef-867c-9b932c02b1ff",
  "expiration": "2026-07-06T12:15:00Z"
}
```

---

### 4. Cerrar Sesión (Logout)
* **Endpoint**: `POST /api/auth/logout`
* **Descripción**: Revoca y elimina de forma inmediata el Refresh Token del usuario del almacén de sesiones.
* **Request Body**:
```json
{
  "token": "f89d3a42-bc89-49ef-867c-9b932c02b1ff"
}
```
* **Response (200 OK)**:
```json
{
  "message": "Sesión revocada exitosamente."
}
```

---

## 📋 Módulo: Leads

### 1. Listar Leads (Paginado)
* **Endpoint**: `GET /api/leads`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Query Params**:
  * `page` (int, opcional, default 1)
  * `pageSize` (int, opcional, default 20)
  * `campaignId` (long, opcional)
  * `search` (string, opcional)
* **Response (200 OK)**:
```json
{
  "data": [
    {
      "id": 1,
      "firstName": "Juan",
      "lastName": "Pérez",
      "email": "juan@email.com",
      "phoneNumber": "+51999888777",
      "campaignId": 1,
      "statusId": 1,
      "createdAt": "2026-07-01T15:30:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

---

### 2. Crear Lead
* **Endpoint**: `POST /api/leads`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Request Body**:
```json
{
  "firstName": "María",
  "lastName": "García",
  "email": "maria.garcia@email.com",
  "phoneNumber": "+51999111222",
  "campaignId": 1
}
```
* **Response (201 Created)**:
```json
{
  "id": 42,
  "firstName": "María",
  "lastName": "García",
  "message": "Lead creado exitosamente."
}
```

---

### 3. Obtener Lead por ID
* **Endpoint**: `GET /api/leads/{id}`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
{
  "id": 42,
  "firstName": "María",
  "lastName": "García",
  "email": "maria.garcia@email.com",
  "phoneNumber": "+51999111222",
  "campaignId": 1,
  "statusId": 1,
  "createdAt": "2026-07-01T15:30:00Z"
}
```

---

## 📞 Módulo: Pre-Venta

### 1. Registrar Pre-Venta
* **Endpoint**: `POST /api/leads/{leadId}/presale`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Request Body**:
```json
{
  "assignedUserId": 10,
  "notes": "Interesado en Plan Premium Claro, llamada de seguimiento agendada.",
  "scheduledContactDate": "2026-07-08T14:00:00Z"
}
```
* **Response (201 Created)**:
```json
{
  "id": 5,
  "leadId": 42,
  "message": "Pre-venta registrada exitosamente."
}
```

---

### 2. Obtener Historial de Pre-Ventas por Lead
* **Endpoint**: `GET /api/leads/{leadId}/presale`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
{
  "data": [
    {
      "id": 5,
      "leadId": 42,
      "assignedUserId": 10,
      "assignedUserName": "Carlos Naranjo",
      "notes": "Cliente contactado, reagendado.",
      "scheduledContactDate": "2026-07-08T14:00:00Z",
      "createdAt": "2026-07-04T10:30:00Z"
    }
  ]
}
```

---

## 🛒 Módulo: Órdenes de Venta

### 1. Crear Orden de Venta
* **Endpoint**: `POST /api/orders`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Request Body**:
```json
{
  "leadId": 42,
  "products": [
    {
      "productId": 3,
      "quantity": 2
    }
  ],
  "notes": "Entrega y despacho urgente coordinado con el cliente."
}
```
* **Response (201 Created)**:
```json
{
  "id": 101,
  "leadId": 42,
  "statusId": 1,
  "statusName": "Pendiente",
  "totalAmount": 250.00,
  "message": "Orden creada exitosamente."
}
```

---

### 2. Obtener Orden por ID
* **Endpoint**: `GET /api/orders/{id}`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
{
  "id": 101,
  "leadId": 42,
  "leadName": "María García",
  "statusId": 1,
  "statusName": "Pendiente",
  "substatusId": null,
  "totalAmount": 250.00,
  "createdAt": "2026-07-04T11:00:00Z",
  "items": [
    {
      "productId": 3,
      "productName": "Plan Premium Claro",
      "quantity": 2,
      "unitPrice": 125.00
    }
  ]
}
```

---

### 3. Actualizar Estado de Orden
* **Endpoint**: `PUT /api/orders/{id}/status`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Request Body**:
```json
{
  "statusId": 2,
  "substatusId": 4,
  "comments": "Pago verificado por el asesor de tesorería."
}
```
* **Response (200 OK)**:
```json
{
  "orderId": 101,
  "newStatusId": 2,
  "newStatusName": "Confirmada",
  "message": "Estado actualizado exitosamente."
}
```

---

## 🗃️ Módulo: Catálogos

### 1. Listar Estados de Orden
* **Endpoint**: `GET /api/catalogs/statuses`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
[
  { "id": 1, "name": "Pendiente", "description": "Orden nueva sin procesar" },
  { "id": 2, "name": "Confirmada", "description": "Pago verificado" }
]
```

---

### 2. Listar Subestados por Estado
* **Endpoint**: `GET /api/catalogs/statuses/{statusId}/substatuses`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
[
  { "id": 4, "name": "Pago parcial", "orderStatusId": 2 },
  { "id": 5, "name": "Pago completo", "orderStatusId": 2 }
]
```

---

### 3. Listar Productos por Campaña
* **Endpoint**: `GET /api/catalogs/campaigns/{campaignId}/products`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
[
  { "id": 3, "sku": "PREM-CLARO-001", "name": "Plan Premium Claro", "unitPrice": 125.00 }
]
```

---

### 4. Listar Monedas
* **Endpoint**: `GET /api/catalogs/currencies`
* **Headers**: `Authorization: Bearer <jwt_token>`
* **Response (200 OK)**:
```json
[
  { "id": 1, "code": "PEN", "name": "Sol Peruano", "symbol": "S/" },
  { "id": 2, "code": "USD", "name": "Dólar Americano", "symbol": "$" }
]
```
