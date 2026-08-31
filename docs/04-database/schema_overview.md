# ISO Header
Código: DB-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Schema Overview (nyx_crm)

El esquema relacional en PostgreSQL está diseñado para alta concurrencia.

## Tablas Principales
- **Users / Roles:** Autenticación y control de acceso.
- **Leads & SalesOrders:** Flujo comercial del CRM.
- **Audit & History:** Tablas inmutables para logs de eventos del negocio.

*(Refiérase a los scripts de migración para los DDL exactos).*
