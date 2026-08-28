<#
.SYNOPSIS
    Extrae la estructura DDL completa (Schema-Only) de nyx_crm sin datos sensibles.
.DESCRIPTION
    Ejecuta pg_dump en el contenedor local crm_postgres y genera un archivo SQL sanitizado.
#>

param(
    [string]$ContainerName = "crm_postgres",
    [string]$DbUser = "ronald",
    [string]$DbName = "nyx_crm",
    [string]$OutputDir = "$PSScriptRoot\..\db_export\dumps",
    [string]$OutputFile = "nyx_crm_schema_only.sql"
)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [NYX CRM] EXTRACTOR DE ESTRUCTURA DDL (SCHEMA-ONLY)" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Verificar si el contenedor está en ejecución
$isRunning = docker ps --filter "name=$ContainerName" --format "{{.Names}}"
if (-not $isRunning) {
    Write-Warning "El contenedor '$ContainerName' no se encuentra en ejecucion o Docker no responde."
    Write-Host "Asegurese de que Docker Desktop este corriendo y el contenedor crm_postgres este activo." -ForegroundColor Yellow
    exit 1
}

# 2. Asegurar directorio de destino
$resolvedOutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (-not (Test-Path $resolvedOutputDir)) {
    New-Item -ItemType Directory -Path $resolvedOutputDir -Force | Out-Null
}

$targetPath = Join-Path $resolvedOutputDir $OutputFile
$tempContainerPath = "/tmp/$OutputFile"

Write-Host "-> Generando DDL schema-only desde '$ContainerName' ($DbName)..." -ForegroundColor Yellow

# 3. Ejecutar pg_dump dentro del contenedor
docker exec $ContainerName pg_dump `
    -U $DbUser `
    -d $DbName `
    --schema-only `
    --clean `
    --if-exists `
    --no-owner `
    --no-privileges `
    --encoding=UTF8 `
    -f $tempContainerPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo la ejecucion de pg_dump dentro del contenedor."
    exit 1
}

# 4. Copiar dump al host
Write-Host "-> Copiando archivo a $targetPath..." -ForegroundColor Yellow
docker cp "${ContainerName}:${tempContainerPath}" $targetPath

# 5. Limpiar archivo temporal en el contenedor
docker exec $ContainerName rm -f $tempContainerPath

# 6. Validar tamaño
$fileSize = (Get-Item $targetPath).Length / 1KB
Write-Host "-> Estructura DDL extraida con exito: $OutputFile ($([math]::Round($fileSize, 2)) KB)" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan
