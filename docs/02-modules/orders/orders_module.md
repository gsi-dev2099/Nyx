# ISO Header
Código: MOD-ORD-001
Versión: 1.0
Fecha: 2026-08-28
Autor: Tech Lead

# Módulo de Órdenes de Venta (Sales Orders)

## Descripción General
El módulo de Órdenes de Venta (Sales Orders) en CRM.ApiHub gestiona la transición de un Lead a una Venta concretada. Es el core transaccional que documenta el producto vendido, las condiciones comerciales y dispara los flujos operativos en los motores satélites (`Nyx.SlaEngine`, `Nyx.FlowEngine` y `Nyx.ApprovalEngine`).

## Proceso de Inserción
El caso de uso `CreateSalesOrderUseCase` es el encargado de instanciar la orden:
1. Recibe el DTO `SalesOrderCreateDto`.
2. Evalúa reglas de negocio (ej. `DiscountPercentage > 10`).
3. Registra la orden de manera inmutable en PostgreSQL usando Dapper.
4. Si la regla lo exige, el estado inicial será `PENDING_APPROVAL` e invocará de manera resiliente al `ApprovalEngine`.
5. Si no requiere aprobación, la orden se crea en estado por defecto (`BORRADOR` o `APPROVED`) e inicia de inmediato su flujo.
6. Notifica al `SlaEngine` para los tiempos de atención y al `FlowEngine` para determinar el pipeline (ej. `PIPELINE_TELECOM` o `PIPELINE_ALARMAS`).

## Reglas de Negocio
- **Límite de Descuento:** Toda orden de venta con un `DiscountPercentage > 10` requiere aprobación explícita de un Supervisor o Backoffice. El motor satélite maneja las delegaciones y reglas ISO 27001 para la Segregación de Funciones (un Asesor no puede autorizar su propia venta).
