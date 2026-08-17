-- ========================================================
-- Nyx SLA Engine — Database Schema (nyx_sla)
-- Autonomous SLA Engine for Multi-Department Tracking
-- ========================================================

-- Enable extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. SLA Policies (Configurable per Context)
CREATE TABLE IF NOT EXISTS sla_policy (
    id_policy       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code            VARCHAR(80) NOT NULL UNIQUE,
    name            VARCHAR(200) NOT NULL,
    description     TEXT,
    scope_type      VARCHAR(50) NOT NULL DEFAULT 'GLOBAL',  -- GLOBAL | CAMPAIGN | DIVISION | USER
    scope_id        BIGINT,
    target_minutes  INT NOT NULL,                           -- Target SLA in minutes
    warning_pct     SMALLINT NOT NULL DEFAULT 75,           -- % for warning alert (yellow)
    critical_pct    SMALLINT NOT NULL DEFAULT 100,          -- % for critical breach (red)
    escalation_pct  SMALLINT DEFAULT 120,                   -- % for auto-escalation
    applies_to      VARCHAR(50) NOT NULL DEFAULT 'ORDER',   -- ORDER | LEAD | INCIDENT | CUSTOM
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_by      BIGINT NOT NULL DEFAULT 1,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Work Calendars & Shifts
CREATE TABLE IF NOT EXISTS work_calendar (
    id_calendar     BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code            VARCHAR(80) NOT NULL UNIQUE,
    name            VARCHAR(200) NOT NULL,
    timezone        VARCHAR(50) NOT NULL DEFAULT 'America/Lima',
    is_default      BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS work_schedule (
    id_schedule     BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_calendar     BIGINT NOT NULL REFERENCES work_calendar(id_calendar) ON DELETE CASCADE,
    day_of_week     SMALLINT NOT NULL CHECK (day_of_week BETWEEN 0 AND 6), -- 0=Sunday, 6=Saturday
    start_time      TIME NOT NULL,
    end_time        TIME NOT NULL,
    UNIQUE (id_calendar, day_of_week)
);

CREATE TABLE IF NOT EXISTS holiday (
    id_holiday      BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_calendar     BIGINT NOT NULL REFERENCES work_calendar(id_calendar) ON DELETE CASCADE,
    holiday_date    DATE NOT NULL,
    name            VARCHAR(200) NOT NULL,
    is_half_day     BOOLEAN NOT NULL DEFAULT false,
    UNIQUE (id_calendar, holiday_date)
);

CREATE TABLE IF NOT EXISTS user_work_shifts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    id_user         BIGINT NOT NULL,
    day_of_week     INT NOT NULL CHECK (day_of_week BETWEEN 0 AND 6),
    start_time      TIME NOT NULL,
    end_time        TIME NOT NULL,
    is_active       BOOLEAN DEFAULT true,
    created_at      TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_user_day UNIQUE (id_user, day_of_week)
);

-- 3. SLA Measurements (Realtime Instances)
CREATE TABLE IF NOT EXISTS sla_measurement (
    id_measurement  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_policy       BIGINT NOT NULL REFERENCES sla_policy(id_policy),
    entity_type     VARCHAR(100) NOT NULL,                  -- "order", "incident", "lead"
    entity_id       BIGINT NOT NULL,
    owner_user_id   BIGINT,
    started_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    paused_at       TIMESTAMPTZ,
    resolved_at     TIMESTAMPTZ,
    elapsed_minutes INT NOT NULL DEFAULT 0,
    status          VARCHAR(20) NOT NULL DEFAULT 'RUNNING',  -- RUNNING | PAUSED | WARNING | BREACHED | COMPLETED
    breach_at       TIMESTAMPTZ,
    metadata        JSONB NOT NULL DEFAULT '{}',
    CONSTRAINT uq_sla_measurement UNIQUE (id_policy, entity_type, entity_id)
);

-- 4. SLA Alerts & Escalations
CREATE TABLE IF NOT EXISTS sla_alert (
    id_alert        BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_measurement  BIGINT NOT NULL REFERENCES sla_measurement(id_measurement) ON DELETE CASCADE,
    alert_level     VARCHAR(20) NOT NULL,                    -- WARNING | CRITICAL | BREACH | ESCALATED
    notified_to     BIGINT,
    callback_sent   BOOLEAN NOT NULL DEFAULT false,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 5. Immutable Audit Trail (ISO 27001)
CREATE TABLE IF NOT EXISTS sla_audit_log (
    id_log          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_measurement  BIGINT,
    id_policy       BIGINT,
    action          VARCHAR(50) NOT NULL,                    -- CREATED | PAUSED | RESUMED | RESOLVED | BREACHED | ESCALATED
    actor_id        BIGINT NOT NULL,
    detail          JSONB NOT NULL DEFAULT '{}',
    checksum        VARCHAR(128) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexing for high throughput queries
CREATE INDEX IF NOT EXISTS idx_sla_meas_status ON sla_measurement(status);
CREATE INDEX IF NOT EXISTS idx_sla_meas_entity ON sla_measurement(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_sla_meas_owner ON sla_measurement(owner_user_id);
CREATE INDEX IF NOT EXISTS idx_sla_alert_meas ON sla_alert(id_measurement);

-- Default Work Calendar Seed Data
INSERT INTO work_calendar (code, name, timezone, is_default)
VALUES ('DEFAULT_PE', 'Horario Estándar Perú', 'America/Lima', true)
ON CONFLICT (code) DO NOTHING;

INSERT INTO work_schedule (id_calendar, day_of_week, start_time, end_time)
VALUES 
    (1, 1, '08:00:00', '18:00:00'), -- Mon
    (1, 2, '08:00:00', '18:00:00'), -- Tue
    (1, 3, '08:00:00', '18:00:00'), -- Wed
    (1, 4, '08:00:00', '18:00:00'), -- Thu
    (1, 5, '08:00:00', '18:00:00'), -- Fri
    (1, 6, '09:00:00', '13:00:00')  -- Sat
ON CONFLICT (id_calendar, day_of_week) DO NOTHING;

-- Default Policy Seed
INSERT INTO sla_policy (code, name, description, scope_type, target_minutes, warning_pct, critical_pct, applies_to)
VALUES 
    ('SLA_INCIDENT_CRITICAL', 'SLA Incidencias Críticas', 'Resolución de incidencias críticas en menos de 2 horas', 'GLOBAL', 120, 75, 100, 'INCIDENT'),
    ('SLA_SALES_VALIDATION', 'SLA Validación Interna de Ventas', 'Validación por backoffice en menos de 24 horas hábiles', 'GLOBAL', 1440, 80, 100, 'ORDER')
ON CONFLICT (code) DO NOTHING;
