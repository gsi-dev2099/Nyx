# ISO Header
Código: MOD-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Módulo de Leads (Core)

## Descripción General
El módulo de Leads gestiona el ciclo de vida inicial de los prospectos de ventas en el CRM, operando bajo una estricta **Arquitectura Hexagonal**.

## Estructura (Arquitectura Hexagonal)
1. **Domain:** Entidad `Lead.cs` y abstracción `ILeadRepository.cs`.
2. **Application (Use Cases):**
   - `CreateLeadUseCase`: Crea un prospecto dejando el `owner_user_id` como `null` (bolsa de trabajo por defecto).
   - `UpdateLeadStatusUseCase`: Cambia el estado del prospecto inyectando el `actor_id` (usuario que ejecuta la acción) para registro de auditoría.
3. **Infrastructure:**
   - `LeadRepository`: Implementación concreta usando **Dapper** y consultas SQL explícitas contra `PostgreSQL`. Para actualizaciones de estado, llama directamente a la función de BD `sales_service.validate_status_transition()` validando reglas y roles.
4. **Api:**
   - `LeadController`: Expone endpoints HTTP asegurados por JWT (Capa 1 Token Family) y protegidos por un **Rate Limiter de 100 req/min** (`ApiLimit`).
