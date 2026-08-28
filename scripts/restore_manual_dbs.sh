#!/bin/bash
set -e

TARGET_HOST="${1:-127.0.0.1}"
TARGET_PORT="${2:-5432}"
TARGET_USER="${3:-ronald}"
TARGET_PASS="${4:-Gs1_2099Zx23rO24M4r25}"
DUMPS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../db_export/dumps" && pwd)"

export PGPASSWORD="$TARGET_PASS"

echo "================================================================"
echo " [NYX CRM] RESTAURACIÓN MANUAL DE BASES DE DATOS (BASH)"
echo " Target Host : $TARGET_HOST:$TARGET_PORT"
echo " Usuario     : $TARGET_USER"
echo " Carpeta SQL : $DUMPS_DIR"
echo "================================================================"

databases=("nx_ecosystem" "nyx_crm" "nyx_flow" "nyx_approval" "nyx_sla")

for db in "${databases[@]}"; do
    dump_file="$DUMPS_DIR/${db}_backup.sql"
    if [ -f "$dump_file" ]; then
        echo -e "\n[+] Restaurando '$db' desde $dump_file..."
        psql -h "$TARGET_HOST" -p "$TARGET_PORT" -U "$TARGET_USER" -d postgres -c "SELECT 'CREATE DATABASE $db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db')\gexec" 2>/dev/null || true
        psql -h "$TARGET_HOST" -p "$TARGET_PORT" -U "$TARGET_USER" -d "$db" -f "$dump_file" -q
        echo "  [OK] Base de datos '$db' restaurada con éxito."
    else
        echo "  [-] Archivo de volcado no encontrado: $dump_file"
    fi
done

echo -e "\n================================================================"
echo " [RESTAURACIÓN FINALIZADA]"
echo "================================================================"
