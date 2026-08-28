--
-- PostgreSQL database dump
--

\restrict kqJqP92HylgqYjVRP6OPLftiri2o0ut4k0PDBnK2XYNMgCE8hSqLvH3rW5jqYmS

-- Dumped from database version 16.14
-- Dumped by pg_dump version 16.15

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public.request DROP CONSTRAINT IF EXISTS request_id_policy_fkey;
ALTER TABLE IF EXISTS ONLY public.policy_version DROP CONSTRAINT IF EXISTS policy_version_id_policy_fkey;
ALTER TABLE IF EXISTS ONLY public.delegation DROP CONSTRAINT IF EXISTS delegation_id_policy_fkey;
ALTER TABLE IF EXISTS ONLY public.decision DROP CONSTRAINT IF EXISTS decision_id_request_fkey;
ALTER TABLE IF EXISTS ONLY public.chain_step DROP CONSTRAINT IF EXISTS chain_step_id_chain_fkey;
ALTER TABLE IF EXISTS ONLY public.chain DROP CONSTRAINT IF EXISTS chain_id_policy_fkey;
DROP INDEX IF EXISTS public.idx_appr_req_user;
DROP INDEX IF EXISTS public.idx_appr_req_status;
DROP INDEX IF EXISTS public.idx_appr_req_entity;
DROP INDEX IF EXISTS public.idx_appr_del_users;
DROP INDEX IF EXISTS public.idx_appr_dec_req;
ALTER TABLE IF EXISTS ONLY public.policy_version DROP CONSTRAINT IF EXISTS uq_policy_version;
ALTER TABLE IF EXISTS ONLY public.chain_step DROP CONSTRAINT IF EXISTS uq_chain_step;
ALTER TABLE IF EXISTS ONLY public.request DROP CONSTRAINT IF EXISTS request_pkey;
ALTER TABLE IF EXISTS ONLY public.policy_version DROP CONSTRAINT IF EXISTS policy_version_pkey;
ALTER TABLE IF EXISTS ONLY public.policy DROP CONSTRAINT IF EXISTS policy_pkey;
ALTER TABLE IF EXISTS ONLY public.policy DROP CONSTRAINT IF EXISTS policy_code_key;
ALTER TABLE IF EXISTS ONLY public.delegation DROP CONSTRAINT IF EXISTS delegation_pkey;
ALTER TABLE IF EXISTS ONLY public.decision DROP CONSTRAINT IF EXISTS decision_pkey;
ALTER TABLE IF EXISTS ONLY public.chain_step DROP CONSTRAINT IF EXISTS chain_step_pkey;
ALTER TABLE IF EXISTS ONLY public.chain DROP CONSTRAINT IF EXISTS chain_pkey;
ALTER TABLE IF EXISTS ONLY public.audit_log DROP CONSTRAINT IF EXISTS audit_log_pkey;
DROP TABLE IF EXISTS public.request;
DROP TABLE IF EXISTS public.policy_version;
DROP TABLE IF EXISTS public.policy;
DROP TABLE IF EXISTS public.delegation;
DROP TABLE IF EXISTS public.decision;
DROP TABLE IF EXISTS public.chain_step;
DROP TABLE IF EXISTS public.chain;
DROP TABLE IF EXISTS public.audit_log;
DROP EXTENSION IF EXISTS "uuid-ossp";
--
-- Name: uuid-ossp; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS "uuid-ossp" WITH SCHEMA public;


