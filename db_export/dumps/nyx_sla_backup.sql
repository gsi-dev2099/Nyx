--
-- PostgreSQL database dump
--

\restrict COtfnd1XqvlJrN6e0rafqAGzKtRCnzAcbB75pThwa8FaYqwWDfcUJJwVaUQV0bB

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

ALTER TABLE IF EXISTS ONLY public.work_schedule DROP CONSTRAINT IF EXISTS work_schedule_id_calendar_fkey;
ALTER TABLE IF EXISTS ONLY public.sla_measurement DROP CONSTRAINT IF EXISTS sla_measurement_id_policy_fkey;
ALTER TABLE IF EXISTS ONLY public.sla_alert DROP CONSTRAINT IF EXISTS sla_alert_id_measurement_fkey;
ALTER TABLE IF EXISTS ONLY public.holiday DROP CONSTRAINT IF EXISTS holiday_id_calendar_fkey;
DROP INDEX IF EXISTS public.idx_sla_meas_status;
DROP INDEX IF EXISTS public.idx_sla_meas_owner;
DROP INDEX IF EXISTS public.idx_sla_meas_entity;
DROP INDEX IF EXISTS public.idx_sla_alert_meas;
ALTER TABLE IF EXISTS ONLY public.work_schedule DROP CONSTRAINT IF EXISTS work_schedule_pkey;
ALTER TABLE IF EXISTS ONLY public.work_schedule DROP CONSTRAINT IF EXISTS work_schedule_id_calendar_day_of_week_key;
ALTER TABLE IF EXISTS ONLY public.work_calendar DROP CONSTRAINT IF EXISTS work_calendar_pkey;
ALTER TABLE IF EXISTS ONLY public.work_calendar DROP CONSTRAINT IF EXISTS work_calendar_code_key;
ALTER TABLE IF EXISTS ONLY public.user_work_shifts DROP CONSTRAINT IF EXISTS user_work_shifts_pkey;
ALTER TABLE IF EXISTS ONLY public.user_work_shifts DROP CONSTRAINT IF EXISTS uq_user_day;
ALTER TABLE IF EXISTS ONLY public.sla_measurement DROP CONSTRAINT IF EXISTS uq_sla_measurement;
ALTER TABLE IF EXISTS ONLY public.sla_policy DROP CONSTRAINT IF EXISTS sla_policy_pkey;
ALTER TABLE IF EXISTS ONLY public.sla_policy DROP CONSTRAINT IF EXISTS sla_policy_code_key;
ALTER TABLE IF EXISTS ONLY public.sla_measurement DROP CONSTRAINT IF EXISTS sla_measurement_pkey;
ALTER TABLE IF EXISTS ONLY public.sla_audit_log DROP CONSTRAINT IF EXISTS sla_audit_log_pkey;
ALTER TABLE IF EXISTS ONLY public.sla_alert DROP CONSTRAINT IF EXISTS sla_alert_pkey;
ALTER TABLE IF EXISTS ONLY public.holiday DROP CONSTRAINT IF EXISTS holiday_pkey;
ALTER TABLE IF EXISTS ONLY public.holiday DROP CONSTRAINT IF EXISTS holiday_id_calendar_holiday_date_key;
DROP TABLE IF EXISTS public.work_schedule;
DROP TABLE IF EXISTS public.work_calendar;
DROP TABLE IF EXISTS public.user_work_shifts;
DROP TABLE IF EXISTS public.sla_policy;
DROP TABLE IF EXISTS public.sla_measurement;
DROP TABLE IF EXISTS public.sla_audit_log;
DROP TABLE IF EXISTS public.sla_alert;
DROP TABLE IF EXISTS public.holiday;
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
-- Name: holiday; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.holiday (
    id_holiday bigint NOT NULL,
    id_calendar bigint NOT NULL,
    holiday_date date NOT NULL,
    name character varying(200) NOT NULL,
    is_half_day boolean DEFAULT false NOT NULL
);


