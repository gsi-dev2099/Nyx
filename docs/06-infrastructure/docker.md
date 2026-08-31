# Infraestructura Docker y Contenedores

Este documento detalla la configuración y despliegue del ecosistema Nyx CRM usando Docker.

## Gestión de Secretos: HashiCorp Vault (ISO 27001)
El entorno de desarrollo incluye una instancia de **HashiCorp Vault** en modo `dev`.

### ¿Cómo funciona Vault en el entorno local?
Al ejecutar `docker-compose up`, se levanta el servicio `crm_vault`. Inmediatamente después, un contenedor efímero llamado `vault_init` arranca, se conecta a Vault usando el token de desarrollo y ejecuta los comandos necesarios para inyectar la configuración inicial (ej. connection strings, encryption keys). Una vez inyectados los secretos, `vault_init` se apaga (Exit 0) de forma segura.

### Interfaz Web (UI) de Vault
Puedes acceder a la interfaz gráfica local para auditar o modificar los secretos manualmente:
- **URL**: `http://localhost:8200/ui/`
- **Método de Autenticación**: Token
- **Token**: `NyxVaultRootToken2026`

### Rutas de Secretos Inicializadas
- `secret/nyxcrm/database`: Contiene credenciales de Postgres y llaves de encriptación de Dapper.
- `secret/nyxcrm/dataprotection`: Creada dinámicamente por .NET al arrancar para guardar los Key Rings de las cookies y JWT.

## Troubleshooting (Fail-Safe)
Si `CRM.ApiHub` no encuentra Vault al arrancar, el contenedor **morirá de forma controlada (Exit)**. Docker Compose volverá a intentar levantarlo automáticamente. Esto garantiza que la aplicación jamás inicie con una configuración por defecto insegura.
