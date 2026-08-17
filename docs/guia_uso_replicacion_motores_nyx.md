# Guía de Uso & Replicación Manual — Suite de Motores Autónomos Nyx CRM

**Versión**: 2.0  
**Fecha**: 14 de Agosto de 2026  
**Autor**: @tech-lead  
**Estado**: Documento Oficial de Operación y Replicación Técnica

---

## 📌 1. Introducción y Filosofía del Sistema

El ecosistema **NYX CRM** ha evolucionado de un modelo basado en estados rígidos a un esquema descentralizado gobernado por **3 Motores Autónomos**:

1. **`Nyx.FlowEngine` (Puerto 5072, BD `nyx_flow`)**: Motor primario de ciclo de vida, flujos comerciales y modelo de Checkpoints Multi-Capa.
2. **`Nyx.ApprovalEngine` (Puerto 5071, BD `nyx_approval`)**: Motor de evaluación SOX / ISO 27001 para la firma y aprobación jerárquica de políticas.
3. **`Nyx.SlaEngine` (Puerto 5070, BD `nyx_sla`)**: Motor de ventanas laborables, tiempos de respuesta y escalamiento de incidencias.

---

## 🏗️ 2. Arquitectura de Checkpoints N-Capas

El gobierno del sistema opera en una **Arquitectura N-Capas (N-Tier)** donde cada capa tiene responsabilidades y reglas específicas:

```
┌────────────────────────────────────────────────────────────────────────┐
│  CAPA 1: Catálogo Institucional Gobernado (Triple Firma SOX/ISO 27001)   │
├────────────────────────────────────────────────────────────────────────┤
│  CAPA 2: Instancia Operativa de Venta y Ficha CRM                      │
├────────────────────────────────────────────────────────────────────────┤
│  CAPA 3: Auditoría & Aseguramiento de Calidad de Audio                 │
├────────────────────────────────────────────────────────────────────────┤
│  CAPA 4: Cortes Contables & Habilitación Comercial Proveedores          │
├────────────────────────────────────────────────────────────────────────┤
│  CAPA N: Capas Dinámicas Configurables (Postventa, Garantías, etc.)    │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 🖥️ 3. Guía de Operación por Perfil de Usuario

### 👤 A. Rol Admin CRM (`ADMIN_CRM`) — Usuario: `ronald` (ID 2)
1. **Panel Principal Admin (`/admin` / `/admin/dashboard`)**:
   - Visualiza en la cabecera el estado de los 3 Motores en tiempo real (`3/3 En Línea`).
   - Usa el **Buscador Interactivo** para encontrar módulos por palabra clave (ej. *"comisiones"*, *"canvas"*, *"SLA"*).
   - Accede a los 4 bloques principales:
     - 👥 *Usuarios & RBAC*
     - ⚙️ *Configuración Operativa & Canvas*
     - ⚡ *Motores Nyx Hub*
     - 📊 *Auditoría, Comisiones & Logs*
2. **Engines Hub Control (`/engines/hub`)**:
   - Monitorea `/health`, latencia y puertos (:5070, :5071, :5072).
3. **Gestor N-Capas (`/engines/checkpoints`)**:
   - Consulta el Catálogo Capa 1 y Ficha Capa 2.
   - Revisa las reglas de Capa 3 (Audio) y Capa 4 (Cortes Proveedor).
   - Registra una **Nueva Capa Dinámica** usando el formulario interactivo de la pestaña `⚙️ Gestor N-Capas (+Nueva)`.

---

### 🛡️ B. Rol Supervisor / Coordinador (`SUPERVISOR`) — Usuarios: `cnaranjo` (ID 9), `dramos` (ID 251)
1. **Bandeja de Aprobaciones SOX (`/engines/checkpoints` -> Tab `Bandeja SOX`)**:
   - Ingresa como supervisor y visualiza solicitudes en estado `PENDING`.
   - Revisa la política asociada (ej. `HIGH_DISCOUNT`, `ORDER_CANCELLATION`).
   - Haz clic en `✓ Aprobar` o `✗ Rechazar` ingresando la justificación.
2. **Seguimiento de Equipo & Kanban (`/supervisor/dashboard`)**:
   - Supervisa el avance comercial y deriva solicitudes.

---

### ⚡ C. Rol Backoffice (`BACKOFFICE`) — Usuario: `gvillanueva` (ID 237)
1. **Panel de Activaciones (`/backoffice/activations`)**:
   - Revisa las solicitudes de servicio en provisión.
   - Haz clic en el botón `🛡️ Checkpoints & Aprobaciones` para verificar que la regla de Capa 2/3 no bloquee la activación.
   - Ejecuta la activación final del cliente.

---

### 🛒 D. Rol Asesor (`ASESOR`) — Usuario: `patricia` (ID 101)
1. **Bandeja de Ventas (`/asesor/orders`)**:
   - Visualiza en la tabla la columna **Mini Línea de Tiempo** con la etapa actual (`Etapa X de 10`).
   - Haz clic sobre la línea de tiempo para abrir el modal popup **Progreso de la Orden** con las 10 etapas del pipeline.
2. **Ficha de Venta (`/asesor/orders/{id}`)**:
   - Inspecciona el badge distintivo `Motor Nyx` en la cabecera.
   - Haz clic en `Línea de Tiempo` para filtrar eventos por *Todos*, *Estados*, *Documentos* o *Ficha*.
   - Haz clic en `Ver campos guardados` para auditar qué datos cambiaron en cada guardado.

---

### 👁️ E. Todos los Roles: Perfil & Accesibilidad Visual (`/profile`)
1. Haz clic en la opción **Perfil & Accesibilidad** del Menú Lateral o en tu Avatar.
2. Selecciona la paleta de colores inclusiva deseada:
   - 🎨 **NYX Corporativo Mate Neutro**: Colores sobrios de alta legibilidad.
   - 👁️ **Rojo-Verde (Protanopía / Deuteranopía)**: Uso de Azul Cobalto / Ámbar.
   - 👁️ **Azul-Amarillo (Tritanopía)**: Uso de Carmesí / Teal.
   - 🌓 **Alto Contraste / Heterocromía**: Modo oscuro mate anti-fatiga visual.
3. La preferencia se guarda automáticamente en `localStorage` y se mantiene al recargar.

---

## 🛠️ 4. Replicación Manual Paso a Paso (Guía Técnica)

### Paso 1: Configuración de la Base de Datos y Extensión `pgcrypto`
Para actualizar contraseñas usando BCrypt nativo en PostgreSQL:

```sql
-- 1. Conectarse a PostgreSQL en el contenedor
docker exec -it crm_postgres psql -U postgres -d nyx_crm