--
-- Name: holiday_id_holiday_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.holiday ALTER COLUMN id_holiday ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.holiday_id_holiday_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sla_alert; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sla_alert (
    id_alert bigint NOT NULL,
    id_measurement bigint NOT NULL,
    alert_level character varying(20) NOT NULL,
    notified_to bigint,
    callback_sent boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: sla_alert_id_alert_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sla_alert ALTER COLUMN id_alert ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.sla_alert_id_alert_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sla_audit_log; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sla_audit_log (
    id_log bigint NOT NULL,
    id_measurement bigint,
    id_policy bigint,
    action character varying(50) NOT NULL,
    actor_id bigint NOT NULL,
    detail jsonb DEFAULT '{}'::jsonb NOT NULL,
    checksum character varying(128) NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: sla_audit_log_id_log_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sla_audit_log ALTER COLUMN id_log ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.sla_audit_log_id_log_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sla_measurement; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sla_measurement (
    id_measurement bigint NOT NULL,
    id_policy bigint NOT NULL,
    entity_type character varying(100) NOT NULL,
    entity_id bigint NOT NULL,
    owner_user_id bigint,
    started_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    paused_at timestamp with time zone,
    resolved_at timestamp with time zone,
    elapsed_minutes integer DEFAULT 0 NOT NULL,
    status character varying(20) DEFAULT 'RUNNING'::character varying NOT NULL,
    breach_at timestamp with time zone,
    metadata jsonb DEFAULT '{}'::jsonb NOT NULL
);


--
-- Name: sla_measurement_id_measurement_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sla_measurement ALTER COLUMN id_measurement ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.sla_measurement_id_measurement_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: sla_policy; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sla_policy (
    id_policy bigint NOT NULL,
    code character varying(80) NOT NULL,
    name character varying(200) NOT NULL,
    description text,
    scope_type character varying(50) DEFAULT 'GLOBAL'::character varying NOT NULL,
    scope_id bigint,
    target_minutes integer NOT NULL,
    warning_pct smallint DEFAULT 75 NOT NULL,
    critical_pct smallint DEFAULT 100 NOT NULL,
    escalation_pct smallint DEFAULT 120,
    applies_to character varying(50) DEFAULT 'ORDER'::character varying NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_by bigint DEFAULT 1 NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: sla_policy_id_policy_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.sla_policy ALTER COLUMN id_policy ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.sla_policy_id_policy_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: user_work_shifts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_work_shifts (
    id uuid DEFAULT gen_random_uuid() NOT NULL,
    id_user bigint NOT NULL,
    day_of_week integer NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL,
    is_active boolean DEFAULT true,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT user_work_shifts_day_of_week_check CHECK (((day_of_week >= 0) AND (day_of_week <= 6)))
);


--
-- Name: work_calendar; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.work_calendar (
    id_calendar bigint NOT NULL,
    code character varying(80) NOT NULL,
    name character varying(200) NOT NULL,
    timezone character varying(50) DEFAULT 'America/Lima'::character varying NOT NULL,
    is_default boolean DEFAULT false NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL
);


--
-- Name: work_calendar_id_calendar_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.work_calendar ALTER COLUMN id_calendar ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.work_calendar_id_calendar_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: work_schedule; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.work_schedule (
    id_schedule bigint NOT NULL,
    id_calendar bigint NOT NULL,
    day_of_week smallint NOT NULL,
    start_time time without time zone NOT NULL,
    end_time time without time zone NOT NULL,
    CONSTRAINT work_schedule_day_of_week_check CHECK (((day_of_week >= 0) AND (day_of_week <= 6)))
);


--
-- Name: work_schedule_id_schedule_seq; Type: SEQUENCE; Schema: public; Owner: -
--

ALTER TABLE public.work_schedule ALTER COLUMN id_schedule ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.work_schedule_id_schedule_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: holiday; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.holiday (id_holiday, id_calendar, holiday_date, name, is_half_day) FROM stdin;
\.


--
-- Data for Name: sla_alert; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sla_alert (id_alert, id_measurement, alert_level, notified_to, callback_sent, created_at) FROM stdin;
\.


--
-- Data for Name: sla_audit_log; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sla_audit_log (id_log, id_measurement, id_policy, action, actor_id, detail, checksum, created_at) FROM stdin;
1	1	2	MEASUREMENT_STARTED	1	{"breachAt": "2026-08-13T18:04:27.7390141Z", "entityId": 9901, "entityType": "order"}	23333E9793401007015735073CF921F9970003ABBF26467EC3583A0DD0330E92D03FBA1C1D4F04BCFA7C206DF1D6836A47823CD51E0BD38AA935DFF7DF42772F	2026-08-12 18:04:27.770527+00
2	1	2	MEASUREMENT_RESOLVED	1	{"status": "COMPLETED", "elapsedMinutes": 2}	39D7350AFFE16365F2103463CFC9E82F5BCD37C3E39F00A01197A7F6CAE0C18363DCCF9F4524CD07E7AADF7524B6AB0C2B97566AF70837E4A6FDB7D13D36877B	2026-08-12 18:06:37.540699+00
3	2	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-14T12:59:45.3452490Z", "entityId": 6001, "entityType": "order"}	DDD2D1C7B917F21225264A4C909972CEC5E25E5E88D70A7BB3F3979F5D0B84BBADD52869266542CA383429C7C87767FFF36C21B53003FAE5162FCB1730F7A6AF	2026-08-13 12:59:45.357137+00
4	2	2	MEASUREMENT_RESOLVED	251	{"status": "COMPLETED", "elapsedMinutes": 0}	0127B1CD5753A9BC7DB7D5350065DCE17F16C71B252AA7B872CA6FE5F2A4E3869D3C9E9FFD61C1AD60DC5ABE035D49B08078FA85CAB68C84B0D3A46CB5EFC7E8	2026-08-13 12:59:45.909651+00
5	3	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-15T20:41:51.9693614Z", "entityId": 12, "entityType": "order"}	747C156AB3ECFBA448E375F6ADADF5FEF6D6B11230879C6BD1A7A13B41EB9CD38F1567D3A2967ACB394132E9D98CEB8D90ADA19506E248F2ABDA3A42FB80B32A	2026-08-14 20:41:51.984651+00
6	4	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-18T17:34:03.4849867Z", "entityId": 13, "entityType": "order"}	2C9C9C0F798CAAAF16998CF5F12FFCBE4EF8FA76FB4908E0CFF29193806F82ED3BF1BE1D84101783BE32F16C70A497C17C9B06683418C81ECAE50F802512E93E	2026-08-17 17:34:03.512801+00
7	5	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-18T17:44:34.4026236Z", "entityId": 14, "entityType": "order"}	857EC9845D57B19F9DB71EC952B636BF990166C185F7FC6DA9E4C16C75C5686533134B1596F333A524C0E317D4E722EF99A2003F0F58AC17C898CFD73E60BE7C	2026-08-17 17:44:34.409737+00
8	6	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-18T18:06:07.0511045Z", "entityId": 15, "entityType": "order"}	AD042BBA01265A81486567B962EB0816E164771F6D62FE4692945B273F293BDA1E9937512AF415A100D151863E3E51B82F2F8D3F3691C986123D2FAA4DA3FCC3	2026-08-17 18:06:07.056682+00
9	7	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-18T18:44:47.8103032Z", "entityId": 16, "entityType": "order"}	3BCFF41D158B5952EDAA5BE0EAE13B36BE3B7837242F2718173A1A9E8198C319247170DA6415F14EF2E6C82A56289FC7BD81D036222549D666A41593E642D746	2026-08-17 18:44:47.818633+00
10	8	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-18T19:14:34.6492979Z", "entityId": 17, "entityType": "order"}	80DA2218838E7269B87793D9B5CBAE6C7C65F57ABD8EBF8FD9E8F5247D52AEDC4831045DC2ACBB7DAACC81ECA90BBDB90B0F758176E1C65A624562460D00A0EE	2026-08-17 19:14:34.655408+00
11	9	2	MEASUREMENT_STARTED	101	{"breachAt": "2026-08-18T19:27:17.5452242Z", "entityId": 18, "entityType": "order"}	CE37CD3BD0E85F48D3B38EC67D3A03B1F71EAFA83F201B785490DF5B89393C7F8D9CDDD6D6138C0EAA25750C388DFC067D159501EE0639F7B9472CE19C2A83EE	2026-08-17 19:27:17.550013+00
\.


--
-- Data for Name: sla_measurement; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sla_measurement (id_measurement, id_policy, entity_type, entity_id, owner_user_id, started_at, paused_at, resolved_at, elapsed_minutes, status, breach_at, metadata) FROM stdin;
1	2	order	9901	101	2026-08-12 18:04:27.739014+00	\N	2026-08-12 18:06:37.525094+00	2	COMPLETED	2026-08-13 18:04:27.739014+00	{}
2	2	order	6001	101	2026-08-13 12:59:45.345249+00	\N	2026-08-13 12:59:45.90638+00	0	COMPLETED	2026-08-14 12:59:45.345249+00	{}
3	2	order	12	101	2026-08-14 20:41:51.969361+00	\N	\N	0	RUNNING	2026-08-15 20:41:51.969361+00	{}
4	2	order	13	101	2026-08-17 17:34:03.484986+00	\N	\N	0	RUNNING	2026-08-18 17:34:03.484986+00	{}
5	2	order	14	101	2026-08-17 17:44:34.402623+00	\N	\N	0	RUNNING	2026-08-18 17:44:34.402623+00	{}
6	2	order	15	101	2026-08-17 18:06:07.051104+00	\N	\N	0	RUNNING	2026-08-18 18:06:07.051104+00	{}
7	2	order	16	101	2026-08-17 18:44:47.810303+00	\N	\N	0	RUNNING	2026-08-18 18:44:47.810303+00	{}
8	2	order	17	101	2026-08-17 19:14:34.649297+00	\N	\N	0	RUNNING	2026-08-18 19:14:34.649297+00	{}
9	2	order	18	101	2026-08-17 19:27:17.545224+00	\N	\N	0	RUNNING	2026-08-18 19:27:17.545224+00	{}
\.


--
-- Data for Name: sla_policy; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.sla_policy (id_policy, code, name, description, scope_type, scope_id, target_minutes, warning_pct, critical_pct, escalation_pct, applies_to, is_active, created_by, created_at) FROM stdin;
1	SLA_INCIDENT_CRITICAL	SLA Incidencias Críticas	Resolución de incidencias críticas en menos de 2 horas	GLOBAL	\N	120	75	100	120	INCIDENT	t	1	2026-08-12 16:38:44.106483+00
2	SLA_SALES_VALIDATION	SLA Validación Interna de Ventas	Validación por backoffice en menos de 24 horas hábiles	GLOBAL	\N	1440	80	100	120	ORDER	t	1	2026-08-12 16:38:44.106483+00
\.


--
-- Data for Name: user_work_shifts; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.user_work_shifts (id, id_user, day_of_week, start_time, end_time, is_active, created_at) FROM stdin;
\.


--
-- Data for Name: work_calendar; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.work_calendar (id_calendar, code, name, timezone, is_default, created_at) FROM stdin;
1	DEFAULT_PE	Horario Estándar Perú	America/Lima	t	2026-08-12 16:38:44.094508+00
\.


--
-- Data for Name: work_schedule; Type: TABLE DATA; Schema: public; Owner: -
--

COPY public.work_schedule (id_schedule, id_calendar, day_of_week, start_time, end_time) FROM stdin;
1	1	1	08:00:00	18:00:00
2	1	2	08:00:00	18:00:00
3	1	3	08:00:00	18:00:00
4	1	4	08:00:00	18:00:00
5	1	5	08:00:00	18:00:00
6	1	6	09:00:00	13:00:00
\.


--
-- Name: holiday_id_holiday_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.holiday_id_holiday_seq', 1, false);


--
-- Name: sla_alert_id_alert_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sla_alert_id_alert_seq', 1, false);


--
-- Name: sla_audit_log_id_log_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sla_audit_log_id_log_seq', 11, true);


--
-- Name: sla_measurement_id_measurement_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sla_measurement_id_measurement_seq', 9, true);


--
-- Name: sla_policy_id_policy_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.sla_policy_id_policy_seq', 2, true);


--
-- Name: work_calendar_id_calendar_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.work_calendar_id_calendar_seq', 1, true);


--
-- Name: work_schedule_id_schedule_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.work_schedule_id_schedule_seq', 6, true);


--
-- Name: holiday holiday_id_calendar_holiday_date_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.holiday
    ADD CONSTRAINT holiday_id_calendar_holiday_date_key UNIQUE (id_calendar, holiday_date);


--
-- Name: holiday holiday_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.holiday
    ADD CONSTRAINT holiday_pkey PRIMARY KEY (id_holiday);


--
-- Name: sla_alert sla_alert_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_alert
    ADD CONSTRAINT sla_alert_pkey PRIMARY KEY (id_alert);


--
-- Name: sla_audit_log sla_audit_log_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_audit_log
    ADD CONSTRAINT sla_audit_log_pkey PRIMARY KEY (id_log);


--
-- Name: sla_measurement sla_measurement_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_measurement
    ADD CONSTRAINT sla_measurement_pkey PRIMARY KEY (id_measurement);


--
-- Name: sla_policy sla_policy_code_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_policy
    ADD CONSTRAINT sla_policy_code_key UNIQUE (code);


--
-- Name: sla_policy sla_policy_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_policy
    ADD CONSTRAINT sla_policy_pkey PRIMARY KEY (id_policy);


--
-- Name: sla_measurement uq_sla_measurement; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_measurement
    ADD CONSTRAINT uq_sla_measurement UNIQUE (id_policy, entity_type, entity_id);


--
-- Name: user_work_shifts uq_user_day; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_work_shifts
    ADD CONSTRAINT uq_user_day UNIQUE (id_user, day_of_week);


--
-- Name: user_work_shifts user_work_shifts_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_work_shifts
    ADD CONSTRAINT user_work_shifts_pkey PRIMARY KEY (id);


--
-- Name: work_calendar work_calendar_code_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.work_calendar
    ADD CONSTRAINT work_calendar_code_key UNIQUE (code);


--
-- Name: work_calendar work_calendar_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.work_calendar
    ADD CONSTRAINT work_calendar_pkey PRIMARY KEY (id_calendar);


--
-- Name: work_schedule work_schedule_id_calendar_day_of_week_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.work_schedule
    ADD CONSTRAINT work_schedule_id_calendar_day_of_week_key UNIQUE (id_calendar, day_of_week);


--
-- Name: work_schedule work_schedule_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.work_schedule
    ADD CONSTRAINT work_schedule_pkey PRIMARY KEY (id_schedule);


--
-- Name: idx_sla_alert_meas; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_sla_alert_meas ON public.sla_alert USING btree (id_measurement);


--
-- Name: idx_sla_meas_entity; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_sla_meas_entity ON public.sla_measurement USING btree (entity_type, entity_id);


--
-- Name: idx_sla_meas_owner; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_sla_meas_owner ON public.sla_measurement USING btree (owner_user_id);


--
-- Name: idx_sla_meas_status; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX idx_sla_meas_status ON public.sla_measurement USING btree (status);


--
-- Name: holiday holiday_id_calendar_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.holiday
    ADD CONSTRAINT holiday_id_calendar_fkey FOREIGN KEY (id_calendar) REFERENCES public.work_calendar(id_calendar) ON DELETE CASCADE;


--
-- Name: sla_alert sla_alert_id_measurement_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_alert
    ADD CONSTRAINT sla_alert_id_measurement_fkey FOREIGN KEY (id_measurement) REFERENCES public.sla_measurement(id_measurement) ON DELETE CASCADE;


--
-- Name: sla_measurement sla_measurement_id_policy_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sla_measurement
    ADD CONSTRAINT sla_measurement_id_policy_fkey FOREIGN KEY (id_policy) REFERENCES public.sla_policy(id_policy);


--
-- Name: work_schedule work_schedule_id_calendar_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.work_schedule
    ADD CONSTRAINT work_schedule_id_calendar_fkey FOREIGN KEY (id_calendar) REFERENCES public.work_calendar(id_calendar) ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict COtfnd1XqvlJrN6e0rafqAGzKtRCnzAcbB75pThwa8FaYqwWDfcUJJwVaUQV0bB

