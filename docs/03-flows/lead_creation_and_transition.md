# ISO Header
Código: FLW-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Flujo de Creación y Transición de Leads

## 1. Creación del Lead
- **Endpoint:** `POST /api/leads`
- **Actor:** API Externa, Landing Page o Asesor.
- **Regla:** El `OwnerUserId` y `CustodyUserId` permanecen `null` al crear. El estado inicial se asigna automáticamente como `1` (NUEVO).

## 2. Asignación y Gestión
- El Lead ingresa a una bolsa de trabajo. 
- Los asesores con permiso pueden solicitar su custodia, lo cual desencadena un `UpdateLeadStatus` para cambiar el estado a "EN PROCESO".

## 3. Transición de Estado
- **Endpoint:** `PATCH /api/leads/{id}/status`
- **Actor:** Asesor o Supervisor.
- **Lógica de Transacción Orquestada:**
  1. Se consulta el estado actual del Lead.
  2. **Validación de Reglas de Negocio:** El `ApiHub` envía una petición RPC al motor satélite `FlowEngine` para verificar la validez de la transición (`CurrentState` -> `TargetState`).
  3. **Resiliencia:** Si el motor satélite falla, el patrón *Circuit Breaker* (Polly) protegerá el orquestador abortando la operación con un HTTP 400 (InvalidTransitionException), evitando guardar un estado sucio.
  4. Si es válida, Dapper bloquea la fila (`FOR UPDATE`) y ejecuta la actualización inmutable del tracking.
