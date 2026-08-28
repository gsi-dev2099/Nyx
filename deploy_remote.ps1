<#
.SYNOPSIS
    Script de Despliegue y Actualización de CRM Nyx en Servidor Remoto Debian Trixie (10.10.40.12).
.DESCRIPTION
    Permite desplegar desde cero (con instalación de dependencias en Debian) o actualizar el código
    en caliente (rebuild de contenedores) en la máquina virtual remota mediante SSH/SCP con usuario root.
.PARAMETER Action
    Acción a ejecutar:
    - Update: (Por defecto) Sincroniza código y reconstruye contenedores sin tocar la base de datos.
    - Deploy: Instalación completa (paquetes Debian, Docker, permisos, BD inicial y contenedores).
    - Restart: Reinicia los contenedores en la máquina virtual.
    - Status: Muestra el estado de salud de los contenedores remotos.
    - Logs: Muestra los logs en tiempo real de los servicios remotos.
.PARAMETER ServerIP
    Dirección IP de la máquina virtual (por defecto: 10.10.40.12).
.PARAMETER User
    Usuario con privilegios root (por defecto: root).
.PARAMETER RemotePath
    Ruta de instalación en la máquina virtual (por defecto: /srv/crm_nyx).
.PARAMETER SSHKeyPath
    Ruta opcional a la clave privada SSH (si no se usa autenticación por contraseña).
.EXAMPLE
    # Actualización rápida tras modificar código (por defecto a 10.10.40.12)
    .\deploy_remote.ps1

    # Despliegue inicial completo con instalación de dependencias en Debian Trixie
    .\deploy_remote.ps1 -Action Deploy

    # Ver estado de los contenedores en la VM
    .\deploy_remote.ps1 -Action Status
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Update", "Deploy", "Restart", "Status", "Logs")]
    [string]$Action = "Update",

    [Parameter(Mandatory=$false)]
    [string]$ServerIP = "10.10.40.12",

    [Parameter(Mandatory=$false)]
    [string]$User = "root",

    [Parameter(Mandatory=$false)]
    [string]$RemotePath = "/srv/crm_nyx",

    [Parameter(Mandatory=$false)]
    [string]$SSHKeyPath = ""
)

$ErrorActionPreference = "Stop"
$targetHost = "${User}@${ServerIP}"

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host " [NYX CRM] DESPLIEGUE & ACTUALIZACION REMOTA (DEBIAN TRIXIE)" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host " Servidor Destino : $targetHost" -ForegroundColor Yellow
Write-Host " Ruta Remota      : $RemotePath" -ForegroundColor Yellow
Write-Host " Accion           : $Action" -ForegroundColor Yellow
Write-Host "======================================================================" -ForegroundColor Cyan

# 1. Configurar opciones SSH / SCP
$sshOptions = @(
    "-o", "StrictHostKeyChecking=no",
    "-o", "UserKnownHostsFile=/dev/null",
    "-o", "LogLevel=ERROR"
)
if ($SSHKeyPath -ne "" -and (Test-Path $SSHKeyPath)) {
    $sshOptions += @("-i", $SSHKeyPath)
}

# 2. Localizar directorio base del proyecto
$scriptDir = $PSScriptRoot
if (-not $scriptDir) { $scriptDir = Get-Location }

# Buscar la carpeta CRM_API
$crmApiDir = $scriptDir
if (Test-Path (Join-Path $scriptDir "CRM_API")) {
    $crmApiDir = Join-Path $scriptDir "CRM_API"
}

# 3. Procesar Acción Solicitada

