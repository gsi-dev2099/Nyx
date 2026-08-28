-- ============================================================================
-- SCRIPT DE ACTUALIZACION Y CORRECCION DE ETAPAS Y CHECKPOINTS (GSI AUDITORIA)
-- Base de datos: nyx_flow
-- ============================================================================

BEGIN;

-- 1. Desactivar o desvincular checkpoints heredados/duplicados y basura de prueba
UPDATE checkpoint_catalog 
SET trigger_stage_id = NULL, is_active = false, id_flow = NULL
WHERE id_checkpoint IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 64);

DELETE FROM checkpoint_catalog WHERE id_checkpoint IN (5, 6);

-- 2. Asegurar que los 57 checkpoints de GSI estén activos, vinculados al flujo 2 (Telecom) con sus etapas y órdenes exactos

-- ETAPA 1: PREVENTA (id_stage = 12) -> 7 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 1, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 11; -- CP_TEL_011 (Aceptación de Fichero)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 2, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 12; -- CP_TEL_012 (Preventa — 1ª Llamada)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 3, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 13; -- CP_TEL_013 (Preventa — 2ª Llamada)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 4, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 14; -- CP_TEL_014 (Preventa — 3ª Llamada)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 5, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 15; -- CP_TEL_015 (Preventa — Alerta Cambio/Retencion)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 6, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = 15,   is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 75; -- CP_TEL_075 (Gestion Botada)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12, execution_order = 7, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = 75,   is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 76; -- CP_TEL_076 (Gestion Compañias Alternas)

-- ETAPA 2: VENTA CREADA (id_stage = 13) -> 6 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13, execution_order = 1, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 16; -- CP_TEL_016 (Toma de Datos Personales)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13, execution_order = 2, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 79; -- CP_TEL_079 (Pre Carga Verificacion Scoring Venta)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13, execution_order = 3, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 18; -- CP_TEL_018 (Verificación de Score Cliente)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13, execution_order = 4, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 77; -- CP_TEL_077 (Verificacion Cobertura)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13, execution_order = 5, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 78; -- CP_TEL_078 (Verificacion Robinson)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13, execution_order = 6, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = 79,   is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 80; -- CP_TEL_080 (Gestion Scoring)

-- ETAPA 3: GESTION INICIAL (id_stage = 14) -> 1 Checkpoint
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 14, execution_order = 1, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 13, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 19; -- CP_TEL_019 (Revisión de Supervisor)

-- ETAPA 4: VALIDACION INTERNA (id_stage = 15) -> 6 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15, execution_order = 1, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 14, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 23; -- CP_TEL_023 (Scoring Datos)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15, execution_order = 2, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 14, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 30; -- CP_TEL_030 (Revisión ficha de datos)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15, execution_order = 3, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 32; -- CP_TEL_032 (Revisión funcionalidad)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15, execution_order = 4, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 31; -- CP_TEL_031 (Llamada de Validación)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15, execution_order = 5, blocks_advance = false, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 73; -- CP_TEL_073 (Llamada Filtro Alarma)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15, execution_order = 6, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 33; -- CP_TEL_033 (Correcciones Operación)

-- ETAPA 5: ENVIO PROVEEDOR (id_stage = 16) -> 9 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 1, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 20; -- CP_TEL_020 (Envío Carga Solivesa)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 2, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 21; -- CP_TEL_021 (Envío Carga Leyash)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 3, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 18, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 22; -- CP_TEL_022 (Confirmación Contrato)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 4, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 15, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 34; -- CP_TEL_034 (Confirmación carga)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 5, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 15, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 35; -- CP_TEL_035 (Confirmación datos)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 6, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 16, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 36; -- CP_TEL_036 (Carga sistema prov.)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 7, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, rollback_to_stage = 16, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 37; -- CP_TEL_037 (Verificación final)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 8, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 38; -- CP_TEL_038 (Programación cita)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16, execution_order = 9, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = 20,   is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 74; -- CP_TEL_074 (Gestion Recuperacion Pre)

-- ETAPA 6: VALIDACION EXTERNA (id_stage = 17) -> 7 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 1, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 39; -- CP_TEL_039 (Verificación contacto)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 2, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 40; -- CP_TEL_040 (Revisión obs prov.)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 3, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 41; -- CP_TEL_041 (ID subsanable BO/Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 4, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 42; -- CP_TEL_042 (Obs. subsanada BO)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 5, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 43; -- CP_TEL_043 (Obs. subsanada Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 6, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 44; -- CP_TEL_044 (Val. externa fin)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17, execution_order = 7, blocks_advance = true, finalizes_cycle = true,  triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 70; -- CP_TEL_070 (Fraude Definitivo)

-- ETAPA 7: FIRMA (id_stage = 18) -> 6 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18, execution_order = 1, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 45; -- CP_TEL_045 (Verificar contrato env)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18, execution_order = 2, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 46; -- CP_TEL_046 (Info a Operaciones)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18, execution_order = 3, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 47; -- CP_TEL_047 (Confirma firma Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18, execution_order = 4, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 48; -- CP_TEL_048 (Verificar firma Smart)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18, execution_order = 5, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 49; -- CP_TEL_049 (Confirma firma a Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18, execution_order = 6, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 50; -- CP_TEL_050 (Envío constancia)

-- ETAPA 8: TRAMITACION (id_stage = 19) -> 7 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 1, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 51; -- CP_TEL_051 (Verificar cita)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 2, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 52; -- CP_TEL_052 (Seguimiento instal.)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 3, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 53; -- CP_TEL_053 (ID incidencias pre)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 4, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 54; -- CP_TEL_054 (Seguimiento inc prov)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 5, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 55; -- CP_TEL_055 (Traslado inc a Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 6, blocks_advance = false, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 56; -- CP_TEL_056 (Confirmación subs. Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 19, execution_order = 7, blocks_advance = true,  finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 57; -- CP_TEL_057 (Confirma instalación)

-- ETAPA 9: ACTIVACION (id_stage = 20) -> 6 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20, execution_order = 1, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 58; -- CP_TEL_058 (Seguimiento Activ.)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20, execution_order = 2, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 59; -- CP_TEL_059 (ID incidencias post)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20, execution_order = 3, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 60; -- CP_TEL_060 (Seguimiento inc prov)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20, execution_order = 4, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 61; -- CP_TEL_061 (Traslado inc Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20, execution_order = 5, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 62; -- CP_TEL_062 (Confirmación Op)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20, execution_order = 6, blocks_advance = true, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 63; -- CP_TEL_063 (Confirma Activ. Full)

-- ETAPA 10: POSTVENTA (id_stage = 21) -> 2 Checkpoints
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 21, execution_order = 1, blocks_advance = false, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 24; -- CP_TEL_024 (Postventa Seguimiento)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 21, execution_order = 2, blocks_advance = false, finalizes_cycle = false, triggered_by_ko = NULL, is_active = true, approval_status = 'ACTIVE' WHERE id_checkpoint = 72; -- CP_TEL_072 (Gestión Recup Post)

COMMIT;
