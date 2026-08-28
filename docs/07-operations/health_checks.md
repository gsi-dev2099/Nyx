# ISO Header
Código: OPS-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# API Health Checks

## Endpoint: `GET /api/health`

Valida activamente las conexiones a la base de datos y servicios externos.
- **Base de Datos (nyx_crm):** Realiza un `SELECT 1;`.
- **Ecosistema (FDW):** Verifica la conexión al esquema externo `ext_ecosystem`.
