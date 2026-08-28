-- ==========================================================================
-- NYX FLOW ENGINE — CARGA MAESTRA DE 57 CHECKPOINTS Y 10 ETAPAS (GSI BACKUP)
-- Base de datos: nyx_flow
-- ==========================================================================

BEGIN;

-- 0. TABLAS DE METADATOS Y CATÁLOGOS DINÁMICOS
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

CREATE TABLE IF NOT EXISTS nyx_flow.checkpoint_step (
    id_step BIGSERIAL PRIMARY KEY,
    id_checkpoint BIGINT NOT NULL REFERENCES nyx_flow.checkpoint_catalog(id_checkpoint) ON DELETE CASCADE,
    step_order SMALLINT NOT NULL DEFAULT 1,
    name VARCHAR(255) NOT NULL,
    is_required BOOLEAN NOT NULL DEFAULT TRUE
);

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

-- 1. REGISTRO DEL CICLO DE VENTAS TELECOM
INSERT INTO nyx_flow.cycle_definition (id_cycle, cycle_code, name, description, scope_type, is_active)
VALUES (1, 'CYCLE_SALES_TELECOM', 'Ciclo de Ventas Telecomunicaciones', 'Pipeline comercial y técnico integral (10 Etapas, 57 Checkpoints GSI)', 'COMMERCIAL', TRUE)
ON CONFLICT (cycle_code) DO NOTHING;

-- 2. REGISTRO DE LAS 10 ETAPAS CANÓNICAS
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (1, 1, 'STAGE_PREVENTA', '1. Preventa', 1, FALSE, 2)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (2, 1, 'STAGE_VENTA_CREADA', '2. Venta Creada', 2, FALSE, 4)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (3, 1, 'STAGE_GESTION_INICIAL', '3. Gestión Inicial', 3, FALSE, 4)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (4, 1, 'STAGE_VALIDACION_INTERNA', '4. Validación Interna', 4, FALSE, 12)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (5, 1, 'STAGE_ENVIO_PROVEEDOR', '5. Envío Proveedor', 5, FALSE, 24)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (6, 1, 'STAGE_VALIDACION_EXTERNA', '6. Validación Externa', 6, FALSE, 24)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (7, 1, 'STAGE_FIRMA', '7. Firma', 7, FALSE, 12)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (8, 1, 'STAGE_TRAMITACION', '8. Tramitación', 8, FALSE, 48)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (9, 1, 'STAGE_ACTIVACION', '9. Activación', 9, FALSE, 48)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;
INSERT INTO nyx_flow.cycle_stage (id_stage, id_cycle, stage_code, name, order_index, is_terminal, sla_hours)
VALUES (10, 1, 'STAGE_POSTVENTA', '10. Postventa', 10, TRUE, 72)
ON CONFLICT (id_cycle, stage_code) DO UPDATE 
SET name = EXCLUDED.name, order_index = EXCLUDED.order_index, is_terminal = EXCLUDED.is_terminal, sla_hours = EXCLUDED.sla_hours;

-- 3. REGISTRO Y ACTUALIZACIÓN DE LOS 57 CHECKPOINTS DE GSI
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    11, 1, 1, 'CP_TEL_011', 'Aceptación de Fichero', 'Presentar Subida',
    'Shirley+Dayana', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 1, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    12, 1, 1, 'CP_TEL_012', 'Preventa — 1ª Llamada', 'Llamar de 1ª compañía 
No recibir aceptación de la oferta',
    'Shirley+Dayana', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 2, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    13, 1, 1, 'CP_TEL_013', 'Preventa — 2ª Llamada', 'Escucha 2ª compañía',
    'Shirley+Dayana', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 3, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    14, 1, 1, 'CP_TEL_014', 'Preventa — 3ª Llamada', 'Llamar por 3ra compañia',
    'Shirley+Dayana', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 4, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    15, 1, 1, 'CP_TEL_015', 'Preventa — Alerta Cambio/Retencion ', 'Informar Alerta de cambio
Confirmar aceptación de cambio',
    'Shirley+Dayana', 'Telecom', TRUE, FALSE, 75, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 5, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    16, 1, 2, 'CP_TEL_016', 'Toma de Datos Personales', 'Solicitar Nombre Completo (Titular / Origen)
Solicitar Dirección Completa
Solicitar Cuenta Bancaria
Solicitar NIF/NIE/CIF (Titular / Origen)
Solicitar Correo Electrónico
Solicitar Fecha Nacimiento
Solicitar Operador Actual
Solicitar Tipo de Contrato
Solicitar Tipo Cliente
Solicitar/Confirmar Teléfono Contacto
Solicitar Movil Principal
Solicitar Números a Portar (Fijos/Moviles)
Confirmar Proveedor
Confirmar Oferta: Tecnología + Promoción + Velocidad + Descuento + Tarifas Móviles + Convergentes + Líneas Adicionales + Plataforma + Permanencia',
    'Shirley+Dayana', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 6, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    79, 1, 2, 'CP_TEL_079', 'Pre Carga Verificacion Scoring Venta', 'Precargar al CRM de la Campaña
