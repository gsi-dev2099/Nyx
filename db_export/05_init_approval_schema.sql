-- ========================================================
-- Nyx Approval Engine â€” Database Schema (nyx_approval)
-- ISO 9001:2015 & ISO 27001:2022 Compliant Approval Engine
-- ========================================================

-- Enable extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. Approval Policy (Governed Template)
CREATE TABLE IF NOT EXISTS policy (
    id_policy         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code              VARCHAR(80) NOT NULL UNIQUE,          -- e.g. "SALES_DISCOUNT_GT_15PCT"
    name              VARCHAR(200) NOT NULL,
    description       TEXT,
    scope_type        VARCHAR(50) NOT NULL DEFAULT 'GLOBAL', -- GLOBAL | ORGANIZATION | DIVISION | CAMPAIGN
    scope_id          BIGINT,
    is_active         BOOLEAN NOT NULL DEFAULT true,
    current_version   INT NOT NULL DEFAULT 1,
    created_by        BIGINT NOT NULL DEFAULT 1,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Policy Versions (ISO 9001 Document Control)
CREATE TABLE IF NOT EXISTS policy_version (
    id_version        BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_policy         BIGINT NOT NULL REFERENCES policy(id_policy) ON DELETE CASCADE,
    version_number    INT NOT NULL,
    change_reason     TEXT NOT NULL,
    snapshot_json     JSONB NOT NULL,                       -- Complete JSON snapshot of the chain configuration
    published_by      BIGINT NOT NULL,
    published_at      TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_policy_version UNIQUE (id_policy, version_number)
);

-- 3. Approval Chain (Sequence Configuration)
CREATE TABLE IF NOT EXISTS chain (
    id_chain          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_policy         BIGINT NOT NULL REFERENCES policy(id_policy) ON DELETE CASCADE,
    chain_mode        VARCHAR(20) NOT NULL DEFAULT 'SEQUENTIAL', -- SEQUENTIAL | PARALLEL | ANY_ONE | UNANIMOUS
    max_sla_hours     SMALLINT,                             -- Max SLA before escalation
    on_timeout_action VARCHAR(30) DEFAULT 'ESCALATE'        -- ESCALATE | AUTO_APPROVE | AUTO_REJECT
);

-- 4. Chain Steps (Individual Approver Steps)
CREATE TABLE IF NOT EXISTS chain_step (
    id_step           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_chain          BIGINT NOT NULL REFERENCES chain(id_chain) ON DELETE CASCADE,
    step_order        SMALLINT NOT NULL,
    approver_type     VARCHAR(30) NOT NULL,                 -- USER | ROLE | DIVISION | POSITION | CONDITIONAL
    approver_ref      VARCHAR(200) NOT NULL,                -- User ID, Role Code, Division ID, etc.
    condition_expr    JSONB,                                -- Dynamic evaluation: {"field":"amount","op":">","value":5000}
    can_delegate      BOOLEAN NOT NULL DEFAULT true,        -- Delegation allowed
    sla_hours         SMALLINT,
    is_optional       BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT uq_chain_step UNIQUE (id_chain, step_order)
);

-- 5. Approval Requests (Live Execution Instances)
CREATE TABLE IF NOT EXISTS request (
    id_request        BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_policy         BIGINT NOT NULL REFERENCES policy(id_policy),
    policy_version    INT NOT NULL DEFAULT 1,
    entity_type       VARCHAR(100) NOT NULL,                -- "sales_order", "purchase_order", "user_access"
    entity_id         BIGINT NOT NULL,
    entity_context    JSONB NOT NULL DEFAULT '{}',
    status            VARCHAR(20) NOT NULL DEFAULT 'PENDING',-- PENDING | IN_PROGRESS | APPROVED | REJECTED | ESCALATED | EXPIRED | CANCELLED
    current_step      SMALLINT NOT NULL DEFAULT 1,
    requested_by      BIGINT NOT NULL,
    callback_url      VARCHAR(500),
    expires_at        TIMESTAMPTZ,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    resolved_at       TIMESTAMPTZ,
    CONSTRAINT chk_request_status CHECK (status IN ('PENDING','IN_PROGRESS','APPROVED','REJECTED','ESCALATED','EXPIRED','CANCELLED'))
);

-- 6. Individual Decisions (Approval / Rejection Audit)
CREATE TABLE IF NOT EXISTS decision (
    id_decision       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_request        BIGINT NOT NULL REFERENCES request(id_request) ON DELETE CASCADE,
    step_order        SMALLINT NOT NULL,
    decided_by        BIGINT NOT NULL,                      -- Actual decision maker
    original_approver BIGINT,                               -- Original approver if delegated
    decision          VARCHAR(20) NOT NULL,                 -- APPROVED | REJECTED | ESCALATED
    reason            TEXT,
    evidence_path     VARCHAR(500),                         -- Document or proof attachment
    decided_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_decision_type CHECK (decision IN ('APPROVED','REJECTED','ESCALATED'))
);

-- 7. Temporary Delegations of Authority (ISO 27001 SoD)
CREATE TABLE IF NOT EXISTS delegation (
    id_delegation     BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    delegator_id      BIGINT NOT NULL,                      -- Who delegates authority
    delegate_id       BIGINT NOT NULL,                      -- Who receives authority
    id_policy         BIGINT REFERENCES policy(id_policy),  -- NULL = all policies
    reason            TEXT NOT NULL,
    valid_from        TIMESTAMPTZ NOT NULL,
    valid_until       TIMESTAMPTZ NOT NULL,
    is_active         BOOLEAN NOT NULL DEFAULT true,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 8. Immutable Audit Trail (ISO 27001 / SOX Compliance)
CREATE TABLE IF NOT EXISTS audit_log (
    id_log            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_request        BIGINT,
    id_policy         BIGINT,
    action            VARCHAR(50) NOT NULL,                 -- CREATED | DECIDED | ESCALATED | DELEGATED | EXPIRED | POLICY_UPDATED
    actor_id          BIGINT NOT NULL,
    actor_ip          INET,
    actor_user_agent  VARCHAR(500),
    detail            JSONB NOT NULL DEFAULT '{}',
    checksum          VARCHAR(128) NOT NULL,                -- SHA-512 record integrity checksum
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexing for performance and security lookups
CREATE INDEX IF NOT EXISTS idx_appr_req_status ON request(status);
CREATE INDEX IF NOT EXISTS idx_appr_req_entity ON request(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_appr_req_user ON request(requested_by);
CREATE INDEX IF NOT EXISTS idx_appr_dec_req ON decision(id_request);
CREATE INDEX IF NOT EXISTS idx_appr_del_users ON delegation(delegator_id, delegate_id) WHERE is_active = true;

-- Default Policy Seed
INSERT INTO policy (code, name, description, scope_type)
VALUES 
    ('APPROVAL_HIGH_DISCOUNT', 'AprobaciÃ³n de Descuentos Mayores a 15%', 'Requiere visto bueno de Supervisor y Gerencia Financiera', 'GLOBAL'),
    ('APPROVAL_ORDER_CANCELLATION', 'AprobaciÃ³n de AnulaciÃ³n de Pedidos', 'Requiere aprobaciÃ³n de Operaciones', 'GLOBAL')
ON CONFLICT (code) DO NOTHING;

INSERT INTO chain (id_policy, chain_mode, max_sla_hours)
VALUES (1, 'SEQUENTIAL', 48)
ON CONFLICT DO NOTHING;

INSERT INTO chain_step (id_chain, step_order, approver_type, approver_ref, can_delegate, sla_hours)
VALUES 
    (1, 1, 'ROLE', 'SUPERVISOR', true, 24),
    (1, 2, 'DIVISION', 'FINANZAS', true, 24)
ON CONFLICT DO NOTHING;
