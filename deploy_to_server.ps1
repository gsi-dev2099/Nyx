<#
.SYNOPSIS
    Script de Despliegue Automático de CRM Nyx a Servidor Linux / VM Debian.
.DESCRIPTION
    Crea el directorio remoto si no existe, transfiere el código del proyecto y scripts de BD vía SCP/SSH,
    genera las variables de entorno si faltan y levanta los contenedores con Docker Compose.
.EXAMPLE
    .\deploy_to_server.ps1 -ServerIP "10.10.40.12" -User "root" -RemotePath "/srv/crm_nyx" -ResetDB
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, HelpMessage="IP o Hostname del servidor Debian")]
    [string]$ServerIP,

    [Parameter(Mandatory=$false)]
    [string]$User = "root",

    [Parameter(Mandatory=$false)]
    [string]$RemotePath = "/srv/crm_nyx",

    [Parameter(Mandatory=$false)]
    [string]$SSHKeyPath = "",

    [Parameter(Mandatory=$false)]
    [switch]$ResetDB
)

$ErrorActionPreference = "Stop"

$targetHost = "${User}@${ServerIP}"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "DESPLIEGUE AUTOMATICO DE CRM NYX A SERVIDOR REMOTO (DEBIAN)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Servidor destino: $targetHost" -ForegroundColor Yellow
Write-Host "Ruta remota:      $RemotePath" -ForegroundColor Yellow
Write-Host "Reiniciar BD:     $ResetDB" -ForegroundColor Yellow
Write-Host "=========================================================="

# Construir opciones de SSH
$sshOptions = @("-o", "StrictHostKeyChecking=no")
if ($SSHKeyPath -ne "" -and (Test-Path $SSHKeyPath)) {
    $sshOptions += @("-i", $SSHKeyPath)
}

# 1. Crear ruta en el servidor remoto si no existe
Write-Host "`n[1/4] Verificando y creando directorio remoto..." -ForegroundColor Green
$createDirCmd = "mkdir -p $RemotePath"
ssh @sshOptions $targetHost $createDirCmd
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error creando el directorio remoto $RemotePath en $ServerIP"
}
Write-Host "  Ruta remota lista en: $RemotePath" -ForegroundColor Gray

# 2. Empaquetar y transferir archivos (incluyendo db_export)
Write-Host "`n[2/4] Empaquetando y transfiriendo archivos al servidor (incluye scripts de BD)..." -ForegroundColor Green

$scriptDir = $PSScriptRoot
if (-not $scriptDir) { $scriptDir = Get-Location }

# Crear archivo temporal tar
$tempTar = Join-Path $env:TEMP "crm_nyx_deploy.tar"
if (Test-Path $tempTar) { Remove-Item $tempTar -Force }

Write-Host "  Comprimiendo proyecto y backups de base de datos..." -ForegroundColor Gray
tar --exclude='.git' --exclude='bin' --exclude='obj' --exclude='.vs' --exclude='*.user' -cvf $tempTar -C $scriptDir .

Write-Host "  Transfiriendo paquete via SCP..." -ForegroundColor Gray
$destinationTarget = "${targetHost}:${RemotePath}/crm_nyx_deploy.tar"
scp @sshOptions $tempTar $destinationTarget
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error durante la transferencia SCP a $ServerIP"
}

# 3. Descomprimir, construir y desplegar mediante el script nativo de Linux
Write-Host "`n[3/3] Descomprimiendo y ejecutando despliegue nativo en el servidor..." -ForegroundColor Green

$remoteDeployCmd = "cd $RemotePath && tar -xvf crm_nyx_deploy.tar db_export/deploy_remote.sh && chmod +x db_export/deploy_remote.sh && ./db_export/deploy_remote.sh"

ssh @sshOptions $targetHost $remoteDeployCmd
if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo la ejecucion del despliegue en el servidor $ServerIP"
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "DESPLIEGUE FINALIZADO CON EXITO" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Acceso Web Frontend: http://${ServerIP}/" -ForegroundColor Yellow
Write-Host "Acceso API Hub Swagger: http://${ServerIP}/swagger/" -ForegroundColor Yellow
Write-Host "Acceso SLA Engine API:  http://${ServerIP}/sla/" -ForegroundColor Yellow
Write-Host "=========================================================="
