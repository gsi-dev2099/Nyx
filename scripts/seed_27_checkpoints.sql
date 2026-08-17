-- Script de Carga Consolidada: 27 Checkpoints de Producción (Nyx.FlowEngine)

-- 1. ALARMAS (10 Checkpoints)
INSERT INTO checkpoint_catalog 
(code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, rollback_to_stage, is_recurrent, recurrence_days, max_occurrences, owner_dept, portfolio, campaign, provider, finalizes_cycle, target_roles, approval_status)
VALUES
('CP_ALARM_01', 'Captación y calificación', 'Identificación inicial del prospecto y calificación de perfil de riesgo', 1, 1, 'INTERNAL', 'ENTITY', '{}', false, NULL, false, NULL, NULL, 'PREVENTA', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'ASESOR,SUPERVISOR', 'ACTIVE'),
('CP_ALARM_02', 'CRA / Elección de compañía', 'Elección de la Central Receptora de Alarmas y tipo de tecnología', 1, 1, 'INTERNAL', 'ENTITY', '{}', false, NULL, false, NULL, NULL, 'PREVENTA', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'ASESOR,SUPERVISOR', 'ACTIVE'),
('CP_ALARM_03', 'Venta y formalización', 'Formalización de la venta con oferta y tarifa seleccionada', 1, 2, 'INTERNAL', 'ENTITY', '{"COMMISSION"}', true, NULL, false, NULL, NULL, 'OPERACIONES', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'SUPERVISOR,BACKOFFICE', 'ACTIVE'),
('CP_ALARM_04', 'Verificación de gestión', 'Revisión y validación de consistencia en la gestión inicial', 1, 3, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'OPERACIONES', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'SUPERVISOR', 'ACTIVE'),
('CP_ALARM_05', 'Validación interna', 'Evaluación de scoring de riesgo y criterios anti-fraude', 1, 4, 'INTERNAL', 'ENTITY', '{"COMMISSION","SERVICE_ACTIVATION"}', true, 3, false, NULL, NULL, 'BACKOFFICE', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'BACKOFFICE,GERENCIA', 'ACTIVE'),
('CP_ALARM_06', 'Firma de contrato', 'Verificación de la firma digital o física del contrato de servicio', 1, 7, 'INTERNAL', 'ENTITY', '{"LIQUIDATION"}', true, NULL, false, NULL, NULL, 'BACKOFFICE', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'BACKOFFICE', 'ACTIVE'),
('CP_ALARM_07', 'Bienvenida', 'Llamada o mensaje de bienvenida al cliente por el proveedor', 1, 6, 'EXTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'PROVEEDOR', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'BACKOFFICE', 'ACTIVE'),
('CP_ALARM_08', 'Llamada técnica — Agendamiento', 'Coordinación de cita técnica para instalación de equipos', 1, 8, 'EXTERNAL', 'ENTITY', '{}', false, NULL, false, NULL, NULL, 'TRAMITACION', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'BACKOFFICE', 'ACTIVE'),
('CP_ALARM_09', 'Instalación — Activación', 'Confirmación de instalación física y activación en CRA', 1, 9, 'EXTERNAL', 'ENTITY', '{"SERVICE_ACTIVATION"}', true, NULL, false, NULL, NULL, 'PROVEEDOR', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', false, 'BACKOFFICE', 'ACTIVE'),
('CP_ALARM_10', 'Seguimiento de satisfacción postventa', 'Seguimiento recurrente a 6 meses de permanencia sin clawback', 1, 10, 'INTERNAL', 'ENTITY', '{}', false, NULL, true, 30, 6, 'POSTVENTA', 'ALARMAS', 'SECURITAS', 'SECURITAS_DIRECT', true, 'POSTVENTA,SUPERVISOR', 'ACTIVE')
ON CONFLICT (code) DO UPDATE SET
name = EXCLUDED.name, description = EXCLUDED.description, trigger_stage_id = EXCLUDED.trigger_stage_id, origin = EXCLUDED.origin, blocks = EXCLUDED.blocks, blocks_advance = EXCLUDED.blocks_advance, rollback_to_stage = EXCLUDED.rollback_to_stage, is_recurrent = EXCLUDED.is_recurrent, recurrence_days = EXCLUDED.recurrence_days, max_occurrences = EXCLUDED.max_occurrences, owner_dept = EXCLUDED.owner_dept, portfolio = EXCLUDED.portfolio, campaign = EXCLUDED.campaign, provider = EXCLUDED.provider, finalizes_cycle = EXCLUDED.finalizes_cycle, target_roles = EXCLUDED.target_roles, approval_status = EXCLUDED.approval_status;

-- 2. TELECOM — Propuesta Shirley (6 Checkpoints)
INSERT INTO checkpoint_catalog 
(code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, rollback_to_stage, is_recurrent, recurrence_days, max_occurrences, owner_dept, portfolio, campaign, provider, finalizes_cycle, target_roles, approval_status)
VALUES
('CP_TEL_SHIRLEY_01', 'Preventa Shirley', 'Evaluación inicial de líneas y cobertura comercial', 1, 1, 'INTERNAL', 'ENTITY', '{}', false, NULL, false, NULL, NULL, 'PREVENTA', 'TELECOM', 'GENERAL', 'INTERNO', false, 'ASESOR', 'ACTIVE'),
('CP_TEL_SHIRLEY_02', 'Creación CRM GSI', 'Registro formal de la venta en CRM GSI', 1, 2, 'INTERNAL', 'ENTITY', '{"COMMISSION"}', true, NULL, false, NULL, NULL, 'OPERACIONES', 'TELECOM', 'GENERAL', 'INTERNO', false, 'SUPERVISOR', 'ACTIVE'),
('CP_TEL_SHIRLEY_03', 'Revisión de Supervisor', 'Supervisión de calidad de llamada y oferta ofertada', 1, 3, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'SUPERVISOR', 'TELECOM', 'GENERAL', 'INTERNO', false, 'SUPERVISOR', 'ACTIVE'),
('CP_TEL_SHIRLEY_04', 'Validación Interna Shirley', 'Revisión de documentación y scoring de crédito', 1, 4, 'INTERNAL', 'ENTITY', '{"SERVICE_ACTIVATION"}', true, 3, false, NULL, NULL, 'BACKOFFICE', 'TELECOM', 'GENERAL', 'INTERNO', false, 'BACKOFFICE', 'ACTIVE'),
('CP_TEL_SHIRLEY_05', 'Envío de Carga a España', 'Transferencia de expediente de venta a la matriz en España', 1, 5, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'BACKOFFICE', 'TELECOM', 'GENERAL', 'INTERNO', false, 'BACKOFFICE,COORDINADOR', 'ACTIVE'),
('CP_TEL_SHIRLEY_06', 'Postventa / Seguimiento (7 touchpoints)', 'Matriz de 7 contactos postventa en 4 dimensiones', 1, 10, 'INTERNAL', 'ENTITY', '{}', false, NULL, true, 30, 7, 'POSTVENTA', 'TELECOM', 'GENERAL', 'INTERNO', false, 'POSTVENTA', 'ACTIVE')
ON CONFLICT (code) DO UPDATE SET
name = EXCLUDED.name, description = EXCLUDED.description, trigger_stage_id = EXCLUDED.trigger_stage_id, origin = EXCLUDED.origin, blocks = EXCLUDED.blocks, blocks_advance = EXCLUDED.blocks_advance, rollback_to_stage = EXCLUDED.rollback_to_stage, is_recurrent = EXCLUDED.is_recurrent, recurrence_days = EXCLUDED.recurrence_days, max_occurrences = EXCLUDED.max_occurrences, owner_dept = EXCLUDED.owner_dept, portfolio = EXCLUDED.portfolio, campaign = EXCLUDED.campaign, provider = EXCLUDED.provider, finalizes_cycle = EXCLUDED.finalizes_cycle, target_roles = EXCLUDED.target_roles, approval_status = EXCLUDED.approval_status;

-- 3. TELECOM — Backoffice (Vodafone: Leyash + Solivesa - 11 Checkpoints)
INSERT INTO checkpoint_catalog 
(code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, rollback_to_stage, is_recurrent, recurrence_days, max_occurrences, owner_dept, portfolio, campaign, provider, finalizes_cycle, target_roles, approval_status)
VALUES
('CP_TEL_VODA_01', 'Información Apta para Validación', 'Verificación de ficha completa y llamada al cliente para validación', 1, 4, 'INTERNAL', 'ENTITY', '{"COMMISSION"}', true, 3, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_LEYASH', 'AGENDO', false, 'BACKOFFICE,SUPERVISOR', 'ACTIVE'),
('CP_TEL_VODA_02', 'Conformidad de Validación Interna', 'Criterios anti-penalidad, anti-fraude por doble llamada y ofertas autorizadas', 1, 4, 'INTERNAL', 'ENTITY', '{"COMMISSION","SERVICE_ACTIVATION"}', true, 3, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_LEYASH', 'AGENDO', false, 'BACKOFFICE,SUPERVISOR', 'ACTIVE'),
('CP_TEL_VODA_03', 'Gestión de Datos Corregidos (Condicional)', 'Registro de corrección indicada por el cliente e informe a Operaciones', 1, 4, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_LEYASH', 'AGENDO', false, 'BACKOFFICE', 'ACTIVE'),
('CP_TEL_VODA_04', 'Confirmación de Datos para Cargar (Condicional)', 'Traslado de observación a Operaciones y confirmación de datos a usar', 1, 4, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_LEYASH', 'AGENDO', false, 'BACKOFFICE,COORDINADOR', 'ACTIVE'),
('CP_TEL_VODA_05', 'Solicitud de Carga por Operaciones', 'Gate final de salida de la etapa de Validación Interna', 1, 4, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'OPERACIONES', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'WIKITY', false, 'BACKOFFICE,COORDINADOR', 'ACTIVE'),
('CP_TEL_VODA_06', 'Confirmación de Carga — Operaciones', 'Respuesta definitiva de Operaciones autorizando la carga con datos finales', 1, 5, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'OPERACIONES', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'WIKITY', false, 'BACKOFFICE,SUPERVISOR', 'ACTIVE'),
('CP_TEL_VODA_07', 'Revisión de Información Antes de la Carga', 'Actualización de campos y verificación contra CRM GSI previo a lanzar', 1, 5, 'INTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'WIKITY', false, 'BACKOFFICE', 'ACTIVE'),
('CP_TEL_VODA_08', 'Carga de Venta al Proveedor', 'Lanzamiento de venta en CRM del proveedor (Agendo/Wikity) - Ineditable', 1, 5, 'EXTERNAL', 'ENTITY', '{"SERVICE_ACTIVATION"}', true, NULL, false, NULL, NULL, 'PROVEEDOR', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'WIKITY', false, 'BACKOFFICE', 'ACTIVE'),
('CP_TEL_VODA_09', 'Confirmación de Proveedor — Contacto con Cliente', 'Llamada de proveedor (10-15 min) con escalamiento por grupos', 1, 6, 'EXTERNAL', 'ENTITY', '{}', true, NULL, false, NULL, NULL, 'PROVEEDOR', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'AGENDO', false, 'BACKOFFICE,SUPERVISOR', 'ACTIVE'),
('CP_TEL_VODA_10', 'Gestión de Fraude — Validación Externa', 'Rastrear y mapear verificación de fraude en Validación Externa', 1, 6, 'EXTERNAL', 'ENTITY', '{}', false, NULL, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'AGENDO', false, 'BACKOFFICE,SUPERVISOR', 'ACTIVE'),
('CP_TEL_VODA_11', 'Fraude Confirmado — Sin Remedio', 'Fraude definitivo sin posibilidad de subsanación. Finaliza ciclo de vida.', 1, 6, 'EXTERNAL', 'ENTITY', '{"COMMISSION","SERVICE_ACTIVATION"}', true, NULL, false, NULL, NULL, 'BACKOFFICE', 'TELECOM_VODAFONE', 'VODAFONE_SOLIVESA', 'AGENDO', true, 'BACKOFFICE,GERENCIA', 'ACTIVE')
ON CONFLICT (code) DO UPDATE SET
name = EXCLUDED.name, description = EXCLUDED.description, trigger_stage_id = EXCLUDED.trigger_stage_id, origin = EXCLUDED.origin, blocks = EXCLUDED.blocks, blocks_advance = EXCLUDED.blocks_advance, rollback_to_stage = EXCLUDED.rollback_to_stage, is_recurrent = EXCLUDED.is_recurrent, recurrence_days = EXCLUDED.recurrence_days, max_occurrences = EXCLUDED.max_occurrences, owner_dept = EXCLUDED.owner_dept, portfolio = EXCLUDED.portfolio, campaign = EXCLUDED.campaign, provider = EXCLUDED.provider, finalizes_cycle = EXCLUDED.finalizes_cycle, target_roles = EXCLUDED.target_roles, approval_status = EXCLUDED.approval_status;

-- 4. MECANISMO GENÉRICO (1 Checkpoint de Recuperación)
INSERT INTO checkpoint_catalog 
(code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, rollback_to_stage, is_recurrent, recurrence_days, max_occurrences, owner_dept, portfolio, campaign, provider, finalizes_cycle, target_roles, approval_status)
VALUES
('CP_POSTVENTA_RECUP', 'Gestión de Recuperación', 'Disparo automático por KO en satisfacción para retención de cliente', 1, 10, 'INTERNAL', 'ENTITY', '{"COMMISSION"}', true, NULL, false, NULL, NULL, 'POSTVENTA', 'GENERAL', 'GENERAL', 'INTERNO', false, 'POSTVENTA,SUPERVISOR', 'ACTIVE')
ON CONFLICT (code) DO UPDATE SET
name = EXCLUDED.name, description = EXCLUDED.description, trigger_stage_id = EXCLUDED.trigger_stage_id, origin = EXCLUDED.origin, blocks = EXCLUDED.blocks, blocks_advance = EXCLUDED.blocks_advance, rollback_to_stage = EXCLUDED.rollback_to_stage, is_recurrent = EXCLUDED.is_recurrent, recurrence_days = EXCLUDED.recurrence_days, max_occurrences = EXCLUDED.max_occurrences, owner_dept = EXCLUDED.owner_dept, portfolio = EXCLUDED.portfolio, campaign = EXCLUDED.campaign, provider = EXCLUDED.provider, finalizes_cycle = EXCLUDED.finalizes_cycle, target_roles = EXCLUDED.target_roles, approval_status = EXCLUDED.approval_status;

-- 5. PASOS SECUENCIALES (Checkpoint Steps)
INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 1, '1) Verificar ficha completa', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_01'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 2, '2) Llamar al cliente para pasar validación', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_01'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 1, '1) Registrar corrección indicada por el cliente', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_03'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 2, '2) Informar a Operaciones', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_03'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 1, '1) Trasladar observación a Operaciones', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_04'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 2, '2) Esperar confirmación de qué datos usar', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_04'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 1, '1) Actualizar campos si es necesario', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_07'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 2, '2) Revisar que coincida con el CRM de GSI antes de lanzar', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_07'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 1, '1) El proveedor llama al cliente (10-15 min)', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_09'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;

INSERT INTO checkpoint_step (id_checkpoint, step_order, name, is_required)
SELECT id_checkpoint, 2, '2) Si pasan >15 min, Backoffice escala por grupos', true FROM checkpoint_catalog WHERE code = 'CP_TEL_VODA_09'
ON CONFLICT (id_checkpoint, step_order) DO UPDATE SET name = EXCLUDED.name;
