#!/bin/bash
set -e

# Cambiar al directorio del proyecto en la VM
cd /srv/crm_nyx

echo "=== 1. Descomprimiendo archivos y configurando entorno ==="
tar -xvf crm_nyx_deploy.tar
rm -f crm_nyx_deploy.tar
chmod +x db_export/*.sh 2>/dev/null || true

if [ ! -f .env ]; then
    cp .env.example .env
fi

echo "=== 2. Construyendo y levantando contenedores Docker ==="
docker compose -f docker-compose.prod.yml up -d --build

echo "=== 3. Esperando respuesta de PostgreSQL ==="
until docker exec crm_postgres pg_isready -U ronald > /dev/null 2>&1; do
    sleep 1
done
sleep 2

echo "=== 4. Verificando bases de datos y aplicando migraciones ==="
docker exec crm_postgres psql -U ronald -d postgres -c "CREATE DATABASE nyx_crm;" 2>/dev/null || true
docker exec crm_postgres psql -U ronald -d postgres -c "CREATE DATABASE nx_ecosystem;" 2>/dev/null || true

DB_CHECK=$(docker exec crm_postgres psql -U ronald -d nyx_crm -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'sales_service';" 2>/dev/null || echo "0")

if [ "$DB_CHECK" = "0" ] || [ -z "$DB_CHECK" ]; then
    echo "Restaurando respaldo inicial de base de datos..."
    docker exec crm_postgres psql -U ronald -d nyx_crm -f /docker-entrypoint-initdb.d/nyx_crm_backup.sql 2>/dev/null || true
    docker exec crm_postgres psql -U ronald -d nx_ecosystem -f /docker-entrypoint-initdb.d/nx_ecosystem_backup.sql 2>/dev/null || true
fi

docker exec -i crm_postgres psql -U ronald -d nyx_crm < db_export/02_update_roles_substatuses.sql
docker exec -i crm_postgres psql -U ronald -d nx_ecosystem < db_export/03_fix_passwords.sql

echo "=== Despliegue remoto finalizado con exito ==="
