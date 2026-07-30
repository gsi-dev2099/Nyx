# 📚 Documentación Técnica de API (CRM API Hub)

Esta es la documentación técnica y oficial de los endpoints principales del sistema Nyx CRM, orientada a los desarrolladores de Frontend y QA. Detalla los contratos, estructuras de petición/respuesta y reglas de negocio.

---

## ⚙️ Variables de Entorno Requeridas

Para desplegar y ejecutar el backend (`CRM.ApiHub`) correctamente, debes configurar las siguientes variables de entorno en el archivo `appsettings.json` o `.env`:

| Variable | Descripción | Ejemplo de Valor |
|----------|-------------|---------|
| `ConnectionStrings:DefaultConnection` | Cadena de conexión principal hacia la base de datos PostgreSQL. | `Host=localhost;Database=nyx_crm;Username=postgres;Password=Secret123!` |
| `Jwt:Key` | Clave secreta (mínimo 256 bits) para firmar los tokens de autenticación JWT. | `TuClaveSecretaSuperSeguraParaElCRM_2099!` |
| `Jwt:Issuer` | Emisor autorizado que expide los tokens. | `CRMApiHub` |
| `Jwt:Audience` | Audiencia permitida para consumir los tokens (Frontend). | `CRMWebFrontend` |
| `AllowedHosts` | Control de CORS para especificar desde qué orígenes se aceptan peticiones. | `*` o `http://localhost:5261` |
| `NYX_DB_ENCRYPTION_KEY` | Llave de encriptación para datos sensibles en la base de datos (Ej. credenciales de proveedores). | `KeyDe32BytesParaEncriptacionFuerte==` |

---

## 🚀 Uso de la Colección OpenAPI (Postman)

El proyecto cuenta con un archivo `swagger.json` autogenerado que contiene todos los esquemas técnicos. Para generar tu colección de pruebas:
1. Asegúrate de tener el backend corriendo y navega a `http://localhost:5068/swagger/v1/swagger.json`.
2. Guarda el archivo o copia su contenido.
3. Abre **Postman** -> **Import** -> pega el contenido o arrastra el archivo.
4. Postman te generará automáticamente todas las carpetas, endpoints y cuerpos de petición (Request Bodies) basados en nuestros DTOs.

---

## 🔗 Catálogo de Endpoints Principales

A continuación se documentan los módulos centrales del flujo de negocio. Todos los endpoints (excepto Auth) requieren incluir el token en la cabecera: `Authorization: Bearer <token>`.

### 🔐 1. Módulo: Autenticación (Auth)

#### `POST` `/api/auth/login`
**Descripción:** Valida las credenciales del usuario, genera un token JWT firmado y devuelve el perfil completo (rol, ID, nombre). Único endpoint público.

**Cuerpo de la Petición (Request):**
```json
{
  "username": "dramos",
  "password": "dayanyy2010"
}
```

