# ISO Header
Código: SEC-001
Versión: 2.0
Fecha: 2026-08-28
Autor: Tech Lead

# Registro de Secretos y Criptografía (Gestión ISO 27001)
**Regla Inquebrantable:** NUNCA documentar valores reales. Solo rutas lógicas de almacenamiento y claves.

## Gestor Centralizado: HashiCorp Vault
A partir de la versión 2.0, el ecosistema Nyx CRM utiliza HashiCorp Vault como única fuente de la verdad (SSOT) para credenciales y claves criptográficas (Zero-Trust).

### Rutas de Secretos (KV Engine v2)

#### 1. `secret/data/nyxcrm/database`
Contiene las credenciales e información de conexión a PostgreSQL.
- `ConnectionStrings__DefaultConnection`: Cadena de conexión principal.
- `NYX_DB_ENCRYPTION_KEY`: Clave maestra (32 bytes) para cifrado AES-256 de cadenas en la BD.
- `NYX_DB_ENCRYPTION_IV`: Vector de inicialización (16 bytes) para AES-256.

#### 2. `secret/data/nyxcrm/dataprotection`
Ruta administrada automáticamente por la Data Protection API de ASP.NET Core (`VaultXmlRepository`).
- Contiene los Key Rings XML que cifran las cookies de sesión y tokens JWT.
- Rotación automática cada 90 días gestionada por el framework.