switch ($Action) {
    "Status" {
        Write-Host "`n[*] Consultando estado de los contenedores en $ServerIP..." -ForegroundColor Green
        $cmd = "cd $RemotePath && docker compose -f docker-compose.prod.yml ps"
        ssh @sshOptions $targetHost $cmd
        break
    }

    "Logs" {
        Write-Host "`n[*] Obteniendo logs en tiempo real de $ServerIP (Ctrl+C para salir)..." -ForegroundColor Green
        $cmd = "cd $RemotePath && docker compose -f docker-compose.prod.yml logs -f --tail=100"
        ssh @sshOptions $targetHost $cmd
        break
    }

    "Restart" {
        Write-Host "`n[*] Reiniciando contenedores en $ServerIP..." -ForegroundColor Green
        $cmd = "cd $RemotePath && docker compose -f docker-compose.prod.yml restart"
        ssh @sshOptions $targetHost $cmd
        Write-Host "`n[+] Contenedores reiniciados correctamente." -ForegroundColor Green
        break
    }

    { $_ -in @("Deploy", "Update") } {
        # Paso A: Crear directorio remoto si no existe, asegurar permisos y limpiar cache previo si hay poco espacio
        Write-Host "`n[1/4] Verificando directorio, permisos y liberando espacio en el servidor..." -ForegroundColor Green
        $createDirCmd = "mkdir -p $RemotePath && chmod 755 $RemotePath && chown -R ${User}:${User} $RemotePath && docker builder prune -f 2>/dev/null || true"
        ssh @sshOptions $targetHost $createDirCmd
        if ($LASTEXITCODE -ne 0) {
            Write-Error "No se pudo conectar o preparar el directorio en $targetHost"
        }

        # Paso B: Comprimir el proyecto excluyendo temporales, binarios pesados y uploads locales
        Write-Host "`n[2/4] Empaquetando archivos fuente del proyecto (optimizado)..." -ForegroundColor Green
        $tempTar = Join-Path $env:TEMP "crm_nyx_deploy.tar"
        if (Test-Path $tempTar) { Remove-Item $tempTar -Force }

        Write-Host "  -> Comprimiendo desde: $crmApiDir" -ForegroundColor Gray
        tar --exclude='.git' `
            --exclude='bin' `
            --exclude='obj' `
            --exclude='.vs' `
            --exclude='*.user' `
            --exclude='storage/Documents/*' `
            --exclude='CRM.ApiHub/Storage/Documents/*' `
            --exclude='db_export/dumps/full_cluster_backup.sql' `
            -cf $tempTar -C $crmApiDir .
        
        $tarSizeMb = [math]::Round((Get-Item $tempTar).Length / 1MB, 2)
        Write-Host "  -> Paquete optimizado generado ($tarSizeMb MB)." -ForegroundColor Gray

        # Paso C: Transferir vía SCP
        Write-Host "`n[3/4] Transfiriendo paquete a $ServerIP..." -ForegroundColor Green
        $dest = "${targetHost}:${RemotePath}/crm_nyx_deploy.tar"
        scp @sshOptions $tempTar $dest
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Fallo la transferencia SCP hacia $targetHost"
        }
        Remove-Item $tempTar -Force

        # Paso D: Ejecutar según Acción
        if ($Action -eq "Deploy") {
            Write-Host "`n[4/4] Descomprimiendo archivos y ejecutando instalacion en Debian..." -ForegroundColor Green
            $remoteCmd = @"
cd $RemotePath
tar -xf crm_nyx_deploy.tar
chmod +x scripts/*.sh 2>/dev/null || true
chmod +x db_export/*.sh 2>/dev/null || true
./scripts/remote_setup_debian.sh
"@
            ssh @sshOptions $targetHost $remoteCmd
        }
        else {
            # Update rápido en caliente
            Write-Host "`n[4/4] Actualizando codigo y reconstruyendo contenedores..." -ForegroundColor Green
            $updateCmd = @"
cd $RemotePath
tar -xf crm_nyx_deploy.tar
rm -f crm_nyx_deploy.tar
if [ ! -f .env ]; then
    cp .env.example .env 2>/dev/null || true
fi
docker builder prune -f 2>/dev/null || true
docker compose -f docker-compose.prod.yml up -d --build
sleep 3
docker compose -f docker-compose.prod.yml ps
"@
            ssh @sshOptions $targetHost $updateCmd
        }

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Ocurrio un error durante la ejecucion en la maquina virtual."
        }

        Write-Host "`n======================================================================" -ForegroundColor Cyan
        Write-Host " [NYX CRM] PROCESO ($Action) COMPLETADO EXITOSAMENTE" -ForegroundColor Green
        Write-Host "======================================================================" -ForegroundColor Cyan
        Write-Host " Acceso Web Frontend    : http://${ServerIP}/" -ForegroundColor Yellow
        Write-Host " Acceso API Hub Swagger : http://${ServerIP}/swagger/" -ForegroundColor Yellow
        Write-Host " Acceso MinIO Console   : http://${ServerIP}:9001/" -ForegroundColor Yellow
        Write-Host "======================================================================" -ForegroundColor Cyan
    }
}
