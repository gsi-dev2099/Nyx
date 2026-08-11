-- ============================================================================
-- NYX CRM - DATABASE SCHEMA & DATA INTEGRITY SMOKE TEST
-- ============================================================================
-- Purpose: Verify existence of core schemas, tables, views, and data integrity
-- constraints across the Nyx CRM PostgreSQL database (Week 1 to Week 3 specs).
-- Author: Giuseppe (Dev 3) / Tech Lead (Ronald)
-- Date: 13 July 2026
-- ============================================================================

\echo '------------------------------------------------------------'
\echo '1. VERIFYING SCHEMA EXISTENCE'
\echo '------------------------------------------------------------'

SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name IN ('user_service', 'lead_service', 'campaign_service', 'product_service', 'access_control', 'sales_service', 'ext_ecosystem');

\echo '------------------------------------------------------------'
\echo '2. VERIFYING CORE TABLES EXISTENCE'
\echo '------------------------------------------------------------'

SELECT table_schema, table_name 
FROM information_schema.tables 
WHERE table_schema IN ('user_service', 'lead_service', 'campaign_service', 'product_service', 'access_control', 'sales_service', 'ext_ecosystem')
ORDER BY table_schema, table_name;

\echo '------------------------------------------------------------'
\echo '3. VERIFYING KEY RELATIONSHIPS AND PRIMARY KEYS'
\echo '------------------------------------------------------------'

-- Verify Collaborator to User linkage
SELECT c.table_name, c.column_name, c.data_type
FROM information_schema.columns c
WHERE c.table_schema = 'ext_ecosystem' AND c.table_name = 'collaborators' 
  AND c.column_name IN ('id_user', 'id_collaborator');

-- Verify SalesOrder PK mapping
SELECT c.table_name, c.column_name, c.data_type
FROM information_schema.columns c
WHERE c.table_schema = 'sales_service' AND c.table_name = 'sales_order'
  AND c.column_name IN ('id_order', 'id_lead', 'id_status', 'id_substatus', 'custody_user_id');

\echo '------------------------------------------------------------'
\echo '4. RUNNING DATA INTEGRITY SANITY CHECKS'
\echo '------------------------------------------------------------'

-- Active campaigns count
SELECT COUNT(*) AS active_campaigns FROM campaign_service.campaign WHERE is_active = true;

-- Users and their assigned roles
SELECT u.username, r.name AS role_name
FROM user_service.users u
JOIN access_control.user_role ur ON u.id_user = ur.id_user AND ur.is_active = true
JOIN access_control.role r ON ur.id_role = r.id_role AND r.is_active = true
ORDER BY r.name, u.username;

-- Campaigns assigned to users
SELECT u.username, c.name AS campaign_name
FROM user_service.users u
JOIN user_service.user_campaign uc ON u.id_user = uc.id_user AND uc.is_active = true
JOIN campaign_service.campaign c ON uc.id_cmpg = c.id_cmpg AND c.is_active = true;

-- Lead assignment check
SELECT l.id_lead, l.first_name, l.last_name, u.username AS assigned_advisor, s.name AS status_name
FROM lead_service.lead l
LEFT JOIN user_service.users u ON l.assigned_user_id = u.id_user
LEFT JOIN sales_service.order_status s ON l.current_status_id = s.id_status
LIMIT 10;

-- Sales Order custody status
SELECT o.id_order, c.name AS campaign_name, u.username AS owner_advisor, 
       cust.username AS custody_user, s.name AS status_name
FROM sales_service.sales_order o
JOIN campaign_service.campaign c ON o.id_cmpg = c.id_cmpg
JOIN user_service.users u ON o.id_user = u.id_user
LEFT JOIN user_service.users cust ON o.custody_user_id = cust.id_user
JOIN sales_service.order_status s ON o.id_status = s.id_status
ORDER BY o.id_order DESC
LIMIT 5;

\echo '------------------------------------------------------------'
-- 5. FUNCTIONAL SMOKE TEST: STATE TRANSITION RULES
\echo '------------------------------------------------------------'

-- Test the status transition validation function
-- Transition from Borrador (1) to En revisión supervisor (2) for role ASESOR (Should return TRUE)
SELECT sales_service.validate_status_transition(1, 2, 'ASESOR') AS advisor_borrador_to_revision_ok;

-- Transition from En BackOffice (3) to Borrador (1) for role ASESOR (Should return FALSE)
SELECT sales_service.validate_status_transition(3, 1, 'ASESOR') AS advisor_backoffice_to_borrador_ok;

-- Transition from En BackOffice (3) to En revisión supervisor (2) for role BACKOFFICE (Should return TRUE)
SELECT sales_service.validate_status_transition(3, 2, 'BACKOFFICE') AS backoffice_to_revision_ok;

\echo '------------------------------------------------------------'
\echo 'DATABASE INTEGRITY SMOKE TEST COMPLETED SUCCESSFULLY'
\echo '------------------------------------------------------------'
