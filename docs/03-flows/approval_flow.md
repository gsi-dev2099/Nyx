# ISO Header
Código: FLW-APP-001
Versión: 1.0
Fecha: 2026-08-28
Autor: Tech Lead

# Flujo de Aprobación de Órdenes

## Diagrama del Flujo

```mermaid
sequenceDiagram
    participant Frontend as CRM Frontend (Asesor)
    participant ApiHub as CRM.ApiHub
    participant Db as PostgreSQL (SalesOrder)
    participant ApprovalEngine as Nyx.ApprovalEngine
    participant Supervisor as CRM Frontend (Supervisor)

    Frontend->>ApiHub: POST /api/orders (Discount > 10%)
    activate ApiHub
    ApiHub->>Db: Inserción Inmutable (Status: PENDING_APPROVAL)
    ApiHub->>ApprovalEngine: POST /api/approval/requests/submit (Policy: HIGH_DISCOUNT)
    activate ApprovalEngine
    ApprovalEngine-->>ApiHub: ApprovalRequest ID
    deactivate ApprovalEngine
    ApiHub-->>Frontend: Order Created (Status: PENDING_APPROVAL)
    deactivate ApiHub

    Note over Supervisor,ApprovalEngine: Tiempo después... el Supervisor revisa su bandeja

    Supervisor->>ApiHub: GET /api/approvals/pending
    ApiHub->>ApprovalEngine: GET /api/approval/requests/pending
    ApprovalEngine-->>ApiHub: Listado de Pendientes
    ApiHub-->>Supervisor: Listado Renderizado en <Virtualize>

    Supervisor->>ApiHub: PATCH /api/approvals/{id} (APPROVED/REJECTED)
    activate ApiHub
    ApiHub->>ApprovalEngine: POST /api/approval/requests/{id}/decide
    ApprovalEngine-->>ApiHub: Decisión Registrada
    ApiHub->>Db: Actualiza Estado de SalesOrder
    ApiHub-->>Supervisor: OK (200)
    deactivate ApiHub
```

## Consideraciones
- **Transaccionalidad**: La orden se inserta inmutablemente en la base de datos principal, sin importar si el motor de aprobación está tardando en responder. El cliente de integración (`ApprovalEngineClient`) maneja los reintentos vía `Polly`.
- **Segregación de Deberes (SoD)**: El motor `Nyx.ApprovalEngine` valida internamente que el creador de la solicitud no pueda ser el mismo que toma la decisión, garantizando el cumplimiento normativo.
- **Rendimiento UI**: La interfaz de Supervisor (`SupervisorApprovals.razor`) emplea `<Virtualize>` para mantener la fluidez en el cliente independientemente de la cantidad de aprobaciones en cola.
