-- ========================================================
-- Nyx Flow Engine â€” Database Schema (nyx_flow)
-- Autonomous Lifecycle Stages & Checkpoint Governance Engine
-- ========================================================

-- Enable extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Flow Definition (Pipeline Definition)
CREATE TABLE IF NOT EXISTS flow_definition (
    id_flow           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code              VARCHAR(80) NOT NULL UNIQUE,          -- e.g. "PIPELINE_ALARMAS", "PIPELINE_TELECOM"
    name              VARCHAR(200) NOT NULL,
    description       TEXT,
    scope_type        VARCHAR(50) DEFAULT 'CAMPAIGN',       -- CAMPAIGN | ORGANIZATION | GLOBAL
    scope_id          BIGINT,
    is_active         BOOLEAN NOT NULL DEFAULT true,
    current_version   INT NOT NULL DEFAULT 1,
    created_by        BIGINT NOT NULL DEFAULT 1,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Pipeline Stages
CREATE TABLE IF NOT EXISTS stage (
    id_stage          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_flow           BIGINT NOT NULL REFERENCES flow_definition(id_flow) ON DELETE CASCADE,
    stage_code        VARCHAR(80) NOT NULL,
    name              VARCHAR(200) NOT NULL,
    description       TEXT,
    order_index       SMALLINT NOT NULL,
    is_terminal       BOOLEAN NOT NULL DEFAULT false,
    portfolio         VARCHAR(100) DEFAULT 'GENERAL',
    campaign          VARCHAR(100) DEFAULT 'GENERAL',
    sla_hours         SMALLINT,
    metadata          JSONB NOT NULL DEFAULT '{}',
    CONSTRAINT uq_flow_stage_code UNIQUE (id_flow, stage_code),
    CONSTRAINT uq_flow_stage_order UNIQUE (id_flow, order_index)
);

-- 3. Checkpoint Catalog (Layer 1: Governed Definitions with Triple Approval)
CREATE TABLE IF NOT EXISTS checkpoint_catalog (
    id_checkpoint     BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code              VARCHAR(80) NOT NULL UNIQUE,
    name              VARCHAR(200) NOT NULL,
    description       TEXT,
    id_flow           BIGINT REFERENCES flow_definition(id_flow),  -- NULL = applies globally to all flows
    trigger_stage_id  BIGINT REFERENCES stage(id_stage),           -- Stage that triggers this checkpoint
    origin            VARCHAR(20) NOT NULL DEFAULT 'INTERNAL',     -- INTERNAL | EXTERNAL (provider)
    scope             VARCHAR(20) NOT NULL DEFAULT 'ENTITY',       -- ENTITY (sale) | ITEM
    blocks            TEXT[] NOT NULL DEFAULT '{}',                -- {'COMMISSION','LIQUIDATION','SERVICE_ACTIVATION'}
    blocks_advance    BOOLEAN NOT NULL DEFAULT false,               -- Impedes stage advance while pending
    rollback_to_stage BIGINT REFERENCES stage(id_stage),           -- Rollback target if KO
    triggered_by_ko   BIGINT REFERENCES checkpoint_catalog(id_checkpoint), -- Chained trigger on KO of another
    is_recurrent      BOOLEAN NOT NULL DEFAULT false,
    recurrence_days   SMALLINT,
    max_occurrences   SMALLINT,
    owner_dept        VARCHAR(100),                                -- Responsible department
    approval_status   VARCHAR(20) NOT NULL DEFAULT 'PROPOSED',    -- PROPOSED | ACTIVE | DEPRECATED
    approved_by       JSONB NOT NULL DEFAULT '[]',                -- Array of signoff signatures
    is_active         BOOLEAN NOT NULL DEFAULT true,
    version           INT NOT NULL DEFAULT 1,
    created_by        BIGINT NOT NULL DEFAULT 1,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_checkpoint_approval CHECK (approval_status IN ('PROPOSED','ACTIVE','DEPRECATED'))
);

-- 4. Checkpoint Internal Steps (Layer 1b)
CREATE TABLE IF NOT EXISTS checkpoint_step (
    id_step           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_checkpoint     BIGINT NOT NULL REFERENCES checkpoint_catalog(id_checkpoint) ON DELETE CASCADE,
    step_order        SMALLINT NOT NULL,
    name              VARCHAR(300) NOT NULL,
    is_required       BOOLEAN NOT NULL DEFAULT true,
    CONSTRAINT uq_checkpoint_step_order UNIQUE (id_checkpoint, step_order)
);

-- 5. Flow Instances (Layer 2: Realtime Entity Pipelines)
CREATE TABLE IF NOT EXISTS flow_instance (
    id_instance       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_flow           BIGINT NOT NULL REFERENCES flow_definition(id_flow),
    entity_type       VARCHAR(100) NOT NULL,                -- "sales_order", "lead", "onboarding"
    entity_id         BIGINT NOT NULL,
    current_stage_id  BIGINT NOT NULL REFERENCES stage(id_stage),
    day_counter       INT NOT NULL DEFAULT 1,              -- Simulated day / tracking counter
    facts             JSONB NOT NULL DEFAULT '{}',
    metadata          JSONB NOT NULL DEFAULT '{}',
    status            VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',-- ACTIVE | COMPLETED | CANCELLED
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at      TIMESTAMPTZ,
    CONSTRAINT uq_flow_instance_entity UNIQUE (id_flow, entity_type, entity_id)
);

ALTER TABLE flow_instance ADD COLUMN IF NOT EXISTS facts JSONB DEFAULT '{}';

-- 5b. Flow Campaign Mapping
CREATE TABLE IF NOT EXISTS flow_campaign_mapping (
    id_cmpg           BIGINT PRIMARY KEY,
    flow_code         VARCHAR(80) NOT NULL,
    description       VARCHAR(100) NOT NULL DEFAULT '',
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO flow_campaign_mapping (id_cmpg, flow_code, description) VALUES
    (1, 'PIPELINE_TELECOM', 'Vodafone'),
    (2, 'PIPELINE_TELECOM', 'Lowi'),
    (3, 'PIPELINE_TELECOM', 'Yoigo'),
    (4, 'PIPELINE_TELECOM', 'MÃ¡sMÃ³vil'),
    (5, 'PIPELINE_TELECOM', 'Orange')
ON CONFLICT (id_cmpg) DO UPDATE SET flow_code = EXCLUDED.flow_code, description = EXCLUDED.description;

-- 6. Checkpoint Instances (Layer 2: Live Active Checkpoints)
CREATE TABLE IF NOT EXISTS checkpoint_instance (
    id_cp_instance    BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_instance       BIGINT NOT NULL REFERENCES flow_instance(id_instance) ON DELETE CASCADE,
    id_checkpoint     BIGINT NOT NULL REFERENCES checkpoint_catalog(id_checkpoint),
    status            VARCHAR(20) NOT NULL DEFAULT 'PENDING',-- PENDING | SUBSANADO | KO | SCHEDULED
    opened_at_stage   BIGINT REFERENCES stage(id_stage),
    is_retroactive    BOOLEAN NOT NULL DEFAULT false,
    occurrence_number SMALLINT NOT NULL DEFAULT 1,
    scheduled_for     TIMESTAMPTZ,
    resolved_by       BIGINT,
    resolved_at       TIMESTAMPTZ,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_cp_instance_status CHECK (status IN ('PENDING','SUBSANADO','KO','SCHEDULED'))
);

-- 7. Checkpoint Step Progress (Layer 2b)
CREATE TABLE IF NOT EXISTS checkpoint_step_progress (
    id_progress       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_cp_instance    BIGINT NOT NULL REFERENCES checkpoint_instance(id_cp_instance) ON DELETE CASCADE,
    id_step           BIGINT NOT NULL REFERENCES checkpoint_step(id_step),
    is_completed      BOOLEAN NOT NULL DEFAULT false,
    completed_by      BIGINT,
    completed_at      TIMESTAMPTZ,
    CONSTRAINT uq_cp_step_progress UNIQUE (id_cp_instance, id_step)
);

-- 8. Stage Transitions Log
CREATE TABLE IF NOT EXISTS stage_transition (
    id_transition     BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_instance       BIGINT NOT NULL REFERENCES flow_instance(id_instance) ON DELETE CASCADE,
    from_stage_id     BIGINT REFERENCES stage(id_stage),
    to_stage_id       BIGINT NOT NULL REFERENCES stage(id_stage),
    direction         VARCHAR(10) NOT NULL DEFAULT 'FORWARD',-- FORWARD | BACKWARD | SKIP
    triggered_by      VARCHAR(50),                          -- USER | CHECKPOINT_KO | SYSTEM
    actor_id          BIGINT,
    transitioned_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 9. Immutable Audit Trail
CREATE TABLE IF NOT EXISTS audit_log (
    id_log            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_instance       BIGINT,
    id_checkpoint     BIGINT,
    action            VARCHAR(50) NOT NULL,                 -- INSTANCED | STAGE_ADVANCED | CHECKPOINT_RESOLVED | ROLLBACK
    actor_id          BIGINT NOT NULL,
    actor_ip          INET,
    detail            JSONB NOT NULL DEFAULT '{}',
    checksum          VARCHAR(128) NOT NULL,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_flow_inst_entity ON flow_instance(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_flow_inst_stage ON flow_instance(current_stage_id);
CREATE INDEX IF NOT EXISTS idx_cp_inst_instance ON checkpoint_instance(id_instance);
CREATE INDEX IF NOT EXISTS idx_cp_inst_status ON checkpoint_instance(status);

-- Seed Data: Sample Alarmas & Telecom Flows & Stages
INSERT INTO flow_definition (code, name, description, scope_type)
VALUES 
    ('PIPELINE_ALARMAS', 'Pipeline EstÃ¡ndar de Alarmas', 'Flujo de venta de 10 etapas con checkpoints de calidad y proveedor', 'CAMPAIGN'),
    ('PIPELINE_TELECOM', 'Pipeline EstÃ¡ndar Telecom', 'Flujo Telecom â€” comparte etapas con Alarmas', 'CAMPAIGN')
ON CONFLICT (code) DO NOTHING;

INSERT INTO stage (id_flow, stage_code, name, order_index, is_terminal, portfolio, campaign) VALUES
    (1, 'PREVENTA', 'Preventa', 1, false, 'GENERAL', 'GENERAL'),
    (1, 'VENTA_CREADA', 'Venta Creada', 2, false, 'GENERAL', 'GENERAL'),
    (1, 'GESTION_INICIAL', 'GestiÃ³n Inicial', 3, false, 'GENERAL', 'GENERAL'),
    (1, 'VALIDACION_INTERNA', 'ValidaciÃ³n Interna', 4, false, 'GENERAL', 'GENERAL'),
    (1, 'ENVIO_PROVEEDOR', 'EnvÃ­o Proveedor', 5, false, 'GENERAL', 'GENERAL'),
    (1, 'VALIDACION_EXTERNA', 'ValidaciÃ³n Externa', 6, false, 'GENERAL', 'GENERAL'),
    (1, 'FIRMA', 'Firma', 7, false, 'GENERAL', 'GENERAL'),
    (1, 'TRAMITACION', 'TramitaciÃ³n', 8, false, 'GENERAL', 'GENERAL'),
    (1, 'ACTIVACION', 'ActivaciÃ³n', 9, false, 'GENERAL', 'GENERAL'),
    (1, 'POSTVENTA', 'Postventa', 10, false, 'GENERAL', 'GENERAL'),
    (1, 'CERRADA', 'Cerrada', 11, true, 'GENERAL', 'GENERAL')
ON CONFLICT (id_flow, stage_code) DO NOTHING;

-- Copy stages to PIPELINE_TELECOM
INSERT INTO stage (id_flow, stage_code, name, order_index, is_terminal, portfolio, campaign)
SELECT f.id_flow, s.stage_code, s.name, s.order_index, s.is_terminal, s.portfolio, s.campaign
FROM stage s
CROSS JOIN flow_definition f
WHERE s.id_flow = (SELECT id_flow FROM flow_definition WHERE code = 'PIPELINE_ALARMAS')
  AND f.code = 'PIPELINE_TELECOM'
ON CONFLICT (id_flow, stage_code) DO NOTHING;

-- 10. Status Stage Mapping (CRM id_status -> FlowEngine id_stage)
CREATE TABLE IF NOT EXISTS status_stage_mapping (
    id_status   INT NOT NULL,
    id_stage    BIGINT NOT NULL REFERENCES stage(id_stage),
    PRIMARY KEY (id_status)
);

INSERT INTO status_stage_mapping (id_status, id_stage) VALUES
    (1, 1),   -- Borrador -> Preventa
    (2, 2),   -- En revisiÃ³n supervisor -> Venta Creada
    (3, 4),   -- En BackOffice -> ValidaciÃ³n Interna
    (4, 3),   -- En gestiÃ³n -> GestiÃ³n Inicial
    (5, 5),   -- Enviado al proveedor -> EnvÃ­o Proveedor
    (6, 6),   -- Validado por proveedor -> ValidaciÃ³n Externa
    (7, 7),   -- Contrato enviado -> Firma
    (8, 7),   -- Contrato firmado -> Firma
    (9, 9),   -- Activado -> ActivaciÃ³n
    (10, 4),  -- Enviado KO -> ValidaciÃ³n Interna (rollback)
    (11, 4),  -- En incidencia -> ValidaciÃ³n Interna
    (15, 9)   -- Reportado -> ActivaciÃ³n
ON CONFLICT (id_status) DO UPDATE SET id_stage = EXCLUDED.id_stage;

-- Seed Checkpoints in Catalog (Layer 1)
INSERT INTO checkpoint_catalog (code, name, description, id_flow, trigger_stage_id, origin, scope, blocks, blocks_advance, owner_dept, approval_status) VALUES
    ('CP_AUDIO_AUDIT', 'AuditorÃ­a de audio', 'RevisiÃ³n y ediciÃ³n de tramos de audio de sustento de oferta', 1, 9, 'INTERNAL', 'ENTITY', ARRAY['COMMISSION','SERVICE_ACTIVATION'], false, 'Finanzas', 'ACTIVE'),
    ('CP_SUPERVISOR_REV', 'RevisiÃ³n de supervisor', 'Visto bueno del supervisor en gestiÃ³n inicial', 1, 3, 'INTERNAL', 'ENTITY', ARRAY[]::text[], true, 'Operaciones', 'ACTIVE'),
    ('CP_CONTRACT_SIGN', 'ConfirmaciÃ³n de firma de contrato', 'VerificaciÃ³n de firma digital del proveedor', 1, 7, 'EXTERNAL', 'ENTITY', ARRAY['LIQUIDATION','COMMISSION'], true, 'Backoffice', 'ACTIVE')
ON CONFLICT (code) DO NOTHING;

-- Reasignar todos los checkpoints Telecom al flow 2 con sus etapas correspondientes (12-22)
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 12 WHERE code = 'CP_TEL_SHIRLEY_01';
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 13 WHERE code = 'CP_TEL_SHIRLEY_02';
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 14 WHERE code IN ('CP_SUPERVISOR_REV','CP_TEL_SHIRLEY_03');
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 15 WHERE code IN ('CP_TEL_SHIRLEY_04','CP_TEL_VODA_01','CP_TEL_VODA_02','CP_TEL_VODA_03','CP_TEL_VODA_04','CP_TEL_VODA_05');
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 16 WHERE code IN ('CP_TEL_SHIRLEY_05','CP_TEL_VODA_06','CP_TEL_VODA_07','CP_TEL_VODA_08');
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 17 WHERE code IN ('CP_TEL_VODA_09','CP_TEL_VODA_10','CP_TEL_VODA_11');
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 18 WHERE code = 'CP_CONTRACT_SIGN';
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 20 WHERE code = 'CP_AUDIO_AUDIT';
UPDATE checkpoint_catalog SET id_flow = 2, trigger_stage_id = 21 WHERE code IN ('CP_TEL_SHIRLEY_06','CP_POSTVENTA_RECUP');

INSERT INTO checkpoint_step (id_checkpoint, step_order, name) VALUES
    (1, 1, 'Sacar todos los audios de la llamada'),
    (1, 2, 'Identificar tramos que sustentan la oferta'),
    (1, 3, 'Editar/compilar el audio de sustento'),
    (1, 4, 'Enviar sustento por correo al proveedor'),
    (1, 5, 'Esperar respuesta del proveedor')
ON CONFLICT (id_checkpoint, step_order) DO NOTHING;
