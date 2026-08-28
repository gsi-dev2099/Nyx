#!/usr/bin/env bash
set -eo pipefail

CONTAINER_NAME="${1:-crm_postgres}"
DB_USER="${2:-ronald}"
DB_NAME="${3:-nyx_crm}"
OUTPUT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../db_export/dumps" && pwd)"
OUTPUT_PATH="${OUTPUT_DIR}/nyx_crm_schema_only.sql"

echo "=========================================================="
echo " [NYX CRM] EXTRACTOR DE ESTRUCTURA DDL (SCHEMA-ONLY)"
echo "=========================================================="

mkdir -p "$OUTPUT_DIR"

echo "[INFO] Extrayendo DDL desde $CONTAINER_NAME ($DB_NAME)..."

docker exec -t "$CONTAINER_NAME" pg_dump \
  -U "$DB_USER" \
  -d "$DB_NAME" \
  --schema-only \
  --clean \
  --if-exists \
  --no-owner \
  --no-privileges \
  --encoding=UTF8 > "$OUTPUT_PATH"

echo "[SUCCESS] Estructura DDL generada correctamente en: $OUTPUT_PATH"
echo "=========================================================="
