# ISO Header
Código: DEP-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Estrategia de Despliegue en Microsoft Azure

## Topología de Red y Arquitectura
La infraestructura operará sobre **Azure Kubernetes Service (AKS)** o **Azure Container Apps (ACA)**, estructurada de la siguiente manera:

1. **Capa 2: Ingress Controller (Nginx Proxy)**
   - Expuesto mediante un Azure Application Gateway (WAF habilitado).
   - Termina el SSL/TLS.
   - Aplica inyección de cabeceras de seguridad estrictas (HSTS, CSP).

2. **Capa de Aplicación (Microservicios)**
   - `CRM.ApiHub`: Orquestador principal (ReplicaSet autoescalable).
   - `SlaEngine`, `FlowEngine`, `ApprovalEngine`: Servicios satélite.
   - Comunicación interna entre pods vía gRPC/HTTP interno.

3. **Capa de Datos (PaaS)**
   - **PostgreSQL:** Azure Database for PostgreSQL - Flexible Server (Alta disponibilidad, replicación multi-zona).
   - **Redis:** Azure Cache for Redis (Tier Standard o Premium para replicación en red privada).

4. **Gestión de Secretos**
   - **Azure Key Vault:** Integrado vía *Managed Identities* (Identidades Administradas). Los pods acceden a los secretos sin usar contraseñas hardcodeadas. La *Data Protection API* de .NET utiliza el Key Vault para cifrar el anillo de llaves (Key Ring).
