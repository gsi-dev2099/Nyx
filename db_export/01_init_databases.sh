#!/bin/bash
set -e

echo "=== Creating roles for compatibility ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    CREATE ROLE postgres WITH SUPERUSER LOGIN;
    CREATE ROLE ext_nyx;
    CREATE ROLE usr_consultas;
    CREATE ROLE web_seleccion;
    CREATE ROLE backup;
    CREATE ROLE nexus;
    CREATE ROLE api_srv;
EOSQL

echo "=== Initializing multiple PostgreSQL databases ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    CREATE DATABASE nyx_crm;
    CREATE DATABASE nx_ecosystem;
    CREATE DATABASE nyx_sla;
    CREATE DATABASE nyx_approval;
    CREATE DATABASE nyx_flow;

    CREATE USER usr_crm WITH PASSWORD '${CRM_DB_PASSWORD:-Crm\$Nyx2026!Secured#Key}';
    CREATE USER usr_sla WITH PASSWORD '${SLA_DB_PASSWORD:-Sla\$Nyx2026!Engine#Key}';
    CREATE USER usr_approval WITH PASSWORD '${APPROVAL_DB_PASSWORD:-Approval\$Nyx2026!Secured#Key}';
    CREATE USER usr_flow WITH PASSWORD '${FLOW_DB_PASSWORD:-Flow\$Nyx2026!Engine#Key}';

    GRANT ALL PRIVILEGES ON DATABASE nyx_crm TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nx_ecosystem TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nyx_crm TO usr_crm;
    GRANT ALL PRIVILEGES ON DATABASE nyx_sla TO usr_sla;
    GRANT ALL PRIVILEGES ON DATABASE nyx_approval TO usr_approval;
    GRANT ALL PRIVILEGES ON DATABASE nyx_flow TO usr_flow;
    GRANT ALL PRIVILEGES ON DATABASE nyx_sla TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nyx_approval TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nyx_flow TO "$POSTGRES_USER";
EOSQL

echo "=== Restoring nyx_crm database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_crm" -f /docker-entrypoint-initdb.d/nyx_crm_backup.sql

echo "=== Restoring nx_ecosystem database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nx_ecosystem" -f /docker-entrypoint-initdb.d/nx_ecosystem_backup.sql

echo "=== Initializing nyx_sla database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_sla" -f /docker-entrypoint-initdb.d/04_init_sla_schema.sql

echo "=== Initializing nyx_approval database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_approval" -f /docker-entrypoint-initdb.d/05_init_approval_schema.sql

echo "=== Initializing nyx_flow database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_flow" -f /docker-entrypoint-initdb.d/06_init_flow_schema.sql

echo "=== Databases restored and initialized successfully ==="