Confirmar Validacion Scoring',
    'Nuevo', 'Telecom', TRUE, FALSE, 80, NULL,
    'IMMEDIATE', 'Telecom', 'BackOffice', 7, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    18, 1, 2, 'CP_TEL_018', 'Verificación de Score Cliente', 'Verificar Scoring',
    'Solo Dayana', 'Telecom', TRUE, FALSE, 80, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 8, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    77, 1, 2, 'CP_TEL_077', 'Verificacion Cobertura', 'Verificar Cobertura
',
    'Nuevo', 'Telecom', TRUE, FALSE, 76, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 9, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    78, 1, 2, 'CP_TEL_078', 'Verificacion Robinson', 'Verificar Robinson',
    'Nuevo', 'Telecom', TRUE, FALSE, 76, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 10, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    19, 1, 3, 'CP_TEL_019', 'Revisión de Supervisor', '1) Revisar el CRM
2) Verificar cumplimiento del checkpoint "Creación CRM GSI"
3) Identificar errores
4) Devolver al asesor si corresponde
5) Autorizar envío a Validación Perú
',
    'Shirley+Dayana', 'Telecom', TRUE, FALSE, NULL, 2,
    'IMMEDIATE', 'Telecom', 'Supervisor', 11, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    20, 1, 5, 'CP_TEL_020', 'Envío Carga Solivesa', 'Llamar Cliente por Linea Filtro (10 Min)
Corroborar Datos Personales
Corroborar Oferta
Fidelización',
    'Shirley+Dayana', 'Telecom', TRUE, FALSE, 74, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 12, TRUE, '["Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    21, 1, 5, 'CP_TEL_021', 'Envío Carga Leyash', 'Llamar Cliente por OCM (7 Min) / Goautodial
Corroborar Datos Personales
Corroborar Oferta
Fidelización',
    'Shirley+Dayana', 'Telecom', TRUE, FALSE, 74, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 13, TRUE, '["Leyash"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    22, 1, 5, 'CP_TEL_022', 'Confirmación Contrato', '',
    'Solo Dayana', 'Telecom', TRUE, FALSE, NULL, 7,
    'IMMEDIATE', 'Telecom', 'Proveedor', 14, FALSE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    23, 1, 4, 'CP_TEL_023', 'Scoring Datos', '',
    'Solo Dayana', 'Telecom', TRUE, FALSE, NULL, 3,
    'IMMEDIATE', 'Telecom', 'Calidad', 15, FALSE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    24, 1, 10, 'CP_TEL_024', 'Postventa Seguimiento', '',
    'Shirley+Dayana', 'Telecom', FALSE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Auxiliar', 16, FALSE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    30, 1, 4, 'CP_TEL_030', 'Revisión ficha de datos', 'Verificar CRM',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, 3,
    'IMMEDIATE', 'Telecom', 'Backoffice', 17, TRUE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    32, 1, 4, 'CP_TEL_032', 'Revisión funcionalidad', 'Verificar funcionalidad OCM',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 18, TRUE, '["LEYASH"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    31, 1, 4, 'CP_TEL_031', 'Llamada de Validación', 'Confirmación de Datos personales
Confirmación de Oferta
Confirmación de preguntas antifraude
',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 19, TRUE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    73, 1, 4, 'CP_TEL_073', 'Llamada Filtro Alarma', 'Validar Servicio De Alarma',
    'Creado en vivo', 'Telecom', FALSE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 20, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    33, 1, 4, 'CP_TEL_033', 'Correcciones Operación', 'Aplicar Correcciones CRM
