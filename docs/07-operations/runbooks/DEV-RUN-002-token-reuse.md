# ISO Header
Código: DEV-RUN-002
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Runbook: Alerta de Seguridad por Re-uso de Refresh Token

## Propósito
Procedimiento de respuesta ante incidentes cuando el `CRM.ApiHub` detecta un ataque de repetición (Replay Attack) o robo de sesión y ejecuta la revocación de la Token Family.

## Pre-requisitos
- Acceso a los logs centralizados (Log Analytics Workspace / Datadog).
- Panel de control de usuarios del CRM o acceso directo a la BD (tabla Users).

## Pasos de Ejecución
1. **Analizar la Alerta:**
   - Buscar en los logs el mensaje de advertencia emitido por `RefreshTokenUseCase`.
   - Identificar el `UserId` y el `FamilyId` afectado, así como la IP atacante y la IP legítima.
2. **Evaluación de Daños:**
   - Si la IP atacante pertenece a un país o ASN inusual, escalar inmediatamente a L3 (Ciberseguridad).
3. **Validación Manual:**
   - El sistema ya expulsó al usuario cerrando todas sus sesiones.
   - El equipo de soporte debe contactar al usuario afectado vía canal seguro (Slack, Teams, o teléfono) para verificar si su máquina fue comprometida.
4. **Restablecimiento:**
   - Forzar el reseteo de contraseña (Password Reset) del usuario antes de permitirle un nuevo inicio de sesión.

## Rollback
- Ninguno. Una familia revocada en Redis no puede ser restaurada. Es una política estricta de seguridad.