-- 2. Habilitar pgcrypto
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 3. Actualizar contraseñas de los 5 usuarios a 'Nyx2024!'
UPDATE user_service.users 
SET password_hash = crypt('Nyx2024!', gen_salt('bf', 10)) 
WHERE username IN ('ronald','cnaranjo','gvillanueva','dramos','patricia');
```

---

### Paso 2: Configuración del Rate Limiter en `CRM.ApiHub/Program.cs`
Para evitar bloqueos durante pruebas masivas E2E:

```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginLimit", opt =>
    {
        opt.PermitLimit = 30; // Aumentado para testing E2E (límite original: 5)
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

---

### Paso 3: Configuración de Proxy Reverso YARP en `CRM.WebFrontend/appsettings.json`
Ruta relativa para evitar CORS entre Blazor Frontend y la API Hub:

```json
{
  "ReverseProxy": {
    "Routes": {
      "apihub-route": {
        "ClusterId": "apihub-cluster",
        "Match": {
          "Path": "/api/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "apihub-cluster": {
        "Destinations": {
          "apihub-destination": {
            "Address": "http://crm_apihub:5068"
          }
        }
      }
    }
  }
}
```

---

### Paso 4: Reconstrucción y Despliegue con Docker Compose
```bash
# Reconstruir contenedores
docker compose up -d --build crm_apihub crm_webfrontend

# Verificar que los 5 contenedores estén corriendo
docker ps
```

---

### Paso 5: Script de Prueba E2E Automática (`scripts/test_engines_hub.ps1`)
Para validar los 5 usuarios y los 3 motores en un solo paso:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test_engines_hub.ps1
```

---

## 📊 5. Matriz de Verificación de Endpoints de Motores

| Método | Endpoint API Hub | Servicio Destino | Propósito |
|---|---|---|---|
| `GET` | `/api/engines/status` | Hub Multi-Motor | Estado de salud (`Healthy: true`) de los 3 motores |
| `GET` | `/api/engines/flow/catalogs` | `Nyx.FlowEngine` | Checkpoints Capa 1 registrados en catálogo |
| `GET` | `/api/engines/approval/pending` | `Nyx.ApprovalEngine` | Solicitudes pendientes por rol de aprobador |
| `POST` | `/api/approval/requests/{id}/decide` | `Nyx.ApprovalEngine` | Decisión de aprobación SOX / ISO 27001 |
| `GET` | `/api/engines/flow/instances/{id}` | `Nyx.FlowEngine` | Instancia Capa 2 de ficha de venta |

---

## 🎯 Conclusión
El sistema **NYX CRM** queda 100% articulado alrededor del **Motor Gobernador de Checkpoints N-Capas**, con soporte de accesibilidad inclusiva y capacidad de replicación técnica documentada.
