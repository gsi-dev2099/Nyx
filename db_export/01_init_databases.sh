#!/bin/sh
set -e

DUMPS_DIR="/docker-entrypoint-initdb.d/dumps"
if [ ! -d "$DUMPS_DIR" ]; then
    DUMPS_DIR="/docker-entrypoint-initdb.d"
fi

echo "=========================================================="
echo " [NYX CRM] INICIALIZANDO CLUSTER POSTGRESQL MULTI-DATABASE"
echo "=========================================================="

echo "[1/6] Creando roles de compatibilidad del sistema..."
psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    CREATE ROLE postgres WITH SUPERUSER LOGIN;
    CREATE ROLE ext_nyx;
    CREATE ROLE usr_consultas;
    CREATE ROLE web_seleccion;
    CREATE ROLE backup;
    CREATE ROLE nexus;
    CREATE ROLE api_srv;
EOSQL

echo "[2/6] Creando bases de datos y usuarios de microservicios..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "postgres" <<-EOSQL
    CREATE DATABASE nyx_crm;
    CREATE DATABASE nx_ecosystem;
    CREATE DATABASE nyx_sla;
    CREATE DATABASE nyx_approval;
    CREATE DATABASE nyx_flow;

    CREATE USER usr_crm WITH PASSWORD '${CRM_DB_PASSWORD:-Crm_Nyx2026_Secured_Key}';
    CREATE USER usr_sla WITH PASSWORD '${SLA_DB_PASSWORD:-Sla_Nyx2026_Engine_Key}';
    CREATE USER usr_approval WITH PASSWORD '${APPROVAL_DB_PASSWORD:-Approval_Nyx2026_Secured_Key}';
    CREATE USER usr_flow WITH PASSWORD '${FLOW_DB_PASSWORD:-Flow_Nyx2026_Engine_Key}';

    GRANT ALL PRIVILEGES ON DATABASE nyx_crm TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nx_ecosystem TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nyx_sla TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nyx_approval TO "$POSTGRES_USER";
    GRANT ALL PRIVILEGES ON DATABASE nyx_flow TO "$POSTGRES_USER";

    GRANT ALL PRIVILEGES ON DATABASE nyx_crm TO usr_crm;
    GRANT ALL PRIVILEGES ON DATABASE nyx_sla TO usr_sla;
    GRANT ALL PRIVILEGES ON DATABASE nyx_approval TO usr_approval;
    GRANT ALL PRIVILEGES ON DATABASE nyx_flow TO usr_flow;
EOSQL

echo "[3/6] Restaurando base de datos principal: nyx_crm..."
if [ -f "$DUMPS_DIR/nyx_crm_backup.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_crm" -f "$DUMPS_DIR/nyx_crm_backup.sql"
    echo "  -> nyx_crm restaurada exitosamente."
else
    echo "  -> AVISO: No se encontro nyx_crm_backup.sql."
fi

echo "[4/6] Restaurando ecosistema central: nx_ecosystem..."
if [ -f "$DUMPS_DIR/nx_ecosystem_backup.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nx_ecosystem" -f "$DUMPS_DIR/nx_ecosystem_backup.sql"
    echo "  -> nx_ecosystem restaurada exitosamente."
else
    echo "  -> AVISO: No se encontro nx_ecosystem_backup.sql."
fi

echo "[5/6] Restaurando motores de microservicios (flow, approval, sla)..."
if [ -f "$DUMPS_DIR/nyx_flow_backup.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_flow" -f "$DUMPS_DIR/nyx_flow_backup.sql"
    echo "  -> nyx_flow restaurada exitosamente."
fi

if [ -f "$DUMPS_DIR/nyx_approval_backup.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_approval" -f "$DUMPS_DIR/nyx_approval_backup.sql"
    echo "  -> nyx_approval restaurada exitosamente."
fi

if [ -f "$DUMPS_DIR/nyx_sla_backup.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_sla" -f "$DUMPS_DIR/nyx_sla_backup.sql"
    echo "  -> nyx_sla restaurada exitosamente."
fi

echo "[6/6] Aplicando parches y sincronizacion de esquemas..."
if [ -f "/docker-entrypoint-initdb.d/02_update_roles_substatuses.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nyx_crm" -f /docker-entrypoint-initdb.d/02_update_roles_substatuses.sql
fi

if [ -f "/docker-entrypoint-initdb.d/03_fix_passwords.sql" ]; then
    psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "nx_ecosystem" -f /docker-entrypoint-initdb.d/03_fix_passwords.sql
fi

echo "=========================================================="
echo " [NYX CRM] TODAS LAS BASES DE DATOS RESTAURADAS CON EXITO"
echo "=========================================================="
