# ISO Header
Código: INT-001
Versión: 1.1
Fecha: 2026-08-27
Autor: Tech Lead

# Integraciones y Motores Internos

El `CRM.ApiHub` actúa como orquestador principal y se comunica con los siguientes servicios satélite (microservicios):

1. **SlaEngine (SLA API):** Supervisa tiempos de atención y cuellos de botella.
2. **FlowEngine (Flow API):** Orquesta el flujo de estados de órdenes de venta y de validación de estado de Leads.
3. **ApprovalEngine (Approval API):** Gestiona reglas de negocio para rechazos o validaciones de documentos.

## Contratos Específicos

### FlowEngine (Puerto 5072)

#### Validación de Transiciones (Leads y Órdenes)
- **Endpoint:** `POST /api/flow/validate-transition`
- **Responsabilidad:** Valida si un cambio de estado es legal según la máquina de estados definida en el motor.
- **Request (JSON):**
  ```json
  {
      "entityType": "LEAD",
      "currentState": 1,
      "targetState": 2
  }
  ```
- **Response:**
  - `200 OK` con body `true` si es válido.
  - `200 OK` con body `false` si es inválido.
- **Resiliencia (Circuit Breaker):** Todos los llamados a motores internos están protegidos por `Microsoft.Extensions.Http.Resilience` (Polly v8). Ante caída prolongada o timeouts, el circuito se abrirá y el orquestador abortará la operación central lanzando una excepción de dominio (HTTP 400).
