-- ============================================================================
-- SCRIPT PARA ELIMINAR ETAPAS DUPLICADAS Y UNIFICAR EL FLUJO EN NYX_FLOW
-- ============================================================================

BEGIN;

-- 1. Actualizar status_stage_mapping para que apunte a las etapas activas (12..22)
UPDATE status_stage_mapping SET id_stage = 12 WHERE id_stage = 1;
UPDATE status_stage_mapping SET id_stage = 13 WHERE id_stage = 2;
UPDATE status_stage_mapping SET id_stage = 14 WHERE id_stage = 3;
UPDATE status_stage_mapping SET id_stage = 15 WHERE id_stage = 4;
UPDATE status_stage_mapping SET id_stage = 16 WHERE id_stage = 5;
UPDATE status_stage_mapping SET id_stage = 17 WHERE id_stage = 6;
UPDATE status_stage_mapping SET id_stage = 18 WHERE id_stage = 7;
UPDATE status_stage_mapping SET id_stage = 19 WHERE id_stage = 8;
UPDATE status_stage_mapping SET id_stage = 20 WHERE id_stage = 9;
UPDATE status_stage_mapping SET id_stage = 21 WHERE id_stage = 10;
UPDATE status_stage_mapping SET id_stage = 22 WHERE id_stage = 11;

-- 2. Actualizar flow_instance para que apunte a las etapas 12..22 y flujo 2
UPDATE flow_instance SET current_stage_id = current_stage_id + 11, id_flow = 2 WHERE current_stage_id BETWEEN 1 AND 11;

-- 3. Actualizar checkpoint_instance para que apunte a etapas 12..22
UPDATE checkpoint_instance SET opened_at_stage = opened_at_stage + 11 WHERE opened_at_stage BETWEEN 1 AND 11;

-- 4. Actualizar stage_transition para que apunte a etapas 12..22
UPDATE stage_transition SET from_stage_id = from_stage_id + 11 WHERE from_stage_id BETWEEN 1 AND 11;
UPDATE stage_transition SET to_stage_id = to_stage_id + 11 WHERE to_stage_id BETWEEN 1 AND 11;

-- 5. Actualizar checkpoint_catalog por si algún rollback_to_stage apuntaba a 1..11
UPDATE checkpoint_catalog SET rollback_to_stage = rollback_to_stage + 11 WHERE rollback_to_stage BETWEEN 1 AND 11;
UPDATE checkpoint_catalog SET trigger_stage_id = NULL WHERE trigger_stage_id BETWEEN 1 AND 11;

-- 6. Eliminar las 11 etapas duplicadas antiguas (1..11) del flujo 1
DELETE FROM stage WHERE id_stage BETWEEN 1 AND 11;

-- 7. Desactivar o eliminar flujo 1 heredado
UPDATE flow_definition SET is_active = false WHERE id_flow = 1;

COMMIT;
