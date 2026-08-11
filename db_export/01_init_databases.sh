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
    GRANT ALL PRIVILEGES ON DATABASE nyx_crm TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nx_ecosystem TO "$POSTGRES_USER";
EOSQL

echo "=== Restoring nyx_crm database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_crm" -f /docker-entrypoint-initdb.d/nyx_crm_backup.sql

echo "=== Restoring nx_ecosystem database ==="
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nx_ecosystem" -f /docker-entrypoint-initdb.d/nx_ecosystem_backup.sql

echo "=== Databases restored successfully ==="
