-- ==========================================================
-- NYX FLOW ENGINE STANDALONE — DDL & MASTER DATA
-- JERARQUÍA OFICIAL: CICLOS -> ETAPAS -> CHECKPOINTS -> CANVAS
-- ==========================================================

CREATE SCHEMA IF NOT EXISTS nyx_flow;

-- 1. TABLA TOPE: CICLOS
CREATE TABLE IF NOT EXISTS nyx_flow.cycle_definition (
    id_cycle BIGSERIAL PRIMARY KEY,
    cycle_code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    description TEXT,
    scope_type VARCHAR(50) NOT NULL DEFAULT 'COMMERCIAL',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    current_version INT NOT NULL DEFAULT 1,
    entry_policy_json JSONB NOT NULL DEFAULT '{}',
    exit_policy_json JSONB NOT NULL DEFAULT '{}',
    created_by BIGINT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 2. TABLA: ETAPAS DEL CICLO
CREATE TABLE IF NOT EXISTS nyx_flow.cycle_stage (
    id_stage BIGSERIAL PRIMARY KEY,
    id_cycle BIGINT NOT NULL REFERENCES nyx_flow.cycle_definition(id_cycle) ON DELETE CASCADE,
    stage_code VARCHAR(50) NOT NULL,
    name VARCHAR(150) NOT NULL,
    description TEXT,
    order_index SMALLINT NOT NULL DEFAULT 1,
    is_terminal BOOLEAN NOT NULL DEFAULT FALSE,
    sla_hours SMALLINT DEFAULT NULL,
    policies_json JSONB NOT NULL DEFAULT '{}',
    CONSTRAINT uk_stage_cycle_code UNIQUE (id_cycle, stage_code)
);

-- 3. TABLA: CATÁLOGO DE CHECKPOINTS
CREATE TABLE IF NOT EXISTS nyx_flow.checkpoint_catalog (
    id_checkpoint BIGSERIAL PRIMARY KEY,
    id_cycle BIGINT NOT NULL REFERENCES nyx_flow.cycle_definition(id_cycle) ON DELETE CASCADE,
    trigger_stage_id BIGINT REFERENCES nyx_flow.cycle_stage(id_stage) ON DELETE SET NULL,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    description TEXT,
    origin VARCHAR(20) NOT NULL DEFAULT 'INTERNAL',
    scope VARCHAR(20) NOT NULL DEFAULT 'ENTITY',
    blocks_advance BOOLEAN NOT NULL DEFAULT TRUE,
    finalizes_cycle BOOLEAN NOT NULL DEFAULT FALSE,
    triggered_by_ko BIGINT DEFAULT NULL REFERENCES nyx_flow.checkpoint_catalog(id_checkpoint) ON DELETE SET NULL,
    rollback_to_stage BIGINT DEFAULT NULL REFERENCES nyx_flow.cycle_stage(id_stage),
    is_recurrent BOOLEAN NOT NULL DEFAULT FALSE,
    recurrence_days SMALLINT DEFAULT NULL,
    activation_trigger VARCHAR(50) NOT NULL DEFAULT 'IMMEDIATE',
    delay_days INT DEFAULT NULL,
    precondition_fact TEXT DEFAULT NULL,
    template_schema_json JSONB NOT NULL DEFAULT '{}',
    policies_json JSONB NOT NULL DEFAULT '{}',
    providers_json JSONB NOT NULL DEFAULT '["Genérico"]',
    allowed_actions_json JSONB NOT NULL DEFAULT '[]',
    branching_rules_json JSONB NOT NULL DEFAULT '{}',
    category VARCHAR(50) NOT NULL DEFAULT 'GENERAL',
    owner_dept VARCHAR(50) NOT NULL DEFAULT 'Asesor',
    execution_order INT NOT NULL DEFAULT 1,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    version INT NOT NULL DEFAULT 1,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 4. TABLA: PASOS DE CHECKPOINT
CREATE TABLE IF NOT EXISTS nyx_flow.checkpoint_step (
    id_step BIGSERIAL PRIMARY KEY,
    id_checkpoint BIGINT NOT NULL REFERENCES nyx_flow.checkpoint_catalog(id_checkpoint) ON DELETE CASCADE,
    step_order SMALLINT NOT NULL DEFAULT 1,
    name VARCHAR(255) NOT NULL,
    is_required BOOLEAN NOT NULL DEFAULT TRUE
);

-- 5. TABLA: CATÁLOGOS DINÁMICOS DE METADATOS
CREATE TABLE IF NOT EXISTS nyx_flow.meta_role (
    id_role SERIAL PRIMARY KEY,
    role_code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    external_system_code VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS nyx_flow.meta_portfolio (
    id_portfolio SERIAL PRIMARY KEY,
    portfolio_code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    external_system_code VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS nyx_flow.meta_campaign (
    id_campaign SERIAL PRIMARY KEY,
    campaign_code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    external_system_code VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 6. TABLA: POLÍTICAS Y REGLAS DE ACTUACIÓN
CREATE TABLE IF NOT EXISTS nyx_flow.cycle_policy_rule (
    id_rule BIGSERIAL PRIMARY KEY,
    rule_code VARCHAR(50) NOT NULL UNIQUE,
    id_cycle BIGINT REFERENCES nyx_flow.cycle_definition(id_cycle) ON DELETE CASCADE,
    name VARCHAR(150) NOT NULL,
    description TEXT,
    entity_type VARCHAR(50) NOT NULL,
    action_trigger VARCHAR(50) NOT NULL,
    rule_definition_json JSONB NOT NULL DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 7. TABLA: INSTANCIAS ACTIVAS DE CICLO
CREATE TABLE IF NOT EXISTS nyx_flow.cycle_instance (
    id_instance BIGSERIAL PRIMARY KEY,
    id_cycle BIGINT NOT NULL REFERENCES nyx_flow.cycle_definition(id_cycle),
    entity_type VARCHAR(50) NOT NULL,
    entity_id BIGINT NOT NULL,
    current_stage_id BIGINT NOT NULL REFERENCES nyx_flow.cycle_stage(id_stage),
    owner_actor_id BIGINT DEFAULT NULL,
    current_actor_id BIGINT DEFAULT NULL,
    handshake_status VARCHAR(50) NOT NULL DEFAULT 'NONE',
    handshake_target_actor_id BIGINT DEFAULT NULL,
    handshake_requested_at TIMESTAMPTZ DEFAULT NULL,
    day_counter INT NOT NULL DEFAULT 1,
    metadata JSONB NOT NULL DEFAULT '{}',
    facts JSONB NOT NULL DEFAULT '{}',
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ DEFAULT NULL
);

-- 8. TABLA: INSTANCIAS DE CHECKPOINT
CREATE TABLE IF NOT EXISTS nyx_flow.checkpoint_instance (
    id_cp_instance BIGSERIAL PRIMARY KEY,
    id_instance BIGINT NOT NULL REFERENCES nyx_flow.cycle_instance(id_instance) ON DELETE CASCADE,
    id_checkpoint BIGINT NOT NULL REFERENCES nyx_flow.checkpoint_catalog(id_checkpoint),
    status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    opened_at_stage BIGINT REFERENCES nyx_flow.cycle_stage(id_stage),
    scheduled_for TIMESTAMPTZ DEFAULT NULL,
    resolved_by BIGINT DEFAULT NULL,
    resolved_at TIMESTAMPTZ DEFAULT NULL,
    answers_json JSONB NOT NULL DEFAULT '{}',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 9. TABLA: PROGRESO DE PASOS
CREATE TABLE IF NOT EXISTS nyx_flow.checkpoint_step_progress (
    id_progress BIGSERIAL PRIMARY KEY,
    id_cp_instance BIGINT NOT NULL REFERENCES nyx_flow.checkpoint_instance(id_cp_instance) ON DELETE CASCADE,
    id_step BIGINT NOT NULL REFERENCES nyx_flow.checkpoint_step(id_step) ON DELETE CASCADE,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    completed_by BIGINT DEFAULT NULL,
    completed_at TIMESTAMPTZ DEFAULT NULL,
    CONSTRAINT uk_cp_instance_step UNIQUE (id_cp_instance, id_step)
);

-- 10. TABLA: TRANSICIONES DE ETAPA
CREATE TABLE IF NOT EXISTS nyx_flow.stage_transition (
    id_transition BIGSERIAL PRIMARY KEY,
    id_instance BIGINT NOT NULL REFERENCES nyx_flow.cycle_instance(id_instance) ON DELETE CASCADE,
    from_stage_id BIGINT REFERENCES nyx_flow.cycle_stage(id_stage),
    to_stage_id BIGINT NOT NULL REFERENCES nyx_flow.cycle_stage(id_stage),
    direction VARCHAR(20) NOT NULL DEFAULT 'FORWARD',
    triggered_by VARCHAR(100) DEFAULT NULL,
    actor_id BIGINT DEFAULT NULL,
    transitioned_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- 11. TABLA: LOG DE AUDITORÍA SHA-512
CREATE TABLE IF NOT EXISTS nyx_flow.cycle_audit_log (
    id_log BIGSERIAL PRIMARY KEY,
    id_instance BIGINT REFERENCES nyx_flow.cycle_instance(id_instance) ON DELETE SET NULL,
    id_checkpoint BIGINT REFERENCES nyx_flow.checkpoint_catalog(id_checkpoint) ON DELETE SET NULL,
    action VARCHAR(100) NOT NULL,
    actor_id BIGINT NOT NULL,
    detail JSONB NOT NULL DEFAULT '{}',
    checksum VARCHAR(128) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ==========================================================
-- DATOS SEMILLA (METADATOS, CICLO Y ETAPAS)
-- ==========================================================
INSERT INTO nyx_flow.meta_role (role_code, name, description)
VALUES 
    ('Asesor', 'Asesor Comercial / Front', 'Gestión de primer contacto y cierre'),
    ('Supervisor', 'Supervisor de Turno / TM', 'Gobernanza y autorizaciones'),
    ('Backoffice', 'Mesa de Backoffice / Operaciones', 'Tramitación y validaciones'),
    ('Calidad', 'Auditoría de Calidad', 'Revisión de llamadas y cumplimiento'),
    ('Operaciones', 'Operaciones y Logística', 'Despacho y agendamiento'),
    ('Postventa', 'Atención Postventa y Retenciones', 'Resolución y retención'),
    ('Proveedor', 'Instalador / Tercero Externo', 'Técnico de campo')
ON CONFLICT (role_code) DO NOTHING;

INSERT INTO nyx_flow.meta_portfolio (portfolio_code, name, description)
VALUES 
    ('Telecom', 'Telecomunicaciones (Fibra / Móvil)', 'Operadores de telecomunicaciones'),
    ('Energia', 'Energía (Luz / Gas)', 'Comercializadoras energéticas'),
    ('Alarma', 'Seguridad y Alarmas', 'Sistemas de seguridad y domótica'),
    ('Seguros', 'Seguros y Pólizas', 'Pólizas de salud, hogar y autos'),
    ('ENTITY', 'Genérico / Multi-cartera', 'Ámbito transversal')
ON CONFLICT (portfolio_code) DO NOTHING;

INSERT INTO nyx_flow.meta_campaign (campaign_code, name, description)
VALUES 
    ('GENERAL', 'Campaña General / Transversal', 'Campaña comercial estándar'),
    ('PORTABILIDAD_FIBRA', 'Portabilidad Fibra + Móvil', 'Campañas de captación y portabilidad fija/móvil'),
    ('RETENCION_INBOUND', 'Retención Inbound', 'Campañas de fidelización y retención'),
    ('RENOVACION_UPGRADE', 'Renovación y Upgrade', 'Mejora de velocidad y terminales'),
    ('ENERGIA_DUAL', 'Dual Luz + Gas Residencial', 'Campañas de suministro de energía')
ON CONFLICT (campaign_code) DO NOTHING;

INSERT INTO nyx_flow.cycle_definition (id_cycle, cycle_code, name, description, scope_type, is_active)
VALUES (1, 'CYCLE_SALES_TELECOM', 'Ciclo de Ventas Telecomunicaciones', 'Pipeline integral de prospección, venta y verificación de servicios telecom', 'COMMERCIAL', TRUE)
ON CONFLICT (cycle_code) DO NOTHING;

INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES 
    (1, 1, 'STAGE_RECEPTION', '1. Recepción & Cualificación', 1, FALSE, 2),
    (2, 1, 'STAGE_CONTACT_HANDSHAKE', '2. Contacto & Handshake', 2, FALSE, 4),
    (3, 1, 'STAGE_COVERAGE_VERIF', '3. Verificación Cobertura Fibra', 3, FALSE, 24),
    (4, 1, 'STAGE_CONTRACT_SIGN', '4. Contratación & Firma Digital', 4, FALSE, 12),
    (5, 1, 'STAGE_INSTALLATION', '5. Provisión & Cierre', 5, TRUE, 48)
ON CONFLICT (id_cycle, stage_code) DO NOTHING;

INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description, blocks_advance, category, owner_dept, execution_order
) VALUES 
    (11, 1, 1, 'CP11_ACCEPT_FILE', 'CP#11 Aceptación de Fichero', 'Presentar Subida completada al registrar pre-venta', FALSE, 'OPERACIONES', 'Asesor', 1),
    (12, 1, 2, 'CP12_CALL_HANDSHAKE', 'CP#12 Handshake Telefónico', 'Confirmación de recepción de llamada y titularidad', TRUE, 'CTI', 'Asesor', 2),
    (13, 1, 3, 'CP13_FIBER_COVERAGE', 'CP#13 Factibilidad Técnica Fibra', 'Validación CTO y velocidad de puerto GPON', TRUE, 'TECNICA', 'Backoffice', 3),
    (14, 1, 4, 'CP14_DIGITAL_SIGN', 'CP#14 Firma Digital del Contrato', 'Firma OTP vía SMS o Canvas Pad', TRUE, 'LEGAL', 'Asesor', 4),
    (15, 1, 5, 'CP15_POST_D30_AUDIT', 'CP#15 Auditoría D+30 Calidad', 'Verificación de primera factura pagada', FALSE, 'CALIDAD', 'Calidad', 5)
ON CONFLICT (code) DO NOTHING;
