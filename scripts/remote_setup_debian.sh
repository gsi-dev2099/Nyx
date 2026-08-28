#!/bin/bash
# ==============================================================================
# NYX CRM - SCRIPT DE INSTALACIÓN Y CONFIGURACIÓN EN DEBIAN TRIXIE (VM REMOTA)
# ==============================================================================
set -e

APP_DIR="/srv/crm_nyx"
COMPOSE_FILE="docker-compose.prod.yml"

echo "================================================================="
echo " [NYX CRM] APROVISIONAMIENTO Y DESPLIEGUE EN DEBIAN TRIXIE"
echo "================================================================="

# 1. Verificar si se ejecuta como root
if [ "$(id -u)" -ne 0 ]; then
    echo "[-] ERROR: Este script debe ejecutarse como root." >&2
    exit 1
fi

# 2. Instalar dependencias del sistema si faltan
echo "[1/6] Verificando e instalando paquetes necesarios en Debian Trixie..."
export DEBIAN_FRONTEND=noninteractive

PACKAGES_TO_INSTALL=()
for pkg in curl ca-certificates gnupg tar rsync git; do
    if ! dpkg -s "$pkg" >/dev/null 2>&1; then
        PACKAGES_TO_INSTALL+=("$pkg")
    fi
done

if [ ${#PACKAGES_TO_INSTALL[@]} -gt 0 ]; then
    echo "  -> Instalando paquetes base: ${PACKAGES_TO_INSTALL[*]}..."
    apt-get update -y
    apt-get install -y "${PACKAGES_TO_INSTALL[@]}"
fi

# 3. Verificar e instalar Docker Engine + Docker Compose Plugin
if ! command -v docker >/dev/null 2>&1 || ! docker compose version >/dev/null 2>&1; then
    echo "[2/6] Docker o Docker Compose Plugin no encontrados. Instalando repositorio oficial..."
    
    install -m 0755 -d /etc/apt/keyrings
    if [ ! -f /etc/apt/keyrings/docker.gpg ]; then
        curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
        chmod a+r /etc/apt/keyrings/docker.gpg
    fi

    # Para Debian Trixie (testing/13) o Bookworm fallback
    DEBIAN_CODENAME=$(lsb_release -cs 2>/dev/null || echo "trixie")
    if [ "$DEBIAN_CODENAME" = "trixie" ] || [ "$DEBIAN_CODENAME" = "testing" ]; then
        echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian bookworm stable" > /etc/apt/sources.list.d/docker.list
    else
        echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $DEBIAN_CODENAME stable" > /etc/apt/sources.list.d/docker.list
    fi

    apt-get update -y
    apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin || apt-get install -y docker.io docker-compose-plugin

    systemctl enable --now docker
    echo "  -> Docker y Docker Compose instalados correctamente."
else
    echo "[2/6] Docker y Docker Compose Plugin ya se encuentran instalados."
    systemctl start docker || true
fi

# 4. Configurar Directorio y Permisos
echo "[3/6] Configurando directorio de aplicacion y permisos en $APP_DIR..."
mkdir -p "$APP_DIR"
mkdir -p "$APP_DIR/storage"
mkdir -p "$APP_DIR/db_export/dumps"
chmod 755 "$APP_DIR"
chown -R root:root "$APP_DIR"

cd "$APP_DIR"

# 5. Descomprimir archivos transferidos
if [ -f "crm_nyx_deploy.tar" ]; then
    echo "[4/6] Descomprimiendo archivos del proyecto..."
    tar -xf crm_nyx_deploy.tar
    rm -f crm_nyx_deploy.tar
    chmod +x scripts/*.sh 2>/dev/null || true
    chmod +x db_export/*.sh 2>/dev/null || true
fi

# Configurar .env si no existe
if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        cp .env.example .env
        echo "  -> Creado archivo .env desde .env.example"
    else
        cat << 'EOF' > .env
POSTGRES_USER=ronald
POSTGRES_PASSWORD=Gs1_2099Zx23rO24M4r25
POSTGRES_MULTIPLE_DATABASES=nyx_crm,nx_ecosystem,nyx_sla,nyx_approval,nyx_flow
POSTGRES_PORT=5432
CRM_DB_PASSWORD=Crm_Nyx2026_Secured_Key
SLA_DB_PASSWORD=Sla_Nyx2026_Engine_Key
APPROVAL_DB_PASSWORD=Approval_Nyx2026_Secured_Key
FLOW_DB_PASSWORD=Flow_Nyx2026_Engine_Key
MINIO_ROOT_USER=nyx_admin
MINIO_ROOT_PASSWORD=NyxMinio_2026StorageKey!
MINIO_PORT=9000
MINIO_CONSOLE_PORT=9001
REDIS_PORT=6379
ASPNETCORE_ENVIRONMENT=Production
JWT_SECRET_KEY=NyxCRM_SuperSecret_JwtKey_2026_Prod_256bits_Key!
HTTP_PORT=80
EOF
        echo "  -> Generado archivo .env por defecto."
    fi
fi

# 6. Limpieza preventiva de espacio en disco en Docker
echo "  -> Liberando espacio en disco de Docker (build cache & dangling layers)..."
docker builder prune -f 2>/dev/null || true
docker image prune -f 2>/dev/null || true

# 7. Construir y desplegar Contenedores
echo "[5/6] Levantando servicios con Docker Compose..."
if [ -f "$COMPOSE_FILE" ]; then
    docker compose -f "$COMPOSE_FILE" up -d --build
    docker compose -f "$COMPOSE_FILE" restart nginx
else
    docker compose up -d --build
    docker compose restart nginx
fi

echo "  -> Esperando inicializacion de PostgreSQL..."
for i in {1..30}; do
    if docker exec crm_postgres pg_isready -U ronald >/dev/null 2>&1; then
        echo "  -> PostgreSQL disponible."
        break
    fi
    sleep 2
done

# Ejecutar scripts de migraciones / corrección si existen
if [ -d "db_export" ]; then
    echo "  -> Verificando migraciones y ajustes de base de datos..."
    docker exec -i crm_postgres psql -U ronald -d nyx_crm < db_export/02_update_roles_substatuses.sql 2>/dev/null || true
    docker exec -i crm_postgres psql -U ronald -d nx_ecosystem < db_export/03_fix_passwords.sql 2>/dev/null || true
fi

# 8. Verificación de Salud
echo "[6/6] Verificando salud de los contenedores..."
sleep 3
docker compose -f "$COMPOSE_FILE" ps

echo "================================================================="
echo " [NYX CRM] DESPLIEGUE FINALIZADO EXITOSAMENTE"
echo "================================================================="