--
-- Name: EXTENSION "uuid-ossp"; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION "uuid-ossp" IS 'generate universally unique identifiers (UUIDs)';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: audit_log; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.audit_log (
    id_log bigint NOT NULL,
    id_request bigint,
    id_policy bigint,
    action character varying(50) NOT NULL,
    actor_id bigint NOT NULL,
    actor_ip inet,
    actor_user_agent character varying(500),
    detail jsonb DEFAULT '{}'::jsonb NOT NULL,
    checksum character varying(128) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: audit_log_id_log_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.audit_log ALTER COLUMN id_log ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.audit_log_id_log_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: chain; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.chain (
    id_chain bigint NOT NULL,
    id_policy bigint NOT NULL,
    chain_mode character varying(20) DEFAULT 'SEQUENTIAL'::character varying NOT NULL,
    max_sla_hours smallint,
    on_timeout_action character varying(30) DEFAULT 'ESCALATE'::character varying
);


--
-- Name: chain_id_chain_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.chain ALTER COLUMN id_chain ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.chain_id_chain_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: chain_step; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.chain_step (
    id_step bigint NOT NULL,
    id_chain bigint NOT NULL,
    step_order smallint NOT NULL,
    approver_type character varying(30) NOT NULL,
    approver_ref character varying(200) NOT NULL,
    condition_expr jsonb,
    can_delegate boolean DEFAULT true NOT NULL,
    sla_hours smallint,
    is_optional boolean DEFAULT false NOT NULL
);


--
-- Name: chain_step_id_step_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.chain_step ALTER COLUMN id_step ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.chain_step_id_step_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: decision; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.decision (
    id_decision bigint NOT NULL,
    id_request bigint NOT NULL,
    step_order smallint NOT NULL,
    decided_by bigint NOT NULL,
    original_approver bigint,
    decision character varying(20) NOT NULL,
    reason text,
    evidence_path character varying(500),
    decided_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT chk_decision_type CHECK (((decision)::text = ANY (ARRAY[('APPROVED'::character varying)::text, ('REJECTED'::character varying)::text, ('ESCALATED'::character varying)::text])))
);


--
-- Name: decision_id_decision_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.decision ALTER COLUMN id_decision ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.decision_id_decision_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: delegation; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.delegation (
    id_delegation bigint NOT NULL,
    delegator_id bigint NOT NULL,
    delegate_id bigint NOT NULL,
    id_policy bigint,
    reason text NOT NULL,
    valid_from timestamp with time zone NOT NULL,
    valid_until timestamp with time zone NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: delegation_id_delegation_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.delegation ALTER COLUMN id_delegation ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.delegation_id_delegation_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: policy; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.policy (
    id_policy bigint NOT NULL,
    code character varying(80) NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    scope_type character varying(50) DEFAULT 'GLOBAL'::character varying NOT NULL,
    scope_id bigint,
    is_active boolean DEFAULT true NOT NULL,
    current_version integer DEFAULT 1 NOT NULL,
    created_by bigint DEFAULT 1 NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: policy_id_policy_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.policy ALTER COLUMN id_policy ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.policy_id_policy_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: policy_version; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.policy_version (
    id_version bigint NOT NULL,
    id_policy bigint NOT NULL,
    version_number integer NOT NULL,
    change_reason text NOT NULL,
    snapshot_json jsonb NOT NULL,
    published_by bigint NOT NULL,
    published_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: policy_version_id_version_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.policy_version ALTER COLUMN id_version ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.policy_version_id_version_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: request; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.request (
    id_request bigint NOT NULL,
    id_policy bigint NOT NULL,
    policy_version integer DEFAULT 1 NOT NULL,
    entity_type character varying(100) NOT NULL,
    entity_id bigint NOT NULL,
    entity_context jsonb DEFAULT '{}'::jsonb NOT NULL,
    status character varying(20) DEFAULT 'PENDING'::character varying NOT NULL,
    current_step smallint DEFAULT 1 NOT NULL,
    requested_by bigint NOT NULL,
    callback_url character varying(500),
    expires_at timestamp with time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    resolved_at timestamp with time zone,
    CONSTRAINT chk_request_status CHECK (((status)::text = ANY (ARRAY[('PENDING'::character varying)::text, ('IN_PROGRESS'::character varying)::text, ('APPROVED'::character varying)::text, ('REJECTED'::character varying)::text, ('ESCALATED'::character varying)::text, ('EXPIRED'::character varying)::text, ('CANCELLED'::character varying)::text])))
);


--
-- Name: request_id_request_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.request ALTER COLUMN id_request ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.request_id_request_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: audit_log; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.audit_log (id_log, id_request, id_policy, action, actor_id, actor_ip, actor_user_agent, detail, checksum, created_at) FROM stdin;
1	1	1	REQUEST_SUBMITTED	101	127.0.0.1	\N	{"entityId": 9903, "entityType": "order"}	F53494C65A5072DB889B6FA0D8A0C009A06D9527BD30F87BCBACA23FE2A69910C3A491C8EA93989D7D212EA2E439393224F310F9DE712B50DA7DFD2659D6CCF6	2026-08-12 18:33:50.765347+00
2	1	1	REQUEST_APPROVED	9	127.0.0.1	\N	{"step": 2, "reason": "Aprobado por volumen de venta"}	174F5D0B0BC489A1D55AD2D16FF0BB32946FC7C6BD783554F499533B3112A9888C4B9FAA28CABBFE47704EABA92AC0C060873E3B1932A31A7B9E16F5D3748384	2026-08-12 18:33:50.957708+00
3	2	1	REQUEST_SUBMITTED	101	127.0.0.1	\N	{"entityId": 9904, "entityType": "order"}	88B156B538BB118483F1CC60CD4B2E8D7FDA8FA78EB3935691EF315839E5B856D3A2C62FC76FC9F122E7BEADE30C82107231C6739315AE3963EC0F63FB1E4766	2026-08-12 18:33:56.452704+00
4	3	1	REQUEST_SUBMITTED	101	127.0.0.1	\N	{"entityId": 6001, "entityType": "order"}	9EFD9AEDCF9A8FED7E7DE6AFA1D419F4B0D85C1CA33879BFFABEDE899AAD444BF55F46077BD62CE01708A1AA065DAE128DC46CDFED8A8516DE577A630339253E	2026-08-13 12:59:45.635577+00
5	3	1	REQUEST_APPROVED	9	127.0.0.1	\N	{"step": 2, "reason": "Aprobado por supervisor cnaranjo por volumen"}	699585E17EE54EB55CB6D702E734EBB78C709426189B23B8C4B5005CC669FA5C58168242032AFAD91D8ACBE72FAD1302C8463C7B3433B3CCA137AC60C1BA5A18	2026-08-13 12:59:45.671504+00
6	4	2	REQUEST_SUBMITTED	9	127.0.0.1	\N	{"entityId": 6001, "entityType": "order"}	0E8E12F846E2A38D253D095B25DE598279409F0C8F66F426D74E91BBE141F406147D4FBEA731A7A344CF24B6641BA3B43C24E6039159188C948EE265DC6E14F9	2026-08-13 12:59:45.725836+00
7	4	2	REQUEST_APPROVED	237	127.0.0.1	\N	{"step": 1, "reason": "Aprobado por Backoffice gvillanueva"}	50D22145C6CE79F27E3166D83DF37137AB772969C974A0989DE9A1AF3B9AE91A2BEF0FD70C86D92487064591396DEC68958FFE3B910BF5145471FEE93C59D5DC	2026-08-13 12:59:45.782447+00
8	5	1	REQUEST_SUBMITTED	2	127.0.0.1	\N	{"entityId": 6002, "entityType": "order"}	FD90757568BA51EC065719FC7608E9473CEE8D4A1CCE0165D0A2F947DBADB54F9108F012B480CC478BC1F8D0792064BC47D2C718240D23B236FC2A6A57470C1F	2026-08-13 12:59:45.836463+00
\.


--
-- Data for Name: chain; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.chain (id_chain, id_policy, chain_mode, max_sla_hours, on_timeout_action) FROM stdin;
1	1	SEQUENTIAL	48	ESCALATE
\.


--
-- Data for Name: chain_step; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.chain_step (id_step, id_chain, step_order, approver_type, approver_ref, condition_expr, can_delegate, sla_hours, is_optional) FROM stdin;
1	1	1	ROLE	SUPERVISOR	\N	t	24	f
2	1	2	DIVISION	FINANZAS	\N	t	24	f
\.


--
-- Data for Name: decision; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.decision (id_decision, id_request, step_order, decided_by, original_approver, decision, reason, evidence_path, decided_at) FROM stdin;
1	1	1	9	\N	APPROVED	Aprobado por volumen de venta	\N	2026-08-12 18:33:50.945578+00
2	3	1	9	\N	APPROVED	Aprobado por supervisor cnaranjo por volumen	\N	2026-08-13 12:59:45.66435+00
3	4	1	237	\N	APPROVED	Aprobado por Backoffice gvillanueva	\N	2026-08-13 12:59:45.776427+00
\.


--
-- Data for Name: delegation; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.delegation (id_delegation, delegator_id, delegate_id, id_policy, reason, valid_from, valid_until, is_active, created_at) FROM stdin;
\.


--
-- Data for Name: policy; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.policy (id_policy, code, name, description, scope_type, scope_id, is_active, current_version, created_by, created_at) FROM stdin;
1	APPROVAL_HIGH_DISCOUNT	Aprobación de Descuentos Mayores a 15%	Requiere visto bueno de Supervisor y Gerencia Financiera	GLOBAL	\N	t	1	1	2026-08-12 16:38:44.505385+00
2	APPROVAL_ORDER_CANCELLATION	Aprobación de Anulación de Pedidos	Requiere aprobación de Operaciones	GLOBAL	\N	t	1	1	2026-08-12 16:38:44.505385+00
\.


--
-- Data for Name: policy_version; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.policy_version (id_version, id_policy, version_number, change_reason, snapshot_json, published_by, published_at) FROM stdin;
\.


--
-- Data for Name: request; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.request (id_request, id_policy, policy_version, entity_type, entity_id, entity_context, status, current_step, requested_by, callback_url, expires_at, created_at, resolved_at) FROM stdin;
1	1	1	order	9903	{}	IN_PROGRESS	2	101	\N	2026-08-19 18:33:50.730348+00	2026-08-12 18:33:50.745417+00	\N
2	1	1	order	9904	{}	PENDING	1	101	\N	2026-08-19 18:33:56.44996+00	2026-08-12 18:33:56.450328+00	\N
3	1	1	order	6001	{"discountPct": 20}	IN_PROGRESS	2	101	\N	2026-08-20 12:59:45.624573+00	2026-08-13 12:59:45.626196+00	\N
4	2	1	order	6001	{}	APPROVED	1	9	\N	2026-08-20 12:59:45.72164+00	2026-08-13 12:59:45.722153+00	2026-08-13 12:59:45.778947+00
5	1	1	order	6002	{}	PENDING	1	2	\N	2026-08-20 12:59:45.83344+00	2026-08-13 12:59:45.833842+00	\N
\.


--
-- Name: audit_log_id_log_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.audit_log_id_log_seq', 8, true);


--
-- Name: chain_id_chain_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.chain_id_chain_seq', 1, true);


--
-- Name: chain_step_id_step_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.chain_step_id_step_seq', 2, true);


--
-- Name: decision_id_decision_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.decision_id_decision_seq', 3, true);


--
-- Name: delegation_id_delegation_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.delegation_id_delegation_seq', 1, false);


--
-- Name: policy_id_policy_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.policy_id_policy_seq', 2, true);


--
-- Name: policy_version_id_version_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.policy_version_id_version_seq', 1, false);


--
-- Name: request_id_request_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.request_id_request_seq', 5, true);


--
-- Name: audit_log audit_log_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.audit_log
    ADD CONSTRAINT audit_log_pkey PRIMARY KEY (id_log);


--
-- Name: chain chain_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chain
    ADD CONSTRAINT chain_pkey PRIMARY KEY (id_chain);


--
-- Name: chain_step chain_step_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chain_step
    ADD CONSTRAINT chain_step_pkey PRIMARY KEY (id_step);


--
-- Name: decision decision_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.decision
    ADD CONSTRAINT decision_pkey PRIMARY KEY (id_decision);


--
-- Name: delegation delegation_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.delegation
    ADD CONSTRAINT delegation_pkey PRIMARY KEY (id_delegation);


--
-- Name: policy policy_code_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.policy
    ADD CONSTRAINT policy_code_key UNIQUE (code);


--
-- Name: policy policy_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.policy
    ADD CONSTRAINT policy_pkey PRIMARY KEY (id_policy);


--
-- Name: policy_version policy_version_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.policy_version
    ADD CONSTRAINT policy_version_pkey PRIMARY KEY (id_version);


--
-- Name: request request_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.request
    ADD CONSTRAINT request_pkey PRIMARY KEY (id_request);


--
-- Name: chain_step uq_chain_step; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chain_step
    ADD CONSTRAINT uq_chain_step UNIQUE (id_chain, step_order);


--
-- Name: policy_version uq_policy_version; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.policy_version
    ADD CONSTRAINT uq_policy_version UNIQUE (id_policy, version_number);


--
-- Name: idx_appr_dec_req; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_appr_dec_req ON public.decision USING btree (id_request);


--
-- Name: idx_appr_del_users; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_appr_del_users ON public.delegation USING btree (delegator_id, delegate_id) WHERE (is_active = true);


--
-- Name: idx_appr_req_entity; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_appr_req_entity ON public.request USING btree (entity_type, entity_id);


--
-- Name: idx_appr_req_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_appr_req_status ON public.request USING btree (status);


--
-- Name: idx_appr_req_user; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_appr_req_user ON public.request USING btree (requested_by);


--
-- Name: chain chain_id_policy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chain
    ADD CONSTRAINT chain_id_policy_fkey FOREIGN KEY (id_policy) REFERENCES public.policy(id_policy) ON DELETE CASCADE;


--
-- Name: chain_step chain_step_id_chain_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chain_step
    ADD CONSTRAINT chain_step_id_chain_fkey FOREIGN KEY (id_chain) REFERENCES public.chain(id_chain) ON DELETE CASCADE;


--
-- Name: decision decision_id_request_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.decision
    ADD CONSTRAINT decision_id_request_fkey FOREIGN KEY (id_request) REFERENCES public.request(id_request) ON DELETE CASCADE;


--
-- Name: delegation delegation_id_policy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.delegation
    ADD CONSTRAINT delegation_id_policy_fkey FOREIGN KEY (id_policy) REFERENCES public.policy(id_policy);


--
-- Name: policy_version policy_version_id_policy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.policy_version
    ADD CONSTRAINT policy_version_id_policy_fkey FOREIGN KEY (id_policy) REFERENCES public.policy(id_policy) ON DELETE CASCADE;


--
-- Name: request request_id_policy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.request
    ADD CONSTRAINT request_id_policy_fkey FOREIGN KEY (id_policy) REFERENCES public.policy(id_policy);


--
-- PostgreSQL database dump complete
--

\unrestrict kqJqP92HylgqYjVRP6OPLftiri2o0ut4k0PDBnK2XYNMgCE8hSqLvH3rW5jqYmS