**Respuestas Esperadas:**
- **200 (OK)**: Login exitoso.
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "idUser": 9,
      "username": "dramos",
      "fullName": "Dayany Ramos",
      "role": "SUPERVISOR",
      "idCampaign": 2
    },
    "message": "Autenticación exitosa."
  }
  ```
- **400 (Bad Request)**: El usuario o contraseña están vacíos.
- **401 (Unauthorized)**: Credenciales incorrectas o usuario inactivo.
- **500 (Internal Server Error)**: Fallo en la conexión a la base de datos.

---

### 📊 2. Módulo: Ventas (SalesOrder)

#### `PATCH` `/api/orders/{id}/status`
**Descripción:** Actualiza el estado de una orden de venta de manera individual. Maneja la lógica de cambio de custodia (Ej: Asignar a un Analista Backoffice) y dispara eventos de notificación (SignalR) al usuario correspondiente.

**Parámetros:**
- `id` (path, numérico): ID de la orden de venta.

**Cuerpo de la Petición (Request):**
```json
{
  "toStatusId": 3,
  "toSubstatusId": null,
  "comment": "Transferido al BackOffice para revisión final.",
  "isBulk": false
}
```

**Respuestas Esperadas:**
- **200 (OK)**: Estado actualizado correctamente.
- **400 (Bad Request)**: Transición de estado no permitida o datos inválidos.
- **403 (Forbidden)**: El rol actual no tiene permisos para realizar este cambio de estado.
- **404 (Not Found)**: La orden no existe.
- **422 (Unprocessable Entity)**: Regla de negocio rota (Ej: Intentar enviar a BAC sin los documentos obligatorios adjuntos).

#### `GET` `/api/orders/{id}`
**Descripción:** Retorna la ficha "360°" de una venta. Incluye los datos cruzados del lead, la orden, el asesor asignado y los KPIs básicos.

**Respuestas Esperadas:**
- **200 (OK)**: Datos devueltos exitosamente.
  ```json
  {
    "idOrder": 18,
    "clientName": "Juan Perez",
    "campaignName": "Campaña España",
    "advisorName": "Ronald Asesor",
    "backofficeName": "Gina Villanueva",
    "statusName": "En BackOffice",
    "totalProducts": 2,
    "totalValue": 150.50
  }
  ```
- **404 (Not Found)**: Orden inexistente o sin permisos de lectura.

---

### 👥 3. Módulo: Supervisor

#### `POST` `/api/supervisor/bulk-transfer`
**Descripción:** Endpoint utilizado por los Supervisores para transferir múltiples órdenes simultáneamente (Bulk Transfer) hacia la bandeja de un analista de BackOffice. Asigna la custodia y notifica en masa.

**Cuerpo de la Petición (Request):**
```json
{
  "orderIds": [18, 29, 34],
  "backofficeUserId": 237,
  "comment": "Lote de ventas de la mañana"
}
```

**Respuestas Esperadas:**
- **200 (OK)**: Todas las órdenes o una parte de ellas fueron transferidas con éxito.
  ```json
  {
    "message": "Transferencia masiva procesada.",
    "details": {
      "successfulCount": 3,
      "failedCount": 0
    }
  }
  ```
- **400 (Bad Request)**: La lista de `orderIds` está vacía o el `backofficeUserId` no es válido (o ningún ID pudo ser procesado).
- **403 (Forbidden)**: El usuario ejecutando no tiene el rol `SUPERVISOR`.

---

### 🛡️ 4. Módulo: Backoffice

#### `GET` `/api/backoffice/orders`
**Descripción:** Obtiene la bandeja de entrada de órdenes asignadas al analista Backoffice autenticado. El backend filtra automáticamente por la columna `custody_user_id` correspondiente al token JWT y excluye leads basura (Ej: ID 99998).

**Respuestas Esperadas:**
- **200 (OK)**: Arreglo de órdenes asignadas.
  ```json
  [
    {
      "idOrder": 18,
      "idLead": 22,
      "campaignName": "Nyx Campaña 1",
      "statusName": "En BackOffice",
      "registerDate": "2026-07-30T10:00:00Z"
    }
  ]
  ```
- **401 (Unauthorized)**: Sesión expirada.
- **403 (Forbidden)**: Usuario no es un analista BAC.

---

## 📋 Diccionario General de Códigos de Error HTTP

Para asegurar consistencia en el manejo de excepciones en el Frontend, el backend responde con los siguientes códigos HTTP estándar:

- **200 (OK) / 201 (Created):** Solicitud ejecutada correctamente.
- **400 (Bad Request):** Error de validación en la capa de transporte (Ej: DTO mal formado, JSON inválido o nulo, campos requeridos faltantes).
- **401 (Unauthorized):** El usuario no está logueado o su JWT expiró o fue alterado.
- **403 (Forbidden):** El JWT es válido, pero el usuario no cuenta con el `Role` adecuado para consumir el endpoint (Ej: Un `ASESOR` intentando ejecutar `/api/supervisor/bulk-transfer`).
- **404 (Not Found):** El recurso (orden, lead, documento, etc.) no existe en la base de datos.
- **422 (Unprocessable Entity):** El request es sintácticamente correcto, pero no cumple con las reglas de negocio en la capa de dominio (Ej: Cambiar estado a "Vendido" cuando aún existen incidencias abiertas).
- **500 (Internal Server Error):** Excepción no capturada, caída de base de datos o fallo crítico en infraestructura.