Trasladar correcciones Operaciones',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 21, TRUE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    34, 1, 5, 'CP_TEL_034', 'Confirmación carga', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, 4,
    'IMMEDIATE', 'Telecom', 'Operaciones', 22, TRUE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    35, 1, 5, 'CP_TEL_035', 'Confirmación datos', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, 4,
    'IMMEDIATE', 'Telecom', 'Backoffice', 23, TRUE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    36, 1, 5, 'CP_TEL_036', 'Carga sistema prov.', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, 5,
    'IMMEDIATE', 'Telecom', 'Backoffice', 24, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    37, 1, 5, 'CP_TEL_037', 'Verificación final', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, 5,
    'IMMEDIATE', 'Telecom', 'Backoffice', 25, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    38, 1, 5, 'CP_TEL_038', 'Programación cita', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 26, FALSE, '["Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    39, 1, 6, 'CP_TEL_039', 'Verificación contacto', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 27, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    40, 1, 6, 'CP_TEL_040', 'Revisión obs prov.', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 28, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    41, 1, 6, 'CP_TEL_041', 'ID subsanable BO/Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 29, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    42, 1, 6, 'CP_TEL_042', 'Obs. subsanada BO', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 30, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    43, 1, 6, 'CP_TEL_043', 'Obs. subsanada Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Operaciones', 31, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    44, 1, 6, 'CP_TEL_044', 'Val. externa fin', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 32, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    45, 1, 7, 'CP_TEL_045', 'Verificar contrato env', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 33, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    46, 1, 7, 'CP_TEL_046', 'Info a Operaciones', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 34, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    47, 1, 7, 'CP_TEL_047', 'Confirma firma Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Operaciones', 35, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    48, 1, 7, 'CP_TEL_048', 'Verificar firma Smart', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 36, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    49, 1, 7, 'CP_TEL_049', 'Confirma firma a Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 37, FALSE, '["Leyash", "Solivesa"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    50, 1, 7, 'CP_TEL_050', 'Envío constancia', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 38, FALSE, '["Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    51, 1, 8, 'CP_TEL_051', 'Verificar cita', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 39, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    52, 1, 8, 'CP_TEL_052', 'Seguimiento instal.', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 40, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    53, 1, 8, 'CP_TEL_053', 'ID incidencias pre', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 41, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    54, 1, 8, 'CP_TEL_054', 'Seguimiento inc prov', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 42, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    55, 1, 8, 'CP_TEL_055', 'Traslado inc a Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 43, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    56, 1, 8, 'CP_TEL_056', 'Confirmación subs. Op', '',
    'Backoffice', 'Telecom', FALSE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Operaciones', 44, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    57, 1, 8, 'CP_TEL_057', 'Confirma instalación', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 45, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    58, 1, 9, 'CP_TEL_058', 'Seguimiento Activ.', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 46, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    59, 1, 9, 'CP_TEL_059', 'ID incidencias post', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 47, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    60, 1, 9, 'CP_TEL_060', 'Seguimiento inc prov', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 48, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    61, 1, 9, 'CP_TEL_061', 'Traslado inc Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 49, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    62, 1, 9, 'CP_TEL_062', 'Confirmación Op', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Operaciones', 50, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    63, 1, 9, 'CP_TEL_063', 'Confirma Activ. Full', '',
    'Backoffice', 'Telecom', TRUE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 51, FALSE, '["Leyash", "Solivesa", "Yoigo"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    70, 1, 6, 'CP_TEL_070', 'Fraude Definitivo', '',
    'Data', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Backoffice', 52, FALSE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    72, 1, 10, 'CP_TEL_072', 'Gestión Recup Post', '',
    'Data', 'Telecom', FALSE, FALSE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Postventa', 53, FALSE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    74, 1, 5, 'CP_TEL_074', 'Gestion Recuperacion Pre', '',
    'Creado en vivo', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Operación', 54, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    75, 1, 1, 'CP_TEL_075', 'Gestion Botada', 'Presionar Cambio de Compañía.
Confirmar nombre de compañía de cambio.
',
    'Nuevo', 'Telecom', TRUE, FALSE, 76, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 55, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    76, 1, 1, 'CP_TEL_076', 'Gestion Compañias Alternas', 'Ofrecer Campañas Alternas vigentes
Confirmar Aceptación',
    'Nuevo', 'Telecom', TRUE, TRUE, NULL, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 56, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;
INSERT INTO nyx_flow.checkpoint_catalog (
    id_checkpoint, id_cycle, trigger_stage_id, code, name, description,
    origin, scope, blocks_advance, finalizes_cycle, triggered_by_ko, rollback_to_stage,
    activation_trigger, category, owner_dept, execution_order, is_active, providers_json
) VALUES (
    80, 1, 2, 'CP_TEL_080', 'Gestion Scoring', 'Cambiar titularidad por Familiar/Tercero
Cambiar Cuenta Bancaria
Cambiar Dirección
',
    'Nuevo', 'Telecom', TRUE, FALSE, 76, NULL,
    'IMMEDIATE', 'Telecom', 'Asesor', 57, TRUE, '["Genérico"]'::jsonb
) ON CONFLICT (code) DO UPDATE SET
    id_cycle = EXCLUDED.id_cycle,
    trigger_stage_id = EXCLUDED.trigger_stage_id,
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    origin = EXCLUDED.origin,
    scope = EXCLUDED.scope,
    blocks_advance = EXCLUDED.blocks_advance,
    finalizes_cycle = EXCLUDED.finalizes_cycle,
    triggered_by_ko = EXCLUDED.triggered_by_ko,
    rollback_to_stage = EXCLUDED.rollback_to_stage,
    owner_dept = EXCLUDED.owner_dept,
    execution_order = EXCLUDED.execution_order,
    is_active = EXCLUDED.is_active,
    providers_json = EXCLUDED.providers_json;

COMMIT;
