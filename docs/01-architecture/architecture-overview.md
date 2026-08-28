# ISO Header
Código: ARC-001
Versión: 1.0
Fecha: 2026-08-27
Autor: Tech Lead

# Architecture Overview

## Patrón de Integración Híbrido: Motores Satélites

El ecosistema CRM utiliza una arquitectura orquestada por el `ApiHub`, que delega responsabilidades complejas a distintos motores (SlaEngine, FlowEngine, ApprovalEngine). 

Para la comunicación entre el `ApiHub` y estos motores, se ha adoptado un **Patrón Híbrido de Integración** basado en la criticidad transaccional:

### 1. Comunicación Síncrona / Bloqueante (Ej: FlowEngine)
- **Motivo:** Integridad de Estado.
- **Flujo:** Antes de grabar un cambio de estado en la base de datos local (PostgreSQL), el `ApiHub` debe confirmar si la transición es permitida legalmente por la máquina de estados configurada en el `FlowEngine`.
- **Fallo:** Si el FlowEngine no responde o devuelve error (circuito abierto mediante `Microsoft.Extensions.Http.Resilience`), la transacción se **ABORTA** y se rechaza la solicitud del frontend (HTTP 400 - `InvalidTransitionException`). No podemos tolerar que la base de datos asuma un estado ilegal o "fantasma".

### 2. Comunicación Asíncrona / Fire-and-Forget (Ej: SlaEngine)
- **Motivo:** Telemetría Operativa No Intrusiva.
- **Flujo:** Una vez que la transacción principal se consolida exitosamente en Dapper, el `ApiHub` envía una notificación asíncrona (Fire-and-Forget) al `SlaEngine` mediante HTTP para iniciar o detener los cronómetros de SLA (Acuerdos de Nivel de Servicio) de los asesores.
- **Fallo:** Si el SlaEngine no responde, el fallo se **aísla y se captura localmente** sin revertir la transacción principal de negocio. Se registra un error crítico estructurado vía Serilog en la salida estándar, lo cual permite indexarlo posteriormente (vía Seq/Grafana/ELK), garantizando que el usuario del frontend perciba una experiencia fluida e ininterrumpida.
