<#
.SYNOPSIS
    Script de restauración manual de bases de datos para NYX CRM.
.DESCRIPTION
    Restaura las 5 bases de datos (nx_ecosystem, nyx_crm, nyx_flow, nyx_approval, nyx_sla) 
    o una específica desde la carpeta db_export/dumps.
.EXAMPLE
    .\CRM_API\scripts\restore_manual_dbs.ps1 -TargetHost "127.0.0.1" -Port 5432 -User "ronald" -Password "Gs1$2099Zx23rO24M4r25"
    .\CRM_API\scripts\restore_manual_dbs.ps1 -TargetHost "10.10.40.12" -Port 5432 -User "ronald" -Password "Gs1_2099Zx23rO24M4r25"
#>
param(
    [string]$TargetHost = "127.0.0.1",
    [int]$Port = 5432,
    [string]$User = "ronald",
    [string]$Password = "Gs1$2099Zx23rO24M4r25",
    [string]$Database = "ALL"
)

$ErrorActionPreference = "Continue"
$helperScript = Join-Path $PSScriptRoot "restore_db_helper.py"

python "$helperScript" --host "$TargetHost" --port $Port --user "$User" --password "$Password" --database "$Database"
