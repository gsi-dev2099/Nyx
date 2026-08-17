# Guía Práctica de Producción y Replicación — Suite de Motores Autónomos Nyx CRM

**Versión**: 3.0 (Orientada a Despliegue en Producción Real)  
**Fecha**: 14 de Agosto de 2026  
**Autor**: @tech-lead  
**Documento Oficial**: Operación en Producción, Permisos RBAC y Replicación por Interfaz

---

## 🏢 1. Arquitectura de Producción Real

En un entorno de **Producción**, la plataforma opera bajo una arquitectura de microservicios contenerizada detrás de un API Gateway y un Proxy Inverso Nginx con SSL/TLS:

```
                          [ USUARIOS / CLIENTES EN PRODUCCIÓN ]
                                          │
                                          ▼ (HTTPS / TLS 1.3 - Puerto 443)
                             ┌─────────────────────────┐
                             │   NGINX REVERSE PROXY   │
                             └────────────┬────────────┘
                                          │
                  ┌───────────────────────┴───────────────────────┐
                  ▼ (Red Interna Docker)                         ▼ (Red Interna Docker)
     ┌───────────────────────────┐                  ┌───────────────────────────┐
     │ CRM.WebFrontend (Blazor)  │                  │   CRM.ApiHub (:5068)      │
     │      (Puerto 5261)        │                  │   (REST & Rate Limiting)  │
     └───────────────────────────┘                  └─────────────┬─────────────┘
                                                                  │ (YARP / HTTP Intranet)
                                     ┌────────────────────────────┼────────────────────────────┐
                                     ▼                            ▼                            ▼
                        ┌────────────────────────┐  ┌────────────────────────┐  ┌────────────────────────┐
                        │ Nyx.FlowEngine (:5072) │  │Nyx.ApprovalEngine(:5071│  │ Nyx.SlaEngine (:5070)  │
                        │    (BD nyx_flow)       │  │   (BD nyx_approval)    │  │    (BD nyx_sla)        │
                        └────────────────────────┘  └────────────────────────┘  └────────────────────────┘
```

---

## 🔐 2. Segmentación Estricta de Permisos por Rol

| Rol | Módulo Asignado | Permisos y Alcance | Acceso por Interfaz (Sin teclear URLs) |
|---|---|---|---|
| **`ADMIN_CRM`** | `⚙️ Gobierno N-Capas` (`/engines/checkpoints`) | **Administración Total**: Crear/Eliminar Capas dinámicas N-Tier, seleccionar capas y registrar Checkpoints/Tasks por Interfaz Web consumiendo APIs. | Botón en Panel Admin (`/admin`), Tarjeta en Dashboard y Menú Lateral. |
| **`SUPERVISOR` / `COORDINADOR`** | `🛡️ Bandeja Aprobaciones` (`/engines/approvals`) | **Firma SOX / ISO 27001**: Revisar solicitudes pendientes asignadas, justificar y responder `Aprobar` o `Rechazar`. | Botón en Dashboard Supervisor (`/supervisor/dashboard`) y Menú Lateral. |
| **`BACKOFFICE`** | `🛡️ Bandeja Aprobaciones` & Activaciones (`/backoffice/activations`) | **Verificación & Provisión**: Auditar bloqueos antes de activar servicio y autorizar firmas. | Botón en Panel Activaciones (`/backoffice/activations`) y Menú Lateral. |
| **`ASESOR`** | `🛒 Mis Ventas` (`/asesor/orders`) | **Operativo**: Creación de ventas, consulta de 10 etapas en popup y badge `Motor Nyx` en ficha. | Menú Lateral y redirección tras Login. |
| **TODOS LOS ROLES** | `👥 Perfil & Accesibilidad` (`/profile`) | **Inclusividad Visual**: Selector de temas (Corporativo Mate, Protanopía, Tritanopía, Alto Contraste). | Menú Lateral y clic en Avatar. |

---

## 🔄 3. Ciclo de Vida Práctico de una Venta en Producción

### **Fase 1: Registro e Inspección en Tiempo Real (Rol Asesor)**
1. El Asesor registra una venta en `/asesor/orders/new`.
2. `Nyx.FlowEngine` evalúa la orden contra la **Capa 1 (Catálogo Institucional)** y **Capa 2 (Instancia Ficha)** en menos de `15ms`.
3. En la Bandeja del Asesor (`/asesor/orders`), el sistema muestra la **Línea de Tiempo Compacta (`Etapa X de 10`)**. Al hacer clic, se abre el modal interactivo de 10 etapas.
4. En el Detalle de la Ficha (`/asesor/orders/{id}`), el badge `Motor Nyx` confirma el estado de gobernanza.

