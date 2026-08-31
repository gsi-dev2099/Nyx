# ISO Header
Código: DEV-RUN-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Runbook: Caída de la conexión con la base de datos PostgreSQL

## Propósito
Guiar al equipo de operaciones y guardia (L1/L2) cuando las sondas de `/api/health` reporten `Unhealthy` por pérdida de conexión con PostgreSQL en Azure.

## Pre-requisitos
- Acceso de lectura al Portal de Azure (Resource Group de BD).
- Permisos de consulta en los logs de los contenedores `CRM.ApiHub`.

## Pasos de Ejecución
1. **Verificar el Health Check:**
   - Confirmar si el error es específico de `nyx_crm` o incluye `ext_ecosystem` (FDW).
2. **Revisar Azure PostgreSQL Metrics:**
   - Entrar al Portal de Azure.
   - Revisar las métricas de `CPU`, `Active Connections` y `Storage Quota`. (Una cuota llena tira la BD abajo).
3. **Escalar o Reiniciar (Mitigación):**
   - Si las conexiones activas llegaron al límite (ej. fuga de conexiones), forzar un reinicio del Pooler (PgBouncer) o escalar el Tier temporalmente.
   - Si la DB está inaccesible por red, revisar el Azure VNet Peering.

## Rollback
- Si el escalamiento afectó la facturación, programar un Downscale inmediato tras la estabilización y purga de conexiones en el orquestador.