### **Fase 2: Evaluación & Retención SOX (Motor de Aprobaciones)**
1. Si la orden excede políticas sensibles (ej: *Descuento mayor al 25%* o *Cancelación de contrato*), `Nyx.ApprovalEngine` bloquea el avance y genera una solicitud en estado `PENDING`.
2. El Supervisor **recibe la notificación y accede directamente a su Bandeja de Aprobaciones SOX (`/engines/approvals`)** haciendo clic en el botón `🛡️ Bandeja Aprobaciones SOX` del Dashboard de Supervisor.

### **Fase 3: Firma Digital & Justificación (Rol Supervisor)**
1. En `/engines/approvals`, el Supervisor visualiza únicamente sus solicitudes pendientes.
2. Ingresa la justificación en el campo de texto y presiona `✓ Aprobar` o `✗ Rechazar`.
3. La API `/api/approval/requests/{id}/decide` procesa la firma y `Nyx.FlowEngine` libera la orden.

### **Fase 4: Activación y Entrega al Cliente (Rol Backoffice)**
1. El analista de Backoffice ingresa a `/backoffice/activations`.
2. Verifica mediante el botón `🛡️ Checkpoints & Aprobaciones` que no existan bloqueos de Capa 3 (Audio) o Capa 4 (Cortes Proveedor).
3. Activa el servicio del cliente.

---

## ⚙️ 4. Guía de Administración por Interfaz Web (Rol `ADMIN_CRM`)

Toda la administración del motor se realiza **100% desde la Interfaz Web consumiendo las APIs**, sin necesidad de ejecutar comandos en consola:

### **A. Administración de Capas N-Tier**
1. Accede a `⚙️ Gobierno N-Capas` (`/engines/checkpoints`) desde el Panel Admin o Menú.
2. En la sección `Registrar Nueva Capa en el Motor`:
   - Ingresa el **Nombre** (ej: *Capa 5: Postventa & Garantías*).
   - Define el **Ámbito** (ej: *POSTSALES_SERVICE*).
   - Especifica la **Entidad Target CRM** (ej: *CLAIM_TICKET*).
   - Presiona `+ Crear Capa`.
3. **Protección de Capas**:
   - Las capas base de sistema (`Capa 1` y `Capa 2`) tienen la insignia `[SISTEMA BASE]` y su botón de eliminación está **bloqueado**.
   - Las capas creadas dinámicamente muestran la insignia `[DINÁMICA N-CAPA]` y disponen del botón `🗑️ Eliminar Capa`.

### **B. Selección de Capa y Registro de Checkpoints vía API**
1. En la tabla de capas, haz clic en el botón `🔍 Administrar Flujos` de la capa deseada.
2. El editor enfoca la capa seleccionada.
3. En el formulario `+ Crear Nuevo Checkpoint para esta Capa (Vía API)`:
   - Código: `CP_POST_WARRANTY`
   - Nombre: *Verificación de Cobertura de Garantía*
   - Origen: `INTERNAL` / `EXTERNAL`
   - Bloqueante: *Activado/Desactivado*
4. Presiona **`Guardar en API`**. La aplicación realiza el `POST /api/engines/flow/catalogs` y actualiza la lista en tiempo real.

---

## 🛠️ 5. Guía de Replicación Técnica para Despliegue en Servidor

### **Paso 1: Variables de Entorno de Producción (`.env`)**
```ini
POSTGRES_USER=postgres
POSTGRES_PASSWORD=TuPasswordSuperSeguro2026!
POSTGRES_DB=nyx_crm
JWT_SECRET=TuSecretoJWTSuperSeguroDeMinimo32CaracteresNyx2026!
ASPNETCORE_ENVIRONMENT=Production
```

### **Paso 2: Comando de Despliegue en Producción**
```bash
# Reconstruir y desplegar contenedores en modo producción
docker compose -f docker-compose.prod.yml up -d --build
```

### **Paso 3: Verificación Automática de Salud de Motores**
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\test_engines_hub.ps1
```

---

## 🎯 Conclusión
El sistema **NYX CRM** en producción garantiza **segmentación de permisos por rol**, una **Bandeja de Aprobaciones dedicada para supervisores**, **gobierno N-Capas configurable 100% por interfaz web** y **redirección visual directa en todos los módulos**.
